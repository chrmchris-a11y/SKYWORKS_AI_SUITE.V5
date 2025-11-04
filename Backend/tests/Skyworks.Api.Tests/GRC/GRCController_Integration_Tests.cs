using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Skyworks.Core.Models.GRC;
using Xunit;

public class GRCController_Integration_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GRCController_Integration_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { /* default appsettings ok */ });
    }

    [Fact]
    public async Task Versions_Endpoint_Contains_AtLeast_SORA_20()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/grc/versions");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        // Accept 2.0 always; 2.5 may be feature-flagged
        Assert.Contains("\"version\":\"2.0\"", json.Replace(" ", string.Empty));
    }

    [Fact]
    public async Task Intrinsic_V25_MicroUAS_Returns_GRC1()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            maxCharacteristicDimension = 0.2,
            maxSpeed = 20.0,
            mtom_kg = 0.2,
            populationDensity = 10000.0,
            isControlledGroundArea = false
        };

        var resp = await client.PostAsJsonAsync("/api/grc/v2.5/intrinsic", payload);
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<IntrinsicResultDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.IGRC);
        Assert.False(result.IsOutOfScope);
    }

    [Fact]
    public async Task Final_V25_With_Mitigations_Computes_Final()
    {
        var client = _factory.CreateClient();

        var payload = new
        {
            intrinsicGRC = 7,
            mitigations = new object[]
            {
                new { type = "M1A_Sheltering", robustness = "Low" },    // −1
                new { type = "M1B_OperationalRestrictions", robustness = "Medium" }, // −1
                new { type = "M1C_GroundObservation", robustness = "Low" }, // −1
                new { type = "M2_ImpactDynamics", robustness = "Medium" } // −1
            }
        };

        var resp = await client.PostAsJsonAsync("/api/grc/v2.5/final", payload);
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<FinalResultDto>();
        Assert.NotNull(result);
        Assert.True(result!.FinalGRC <= 7);
        Assert.True(result.FinalGRC >= 1);
        Assert.InRange(result.TotalGRCReduction, 1, 6);
        Assert.True(result.IsValid);
    }

    private record IntrinsicResultDto(int IGRC, bool IsOutOfScope);
    private record FinalResultDto(int FinalGRC, int TotalGRCReduction, bool IsValid);
}
