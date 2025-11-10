using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services;
using Skyworks.Core.Services.DTOs;
using System;
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

    public MissionsController(IMissionOrchestratorService missionOrchestrator)
    {
        _missionOrchestrator = missionOrchestrator;
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
}
