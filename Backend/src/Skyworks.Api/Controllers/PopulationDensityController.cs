using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Services.GRC;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/population")]
public class PopulationDensityController : ControllerBase
{
    private readonly IPopulationDensityProvider _provider;
    private readonly IGRCCalculationService _grcService;
    private readonly ILogger<PopulationDensityController> _logger;

    public PopulationDensityController(
        IPopulationDensityProvider provider,
        IGRCCalculationService grcService,
        ILogger<PopulationDensityController> logger)
    {
        _provider = provider;
        _grcService = grcService;
        _logger = logger;
    }

    /// <summary>
    /// Get population density at a point location (SORA 2.5 Table 3)
    /// </summary>
    [HttpGet("density")]
    public async Task<IActionResult> GetDensity([FromQuery] double latitude, [FromQuery] double longitude)
    {
        var density = await _provider.GetDensityAsync(latitude, longitude);
        if (!density.HasValue)
            return NotFound(new { Message = "No population density data available for this location." });

        return Ok(new { Latitude = latitude, Longitude = longitude, Density = density.Value });
    }

    /// <summary>
    /// Get SORA 2.5 population density category for a point (Table 3)
    /// </summary>
    [HttpGet("category")]
    public async Task<IActionResult> GetCategory(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] bool isControlledArea = false)
    {
        var density = await _provider.GetDensityAsync(latitude, longitude);
        if (!density.HasValue)
            return NotFound(new { Message = "No population density data available for this location." });

        var category = _grcService.GetPopulationCategory(density.Value, isControlledArea);
        return Ok(new
        {
            Latitude = latitude,
            Longitude = longitude,
            Density = density.Value,
            IsControlledArea = isControlledArea,
            Category = category.ToString(),
            Notes = "Per SORA 2.5 Table 3"
        });
    }

    /// <summary>
    /// Get maximum population density within an operational area (bounding polygon)
    /// </summary>
    [HttpPost("area/max-density")]
    public async Task<IActionResult> GetMaxDensityInArea([FromBody] List<LocationPoint> polygon)
    {
        if (polygon == null || polygon.Count < 3)
            return BadRequest(new { Message = "A valid polygon requires at least 3 points." });

        var points = polygon.Select(p => (p.Latitude, p.Longitude));
        var maxDensity = await _provider.GetMaxDensityInAreaAsync(points);

        if (!maxDensity.HasValue)
            return NotFound(new { Message = "No population density data available for this area." });

        return Ok(new { MaxDensity = maxDensity.Value, PointCount = polygon.Count });
    }
}

public record LocationPoint(double Latitude, double Longitude);
