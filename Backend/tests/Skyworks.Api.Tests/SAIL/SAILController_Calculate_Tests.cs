using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests.SAIL;

public class SAILController_Calculate_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SAILController_Calculate_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_SAIL_Calculate_V20_GRC5_ARC_c_Returns_SAIL_IV()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            soraVersion = "2.0",
            groundRisk = new
            {
                scenario_V2_0 = "VLOS_Populated",
                maxCharacteristicDimension = 3.0,
                mitigations = new object[]
                {
                    new { type = "M3", robustness = "Medium" } // Avoid ERP penalty
                }
            },
            airRisk = new
            {
                explicitARC = "ARC_c",
                strategicMitigations = Array.Empty<string>()
            },
            implementedOSOs = Array.Empty<object>()
        };

        var resp = await client.PostAsJsonAsync("/api/sail/calculate", request);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.True(resp.StatusCode == HttpStatusCode.OK, $"Expected 200 OK, got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("2.0", json.GetProperty("soraVersion").GetString());
        Assert.Equal(5, json.GetProperty("iGRC").GetInt32());
        Assert.Equal(5, json.GetProperty("finalGRC").GetInt32());
        Assert.Equal("c", json.GetProperty("initialARC").GetString());
        Assert.Equal("c", json.GetProperty("residualARC").GetString());
        Assert.Equal("IV", json.GetProperty("sail").GetString());
    }

    [Fact]
    public async Task Post_SAIL_Calculate_V25_Sub250g_Returns_iGRC1_SAIL_I()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            soraVersion = "2.5",
            groundRisk = new
            {
                mtom_kg = 0.24,
                populationDensity = 10.0,
                isControlledGroundArea = false,
                maxCharacteristicDimension = 0.20,
                maxSpeed = 20.0,
                mitigations = Array.Empty<object>()
            },
            airRisk = new
            {
                explicitARC = "ARC_a",
                strategicMitigations = Array.Empty<string>()
            },
            implementedOSOs = Array.Empty<object>()
        };

        var resp = await client.PostAsJsonAsync("/api/sail/calculate", request);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.True(resp.StatusCode == HttpStatusCode.OK, $"Expected 200 OK, got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("2.5", json.GetProperty("soraVersion").GetString());
        Assert.Equal(1, json.GetProperty("iGRC").GetInt32());
        Assert.Equal(1, json.GetProperty("finalGRC").GetInt32());
        Assert.Equal("a", json.GetProperty("initialARC").GetString());
        Assert.Equal("a", json.GetProperty("residualARC").GetString());
        Assert.Equal("I", json.GetProperty("sail").GetString());
    }
}
