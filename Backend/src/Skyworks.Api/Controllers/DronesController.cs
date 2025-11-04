using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.Drones;
using Skyworks.Core.Services.Drones;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/drones")]
public class DronesController : ControllerBase
{
    private readonly IDroneCatalogService _catalogService;

    public DronesController(IDroneCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Get all drones in the catalog.
    /// </summary>
    [HttpGet("catalog")]
    public async Task<ActionResult<List<DroneSpecification>>> GetCatalog()
    {
        var drones = await _catalogService.GetAllDronesAsync();
        return Ok(new
        {
            count = drones.Count,
            drones
        });
    }

    /// <summary>
    /// Get a specific drone by ID.
    /// </summary>
    [HttpGet("{droneId}")]
    public async Task<ActionResult<DroneSpecification>> GetDrone(string droneId)
    {
        var drone = await _catalogService.GetDroneByIdAsync(droneId);
        if (drone == null)
        {
            return NotFound(new { error = $"Drone '{droneId}' not found in catalog." });
        }
        return Ok(drone);
    }

    /// <summary>
    /// Search drones by filter criteria.
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<List<DroneSpecification>>> SearchDrones([FromBody] DroneCatalogFilter filter)
    {
        var results = await _catalogService.SearchDronesAsync(filter);
        return Ok(new
        {
            count = results.Count,
            filter,
            results
        });
    }

    /// <summary>
    /// Get drone with SORA compliance hints.
    /// </summary>
    [HttpGet("{droneId}/sora-hints")]
    public async Task<ActionResult<DroneSelectionResult>> GetDroneWithSORAHints(string droneId)
    {
        var result = await _catalogService.GetDroneWithSORASuggestionsAsync(droneId);
        if (result == null || result.Drone == null)
        {
            return NotFound(new { error = $"Drone '{droneId}' not found in catalog." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Get manufacturers list.
    /// </summary>
    [HttpGet("manufacturers")]
    public async Task<ActionResult<List<string>>> GetManufacturers()
    {
        var drones = await _catalogService.GetAllDronesAsync();
        var manufacturers = drones
            .Select(d => d.Manufacturer)
            .Distinct()
            .OrderBy(m => m)
            .ToList();
        return Ok(new { count = manufacturers.Count, manufacturers });
    }

    /// <summary>
    /// Get lightweight list for dropdown (id, manufacturer, model, SORA specs).
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult> GetDropdownList()
    {
        var drones = await _catalogService.GetAllDronesAsync();
        var dropdown = drones
            .OrderBy(d => d.Manufacturer)
            .ThenBy(d => d.Model)
            .Select(d => new
            {
                id = d.Id,
                manufacturer = d.Manufacturer,
                model = d.Model,
                displayName = $"{d.Manufacturer} {d.Model}",
                cClass = d.CClass.ToString(),
                mTOM_kg = d.MTOM_kg,
                characteristicDimension_m = d.CharacteristicDimension_m,
                maxSpeed_mps = d.MaxSpeed_mps
            })
            .ToList();
        
        return Ok(new { count = dropdown.Count, drones = dropdown });
    }

    /// <summary>
    /// Get summary statistics.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStatistics()
    {
        var drones = await _catalogService.GetAllDronesAsync();
        
        var stats = new
        {
            totalDrones = drones.Count,
            byClass = drones.GroupBy(d => d.CClass).Select(g => new { cClass = g.Key.ToString(), count = g.Count() }),
            byManufacturer = drones.GroupBy(d => d.Manufacturer).Select(g => new { manufacturer = g.Key, count = g.Count() }).OrderByDescending(x => x.count),
            avgMTOM_kg = drones.Average(d => d.MTOM_kg),
            avgFlightTime_min = drones.Average(d => d.MaxFlightTime_min),
            bvlosCapable = drones.Count(d => d.IsBVLOSCapable),
            withObstacleAvoidance = drones.Count(d => d.HasObstacleAvoidance),
            withRemoteID = drones.Count(d => d.HasRemoteID)
        };

        return Ok(stats);
    }
}
