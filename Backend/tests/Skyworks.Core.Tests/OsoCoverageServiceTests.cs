using Xunit;
using Skyworks.Core.Models;
using Skyworks.Core.Services;
using System.Text.Json;

namespace Skyworks.Core.Tests;

/// <summary>
/// Unit tests for OsoCoverageService - OSO checklist generation per SAIL.
/// Tests verify progressive OSO requirements, mission-based coverage, and missing OSO detection.
/// </summary>
public class OsoCoverageServiceTests
{
    private readonly IOsoCatalog _osoCatalog;
    private readonly IOsoCoverageService _osoCoverageService;

    public OsoCoverageServiceTests()
    {
        _osoCatalog = new OsoCatalog();
        _osoCoverageService = new OsoCoverageService(_osoCatalog);
    }

    #region Test 1: Progressive SAIL Requirements

    [Fact]
    public void BuildChecklist_SailII_ShouldHaveFewerRequiredOsosThanSailV()
    {
        // Arrange
        var mission = CreateMission(MissionCategory.AgricultureAndForestry, MissionType.Agriculture);
        var erpSailII = CreateErp(false); // Basic ERP
        var erpSailV = CreateErp(false);
        var soraSailII = CreateSora("II");
        var soraSailV = CreateSora("V");

        // Act
        var resultSailII = _osoCoverageService.BuildChecklist(mission, erpSailII, soraSailII);
        var resultSailV = _osoCoverageService.BuildChecklist(mission, erpSailV, soraSailV);

        var requiredOsosSailII = JsonSerializer.Deserialize<List<OsoItem>>(resultSailII.RequiredOsosJson);
        var requiredOsosSailV = JsonSerializer.Deserialize<List<OsoItem>>(resultSailV.RequiredOsosJson);

        // Assert
        Assert.NotNull(requiredOsosSailII);
        Assert.NotNull(requiredOsosSailV);
        
        // SAIL II should have 11 OSOs, SAIL V should have 23
        Assert.Equal(11, requiredOsosSailII.Count);
        Assert.Equal(23, requiredOsosSailV.Count);

        // SAIL II OSOs should be subset of SAIL V
        var sailIICodes = requiredOsosSailII.Select(o => o.Code).ToHashSet();
        var sailVCodes = requiredOsosSailV.Select(o => o.Code).ToHashSet();
        Assert.True(sailIICodes.IsSubsetOf(sailVCodes));
    }

    #endregion

    #region Test 2: Minimal Coverage for Simple Mission

    [Fact]
    public void BuildChecklist_AgriculturalFieldMapping_SailII_Rural_ShouldHaveMinimalCovered()
    {
        // Arrange: Simple agricultural mission, SAIL II, basic ERP
        var mission = CreateMission(MissionCategory.AgricultureAndForestry, MissionType.Agriculture);
        var erp = CreateErp(completeErp: false); // Minimal ERP
        var sora = CreateSora("II");

        // Act
        var result = _osoCoverageService.BuildChecklist(mission, erp, sora);
        var requiredOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.RequiredOsosJson);
        var coveredOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.CoveredOsosJson);
        var missingOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.MissingOsosJson);

        // Assert
        Assert.NotNull(requiredOsos);
        Assert.NotNull(coveredOsos);
        Assert.NotNull(missingOsos);

        // SAIL II requires 11 OSOs
        Assert.Equal(11, requiredOsos.Count);

        // For minimal agricultural mission with basic ERP, expect reasonable coverage
        // Covered: OSO#01, #04, #05, #06, #09, #10, #23 (7 OSOs)
        Assert.InRange(coveredOsos.Count, 6, 8);

        // Should have missing OSOs (most of the required 11)
        Assert.InRange(missingOsos.Count, 3, 5);

        // Missing + Covered should equal Required
        Assert.Equal(requiredOsos.Count, coveredOsos.Count + missingOsos.Count);
    }

    #endregion

    #region Test 3: High-SAIL Mission with Significant Gaps

    [Fact]
    public void BuildChecklist_PhotovoltaicParkInspection_SailV_ShouldHaveSignificantMissing()
    {
        // Arrange: Complex PV inspection, SAIL V, basic ERP
        var mission = CreateMission(MissionCategory.EnergyAndUtilities, MissionType.Solar);
        var erp = CreateErp(completeErp: false); // Basic ERP without full emergency procedures
        var sora = CreateSora("V");

        // Act
        var result = _osoCoverageService.BuildChecklist(mission, erp, sora);
        var requiredOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.RequiredOsosJson);
        var coveredOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.CoveredOsosJson);
        var missingOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.MissingOsosJson);

        // Assert
        Assert.NotNull(requiredOsos);
        Assert.NotNull(coveredOsos);
        Assert.NotNull(missingOsos);

        // SAIL V requires 23 OSOs
        Assert.Equal(23, requiredOsos.Count);

        // With incomplete ERP and complex mission, coverage should be limited
        Assert.InRange(coveredOsos.Count, 4, 10);

        // Should have many missing OSOs (SAIL V is demanding)
        Assert.InRange(missingOsos.Count, 13, 19);

        // Verify required = covered + missing
        Assert.Equal(requiredOsos.Count, coveredOsos.Count + missingOsos.Count);

        // Check that OSO#22 (ERP) is in missing (since ERP is incomplete)
        Assert.Contains(missingOsos, o => o.Code == "OSO#22");
    }

    #endregion

    #region Test 4: Complete ERP Covers Emergency OSOs

    [Fact]
    public void BuildChecklist_ErpWithFullEmergencyPlan_ShouldCoverEmergencyRelatedOsos()
    {
        // Arrange: Training mission, SAIL IV (includes autonomous/emergency OSOs), complete ERP
        var mission = CreateMission(MissionCategory.TrainingAndTest, MissionType.Agriculture);
        var erp = CreateErp(completeErp: true); // Full ERP with all sections
        var sora = CreateSora("IV"); // SAIL IV includes OSO#07, OSO#08, OSO#21, OSO#22

        // Act
        var result = _osoCoverageService.BuildChecklist(mission, erp, sora);
        var coveredOsos = JsonSerializer.Deserialize<List<OsoItem>>(result.CoveredOsosJson);

        // Assert
        Assert.NotNull(coveredOsos);

        // Should cover emergency-related OSOs
        var coveredCodes = coveredOsos.Select(o => o.Code).ToHashSet();
        
        // OSO#21 (Abnormal/emergency procedures)
        Assert.Contains("OSO#21", coveredCodes);
        
        // OSO#22 (Emergency Response Plan)
        Assert.Contains("OSO#22", coveredCodes);

        // OSO#07 (Autonomous operations) - covered if ERP handles C2 loss
        Assert.Contains("OSO#07", coveredCodes);

        // OSO#08 (System deterioration) - covered if ERP addresses fly-away
        Assert.Contains("OSO#08", coveredCodes);

        // Training mission should also cover OSO#02, OSO#03
        Assert.Contains("OSO#02", coveredCodes);
        Assert.Contains("OSO#03", coveredCodes);
    }

    #endregion

    #region Helper Methods

    private Mission CreateMission(MissionCategory category, MissionType type)
    {
        return new Mission
        {
            MissionId = Guid.NewGuid(),
            Name = "Test Mission",
            Category = category,
            Type = type,
            TemplateCode = MissionTemplateCode.AgriculturalFieldMapping
        };
    }

    private MissionErp CreateErp(bool completeErp)
    {
        var erpJson = completeErp ? @"{
""lossOfC2"": { ""rthAltitude_m"": 60, ""maxTime_s"": 15, ""actions"": [""RTH"", ""Monitor""] },
""flyAway"": { ""preventative"": [""Geo-fencing""] },
""emergencyLanding"": { ""sites"": [{ ""lat"": 41.0, ""lon"": 25.0 }] },
""groundNotification"": { ""required"": true },
""atsCoordination"": { ""required"": true }
}" : @"{ ""lossOfC2"": { ""actions"": [] } }";

        return new MissionErp
        {
            MissionErpId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            ErpJson = erpJson
        };
    }

    private MissionSoraAssessment CreateSora(string sail)
    {
        return new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            SoraVersion = "2.5",
            Sail = sail,
            InputsSnapshotJson = "{\"kinetic\":5}"
        };
    }

    #endregion
}
