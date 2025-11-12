using System;
using System.Text.Json;
using System.Threading.Tasks;
using Skyworks.Core.Models;
using Skyworks.Core.Services.DTOs;

namespace Skyworks.Core.Services;

/// <summary>
/// Orchestrates the complete mission creation pipeline.
/// Coordinates all services to create a mission with geometry, SORA, ERP, and OSO checklist.
/// </summary>
public class MissionOrchestratorService : IMissionOrchestratorService
{
    private readonly IMissionTemplatesConfig _templatesConfig;
    private readonly IMissionGeometryService _geometryService;
    private readonly ISoraCalculatorService _soraCalculator;
    private readonly IErpGeneratorService _erpGenerator;
    private readonly IOsoCoverageService _osoCoverageService;
    private readonly IMissionRepository _missionRepository;

    public MissionOrchestratorService(
        IMissionTemplatesConfig templatesConfig,
        IMissionGeometryService geometryService,
        ISoraCalculatorService soraCalculator,
        IErpGeneratorService erpGenerator,
        IOsoCoverageService osoCoverageService,
        IMissionRepository missionRepository)
    {
        _templatesConfig = templatesConfig;
        _geometryService = geometryService;
        _soraCalculator = soraCalculator;
        _erpGenerator = erpGenerator;
        _osoCoverageService = osoCoverageService;
        _missionRepository = missionRepository;
    }

    /// <summary>
    /// Creates a complete mission following the 7-step pipeline:
    /// 1. Resolve template metadata
    /// 2. Create Mission entity
    /// 3. Generate geometry
    /// 4. Calculate SORA (GRC/ARC/SAIL)
    /// 5. Generate ERP
    /// 6. Build OSO checklist
    /// 7. Persist to database
    /// </summary>
    public async Task<Guid> CreateMissionAsync(CreateMissionRequest request)
    {
        // =========================================================================
        // STEP 1: Resolve Template Metadata
        // =========================================================================
        var metadata = _templatesConfig.Get(request.TemplateCode);
        if (metadata == null)
        {
            throw new ArgumentException($"Unknown template code: {request.TemplateCode}");
        }

        // =========================================================================
        // STEP 2: Create Mission Entity
        // =========================================================================
        var mission = new Mission
        {
            MissionId = Guid.NewGuid(),
            Name = request.MissionName ?? GenerateMissionName(metadata, request),
            TemplateCode = request.TemplateCode,
            Category = metadata.Category,
            Type = metadata.DefaultMissionType,
            SoraVersion = request.SoraVersion ?? metadata.DefaultSoraVersion,
            ClientName = request.ClientName,
            LocationAddress = null, // Can be enhanced with geocoding later
            LocationCity = null,
            LocationCountry = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Use authenticated user
        };

        // =========================================================================
        // STEP 3: Generate Geometry
        // =========================================================================
        var geometryRequest = new GeometryGenerationRequest
        {
            MissionType = metadata.DefaultMissionType,
            CenterLat = request.CenterLat,
            CenterLon = request.CenterLon,
            MaxHeightAGL_m = request.MaxHeightAGL_m,
            AreaWidth_m = request.AreaWidth_m ?? GetDefaultAreaWidth(metadata),
            AreaLength_m = request.AreaLength_m ?? GetDefaultAreaLength(metadata)
        };

        var missionGeometry = await _geometryService.GenerateAsync(geometryRequest);

        // Override MissionId
        missionGeometry.MissionId = mission.MissionId;

        // =========================================================================
        // STEP 4: Calculate SORA (GRC, ARC, SAIL)
        // =========================================================================
        var soraRequest = BuildSoraRequest(request, metadata, missionGeometry);
        var soraResult = await _soraCalculator.CalculateAsync(soraRequest);

        var missionSoraAssessment = new MissionSoraAssessment
        {
            MissionSoraAssessmentId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            SoraVersion = soraRequest.SoraVersion,
            Sail = soraResult.Sail,
            InitialGrc = soraResult.InitialGrc,
            FinalGrc = soraResult.FinalGrc,
            InitialArc = soraResult.InitialArc,
            ResidualArc = soraResult.ResidualArc,
            InputsSnapshotJson = soraResult.InputsJson
        };

        // =========================================================================
        // STEP 5: Generate ERP (Emergency Response Plan)
        // =========================================================================
        var erpResult = _erpGenerator.Generate(mission, missionGeometry, missionSoraAssessment);

        var missionErp = new MissionErp
        {
            MissionErpId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            ErpJson = erpResult.ErpJson,
            ErpText = erpResult.ErpText
        };

        // =========================================================================
        // STEP 6: Build OSO Coverage Checklist
        // =========================================================================
        var osoResult = _osoCoverageService.BuildChecklist(mission, missionErp, missionSoraAssessment);

        var missionOsoCoverage = new MissionOsoCoverage
        {
            MissionOsoCoverageId = Guid.NewGuid(),
            MissionId = mission.MissionId,
            RequiredOsosJson = osoResult.RequiredOsosJson,
            CoveredOsosJson = osoResult.CoveredOsosJson,
            MissingOsosJson = osoResult.MissingOsosJson
        };

        // =========================================================================
        // STEP 7: Persist to Database
        // =========================================================================
        // Attach navigation properties
        mission.Geometry = missionGeometry;
        mission.SoraAssessment = missionSoraAssessment;
        mission.Erp = missionErp;
        mission.OsoCoverage = missionOsoCoverage;

        var missionId = await _missionRepository.SaveMissionAsync(mission);

        return missionId;
    }

    #region Private Helpers

    /// <summary>
    /// Generates a default mission name from template and location.
    /// </summary>
    private string GenerateMissionName(MissionTemplateMetadata metadata, CreateMissionRequest request)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var templateName = request.TemplateCode.ToString().Replace("_", " ");
        return $"{templateName} - {request.CenterLat:F4},{request.CenterLon:F4} ({timestamp})";
    }

    /// <summary>
    /// Gets default area width based on template metadata.
    /// </summary>
    private double GetDefaultAreaWidth(MissionTemplateMetadata metadata)
    {
        // Defaults based on mission type
        return metadata.DefaultMissionType switch
        {
            MissionType.Facade => 50.0,    // 50m wide building
            MissionType.Roof => 40.0,      // 40m wide roof
            MissionType.Solar => 100.0,    // 100m solar park section
            MissionType.Linear => 20.0,    // 20m corridor width
            MissionType.Agriculture => 200.0, // 200m field width
            _ => 50.0
        };
    }

    /// <summary>
    /// Gets default area length based on template metadata.
    /// </summary>
    private double GetDefaultAreaLength(MissionTemplateMetadata metadata)
    {
        return metadata.DefaultMissionType switch
        {
            MissionType.Facade => 80.0,    // 80m long building
            MissionType.Roof => 60.0,      // 60m long roof
            MissionType.Solar => 150.0,    // 150m solar park section
            MissionType.Linear => 500.0,   // 500m pipeline/road
            MissionType.Agriculture => 300.0, // 300m field length
            _ => 100.0
        };
    }

    /// <summary>
    /// Builds SORA request from mission parameters and metadata.
    /// Applies conservative default mitigations.
    /// </summary>
    private SoraRequest BuildSoraRequest(
        CreateMissionRequest request,
        MissionTemplateMetadata metadata,
        MissionGeometry geometry)
    {
        var soraRequest = new SoraRequest
        {
            SoraVersion = request.SoraVersion ?? metadata.DefaultSoraVersion,
            DroneMtom_kg = request.DroneMtom_kg,
            DroneMaxSpeed_ms = EstimateDroneSpeed(request.DroneMtom_kg),
            DroneCharacteristicDimension_m = EstimateDroneDimension(request.DroneMtom_kg),
            MaxHeightAGL_m = request.MaxHeightAGL_m,
            ControlledGroundArea = geometry.CgaArea_m2 > 0,
            Environment = metadata.DefaultEnvironment,
            IsVLOS = request.MaxHeightAGL_m <= 120 && geometry.RouteLength_m < 500
        };

        // Apply default SORA 2.5 mitigations (conservative for automation)
        if (soraRequest.SoraVersion == "2.5")
        {
            soraRequest.PopulationDensity = MapEnvironmentToPopulationDensity(metadata.DefaultEnvironment);
            soraRequest.M1a_Sheltering = geometry.CgaArea_m2 > 0 ? "Low" : "None";
            soraRequest.M1b_OperationalRestrictions = "Medium"; // Standard operational procedures
            soraRequest.M1c_GroundObservation = soraRequest.IsVLOS ? "Low" : "None";
            soraRequest.M2_ImpactDynamics = request.DroneMtom_kg < 5 ? "Medium" : "High";
        }

        return soraRequest;
    }

    /// <summary>
    /// Estimates drone max speed based on MTOM.
    /// </summary>
    private double EstimateDroneSpeed(double mtom_kg)
    {
        // Conservative estimates for common drone classes
        return mtom_kg switch
        {
            < 1 => 10.0,      // Sub-250g toy drones
            < 5 => 15.0,      // C0-C1 consumer drones
            < 25 => 20.0,     // C2-C3 prosumer drones
            _ => 25.0         // Heavy industrial drones
        };
    }

    /// <summary>
    /// Estimates drone characteristic dimension based on MTOM.
    /// </summary>
    private double EstimateDroneDimension(double mtom_kg)
    {
        return mtom_kg switch
        {
            < 1 => 0.2,       // ~20cm
            < 5 => 0.4,       // ~40cm
            < 25 => 0.8,      // ~80cm
            _ => 1.5          // ~1.5m for heavy drones
        };
    }

    /// <summary>
    /// Maps environment string to SORA 2.5 population density category.
    /// </summary>
    private string MapEnvironmentToPopulationDensity(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "urban" => "<5000",
            "suburban" => "<500",
            "rural" => "<50",
            _ => "<500" // Default to suburban
        };
    }

    #endregion
}
