using Xunit;
using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Services.GRC;

namespace Skyworks.Api.Tests.GRC;

/// <summary>
/// Comprehensive test suite for SORA 2.5 GRC calculations
/// Validates Table 2 (iGRC) and Table 5 (mitigations) accuracy
/// </summary>
public class GRCCalculationTests_SORA_2_5
{
    private readonly IGRCCalculationService _grcService;

        public GRCCalculationTests_SORA_2_5()
    {
        _grcService = new GRCCalculationService();
    }

    #region Intrinsic GRC Tests (SORA 2.5 Table 2)

    [Fact]
    public void CalculateIGRC_SmallUA_ControlledArea_ReturnsGRC1()
    {
        // Arrange: 1m UA, 25 m/s, Controlled Ground Area
        var input = new IntrinsicGRCInput
        {
            MaxCharacteristicDimension = 1.0,
            MaxSpeed = 25.0,
            PopulationDensity = 0,
            IsControlledGroundArea = true
        };

        // Act
        var result = _grcService.CalculateIntrinsicGRC_V2_5(input);

        // Assert
        Assert.Equal(1, result.IGRC);
        Assert.Equal(UADimensionCategory.Small, result.DimensionCategory);
        Assert.Equal(PopulationDensityCategory.ControlledGroundArea, result.PopulationCategory);
        Assert.False(result.IsOutOfScope);
    }

    [Fact]
    public void CalculateIGRC_MediumUA_Remote_ReturnsGRC3()
    {
        // Arrange: 3m UA, 35 m/s, <5 people/km²
        var input = new IntrinsicGRCInput
        {
            MaxCharacteristicDimension = 3.0,
            MaxSpeed = 35.0,
            PopulationDensity = 4.0,
            IsControlledGroundArea = false
        };

        // Act
        var result = _grcService.CalculateIntrinsicGRC_V2_5(input);

        // Assert
        Assert.Equal(3, result.IGRC);
        Assert.Equal(PopulationDensityCategory.Remote, result.PopulationCategory);
    }

    [Fact]
    public void CalculateIGRC_LargeUA_Suburban_ReturnsGRC7()
    {
        // Arrange: 8m UA, 75 m/s, <5000 people/km²
        var input = new IntrinsicGRCInput
        {
            MaxCharacteristicDimension = 8.0,
            MaxSpeed = 75.0,
            PopulationDensity = 4500.0,
            IsControlledGroundArea = false
        };

        // Act
        var result = _grcService.CalculateIntrinsicGRC_V2_5(input);

        // Assert
        Assert.Equal(7, result.IGRC);
    }

    [Fact]
    public void CalculateIGRC_VeryLargeUA_HighDensity_ReturnsGRC9()
    {
        // Arrange: 20m UA, 120 m/s, <50000 people/km²
        var input = new IntrinsicGRCInput
        {
            MaxCharacteristicDimension = 20.0,
            MaxSpeed = 120.0,
            PopulationDensity = 30000.0,
            IsControlledGroundArea = false
        };

        // Act
        var result = _grcService.CalculateIntrinsicGRC_V2_5(input);

        // Assert
        Assert.Equal(9, result.IGRC);
    }

    [Fact]
    public void CalculateIGRC_ExtraLargeUA_Assemblies_OutOfScope()
    {
        // Arrange: 40m+ UA, 200 m/s, >50000 people/km² → Out of SORA scope
        var input = new IntrinsicGRCInput
        {
            MaxCharacteristicDimension = 45.0,
            MaxSpeed = 200.0,
            PopulationDensity = 60000.0,
            IsControlledGroundArea = false
        };

        // Act
        var result = _grcService.CalculateIntrinsicGRC_V2_5(input);

        // Assert
    Assert.True(result.IsOutOfScope);
    Assert.Contains("out of scope", result.Notes, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CalculateIGRC_250gUA_25ms_AlwaysGRC1()
    {
        // Arrange: Special case ≤250g & ≤25m/s → always iGRC=1
        var input = new IntrinsicGRCInput
        {
            MaxCharacteristicDimension = 0.20, // 200g equivalent
            MaxSpeed = 20.0,
            PopulationDensity = 10000.0, // Even in dense area
            IsControlledGroundArea = false
        };

        // Act
        var result = _grcService.CalculateIntrinsicGRC_V2_5(input);

        // Assert
        Assert.Equal(1, result.IGRC);
        Assert.Contains("250g", result.Notes, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Final GRC with Mitigations Tests (SORA 2.5 Table 5)

    [Fact]
    public void CalculateFinalGRC_NoMitigations_ReturnsSameAsIntrinsic()
    {
        // Arrange
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 5,
            Mitigations = new List<AppliedMitigation>()
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(5, result.FinalGRC);
        Assert.Equal(0, result.TotalGRCReduction);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CalculateFinalGRC_M1A_Low_ReducesBy1()
    {
        // Arrange: M1(A) Sheltering - Low robustness → -1 GRC
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 4,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1A_Sheltering, Robustness = RobustnessLevel.Low }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(3, result.FinalGRC);
        Assert.Equal(1, result.TotalGRCReduction);
        Assert.Single(result.AppliedMitigations);
    }

    [Fact]
    public void CalculateFinalGRC_M1A_Medium_ReducesBy2()
    {
        // Arrange: M1(A) Sheltering - Medium robustness → -2 GRC
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 6,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1A_Sheltering, Robustness = RobustnessLevel.Medium }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(4, result.FinalGRC);
        Assert.Equal(2, result.TotalGRCReduction);
    }

    [Fact]
    public void CalculateFinalGRC_M1B_High_ReducesBy2()
    {
        // Arrange: M1(B) Operational restrictions - High robustness → -2 GRC
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 7,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1B_OperationalRestrictions, Robustness = RobustnessLevel.High }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(5, result.FinalGRC);
    }

    [Fact]
    public void CalculateFinalGRC_M2_High_ReducesBy2()
    {
        // Arrange: M2 Impact dynamics (parachute) - High robustness → -2 GRC
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 6,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M2_ImpactDynamics, Robustness = RobustnessLevel.High }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(4, result.FinalGRC);
    }

    [Fact]
    public void CalculateFinalGRC_MultipleMitigations_AppliesAll()
    {
        // Arrange: M1A(Low) + M1B(High) + M2(Medium) = -1 -2 -1 = -4
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 7,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1A_Sheltering, Robustness = RobustnessLevel.Low },
                new() { Type = GroundRiskMitigation.M1B_OperationalRestrictions, Robustness = RobustnessLevel.High },
                new() { Type = GroundRiskMitigation.M2_ImpactDynamics, Robustness = RobustnessLevel.Medium }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(3, result.FinalGRC); // 7 - 4 = 3
        Assert.Equal(4, result.TotalGRCReduction);
        Assert.Equal(3, result.AppliedMitigations.Count);
    }

    [Fact]
    public void CalculateFinalGRC_FinalGRCAbove7_Invalid()
    {
        // Arrange: Final GRC > 7 → out of SORA scope
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 9,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1A_Sheltering, Robustness = RobustnessLevel.Low } // -1 only
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(8, result.FinalGRC); // 9 - 1 = 8
    Assert.False(result.IsValid);
    Assert.Contains("out of SORA 2.5 scope", result.ValidationMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CalculateFinalGRC_CannotGoBelow1()
    {
        // Arrange: Cannot reduce below 1 (controlled ground area minimum)
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 2,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1A_Sheltering, Robustness = RobustnessLevel.Medium } // -2
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(1, result.FinalGRC); // Cannot go below 1
    }

    [Fact]
    public void CalculateFinalGRC_M1A_High_NoCredit()
    {
        // Arrange: M1(A) Sheltering - High robustness → 0 credit στην τρέχουσα υλοποίηση
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 5,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1A_Sheltering, Robustness = RobustnessLevel.High }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(5, result.FinalGRC);
        Assert.Equal(0, result.TotalGRCReduction);
        Assert.Single(result.AppliedMitigations);
    }

    [Fact]
    public void CalculateFinalGRC_M1C_Medium_NoCredit()
    {
        // Arrange: M1(C) Ground observation - Medium robustness → 0 credit
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 6,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1C_GroundObservation, Robustness = RobustnessLevel.Medium }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(6, result.FinalGRC);
        Assert.Equal(0, result.TotalGRCReduction);
        Assert.Single(result.AppliedMitigations);
    }

    [Fact]
    public void CalculateFinalGRC_M1B_Low_NoCredit()
    {
        // Arrange: M1(B) Operational restrictions - Low robustness → 0 credit
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 6,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1B_OperationalRestrictions, Robustness = RobustnessLevel.Low }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(6, result.FinalGRC);
        Assert.Equal(0, result.TotalGRCReduction);
        Assert.Single(result.AppliedMitigations);
    }

    [Fact]
    public void CalculateFinalGRC_M2_Medium_ReducesBy1()
    {
        // Arrange: M2 Impact dynamics - Medium robustness → -1
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 5,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M2_ImpactDynamics, Robustness = RobustnessLevel.Medium }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(4, result.FinalGRC);
        Assert.Equal(1, result.TotalGRCReduction);
        Assert.Single(result.AppliedMitigations);
    }

    [Fact]
    public void CalculateFinalGRC_M1C_High_NoCredit()
    {
        // Arrange: M1(C) Ground observation - High robustness → 0 credit
        var input = new FinalGRCInput
        {
            IntrinsicGRC = 5,
            Mitigations = new List<AppliedMitigation>
            {
                new() { Type = GroundRiskMitigation.M1C_GroundObservation, Robustness = RobustnessLevel.High }
            }
        };

        // Act
        var result = _grcService.CalculateFinalGRC_V2_5(input);

        // Assert
        Assert.Equal(5, result.FinalGRC);
        Assert.Equal(0, result.TotalGRCReduction);
        Assert.Single(result.AppliedMitigations);
    }

    #endregion

    #region Population Category Tests

    [Theory]
    [InlineData(0, true, PopulationDensityCategory.ControlledGroundArea)]
    [InlineData(3, false, PopulationDensityCategory.Remote)]
    [InlineData(25, false, PopulationDensityCategory.LightlyPopulated)]
    [InlineData(200, false, PopulationDensityCategory.SparselyPopulated)]
    [InlineData(2500, false, PopulationDensityCategory.Suburban)]
    [InlineData(30000, false, PopulationDensityCategory.HighDensityMetro)]
    [InlineData(80000, false, PopulationDensityCategory.AssembliesOfPeople)]
    public void GetPopulationCategory_ReturnsCorrectCategory(double density, bool controlled, PopulationDensityCategory expected)
    {
        // Act
        var result = _grcService.GetPopulationCategory(density, controlled);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region UA Dimension Category Tests

    [Theory]
    [InlineData(0.8, 20, UADimensionCategory.Small)]
    [InlineData(2.5, 30, UADimensionCategory.Medium)]
    [InlineData(7.0, 65, UADimensionCategory.Large)]
    [InlineData(15.0, 110, UADimensionCategory.VeryLarge)]
    [InlineData(50.0, 220, UADimensionCategory.ExtraLarge)]
    public void GetDimensionCategory_ReturnsCorrectCategory(double dimension, double speed, UADimensionCategory expected)
    {
        // Act
        var result = _grcService.GetDimensionCategory(dimension, speed);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
