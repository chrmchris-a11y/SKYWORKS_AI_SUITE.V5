using Xunit;
using Skyworks.Core.Services;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Api.Tests.OSO;

/// <summary>
/// Tests for OSO Service - JARUS SORA 2.0 Table 6 & 2.5 Table 14
/// Step 32: OSO Requirements Implementation
/// </summary>
public class OSOService_Tests
{
    private readonly IOSOService _osoService;

    public OSOService_Tests()
    {
        _osoService = new OSOService();
    }

    #region SORA 2.0 Table 6 - 24 OSOs Tests

    [Fact]
    public void SORA20_SAIL_I_Returns_24_OSOs()
    {
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.I, "2.0");

        // Assert
        Assert.Equal("2.0", result.SORAVersion);
        Assert.Equal(24, result.TotalCount);
        Assert.True(result.RequiredCount > 0);
    }

    [Fact]
    public void SORA20_SAIL_VI_Has_All_OSOs_Required()
    {
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.VI, "2.0");

        // Assert
        Assert.Equal(24, result.TotalCount);
        var allHigh = result.Requirements.Count(r => r.RobustnessLevel == OSORobustnessLevel.High);
        Assert.True(allHigh > 15); // Most should be High for SAIL VI
    }

    [Fact]
    public void SORA20_OSO01_Operator_Competence_Progressive_Robustness()
    {
        // Arrange: OSO#01 should be NR/L/M/H/H/H
        
        // Act
        var sail1 = _osoService.GetOSORequirements(SAILLevel.I, "2.0");
        var sail3 = _osoService.GetOSORequirements(SAILLevel.III, "2.0");
        var sail6 = _osoService.GetOSORequirements(SAILLevel.VI, "2.0");

        // Assert
        var oso1_sail1 = sail1.Requirements.First(r => r.OSOId == 1);
        var oso1_sail3 = sail3.Requirements.First(r => r.OSOId == 1);
        var oso1_sail6 = sail6.Requirements.First(r => r.OSOId == 1);

        Assert.Equal(OSORobustnessLevel.NotRequired, oso1_sail1.RobustnessLevel);
        Assert.Equal(OSORobustnessLevel.Medium, oso1_sail3.RobustnessLevel);
        Assert.Equal(OSORobustnessLevel.High, oso1_sail6.RobustnessLevel);
    }

    #endregion

    #region SORA 2.5 Table 14 - 17 OSOs Tests

    [Fact]
    public void SORA25_SAIL_I_Returns_17_OSOs()
    {
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.I, "2.5");

        // Assert
        Assert.Equal("2.5", result.SORAVersion);
        Assert.Equal(17, result.TotalCount); // 7 OSOs removed in 2.5
        Assert.True(result.RequiredCount >= 0);
    }

    [Fact]
    public void SORA25_Missing_OSOs_Not_Included()
    {
        // Arrange: OSOs #10, #11, #12, #14, #15, #21, #22 removed in SORA 2.5
        var removedOSOs = new[] { 10, 11, 12, 14, 15, 21, 22 };

        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.III, "2.5");

        // Assert
        foreach (var osoId in removedOSOs)
        {
            Assert.DoesNotContain(result.Requirements, r => r.OSOId == osoId);
        }
    }

    [Fact]
    public void SORA25_GetOSODetails_RemovedOSO_Returns_Null()
    {
        // Arrange: OSO#10 removed in SORA 2.5
        
        // Act
        var details = _osoService.GetOSODetails(10, "2.5");

        // Assert
        Assert.Null(details); // Should not exist in 2.5
    }

    [Fact]
    public void SORA25_Uses_NotRequired_Instead_Of_Optional()
    {
        // Arrange: SORA 2.5 uses NR instead of O
        
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.I, "2.5");

        // Assert
        var notRequiredCount = result.Requirements.Count(r => r.RobustnessLevel == OSORobustnessLevel.NotRequired);
        var optionalCount = result.Requirements.Count(r => r.RobustnessLevel == OSORobustnessLevel.Optional);
        
        Assert.True(notRequiredCount > 0);
        Assert.Equal(0, optionalCount); // No Optional in SORA 2.5
    }

    #endregion

    #region Version Comparison Tests

    [Fact]
    public void SORA20_vs_SORA25_SAIL_III_Comparison()
    {
        // Act
        var sora20 = _osoService.GetOSORequirements(SAILLevel.III, "2.0");
        var sora25 = _osoService.GetOSORequirements(SAILLevel.III, "2.5");

        // Assert
        Assert.Equal(24, sora20.TotalCount);
        Assert.Equal(17, sora25.TotalCount);
        Assert.Equal(7, sora20.TotalCount - sora25.TotalCount); // 7 OSOs removed
    }

    [Fact]
    public void Common_OSOs_Have_Same_Robustness_Across_Versions()
    {
        // Arrange: OSO#01, #03, #07, #08, #09 should be same in both versions
        var commonOSOs = new[] { 1, 3, 7, 8, 9 };

        // Act
        var sora20 = _osoService.GetOSORequirements(SAILLevel.IV, "2.0");
        var sora25 = _osoService.GetOSORequirements(SAILLevel.IV, "2.5");

        // Assert
        foreach (var osoId in commonOSOs)
        {
            var oso20 = sora20.Requirements.First(r => r.OSOId == osoId);
            var oso25 = sora25.Requirements.First(r => r.OSOId == osoId);
            
            // Robustness should be identical (accounting for O→NR change)
            if (oso20.RobustnessLevel == OSORobustnessLevel.Optional)
                Assert.Equal(OSORobustnessLevel.NotRequired, oso25.RobustnessLevel);
            else
                Assert.Equal(oso20.RobustnessLevel, oso25.RobustnessLevel);
        }
    }

    #endregion

    #region OSO Details Tests

    [Fact]
    public void GetOSODetails_ValidOSO_Returns_Full_Info()
    {
        // Act
        var details = _osoService.GetOSODetails(1, "2.5");

        // Assert
        Assert.NotNull(details);
        Assert.Equal(1, details.OSOId);
        Assert.Contains("Operator", details.Name);
        Assert.NotEmpty(details.Dependencies);
        Assert.Contains("E.2", details.AnnexESection);
    }

    [Fact]
    public void GetOSODetails_InvalidOSO_Returns_Null()
    {
        // Act
        var details = _osoService.GetOSODetails(99, "2.5");

        // Assert
        Assert.Null(details);
    }

    [Fact]
    public void GetOSODetails_SORA20_OSO10_Exists()
    {
        // Act
        var details = _osoService.GetOSODetails(10, "2.0");

        // Assert
        Assert.NotNull(details); // Should exist in 2.0
        Assert.Contains("technical issue", details.Name.ToLower());
    }

    #endregion

    #region OSO Compliance Validation Tests

    [Fact]
    public void ValidateCompliance_AllOSOs_Met_Returns_Compliant()
    {
        // Arrange: SAIL III with all required OSOs implemented
        var implementedOSOs = new List<OSOImplementation>
        {
            new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 2, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 3, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 4, AchievedRobustness = OSORobustnessLevel.NotRequired },
            new() { OSOId = 5, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 6, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 7, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 8, AchievedRobustness = OSORobustnessLevel.High },
            new() { OSOId = 9, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 13, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 16, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 17, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 18, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 19, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 20, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 23, AchievedRobustness = OSORobustnessLevel.Medium },
            new() { OSOId = 24, AchievedRobustness = OSORobustnessLevel.Medium }
        };

        // Act
        var result = _osoService.ValidateOSOCompliance(SAILLevel.III, implementedOSOs, "2.5");

        // Assert
        Assert.True(result.IsCompliant);
        Assert.Empty(result.MissingOSOs);
        Assert.Empty(result.InsufficientRobustness);
        Assert.Contains("✅", result.Summary);
    }

    [Fact]
    public void ValidateCompliance_MissingOSO_Returns_NonCompliant()
    {
        // Arrange: SAIL II with several OSOs missing (#06, #07, #08, #09)
        var implementedOSOs = new List<OSOImplementation>
        {
            new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 3, AchievedRobustness = OSORobustnessLevel.Low }
            // Required for SAIL II but missing: OSO#06, #07, #08, #09
        };

        // Act
        var result = _osoService.ValidateOSOCompliance(SAILLevel.II, implementedOSOs, "2.5");

        // Assert
        Assert.False(result.IsCompliant);
        Assert.NotEmpty(result.MissingOSOs);
        Assert.True(result.MissingOSOs.Count >= 4); // At least 4 missing
        Assert.Contains("OSO#06", result.MissingOSOs[0]); // First missing is OSO#06
    }

    [Fact]
    public void ValidateCompliance_InsufficientRobustness_Returns_NonCompliant()
    {
        // Arrange: SAIL IV requires OSO#01 at High, but only Medium provided
        var implementedOSOs = new List<OSOImplementation>
        {
            new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.Medium } // Should be High
        };

        // Act
        var result = _osoService.ValidateOSOCompliance(SAILLevel.IV, implementedOSOs, "2.5");

        // Assert
        Assert.False(result.IsCompliant);
        Assert.NotEmpty(result.InsufficientRobustness);
        Assert.Contains("OSO#01", result.InsufficientRobustness[0]);
        Assert.Contains("Required High", result.InsufficientRobustness[0]);
    }

    [Fact]
    public void ValidateCompliance_Optional_OSOs_Not_Required()
    {
        // Arrange: SAIL I - most OSOs are NotRequired/Optional
        var implementedOSOs = new List<OSOImplementation>
        {
            new() { OSOId = 3, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 7, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 8, AchievedRobustness = OSORobustnessLevel.Low },
            new() { OSOId = 9, AchievedRobustness = OSORobustnessLevel.Low }
            // Not providing Optional/NR OSOs
        };

        // Act
        var result = _osoService.ValidateOSOCompliance(SAILLevel.I, implementedOSOs, "2.5");

        // Assert - should be compliant even without Optional OSOs
        Assert.True(result.IsCompliant || result.MissingOSOs.Count < 5); // Most should be optional
    }

    #endregion

    #region OSO Categories Tests

    [Fact]
    public void OSO_Categories_Are_Properly_Assigned()
    {
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.III, "2.5");

        // Assert
        Assert.Contains(result.Requirements, r => r.Category == OSOCategory.Organizational);
        Assert.Contains(result.Requirements, r => r.Category == OSOCategory.Design);
        Assert.Contains(result.Requirements, r => r.Category == OSOCategory.Operational);
        Assert.Contains(result.Requirements, r => r.Category == OSOCategory.Personnel);
    }

    [Fact]
    public void OSO_Dependencies_Include_Operator_Designer_Training()
    {
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.IV, "2.5");

        // Assert
        var operatorOSOs = result.Requirements.Where(r => r.Dependencies.Contains("Operator"));
        var designerOSOs = result.Requirements.Where(r => r.Dependencies.Contains("Designer"));
        
        Assert.NotEmpty(operatorOSOs);
        Assert.NotEmpty(designerOSOs);
    }

    #endregion

    #region Edge Cases & Validation

    [Fact]
    public void GetOSORequirements_InvalidVersion_Defaults_To_25()
    {
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.III, "3.0"); // Invalid version

        // Assert - should default to 2.5
        Assert.Equal(17, result.TotalCount); // SORA 2.5 count
    }

    [Fact]
    public void All_SAIL_Levels_Return_Requirements()
    {
        // Act & Assert
        foreach (SAILLevel sail in Enum.GetValues(typeof(SAILLevel)))
        {
            var result = _osoService.GetOSORequirements(sail, "2.5");
            Assert.True(result.TotalCount > 0);
            Assert.NotNull(result.Summary);
        }
    }

    [Fact]
    public void SORA25_Annex_E_References_Present()
    {
        // Act
        var details = _osoService.GetOSODetails(5, "2.5");

        // Assert
        Assert.NotNull(details);
        Assert.Contains("E.2", details.AnnexESection);
    }

    #endregion

    #region Golden Tests - Cyprus Operations

    [Fact]
    public void Golden_Test_Nicosia_Urban_SAIL_V_OSO_Requirements()
    {
        // Arrange: Urban VLOS Nicosia - SAIL V
        
        // Act
        var result = _osoService.GetOSORequirements(SAILLevel.V, "2.5");

        // Assert
        Assert.Equal(17, result.TotalCount);
        var highRobustness = result.Requirements.Count(r => r.RobustnessLevel == OSORobustnessLevel.High);
        Assert.True(highRobustness >= 10); // SAIL V requires many High robustness OSOs
        Assert.Contains("SAIL V", result.Summary);
    }

    [Fact]
    public void Golden_Test_Limassol_BVLOS_SAIL_II_Comparison()
    {
        // Arrange: Suburban BVLOS Limassol - SAIL II
        
        // Act
        var sora20 = _osoService.GetOSORequirements(SAILLevel.II, "2.0");
        var sora25 = _osoService.GetOSORequirements(SAILLevel.II, "2.5");

        // Assert - operator must choose compliance path
        Assert.Equal(24, sora20.TotalCount);
        Assert.Equal(17, sora25.TotalCount);
        
        // Required count should be similar (removed OSOs were redundant/consolidated)
        // SORA 2.0 SAIL II: 7 removed OSOs (#10,#11,#12,#14,#15,#21,#22) had robustness requirements
        // SORA 2.5 consolidated them into remaining OSOs (e.g., #10→#05, #14/#21→#08)
        var diff = Math.Abs(sora20.RequiredCount - sora25.RequiredCount);
        Assert.True(diff <= 7, $"SORA 2.0 RequiredCount: {sora20.RequiredCount}, SORA 2.5 RequiredCount: {sora25.RequiredCount}, Diff: {diff}");
    }

    #endregion
}
