using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Skyworks.Api.Controllers;

/// <summary>
/// SORA Calculator API - 100% EASA/JARUS Compliant
/// 
/// References:
/// - SORA 2.5 Main Body: JAR-DEL-SRM-SORA-MB-2.5, Edition 2.5, 13.05.2024
/// - SORA 2.5 Annex B: JAR-DEL-SRM-SORA-B-2.5, Edition 2.5, 13.05.2024
/// - SORA 2.0 Main Body: JAR-DEL-WG6-D.04, Edition 2.0, 30.01.2019
/// 
/// See: SORA_FIELD_SPECIFICATIONS.md for complete specifications
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SoraController : ControllerBase
{
    private readonly ILogger<SoraController> _logger;
    private readonly IWebHostEnvironment _env;

    public SoraController(
        ILogger<SoraController> logger,
        IWebHostEnvironment _env)
    {
        _logger = logger;
        this._env = _env;
    }

    /// <summary>
    /// Calculate SORA assessment (Ground Risk, Air Risk, SAIL)
    /// </summary>
    /// <remarks>
    /// POST /api/v1/sora/calculate
    /// 
    /// SORA 2.5 Example:
    /// {
    ///   "soraVersion": "2.5",
    ///   "drone": {
    ///     "mtom_kg": 0.249,
    ///     "maxSpeed_ms": 16,
    ///     "characteristicDimension_m": 0.213
    ///   },
    ///   "populationDensity": "<500",
    ///   "m1a": "Low",
    ///   "m1b": "None",
    ///   "m1c": "None",
    ///   "m2": "Medium",
    ///   "altitude_ft": 400,
    ///   "controlledAirspace": false,
    ///   "airportEnvironment": false,
    ///   "populatedArea": true,
    ///   "atypicalAirspace": false,
    ///   "isVLOS": true
    /// }
    /// 
    /// SORA 2.0 Example:
    /// {
    ///   "soraVersion": "2.0",
    ///   "operationScenario": "VLOS_Populated",
    ///   "drone": {
    ///     "mtom_kg": 0.895,
    ///     "maxSpeed_ms": 21,
    ///     "characteristicDimension_m": 0.35
    ///   },
    ///   "m1": "Low",
    ///   "m2": "Medium",
    ///   "m3": "Adequate",
    ///   "altitude_ft": 400,
    ///   "controlledAirspace": false,
    ///   "airportEnvironment": false,
    ///   "populatedArea": true,
    ///   "atypicalAirspace": false,
    ///   "isVLOS": true
    /// }
    /// </remarks>
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] SoraCalculationRequest request)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.SoraVersion))
                return BadRequest(new { error = "soraVersion is required. Valid: '2.0' or '2.5'" });

            if (request.Drone == null)
                return BadRequest(new { error = "drone object is required with mtom_kg, maxSpeed_ms, characteristicDimension_m" });

            // Call Node.js calculator
            var result = await CallNodeCalculator(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SORA");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Get SORA field specifications (dropdown options, validation rules)
    /// </summary>
    [HttpGet("specifications")]
    public IActionResult GetSpecifications([FromQuery] string version = "2.5")
    {
        if (version == "2.5")
        {
            return Ok(new
            {
                version = "SORA 2.5",
                populationDensityOptions = new[] { "Controlled", "<5", "<50", "<500", "<5000", "<50000", ">50000" },
                // EASA JARUS SORA 2.5 Annex B - Strategic Mitigations
                m1aOptions = new[] { "None", "Low" }, // Annex B p.8 - ONLY Low available
                m1bOptions = new[] { "None", "Medium", "High" }, // Annex B p.14 - Medium, High
                m1cOptions = new[] { "None", "Low" }, // Annex B p.19 - ONLY Low available
                m2Options = new[] { "None", "Low", "Medium", "High" }, // Annex B p.25 - All levels
                constraints = new[]
                {
                    "M1(A) Low CANNOT combine with M1(B) any level (Annex B p.8)",
                    "Small-UA Rule: IF MTOM ≤ 0.25kg AND speed ≤ 25m/s THEN iGRC = 1 (Main Body §4.3)",
                    "Final GRC cannot go below column minimum from Table 2 (Main Body Table 2)"
                },
                reference = "JAR-DEL-SRM-SORA-MB-2.5, Edition 2.5, 13.05.2024"
            });
        }
        else if (version == "2.0")
        {
            return Ok(new
            {
                version = "SORA 2.0",
                operationScenarioOptions = new[]
                {
                    "VLOS_Controlled", "VLOS_Sparsely", "VLOS_Populated", "VLOS_Gathering",
                    "BVLOS_Controlled", "BVLOS_Sparsely", "BVLOS_Populated", "BVLOS_Gathering"
                },
                // AMC1 Article 11 (SORA 2.0) - Tactical Mitigations
                m1Options = new[] { "None", "Low", "Medium", "High" }, // AMC1 Table 3
                m2Options = new[] { "None", "Low", "High" }, // AMC1 Table 4 - NO Medium
                m3Options = new[] { "None", "Adequate", "Validated" }, // AMC1 §3.5 - ERP levels
                constraints = new[]
                {
                    "EVLOS is treated as BVLOS for GRC determination (AMC1 §2.2)",
                    "M1 uses column-minimum clamp method (AMC1 Table 3 footnotes)",
                    "M3 None has penalty (+1 to final GRC) (AMC1 §3.5)",
                    "Final GRC cannot go below column minimum from AMC1 Table 2"
                },
                reference = "JAR-DEL-WG6-D.04, Edition 2.0, 30.01.2019"
            });
        }
        else
        {
            return BadRequest(new { error = "Invalid version. Valid: '2.0' or '2.5'" });
        }
    }

    /// <summary>
    /// Validate SORA parameters without calculating
    /// </summary>
    [HttpPost("validate")]
    public IActionResult Validate([FromBody] SoraCalculationRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.SoraVersion))
            errors.Add("soraVersion is required");

        if (request.Drone == null)
        {
            errors.Add("drone object is required");
        }
        else
        {
            if (request.Drone.Mtom_kg <= 0)
                errors.Add("drone.mtom_kg must be > 0");
            if (request.Drone.MaxSpeed_ms <= 0)
                errors.Add("drone.maxSpeed_ms must be > 0");
            if (request.Drone.CharacteristicDimension_m <= 0)
                errors.Add("drone.characteristicDimension_m must be > 0");
        }

        // Version-specific validation
        if (request.SoraVersion == "2.5")
        {
            // SORA 2.5: Check M1(A) Low + M1(B) constraint (Annex B p.8)
            if (request.M1a == "Low" && !string.IsNullOrWhiteSpace(request.M1b) && request.M1b != "None")
            {
                errors.Add("M1(A) Low cannot be combined with M1(B) (SORA 2.5 Annex B page 8)");
            }

            // Check invalid options (EASA-compliant)
            if (request.M1a == "Medium" || request.M1a == "High")
                errors.Add("M1(A) Medium/High do not exist in SORA 2.5. Valid: None, Low only");
            if (request.M1b == "Low")
                errors.Add("M1(B) Low does not exist in SORA 2.5. Valid: None, Medium, High");
            if (request.M1c == "Medium" || request.M1c == "High")
                errors.Add("M1(C) Medium/High do not exist in SORA 2.5. Valid: None, Low only");
            // M2 accepts all levels in SORA 2.5 (None, Low, Medium, High)

            if (string.IsNullOrWhiteSpace(request.PopulationDensity))
                errors.Add("populationDensity is required for SORA 2.5");
        }
        else if (request.SoraVersion == "2.0")
        {
            if (string.IsNullOrWhiteSpace(request.OperationScenario))
                errors.Add("operationScenario is required for SORA 2.0");

            // Check invalid options (AMC1 Article 11)
            if (request.M2_20 == "Medium")
                errors.Add("M2 Medium does not exist in SORA 2.0. Valid: None, Low, High (AMC1 Table 4)");

            // M3 only exists in SORA 2.0
            if (string.IsNullOrWhiteSpace(request.M3))
                errors.Add("m3 (ERP) is required for SORA 2.0. Valid: None, Adequate, Validated");
        }
        else
        {
            errors.Add("Invalid soraVersion. Valid: '2.0' or '2.5'");
        }

        if (errors.Any())
        {
            return BadRequest(new { valid = false, errors });
        }

        return Ok(new { valid = true, message = "All parameters are valid" });
    }

    // =============================================================================
    // Private Helpers
    // =============================================================================

    private async Task<object> CallNodeCalculator(SoraCalculationRequest request)
    {
        // Find Node.js executable
        var nodePath = FindNodeExecutable();
        if (nodePath == null)
        {
            throw new Exception("Node.js not found. Please install Node.js to use SORA calculator.");
        }

        // Find sora-calculator.js
        var calculatorPath = Path.Combine(
            _env.ContentRootPath, // Backend/src/Skyworks.Api
            "..", "..", "..",     // Navigate to repo root
            "WebPlatform", "wwwroot", "app", "Pages", "ui", "assets", "sora-calculator.js"
        );
        calculatorPath = Path.GetFullPath(calculatorPath);

        if (!System.IO.File.Exists(calculatorPath))
        {
            throw new FileNotFoundException($"SORA calculator not found at {calculatorPath}");
        }

        // Create Node.js wrapper script
        var wrapperScript = GenerateNodeWrapperScript(request);
        var tempScriptPath = Path.Combine(Path.GetTempPath(), $"sora-calc-{Guid.NewGuid()}.js");
        await System.IO.File.WriteAllTextAsync(tempScriptPath, wrapperScript);

        try
        {
            // Execute Node.js
            var processInfo = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = $"\"{tempScriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(calculatorPath)
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new Exception("Failed to start Node.js process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Node.js error: {Error}", error);
                throw new Exception($"Node.js calculation failed: {error}");
            }

            // Parse JSON result
            var result = System.Text.Json.JsonSerializer.Deserialize<object>(output);
            return result;
        }
        finally
        {
            // Cleanup temp file
            try { System.IO.File.Delete(tempScriptPath); } catch { }
        }
    }

    private string? FindNodeExecutable()
    {
        // Try common Node.js paths
        var paths = new[]
        {
            "node", // PATH environment variable
            @"C:\Program Files\nodejs\node.exe",
            @"C:\Program Files (x86)\nodejs\node.exe",
            Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\nodejs\node.exe"
        };

        foreach (var path in paths)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        return path;
                    }
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private string GenerateNodeWrapperScript(SoraCalculationRequest request)
    {
        // Generate Node.js script that calls sora-calculator.js
        var json = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return $@"
const calc = require('./sora-calculator.js');

const request = {json};

try {{
  let result;
  
  if (request.soraVersion === '2.5') {{
    const params = {{
      mtom_kg: request.drone.mtom_kg,
      maxSpeed_ms: request.drone.maxSpeed_ms,
      characteristicDimension_m: request.drone.characteristicDimension_m,
      populationDensity: request.populationDensity || 'Controlled',
      m1a: request.m1a || 'None',
      m1b: request.m1b || 'None',
      m1c: request.m1c || 'None',
      m2: request.m2 || 'None',
      altitude_ft: request.altitude_ft || 0,
      controlledAirspace: request.controlledAirspace || false,
      airportEnvironment: request.airportEnvironment || false,
      populatedArea: request.populatedArea || false,
      atypicalAirspace: request.atypicalAirspace || false,
      isVLOS: request.isVLOS !== undefined ? request.isVLOS : true
    }};
    
    result = calc.calculateSORA25(params);
  }} else if (request.soraVersion === '2.0') {{
    const params = {{
      operationScenario: request.operationScenario || 'VLOS_Controlled',
      characteristicDimension_m: request.drone.characteristicDimension_m,
      m1: request.m1 || 'None',
      m2: request.m2_20 || request.m2 || 'None',
      m3: request.m3 || 'None',
      altitude_ft: request.altitude_ft || 0,
      controlledAirspace: request.controlledAirspace || false,
      airportEnvironment: request.airportEnvironment || false,
      populatedArea: request.populatedArea || false,
      atypicalAirspace: request.atypicalAirspace || false,
      isVLOS: request.isVLOS !== undefined ? request.isVLOS : true
    }};
    
    result = calc.calculateSORA20(params);
  }} else {{
    throw new Error('Invalid SORA version: ' + request.soraVersion);
  }}
  
  console.log(JSON.stringify(result));
}} catch (err) {{
  console.error(JSON.stringify({{ error: err.message, stack: err.stack }}));
  process.exit(1);
}}
";
    }
}

// =============================================================================
// Request/Response Models
// =============================================================================

public class SoraCalculationRequest
{
    public string SoraVersion { get; set; } = "2.5"; // "2.0" or "2.5"

    // Drone specifications
    public DroneSpecs Drone { get; set; } = new();

    // SORA 2.5 fields
    public string? PopulationDensity { get; set; } // "Controlled", "<5", "<50", "<500", "<5000", "<50000", ">50000"
    public string? M1a { get; set; } // M1(A) Sheltering: "None", "Low", "Medium"
    public string? M1b { get; set; } // M1(B) Operational Restrictions: "None", "Medium", "High"
    public string? M1c { get; set; } // M1(C) Ground Observation: "None", "Low"
    public string? M2 { get; set; } // M2 Impact Dynamics: "None", "Medium", "High"

    // SORA 2.0 fields
    public string? OperationScenario { get; set; } // "VLOS_Controlled", "BVLOS_Populated", etc.
    public string? M1 { get; set; } // M1 Strategic: "None", "Low", "Medium", "High"
    public string? M2_20 { get; set; } // M2 for SORA 2.0 (if different from M2)
    public string? M3 { get; set; } // M3 ERP: "None", "Adequate", "Validated" (SORA 2.0 only)

    // Airspace parameters (common)
    public double Altitude_ft { get; set; } = 400;
    public bool ControlledAirspace { get; set; } = false;
    public bool AirportEnvironment { get; set; } = false;
    public bool PopulatedArea { get; set; } = false;
    public bool AtypicalAirspace { get; set; } = false;
    public bool IsVLOS { get; set; } = true;
}

public class DroneSpecs
{
    public double Mtom_kg { get; set; } // Maximum Take-Off Mass
    public double MaxSpeed_ms { get; set; } // Maximum speed in m/s
    public double CharacteristicDimension_m { get; set; } // Characteristic dimension in meters
}
