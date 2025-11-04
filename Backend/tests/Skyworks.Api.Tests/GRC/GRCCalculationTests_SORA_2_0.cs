using Xunit;
using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Services.GRC;

namespace Skyworks.Api.Tests.GRC;

/// <summary>
/// Comprehensive test suite for SORA 2.0 AMC GRC calculations
/// Validates Table 2 (7 scenarios × 4 dimensions) and Table 3 (M1/M2/M3 mitigations)
/// </summary>
public class GRCCalculationTests_SORA_2_0
{
    private readonly IGRCCalculationService _grcService;

    public GRCCalculationTests_SORA_2_0()
    {
        _grcService = new GRCCalculationService();
    }

    #region SORA 2.0 Table 2 Tests

    [Theory]
    [InlineData(OperationalScenario.ControlledGroundArea, 0.8, 1)] // 1m column
    [InlineData(OperationalScenario.ControlledGroundArea, 2.5, 2)] // 3m column
    [InlineData(OperationalScenario.ControlledGroundArea, 7.0, 3)] // 8m column
    [InlineData(OperationalScenario.ControlledGroundArea, 10.0, 4)] // >8m column
    public void SORA20_ControlledArea_ReturnsCorrectGRC(OperationalScenario scenario, double dimension, int expectedGRC)
    {
        var input = new IntrinsicGRCInput_V2_0
        {
            MaxCharacteristicDimension = dimension,
            Scenario = scenario
        };

        var result = _grcService.CalculateIntrinsicGRC_V2_0(input);

        Assert.Equal(expectedGRC, result.IGRC);
        Assert.False(result.IsOutOfScope);
    }

    [Fact]
    public void SORA20_VLOS_SparselyPopulated_3m_ReturnsGRC3()
    {
        var input = new IntrinsicGRCInput_V2_0
        {
            MaxCharacteristicDimension = 3.0,
            Scenario = OperationalScenario.VLOS_SparselyPopulated
        };

        var result = _grcService.CalculateIntrinsicGRC_V2_0(input);

        Assert.Equal(3, result.IGRC);
        Assert.Equal(UASDimensionCategory_V2_0.Medium_3m, result.DimensionCategory);
    }

    [Fact]
    public void SORA20_BVLOS_Populated_Above8m_ReturnsGRC10()
    {
        var input = new IntrinsicGRCInput_V2_0
        {
            MaxCharacteristicDimension = 12.0,
            Scenario = OperationalScenario.BVLOS_Populated
        };

        var result = _grcService.CalculateIntrinsicGRC_V2_0(input);

        Assert.Equal(10, result.IGRC);
    }

    [Fact]
    public void SORA20_VLOS_Gathering_1m_ReturnsGRC7()
    {
        var input = new IntrinsicGRCInput_V2_0
        {
            MaxCharacteristicDimension = 0.9,
            Scenario = OperationalScenario.VLOS_GatheringOfPeople
        };

        var result = _grcService.CalculateIntrinsicGRC_V2_0(input);

        Assert.Equal(7, result.IGRC);
    }

    [Fact]
    public void SORA20_VLOS_Gathering_3m_OutOfScope()
    {
        // Grey cell: VLOS gathering with 3m UA not supported
        var input = new IntrinsicGRCInput_V2_0
        {
            MaxCharacteristicDimension = 3.0,
            Scenario = OperationalScenario.VLOS_GatheringOfPeople
        };

        var result = _grcService.CalculateIntrinsicGRC_V2_0(input);

        Assert.True(result.IsOutOfScope);
    }

    #endregion

    #region SORA 2.0 Mitigations Tests

    [Fact]
    public void SORA20_M1_High_Reduces4GRC()
    {
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 6,
            ColumnMinimumGRC = 2, // 3m column minimum
            Mitigations = new List<AppliedMitigation_V2_0>
            {
                new() { Type = GroundRiskMitigation_V2_0.M1_StrategicMitigations, Robustness = RobustnessLevel.High }
            }
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.Equal(2, result.FinalGRC); // 6 - 4 (M1 High) = 2 (no implicit M3 penalty)
        Assert.Equal(4, result.TotalGRCReduction); // 4 points reduction from M1 High
    }

    [Fact]
    public void SORA20_M1_CannotGoBelowColumnMinimum()
    {
        // M1 cannot reduce below column minimum (e.g., 2 for 3m column)
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 3,
            ColumnMinimumGRC = 2,
            Mitigations = new List<AppliedMitigation_V2_0>
            {
                new() { Type = GroundRiskMitigation_V2_0.M1_StrategicMitigations, Robustness = RobustnessLevel.High } // Would be -4
            }
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.Equal(2, result.FinalGRC); // Stops at column minimum
    }

    [Fact]
    public void SORA20_M2_High_Reduces2GRC()
    {
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 5,
            ColumnMinimumGRC = 1,
            Mitigations = new List<AppliedMitigation_V2_0>
            {
                new() { Type = GroundRiskMitigation_V2_0.M2_ImpactReduction, Robustness = RobustnessLevel.High }
            }
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.Equal(3, result.FinalGRC); // 5 - 2 (M2 High) = 3 (no implicit M3 penalty)
    }

    [Fact]
    public void SORA20_M3_Low_Adds1GRC()
    {
        // M3 with Low robustness adds +1 penalty
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 4,
            ColumnMinimumGRC = 1,
            Mitigations = new List<AppliedMitigation_V2_0>
            {
                new() { Type = GroundRiskMitigation_V2_0.M3_EmergencyResponsePlan, Robustness = RobustnessLevel.Low }
            }
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.Equal(5, result.FinalGRC); // 4 + 1 = 5
    }

    [Fact]
    public void SORA20_M3_High_Reduces1GRC()
    {
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 5,
            ColumnMinimumGRC = 1,
            Mitigations = new List<AppliedMitigation_V2_0>
            {
                new() { Type = GroundRiskMitigation_V2_0.M3_EmergencyResponsePlan, Robustness = RobustnessLevel.High }
            }
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.Equal(4, result.FinalGRC); // 5 - 1 = 4
    }

    [Fact]
    public void SORA20_CombinedMitigations_AppliesInSequence()
    {
        // M1(Med) + M2(High) + M3(Med) = -2 -2 +0 = -4
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 8,
            ColumnMinimumGRC = 1,
            Mitigations = new List<AppliedMitigation_V2_0>
            {
                new() { Type = GroundRiskMitigation_V2_0.M1_StrategicMitigations, Robustness = RobustnessLevel.Medium },
                new() { Type = GroundRiskMitigation_V2_0.M2_ImpactReduction, Robustness = RobustnessLevel.High },
                new() { Type = GroundRiskMitigation_V2_0.M3_EmergencyResponsePlan, Robustness = RobustnessLevel.Medium }
            }
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.Equal(4, result.FinalGRC); // 8 - 4 = 4
        Assert.Equal(3, result.AppliedMitigations.Count);
    }

    [Fact]
    public void SORA20_FinalGRC_Above7_Invalid()
    {
        var input = new FinalGRCInput_V2_0
        {
            IntrinsicGRC = 9,
            ColumnMinimumGRC = 5,
            Mitigations = new List<AppliedMitigation_V2_0>()
        };

        var result = _grcService.CalculateFinalGRC_V2_0(input);

        Assert.False(result.IsValid);
        Assert.Contains("not supported", result.ValidationMessage);
    }

    #endregion
}
