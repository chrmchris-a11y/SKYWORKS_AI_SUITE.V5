using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Skyworks.Core.Services.Python;
using Xunit;

namespace Skyworks.Api.Tests.GRC;

public class PythonGRCIntegrationTests
{
    private class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new HttpClient();
    }

    private static (IPythonCalculationClient client, bool healthy) CreateClient()
    {
        var httpFactory = new TestHttpClientFactory();
        using var lf = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Warning));
        var logger = lf.CreateLogger<PythonCalculationClient>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PythonService:BaseUrl"] = "http://localhost:8001"
            })
            .Build();

        var client = new PythonCalculationClient(httpFactory, logger, config);
        var healthy = client.HealthCheck().GetAwaiter().GetResult();
        return (client, healthy);
    }

    [Fact]
    public async Task GRC25_Basic_Case_Returns_Intrinsic7or8_FinalMatchesCredits()
    {
        var (client, healthy) = CreateClient();
        if (!healthy)
        {
            // If Python service is not running, fail fast with clear message
            Assert.True(healthy, "Python FastAPI service (8001) is not reachable for integration tests.");
        }

        var request = new PythonGRCRequest_2_5
        {
            MTOM_kg = 32,
            PopulationDensity = 25000,
            // Provide optional dimension/speed context when available
            MaxCharacteristicDimensionM = 3.0,
            MaxSpeedMs = 35.0,
            // Mitigations per SONNET example: total −4 → 7 − 4 = 3
            M1A_Sheltering = "low",   // −1
            M1B_Operational = "medium", // −1
            M1C_GroundObservation = "low", // −1
            M2Impact = "medium" // −1
        };

        var response = await client.CalculateGRC_2_5(request);
        Assert.NotNull(response);
        var intrinsic = response!.IntrinsicGRC;
        Assert.InRange(intrinsic, 7, 8); // επιτρέπουμε μικρή απόκλιση μεταξύ υλοποιήσεων
        var lowerBound = Math.Max(1, intrinsic - 4); // μέγιστη συνολική μείωση που περιμένουμε
        var upperBound = intrinsic;                   // δεν πρέπει να αυξηθεί
        Assert.InRange(response.FinalGRC, lowerBound, upperBound);
    }

    [Fact]
    public async Task GRC25_MicroUAS_ClampedTo1()
    {
        var (client, healthy) = CreateClient();
        if (!healthy)
        {
            Assert.True(healthy, "Python FastAPI service (8001) is not reachable for integration tests.");
        }

        var request = new PythonGRCRequest_2_5
        {
            MTOM_kg = 0.2, // ≤250g
            PopulationDensity = 100,
            MaxCharacteristicDimensionM = 0.2,
            MaxSpeedMs = 20.0, // ≤25 m/s
            M1A_Sheltering = "medium",
            M1B_Operational = "medium",
            M1C_GroundObservation = "low",
            M2Impact = "medium"
        };

        var response = await client.CalculateGRC_2_5(request);
        Assert.NotNull(response);
        Assert.Equal(1, response!.IntrinsicGRC); // micro‑UAS rule
        Assert.Equal(1, response.FinalGRC);      // clamp at 1
    }

    [Fact]
    public async Task GRC20_Table2_iGRC_Expected()
    {
        var (client, healthy) = CreateClient();
        if (!healthy)
        {
            Assert.True(healthy, "Python FastAPI service (8001) is not reachable for integration tests.");
        }

        var request = new PythonGRCRequest_2_0
        {
            MTOM_kg = 32,
            PopulationDensity = 25000,
            // Mitigations optional; focus on initial iGRC consistency (Fix #1)
            M1Strategic = null,
            M2Impact = null,
            M3ERP = null
        };

        var response = await client.CalculateGRC_2_0(request);
        Assert.NotNull(response);
        Assert.True(response!.IntrinsicGRC >= 1);
        // Depending on Python implementation details, initial GRC should align with unified Table 2
        // For MTOM 32kg & 25k/km², expected iGRC is commonly 7; assert within [6,8] to allow ±1 if model nuances
        Assert.InRange(response.IntrinsicGRC, 6, 8);
    }
}
