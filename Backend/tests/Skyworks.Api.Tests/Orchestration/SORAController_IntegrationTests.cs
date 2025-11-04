using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests.Orchestration;

public class SORAController_IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SORAController_IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_SORA_Complete_V25_LowRisk_Returns_Success_Result()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            soraVersion = "2.5",
            missionId = "IT-LOW-001",
            missionName = "Integration Low Risk VLOS",
            groundRisk = new
            {
                mtom_kg = 0.7,
                populationDensity = 10,
                isControlledGroundArea = false,
                maxCharacteristicDimension = 0.5,
                maxSpeed = 20,
                mitigations = new object[]
                {
                    new { type = "M1A", robustness = "Medium" }
                }
            },
            airRisk = new
            {
                explicitARC = "ARC_a",
                strategicMitigations = Array.Empty<string>()
            },
            implementedOSOs = new object[]
            {
                new { osoId = 1, achievedRobustness = "Medium" },
                new { osoId = 2, achievedRobustness = "Medium" }
            }
        };

    var resp = await client.PostAsJsonAsync("/api/sora/complete", request);
    var body = await resp.Content.ReadAsStringAsync();
    Assert.True(resp.StatusCode == HttpStatusCode.OK, $"Expected 200 OK, got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");

    var json = JsonDocument.Parse(body).RootElement;

        Assert.True(json.GetProperty("isSuccessful").GetBoolean());
        Assert.Equal("2.5", json.GetProperty("soraVersion").GetString());
        Assert.True(json.GetProperty("finalGRC").GetInt32() >= 1);
        Assert.Equal("ARC_a", json.GetProperty("residualARC").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("summary").GetString()));
        Assert.True(json.GetProperty("errors").GetArrayLength() == 0);
    }

    [Fact]
    public async Task Post_SORA_Complete_V25_OutOfScope_Returns_Error_With_iGRC()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            soraVersion = "2.5",
            missionId = "IT-OOS-001",
            missionName = "Integration Out Of Scope",
            groundRisk = new
            {
                mtom_kg = 10.0,
                populationDensity = 8000,
                isControlledGroundArea = false,
                maxCharacteristicDimension = 10.0,
                maxSpeed = 80.0,
                mitigations = Array.Empty<object>()
            },
            airRisk = new
            {
                explicitARC = "ARC_d",
                strategicMitigations = Array.Empty<string>()
            },
            implementedOSOs = Array.Empty<object>()
        };

    var resp = await client.PostAsJsonAsync("/api/sora/complete", request);
    var body = await resp.Content.ReadAsStringAsync();
    Assert.True(resp.StatusCode == HttpStatusCode.BadRequest, $"Expected 400 BadRequest, got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");

    // Validate we get a meaningful error message
    Assert.Contains("scope", body, StringComparison.OrdinalIgnoreCase);
    }
}
