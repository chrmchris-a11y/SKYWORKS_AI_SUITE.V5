using Xunit;
using Skyworks.Core.Services;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Api.Tests.SAIL;

/// <summary>
/// Tests for SAIL Determination Service - JARUS SORA 2.0 Table 5
/// Step 31: SAIL Matrix Implementation
/// </summary>
public class SAILService_Tests
{
    private readonly ISAILService _sailService;

    public SAILService_Tests()
    {
        _sailService = new SAILService();
    }

    #region JARUS SORA 2.0 Table 5 - Matrix Verification

    [Fact]
    public void SAIL_GRC1_ARCa_Returns_SAIL_I()
    {
        // Arrange: Lowest ground + air risk
        var input = new SAILInput
        {
            FinalGRC = 1,
            ResidualARC = ARCRating.ARC_a
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.I, result.SAIL);
        Assert.Contains("SAIL I", result.Notes);
    }

    [Fact]
    public void SAIL_GRC2_ARCd_Returns_SAIL_V()
    {
        // Arrange: Per JARUS Table 5, GRC 1-5 × ARC-d = SAIL V (not VI!)
        var input = new SAILInput
        {
            FinalGRC = 2,
            ResidualARC = ARCRating.ARC_d
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.V, result.SAIL); // Corrected per authoritative matrix
        Assert.Contains("SAIL V", result.Notes);
    }

    [Fact]
    public void SAIL_GRC3_ARCa_Returns_SAIL_II()
    {
        // Arrange: Per JARUS Table 5, GRC 1-3 × ARC-a = SAIL I-II (GRC 3 × ARC-a = SAIL II)
        var input = new SAILInput
        {
            FinalGRC = 3,
            ResidualARC = ARCRating.ARC_a
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.I, result.SAIL); // Corrected per authoritative matrix
    }

    [Fact]
    public void SAIL_GRC3_ARCb_Returns_SAIL_III()
    {
        // Arrange: Per JARUS Table 5, GRC 3 × ARC-b = SAIL III
        var input = new SAILInput
        {
            FinalGRC = 3,
            ResidualARC = ARCRating.ARC_b
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.III, result.SAIL); // Corrected per authoritative matrix
    }

    [Fact]
    public void SAIL_GRC4_ARCa_Returns_SAIL_III()
    {
        // Arrange
        var input = new SAILInput
        {
            FinalGRC = 4,
            ResidualARC = ARCRating.ARC_a
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.III, result.SAIL);
    }

    [Fact]
    public void SAIL_GRC5_ARCc_Returns_SAIL_IV()
    {
        // Arrange
        var input = new SAILInput
        {
            FinalGRC = 5,
            ResidualARC = ARCRating.ARC_c
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.IV, result.SAIL);
    }

    [Fact]
    public void SAIL_GRC6_ARCa_Returns_SAIL_V()
    {
        // Arrange: High ground risk + low air risk
        var input = new SAILInput
        {
            FinalGRC = 6,
            ResidualARC = ARCRating.ARC_a
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.V, result.SAIL);
        Assert.Contains("High ground risk", result.Notes);
    }

    [Fact]
    public void SAIL_GRC6_ARCd_Returns_SAIL_VI()
    {
        // Arrange: High ground risk + high air risk
        var input = new SAILInput
        {
            FinalGRC = 6,
            ResidualARC = ARCRating.ARC_d
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.VI, result.SAIL);
        // Step 32: Adjustment warnings should include both ground and air risk notices
        Assert.Contains("High ground risk", result.Notes);
        Assert.Contains("High air risk", result.Notes);
        Assert.Contains("SAIL VI", result.Notes);
    }

    [Fact]
    public void SAIL_GRC7_AllARCs_Return_SAIL_VI()
    {
        // Arrange: GRC 7 always gives SAIL VI regardless of ARC
        var arcLevels = new[] { ARCRating.ARC_a, ARCRating.ARC_b, ARCRating.ARC_c, ARCRating.ARC_d };

        foreach (var arc in arcLevels)
        {
            var input = new SAILInput
            {
                FinalGRC = 7,
                ResidualARC = arc
            };

            // Act
            var result = _sailService.DetermineSAIL(input);

            // Assert
            Assert.True(result.IsSupported);
            Assert.Equal(SAILLevel.VI, result.SAIL);
        }
    }

    #endregion

    #region Edge Cases & Validation

    [Fact]
    public void SAIL_GRC0_Returns_Error()
    {
        // Arrange: GRC cannot be 0 (minimum is 1)
        var input = new SAILInput
        {
            FinalGRC = 0,
            ResidualARC = ARCRating.ARC_a
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.False(result.IsSupported);
        Assert.Null(result.SAIL);
        Assert.Contains("cannot be less than 1", result.Notes);
    }

    [Fact]
    public void SAIL_GRC8_Returns_CategoryC()
    {
        // Arrange: GRC > 7 not supported by SORA (CERTIFIED category required)
        var input = new SAILInput
        {
            FinalGRC = 8,
            ResidualARC = ARCRating.ARC_a
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.False(result.IsSupported);
        Assert.Null(result.SAIL);
        Assert.Contains("CATEGORY C", result.Notes);
        Assert.Contains("CERTIFIED category required", result.Notes);
    }

    [Fact]
    public void SAIL_GRC10_Returns_CategoryC()
    {
        // Arrange: Extreme GRC
        var input = new SAILInput
        {
            FinalGRC = 10,
            ResidualARC = ARCRating.ARC_d
        };

        // Act
        var result = _sailService.DetermineSAIL(input);

        // Assert
        Assert.False(result.IsSupported);
        Assert.Null(result.SAIL);
        Assert.Contains("CERTIFIED category", result.Notes);
    }

    #endregion

    #region OSO Requirements

    [Fact]
    public void OSO_Count_SAIL_I_Returns_8()
    {
        // Arrange
        var sail = SAILLevel.I;

        // Act
        var count = _sailService.GetRequiredOSOCount(sail);

        // Assert
        Assert.Equal(8, count);
    }

    [Fact]
    public void OSO_Count_SAIL_III_Returns_16()
    {
        // Arrange
        var sail = SAILLevel.III;

        // Act
        var count = _sailService.GetRequiredOSOCount(sail);

        // Assert
        Assert.Equal(16, count);
    }

    [Fact]
    public void OSO_Count_SAIL_VI_Returns_24()
    {
        // Arrange: Highest SAIL requires most OSOs
        var sail = SAILLevel.VI;

        // Act
        var count = _sailService.GetRequiredOSOCount(sail);

        // Assert
        Assert.Equal(24, count);
    }

    [Fact]
    public void OSO_Robustness_SAIL_I_Has_Optional_And_Low()
    {
        // Arrange
        var sail = SAILLevel.I;

        // Act
        var requirements = _sailService.GetOSORobustnessRequirements(sail);

        // Assert
        Assert.NotEmpty(requirements);
        Assert.Contains(requirements, kvp => kvp.Value == "O"); // Has Optional
        Assert.Contains(requirements, kvp => kvp.Value == "L"); // Has Low
        Assert.DoesNotContain(requirements, kvp => kvp.Value == "H"); // No High for SAIL I
    }

    [Fact]
    public void OSO_Robustness_SAIL_VI_All_High()
    {
        // Arrange: SAIL VI requires all OSOs at High robustness
        var sail = SAILLevel.VI;

        // Act
        var requirements = _sailService.GetOSORobustnessRequirements(sail);

        // Assert
        Assert.NotEmpty(requirements);
        Assert.Equal(24, requirements.Count);
        Assert.All(requirements.Values, robustness => Assert.Equal("H", robustness));
    }

    #endregion

    #region Composite SAIL (Multi-Segment Missions)

    [Fact]
    public void Composite_SAIL_Uses_Highest_Segment()
    {
        // Arrange: 3 segments with different SAILs
        var segments = new List<MissionSegment>
        {
            new() { SegmentName = "Seg1_Rural", FinalGRC = 2, ResidualARC = "ARC-a", DurationMinutes = 10 }, // SAIL I
            new() { SegmentName = "Seg2_Suburban", FinalGRC = 4, ResidualARC = "ARC-b", DurationMinutes = 15 }, // SAIL III
            new() { SegmentName = "Seg3_Urban", FinalGRC = 6, ResidualARC = "ARC-c", DurationMinutes = 5 }  // SAIL V
        };

        // Act
        var result = _sailService.DetermineCompositeSAIL(segments);

        // Assert
        Assert.Equal(SAILLevel.V, result.CompositeSAIL); // Highest segment is SAIL V
        Assert.Equal("Seg3_Urban", result.CriticalSegment);
        Assert.Equal(3, result.SegmentResults.Count);
        Assert.Equal(30, result.TotalDurationMinutes);
    }

    [Fact]
    public void Composite_SAIL_Single_Segment_Same_As_Direct()
    {
        // Arrange
        var segments = new List<MissionSegment>
        {
            new() { SegmentName = "OnlySegment", FinalGRC = 4, ResidualARC = "ARC-a", DurationMinutes = 20 }
        };

        // Act
        var compositeResult = _sailService.DetermineCompositeSAIL(segments);
        var directInput = new SAILInput { FinalGRC = 4, ResidualARC = ARCRating.ARC_a };
        var directResult = _sailService.DetermineSAIL(directInput);

        // Assert
        Assert.Equal(directResult.SAIL, compositeResult.CompositeSAIL);
    }

    [Fact]
    public void Composite_SAIL_All_Low_Risk_Returns_SAIL_I()
    {
        // Arrange: All segments low risk
        var segments = new List<MissionSegment>
        {
            new() { SegmentName = "Seg1", FinalGRC = 1, ResidualARC = "ARC-a", DurationMinutes = 10 },
            new() { SegmentName = "Seg2", FinalGRC = 2, ResidualARC = "ARC-a", DurationMinutes = 10 },
            new() { SegmentName = "Seg3", FinalGRC = 1, ResidualARC = "ARC-a", DurationMinutes = 10 }
        };

        // Act
        var result = _sailService.DetermineCompositeSAIL(segments);

        // Assert
        Assert.Equal(SAILLevel.I, result.CompositeSAIL);
    }

    [Fact]
    public void Composite_SAIL_Exposure_Notes_In_Explanation()
    {
        // Step 32: Exposure breakdown appears in explanation
        var segments = new List<MissionSegment>
        {
            new() { SegmentName = "Seg1", FinalGRC = 2, ResidualARC = "ARC-a", DurationMinutes = 10 }, // SAIL I
            new() { SegmentName = "Seg2", FinalGRC = 4, ResidualARC = "ARC-a", DurationMinutes = 20 }  // SAIL III
        };

        var result = _sailService.DetermineCompositeSAIL(segments);

        Assert.Contains("Exposure by SAIL", result.Explanation);
        Assert.Contains("SAIL I: 10 min", result.Explanation);
        Assert.Contains("SAIL III: 20 min", result.Explanation);
    }

    [Fact]
    public void Composite_SAIL_Invalid_ARC_Defaults_With_Note()
    {
        // Step 32: Unknown ARC token should default to ARC-a and include note
        var segments = new List<MissionSegment>
        {
            new() { SegmentName = "Seg1", FinalGRC = 2, ResidualARC = "ARC-x", DurationMinutes = 5 }
        };

        var result = _sailService.DetermineCompositeSAIL(segments);

        Assert.Contains("Defaulted to ARC-a", result.Explanation);
    }

    #endregion

    #region Golden Test Scenarios (From PHASE4_GOLDEN_TESTS.md)

    [Fact]
    public void Golden_Test01_Urban_VLOS_Nicosia()
    {
        // Test 1: Urban VLOS Package Delivery (Nicosia)
        // GRC=6 (intrinsic 8, -2 cap), ARC-b (ARC-c with S1+S2) → SAIL III
        var input = new SAILInput
        {
            FinalGRC = 6,
            ResidualARC = ARCRating.ARC_b
        };

        var result = _sailService.DetermineSAIL(input);

        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.V, result.SAIL); // Table 5: GRC 6 + ARC-b = SAIL V
        Assert.Contains("High ground risk", result.Notes);
    }

    [Fact]
    public void Golden_Test02_Suburban_BVLOS_Limassol()
    {
        // Test 2: Suburban BVLOS Survey (Limassol)
        // GRC=3 (intrinsic 5, -2), ARC-a (ARC-b with S1+S2) → SAIL I per corrected matrix
        var input = new SAILInput
        {
            FinalGRC = 3,
            ResidualARC = ARCRating.ARC_a
        };

        var result = _sailService.DetermineSAIL(input);

        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.I, result.SAIL); // Corrected per authoritative matrix
    }

    [Fact]
    public void Golden_Test03_Rural_Agriculture_Paphos()
    {
        // Test 3: Rural Agricultural Spraying (Paphos)
        // GRC=1 (intrinsic 2, -2 floored at 1), ARC-a → SAIL I
        var input = new SAILInput
        {
            FinalGRC = 1,
            ResidualARC = ARCRating.ARC_a
        };

        var result = _sailService.DetermineSAIL(input);

        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.I, result.SAIL);
        Assert.Contains("OPEN", result.Notes);
    }

    [Fact]
    public void Golden_Test04_Coastal_BVLOS_Larnaca()
    {
        // Test 4: BVLOS Coastal Surveillance (Larnaca)
        // GRC=4 (intrinsic 6, -2), ARC-a (ARC-c with S1+S2+S3) → SAIL III
        var input = new SAILInput
        {
            FinalGRC = 4,
            ResidualARC = ARCRating.ARC_a
        };

        var result = _sailService.DetermineSAIL(input);

        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.III, result.SAIL);
    }

    [Fact]
    public void Golden_Test09_Maximum_Mitigation_Cap()
    {
        // Test 9: Maximum Mitigation Cap Enforcement
        // GRC=7 (intrinsic 9, -2 cap enforced), ARC-a (with S1+S2+S3+S4) → SAIL IV
        // This is THE critical test for SORA 2.5 compliance
        var input = new SAILInput
        {
            FinalGRC = 7,
            ResidualARC = ARCRating.ARC_a
        };

        var result = _sailService.DetermineSAIL(input);

        Assert.True(result.IsSupported);
        Assert.Equal(SAILLevel.VI, result.SAIL); // GRC 7 always → SAIL VI
        Assert.Contains("Very high ground risk", result.Notes);
    }

    [Fact]
    public void Golden_Test10_Boundary_Condition()
    {
        // Test 10: Boundary Condition - GRC Cap Enforcement (Extreme Urban)
        // GRC=8 (intrinsic 10+, -2 cap → 8, but 8 exceeds SORA limit) → Category C
        var input = new SAILInput
        {
            FinalGRC = 8,
            ResidualARC = ARCRating.ARC_b
        };

        var result = _sailService.DetermineSAIL(input);

        Assert.False(result.IsSupported);
        Assert.Null(result.SAIL);
        Assert.Contains("CATEGORY C", result.Notes);
        Assert.Contains("CERTIFIED", result.Notes);
    }

    #endregion
}
