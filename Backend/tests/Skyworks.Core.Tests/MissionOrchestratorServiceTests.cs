using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Skyworks.Core.Models;
using Skyworks.Core.Services;
using Skyworks.Core.Services.DTOs;
using Xunit;

namespace Skyworks.Core.Tests;

/// <summary>
/// Integration tests for MissionOrchestratorService.
/// Tests the complete pipeline: Template → Mission → Geometry → SORA → ERP → OSO
/// </summary>
public class MissionOrchestratorServiceTests
{
    private readonly IMissionOrchestratorService _orchestrator;
    private readonly InMemoryMissionRepository _repository;

    public MissionOrchestratorServiceTests()
    {
        // Setup dependencies with real services where possible
        var templatesConfig = new MissionTemplatesConfig();
        var geometryService = new MissionGeometryService();
        var erpGenerator = new ErpGeneratorService(templatesConfig);
        var osoCatalog = new OsoCatalog();
        var osoCoverageService = new OsoCoverageService(osoCatalog);
        
        // Mock SORA calculator (would normally call Node.js)
        var soraCalculator = new MockSoraCalculatorService();
        
        // In-memory repository for testing
        _repository = new InMemoryMissionRepository();

        _orchestrator = new MissionOrchestratorService(
            templatesConfig,
            geometryService,
            soraCalculator,
            erpGenerator,
            osoCoverageService,
            _repository
        );
    }

    #region Test 1: Photovoltaic Park Inspection (Full Tree)

    [Fact]
    public async Task CreateMissionAsync_PhotovoltaicParkInspection_ShouldCreateFullMissionTree()
    {
        // Arrange
        var request = new CreateMissionRequest
        {
            TemplateCode = MissionTemplateCode.PhotovoltaicParkInspection,
            CenterLat = 41.0082,
            CenterLon = 28.9784, // Istanbul, Turkey
            MaxHeightAGL_m = 60,
            DroneModel = "DJI Matrice 300 RTK",
            DroneMtom_kg = 6.3,
            DroneClass = "C3"
        };

        // Act
        var missionId = await _orchestrator.CreateMissionAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, missionId);

        // Retrieve the saved mission
        var mission = await _repository.GetMissionByIdAsync(missionId);
        Assert.NotNull(mission);

        // Verify mission properties
        Assert.Equal(MissionTemplateCode.PhotovoltaicParkInspection, mission!.TemplateCode);
        Assert.Equal(MissionCategory.EnergyAndUtilities, mission.Category);
        Assert.Equal("2.5", mission.SoraVersion);

        // Verify geometry was created
        Assert.NotNull(mission.Geometry);
        Assert.True(mission.Geometry.RouteLength_m > 0, "Route length should be > 0");
        Assert.True(mission.Geometry.CgaArea_m2 > 0, "CGA area should be > 0");
        Assert.Equal(60, mission.Geometry.MaxHeightAGL_m);

        // Verify SORA assessment
        Assert.NotNull(mission.SoraAssessment);
        Assert.True(mission.SoraAssessment.InitialGrc > 0, "Initial GRC should be > 0");
        Assert.True(mission.SoraAssessment.FinalGrc > 0, "Final GRC should be > 0");
        Assert.False(string.IsNullOrEmpty(mission.SoraAssessment.Sail), "SAIL should not be empty");
        Assert.Contains(mission.SoraAssessment.Sail, new[] { "I", "II", "III", "IV", "V", "VI" });

        // Verify ERP was generated
        Assert.NotNull(mission.Erp);
        Assert.False(string.IsNullOrEmpty(mission.Erp.ErpJson), "ERP JSON should not be empty");
        Assert.False(string.IsNullOrEmpty(mission.Erp.ErpText), "ERP text should not be empty");

        // Verify OSO coverage
        Assert.NotNull(mission.OsoCoverage);
        Assert.False(string.IsNullOrEmpty(mission.OsoCoverage.RequiredOsosJson), "Required OSOs JSON should not be empty");
        Assert.False(string.IsNullOrEmpty(mission.OsoCoverage.CoveredOsosJson), "Covered OSOs JSON should not be empty");
        Assert.False(string.IsNullOrEmpty(mission.OsoCoverage.MissingOsosJson), "Missing OSOs JSON should not be empty");
    }

    #endregion

    #region Test 2: Solar Panel Cleaning (Roof Pattern + ERP)

    [Fact]
    public async Task CreateMissionAsync_SolarPanelCleaning_ShouldGenerateRoofOrSolarPatternAndErp()
    {
        // Arrange
        var request = new CreateMissionRequest
        {
            TemplateCode = MissionTemplateCode.SolarPanelCleaning,
            CenterLat = 37.9838,
            CenterLon = 23.7275, // Athens, Greece
            MaxHeightAGL_m = 40,
            DroneModel = "DJI Mini 2",
            DroneMtom_kg = 0.249,
            DroneClass = "C0"
        };

        // Act
        var missionId = await _orchestrator.CreateMissionAsync(request);

        // Assert
        var mission = await _repository.GetMissionByIdAsync(missionId);
        Assert.NotNull(mission);

        // Verify mission type is Roof or Solar
        Assert.Contains(mission!.Type, new[] { MissionType.Roof, MissionType.Solar });

        // Verify geometry pattern
        Assert.NotNull(mission.Geometry);
        Assert.True(mission.Geometry.RouteLength_m > 0, "Roof/solar pattern should have route");

        // Verify ERP has required sections
        Assert.NotNull(mission.Erp);
        var erpJson = JsonDocument.Parse(mission.Erp.ErpJson);
        
        // Check for Loss of C2 section (PascalCase from .NET serialization)
        Assert.True(erpJson.RootElement.TryGetProperty("LossOfC2", out var lossOfC2), 
            "ERP should have LossOfC2 section");
        
        // Check for Emergency Landing section
        Assert.True(erpJson.RootElement.TryGetProperty("EmergencyLanding", out var emergencyLanding),
            "ERP should have EmergencyLanding section");
    }

    #endregion

    #region Test 3: Training Flight VLOS (Low Risk Profile)

    [Fact]
    public async Task CreateMissionAsync_TrainingFlightVLOS_ShouldHaveLowRiskProfile()
    {
        // Arrange
        var request = new CreateMissionRequest
        {
            TemplateCode = MissionTemplateCode.TrainingFlightVLOS,
            CenterLat = 51.5074,
            CenterLon = -0.1278, // London, UK (but will use rural environment from metadata)
            MaxHeightAGL_m = 50,
            DroneModel = "DJI Mavic 3",
            DroneMtom_kg = 0.895,
            DroneClass = "C1"
        };

        // Act
        var missionId = await _orchestrator.CreateMissionAsync(request);

        // Assert
        var mission = await _repository.GetMissionByIdAsync(missionId);
        Assert.NotNull(mission);

        // Verify SORA assessment has low SAIL (I or II for training in rural area)
        Assert.NotNull(mission!.SoraAssessment);
        Assert.Contains(mission.SoraAssessment.Sail, new[] { "I", "II", "III" }); // Should be low-medium

        // Verify ERP
        Assert.NotNull(mission.Erp);
        var erpJson = JsonDocument.Parse(mission.Erp.ErpJson);

        // Ground notification should be minimal (likely Client only for rural training)
        if (erpJson.RootElement.TryGetProperty("groundNotification", out var groundNotif))
        {
            // Either not required, or required with low complexity
            if (groundNotif.TryGetProperty("required", out var required))
            {
                // If required, should be simple notification
                Assert.True(required.GetBoolean() == false || 
                           groundNotif.TryGetProperty("notificationType", out _));
            }
        }

        // ATS coordination should NOT be required for low-altitude VLOS training
        if (erpJson.RootElement.TryGetProperty("atsCoordination", out var atsCoord))
        {
            if (atsCoord.TryGetProperty("required", out var atsRequired))
            {
                // For VLOS at 50m in uncontrolled airspace, ATC coordination should be false
                Assert.False(atsRequired.GetBoolean(), 
                    "ATS coordination should not be required for low-altitude VLOS training");
            }
        }
    }

    #endregion
}

#region Test Helpers: In-Memory Repository

/// <summary>
/// In-memory mission repository for testing (no actual database).
/// </summary>
internal class InMemoryMissionRepository : IMissionRepository
{
    private readonly Dictionary<Guid, Mission> _missions = new();

    public Task<Guid> SaveMissionAsync(Mission mission)
    {
        _missions[mission.MissionId] = mission;
        return Task.FromResult(mission.MissionId);
    }

    public Task<Mission?> GetMissionByIdAsync(Guid missionId)
    {
        _missions.TryGetValue(missionId, out var mission);
        return Task.FromResult(mission);
    }
}

#endregion

#region Test Helpers: Mock SORA Calculator

/// <summary>
/// Mock SORA calculator service for testing (no Node.js dependency).
/// Returns deterministic results based on inputs.
/// </summary>
internal class MockSoraCalculatorService : ISoraCalculatorService
{
    public Task<SoraResult> CalculateAsync(SoraRequest request)
    {
        // Simplified SORA logic for testing
        var initialGrc = CalculateInitialGrc(request);
        var finalGrc = ApplyGroundMitigations(initialGrc, request);
        var initialArc = DetermineInitialArc(request);
        var residualArc = ApplyAirMitigations(initialArc, request);
        var sail = DetermineSail(finalGrc, residualArc);

        var result = new SoraResult
        {
            InitialGrc = initialGrc,
            FinalGrc = finalGrc,
            InitialArc = initialArc,
            ResidualArc = residualArc,
            Sail = sail,
            InputsJson = JsonSerializer.Serialize(request)
        };

        return Task.FromResult(result);
    }

    private int CalculateInitialGrc(SoraRequest request)
    {
        // Simplified GRC calculation (Table 2 logic)
        var kineticEnergy = request.DroneMtom_kg * Math.Pow(request.DroneMaxSpeed_ms, 2) / 2;
        
        var popDensityScore = request.PopulationDensity switch
        {
            "Controlled" => 0,
            "<5" => 1,
            "<50" => 2,
            "<500" => 3,
            "<5000" => 4,
            "<50000" => 5,
            _ => 3
        };

        // Combine kinetic energy and population density
        if (kineticEnergy < 100) return Math.Max(1, popDensityScore - 1);
        if (kineticEnergy < 1000) return popDensityScore;
        return Math.Min(8, popDensityScore + 2);
    }

    private int ApplyGroundMitigations(int initialGrc, SoraRequest request)
    {
        var mitigationReduction = 0;

        // Apply mitigations more conservatively (SORA rules are complex)
        if (request.M1a_Sheltering == "Medium") mitigationReduction += 1;
        if (request.M1b_OperationalRestrictions == "High") mitigationReduction += 1;
        if (request.M2_ImpactDynamics == "High") mitigationReduction += 1;

        // Ensure we never reduce below 1 (GRC 0 is only for controlled ground area)
        return Math.Max(1, initialGrc - mitigationReduction);
    }

    private string DetermineInitialArc(SoraRequest request)
    {
        // Simplified ARC determination
        if (request.MaxHeightAGL_m < 120 && request.IsVLOS)
            return "a"; // Low complexity

        if (request.MaxHeightAGL_m < 400)
            return "b"; // Medium complexity

        return "c"; // High complexity
    }

    private string ApplyAirMitigations(string initialArc, SoraRequest request)
    {
        // VLOS provides tactical mitigation (reduce ARC by one level)
        if (request.IsVLOS && initialArc == "b")
            return "a";

        return initialArc;
    }

    private string DetermineSail(int finalGrc, string residualArc)
    {
        // Simplified SAIL determination (Table 5 mapping)
        if (finalGrc == 0) return "I";
        if (finalGrc <= 2 && residualArc == "a") return "I";
        if (finalGrc <= 3 && residualArc == "a") return "II";
        if (finalGrc <= 4) return "III";
        if (finalGrc <= 6) return "IV";
        if (finalGrc <= 7) return "V";
        return "VI";
    }
}

#endregion
