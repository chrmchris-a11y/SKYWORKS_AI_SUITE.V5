using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.ServiceZones;
using Skyworks.Core.Services.Airspace;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services.ServiceZones;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/service-zones")]
public class ServiceZonesController : ControllerBase
{
    private readonly IServiceZoneProvider _zones;
    private readonly IAirspaceProvider _airspace;
    private readonly IGRCCalculationService _arcService;
    private readonly ILogger<ServiceZonesController> _logger;

    public ServiceZonesController(
        IServiceZoneProvider zones,
        IAirspaceProvider airspace,
        IGRCCalculationService arcService,
        ILogger<ServiceZonesController> logger)
    {
        _zones = zones;
        _airspace = airspace;
        _arcService = arcService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> List()
    {
        var all = await _zones.GetAllAsync();
        return Ok(all.Select(z => new
        {
            z.Id,
            z.Name,
            Category = z.Category.ToString(),
            z.RadiusMeters,
            z.Notes,
            z.Center
        }));
    }

    [HttpGet("at-point")]
    public async Task<IActionResult> AtPoint([FromQuery] double latitude, [FromQuery] double longitude)
    {
        var point = new GeoPoint { Latitude = latitude, Longitude = longitude };
        var z = await _zones.FindZoneAtPointAsync(point);
        if (z == null) return NotFound(new { Message = "No service zone found at this point." });
        return Ok(z);
    }

    [HttpGet("suggest-arc/v2.5")]
    public async Task<IActionResult> SuggestArcV25(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double altitudeMeters = 60)
    {
        var point = new GeoPoint { Latitude = latitude, Longitude = longitude, AltitudeMeters = altitudeMeters };
        var zone = await _zones.FindZoneAtPointAsync(point);
        if (zone == null)
            return NotFound(new { Message = "No service zone found at this point to suggest ARC environment." });

        // Determine airspace control via provider
        var asp = await _airspace.GetAirspacesAtPointAsync(point);
        var isControlled = asp.Airspaces.Any(a => a.Type == AirspaceType.CTR || a.Type == AirspaceType.TMA || (a.Class.HasValue && a.Class.Value != Skyworks.Core.Models.Airspace.AirspaceClass.G));

        var env = new ARCEnvironmentInput
        {
            AirspaceControl = isControlled ? AirspaceControl.Controlled : AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = MapCategoryToEnvironment(zone.Category),
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = altitudeMeters
        };

        var result = _arcService.DetermineInitialARC_V2_5(env);
        return Ok(new
        {
            Zone = new { zone.Id, zone.Name, Category = zone.Category.ToString() },
            EnvironmentInput = env,
            InitialARC = result.ARC.ToString(),
            Notes = "Heuristic mapping of business categories to SORA environment for initial ARC"
        });
    }

    private static EnvironmentType MapCategoryToEnvironment(ServiceZoneCategory cat) => cat switch
    {
        // JARUS Annex C Table 1: Only Urban or Rural for ARC determination
        ServiceZoneCategory.Urban => EnvironmentType.Urban,
        ServiceZoneCategory.SemiUrban => EnvironmentType.Urban, // Semi-urban maps to Urban
        ServiceZoneCategory.Agriculture => EnvironmentType.Rural,
        ServiceZoneCategory.Industrial => EnvironmentType.Urban, // Industrial areas are urban
        ServiceZoneCategory.PhotovoltaicPark => EnvironmentType.Rural,
        ServiceZoneCategory.StadiumEvent => EnvironmentType.Urban,
        _ => EnvironmentType.Urban // Default to Urban (higher risk)
    };
}
