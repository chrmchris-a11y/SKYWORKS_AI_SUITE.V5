using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace Skyworks.Api.Controllers
{
    // Snake case naming policy for Python API compatibility
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var result = new StringBuilder();
            result.Append(char.ToLowerInvariant(name[0]));

            for (int i = 1; i < name.Length; i++)
            {
                if (name[i] == '_')
                {
                    // Already an underscore, keep it as single underscore
                    result.Append('_');
                }
                else if (char.IsUpper(name[i]))
                {
                    // Only add underscore if previous char wasn't already an underscore
                    if (result.Length > 0 && result[result.Length - 1] != '_')
                    {
                        result.Append('_');
                    }
                    result.Append(char.ToLowerInvariant(name[i]));
                }
                else
                {
                    result.Append(name[i]);
                }
            }

            return result.ToString();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class SoraProxyController : ControllerBase
    {
        private readonly ILogger<SoraProxyController> _logger;
        private readonly HttpClient _httpClient;
        private const string PYTHON_API_BASE = "http://localhost:8001";

        public SoraProxyController(ILogger<SoraProxyController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] JsonElement requestBody)
        {
            try
            {
                _logger.LogInformation("Received SORA complete request");
                _logger.LogDebug("Request body: {Body}", requestBody.GetRawText());

                // Detect request format
                if (requestBody.TryGetProperty("SoraVersion", out var soraVersion))
                {
                    var versionString = soraVersion.GetString();
                    if (string.IsNullOrEmpty(versionString))
                    {
                        _logger.LogWarning("SoraVersion is empty");
                        return BadRequest(new { 
                            error = "SoraVersion cannot be empty",
                            type = "ValidationError",
                            timestamp = DateTime.UtcNow
                        });
                    }

                    _logger.LogInformation("Detected mission.html format with SORA version: {Version}", versionString);
                    return await HandleMissionHtmlFormat(requestBody, versionString);
                }
                else if (requestBody.TryGetProperty("category", out var categoryProp))
                {
                    var category = categoryProp.GetString();
                    if (string.IsNullOrEmpty(category))
                    {
                        _logger.LogWarning("Category is empty");
                        return BadRequest(new { 
                            error = "Category cannot be empty",
                            type = "ValidationError",
                            timestamp = DateTime.UtcNow
                        });
                    }

                    _logger.LogInformation("Detected standard format with category: {Category}", category);
                    return await HandleStandardFormat(requestBody, category);
                }
                else
                {
                    _logger.LogWarning("Unknown request format");
                    return BadRequest(new { 
                        error = "Unknown request format. Expected 'SoraVersion' or 'category' property.",
                        type = "ValidationError",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Validation error in SORA request");
                return BadRequest(new 
                { 
                    error = argEx.Message,
                    type = "ValidationError",
                    details = "Please check that all required fields are provided",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "JSON parsing error");
                return BadRequest(new { 
                    error = "Invalid JSON format", 
                    type = "JsonParsingError",
                    details = jex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error calling Python API");
                return StatusCode(502, new 
                { 
                    error = "Failed to communicate with calculation service",
                    type = "PythonApiConnectionError",
                    details = httpEx.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in Complete endpoint");
                return StatusCode(500, new 
                { 
                    error = "An unexpected error occurred during SORA calculation",
                    type = ex.GetType().Name,
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        private async Task<IActionResult> HandleMissionHtmlFormat(JsonElement requestBody, string soraVersion)
        {
            try
            {
                _logger.LogInformation("Processing mission.html format for SORA {Version}", soraVersion);

                if (soraVersion == "2.0")
                {
                    // Fix #1: Stop treating SORA 2.0 as GRC-only. Execute full pipeline via local orchestration.
                    return await RunCompleteSora20Workflow(requestBody);
                }
                else if (soraVersion == "2.5")
                {
                    var request = ExtractSora25FromMissionHtml(requestBody);
                    // FIX: Use correct endpoint for SORA 2.5
                    return await ForwardToPythonAndTransform(request, "/api/sora/complete-v25", "SORA-2.5");
                }
                else
                {
                    _logger.LogWarning("Unsupported SORA version: {Version}", soraVersion);
                    return BadRequest(new { error = $"Unsupported SORA version: {soraVersion}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling mission.html format");
                return StatusCode(500, new { error = "Error processing mission format", details = ex.Message });
            }
        }

        private async Task<IActionResult> HandleStandardFormat(JsonElement requestBody, string category)
        {
            try
            {
                _logger.LogInformation("Processing standard format for category: {Category}", category);

                if (category == "SORA-2.0")
                {
                    // Fix #1: Stop treating SORA 2.0 as GRC-only. Execute full pipeline via local orchestration.
                    return await RunCompleteSora20Workflow(requestBody);
                }
                else if (category == "SORA-2.5")
                {
                    return await ForwardToPythonAndTransform(requestBody, "/api/sora/complete-v25", category);
                }
                else
                {
                    _logger.LogWarning("Unknown category: {Category}", category);
                    return BadRequest(new { error = $"Unknown category: {category}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling standard format");
                return StatusCode(500, new { error = "Error processing standard format", details = ex.Message });
            }
        }

        private SoraCompleteRequest ExtractSora20FromMissionHtml(JsonElement json)
        {
            var request = new SoraCompleteRequest { Category = "SORA-2.0" };

            try
            {
                if (!json.TryGetProperty("GroundRisk", out var groundRisk))
                {
                    _logger.LogWarning("GroundRisk not found in request");
                    throw new ArgumentException("GroundRisk is required for SORA 2.0");
                }

                // Fix #8/#11/#14: Do NOT hard-map numeric density to L/M/H. Capture value + optional evidence metadata.
                // Store for orchestration (numeric) and pass evidence references if present.
                if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
                {
                    var popValue = popDensity.GetDouble();
                    if (popValue < 0)
                        throw new ArgumentException("PopulationDensity must be a non-negative number");
                    request.PopulationDensityValue = popValue;
                }
                if (groundRisk.TryGetProperty("PopulationDensitySource", out var popSrc))
                {
                    request.PopulationDensitySource = popSrc.GetString();
                }
                if (groundRisk.TryGetProperty("PopulationDensityWindow", out var popWin))
                {
                    request.PopulationDensityWindow = popWin.GetString();
                }

                // ✅ ENHANCED: Sheltering handling
                if (groundRisk.TryGetProperty("Sheltering", out var shelteringElement))
                {
                    var shelteringValue = shelteringElement.GetString();
                    if (!string.IsNullOrEmpty(shelteringValue) && 
                        (shelteringValue == "Low" || shelteringValue == "Medium" || shelteringValue == "High"))
                    {
                        request.Sheltering = shelteringValue;
                    }
                    else
                    {
                        _logger.LogWarning("Invalid Sheltering value: {Value}, using 'High' default", shelteringValue);
                        request.Sheltering = "High";
                    }
                }
                else
                {
                    _logger.LogDebug("Sheltering not specified, using 'High' default");
                    request.Sheltering = "High";
                }

                // Scenario
                if (groundRisk.TryGetProperty("Scenario_V2_0", out var scenario))
                {
                    request.Scenario_V2_0 = scenario.GetString() ?? "VLOS_SparselyPopulated";
                }
                else
                {
                    request.Scenario_V2_0 = "VLOS_SparselyPopulated";
                }

                // Fix #7: Do NOT derive MTOM from KE with assumed speed. Use declared MTOM only if provided.
                if (groundRisk.TryGetProperty("MtomKg", out var mtomElem))
                {
                    request.MtomKg = mtomElem.GetDouble();
                }

                // Kinetic Energy
                if (groundRisk.TryGetProperty("KineticEnergy", out var ke))
                {
                    request.KineticEnergy = ke.GetDouble();
                }

                // Max Characteristic Dimension
                if (groundRisk.TryGetProperty("MaxCharacteristicDimension", out var dimension))
                {
                    request.MaxCharacteristicDimension = dimension.GetDouble();
                }

                // Fix #1/#8: Do NOT compute iGRC in proxy from global thresholds. Full 2.0 run is handled by orchestration.
                request.InitialGrc = null;

                // Initialize mitigations to default values (no reduction)
                request.M1 = 0;  // Default: no M1 mitigation applied
                request.M3 = 0;  // Default: no M3 mitigation applied
                _logger.LogDebug("Initialized M1=0, M3=0 as defaults");

                // Extract mitigations (will override defaults if present)
                if (groundRisk.TryGetProperty("Mitigations", out var mitigations) && mitigations.ValueKind == JsonValueKind.Array)
                {
                    foreach (var mitigation in mitigations.EnumerateArray())
                    {
                        if (!mitigation.TryGetProperty("Type", out var typeElem) ||
                            !mitigation.TryGetProperty("Robustness", out var robustnessElem))
                        {
                            continue;
                        }

                        var type = typeElem.GetString();
                        var robustness = robustnessElem.GetString();

                        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(robustness))
                        {
                            continue;
                        }

                        switch (type.ToUpper())
                        {
                            case "M1":
                                request.M1 = MapRobustnessToGrcReduction(robustness);
                                _logger.LogDebug("M1 set to {Value} for robustness {Robustness}", request.M1, robustness);
                                break;
                            case "M2":
                                request.M2 = robustness;
                                _logger.LogDebug("M2 set to {Robustness}", robustness);
                                break;
                            case "M3":
                                request.M3 = MapM3RobustnessToReduction(robustness);
                                _logger.LogDebug("M3 set to {Value} for robustness {Robustness}", request.M3, robustness);
                                break;
                        }
                    }
                }

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting SORA 2.0 data from mission.html format");
                throw;
            }
        }

        private SoraCompleteRequest ExtractSora25FromMissionHtml(JsonElement json)
        {
            var request = new SoraCompleteRequest { Category = "SORA-2.5" };

            try
            {
                // Ground Risk
                if (json.TryGetProperty("GroundRisk", out var groundRisk))
                {
                    // Population Density for GRC
                    if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
                    {
                        var popValue = popDensity.GetDouble();
                        request.GrcInputs = new GrcInputs
                        {
                            PopulationDensity = popValue,
                            MaximumUasDimension = 1.2,
                            MtomKg = 25.0
                        };

                        // Extract dimension if available
                        if (groundRisk.TryGetProperty("MaxCharacteristicDimension", out var dimElem))
                        {
                            request.GrcInputs.MaximumUasDimension = dimElem.GetDouble();
                        }

                        // Evidence capture (Fix #11/#14): optional metadata
                        if (groundRisk.TryGetProperty("PopulationDensitySource", out var srcElem))
                            request.PopulationDensitySource = srcElem.GetString();
                        if (groundRisk.TryGetProperty("PopulationDensityWindow", out var winElem))
                            request.PopulationDensityWindow = winElem.GetString();
                    }

                    // Fix #8: Do NOT hard-map density to L/M/H in proxy; calculation handled by backend models.
                    request.InitialGrc = null;
                }

                // Air Risk
                if (json.TryGetProperty("AirRisk", out var airRisk))
                {
                    request.OperationalVolume = new OperationalVolume();

                    // Extract environment (Fix #3): Normalize Suburban -> Urban
                    if (airRisk.TryGetProperty("Environment", out var envElem))
                    {
                        var env = envElem.GetString() ?? "Rural";
                        request.OperationalVolume.Environment = string.Equals(env, "Suburban", StringComparison.OrdinalIgnoreCase)
                            ? "Urban" : env;
                    }

                    // Extract airspace info
                    if (airRisk.TryGetProperty("AirspaceClass", out var classElem))
                    {
                        var cls = classElem.GetString() ?? "G";
                        request.OperationalVolume.AirspaceClass = string.IsNullOrWhiteSpace(cls) ? "G" : cls.Trim().ToUpperInvariant();
                    }

                    // Traffic density
                    if (airRisk.TryGetProperty("TrafficDensity", out var trafficElem))
                    {
                        request.TrafficDensity = trafficElem.GetString() ?? "Low";
                    }

                    // Enhanced ARC inputs for 2.5
                    request.ArcInputs25 = new ArcInputs25
                    {
                        USpaceServices = false,
                        TrafficDensitySource = "Empirical",
                        AirspaceContainment = "None",
                        TemporalSegregation = false,
                        SpatialSegregation = false
                    };

                    // Extract enhanced fields
                    if (airRisk.TryGetProperty("USpaceServices", out var uSpaceElem) && uSpaceElem.ValueKind == JsonValueKind.True)
                    {
                        request.ArcInputs25.USpaceServices = true;
                    }

                    if (airRisk.TryGetProperty("TrafficDensitySource", out var sourceElem))
                    {
                        request.ArcInputs25.TrafficDensitySource = sourceElem.GetString() ?? "Empirical";
                    }

                    if (airRisk.TryGetProperty("AirspaceContainment", out var containElem))
                    {
                        request.ArcInputs25.AirspaceContainment = containElem.GetString() ?? "None";
                    }

                    if (airRisk.TryGetProperty("TemporalSegregation", out var tempElem) && tempElem.ValueKind == JsonValueKind.True)
                    {
                        request.ArcInputs25.TemporalSegregation = true;
                    }

                    if (airRisk.TryGetProperty("SpatialSegregation", out var spatialElem) && spatialElem.ValueKind == JsonValueKind.True)
                    {
                        request.ArcInputs25.SpatialSegregation = true;
                    }

                    // Fix #2: Enforce explicit altitude semantics for SORA 2.5 (meters). No default 400 ft.
                    if (airRisk.TryGetProperty("MaxHeightAglM", out var altM))
                    {
                        request.OperationalVolume.MaxAltitudeM = altM.GetDouble();
                    }
                    else if (airRisk.TryGetProperty("MaxAltitudeFt", out var altFt))
                    {
                        // Accept ft input but convert to meters
                        request.OperationalVolume.MaxAltitudeM = altFt.GetDouble() / 3.28084;
                    }
                    else
                    {
                        throw new ArgumentException("AirRisk.MaxHeightAglM (meters) is required for SORA 2.5");
                    }
                }

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting SORA 2.5 data from mission.html format");
                throw;
            }
        }

        /// <summary>
        /// Execute full SORA 2.0 workflow via local orchestration (Steps #2–#8):
        /// - Ground Risk (Annex B), Air Risk (Annex C), SAIL, TMPR, OSO compliance
        /// Implements Fix #1 and keeps Annex B/C separation (#12), terminology hygiene (#13).
        /// </summary>
        private async Task<IActionResult> RunCompleteSora20Workflow(JsonElement missionJson)
        {
            // Build a minimal SORACompleteRequest payload for local orchestration
            var payload = new Dictionary<string, object?>
            {
                ["SoraVersion"] = "2.0",
            };

            // GroundRisk block
            if (missionJson.TryGetProperty("GroundRisk", out var groundRisk))
            {
                var gr = new Dictionary<string, object?>();
                if (groundRisk.TryGetProperty("Scenario_V2_0", out var scen))
                    gr["Scenario_V2_0"] = scen.GetString();
                if (groundRisk.TryGetProperty("MTOM_kg", out var mtom))
                    gr["MTOM_kg"] = mtom.GetDouble();
                if (groundRisk.TryGetProperty("MaxCharacteristicDimension", out var mcd))
                    gr["MaxCharacteristicDimension"] = mcd.GetDouble();
                if (groundRisk.TryGetProperty("MaxSpeed", out var maxSpd))
                    gr["MaxSpeed"] = maxSpd.GetDouble();
                if (groundRisk.TryGetProperty("KineticEnergy", out var ke))
                    gr["KineticEnergy"] = ke.GetDouble();
                if (groundRisk.TryGetProperty("PopulationDensity", out var pop))
                    gr["PopulationDensity"] = pop.GetDouble();

                // Mitigations array mapping
                if (groundRisk.TryGetProperty("Mitigations", out var mitig) && mitig.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<Dictionary<string, object?>>();
                    foreach (var m in mitig.EnumerateArray())
                    {
                        if (!m.TryGetProperty("Type", out var t) || !m.TryGetProperty("Robustness", out var r))
                            continue;
                        list.Add(new Dictionary<string, object?>
                        {
                            ["Type"] = t.GetString(),
                            ["Robustness"] = r.GetString()
                        });
                    }
                    if (list.Count > 0) gr["Mitigations"] = list;
                }
                payload["GroundRisk"] = gr;
            }

            // AirRisk block
            var ar = new Dictionary<string, object?>();
            if (missionJson.TryGetProperty("AirRisk", out var airRisk))
            {
                // Airspace control: derive from ICAO class if provided
                if (airRisk.TryGetProperty("AirspaceClass", out var cls))
                {
                    var val = (cls.GetString() ?? "G").Trim().ToUpperInvariant();
                    ar["AirspaceControl"] = (val is "A" or "B" or "C" or "D" or "E") ? "Controlled" : "Uncontrolled";
                }
                // Location type
                if (airRisk.TryGetProperty("IsAirportHeliport", out var isAp) && isAp.ValueKind == JsonValueKind.True)
                    ar["LocationType"] = "Airport";
                else
                    ar["LocationType"] = "NonAirport";
                // Environment with normalization (Fix #3)
                if (airRisk.TryGetProperty("Environment", out var env))
                {
                    var e = env.GetString() ?? "Rural";
                    ar["Environment"] = string.Equals(e, "Suburban", StringComparison.OrdinalIgnoreCase) ? "Urban" : e;
                }
                // Explicit altitude (meters preferred)
                if (airRisk.TryGetProperty("MaxHeightAglM", out var mAlt))
                    ar["MaxHeightAGL"] = mAlt.GetDouble();
                else if (airRisk.TryGetProperty("MaxAltitudeFt", out var fAlt))
                    ar["MaxHeightAGL"] = fAlt.GetDouble() / 3.28084;
                // Atypical/segregated pathway flag (Fix #10)
                if (airRisk.TryGetProperty("IsAtypicalSegregated", out var atyp) && atyp.ValueKind == JsonValueKind.True)
                    ar["IsAtypicalSegregated"] = true;
                // Strategic mitigations (pass-through tokens)
                if (airRisk.TryGetProperty("StrategicMitigations", out var sm) && sm.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var s in sm.EnumerateArray()) if (s.ValueKind == JsonValueKind.String) list.Add(s.GetString()!);
                    if (list.Count > 0) ar["StrategicMitigations"] = list;
                }
            }
            payload["AirRisk"] = ar;

            // Call local orchestration endpoint
            var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var localEndpoint = "http://localhost:5210/api/sail/calculate";
            _logger.LogInformation("Forwarding SORA 2.0 complete to local orchestration: {Url}", localEndpoint);
            var resp = await _httpClient.PostAsync(localEndpoint, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Local orchestration error {Status}: {Body}", resp.StatusCode, body);
                return StatusCode((int)resp.StatusCode, new { error = "SORA 2.0 orchestration failed", details = body });
            }
            _logger.LogInformation("SORA 2.0 orchestration succeeded");
            return Ok(JsonDocument.Parse(body).RootElement);
        }

        private async Task<IActionResult> ForwardToPythonAndTransform(object request, string pythonEndpoint, string category)
        {
            try
            {
                // Serialize with snake_case for Python API
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                _logger.LogInformation("Forwarding to Python API: {Endpoint}", pythonEndpoint);
                _logger.LogDebug("Request payload: {Payload}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.PostAsync($"{PYTHON_API_BASE}{pythonEndpoint}", content);
                }
                catch (HttpRequestException hex)
                {
                    _logger.LogError(hex, "Cannot connect to Python API at {BaseUrl}", PYTHON_API_BASE);
                    return StatusCode(503, new 
                    { 
                        error = "Python API unavailable",
                        details = hex.Message,
                        hint = $"Ensure Python API is running on {PYTHON_API_BASE}"
                    });
                }
                catch (TaskCanceledException)
                {
                    _logger.LogError("Python API request timed out");
                    return StatusCode(504, new { error = "Python API request timed out" });
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Python API response status: {Status}", response.StatusCode);
                _logger.LogDebug("Python API response: {Response}", resultJson);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Python API error {StatusCode}: {Error}", response.StatusCode, resultJson);
                    
                    // Try to parse error response
                    try
                    {
                        var errorDoc = JsonDocument.Parse(resultJson);
                        return StatusCode((int)response.StatusCode, errorDoc.RootElement);
                    }
                    catch
                    {
                        return StatusCode((int)response.StatusCode, new 
                        { 
                            error = "Python API error",
                            status = (int)response.StatusCode,
                            details = resultJson 
                        });
                    }
                }

                // Parse Python response
                JsonDocument pythonResponse;
                try
                {
                    pythonResponse = JsonDocument.Parse(resultJson);
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "Failed to parse Python API response");
                    return StatusCode(500, new 
                    { 
                        error = "Invalid JSON from Python API", 
                        response = resultJson,
                        details = jex.Message 
                    });
                }

                // Transform response for frontend compatibility
                var transformedResponse = new Dictionary<string, object?>();
                var root = pythonResponse.RootElement;

                // Always include status
                transformedResponse["status"] = "success";
                transformedResponse["category"] = category;

                // GRC values - support both naming conventions
                if (root.TryGetProperty("initial_grc", out var initialGrc))
                {
                    var grcValue = initialGrc.GetInt32();
                    transformedResponse["initial_grc"] = grcValue;
                    transformedResponse["intrinsicGRC"] = grcValue; // Legacy support
                    transformedResponse["initialGRC"] = grcValue; // Alt naming
                }

                if (root.TryGetProperty("final_grc", out var finalGrc))
                {
                    var grcValue = finalGrc.GetInt32();
                    transformedResponse["final_grc"] = grcValue;
                    transformedResponse["finalGRC"] = grcValue; // Alt naming
                }

                // SORA 2.0 specific fields
                if (category == "SORA-2.0")
                {
                    // Mitigation values
                    if (root.TryGetProperty("m1", out var m1))
                    {
                        transformedResponse["m1"] = m1.GetInt32();
                    }

                    if (root.TryGetProperty("m2", out var m2))
                    {
                        transformedResponse["m2"] = m2.GetString();
                        
                        // Extract m2_value for correct display
                        if (root.TryGetProperty("m2_value", out var m2Value))
                        {
                            transformedResponse["m2_value"] = m2Value.GetInt32();
                        }
                        else
                        {
                            // Calculate if missing
                            var m2Text = m2.GetString()?.ToLower();
                            transformedResponse["m2_value"] = m2Text switch
                            {
                                "low" => -1,
                                "medium" => -2,
                                "high" => -3,
                                _ => 0
                            };
                        }
                    }

                    if (root.TryGetProperty("m3", out var m3))
                    {
                        transformedResponse["m3"] = m3.GetInt32();
                    }

                    if (root.TryGetProperty("calculation", out var calc))
                    {
                        transformedResponse["calculation"] = calc.GetString();
                    }

                    if (root.TryGetProperty("reference", out var reference))
                    {
                        transformedResponse["reference"] = reference.GetString();
                    }
                }

                // SORA 2.5 specific fields
                if (category == "SORA-2.5")
                {
                    // ARC values
                    if (root.TryGetProperty("initial_arc", out var initialArc))
                    {
                        var arcValue = initialArc.GetInt32();
                        transformedResponse["initial_arc"] = arcValue;
                        transformedResponse["initialARC"] = arcValue; // Alt naming
                    }

                    if (root.TryGetProperty("residual_arc", out var residualArc))
                    {
                        var arcValue = residualArc.GetInt32();
                        transformedResponse["residual_arc"] = arcValue;
                        transformedResponse["residualARC"] = arcValue; // Alt naming
                    }

                    // Strategic mitigations
                    if (root.TryGetProperty("strategic_mitigations", out var mitigations) && 
                        mitigations.ValueKind == JsonValueKind.Array)
                    {
                        var mitigationList = new List<string>();
                        foreach (var mitigation in mitigations.EnumerateArray())
                        {
                            if (mitigation.ValueKind == JsonValueKind.String)
                            {
                                mitigationList.Add(mitigation.GetString() ?? "");
                            }
                        }
                        transformedResponse["strategic_mitigations"] = mitigationList;
                    }

                    // Enhanced risk analysis
                    if (root.TryGetProperty("enhanced_analysis", out var enhanced))
                    {
                        transformedResponse["enhanced_analysis"] = enhanced.GetRawText();
                    }
                }

                // Common fields
                if (root.TryGetProperty("sail", out var sail))
                {
                    var sailValue = sail.GetString() ?? "Unknown";
                    transformedResponse["sail"] = sailValue;
                    transformedResponse["finalSAIL"] = sailValue; // Alt naming
                }

                if (root.TryGetProperty("summary", out var summary))
                {
                    transformedResponse["summary"] = summary.GetString();
                }

                // Include raw response for debugging
                transformedResponse["_raw_response"] = resultJson;

                pythonResponse.Dispose();

                _logger.LogInformation("Successfully processed {Category} assessment", category);
                return Ok(transformedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForwardToPythonAndTransform");
                return StatusCode(500, new 
                { 
                    error = "Error processing Python API response", 
                    details = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Helper methods
        private string? ExtractSheltering(JsonElement groundRisk)
        {
            if (groundRisk.TryGetProperty("Sheltering", out var shelteringElem))
            {
                return shelteringElem.GetString();
            }
            
            // Default based on population density if not specified
            if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
            {
                var density = popDensity.GetDouble();
                if (density < 500) return "High";  // Rural/sparse = good sheltering
                if (density < 10000) return "Medium"; // Suburban
                return "Low"; // Urban
            }

            return "High"; // Safe default
        }

        private int CalculateInitialGrc(string populationDensity, string sheltering)
        {
            // Based on JARUS SORA 2.0 Table 3
            return (populationDensity.ToLower(), sheltering.ToLower()) switch
            {
                ("low", "high") => 1,
                ("low", "medium") => 2,
                ("low", "low") => 3,
                ("medium", "high") => 2,
                ("medium", "medium") => 3,
                ("medium", "low") => 4,
                ("high", "high") => 3,
                ("high", "medium") => 4,
                ("high", "low") => 5,
                _ => 3 // Default
            };
        }

        private int CalculateInitialGrc25(string populationDensity)
        {
            // SORA 2.5 simplified GRC calculation
            return populationDensity.ToLower() switch
            {
                "low" => 2,
                "medium" => 3,
                "high" => 4,
                _ => 3
            };
        }

        private int MapRobustnessToGrcReduction(string robustness)
        {
            return robustness.ToLower() switch
            {
                "low" => -1,
                "medium" => -2,
                "high" => -3,
                _ => 0
            };
        }

        private int MapM3RobustnessToReduction(string robustness)
        {
            // M3 typically has no reduction for Low/Medium, -1 for High
            return robustness.ToLower() switch
            {
                "high" => -1,
                _ => 0
            };
        }

        // Model classes with nullable properties
        private class SoraCompleteRequest
        {
            public string? Category { get; set; }
            // Deprecated string mapping removed; keep numeric density and evidence metadata
            public double? PopulationDensityValue { get; set; }
            public string? PopulationDensitySource { get; set; }
            public string? PopulationDensityWindow { get; set; }
            public string? Sheltering { get; set; }
            public string? Scenario_V2_0 { get; set; }
            public double? MtomKg { get; set; }
            public double? KineticEnergy { get; set; }
            public double? MaxCharacteristicDimension { get; set; }
            public int? InitialGrc { get; set; }
            public int? M1 { get; set; }
            public string? M2 { get; set; }
            public int? M3 { get; set; }
            public OperationalVolume? OperationalVolume { get; set; }
            public string? TrafficDensity { get; set; }
            public ArcInputs25? ArcInputs25 { get; set; }
            public GrcInputs? GrcInputs { get; set; }
        }

        private class OperationalVolume
        {
            // Fix #2: prefer meters for SORA 2.5; accept ft via conversion during extraction
            public double? MaxAltitudeM { get; set; }
            public double MaxRadiusKm { get; set; }
            public string? OperationClass { get; set; }
            public string? Location { get; set; }
            public string? Environment { get; set; }
            public string? AirspaceClass { get; set; }
        }

        private class ArcInputs25
        {
            public bool USpaceServices { get; set; }
            public string? TrafficDensitySource { get; set; }
            public string? AirspaceContainment { get; set; }
            public bool TemporalSegregation { get; set; }
            public bool SpatialSegregation { get; set; }
        }

        private class GrcInputs
        {
            public double PopulationDensity { get; set; }
            public double MaximumUasDimension { get; set; }
            public double MtomKg { get; set; }
        }
    }
}
