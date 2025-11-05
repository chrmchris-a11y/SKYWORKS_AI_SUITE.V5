using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;

namespace Skyworks.Api.Services
{
    public class PythonCalculationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PythonCalculationClient> _logger;
        private readonly PythonServiceOptions _options;

        public PythonCalculationClient(HttpClient httpClient, IOptions<PythonServiceOptions> options, ILogger<PythonCalculationClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<PythonSAILResponse> CalculateSAIL20Async(int finalGrc, string residualArc)
        {
            var normalizedArc = NormalizeArcForPython(residualArc);
            
            var request = new Python20SAILRequest
            {
                SoraVersion = "2.0",
                FinalGrc = finalGrc,
                ResidualArc = $"ARC-{normalizedArc}"
            };

            return await ExecuteWithRetry(request, "2.0");
        }

        public async Task<PythonSAILResponse> CalculateSAIL25Async(int finalGrc, int residualArcLevel)
        {
            var request = new Python25SAILRequest
            {
                SoraVersion = "2.5",
                FinalGrc = finalGrc,
                ResidualArcLevel = residualArcLevel
            };

            return await ExecuteWithRetry(request, "2.5");
        }

        private async Task<PythonSAILResponse> ExecuteWithRetry<T>(T request, string version)
        {
            var maxRetries = 3;
            var baseDelay = TimeSpan.FromSeconds(1);
            var timeout = TimeSpan.FromSeconds(10);

            using var cts = new CancellationTokenSource(timeout);
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                    });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await _httpClient.PostAsync("/api/v1/calculate/sail", content, cts.Token);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.BadGateway || 
                        response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        if (attempt < maxRetries)
                        {
                            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt) + Random.Shared.Next(100, 500));
                            await Task.Delay(delay, cts.Token);
                            continue;
                        }
                        throw new HttpRequestException($"Python service returned {response.StatusCode} after {maxRetries} retries");
                    }

                    response.EnsureSuccessStatusCode();
                    var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
                    var pythonResponse = JsonSerializer.Deserialize<PythonSAILResponseRaw>(responseJson, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                    });

                    return MapPythonResponse(pythonResponse, version);
                }
                catch (TaskCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    var correlationId = Guid.NewGuid().ToString();
                    _logger.LogError("Python service timeout after {TimeoutSeconds}s, correlation: {CorrelationId}", 
                        timeout.TotalSeconds, correlationId);
                    throw new TimeoutException($"Python service timeout, correlation: {correlationId}");
                }
                catch (HttpRequestException) when (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt) + Random.Shared.Next(100, 500));
                    await Task.Delay(delay, cts.Token);
                }
            }

            throw new HttpRequestException($"Failed to reach Python service after {maxRetries} retries");
        }

        private PythonSAILResponse MapPythonResponse(PythonSAILResponseRaw raw, string version)
        {
            // If body contains category:"C" ⇒ return CategoryC status with Sail=null, OsoCount=null
            // SAIL feeds Annex-E; Category-C short-circuits SAIL
            if (!string.IsNullOrEmpty(raw.Category) && raw.Category.Equals("C", StringComparison.OrdinalIgnoreCase))
            {
                return new PythonSAILResponse
                {
                    Sail = null,
                    OsoCount = null,
                    Category = "C"
                };
            }

            // If soraVersion=="2.5" ⇒ discard any oso_count in Python echo; keep null in .NET
            int? osoCount = version == "2.5" ? null : raw.OsoCount;

            return new PythonSAILResponse
            {
                Sail = raw.Sail,
                OsoCount = osoCount,
                Category = raw.Category
            };
        }

        private string NormalizeArcForPython(string arcInput)
        {
            return arcInput.ToLowerInvariant()
                .Replace("arc-", "")
                .Replace("arc_", "")
                .Replace("arc", "")
                .Trim();
        }
    }

    // Separate typed DTOs for 2.0 and 2.5 requests to prevent accidental letter/numeric mixing
    public class Python20SAILRequest
    {
        public string SoraVersion { get; set; } = "";
        public int FinalGrc { get; set; }
        public string ResidualArc { get; set; } = "";
    }

    public class Python25SAILRequest
    {
        public string SoraVersion { get; set; } = "";
        public int FinalGrc { get; set; }
        public int ResidualArcLevel { get; set; }
    }

    // Raw response from Python service
    public class PythonSAILResponseRaw
    {
        public string? Sail { get; set; }
        public int? OsoCount { get; set; }
        public string? Category { get; set; }
    }

    // Single PythonSAILResponse with nullable Sail, OsoCount, Category
    public class PythonSAILResponse
    {
        public string? Sail { get; set; }
        public int? OsoCount { get; set; }
        public string? Category { get; set; }
    }

    public class PythonServiceOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:8001";
    }
}

