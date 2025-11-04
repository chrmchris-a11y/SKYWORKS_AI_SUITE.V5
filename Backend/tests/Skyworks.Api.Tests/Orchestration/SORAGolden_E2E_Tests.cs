using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests.Orchestration;

public class SORAGolden_E2E_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SORAGolden_E2E_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Golden_Scenarios_Pass_And_Agree_With_Expectations(object request, int expectedStatusCode, Action<JsonElement> assertBody)
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/sora/complete", request);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Equal(expectedStatusCode, (int)resp.StatusCode);

        var json = JsonDocument.Parse(body).RootElement;
        assertBody(json);
    }

    public static IEnumerable<object[]> Cases()
    {
        yield return new object[]
        {
            // SORA 2.5 Low risk, explicit ARC-a (was "A")
            new
            {
                soraVersion = "2.5",
                missionId = "GOLD-25-LOW-001",
                missionName = "Low Risk VLOS",
                groundRisk = new
                {
                    mtom_kg = 0.8,
                    populationDensity = 20,
                    isControlledGroundArea = false,
                    maxCharacteristicDimension = 0.6,
                    maxSpeed = 18,
                    mitigations = new object[] { new { type = "M1A", robustness = "Medium" } }
                },
                airRisk = new { explicitARC = "ARC_a" },
                implementedOSOs = new object[] { new { osoId = 1, achievedRobustness = "Medium" } }
            },
            (int)HttpStatusCode.OK,
            (Action<JsonElement>)(json =>
            {
                Assert.True(json.GetProperty("isSuccessful").GetBoolean());
                Assert.Equal("2.5", json.GetProperty("soraVersion").GetString());
                Assert.Equal("ARC_a", json.GetProperty("residualARC").GetString());
                Assert.Contains("SORA 2.5", json.GetProperty("summary").GetString());
            })
        };

        yield return new object[]
        {
            // SORA 2.0 Medium case: explicit ARC-c with two strategic mitigations → Residual ARC-b (floor per Annex C)
            new
            {
                soraVersion = "2.0",
                missionId = "GOLD-20-MED-001",
                missionName = "BVLOS Sparse",
                groundRisk = new
                {
                    scenario_V2_0 = "BVLOS_SparselyPopulated",
                    maxCharacteristicDimension = 2.5,
                    mtom_kg = 1.5,  // Add MTOM for GRC calculation (2.5m dimension ≈ 1.5kg drone)
                    mitigations = new object[]
                    {
                        new { type = "M1", robustness = "Medium" },
                        new { type = "M2", robustness = "Medium" }
                    }
                },
                airRisk = new { explicitARC = "ARC_c", strategicMitigations = new []{"S1","S2"} },
                implementedOSOs = new object[]
                {
                    new { osoId = 1, achievedRobustness = "Medium" },
                    new { osoId = 2, achievedRobustness = "Medium" },
                    new { osoId = 3, achievedRobustness = "Medium" },
                    new { osoId = 4, achievedRobustness = "High" }
                }
            },
            (int)HttpStatusCode.OK,
            (Action<JsonElement>)(json =>
            {
                Assert.True(json.GetProperty("isSuccessful").GetBoolean());
                Assert.Equal("2.0", json.GetProperty("soraVersion").GetString());
                Assert.Equal("ARC_c", json.GetProperty("initialARC").GetString());
                Assert.Equal("ARC_b", json.GetProperty("residualARC").GetString());
                Assert.Contains("SORA 2.0", json.GetProperty("summary").GetString());
            })
        };

        yield return new object[]
        {
            // SORA 2.5 Out-of-scope: diagnostics with iGRC, HTTP 400 (ARC-d was "D")
            new
            {
                soraVersion = "2.5",
                missionId = "GOLD-25-OOS-001",
                missionName = "High Risk Urban BVLOS",
                groundRisk = new
                {
                    mtom_kg = 12.0,
                    populationDensity = 9000,
                    isControlledGroundArea = false,
                    maxCharacteristicDimension = 9.0,
                    maxSpeed = 90.0,
                    mitigations = Array.Empty<object>()
                },
                airRisk = new { explicitARC = "ARC_d" },
                implementedOSOs = Array.Empty<object>()
            },
            (int)HttpStatusCode.BadRequest,
            (Action<JsonElement>)(json =>
            {
                Assert.False(json.GetProperty("isSuccessful").GetBoolean());
                // Ensure intrinsicGRC is present and within valid SORA range
                var igrc = json.GetProperty("intrinsicGRC").GetInt32();
                Assert.InRange(igrc, 1, 10);
                Assert.True(json.GetProperty("errors").GetArrayLength() > 0);
            })
        };

        yield return new object[]
        {
            // SORA 2.5 env-based ARC: should attach validation warnings when applicable
            new
            {
                soraVersion = "2.5",
                missionId = "GOLD-25-ENV-001",
                missionName = "Controlled Airspace High AGL",
                groundRisk = new
                {
                    mtom_kg = 1.2,
                    populationDensity = 50,
                    isControlledGroundArea = false,
                    maxCharacteristicDimension = 1.0,
                    maxSpeed = 25.0,
                    mitigations = Array.Empty<object>()
                },
                airRisk = new
                {
                    airspaceControl = "Controlled",
                    locationType = "NonAirport",
                    environment = "Urban",
                    typicality = "Typical",
                    maxHeightAGL = 150
                },
                implementedOSOs = Array.Empty<object>()
            },
            (int)HttpStatusCode.OK,
            (Action<JsonElement>)(json =>
            {
                Assert.True(json.GetProperty("isSuccessful").GetBoolean());
                var warnings = json.GetProperty("warnings");
                Assert.True(warnings.GetArrayLength() >= 1);
                var summary = json.GetProperty("summary").GetString();
                Assert.Contains("Warnings:", summary);
            })
        };
    }
}
