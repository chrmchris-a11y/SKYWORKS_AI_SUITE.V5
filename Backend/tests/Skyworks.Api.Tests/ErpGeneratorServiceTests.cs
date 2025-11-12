using System;
using System.Text.Json;
using Xunit;
using Skyworks.Core.Models;
using Skyworks.Core.Services;

namespace Skyworks.Api.Tests;

public class ErpGeneratorServiceTests
{
    private readonly MissionTemplatesConfig _templatesConfig = new();
    private readonly ErpGeneratorService _service;

    public ErpGeneratorServiceTests()
    {
        _service = new ErpGeneratorService(_templatesConfig);
    }

    [Fact]
    public void Generate_SailI_RuralFacadeCleaning_ShouldHaveLightErp()
    {
        // Arrange - Use AgriculturalFieldMapping which is truly rural
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.AgriculturalFieldMapping,
            Category = MissionCategory.AgricultureAndForestry,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 40m,
            RouteLength_m = 1000m,
            CgaArea_m2 = 20000m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7250,37.9850],[23.7350,37.9850],[23.7350,37.9750],[23.7250,37.9750],[23.7250,37.9850]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 2,
            FinalGrc = 1,
            InitialArc = "a",
            ResidualArc = "a",
            Sail = "I" // Low SAIL
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.ErpJson);
        Assert.NotEmpty(result.ErpText);

        // Verify JSON is valid
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // Light ERP for rural SAIL I
        Assert.False(erpPlan.GroundNotification.Required); // Should be optional for rural + SAIL I
        Assert.False(erpPlan.AtsCoordination.Required); // Not required for low SAIL

        // Should have basic C2 procedures
        Assert.True(erpPlan.LossOfC2.RthAltitude_m > 0);
        Assert.True(erpPlan.LossOfC2.MaxTimeToRth_s > 0);
        Assert.NotEmpty(erpPlan.LossOfC2.Actions);
    }

    [Fact]
    public void Generate_SailIV_CTR_BridgeInspection_ShouldRequireAts()
    {
        // Arrange
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.BridgeStructuralInspection,
            Category = MissionCategory.InfrastructureAndTransport,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 60m,
            RouteLength_m = 300m,
            CgaArea_m2 = 2000m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7275,37.9838],[23.7295,37.9838],[23.7295,37.9818],[23.7275,37.9818],[23.7275,37.9838]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 5,
            FinalGrc = 4,
            InitialArc = "c",
            ResidualArc = "b",
            Sail = "IV" // High SAIL
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // ATS coordination should be required for infrastructure + high SAIL
        Assert.True(erpPlan.AtsCoordination.Required);
        Assert.NotEmpty(erpPlan.AtsCoordination.Contacts);
        Assert.Contains("ATS", erpPlan.AtsCoordination.Contacts[0]);

        // Ground notification should be required
        Assert.True(erpPlan.GroundNotification.Required);
        Assert.Contains("Local Authorities", string.Join(",", erpPlan.GroundNotification.Contacts));
    }

    [Fact]
    public void Generate_SailV_PhotovoltaicParkInspection_ShouldHaveStrongNotification()
    {
        // Arrange
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.PhotovoltaicParkInspection,
            Category = MissionCategory.EnergyAndUtilities,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 50m,
            RouteLength_m = 3000m,
            CgaArea_m2 = 50000m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7200,37.9900],[23.7400,37.9900],[23.7400,37.9700],[23.7200,37.9700],[23.7200,37.9900]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 7,
            FinalGrc = 5,
            InitialArc = "d",
            ResidualArc = "c",
            Sail = "V" // Very high SAIL
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // Strong notification requirements for SAIL V
        Assert.True(erpPlan.GroundNotification.Required);
        Assert.Contains("Local Authorities", string.Join(",", erpPlan.GroundNotification.Contacts));
        
        // Should have multiple notification methods
        Assert.True(erpPlan.GroundNotification.Methods.Count >= 2);

        // Critical infrastructure should have additional contacts
        Assert.Contains("Infrastructure Manager/Owner", string.Join(",", erpPlan.GroundNotification.Contacts));

        // Should have comprehensive ERP actions
        Assert.NotEmpty(erpPlan.FlyAway.Actions);
        Assert.NotEmpty(erpPlan.EmergencyLanding.Actions);
    }

    [Fact]
    public void Generate_SailII_AgriculturalFieldMapping_Rural_ShouldBeMinimal()
    {
        // Arrange
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.AgriculturalFieldMapping,
            Category = MissionCategory.AgricultureAndForestry,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 40m,
            RouteLength_m = 1500m,
            CgaArea_m2 = 30000m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7250,37.9850],[23.7350,37.9850],[23.7350,37.9750],[23.7250,37.9750],[23.7250,37.9850]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 3,
            FinalGrc = 2,
            InitialArc = "b",
            ResidualArc = "a",
            Sail = "II" // Low-medium SAIL
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // Minimal ERP for rural agriculture SAIL II
        Assert.False(erpPlan.GroundNotification.Required); // Rural + low SAIL = optional
        Assert.False(erpPlan.AtsCoordination.Required); // No ATS for rural low SAIL

        // Should still have basic procedures
        Assert.True(erpPlan.LossOfC2.RthAltitude_m > 0);
        Assert.NotEmpty(erpPlan.EmergencyLanding.Sites);
    }

    [Fact]
    public void Generate_AllErpSections_ShouldBeValidJson()
    {
        // Arrange
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.RoofInspection_Flat,
            Category = MissionCategory.BuildingsAndFacades,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 30m,
            RouteLength_m = 200m,
            CgaArea_m2 = 1600m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7275,37.9838],[23.7285,37.9838],[23.7285,37.9828],[23.7275,37.9828],[23.7275,37.9838]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 4,
            FinalGrc = 3,
            InitialArc = "b",
            ResidualArc = "a",
            Sail = "III"
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        Assert.NotEmpty(result.ErpJson);
        
        // Verify JSON is valid and complete
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // All sections should be present and non-null
        Assert.NotNull(erpPlan.LossOfC2);
        Assert.NotNull(erpPlan.FlyAway);
        Assert.NotNull(erpPlan.EmergencyLanding);
        Assert.NotNull(erpPlan.GroundNotification);
        Assert.NotNull(erpPlan.AtsCoordination);

        // LossOfC2 should have valid data
        Assert.True(erpPlan.LossOfC2.RthAltitude_m > 0 && erpPlan.LossOfC2.RthAltitude_m <= 120);
        Assert.True(erpPlan.LossOfC2.MaxTimeToRth_s > 0);
        Assert.NotEmpty(erpPlan.LossOfC2.Actions);

        // EmergencyLanding should have at least one site
        Assert.NotEmpty(erpPlan.EmergencyLanding.Sites);
        Assert.NotEmpty(erpPlan.EmergencyLanding.Actions);

        // Narrative text should also be complete
        Assert.NotEmpty(result.ErpText);
        Assert.Contains("EMERGENCY RESPONSE PLAN", result.ErpText);
        Assert.Contains("LOSS OF C2 LINK", result.ErpText);
        Assert.Contains("EMERGENCY LANDING", result.ErpText);
    }

    [Fact]
    public void Generate_SafeAreaCoordinates_ShouldBeExtractedFromCga()
    {
        // Arrange
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.SolarPanelCleaning,
            Category = MissionCategory.EnergyAndUtilities,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 30m,
            RouteLength_m = 150m,
            CgaArea_m2 = 1000m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7275,37.9838],[23.7285,37.9838],[23.7285,37.9828],[23.7275,37.9828],[23.7275,37.9838]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 3,
            FinalGrc = 2,
            InitialArc = "a",
            ResidualArc = "a",
            Sail = "II"
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // Should have defined safe area with coordinates
        Assert.True(erpPlan.FlyAway.DefinedSafeArea);
        Assert.NotNull(erpPlan.FlyAway.SafeAreaLat);
        Assert.NotNull(erpPlan.FlyAway.SafeAreaLon);
        Assert.True(erpPlan.FlyAway.SafeAreaRadius_m > 0);
    }

    [Fact]
    public void Generate_SailI_UrbanFacadeCleaning_ShouldRequireNotification()
    {
        // Arrange - FacadeCleaning is urban, so even low SAIL requires notification
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            TemplateCode = MissionTemplateCode.FacadeCleaning,
            Category = MissionCategory.BuildingsAndFacades,
            SoraVersion = "2.5"
        };

        var geometry = new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            MaxHeightAGL_m = 30m,
            RouteLength_m = 100m,
            CgaArea_m2 = 500m,
            GeoJson = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""name"":""cga""},""geometry"":{""type"":""Polygon"",""coordinates"":[[[23.7275,37.9838],[23.7285,37.9838],[23.7285,37.9828],[23.7275,37.9828],[23.7275,37.9838]]]}}]}"
        };

        var sora = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = "2.5",
            InitialGrc = 2,
            FinalGrc = 1,
            InitialArc = "a",
            ResidualArc = "a",
            Sail = "I" // Low SAIL but urban environment
        };

        // Act
        var result = _service.Generate(mission, geometry, sora);

        // Assert
        var erpPlan = JsonSerializer.Deserialize<ErpPlan>(result.ErpJson);
        Assert.NotNull(erpPlan);

        // Urban SAIL I should still require notification (stricter than rural)
        Assert.True(erpPlan.GroundNotification.Required);
        Assert.NotEmpty(erpPlan.GroundNotification.Contacts);
        
        // Should have RTH timeout (urban reduces by 10, but low SAIL adds 15 = net +5 from 30)
        Assert.True(erpPlan.LossOfC2.MaxTimeToRth_s > 0);
        Assert.True(erpPlan.LossOfC2.MaxTimeToRth_s == 35); // 30 - 10 (urban) + 15 (SAIL I-II) = 35
        
        // ATS required for urban + high structure (FacadeCleaning is IsHighStructure = true)
        Assert.True(erpPlan.AtsCoordination.Required);
    }
}
