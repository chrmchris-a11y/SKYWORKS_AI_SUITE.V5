using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SORA.Compliance.Services
{
    /// <summary>
    /// Implementation of SAIL service that delegates to Python API endpoints.
    /// Maintains parity with Python implementation without duplicating business logic.
    /// 
    /// References:
    /// - SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1
    /// - SORA 2.5: JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7
    /// </summary>
    public class SAILService : ISAILService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SAILService> _logger;
        private readonly SAILServiceOptions _options;

        public SAILService(HttpClient httpClient, ILogger<SAILService> logger, IOptions<SAILServiceOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Calculate SAIL for SORA 2.0. If GRC > 7, returns Category C.
        /// Reference: EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1
        /// </summary>
        public async Task<SAILResult> CalculateSail20Async(int grc, char arcLetter)
        {
            try
            {
                var request = new
                {
                    grc = grc,
                    arc = arcLetter.ToString().ToLowerInvariant(),
                    sora_version = "2.0"
                };

                var response = await _httpClient.PostAsJsonAsync("/sail/calculate", request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("SAIL calculation failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    return new SAILResult
                    {
                        Success = false,
                        ErrorMessage = $"Calculation failed: {response.StatusCode}",
                        Reference = "EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1"
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SAILCalculationResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return new SAILResult
                {
                    Success = true,
                    Sail = result?.Sail,
                    Category = result?.Category,
                    OsoCount = GetOsoCountForSora20(result?.Sail),
                    Reference = "EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating SAIL 2.0 for GRC={GRC}, ARC={ARC}", grc, arcLetter);
                return new SAILResult
                {
                    Success = false,
                    ErrorMessage = "Internal error during calculation",
                    Reference = "EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1"
                };
            }
        }

        /// <summary>
        /// Calculate SAIL for SORA 2.5. GRC 9-10 always returns SAIL VI.
        /// Reference: JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7
        /// </summary>
        public async Task<SAILResult> CalculateSail25Async(int grc, int residualArcLevel)
        {
            if (residualArcLevel < 1 || residualArcLevel > 10)
            {
                return new SAILResult
                {
                    Success = false,
                    ErrorMessage = "SORA 2.5 requires residual ARC level in [1..10]",
                    Reference = "JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7"
                };
            }

            try
            {
                var request = new
                {
                    grc = grc,
                    residual_arc_level = residualArcLevel,
                    sora_version = "2.5"
                };

                var response = await _httpClient.PostAsJsonAsync("/sail/calculate", request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("SAIL 2.5 calculation failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    return new SAILResult
                    {
                        Success = false,
                        ErrorMessage = $"Calculation failed: {response.StatusCode}",
                        Reference = "JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7"
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SAILCalculationResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return new SAILResult
                {
                    Success = true,
                    Sail = result?.Sail,
                    Category = result?.Category,
                    OsoCount = null, // Never return hard-coded count for SORA 2.5
                    Reference = "JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating SAIL 2.5 for GRC={GRC}, ResidualARC={ResidualARC}", grc, residualArcLevel);
                return new SAILResult
                {
                    Success = false,
                    ErrorMessage = "Internal error during calculation",
                    Reference = "JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7"
                };
            }
        }

        /// <summary>
        /// Get OSO requirements for a SAIL level and SORA version.
        /// </summary>
        public async Task<OSOResponse> GetOSORequirementsAsync(string sail, string soraVersion)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/sail/oso-requirements?sail={sail}&sora_version={soraVersion}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("OSO requirements request failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    return new OSOResponse
                    {
                        Success = false,
                        ErrorMessage = $"Request failed: {response.StatusCode}",
                        Reference = GetReferenceForVersion(soraVersion)
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OSORequirementsResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return new OSOResponse
                {
                    Success = true,
                    OsoCount = result?.OsoCount,
                    Requirements = result?.Requirements ?? new List<OSORequirement>(),
                    Reference = GetReferenceForVersion(soraVersion)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OSO requirements for SAIL={SAIL}, Version={Version}", sail, soraVersion);
                return new OSOResponse
                {
                    Success = false,
                    ErrorMessage = "Internal error retrieving OSO requirements",
                    Reference = GetReferenceForVersion(soraVersion)
                };
            }
        }

        /// <summary>
        /// Get the complete SAIL mapping table for UI display.
        /// </summary>
        public async Task<MappingTable> GetMappingTableAsync(string soraVersion)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/sail/mapping-table?sora_version={soraVersion}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Mapping table request failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    return new MappingTable
                    {
                        Success = false,
                        ErrorMessage = $"Request failed: {response.StatusCode}",
                        Reference = GetReferenceForVersion(soraVersion)
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MappingTable>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (result != null)
                {
                    result.Reference = GetReferenceForVersion(soraVersion);
                }

                return result ?? new MappingTable
                {
                    Success = false,
                    ErrorMessage = "Failed to parse mapping table response",
                    Reference = GetReferenceForVersion(soraVersion)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mapping table for Version={Version}", soraVersion);
                return new MappingTable
                {
                    Success = false,
                    ErrorMessage = "Internal error retrieving mapping table",
                    Reference = GetReferenceForVersion(soraVersion)
                };
            }
        }

        private static int? GetOsoCountForSora20(string? sail)
        {
            return sail switch
            {
                "I" => 6,
                "II" => 10,
                "III" => 15,
                "IV" => 18,
                "V" => 21,
                "VI" => 24,
                _ => null
            };
        }

        private static string GetReferenceForVersion(string soraVersion)
        {
            return soraVersion switch
            {
                "2.0" => "EASA AMC/GM to Reg. (EU) 2019/947, Annex E",
                "2.5" => "JARUS SORA v2.5 Main Body & Annexes, Annex E",
                _ => "Unknown SORA version"
            };
        }
    }

    public class SAILServiceOptions
    {
        public string PythonApiBaseUrl { get; set; } = string.Empty;
    }
}

