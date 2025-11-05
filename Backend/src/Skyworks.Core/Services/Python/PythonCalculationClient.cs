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

    public PythonCalculationClient(IHttpClientFactory httpClientFactory, ILogger<PythonCalculationClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("PythonService");
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("PythonService:BaseUrl") ?? "http://localhost:8001";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
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
            // Normalize request: lowercase mitigation enums + scenario name conversion for Python compatibility
            var normalizedReq = new
            {
                max_dimension_m = request.MaxDimensionM > 0 ? request.MaxDimensionM : (request.DimensionM > 0 ? request.DimensionM : 1.0),
                operational_scenario = string.IsNullOrWhiteSpace(request.OperationalScenario) ? (request.Scenario ?? string.Empty) : request.OperationalScenario,
                m1_strategic = ToTitleCase(request.M1Strategic),
                m2_ground_impact = ToTitleCase(string.IsNullOrWhiteSpace(request.M2GroundImpact) ? (request.M2Impact ?? "") : request.M2GroundImpact),
                m3_emergency_response = ToTitleCase(string.IsNullOrWhiteSpace(request.M3EmergencyResponse) ? (request.M3ERP ?? "") : request.M3EmergencyResponse),
            };
            var json = JsonSerializer.Serialize(normalizedReq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.0", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Python GRC 2.0 calculation");
            throw;
        }
    }

    public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
    {
        try
        {
            // SORA 2.5 (Annex F) expects specific snake_case fields with extra=forbid.
            // Map strictly to Python model: mtom_kg, population_density, max_characteristic_dimension_m, max_speed_ms,
            // and granular mitigations m1a/m1b/m1c + m2_impact. Omit fields not recognized by FastAPI model.
            var normalizedReq = new
            {
                mtom_kg = request.WeightKg ?? (request.MTOM_kg > 0 ? (double?)request.MTOM_kg : (double?)null),
                population_density = request.PopulationDensity,
                max_characteristic_dimension_m = request.MaxCharacteristicDimensionM > 0 ? (double?)request.MaxCharacteristicDimensionM : (request.MaxDimensionM > 0 ? (double?)request.MaxDimensionM : (double?)null),
                max_speed_ms = request.MaxSpeedMs > 0 ? request.MaxSpeedMs : (request.MaxSpeed_mps > 0 ? request.MaxSpeed_mps : 0),
                m1a_sheltering = string.IsNullOrWhiteSpace(request.M1A_Sheltering) ? null : ToTitleCase(request.M1A_Sheltering!),
                m1b_operational = string.IsNullOrWhiteSpace(request.M1B_Operational) ? null : ToTitleCase(request.M1B_Operational!),
                m1c_ground_observation = string.IsNullOrWhiteSpace(request.M1C_GroundObservation) ? null : ToTitleCase(request.M1C_GroundObservation!),
                m2_impact = string.IsNullOrWhiteSpace(request.M2Impact) ? null : ToTitleCase(request.M2Impact!),
            };
            var json = JsonSerializer.Serialize(normalizedReq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.5", content);
            
            // Read response body before checking status (for error message extraction)
            var responseJson = await response.Content.ReadAsStringAsync();
            
            // If error response, try to extract structured error message (handles string, array, or object)
            if (!response.IsSuccessStatusCode)
            {
                string? detailMsg = null;
                try
                {
                    using var errorDoc = JsonDocument.Parse(responseJson);
                    var root = errorDoc.RootElement;
                    if (root.TryGetProperty("detail", out var detailProp))
                    {
                        switch (detailProp.ValueKind)
                        {
                            case JsonValueKind.String:
                                detailMsg = detailProp.GetString();
                                break;
                            case JsonValueKind.Array:
                                // Typical FastAPI/Pydantic validation error: array of {loc, msg, type}
                                var parts = new List<string>();
                                foreach (var item in detailProp.EnumerateArray())
                                {
                                    string loc = string.Empty;
                                    if (item.TryGetProperty("loc", out var locProp))
                                    {
                                        try { loc = string.Join('.', locProp.EnumerateArray().Select(e => e.ToString())); } catch { }
                                    }
                                    string msg = item.TryGetProperty("msg", out var msgProp) ? (msgProp.GetString() ?? string.Empty) : item.ToString();
                                    parts.Add(string.IsNullOrWhiteSpace(loc) ? msg : $"{loc}: {msg}");
                                }
                                detailMsg = string.Join("; ", parts);
                                break;
                            case JsonValueKind.Object:
                                detailMsg = detailProp.ToString();
                                break;
                        }
                    }
                    else
                    {
                        // No 'detail' property; include entire root as context
                        detailMsg = root.ToString();
                    }
                }
                catch (Exception)
                {
                    // If JSON parsing fails, fall through to EnsureSuccessStatusCode
                }

                if (!string.IsNullOrWhiteSpace(detailMsg))
                {
                    throw new HttpRequestException($"Python GRC 2.5 returned {(int)response.StatusCode}: {detailMsg}");
                }

                response.EnsureSuccessStatusCode(); // Throw with generic status code message
            }
            
            return JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Python GRC 2.5 calculation");
            throw;
        }
    }

    public async Task<PythonARCResponse?> CalculateARC_2_0(PythonARCRequest_2_0 request)
    {
        try
        {
            // Normalize request: title-case enums for Python compatibility
            var normalizedReq = new
            {
                max_height_agl_m = request.MaxHeightAglM > 0 ? request.MaxHeightAglM : (request.AltitudeAglFt > 0 ? request.AltitudeAglFt * 0.3048 : 0),
                max_height_amsl_m = request.MaxHeightAmslM,
                airspace_class = request.AirspaceClass,
                is_controlled = request.IsControlled,
                is_modes_veil = request.IsModesVeil,
                is_tmz = request.IsTmz,
                environment = ToTitleCase(request.Environment),
                is_airport_heliport = request.IsAirportHeliport,
                is_atypical_segregated = request.IsAtypicalSegregated,
                tactical_mitigation_level = ToTitleCase(request.TacticalMitigationLevel),
            };
            var json = JsonSerializer.Serialize(normalizedReq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/arc/2.0", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PythonARCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Python ARC 2.0 calculation");
            throw;
        }
    }

    public async Task<PythonARCResponse?> CalculateARC_2_5(PythonARCRequest_2_5 request)
    {
        try
        {
            // Normalize request: title-case enums for Python compatibility
            var normalizedReq = new
            {
                max_height_agl_m = request.MaxHeightAglM > 0 ? request.MaxHeightAglM : request.AltitudeAglM,
                max_speed_ms = request.MaxSpeedMs,
                airspace_class = request.AirspaceClass,
                is_controlled = request.IsControlled,
                is_modes_veil = request.IsModesVeil,
                is_tmz = request.IsTmz,
                environment = ToTitleCase(request.Environment),
                is_airport_heliport = request.IsAirportHeliport,
                is_atypical_segregated = request.IsAtypicalSegregated,
                tactical_mitigation_level = ToTitleCase(request.TacticalMitigationLevel),
            };
            var json = JsonSerializer.Serialize(normalizedReq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/arc/2.5", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PythonARCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Python ARC 2.5 calculation");
            throw;
        }
    }

    public async Task<PythonSAILResponse?> CalculateSAIL(PythonSAILRequest request)
    {
        try
        {
            object normalizedReq;
            // SORA 2.5 requires numeric residual_arc_level (1..10). For 2.0 keep legacy final_arc letter.
            if (string.Equals(request.SoraVersion, "2.5", StringComparison.OrdinalIgnoreCase))
            {
                normalizedReq = new
                {
                    sora_version = "2.5",
                    final_grc = request.FinalGRC,
                    residual_arc_level = request.ResidualARCLevel ?? 0,
                };
            }
            else
            {
                // Normalize request: SAIL 2.0 expects ARC enums like "ARC-a" (hyphens)
                normalizedReq = new
                {
                    sora_version = string.IsNullOrWhiteSpace(request.SoraVersion) ? "2.0" : request.SoraVersion,
                    final_grc = request.FinalGRC,
                    final_arc = ToARCEnum(string.IsNullOrWhiteSpace(request.ResidualARC) ? request.FinalARC : request.ResidualARC),
                };
            }
            var json = JsonSerializer.Serialize(normalizedReq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/calculate/sail", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PythonSAILResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Python SAIL calculation");
            throw;
        }
    }

    // Normalize uppercase enum strings to title-case for Python
    private string ToTitleCase(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s ?? string.Empty;
        var lower = s.ToLowerInvariant();
        if (lower == "none") return "None";
        if (lower == "low") return "Low";
        if (lower == "medium") return "Medium";
        if (lower == "high") return "High";
        // Environments
        if (lower == "rural") return "Rural";
        if (lower == "urban") return "Urban";
        if (lower == "suburban") return "Suburban";
        if (lower == "controlled") return "Controlled";
        if (lower == "industrial") return "Industrial";
        // Default: capitalize first letter
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }

    // Normalize ARC enum from e.g., "ARC_a" -> "ARC-a"
    private string ToARCEnum(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s ?? string.Empty;
        // Python ARCRating uses lowercase suffix with hyphens: ARC-a, ARC-b, ARC-c, ARC-d
        return s.Replace("_", "-");
    }

    // Combine M1 sub-mitigations into a single robustness level
    private string CombineM1(PythonGRCRequest_2_5 r)
    {
        var levels = new[] { r.M1A_Sheltering, r.M1B_Operational, r.M1C_GroundObservation }
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim().ToLowerInvariant())
            .ToList();
        if (levels.Count == 0) return "None";
        if (levels.Contains("high")) return "High";
        if (levels.Contains("medium")) return "Medium";
        if (levels.Contains("low")) return "Low";
        return "None";
    }

    // Convert .NET OperationalScenario enum to Python scenario string
    // Based on JARUS SORA 2.0 Table 2 scenario names
    private string ToScenarioString(string dotnetScenario)
    {
        return dotnetScenario switch
        {
            "VLOS_SparselyPopulated" => "VLOS_Sparsely",
            "BVLOS_SparselyPopulated" => "BVLOS_Sparsely",
            "VLOS_Populated" => "VLOS_Populated",
            "BVLOS_Populated" => "BVLOS_Populated",
            "VLOS_GatheringOfPeople" => "VLOS_Gathering",
            "BVLOS_GatheringOfPeople" => "BVLOS_Gathering",
            "ControlledGroundArea" => "VLOS_Controlled",
            _ => dotnetScenario // Pass through if already correct
        };
    }
}

// ============================================================================
// PYTHON API REQUEST/RESPONSE MODELS
// ============================================================================

#region GRC Models

public class PythonGRCRequest_2_0
{
    [JsonPropertyName("max_dimension_m")]
    public double MaxDimensionM { get; set; }

    [JsonPropertyName("operational_scenario")]
    public string OperationalScenario { get; set; } = string.Empty;

    [JsonPropertyName("m1_strategic")]
    public string M1Strategic { get; set; } = "None";

    [JsonPropertyName("m2_ground_impact")]
    public string M2GroundImpact { get; set; } = "Medium";

    [JsonPropertyName("m3_emergency_response")]
    public string M3EmergencyResponse { get; set; } = "Medium";

    // Legacy aliases used by SORAOrchestrationService (do not serialize directly)
    [JsonIgnore]
    public string? Scenario { get; set; }

    [JsonIgnore]
    public double DimensionM { get; set; }

    [JsonIgnore]
    public string? ContainmentQuality { get; set; }

    [JsonIgnore]
    public string? M2Impact { get; set; }

    [JsonIgnore]
    public string? M3ERP { get; set; }
}

public class PythonGRCRequest_2_5
{
    [JsonPropertyName("max_dimension_m")]
    public double MaxDimensionM { get; set; }

    [JsonPropertyName("max_speed_ms")]
    public double MaxSpeedMs { get; set; }

    [JsonPropertyName("weight_kg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("population_density")]
    public double PopulationDensity { get; set; }

    [JsonPropertyName("is_controlled_ground")]
    public bool IsControlledGround { get; set; }

    [JsonPropertyName("m1_strategic")]
    public string M1Strategic { get; set; } = "None";

    [JsonPropertyName("m2_ground_impact")]
    public string M2GroundImpact { get; set; } = "Medium";

    [JsonPropertyName("m3_emergency_response")]
    public string M3EmergencyResponse { get; set; } = "Medium";

    // Accept additional Annex F fields for FE/Smoke binding
    [JsonPropertyName("mtom_kg")]
    public double MTOM_kg { get; set; }

    [JsonPropertyName("max_speed_mps")]
    public double MaxSpeed_mps { get; set; }

    [JsonPropertyName("max_characteristic_dimension_m")]
    public double MaxCharacteristicDimensionM { get; set; }

    [JsonPropertyName("operation_mode")]
    public string? OperationMode { get; set; }

    [JsonPropertyName("overflown_area")]
    public string? OverflownArea { get; set; }

    [JsonPropertyName("m1a_sheltering")]
    public string? M1A_Sheltering { get; set; }

    [JsonPropertyName("m1b_operational")]
    public string? M1B_Operational { get; set; }

    [JsonPropertyName("m1c_ground_observation")]
    public string? M1C_GroundObservation { get; set; }

    [JsonPropertyName("m2_impact")]
    public string? M2Impact { get; set; }
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
    [JsonPropertyName("max_height_agl_m")]
    public double MaxHeightAglM { get; set; }

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

    // Legacy/alternate inputs
    [JsonIgnore]
    public double AltitudeAglFt { get; set; }

    [JsonIgnore]
    public double? DistanceToAerodrome_nm { get; set; }

    [JsonIgnore]
    public bool? IsInCTR { get; set; }
}

public class PythonARCRequest_2_5
{
    [JsonPropertyName("max_height_agl_m")]
    public double MaxHeightAglM { get; set; }

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

    // Additional optional fields
    [JsonIgnore]
    public double? DistanceToAerodrome_km { get; set; }

    [JsonIgnore]
    public bool? IsInCTR { get; set; }

    [JsonIgnore]
    public bool? IsNearAerodrome { get; set; }

    // Legacy alias used by Orchestration
    [JsonIgnore]
    public double AltitudeAglM { get; set; }
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

    [JsonPropertyName("final_arc")]
    public string FinalARC { get; set; } = string.Empty;

    // SORA 2.5 numeric residual ARC (preferred for 2.5)
    [JsonPropertyName("residual_arc_level")]
    public int? ResidualARCLevel { get; set; }

    // Alias used by Orchestration
    [JsonIgnore]
    public string? ResidualARC { get; set; }
}

public class PythonSAILResponse
{
    [JsonPropertyName("sail")]
    public string SAIL { get; set; } = string.Empty;

    [JsonPropertyName("final_grc")]
    public int FinalGRC { get; set; }

    [JsonPropertyName("final_arc")]
    public string FinalARC { get; set; } = string.Empty;

    [JsonPropertyName("sora_version")]
    public string SoraVersion { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

#endregion
