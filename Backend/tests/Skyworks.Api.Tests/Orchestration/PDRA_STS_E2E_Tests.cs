using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Skyworks.Api.Tests.Orchestration;

/// <summary>
/// End-to-End Tests for PDRA and STS Operations
/// Based on EU Regulation 2019/947 (Easy Access Rules for UAS - July 2024 Edition)
/// Tests verify PDRA-S01, PDRA-S02, STS-01, and STS-02 compliance requirements
/// </summary>
public class PDRA_STS_E2E_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public PDRA_STS_E2E_Tests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [MemberData(nameof(LoadTestCases))]
    public async Task PDRA_STS_Tests_VerifyAgainstEURegulation(PDRASTSTestCase testCase)
    {
        // Arrange
        _output.WriteLine($"═══════════════════════════════════════════════════════");
        _output.WriteLine($"TEST: {testCase.TestId} - {testCase.Description}");
        _output.WriteLine($"Category: {testCase.Category}");
        _output.WriteLine($"Rationale: {testCase.Rationale}");
        if (testCase.ExpectedToFail)
            _output.WriteLine($"⚠️  EXPECTED TO FAIL: Rejection test scenario");
        _output.WriteLine($"═══════════════════════════════════════════════════════");

        var client = _factory.CreateClient();
        var request = BuildPDRASTSRequest(testCase);

        // Act
        var response = await client.PostAsJsonAsync("/api/pdra-sts/evaluate", request);
        var body = await response.Content.ReadAsStringAsync();
        
        // Handle expected failures (rejection scenarios)
        if (testCase.ExpectedToFail)
        {
            // Expected to fail tests should get BadRequest or 200 with approved=false
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                _output.WriteLine($"\n✅ Test PASSED (Expected Rejection - BadRequest): {testCase.TestId}");
                _output.WriteLine($"Response: {body}");
                _output.WriteLine($"═══════════════════════════════════════════════════════\n");
                return; // Test passed - operation correctly rejected
            }
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = JsonDocument.Parse(body).RootElement;
                var approved = json.GetProperty("approved").GetBoolean();
                Assert.False(approved, "Expected operation to be rejected");
                _output.WriteLine($"\n✅ Test PASSED (Expected Rejection - approved=false): {testCase.TestId}");
                _output.WriteLine($"   Operation correctly rejected: {testCase.Expected.RejectionReason}");
                _output.WriteLine($"═══════════════════════════════════════════════════════\n");
                return; // Test passed
            }
            
            Assert.Fail($"Expected rejection test got unexpected status: {response.StatusCode}");
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
        
        var actualApproved = responseJson.GetProperty("approved").GetBoolean();
        var actualSAIL = responseJson.TryGetProperty("sail", out var sail) && sail.ValueKind == JsonValueKind.String
            ? sail.GetString() : "null"; // API already returns SAIL_ prefix
        var actualGRC = responseJson.TryGetProperty("grc", out var grc) && grc.ValueKind == JsonValueKind.String
            ? grc.GetString() : "null";
        var actualARC = responseJson.TryGetProperty("arc", out var arc) && arc.ValueKind == JsonValueKind.String
            ? arc.GetString() : "null";

        _output.WriteLine($"\n[EXPECTED vs ACTUAL]");
        _output.WriteLine($"Approved:      {testCase.Expected.Approved,-10} | {actualApproved}");
        _output.WriteLine($"SAIL:          {testCase.Expected.Sail,-10} | {actualSAIL}");
        _output.WriteLine($"GRC:           {testCase.Expected.Grc,-10} | {actualGRC}");
        _output.WriteLine($"ARC:           {testCase.Expected.Arc,-10} | {actualARC}");

        // Verify outputs
        Assert.Equal(testCase.Expected.Approved, actualApproved);
        
        if (testCase.Expected.Approved)
        {
            Assert.Equal(testCase.Expected.Sail ?? "null", actualSAIL);
            Assert.Equal(testCase.Expected.Grc ?? "null", actualGRC);
            Assert.Equal(testCase.Expected.Arc ?? "null", actualARC);
        }

        _output.WriteLine($"\n✅ Test PASSED: {testCase.TestId}");
        _output.WriteLine($"═══════════════════════════════════════════════════════\n");
    }

    public static IEnumerable<object[]> LoadTestCases()
    {
        var testFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",
            "TestData",
            "PDRA_STS_TestCases.json"
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

        var testSuite = JsonSerializer.Deserialize<PDRASTSTestSuite>(jsonContent, options);

        if (testSuite?.TestCases == null || !testSuite.TestCases.Any())
        {
            throw new InvalidOperationException("No test cases found in JSON file");
        }

        foreach (var testCase in testSuite.TestCases)
        {
            yield return new object[] { testCase };
        }
    }

    private object BuildPDRASTSRequest(PDRASTSTestCase testCase)
    {
        var input = testCase.Input;
        
        // Build base request common to all PDRA/STS types
        var request = new Dictionary<string, object>
        {
            { "category", input.Category },
            { "missionId", testCase.TestId },
            { "description", testCase.Description }
        };

        // Add UAS specifications
        if (input.UasClass != null)
            request["uasClass"] = input.UasClass;
        
        if (input.MaxDimension.HasValue)
            request["maxDimension"] = input.MaxDimension.Value;
        
        if (input.Mtom.HasValue)
            request["mtom"] = input.Mtom.Value;

        // Add operational parameters
        if (input.ControlledGroundArea.HasValue)
            request["controlledGroundArea"] = input.ControlledGroundArea.Value;
        
        if (input.GroundRiskBuffer.HasValue)
            request["groundRiskBuffer"] = input.GroundRiskBuffer.Value;
        
        if (input.PopulationDensity.HasValue)
            request["populationDensity"] = input.PopulationDensity.Value;

        // Add flight parameters
        if (input.Vlos.HasValue)
            request["vlos"] = input.Vlos.Value;
        
        if (input.MaxHeight.HasValue)
            request["maxHeight"] = input.MaxHeight.Value;
        
        if (input.MaxSpeed.HasValue)
            request["maxSpeed"] = input.MaxSpeed.Value;
        
        if (input.FlightRange.HasValue)
            request["flightRange"] = input.FlightRange.Value;

        // Add safety features
        if (input.AirspaceObservers.HasValue)
            request["airspaceObservers"] = input.AirspaceObservers.Value;
        
        if (input.MeteorologicalVisibility.HasValue)
            request["meteorologicalVisibility"] = input.MeteorologicalVisibility.Value;
        
        if (input.LaunchRecoveryVlos.HasValue)
            request["launchRecoveryVlos"] = input.LaunchRecoveryVlos.Value;

        // Add airspace information
        if (input.ControlledAirspace.HasValue)
            request["controlledAirspace"] = input.ControlledAirspace.Value;
        
        if (input.AtcCoordination.HasValue)
            request["atcCoordination"] = input.AtcCoordination.Value;

        // Add operational features
        if (input.TetheredOperation.HasValue)
            request["tetheredOperation"] = input.TetheredOperation.Value;
        
        if (input.TetherLength.HasValue)
            request["tetherLength"] = input.TetherLength.Value;
        
        if (input.AutomaticMode.HasValue)
            request["automaticMode"] = input.AutomaticMode.Value;

        // Add additional requirements (specific to each category)
        if (input.AdditionalRequirements != null && input.AdditionalRequirements.Any())
        {
            request["additionalRequirements"] = input.AdditionalRequirements;
        }

        return request;
    }
}

#region Test Data Models

public class PDRASTSTestSuite
{
    public string Description { get; set; } = string.Empty;
    public string Generated { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<PDRASTSTestCase> TestCases { get; set; } = new();
}

public class PDRASTSTestCase
{
    public string TestId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public bool ExpectedToFail { get; set; } = false;
    public PDRASTSInput Input { get; set; } = new();
    public PDRASTSExpected Expected { get; set; } = new();
}

public class PDRASTSInput
{
    public string Category { get; set; } = string.Empty;
    public string? UasClass { get; set; }
    public double? MaxDimension { get; set; }
    public double? Mtom { get; set; }
    public bool? ControlledGroundArea { get; set; }
    public double? GroundRiskBuffer { get; set; }
    public double? PopulationDensity { get; set; }
    public bool? Vlos { get; set; }
    public double? MaxHeight { get; set; }
    public double? MaxSpeed { get; set; }
    public double? FlightRange { get; set; }
    public int? AirspaceObservers { get; set; }
    public double? MeteorologicalVisibility { get; set; }
    public bool? LaunchRecoveryVlos { get; set; }
    public bool? ControlledAirspace { get; set; }
    public bool? AtcCoordination { get; set; }
    public bool? TetheredOperation { get; set; }
    public double? TetherLength { get; set; }
    public bool? AutomaticMode { get; set; }
    public Dictionary<string, string>? AdditionalRequirements { get; set; }
}

public class PDRASTSExpected
{
    public bool Approved { get; set; }
    public string? Sail { get; set; }
    public string? Grc { get; set; }
    public string? Arc { get; set; }
    public string? RejectionReason { get; set; }
}

#endregion
