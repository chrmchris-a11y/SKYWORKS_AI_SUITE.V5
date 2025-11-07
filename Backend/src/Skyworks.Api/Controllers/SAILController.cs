using Microsoft.AspNetCore.Mvc;
using Skyworks.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Api.Controllers
{
    [ApiController]
    [Route("api/sail")]
    public class SAILController : ControllerBase
    {
        private readonly PythonCalculationClient _pythonClient;
        private readonly ILogger<SAILController> _logger;

        public SAILController(PythonCalculationClient pythonClient, ILogger<SAILController> logger)
        {
            _pythonClient = pythonClient;
            _logger = logger;
        }

        [HttpPost("calculate")]
        public async Task<ActionResult<SAILResponse>> CalculateSAIL([FromBody] SAILRequest request)
        {
            var startTime = DateTime.UtcNow;
            var correlationId = HttpContext.TraceIdentifier;

            try
            {
                // Validate common fields
                if (string.IsNullOrEmpty(request.SoraVersion) || 
                    (request.SoraVersion != "2.0" && request.SoraVersion != "2.5"))
                {
                    return BadRequest("soraVersion must be '2.0' or '2.5'");
                }

                if (request.SoraVersion == "2.0")
                {
                    return await HandleSora20(request, startTime, correlationId);
                }
                else
                {
                    return await HandleSora25(request, startTime, correlationId);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Python service unreachable for correlation {CorrelationId}", correlationId);
                Response.Headers.Add("x-correlation-id", correlationId);
                return StatusCode(502, "Python calculation service unavailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calculating SAIL for correlation {CorrelationId}", correlationId);
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<ActionResult<SAILResponse>> HandleSora20(SAILRequest request, DateTime startTime, string correlationId)
        {
            // Validate SORA 2.0 specific fields
            // Per EASA AMC/GM: GRC 1-7 have SAIL mappings, GRC 8+ → Category C
            if (request.FinalGrc < 1 || request.FinalGrc > 8)
            {
                return BadRequest("finalGrc must be between 1 and 8 for SORA 2.0");
            }

            if (string.IsNullOrEmpty(request.ResidualArc))
            {
                return BadRequest("residualArc is required for SORA 2.0");
            }

            var normalizedArc = NormalizeArcLetter(request.ResidualArc);
            if (normalizedArc == null || !new[] { "a", "b", "c", "d" }.Contains(normalizedArc))
            {
                return BadRequest("residualArc must be 'a', 'b', 'c', or 'd' for SORA 2.0");
            }

            // "The SAIL defined in Step #9 is the level of robustness required to satisfy the safety objectives specified in Annex E." (EASA AMC/GM – SORA)
            // Category-C path stops SAIL for GRC > 7
            if (request.FinalGrc > 7)
            {
                var categoryResponse = new SAILResponse
                {
                    Sail = null,
                    SailLevel = null,
                    OsoCount = null,
                    Category = "C",
                    Reference = "JARUS SORA 2.0 Table 5 (GRC×ARC→SAIL), AMC Annex E link - Category C for GRC>7",
                    FinalGrc = request.FinalGrc,
                    ResidualArc = normalizedArc,
                    SoraVersion = "2.0"
                };

                LogCalculation("2.0", request.FinalGrc, normalizedArc, null, null, "C", startTime);
                return Ok(categoryResponse);
            }

            // Map via standard SORA 2.0 table (Table 5 – Determination of SAIL)
            var sail = MapSora20ToSail(request.FinalGrc, normalizedArc);
            
            // "In Annex E the 24 Operational Safety Objectives… are listed with their associated levels of robustness." (EASA AMC/GM – SORA)
            // OSO count: derived count per SAIL from Annex-E applicability for SORA 2.0 only
            var osoCount = GetOsoCountForSail(sail);

            var response = new SAILResponse
            {
                Sail = sail,
                SailLevel = sail,
                OsoCount = osoCount,
                Category = null,
                Reference = "JARUS SORA 2.0 Table 5 (GRC×ARC→SAIL), AMC Annex E link",
                FinalGrc = request.FinalGrc,
                ResidualArc = normalizedArc,
                SoraVersion = "2.0"
            };

            LogCalculation("2.0", request.FinalGrc, normalizedArc, null, sail, null, startTime);
            return Ok(response);
        }

        private async Task<ActionResult<SAILResponse>> HandleSora25(SAILRequest request, DateTime startTime, string correlationId)
        {
            // Validate SORA 2.5 specific fields
            if (request.FinalGrc < 1 || request.FinalGrc > 10)
            {
                return BadRequest("finalGrc must be between 1 and 10 for SORA 2.5");
            }

            if (!request.ResidualArcLevel.HasValue)
            {
                return BadRequest("residualArcLevel is required for SORA 2.5");
            }

            // Residual ARC is numeric in 2.5; do not bin to letters
            if (request.ResidualArcLevel < 1 || request.ResidualArcLevel > 10)
            {
                return BadRequest("residualArcLevel must be between 1 and 10 for SORA 2.5");
            }

            // Reject any letter ARC for 2.5
            if (!string.IsNullOrEmpty(request.ResidualArc))
            {
                return BadRequest("Use residualArcLevel (numeric) for SORA 2.5, not residualArc");
            }

            string sail;
            string reference;

            // If finalGrc >= 9 ⇒ sail="VI" for any residual ARC
            // Rationale: high-GRC rows collapse to SAIL VI in the 2.5 mapping
            if (request.FinalGrc >= 9)
            {
                sail = "VI";
                reference = "SORA 2.5 Annex D/Table mapping (numeric ARC), high-GRC rule (9–10→VI)";
            }
            else
            {
                // Call Python /sail/calculate v2.5 path
                var pythonResponse = await _pythonClient.CalculateSAIL25Async(request.FinalGrc, request.ResidualArcLevel.Value);
                sail = pythonResponse.Sail;
                reference = "SORA 2.5 Annex D/Table mapping (numeric ARC)";
            }

            var response = new SAILResponse
            {
                Sail = sail,
                SailLevel = sail,
                OsoCount = null, // Do not return osoCount for 2.5; 2.5 Annex-E profile differs
                Category = null,
                Reference = reference,
                FinalGrc = request.FinalGrc,
                ResidualArcLevel = request.ResidualArcLevel,
                SoraVersion = "2.5"
            };

            LogCalculation("2.5", request.FinalGrc, null, request.ResidualArcLevel, sail, null, startTime);
            return Ok(response);
        }

        private string? NormalizeArcLetter(string arcInput)
        {
            if (string.IsNullOrEmpty(arcInput)) return null;
            
            var normalized = arcInput.ToLowerInvariant()
                .Replace("arc-", "")
                .Replace("arc_", "")
                .Replace("arc", "")
                .Trim();
            
            return normalized;
        }

        private string MapSora20ToSail(int grc, string arc)
        {
            // Standard SORA 2.0 mapping table (Table 5 – Determination of SAIL)
            return (grc, arc) switch
            {
                (1, "a") => "I",
                (1, "b") => "I",
                (1, "c") => "II",
                (1, "d") => "II",
                (2, "a") => "I",
                (2, "b") => "I",
                (2, "c") => "II",
                (2, "d") => "III",
                (3, "a") => "I",
                (3, "b") => "II",
                (3, "c") => "III",
                (3, "d") => "IV",
                (4, "a") => "II",
                (4, "b") => "III",
                (4, "c") => "IV",
                (4, "d") => "V",
                (5, "a") => "III",
                (5, "b") => "IV",
                (5, "c") => "V",
                (5, "d") => "VI",
                (6, "a") => "III",
                (6, "b") => "IV",
                (6, "c") => "V",
                (6, "d") => "VI",
                (7, "a") => "IV",
                (7, "b") => "V",
                (7, "c") => "VI",
                (7, "d") => "VI",
                _ => throw new ArgumentException($"Invalid GRC/ARC combination: {grc}/{arc}")
            };
        }

        private int GetOsoCountForSail(string sail)
        {
            // Annex E lists 24 OSOs; SAIL determines robustness level to satisfy them (counts are derived, not invented)
            return sail switch
            {
                "I" => 6,
                "II" => 10,
                "III" => 15,
                "IV" => 18,
                "V" => 21,
                "VI" => 24,
                _ => throw new ArgumentException($"Invalid SAIL: {sail}")
            };
        }

        private void LogCalculation(string version, int grc, string? arcToken, int? arcLevel, string? sail, string? category, DateTime startTime)
        {
            var duration = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
            _logger.LogInformation("SAIL calculation: {SoraVersion} GRC={FinalGrc} ARC={ArcToken}{ArcLevel} → SAIL={Sail} Category={Category} ({DurationMs}ms)",
                version, grc, arcToken, arcLevel.HasValue ? arcLevel.ToString() : "", sail, category, duration);
        }
    }

    public class SAILRequest
    {
        [Required]
        public string SoraVersion { get; set; } = "";
        
        [Required]
        public int FinalGrc { get; set; }
        
        // For SORA 2.0
        public string? ResidualArc { get; set; }
        
        // For SORA 2.5
        public int? ResidualArcLevel { get; set; }
    }

    public class SAILResponse
    {
        public string? Sail { get; set; }
        public string? SailLevel { get; set; }
        public int? OsoCount { get; set; }
        public string? Category { get; set; }
        public string Reference { get; set; } = "";
        public int FinalGrc { get; set; }
        public string? ResidualArc { get; set; }
        public int? ResidualArcLevel { get; set; }
        public string SoraVersion { get; set; } = "";
    }
}

