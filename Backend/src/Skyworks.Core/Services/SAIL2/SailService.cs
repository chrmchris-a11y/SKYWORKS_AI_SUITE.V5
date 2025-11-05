using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skyworks.Core.Services.SAIL2;

/// <summary>
/// Version-aware SAIL service that avoids duplicating logic in C#.
/// 
/// Behavior:
/// - Prefer calling the Python API (/sail/calculate, /sail/oso-requirements, /sail/mapping-table).
/// - If offline is mandatory in the future, load the same JSON tables emitted by Python at startup.
/// - Respect SORA 2.0 Category C when GRC > 7; 2.5: numeric residual ARC 1..10 and rule GRC 9–10 ⇒ VI.
/// 
/// References: EASA AMC/GM Annex D Table D.1 (SORA 2.0); JARUS Annex D Table 7 (SORA 2.5)
/// </summary>
public interface INewSAILService
{
    Task<NewSAILResult> CalculateSail20Async(int grc, char arcLetter);
    Task<NewSAILResult> CalculateSail25Async(int grc, int residualArcLevel);
    Task<OsoResponse> GetOsoRequirementsAsync(string sail, string soraVersion);
    Task<MappingTable> GetMappingTableAsync(string soraVersion);
}

public sealed class SailService : INewSAILService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public SailService(HttpClient? httpClient = null, string? pythonBaseUrl = null)
    {
        _http = httpClient ?? new HttpClient();
        _baseUrl = pythonBaseUrl ?? System.Environment.GetEnvironmentVariable("PYTHON_API_BASE") ?? "http://localhost:8001";
        if (_baseUrl.EndsWith("/")) _baseUrl = _baseUrl.TrimEnd('/');
    }

    public async Task<NewSAILResult> CalculateSail20Async(int grc, char arcLetter)
    {
        // Short-circuit Category C per SORA 2.0
        if (grc > 7)
        {
            return new NewSAILResult
            {
                Category = "C",
                Sail = null,
                SoraVersion = "2.0",
                Reference = "EASA AMC/GM Annex D Table D.1"
            };
        }

        var payload = new
        {
            grc_level = grc,
            arc_level = arcLetter.ToString().ToLower(),
            sora_version = "SORA_2.0"
        };
        var res = await _http.PostAsJsonAsync($"{_baseUrl}/sail/calculate", payload);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<PythonSailResponse>();
        return new NewSAILResult
        {
            Category = doc?.category,
            Sail = doc?.sail_level,
            SoraVersion = "2.0",
            Reference = doc?.reference
        };
    }

    public async Task<NewSAILResult> CalculateSail25Async(int grc, int residualArcLevel)
    {
        var payload = new
        {
            grc_level = grc,
            residual_arc_level = residualArcLevel,
            sora_version = "SORA_2.5"
        };
        var res = await _http.PostAsJsonAsync($"{_baseUrl}/sail/calculate", payload);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<PythonSailResponse>();
        return new NewSAILResult
        {
            Category = doc?.category,
            Sail = doc?.sail_level,
            SoraVersion = "2.5",
            Reference = doc?.reference
        };
    }

    public async Task<OsoResponse> GetOsoRequirementsAsync(string sail, string soraVersion)
    {
        // Defensive: normalize version tokens to 2.0/2.5
        var v = soraVersion.Trim().Replace(" ", "").ToUpper();
        if (v == "SORA_2.0" || v == "SORA2.0") v = "2.0";
        if (v == "SORA_2.5" || v == "SORA2.5") v = "2.5";

        // Expected Python route: /sail/oso-requirements?sail=IV&version=2.0
        var url = $"{_baseUrl}/sail/oso-requirements?sail={Uri.EscapeDataString(sail)}&version={Uri.EscapeDataString(v)}";
        var res = await _http.GetAsync(url);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<OsoResponse>();
        return doc ?? new OsoResponse();
    }

    public async Task<MappingTable> GetMappingTableAsync(string soraVersion)
    {
        var v = soraVersion.Trim().Replace(" ", "").ToUpper();
        if (v == "SORA_2.0" || v == "SORA2.0") v = "2.0";
        if (v == "SORA_2.5" || v == "SORA2.5") v = "2.5";
        var url = $"{_baseUrl}/sail/mapping-table?version={Uri.EscapeDataString(v)}";
        var res = await _http.GetAsync(url);
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<MappingTable>();
        return doc ?? new MappingTable();
    }

    private sealed class PythonSailResponse
    {
        [JsonPropertyName("sail_level")] public string? sail_level { get; set; }
        [JsonPropertyName("category")] public string? category { get; set; }
        [JsonPropertyName("reference")] public string? reference { get; set; }
    }
}

public sealed class NewSAILResult
{
    public string? Category { get; set; }
    public string? Sail { get; set; }
    public string? SoraVersion { get; set; }
    public string? Reference { get; set; }
}

public sealed class OsoResponse
{
    public string? SoraVersion { get; set; }
    public string? Sail { get; set; }
    public int? OsoCount { get; set; }
    public List<OsoItem> Requirements { get; set; } = new();
}

public sealed class OsoItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Robustness { get; set; }
    public string? ApplicabilityNote { get; set; }
}

public sealed class MappingTable
{
    public string? Version { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Table { get; set; }
}
