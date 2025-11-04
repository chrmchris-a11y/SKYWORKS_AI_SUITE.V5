using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Skyworks.Api.Tests.Orchestration;

/// <summary>
/// Authoritative End-to-End Tests Based on JARUS SORA 2.0 and 2.5 Official Documentation
/// Tests verify complete SORA workflow against 20 definitive test cases from official JARUS specs
/// </summary>
public class SORAAuthoritative_E2E_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public SORAAuthoritative_E2E_Tests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [MemberData(nameof(LoadTestCases))]
    public async Task AuthoritativeTests_VerifyAgainstJARUSSpecification(AuthoritativeTestCase testCase)
    {
        // Arrange
        _output.WriteLine($"═══════════════════════════════════════════════════════");
        _output.WriteLine($"TEST: {testCase.TestId} - {testCase.Name}");
        _output.WriteLine($"Version: SORA {testCase.Version}");
        _output.WriteLine($"Description: {testCase.Description}");
        _output.WriteLine($"Rationale: {testCase.Rationale}");
        if (testCase.ExpectedToFail)
            _output.WriteLine($"⚠️  EXPECTED TO FAIL: Out of scope test");
        _output.WriteLine($"═══════════════════════════════════════════════════════");

        var client = _factory.CreateClient();
        var request = BuildSORARequest(testCase);

        // Act
        var response = await client.PostAsJsonAsync("/api/sora/complete", request);
        var body = await response.Content.ReadAsStringAsync();
        
        // Handle expected failures (out of scope tests)
        if (testCase.ExpectedToFail)
        {
            // Expected to fail tests should get BadRequest or 200 with isSuccessful=false
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                _output.WriteLine($"\n✅ Test PASSED (Expected Failure - BadRequest): {testCase.Name}");
                _output.WriteLine($"Response: {body}");
                _output.WriteLine($"═══════════════════════════════════════════════════════\n");
                return; // Test passed - operation correctly rejected
            }
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = JsonDocument.Parse(body).RootElement;
                var isSuccessful = json.GetProperty("isSuccessful").GetBoolean();
                Assert.False(isSuccessful, "Expected operation to be out of scope");
                _output.WriteLine($"\n✅ Test PASSED (Expected Failure - isSuccessful=false): {testCase.Name}");
                _output.WriteLine($"   Operation correctly rejected as out of scope");
                _output.WriteLine($"═══════════════════════════════════════════════════════\n");
                return; // Test passed
            }
            
            Assert.Fail($"Expected failure test got unexpected status: {response.StatusCode}");
        }
        
        // For non-expectedToFail tests, assert OK status
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _output.WriteLine($"\n❌ HTTP {response.StatusCode}");
            _output.WriteLine($"Response body: {body}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Fail here with message visible
        }
        
        var responseJson = JsonDocument.Parse(body).RootElement;

        _output.WriteLine($"\n✅ HTTP 200 OK - Response: {body.Substring(0, Math.Min(300, body.Length))}...");
        
        Assert.True(responseJson.GetProperty("isSuccessful").GetBoolean(), "SORA execution should be successful");

        _output.WriteLine($"\n[EXPECTED vs ACTUAL]");
        
        // API returns integers for GRC, need to convert to "GRC_X" format
        var actualInitialGRC = responseJson.TryGetProperty("intrinsicGRC", out var iGRC) && !iGRC.ValueKind.Equals(JsonValueKind.Null)
            ? $"GRC_{iGRC.GetInt32()}" : "null";
        var actualFinalGRC = responseJson.TryGetProperty("finalGRC", out var fGRC) && !fGRC.ValueKind.Equals(JsonValueKind.Null)
            ? $"GRC_{fGRC.GetInt32()}" : "null";
        var actualInitialARC = responseJson.TryGetProperty("initialARC", out var iARC) && iARC.ValueKind == JsonValueKind.String
            ? iARC.GetString() : "null";
        var actualResidualARC = responseJson.TryGetProperty("residualARC", out var rARC) && rARC.ValueKind == JsonValueKind.String
            ? rARC.GetString() : "null";
        var actualSAIL = responseJson.TryGetProperty("sail", out var sail) && sail.ValueKind == JsonValueKind.String
            ? $"SAIL_{sail.GetString()}" : "null"; // API returns "II", "III", etc. - add "SAIL_" prefix
        var actualTMPR = responseJson.TryGetProperty("tmpr", out var tmpr) && tmpr.ValueKind == JsonValueKind.String
            ? tmpr.GetString() : "null";

        _output.WriteLine($"Initial GRC:   {testCase.Expected.InitialGRC,-10} | {actualInitialGRC}");
        _output.WriteLine($"Final GRC:     {testCase.Expected.FinalGRC,-10} | {actualFinalGRC}");
        _output.WriteLine($"Initial ARC:   {testCase.Expected.InitialARC,-10} | {actualInitialARC}");
        _output.WriteLine($"Residual ARC:  {testCase.Expected.ResidualARC,-10} | {actualResidualARC}");
        _output.WriteLine($"SAIL:          {testCase.Expected.Sail,-10} | {actualSAIL}");
        _output.WriteLine($"TMPR:          {testCase.Expected.Tmpr,-10} | {actualTMPR}");

        // Verify all critical outputs - treat null properties as "null" string
            var expectedInitialGRC = testCase.Expected.InitialGRC;
            var expectedFinalGRC = testCase.Expected.FinalGRC;
            var expectedInitialARC = testCase.Expected.InitialARC;
            var expectedResidualARC = testCase.Expected.ResidualARC;
            var expectedSail = testCase.Expected.Sail;
            // If TMPR is null/empty in test case, don't assert it (accepts any value)
            var expectedTmpr = string.IsNullOrWhiteSpace(testCase.Expected.Tmpr) ? actualTMPR : testCase.Expected.Tmpr;
        
        Assert.Equal(expectedInitialGRC, actualInitialGRC);
        Assert.Equal(expectedFinalGRC, actualFinalGRC);
        Assert.Equal(expectedInitialARC, actualInitialARC);
        Assert.Equal(expectedResidualARC, actualResidualARC);
        Assert.Equal(expectedSail, actualSAIL);
        Assert.Equal(expectedTmpr, actualTMPR);

        _output.WriteLine($"\n✅ Test PASSED: {testCase.Name}");
        _output.WriteLine($"═══════════════════════════════════════════════════════\n");
    }

    public static IEnumerable<object[]> LoadTestCases()
    {
        var testFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",
            "TestData",
            "SORAAuthoritative_TestCases.v2.json"
        );

        if (!File.Exists(testFilePath))
        {
            throw new FileNotFoundException($"Test case file not found: {testFilePath}");
        }

        var jsonContent = File.ReadAllText(testFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        var testSuite = JsonSerializer.Deserialize<AuthoritativeTestSuite>(jsonContent, options);

        if (testSuite?.TestCases == null || !testSuite.TestCases.Any())
        {
            throw new InvalidOperationException("No test cases found in JSON file");
        }

        foreach (var testCase in testSuite.TestCases)
        {
            yield return new object[] { testCase };
        }
    }

    private object BuildSORARequest(AuthoritativeTestCase testCase)
    {
        var input = testCase.Input;
        
        // Build mitigations array
        var mitigations = new List<object>();
        if (input.Sheltering)
        {
            mitigations.Add(new { type = "M1A", robustness = "Low" });
        }

        // Build ground risk section - DIFFERENT FOR V2.0 vs V2.5!
        // Derive MTOM from maxCharacteristicDimension if not provided (realistic estimate)
        double mtom_kg = input.MaxDimension switch
        {
            < 1.0 => 0.5,    // Small drones ~0.5kg
            < 2.0 => 1.5,    // Medium drones ~1.5kg
            < 4.0 => 3.0,    // Large drones ~3kg
            < 10.0 => 8.0,   // Very large ~8kg
            _ => 15.0        // Extra large ~15kg
        };
        
        object groundRisk;
        if (input.SoraVersion == "2.0")
        {
            // SORA 2.0 uses scenario enum, NOT population density
            var scenario = MapPopulationDensityToScenario(input.PopulationDensity, input.ControlledArea);
            groundRisk = new
            {
                scenario_V2_0 = scenario,
                maxCharacteristicDimension = input.MaxDimension,
                mtom_kg = mtom_kg,  // Add MTOM for GRC calculation
                mitigations = mitigations.ToArray()
            };
        }
        else // SORA 2.5
        {
            groundRisk = new
            {
                populationDensity = input.PopulationDensity,
                isControlledGroundArea = input.ControlledArea,
                maxCharacteristicDimension = input.MaxDimension,
                maxSpeed = input.MaxSpeed,
                mtom_kg = mtom_kg,  // Add MTOM for GRC calculation
                mitigations = mitigations.ToArray()
            };
        }

        // Build air risk section - compute explicitARC based on test inputs
        string derivedARC = DeriveARCFromInputs(input);
        
        // Add strategic mitigations if specified
        var airRisk = input.StrategicMitigations != null && input.StrategicMitigations.Any()
            ? new { explicitARC = derivedARC, strategicMitigations = input.StrategicMitigations }
            : (object)new { explicitARC = derivedARC };

        // Build complete request (matching SORAGolden pattern)
        return new
        {
            soraVersion = input.SoraVersion,
            missionId = testCase.TestId,
            missionName = testCase.Name,
            groundRisk,
            airRisk,
            implementedOSOs = new object[] { } // Minimal OSOs for testing
        };
    }

    private string MapPopulationDensityToScenario(double density, bool controlledArea)
    {
        // SORA 2.0 scenarios mapping from population density
        if (controlledArea)
            return "ControlledGroundArea";
        
        // Assembly/Gathering >50k people/km²
        if (density >= 50000)
            return "VLOS_GatheringOfPeople";
        
        // Populated environment 500-50000 people/km² (assume VLOS for these tests)
        if (density >= 500)
            return "VLOS_Populated";
        
        // Sparsely populated <500 people/km² (assume BVLOS if not explicitly VLOS)
        // For tests with altitude <120m, could be VLOS
        return "BVLOS_SparselyPopulated";
    }

    private string DeriveARCFromInputs(TestInput input)
    {
        // Atypical/Segregated → ARC-a
        if (input.IsAtypicalSegregated)
            return "ARC_a";

        // Above AEC threshold (500ft) + Controlled → ARC-d
        if (input.AltitudeAGL_ft > 500 && input.ControlledAirspace)
            return "ARC_d";

        // Above AEC threshold (500ft) + Uncontrolled → ARC-c
        if (input.AltitudeAGL_ft > 500 && !input.ControlledAirspace)
            return "ARC_c";

        // Below AEC, Urban, Controlled → ARC-c
        if (input.UrbanAir && input.ControlledAirspace)
            return "ARC_c";

        // Below AEC, Urban, Uncontrolled → ARC-c
        if (input.UrbanAir && !input.ControlledAirspace)
            return "ARC_c";

        // Below AEC, Rural, Controlled → ARC-c
        if (!input.UrbanAir && input.ControlledAirspace)
            return "ARC_c";

        // Below AEC, Rural, Uncontrolled → ARC-b
        return "ARC_b";
    }
}

#region Test Data Models

public class AuthoritativeTestSuite
{
    public string Description { get; set; } = string.Empty;
    public string Generated { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<AuthoritativeTestCase> TestCases { get; set; } = new();
}

public class AuthoritativeTestCase
{
    public string TestId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public bool ExpectedToFail { get; set; } = false; // For out-of-scope tests
    public TestInput Input { get; set; } = new();
    public TestExpected Expected { get; set; } = new();
}

public class TestInput
{
    public string SoraVersion { get; set; } = string.Empty;
    public string? DroneId { get; set; }
    public double MaxDimension { get; set; }
    public double MaxSpeed { get; set; }
    public string Environment { get; set; } = string.Empty;
    public double PopulationDensity { get; set; }
    public bool ControlledArea { get; set; }
    public bool Sheltering { get; set; }
    public bool IsAtypicalSegregated { get; set; }
    public double AltitudeAGL_ft { get; set; }
    public string AirspaceClass { get; set; } = string.Empty;
    public bool ControlledAirspace { get; set; }
    public bool UrbanAir { get; set; }
    public string[] StrategicMitigations { get; set; } = Array.Empty<string>();
}

public class TestExpected
{
    public string InitialGRC { get; set; } = string.Empty;
    public string FinalGRC { get; set; } = string.Empty;
    public string InitialARC { get; set; } = string.Empty;
    public string ResidualARC { get; set; } = string.Empty;
    public string Sail { get; set; } = string.Empty;
        public string? Tmpr { get; set; }
}

#endregion
