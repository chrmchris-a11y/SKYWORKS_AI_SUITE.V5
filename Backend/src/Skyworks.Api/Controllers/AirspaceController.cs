using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Services.Airspace;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/airspace")]
public class AirspaceController : ControllerBase
{
    private readonly IAirspaceProvider _provider;
    private readonly ILogger<AirspaceController> _logger;

    public AirspaceController(IAirspaceProvider provider, ILogger<AirspaceController> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Επιστρέφει τα airspaces που ισχύουν στο συγκεκριμένο σημείο (lat,lon[,alt]).
    /// </summary>
    [HttpGet("at-point")]
    public async Task<IActionResult> GetAtPoint(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double? altitudeMeters)
    {
        var point = new GeoPoint { Latitude = latitude, Longitude = longitude, AltitudeMeters = altitudeMeters };
        var result = await _provider.GetAirspacesAtPointAsync(point);

        return Ok(new
        {
            Point = point,
            Count = result.Airspaces.Count,
            Airspaces = result.Airspaces.Select(a => new
            {
                a.Id,
                a.Name,
                Type = a.Type.ToString(),
                Class = a.Class?.ToString(),
                a.LowerLimitMeters,
                a.UpperLimitMeters
            }),
            result.Notes
        });
    }

    /// <summary>
    /// Επιστρέφει airspaces που τέμνουν μια επιχειρησιακή περιοχή (πολύγωνο).
    /// </summary>
    [HttpPost("area/intersections")]
    public async Task<IActionResult> GetIntersections([
        FromBody] List<GeoPoint> polygon,
        [FromQuery] double? minAltitudeMeters,
        [FromQuery] double? maxAltitudeMeters)
    {
        if (polygon == null || polygon.Count < 3)
            return BadRequest(new { Message = "A valid polygon requires at least 3 points." });

        var list = await _provider.GetAirspacesIntersectingPolygonAsync(polygon, minAltitudeMeters, maxAltitudeMeters);
        return Ok(new
        {
            Count = list.Count,
            Airspaces = list.Select(a => new
            {
                a.Id,
                a.Name,
                Type = a.Type.ToString(),
                Class = a.Class?.ToString(),
                a.LowerLimitMeters,
                a.UpperLimitMeters
            })
        });
    }
}
