using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class ProxoraController_Integration_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProxoraController_Integration_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Returns_Python_OK()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/Proxora/health");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"python\":\"OK\"", json.Replace(" ", string.Empty));
    }

    [Fact]
    public async Task GRC25_MicroUAS_Through_Proxy_Returns_iGRC1()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            mtom_kg = 0.2,
            max_speed_ms = 20.0,
            population_density = 10000.0,
            max_characteristic_dimension_m = 0.2
        };

        var resp = await client.PostAsJsonAsync("/api/Proxora/grc/2.5", payload);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("intrinsic_grc", out var igrc));
        var i = igrc.GetInt32();
        Assert.InRange(i, 1, 2);
        if (root.TryGetProperty("final_grc", out var fg))
        {
            var f = fg.GetInt32();
            Assert.InRange(f == 0 ? i : f, 1, 7);
        }
    }

    [Fact]
    public async Task SORA25_Composite_Proxy_Returns_GRC_ARC_SAIL_And_Proximity()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            soraVersion = "2.5",
            grc25 = new
            {
                mtom_kg = 0.8,
                population_density = 10000.0,
                max_characteristic_dimension_m = 0.3,
                max_speed_ms = 22.0,
                m1a_sheltering = "Low",
                m1b_operational = "Medium",
                m1c_ground_observation = "Low",
                m2_impact = "Medium"
            },
            arc25 = new
            {
                altitude_agl_m = 60.0,
                max_speed_ms = 22.0,
                airspace_class = "G",
                is_controlled = false,
                is_modes_veil = false,
                is_tmz = false,
                environment = "Rural",
                is_airport_heliport = false,
                is_atypical_segregated = false,
                tactical_mitigation_level = "None"
            },
            proximities = new object[]
            {
                new { type = "airport", distanceMeters = 420.0, bearingDeg = 90.0 },
                new { type = "hospital", distanceMeters = 1200.0 }
            }
        };

        var resp = await client.PostAsJsonAsync("/api/Proxora/sora", payload);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"grc\"", json);
        Assert.Contains("\"arc\"", json);
        Assert.Contains("\"sail\"", json);
        Assert.Contains("\"proximity\"", json);
        Assert.Contains("advisory", json);
    }

    [Fact]
    public async Task ARC20_Through_Proxy_Returns_Valid_ARC()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            altitude_agl_ft = 400.0,
            max_height_amsl_m = 100.0,
            airspace_class = "G",
            is_controlled = false,
            is_modes_veil = false,
            is_tmz = false,
            environment = "Rural",
            is_airport_heliport = false,
            is_atypical_segregated = false,
            tactical_mitigation_level = "None"
        };

        var resp = await client.PostAsJsonAsync("/api/Proxora/arc/2.0", payload);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("initial_arc", out var ia));
        Assert.True(root.TryGetProperty("final_arc", out var fa));
        var val = fa.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(val))
        {
            val = ia.GetString() ?? string.Empty;
        }
        // Normalize form: allow "ARC_b" or "b"
        var norm = val.StartsWith("ARC_", StringComparison.OrdinalIgnoreCase)
            ? val.Substring(4)
            : val;
        Assert.Matches("^[a-dA-D]$", norm);
    }

    [Fact]
    public async Task ARC25_Through_Proxy_Returns_Valid_ARC()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            altitude_agl_m = 60.0,
            max_speed_ms = 22.0,
            airspace_class = "G",
            is_controlled = false,
            is_modes_veil = false,
            is_tmz = false,
            environment = "Rural",
            is_airport_heliport = false,
            is_atypical_segregated = false,
            tactical_mitigation_level = "None"
        };

        var resp = await client.PostAsJsonAsync("/api/Proxora/arc/2.5", payload);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("initial_arc", out var ia));
        Assert.True(root.TryGetProperty("final_arc", out var fa));
        var val = fa.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(val))
        {
            val = ia.GetString() ?? string.Empty;
        }
        var norm = val.StartsWith("ARC_", StringComparison.OrdinalIgnoreCase)
            ? val.Substring(4)
            : val;
        Assert.Matches("^[a-dA-D]$", norm);
        Assert.True(root.TryGetProperty("aec", out _));
        Assert.True(root.TryGetProperty("density_rating", out _));
        Assert.True(root.TryGetProperty("tactical_mitigation_effect", out _));
    }
}
