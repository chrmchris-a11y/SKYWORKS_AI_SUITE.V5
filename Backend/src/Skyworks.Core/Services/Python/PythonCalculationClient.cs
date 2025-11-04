using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;

namespace Skyworks.Core.Services.Python;

/// <summary>
/// HTTP Client for Python Calculation Microservice
/// Calls Python FastAPI service on port 8001 for accurate EASA/JARUS calculations
/// </summary>
public interface IPythonCalculationClient
{
    Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request);
    Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request);
    Task<PythonARCResponse?> CalculateARC_2_0(PythonARCRequest_2_0 request);
    Task<PythonARCResponse?> CalculateARC_2_5(PythonARCRequest_2_5 request);
    Task<PythonSAILResponse?> CalculateSAIL(PythonSAILRequest request);
    Task<bool> HealthCheck();
}

public class PythonCalculationClient : IPythonCalculationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonCalculationClient> _logger;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public PythonCalculationClient(IHttpClientFactory httpClientFactory, ILogger<PythonCalculationClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("PythonService");
        _logger = logger;
        // Prefer appsettings, then environment variable, then default
        _baseUrl = configuration.GetValue<string>("PythonService:BaseUrl")
                   ?? Environment.GetEnvironmentVariable("PYTHON_API_BASE")
                   ?? "http://localhost:8001";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        
        // Configure JSON options to match Python expectations
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<bool> HealthCheck()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python service health check failed");
            return false;
        }
    }

    public async Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request)
    {
        try
        {
            // Create a clean request object that matches Python Pydantic model exactly
            var pythonRequest = new Dictionary<string, object?>
            {
                ["mtom_kg"] = request.MTOM_kg,
                ["population_density"] = request.PopulationDensity
            };

            // Only add mitigation fields if they have values
            if (!string.IsNullOrEmpty(request.M1Strategic))
            {
                pythonRequest["m1_strategic"] = NormalizeMitigationLevel(request.M1Strategic);
            }

            if (!string.IsNullOrEmpty(request.M2Impact))
            {
                pythonRequest["m2_impact"] = NormalizeMitigationLevel(request.M2Impact);
            }

            if (!string.IsNullOrEmpty(request.M3ERP))
            {
                pythonRequest["m3_erp"] = NormalizeMitigationLevel(request.M3ERP);
            }

            var json = JsonSerializer.Serialize(pythonRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending GRC 2.0 request to Python: {Json}", json);
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.0", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Python API returned {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Python GRC 2.0 response: {Response}", responseJson);

            // First try strict model deserialization
            var parsed = JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, _jsonOptions);
            if (parsed != null && (parsed.IntrinsicGRC > 0 || parsed.FinalGRC > 0))
                return parsed;

            // Fallback: manual parse supporting intrinsic_grc | initial_grc shapes
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                int intrinsic = 0;
                int final = 0;
                int m1 = 0, m2 = 0, m3 = 0;
                string notes = string.Empty;
                string source = string.Empty;

                if (root.TryGetProperty("intrinsic_grc", out var ig)) intrinsic = ig.GetInt32();
                if (intrinsic == 0 && root.TryGetProperty("initial_grc", out var initg)) intrinsic = initg.GetInt32();
                if (root.TryGetProperty("final_grc", out var fg)) final = fg.GetInt32();
                if (root.TryGetProperty("m1_effect", out var m1e)) m1 = m1e.GetInt32();
                if (root.TryGetProperty("m2_effect", out var m2e)) m2 = m2e.GetInt32();
                if (root.TryGetProperty("m3_effect", out var m3e)) m3 = m3e.GetInt32();
                if (root.TryGetProperty("notes", out var n)) notes = n.GetString() ?? string.Empty;
                if (root.TryGetProperty("source", out var s)) source = s.GetString() ?? string.Empty;

                if (intrinsic > 0 || final > 0)
                {
                    return new PythonGRCResponse
                    {
                        IntrinsicGRC = intrinsic,
                        FinalGRC = final,
                        M1Effect = m1,
                        M2Effect = m2,
                        M3Effect = m3,
                        Notes = notes,
                        Source = source
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GRC 2.0: Could not parse alternate response shapes");
            }

            return parsed; // may be null
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python GRC 2.0 call failed - {Message}", ex.Message);
            return null;
        }
    }

    public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
    {
        try
        {
            // Create a clean request object for SORA 2.5
            var pythonRequest = new Dictionary<string, object?>
            {
                ["mtom_kg"] = request.MTOM_kg,
                ["population_density"] = request.PopulationDensity
            };

            // Add SORA 2.5 authoritative fields (Table 2)
            if (request.MaxCharacteristicDimensionM.HasValue)
            {
                pythonRequest["max_characteristic_dimension_m"] = request.MaxCharacteristicDimensionM.Value;
            }

            // Add legacy max_dimension_m field for backward compatibility with Python calculator
            if (request.MaxDimensionM.HasValue)
            {
                pythonRequest["max_dimension_m"] = request.MaxDimensionM.Value;
            }

            // Add max_speed_ms if provided
            if (request.MaxSpeedMs.HasValue)
            {
                pythonRequest["max_speed_ms"] = request.MaxSpeedMs.Value;
            }

            if (!string.IsNullOrEmpty(request.OperationMode))
            {
                pythonRequest["operation_mode"] = request.OperationMode;
            }

            if (!string.IsNullOrEmpty(request.OverflownArea))
            {
                pythonRequest["overflown_area"] = request.OverflownArea;
            }

            // Add mitigations if they exist
            if (!string.IsNullOrEmpty(request.M1A_Sheltering))
            {
                pythonRequest["m1a_sheltering"] = NormalizeMitigationLevel(request.M1A_Sheltering);
            }

            if (!string.IsNullOrEmpty(request.M1B_Operational))
            {
                pythonRequest["m1b_operational"] = NormalizeMitigationLevel(request.M1B_Operational);
            }

            if (!string.IsNullOrEmpty(request.M1C_GroundObservation))
            {
                pythonRequest["m1c_ground_observation"] = NormalizeMitigationLevel(request.M1C_GroundObservation);
            }

            if (!string.IsNullOrEmpty(request.M2Impact))
            {
                // FIX #12: SORA 2.5 M2 only accepts None/Medium/High (no Low) - map Low→None
                pythonRequest["m2_impact"] = NormalizeMitigationLevel_SORA25_M2(request.M2Impact);
            }

            var json = JsonSerializer.Serialize(pythonRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending GRC 2.5 request to Python: {Json}", json);
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.5", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Python API returned {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Python GRC 2.5 response: {Response}", responseJson);

            // Unwrap optional envelope { ok, data, error }
            try
            {
                using JsonDocument envDoc = JsonDocument.Parse(responseJson);
                var root = envDoc.RootElement;
                if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object)
                {
                    responseJson = dataEl.GetRawText();
                }
            }
            catch { /* fall through with original responseJson */ }
            
            // First, try normal deserialization (snake_case fields)
            var result = JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, _jsonOptions);
            if (result != null && (result.IntrinsicGRC > 0 || result.FinalGRC > 0))
            {
                return result;
            }

            // Fallback: parse snake_case or camelCase manually
            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                int intrinsic = 0;
                int final = 0;
                int m1 = 0, m2 = 0, m3 = 0;
                string dimensionCategory = string.Empty;
                string notes = string.Empty;
                string source = string.Empty;

                // Prefer snake_case
                if (root.TryGetProperty("intrinsic_grc", out var ig)) intrinsic = ig.GetInt32();
                if (root.TryGetProperty("final_grc", out var fg)) final = fg.GetInt32();
                // Older/alternate schema uses "initial_grc" instead of "intrinsic_grc"
                if (intrinsic == 0 && root.TryGetProperty("initial_grc", out var initg)) intrinsic = initg.GetInt32();
                if (root.TryGetProperty("m1_effect", out var m1e)) m1 = m1e.GetInt32();
                if (root.TryGetProperty("m2_effect", out var m2e)) m2 = m2e.GetInt32();
                if (root.TryGetProperty("m3_effect", out var m3e)) m3 = m3e.GetInt32();
                if (root.TryGetProperty("dimension_category", out var dc)) dimensionCategory = dc.GetString() ?? string.Empty;
                if (root.TryGetProperty("notes", out var n)) notes = n.GetString() ?? string.Empty;
                if (root.TryGetProperty("source", out var s)) source = s.GetString() ?? string.Empty;

                // If snake_case not present, try camelCase
                if (intrinsic == 0 && final == 0)
                {
                    if (root.TryGetProperty("intrinsicGRC", out var igc)) intrinsic = igc.GetInt32();
                    if (root.TryGetProperty("finalGRC", out var fgc)) final = fgc.GetInt32();
                    if (intrinsic == 0 && root.TryGetProperty("initialGRC", out var initGc)) intrinsic = initGc.GetInt32();
                    if (root.TryGetProperty("m1Effect", out var m1c)) m1 = m1c.GetInt32();
                    if (root.TryGetProperty("m2Effect", out var m2c)) m2 = m2c.GetInt32();
                    if (root.TryGetProperty("m3Effect", out var m3c)) m3 = m3c.GetInt32();
                    if (root.TryGetProperty("dimensionCategory", out var dcc)) dimensionCategory = dcc.GetString() ?? string.Empty;
                    if (root.TryGetProperty("notes", out var nc)) notes = nc.GetString() ?? string.Empty;
                    if (root.TryGetProperty("reference", out var rc)) source = rc.GetString() ?? string.Empty;
                }

                if (intrinsic > 0 || final > 0)
                {
                    return new PythonGRCResponse
                    {
                        IntrinsicGRC = intrinsic,
                        FinalGRC = final,
                        M1Effect = m1,
                        M2Effect = m2,
                        M3Effect = m3,
                        DimensionCategory = dimensionCategory,
                        Notes = notes,
                        Source = source
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GRC 2.5: Could not parse alternate response shapes");
            }
            
            return result; // may be null -> caller will handle
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python GRC 2.5 call failed - {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Normalize mitigation level strings to match Python enum expectations exactly
    /// </summary>
    private string? NormalizeMitigationLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return null;

        return level.Trim().ToLowerInvariant() switch
        {
            "low" or "Low" or "LOW" => "Low",
            "medium" or "Medium" or "MEDIUM" => "Medium",
            "high" or "High" or "HIGH" => "High",
            _ => level.Trim() // Return as-is if not recognized
        };
    }

    /// <summary>
    /// FIX #12: SORA 2.5 M2 normalization - Low→None (M2Level25 enum: None/Medium/High only)
    /// </summary>
    private string? NormalizeMitigationLevel_SORA25_M2(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return "None";

        return level.Trim().ToLowerInvariant() switch
        {
            "low" or "Low" or "LOW" or "none" or "None" or "NONE" => "None",
            "medium" or "Medium" or "MEDIUM" => "Medium",
            "high" or "High" or "HIGH" => "High",
            _ => "None" // Default to None if not recognized
        };
    }

    public async Task<PythonARCResponse?> CalculateARC_2_0(PythonARCRequest_2_0 request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending ARC 2.0 request to Python: {Json}", json);
            
            var response = await _httpClient.PostAsync("/arc/v2.0/initial", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Python ARC 2.0 API returned {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            // FastAPI may return an envelope { ok, data, error } with snake_case fields
            // Normalize to PythonARCResponse, tolerating both residual_arc|final_arc and initial_arc
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                // Unwrap envelope if present
                if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object)
                {
                    var data = dataEl;
                    return ParseArcResponseObject(data);
                }

                // No envelope; parse directly
                return ParseArcResponseObject(root);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ARC 2.0: Could not parse response, returning null");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python ARC 2.0 call failed - {Message}", ex.Message);
            return null;
        }
    }

    public async Task<PythonARCResponse?> CalculateARC_2_5(PythonARCRequest_2_5 request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending ARC 2.5 request to Python: {Json}", json);
            
            var response = await _httpClient.PostAsync("/arc/v2.5/initial", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Python ARC 2.5 API returned {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            // Normalize to PythonARCResponse, tolerating both residual_arc|final_arc and initial_arc
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                // Unwrap envelope if present
                if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object)
                {
                    var data = dataEl;
                    return ParseArcResponseObject(data);
                }

                // No envelope; parse directly
                return ParseArcResponseObject(root);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ARC 2.5: Could not parse response, returning null");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python ARC 2.5 call failed - {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Helper to parse ARC response objects from Python, supporting multiple shapes.
    /// </summary>
    private PythonARCResponse ParseArcResponseObject(JsonElement obj)
    {
        // Prefer explicit fields if present
        string initialArc = string.Empty;
        string finalArc = string.Empty;
        int aec = 0;
        int density = 0;
        int tme = 0;
        string notes = string.Empty;
        string source = string.Empty;

        // Accept both snake_case and camelCase
        if (obj.TryGetProperty("initial_arc", out var ia)) initialArc = ia.GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(initialArc) && obj.TryGetProperty("initialARC", out var ia2)) initialArc = ia2.GetString() ?? string.Empty;

        // final_arc (legacy) or residual_arc (authoritative)
        if (obj.TryGetProperty("final_arc", out var fa)) finalArc = fa.GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(finalArc) && obj.TryGetProperty("residual_arc", out var ra)) finalArc = ra.GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(finalArc) && obj.TryGetProperty("finalARC", out var fa2)) finalArc = fa2.GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(finalArc) && obj.TryGetProperty("residualARC", out var ra2)) finalArc = ra2.GetString() ?? string.Empty;

        // Fallback: if final/residual not present, use initial as best-effort so downstream SAIL calls don't 400
        if (string.IsNullOrEmpty(finalArc) && !string.IsNullOrEmpty(initialArc))
        {
            finalArc = initialArc;
        }

        if (obj.TryGetProperty("aec", out var aecEl) && aecEl.ValueKind == JsonValueKind.Number) aec = aecEl.GetInt32();
        if (obj.TryGetProperty("density_rating", out var dr) && dr.ValueKind == JsonValueKind.Number) density = dr.GetInt32();
        if (obj.TryGetProperty("tactical_mitigation_effect", out var tmeEl) && tmeEl.ValueKind == JsonValueKind.Number) tme = tmeEl.GetInt32();
        if (obj.TryGetProperty("notes", out var n)) notes = n.GetString() ?? string.Empty;
        if (obj.TryGetProperty("source", out var s)) source = s.GetString() ?? string.Empty;

        return new PythonARCResponse
        {
            InitialARC = initialArc,
            FinalARC = finalArc,
            AEC = aec,
            DensityRating = density,
            TacticalMitigationEffect = tme,
            Notes = notes,
            Source = source
        };
    }

    public async Task<PythonSAILResponse?> CalculateSAIL(PythonSAILRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Sending SAIL request to Python: {Json}", json);
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/sail", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Python SAIL API returned {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PythonSAILResponse>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python SAIL call failed - {Message}", ex.Message);
            return null;
        }
    }
}

// ============================================================================
// PYTHON API REQUEST/RESPONSE MODELS
// ============================================================================

#region GRC Models

public class PythonGRCRequest_2_0
{
    [JsonPropertyName("mtom_kg")]
    public double MTOM_kg { get; set; }

    // SORA 2.0 Table 2: scenario string (e.g., "VLOS_Controlled", "BVLOS_Populated")
    [JsonPropertyName("scenario")]
    public string Scenario { get; set; } = "VLOS_Sparsely";

    // SORA 2.0 Table 2: aircraft characteristic dimension in meters
    [JsonPropertyName("dimension_m")]
    public double DimensionM { get; set; }

    // Footprint containment quality: "Poor", "Adequate", "Good"
    [JsonPropertyName("containment_quality")]
    public string ContainmentQuality { get; set; } = "Adequate";

    // Deprecated: kept for backward compatibility, use scenario instead
    [JsonPropertyName("population_density")]
    public double PopulationDensity { get; set; }

    [JsonPropertyName("m1_strategic")]
    public string? M1Strategic { get; set; }

    [JsonPropertyName("m2_impact")]
    public string? M2Impact { get; set; }

    [JsonPropertyName("m3_erp")]
    public string? M3ERP { get; set; }
    
    // Deprecated: kept for backward compatibility, use scenario instead
    [JsonPropertyName("environment_type")]
    public string? EnvironmentType { get; set; }
}

public class PythonGRCRequest_2_5
{
    [JsonPropertyName("mtom_kg")]
    public double MTOM_kg { get; set; }

    [JsonPropertyName("population_density")]
    public double PopulationDensity { get; set; }

    // Optional context to support Python models that consider dimension/speed
    [JsonPropertyName("max_dimension_m")]
    public double? MaxDimensionM { get; set; }

    [JsonPropertyName("max_speed_ms")]
    public double? MaxSpeedMs { get; set; }

    // SORA 2.5 Authoritative: Table 2 requires scenario + dimension
    [JsonPropertyName("max_characteristic_dimension_m")]
    public double? MaxCharacteristicDimensionM { get; set; }

    [JsonPropertyName("operation_mode")]
    public string? OperationMode { get; set; }  // "VLOS" or "BVLOS"

    [JsonPropertyName("overflown_area")]
    public string? OverflownArea { get; set; }  // "Sparsely", "Populated", "Gathering"

    [JsonPropertyName("m1a_sheltering")]
    public string? M1A_Sheltering { get; set; }

    [JsonPropertyName("m1b_operational")]
    public string? M1B_Operational { get; set; }

    [JsonPropertyName("m1c_ground_observation")]
    public string? M1C_GroundObservation { get; set; }

    [JsonPropertyName("m2_impact")]
    public string? M2Impact { get; set; }
    
    [JsonPropertyName("environment_type")]
    public string? EnvironmentType { get; set; }
}

public class PythonGRCResponse
{
    [JsonPropertyName("intrinsic_grc")]
    public int IntrinsicGRC { get; set; }

    [JsonPropertyName("final_grc")]
    public int FinalGRC { get; set; }

    [JsonPropertyName("m1_effect")]
    public int M1Effect { get; set; }

    [JsonPropertyName("m2_effect")]
    public int M2Effect { get; set; }

    [JsonPropertyName("m3_effect")]
    public int M3Effect { get; set; }

    [JsonPropertyName("dimension_category")]
    public string DimensionCategory { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

#endregion

#region ARC Models

public class PythonARCRequest_2_0
{
    // CRITICAL FIX: Use correct field name that Python expects
    [JsonPropertyName("altitude_agl_ft")]
    public double AltitudeAglFt { get; set; }

    [JsonPropertyName("max_height_amsl_m")]
    public double MaxHeightAmslM { get; set; }

    [JsonPropertyName("airspace_class")]
    public string AirspaceClass { get; set; } = "G";

    [JsonPropertyName("is_controlled")]
    public bool IsControlled { get; set; }

    [JsonPropertyName("is_modes_veil")]
    public bool IsModesVeil { get; set; }

    [JsonPropertyName("is_tmz")]
    public bool IsTmz { get; set; }

    [JsonPropertyName("environment")]
    public string Environment { get; set; } = "Rural";

    [JsonPropertyName("is_airport_heliport")]
    public bool IsAirportHeliport { get; set; }

    [JsonPropertyName("is_atypical_segregated")]
    public bool IsAtypicalSegregated { get; set; }

    [JsonPropertyName("tactical_mitigation_level")]
    public string TacticalMitigationLevel { get; set; } = "None";

    // Optional proximity fields (advisory)
    [JsonPropertyName("is_in_ctr")]
    public bool IsInCtr { get; set; }

    [JsonPropertyName("distance_to_aerodrome_nm")]
    public double? DistanceToAerodromeNm { get; set; }
}

public class PythonARCRequest_2_5
{
    // CRITICAL FIX: Use correct field name that Python expects
    [JsonPropertyName("altitude_agl_m")]
    public double AltitudeAglM { get; set; }

    [JsonPropertyName("max_speed_ms")]
    public double MaxSpeedMs { get; set; }

    [JsonPropertyName("airspace_class")]
    public string AirspaceClass { get; set; } = "G";

    [JsonPropertyName("is_controlled")]
    public bool IsControlled { get; set; }

    [JsonPropertyName("is_modes_veil")]
    public bool IsModesVeil { get; set; }

    [JsonPropertyName("is_tmz")]
    public bool IsTmz { get; set; }

    [JsonPropertyName("environment")]
    public string Environment { get; set; } = "Rural";

    [JsonPropertyName("is_airport_heliport")]
    public bool IsAirportHeliport { get; set; }

    [JsonPropertyName("is_atypical_segregated")]
    public bool IsAtypicalSegregated { get; set; }

    [JsonPropertyName("tactical_mitigation_level")]
    public string TacticalMitigationLevel { get; set; } = "None";

    // Optional proximity fields (advisory)
    [JsonPropertyName("is_in_ctr")]
    public bool IsInCtr { get; set; }

    [JsonPropertyName("distance_to_aerodrome_km")]
    public double? DistanceToAerodromeKm { get; set; }
}

public class PythonARCResponse
{
    [JsonPropertyName("initial_arc")]
    public string InitialARC { get; set; } = string.Empty;

    [JsonPropertyName("final_arc")]
    public string FinalARC { get; set; } = string.Empty;

    [JsonPropertyName("aec")]
    public int AEC { get; set; }

    [JsonPropertyName("density_rating")]
    public int DensityRating { get; set; }

    [JsonPropertyName("tactical_mitigation_effect")]
    public int TacticalMitigationEffect { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

#endregion

#region SAIL Models

public class PythonSAILRequest
{
    [JsonPropertyName("sora_version")]
    public string SoraVersion { get; set; } = "2.0";

    [JsonPropertyName("final_grc")]
    public int FinalGRC { get; set; }

    [JsonPropertyName("residual_arc")]  // FIXED: Python expects residual_arc, not final_arc
    public string ResidualARC { get; set; } = string.Empty;
}

public class PythonSAILResponse
{
    // Category C handling (SORA 2.5 GRC > 7)
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("sail")]
    public string SAIL { get; set; } = string.Empty;

    [JsonPropertyName("final_grc")]
    public int FinalGRC { get; set; }

    [JsonPropertyName("residual_arc")]
    public string? ResidualARC { get; set; }

    [JsonPropertyName("final_arc")]
    public string FinalARC { get; set; } = string.Empty;

    [JsonPropertyName("oso_count")]
    public int? OsoCount { get; set; }

    [JsonPropertyName("sora_version")]
    public string SoraVersion { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

#endregion
