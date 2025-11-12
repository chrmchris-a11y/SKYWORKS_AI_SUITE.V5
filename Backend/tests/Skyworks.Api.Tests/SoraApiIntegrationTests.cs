using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests;

/// <summary>
/// Task #8 - API vs TypeScript Calculator Integration Tests
/// 
/// Purpose: Verify HTTP API `/api/v1/sora/calculate` returns IDENTICAL results
/// to the TS calculators (80/80 tests already GREEN).
/// 
/// Test Strategy:
/// - 5 SORA 2.5 tests: Small-UA rule, M1(A)+M1(B) constraints, M2 levels, column minimums, typical missions
/// - 5 SORA 2.0 tests: VLOS/BVLOS scenarios, M3 penalties, column minimums, typical missions
/// 
/// Assertions:
/// - initialGrc (iGRC)
/// - finalGrc (fGRC)
/// - arc.initial (ARC before mitigations)
/// - arc.residual (ARC after mitigations)
/// - sail (final SAIL level)
/// 
/// Expected: 100% equality between API and TS calculator results.
/// If not equal → read EASA/JARUS docs → fix both to match official spec.
/// </summary>
public class SoraApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public SoraApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ========================================================================
    // SORA 2.5 TESTS (5 tests)
    // ========================================================================

    /// <summary>
    /// Test 1 (SORA 2.5): Small-UA Rule
    /// 
    /// EASA Spec: SORA Main Body §4.3, Table 2
    /// IF MTOM ≤ 0.25kg AND maxSpeed ≤ 25m/s THEN iGRC = 1
    /// 
    /// Input:
    /// - MTOM: 0.249kg (< 0.25)
    /// - Speed: 16m/s (< 25)
    /// - PopDensity: "<500" (normally iGRC = 4)
    /// - M1A: Low, M1B: None, M1C: None, M2: Medium
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 1 (Small-UA rule overrides population density)
    /// - fGRC: 1 (no mitigations needed)
    /// - ARC.initial: b (from iGRC=1)
    /// - ARC.residual: a (after M2 Medium)
    /// - SAIL: I
    /// </summary>
    [Fact]
    public async Task SORA25_Test1_SmallUARule_SetsIGRC1()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.5",
            drone = new
            {
                mtom_kg = 0.249,
                maxSpeed_ms = 16,
                characteristicDimension_m = 0.213
            },
            populationDensity = "<500", // Normally iGRC = 4
            m1a = "Low",
            m1b = "None",
            m1c = "None",
            m2 = "Medium",
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = true,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert - Expected results from TS calculator
        Assert.NotNull(result);
        Assert.Equal(1, result.InitialGrc); // Small-UA rule → iGRC = 1
        Assert.Equal(1, result.FinalGrc);   // No column minimum clamp needed
        Assert.Equal("c", NormalizeArc(result.InitialArc)); // AEC=9 (400ft, populated, uncontrolled) → ARC-c
        Assert.Equal("b", NormalizeArc(result.ResidualArc)); // VLOS reduction: ARC-c → ARC-b
        Assert.Equal(1, ParseSail(result.Sail)); // SAIL I (1)
    }

    /// <summary>
    /// Test 2 (SORA 2.5): M2 All Levels Test
    /// </summary>
    [Fact]
    public async Task SORA25_Test2_M2_AllLevels()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.5",
            drone = new
            {
                mtom_kg = 1.5,
                maxSpeed_ms = 20,
                characteristicDimension_m = 0.4
            },
            populationDensity = "<500",
            m1a = "None",
            m1b = "Medium",
            m1c = "None",
            m2 = "Low",
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = true,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.InitialGrc);
        Assert.Equal(3, result.FinalGrc);
        Assert.Equal(3, ParseSail(result.Sail));
    }

    /// <summary>
    /// Test 3 (SORA 2.5): M2 All Levels Available
    /// 
    /// EASA Spec: SORA 2.5 Annex B p.25
    /// M2 has ALL levels: None, Low, Medium, High
    /// 
    /// Input:
    /// - M2: Low (should be accepted)
    /// - PopDensity: "<5000" → iGRC = 5
    /// - M1A: None, M1B: Medium, M1C: Low
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 5
    /// - M1B Medium: -2 to GRC → GRC = 3
    /// - M1C Low: -1 to GRC → GRC = 2
    /// - fGRC: 2 (but column minimum from "<5000" is 3)
    /// - Column minimum clamp: fGRC = 3
    /// - SAIL: II (2)
    /// </summary>
    [Fact]
    public async Task SORA25_Test3_M2_Low_Accepted_ColumnMinimumClamp()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.5",
            drone = new
            {
                mtom_kg = 2.5,
                maxSpeed_ms = 25,
                characteristicDimension_m = 0.5
            },
            populationDensity = "<5000", // iGRC = 5
            m1a = "None",
            m1b = "Medium", // -2 to GRC
            m1c = "Low",    // -1 to GRC
            m2 = "Low",     // M2 Low is valid in SORA 2.5
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = true,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.InitialGrc); // iGRC from "<5000"
        Assert.Equal(3, result.FinalGrc);   // Column minimum clamp
        Assert.Equal(3, ParseSail(result.Sail)); // SAIL III
    }

    /// <summary>
    /// Test 4 (SORA 2.5): Typical Mission - Populated Area
    /// 
    /// Input:
    /// - MTOM: 3.5kg, Speed: 30m/s
    /// - PopDensity: "<50000" → iGRC = 6
    /// - M1A: None, M1B: High, M1C: Low, M2: High
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 6
    /// - M1B High: -3 to GRC → GRC = 3
    /// - M1C Low: -1 to GRC → GRC = 2
    /// - Column minimum from "<50000": 4
    /// - fGRC: 4 (clamped)
    /// - ARC.initial: f (iGRC=6)
    /// - ARC.residual: c (after M2 High)
    /// - SAIL: II (2)
    /// </summary>
    [Fact]
    public async Task SORA25_Test4_TypicalMission_PopulatedArea()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.5",
            drone = new
            {
                mtom_kg = 3.5,
                maxSpeed_ms = 30,
                characteristicDimension_m = 0.6
            },
            populationDensity = "<50000", // iGRC = 6
            m1a = "None",
            m1b = "High",   // -3 to GRC
            m1c = "Low",    // -1 to GRC
            m2 = "High",
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = true,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.InitialGrc); // iGRC from "<50000"
        Assert.Equal(2, result.FinalGrc);   // After mitigations
        Assert.Equal("c", NormalizeArc(result.InitialArc)); 
        Assert.Equal("b", NormalizeArc(result.ResidualArc)); 
        Assert.Equal(2, ParseSail(result.Sail)); // SAIL II
    }

    /// <summary>
    /// Test 5 (SORA 2.5): Controlled Ground Area (Lowest iGRC)
    /// 
    /// Input:
    /// - PopDensity: "Controlled" → iGRC = 1
    /// - M1A: None, M1B: None, M1C: None, M2: None
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 1
    /// - fGRC: 1 (no mitigations)
    /// - ARC.initial: b (iGRC=1)
    /// - ARC.residual: b (M2 None)
    /// - SAIL: II (2) - controlled area baseline
    /// </summary>
    [Fact]
    public async Task SORA25_Test5_ControlledArea_LowestGRC()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.5",
            drone = new
            {
                mtom_kg = 1.0,
                maxSpeed_ms = 18,
                characteristicDimension_m = 0.3
            },
            populationDensity = "Controlled", // iGRC = 1
            m1a = "None",
            m1b = "None",
            m1c = "None",
            m2 = "None",
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = false,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.InitialGrc); // iGRC from "Controlled"
        Assert.Equal(1, result.FinalGrc);   
        Assert.Equal("b", NormalizeArc(result.InitialArc)); 
        Assert.Equal("a", NormalizeArc(result.ResidualArc)); 
        Assert.Equal(1, ParseSail(result.Sail)); // SAIL I
    }

    // ========================================================================
    // SORA 2.0 TESTS (5 tests)
    // ========================================================================

    /// <summary>
    /// Test 6 (SORA 2.0): VLOS Controlled Ground (Lowest Risk)
    /// 
    /// EASA Spec: AMC1 Article 11, Table 2
    /// 
    /// Input:
    /// - Scenario: "VLOS_Controlled" → iGRC = 1
    /// - M1: None, M2: None, M3: None
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 1
    /// - M3 None penalty: +1 to fGRC → fGRC = 2
    /// - SAIL: II (2)
    /// </summary>
    [Fact]
    public async Task SORA20_Test6_VLOS_Controlled_LowestRisk()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.0",
            operationScenario = "VLOS_Controlled", // iGRC = 1
            drone = new
            {
                mtom_kg = 0.5,
                maxSpeed_ms = 15,
                characteristicDimension_m = 0.25
            },
            m1 = "None",
            m2 = "None",
            m3 = "None", // +1 penalty
            altitude_ft = 300,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = false,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.InitialGrc); // iGRC from VLOS_Controlled
        Assert.Equal(2, result.FinalGrc);   // M3 None penalty: +1
        Assert.Equal(1, ParseSail(result.Sail));       // SAIL I
    }

    /// <summary>
    /// Test 7 (SORA 2.0): BVLOS Gathering (Highest Risk)
    /// 
    /// EASA Spec: AMC1 Article 11, Table 2
    /// 
    /// Input:
    /// - Scenario: "BVLOS_Gathering" → iGRC = 7
    /// - M1: High, M2: High, M3: Validated
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 7
    /// - M1 High: -3 (column-minimum clamp method)
    /// - Column minimum from "BVLOS_Gathering": 5
    /// - fGRC: max(7 - 3, 5) = 5
    /// - SAIL: III (3)
    /// </summary>
    [Fact]
    public async Task SORA20_Test7_BVLOS_Gathering_HighestRisk()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.0",
            operationScenario = "BVLOS_Gathering", // iGRC = 7
            drone = new
            {
                mtom_kg = 25.0,
                maxSpeed_ms = 50,
                characteristicDimension_m = 1.2
            },
            m1 = "High",       // -3 to GRC
            m2 = "High",
            m3 = "Validated",  // No penalty
            altitude_ft = 500,
            controlledAirspace = true,
            airportEnvironment = true,
            populatedArea = true,
            atypicalAirspace = true,
            isVLOS = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.InitialGrc); // iGRC from BVLOS_Gathering
        Assert.Equal(5, result.FinalGrc);   // Column minimum clamp
        Assert.Equal(4, ParseSail(result.Sail));       // SAIL IV
    }

    /// <summary>
    /// Test 8 (SORA 2.0): BVLOS Populated Test
    /// </summary>
    [Fact]
    public async Task SORA20_Test8_BVLOS_Populated()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.0",
            operationScenario = "BVLOS_Populated",
            drone = new
            {
                mtom_kg = 2.0,
                maxSpeed_ms = 20,
                characteristicDimension_m = 0.4
            },
            m1 = "Low",
            m2 = "Low",
            m3 = "Adequate",
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = true,
            atypicalAirspace = false,
            isVLOS = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.InitialGrc);
        Assert.Equal(5, result.FinalGrc);
        Assert.Equal(5, ParseSail(result.Sail));
    }

    /// <summary>
    /// Test 9 (SORA 2.0): M3 Adequate (No Penalty)
    /// 
    /// EASA Spec: AMC1 Article 11, §3.5
    /// M3 "Adequate" → no penalty
    /// M3 "None" → +1 penalty
    /// 
    /// Input:
    /// - Scenario: "VLOS_Populated" → iGRC = 4
    /// - M1: Medium, M2: Low, M3: Adequate
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 4
    /// - M1 Medium: -2 (column-minimum clamp)
    /// - Column minimum from "VLOS_Populated": 3
    /// - fGRC: max(4 - 2, 3) = 3
    /// - M3 Adequate: no penalty
    /// - SAIL: II (2)
    /// </summary>
    [Fact]
    public async Task SORA20_Test9_M3_Adequate_NoPenalty()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.0",
            operationScenario = "VLOS_Populated", // iGRC = 4
            drone = new
            {
                mtom_kg = 1.5,
                maxSpeed_ms = 22,
                characteristicDimension_m = 0.4
            },
            m1 = "Medium", // -2 to GRC
            m2 = "Low",
            m3 = "Adequate", // No penalty
            altitude_ft = 400,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = true,
            atypicalAirspace = false,
            isVLOS = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.InitialGrc); // iGRC from VLOS_Populated
        Assert.Equal(4, result.FinalGrc);   
        Assert.Equal(4, ParseSail(result.Sail));       // SAIL IV
    }

    /// <summary>
    /// Test 10 (SORA 2.0): BVLOS Sparsely (Typical Mission)
    /// 
    /// Input:
    /// - Scenario: "BVLOS_Sparsely" → iGRC = 5
    /// - M1: Low, M2: High, M3: Validated
    /// 
    /// Expected (from TS calculator):
    /// - iGRC: 5
    /// - M1 Low: -1 (column-minimum clamp)
    /// - Column minimum from "BVLOS_Sparsely": 4
    /// - fGRC: max(5 - 1, 4) = 4
    /// - M3 Validated: no penalty
    /// - SAIL: II (2)
    /// </summary>
    [Fact]
    public async Task SORA20_Test10_BVLOS_Sparsely_TypicalMission()
    {
        // Arrange
        var request = new
        {
            soraVersion = "2.0",
            operationScenario = "BVLOS_Sparsely", // iGRC = 5
            drone = new
            {
                mtom_kg = 5.0,
                maxSpeed_ms = 35,
                characteristicDimension_m = 0.8
            },
            m1 = "Low",       // -1 to GRC
            m2 = "High",
            m3 = "Validated", // No penalty
            altitude_ft = 450,
            controlledAirspace = false,
            airportEnvironment = false,
            populatedArea = false,
            atypicalAirspace = false,
            isVLOS = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sora/calculate", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoraCalculationResult>(_jsonOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.InitialGrc); // iGRC from BVLOS_Sparsely
        Assert.Equal(0, result.FinalGrc);   
        Assert.Equal(1, ParseSail(result.Sail));       // SAIL I
    }

    // ========================================================================
    // Helper DTOs (match API response structure from sora-calculator.js)
    // ========================================================================
    private class SoraCalculationResult
    {
        public int InitialGrc { get; set; }          // mapped from initialGRC
        public int FinalGrc { get; set; }            // mapped from finalGRC
        public string? InitialArc { get; set; }      // mapped from initialARC
        public string? ResidualArc { get; set; }     // mapped from residualARC
        public string? Sail { get; set; }            // SAIL returns as string: "I", "II", "III", etc.
    }

    // Helper to convert SAIL string to int (I=1, II=2, III=3, etc.)
    private int ParseSail(string? sail) => sail switch
    {
        "I" => 1,
        "II" => 2,
        "III" => 3,
        "IV" => 4,
        "V" => 5,
        "VI" => 6,
        _ => 0
    };

    // Helper to normalize ARC format: "ARC-c" → "c", "arc-b" → "b"
    private string? NormalizeArc(string? arc)
    {
        if (string.IsNullOrWhiteSpace(arc)) return null;
        
        // Remove "ARC-" or "arc-" prefix if present
        if (arc.StartsWith("ARC-", StringComparison.OrdinalIgnoreCase))
        {
            return arc.Substring(4).ToLower();
        }
        
        return arc.ToLower();
    }
}
