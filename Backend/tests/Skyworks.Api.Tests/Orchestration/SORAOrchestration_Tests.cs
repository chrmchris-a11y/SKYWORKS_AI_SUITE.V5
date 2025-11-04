using Xunit;
using Skyworks.Core.Services.Orchestration;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services;
using Skyworks.Core.Services.Risk;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Api.Tests.Orchestration;

public class SORAOrchestration_Tests
{
    private readonly ISORAOrchestrationService _orchestration;

    public SORAOrchestration_Tests()
    {
        var grcService = new GRCCalculationService();
        var sailService = new SAILService();
        var osoService = new OSOService();
        var riskService = new RiskModelingService();
        
        _orchestration = new SORAOrchestrationService(grcService, sailService, osoService, riskService);
    }

    [Fact]
    public void SORA_2_5_Complete_Low_Risk_Scenario()
    {
        var request = new SORACompleteRequest
        {
            SoraVersion = "2.5",
            MissionId = "TEST-001",
            MissionName = "Low Risk VLOS Mission",
            GroundRisk = new GroundRiskInput
            {
                PopulationDensity = 10, // <50 people/km²
                IsControlledGroundArea = false,
                MaxCharacteristicDimension = 0.5, // 500mm drone
                MaxSpeed = 20, // 20 m/s
                Mitigations = new List<MitigationInput>
                {
                    new() { Type = "M1A", Robustness = RobustnessLevel.Medium }
                }
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = ARCRating.ARC_a,
                StrategicMitigations = new List<string>()
            },
            ImplementedOSOs = new List<OSOImplementation>
            {
                new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.Medium },
                new() { OSOId = 2, AchievedRobustness = OSORobustnessLevel.Medium }
            }
        };

        var result = _orchestration.ExecuteComplete(request);

        Assert.True(result.IsSuccessful);
        Assert.Equal("2.5", result.SoraVersion);
        Assert.NotNull(result.IntrinsicGRC);
        Assert.NotNull(result.FinalGRC);
    Assert.True(result.FinalGRC <= 3); // Low GRC expected
    // Annex C: ARC-a is not an initial state; low-risk residual without segregation remains ARC-b
    Assert.Equal(ARCRating.ARC_b, result.ResidualARC);
        Assert.NotNull(result.SAIL);
        Assert.True(result.SAIL <= SAILLevel.II); // Low SAIL expected
        Assert.NotNull(result.TMPR);
        Assert.NotNull(result.RiskScore);
        Assert.Contains("Low", result.RiskBand ?? "");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void SORA_2_0_Complete_Medium_Risk_Scenario()
    {
        var request = new SORACompleteRequest
        {
            SoraVersion = "2.0",
            MissionId = "TEST-002",
            MissionName = "Medium Risk BVLOS Mission",
            GroundRisk = new GroundRiskInput
            {
                Scenario_V2_0 = OperationalScenario.BVLOS_SparselyPopulated,
                MaxCharacteristicDimension = 2.5, // 2.5m drone
                KineticEnergy = null,
                Mitigations = new List<MitigationInput>
                {
                    new() { Type = "M1", Robustness = RobustnessLevel.Medium },
                    new() { Type = "M2", Robustness = RobustnessLevel.Medium }
                }
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = ARCRating.ARC_c,
                StrategicMitigations = new List<string> { "S1", "S2" }
            },
            ImplementedOSOs = new List<OSOImplementation>
            {
                new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.Medium },
                new() { OSOId = 2, AchievedRobustness = OSORobustnessLevel.Medium },
                new() { OSOId = 3, AchievedRobustness = OSORobustnessLevel.Medium },
                new() { OSOId = 4, AchievedRobustness = OSORobustnessLevel.High }
            }
        };

        var result = _orchestration.ExecuteComplete(request);

        Assert.True(result.IsSuccessful);
        Assert.Equal("2.0", result.SoraVersion);
        Assert.NotNull(result.FinalGRC);
        Assert.True(result.FinalGRC.Value >= 1); // GRC reduced by mitigations
    Assert.Equal(ARCRating.ARC_c, result.InitialARC);
    Assert.Equal(ARCRating.ARC_b, result.ResidualARC); // C → B; ARC-a reserved for atypical/segregated
        Assert.NotNull(result.SAIL);
        Assert.NotNull(result.TMPR);
    Assert.Equal("Medium", result.TMPR); // ARC-b → TMPR Medium (updated mapping)
        Assert.True(result.RequiredOSOCount > 0);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void SORA_2_5_High_Risk_OutOfScope()
    {
        // Test case: Very high-risk scenario that exceeds SORA 2.5 scope (GRC > 7)
        var request = new SORACompleteRequest
        {
            SoraVersion = "2.5",
            MissionId = "TEST-003",
            MissionName = "High Risk Urban BVLOS",
            GroundRisk = new GroundRiskInput
            {
                PopulationDensity = 8000, // High density urban
                IsControlledGroundArea = false,
                MaxCharacteristicDimension = 10, // Large drone
                MaxSpeed = 80,
                Mitigations = new List<MitigationInput>()
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = ARCRating.ARC_d,
                StrategicMitigations = new List<string>()
            },
            ImplementedOSOs = new List<OSOImplementation>() // No OSOs
        };

        var result = _orchestration.ExecuteComplete(request);

        // Very high risk scenarios may exceed SORA scope → IsSuccessful = false
        Assert.False(result.IsSuccessful, "Expected out-of-scope scenario to fail");
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("out of SORA") || e.Contains("scope"));
        
        // iGRC should be calculated, but FinalGRC may be null if out of scope
        Assert.NotNull(result.IntrinsicGRC);
        Assert.True(result.IntrinsicGRC > 7, $"Expected iGRC > 7 (out of SORA 2.5 scope), got {result.IntrinsicGRC}");
    }

    [Fact]
    public void SORA_2_0_Invalid_Scenario_Returns_Error()
    {
        var request = new SORACompleteRequest
        {
            SoraVersion = "2.0",
            GroundRisk = new GroundRiskInput
            {
                // Missing Scenario_V2_0
                MaxCharacteristicDimension = 1.5
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = ARCRating.ARC_b
            }
        };

        var result = _orchestration.ExecuteComplete(request);

        Assert.False(result.IsSuccessful);
        Assert.True(result.Errors.Count > 0);
        Assert.Contains(result.Errors, e => e.Contains("Scenario_V2_0"));
    }

    [Fact]
    public void Summary_Contains_All_Key_Elements()
    {
        var request = new SORACompleteRequest
        {
            SoraVersion = "2.5",
            MissionId = "TEST-004",
            GroundRisk = new GroundRiskInput
            {
                PopulationDensity = 100,
                IsControlledGroundArea = false,
                MaxCharacteristicDimension = 1.2,
                MaxSpeed = 30
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = ARCRating.ARC_b
            },
            ImplementedOSOs = new List<OSOImplementation>
            {
                new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.High }
            }
        };

        var result = _orchestration.ExecuteComplete(request);

        Assert.True(result.IsSuccessful);
        Assert.NotEmpty(result.Summary);
        Assert.Contains("GRC", result.Summary);
        Assert.Contains("ARC", result.Summary);
        Assert.Contains("SAIL", result.Summary);
        Assert.Contains("TMPR", result.Summary);
        Assert.Contains("OSO", result.Summary);
        Assert.Contains("Compliant", result.Summary);
        Assert.Contains("Risk", result.Summary);
    }
}
