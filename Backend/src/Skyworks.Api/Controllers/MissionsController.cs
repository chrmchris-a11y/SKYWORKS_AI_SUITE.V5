using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services;
using Skyworks.Core.Services.DTOs;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Skyworks.Api.Controllers;

/// <summary>
/// Controller for mission management operations.
/// </summary>
[ApiController]
[Route("api/v1/missions")]
public class MissionsController : ControllerBase
{
    private readonly IMissionOrchestratorService _missionOrchestrator;
    private readonly IMissionRepository _missionRepository;

    public MissionsController(
        IMissionOrchestratorService missionOrchestrator,
        IMissionRepository missionRepository)
    {
        _missionOrchestrator = missionOrchestrator;
        _missionRepository = missionRepository;
    }

    /// <summary>
    /// Auto-creates a complete mission with geometry, SORA assessment, ERP, and OSO checklist.
    /// </summary>
    /// <param name="request">Mission creation parameters</param>
    /// <returns>Mission ID and summary</returns>
    /// <remarks>
    /// Executes the complete mission creation pipeline:
    /// 1. Resolves template metadata
    /// 2. Creates mission entity
    /// 3. Generates geometry pattern
    /// 4. Calculates SORA (GRC/ARC/SAIL)
    /// 5. Generates ERP (emergency procedures)
    /// 6. Builds OSO checklist
    /// 7. Persists to database
    /// 
    /// Sample request:
    /// ```json
    /// {
    ///   "templateCode": "PhotovoltaicParkInspection",
    ///   "centerLat": 41.0082,
    ///   "centerLon": 28.9784,
    ///   "maxHeightAGL_m": 60,
    ///   "droneModel": "DJI Matrice 300 RTK",
    ///   "droneMtom_kg": 6.3,
    ///   "droneClass": "C3"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Mission created successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error during mission creation</response>
    [HttpPost("auto-create")]
    [ProducesResponseType(typeof(CreateMissionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CreateMissionResponse>> AutoCreateMission([FromBody] CreateMissionRequest request)
    {
        // Validate request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Execute pipeline
            var missionId = await _missionOrchestrator.CreateMissionAsync(request);

            // Return response
            var response = new CreateMissionResponse
            {
                MissionId = missionId
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create mission", details = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves comprehensive overview of a mission including all components.
    /// </summary>
    /// <param name="id">Mission ID (GUID)</param>
    /// <returns>Mission overview with geometry, SORA, ERP, and OSO data</returns>
    /// <remarks>
    /// Returns all mission data needed for UI display:
    /// - Basic info (name, template, category, type, SORA version)
    /// - Geometry (GeoJSON route, CGA, dimensions)
    /// - SORA assessment (GRC, ARC, SAIL)
    /// - Emergency Response Plan (JSON + text)
    /// - OSO coverage (required, covered, missing counts)
    /// 
    /// Sample response:
    /// ```json
    /// {
    ///   "missionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "name": "PhotovoltaicParkInspection - 41.0082,28.9784 (2025-11-10)",
    ///   "templateCode": "PhotovoltaicParkInspection",
    ///   "category": "EnergyAndUtilities",
    ///   "missionType": "Solar",
    ///   "soraVersion": "SORA 2.5",
    ///   "geometry": {
    ///     "routeGeoJson": "{...}",
    ///     "routeLength_m": 1250.5,
    ///     "cgaArea_m2": 15000.0,
    ///     "maxHeightAGL_m": 60.0
    ///   },
    ///   "sora": {
    ///     "initialGrc": 5,
    ///     "finalGrc": 3,
    ///     "initialArc": "ARC-b",
    ///     "residualArc": "ARC-a",
    ///     "sail": "III"
    ///   },
    ///   "erp": {
    ///     "erpJson": "{...}",
    ///     "erpText": "Emergency Response Plan..."
    ///   },
    ///   "oso": {
    ///     "requiredCount": 23,
    ///     "coveredCount": 8,
    ///     "missingCount": 15
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Mission overview retrieved successfully</response>
    /// <response code="404">Mission not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/overview")]
    [ProducesResponseType(typeof(MissionOverviewResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<MissionOverviewResponse>> GetMissionOverview(Guid id)
    {
        try
        {
            // Fetch mission from repository
            var mission = await _missionRepository.GetMissionByIdAsync(id);
            
            if (mission == null)
            {
                return NotFound(new { error = "Mission not found", missionId = id });
            }
            
            // Build response DTO
            var response = new MissionOverviewResponse
            {
                MissionId = mission.MissionId,
                Name = mission.Name,
                TemplateCode = mission.TemplateCode.ToString(),
                Category = mission.Category.ToString(),
                MissionType = mission.Type.ToString(),
                SoraVersion = mission.SoraVersion,
                CreatedAt = mission.CreatedAt
            };
            
            // Geometry
            if (mission.Geometry != null)
            {
                response.Geometry = new MissionOverviewResponse.GeometryData
                {
                    RouteGeoJson = mission.Geometry.GeoJson,
                    RouteLength_m = (double)mission.Geometry.RouteLength_m,
                    CgaArea_m2 = (double)mission.Geometry.CgaArea_m2,
                    MaxHeightAGL_m = (double)mission.Geometry.MaxHeightAGL_m,
                    ControlledGroundAreaGeoJson = null // CGA is part of GeoJson FeatureCollection
                };
            }
            
            // SORA Assessment
            if (mission.SoraAssessment != null)
            {
                response.Sora = new MissionOverviewResponse.SoraData
                {
                    InitialGrc = mission.SoraAssessment.InitialGrc,
                    FinalGrc = mission.SoraAssessment.FinalGrc,
                    InitialArc = mission.SoraAssessment.InitialArc,
                    ResidualArc = mission.SoraAssessment.ResidualArc,
                    Sail = mission.SoraAssessment.Sail,
                    InputsJson = mission.SoraAssessment.InputsSnapshotJson
                };
            }
            
            // ERP
            if (mission.Erp != null)
            {
                response.Erp = new MissionOverviewResponse.ErpData
                {
                    ErpJson = mission.Erp.ErpJson,
                    ErpText = mission.Erp.ErpText
                };
            }
            
            // OSO Coverage
            if (mission.OsoCoverage != null)
            {
                // Parse JSON arrays to get counts
                int requiredCount = 0, coveredCount = 0, missingCount = 0;
                
                try
                {
                    if (!string.IsNullOrEmpty(mission.OsoCoverage.RequiredOsosJson))
                    {
                        var required = JsonSerializer.Deserialize<string[]>(mission.OsoCoverage.RequiredOsosJson);
                        requiredCount = required?.Length ?? 0;
                    }
                    if (!string.IsNullOrEmpty(mission.OsoCoverage.CoveredOsosJson))
                    {
                        var covered = JsonSerializer.Deserialize<string[]>(mission.OsoCoverage.CoveredOsosJson);
                        coveredCount = covered?.Length ?? 0;
                    }
                    if (!string.IsNullOrEmpty(mission.OsoCoverage.MissingOsosJson))
                    {
                        var missing = JsonSerializer.Deserialize<string[]>(mission.OsoCoverage.MissingOsosJson);
                        missingCount = missing?.Length ?? 0;
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, leave counts at 0
                }
                
                response.Oso = new MissionOverviewResponse.OsoData
                {
                    RequiredOsosJson = mission.OsoCoverage.RequiredOsosJson,
                    CoveredOsosJson = mission.OsoCoverage.CoveredOsosJson,
                    MissingOsosJson = mission.OsoCoverage.MissingOsosJson,
                    RequiredCount = requiredCount,
                    CoveredCount = coveredCount,
                    MissingCount = missingCount
                };
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve mission overview", details = ex.Message });
        }
    }
}
