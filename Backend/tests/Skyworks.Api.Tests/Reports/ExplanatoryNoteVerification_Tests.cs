using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests.Reports;

public class ExplanatoryNoteVerification_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExplanatoryNoteVerification_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Verify_ExplanatoryNote_Checklist_Returns_Items()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            soraVersion = "2.5",
            missionId = "EN-TEST-001",
            missionName = "Checklist Demo",
            missionDescription = "BVLOS linear inspection with geofencing and NOTAM.",
            groundRisk = new
            {
                mtom_kg = 1.2,
                populationDensity = 500,
                isControlledGroundArea = true,
                maxCharacteristicDimension = 1.2,
                maxSpeed = 15,
                mitigations = new object[]
                {
                    new { type = "M1A", robustness = "Medium" },
                    new { type = "M2", robustness = "Low" }
                }
            },
            airRisk = new
            {
                airspaceControl = "Uncontrolled",
                locationType = "NonAirport",
                environment = "Suburban",
                typicality = "Typical",
                maxHeightAGL = 100,
                strategicMitigations = new []{"S1","S2"},
                localDensityRating = 2
            },
            implementedOSOs = new object[]
            {
                new { osoId = 1, achievedRobustness = "Medium" },
                new { osoId = 2, achievedRobustness = "Medium" }
            }
        };

        var resp = await client.PostAsJsonAsync("/api/sora/explanatory-note/verify", request);
        var body = await resp.Content.ReadAsStringAsync();

        Assert.True(resp.StatusCode == HttpStatusCode.OK, $"Expected 200 OK, got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("2.5", json.GetProperty("soraVersion").GetString());
        Assert.True(json.GetProperty("items").GetArrayLength() >= 5);
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("summary").GetString()));
    }
}
