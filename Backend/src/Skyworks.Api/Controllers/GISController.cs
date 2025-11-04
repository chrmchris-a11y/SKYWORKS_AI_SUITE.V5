// Phase 6 Step 51 — Skyworks V5
using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.GIS;
using Skyworks.Core.Services.GIS;

namespace Skyworks.Api.Controllers;

/// <summary>
/// GIS API Controller for operational volumes, population density, airspace, no-fly zones, and terrain.
/// Supports dual SORA compliance (EASA AMC 2.0 and JARUS 2.5).
/// </summary>
[ApiController]
[Route("api/gis")]
[Produces("application/json")]
public class GISController : ControllerBase
{
    private readonly IGISService _gisService;
    private readonly ILogger<GISController> _logger;

    public GISController(IGISService gisService, ILogger<GISController> logger)
    {
        _gisService = gisService;
        _logger = logger;
    }

    // --- Operational Volume Endpoints ---

    /// <summary>Creates a new operational volume (OSO #17 compliance).</summary>
    [HttpPost("operational-volume")]
    public async Task<ActionResult<OperationalVolume>> CreateOperationalVolume(
        [FromBody] OperationalVolume request)
    {
        try
        {
            var volume = await _gisService.CreateOperationalVolumeAsync(request);
            return CreatedAtAction(
                nameof(GetOperationalVolumesByMission),
                new { missionId = volume.MissionId },
                volume);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create operational volume");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Gets all operational volumes for a mission.</summary>
    [HttpGet("operational-volume/mission/{missionId}")]
    public async Task<ActionResult<List<OperationalVolume>>> GetOperationalVolumesByMission(Guid missionId)
    {
        try
        {
            var volumes = await _gisService.GetOperationalVolumesByMissionAsync(missionId);
            return Ok(volumes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get operational volumes for mission {MissionId}", missionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Validates an operational volume.</summary>
    [HttpPost("operational-volume/validate")]
    public async Task<ActionResult<bool>> ValidateOperationalVolume([FromBody] OperationalVolume volume)
    {
        try
        {
            var isValid = await _gisService.ValidateOperationalVolumeAsync(volume);
            return Ok(new { isValid, volumeId = volume.Id, soraVersion = volume.SoraVersion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate operational volume {Id}", volume.Id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Deletes an operational volume.</summary>
    [HttpDelete("operational-volume/{volumeId}")]
    public async Task<ActionResult> DeleteOperationalVolume(Guid volumeId)
    {
        try
        {
            var deleted = await _gisService.DeleteOperationalVolumeAsync(volumeId);
            if (!deleted)
            {
                return NotFound(new { error = "Operational volume not found", volumeId });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete operational volume {Id}", volumeId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // --- Population Density Endpoints ---

    /// <summary>Gets population density for a specific point.</summary>
    [HttpGet("population-density")]
    public async Task<ActionResult<PopulationDensityZone>> GetPopulationDensity(
        [FromQuery] double lat,
        [FromQuery] double lon)
    {
        try
        {
            var zone = await _gisService.GetPopulationDensityAsync(lat, lon);
            if (zone == null)
            {
                return NotFound(new { error = "No population data available", latitude = lat, longitude = lon });
            }
            return Ok(zone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get population density at ({Lat},{Lon})", lat, lon);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Gets population density zones within a bounding box.</summary>
    [HttpGet("population-density/zones")]
    public async Task<ActionResult<List<PopulationDensityZone>>> GetPopulationDensityZones(
        [FromQuery] double minLat,
        [FromQuery] double maxLat,
        [FromQuery] double minLon,
        [FromQuery] double maxLon)
    {
        try
        {
            var zones = await _gisService.GetPopulationDensityZonesAsync(minLat, maxLat, minLon, maxLon);
            return Ok(zones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get population density zones");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Calculates Intrinsic GRC for a location (dual SORA support).</summary>
    [HttpGet("population-density/igrc")]
    public async Task<ActionResult<object>> CalculateIntrinsicGRC(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] string soraVersion,
        [FromQuery] double uaDimension,
        [FromQuery] double? uaSpeed = null)
    {
        try
        {
            if (soraVersion != "2.0" && soraVersion != "2.5")
            {
                return BadRequest(new { error = "Invalid SORA version. Must be '2.0' or '2.5'" });
            }

            var iGRC = await _gisService.CalculateIntrinsicGRCAsync(lat, lon, soraVersion, uaDimension, uaSpeed);
            
            return Ok(new
            {
                latitude = lat,
                longitude = lon,
                soraVersion,
                uaDimension,
                uaSpeed,
                intrinsicGRC = iGRC,
                calculatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate iGRC at ({Lat},{Lon})", lat, lon);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // --- Airspace Endpoints ---

    /// <summary>Gets airspace volumes containing a specific point.</summary>
    [HttpGet("airspace")]
    public async Task<ActionResult<List<AirspaceVolume>>> GetAirspace(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double altitude)
    {
        try
        {
            var volumes = await _gisService.GetAirspaceVolumesAsync(lat, lon, altitude);
            return Ok(volumes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get airspace at ({Lat},{Lon},{Alt})", lat, lon, altitude);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Determines airspace class for a point and altitude.</summary>
    [HttpGet("airspace/class")]
    public async Task<ActionResult<object>> DetermineAirspaceClass(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double altitude)
    {
        try
        {
            var airspaceClass = await _gisService.DetermineAirspaceClassAsync(lat, lon, altitude);
            return Ok(new
            {
                latitude = lat,
                longitude = lon,
                altitudeAMSL = altitude,
                airspaceClass,
                queriedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine airspace class at ({Lat},{Lon},{Alt})", lat, lon, altitude);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Calculates Initial ARC using SORA decision tree.</summary>
    [HttpGet("airspace/initial-arc")]
    public async Task<ActionResult<object>> CalculateInitialARC(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double altitudeAGL,
        [FromQuery] double altitudeAMSL,
        [FromQuery] string environment,
        [FromQuery] string soraVersion)
    {
        try
        {
            if (soraVersion != "2.0" && soraVersion != "2.5")
            {
                return BadRequest(new { error = "Invalid SORA version. Must be '2.0' or '2.5'" });
            }

            var initialARC = await _gisService.CalculateInitialARCAsync(
                lat, lon, altitudeAGL, altitudeAMSL, environment, soraVersion);
            
            return Ok(new
            {
                latitude = lat,
                longitude = lon,
                altitudeAGL,
                altitudeAMSL,
                environment,
                soraVersion,
                initialARC,
                calculatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate initial ARC at ({Lat},{Lon})", lat, lon);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // --- No-Fly Zone Endpoints ---

    /// <summary>Gets no-fly zones within a bounding box for a specific date.</summary>
    [HttpGet("no-fly-zones")]
    public async Task<ActionResult<List<NoFlyZone>>> GetNoFlyZones(
        [FromQuery] double minLat,
        [FromQuery] double maxLat,
        [FromQuery] double minLon,
        [FromQuery] double maxLon,
        [FromQuery] DateTime operationDate)
    {
        try
        {
            var zones = await _gisService.GetNoFlyZonesAsync(minLat, maxLat, minLon, maxLon, operationDate);
            return Ok(zones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get no-fly zones");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Checks if a flight path violates any no-fly zones.</summary>
    [HttpPost("no-fly-zones/check-violation")]
    public async Task<ActionResult<object>> CheckNoFlyZoneViolation(
        [FromBody] FlightPathRequest request)
    {
        try
        {
            var hasViolation = await _gisService.CheckNoFlyZoneViolationAsync(
                request.FlightPathGeoJson,
                request.OperationDate,
                request.AltitudeAMSL);
            
            return Ok(new
            {
                hasViolation,
                operationDate = request.OperationDate,
                altitudeAMSL = request.AltitudeAMSL,
                checkedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check no-fly zone violation");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Creates a new no-fly zone (admin only).</summary>
    [HttpPost("no-fly-zones")]
    public async Task<ActionResult<NoFlyZone>> CreateNoFlyZone([FromBody] NoFlyZone request)
    {
        try
        {
            var zone = await _gisService.CreateNoFlyZoneAsync(request);
            return CreatedAtAction(nameof(GetNoFlyZones), null, zone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create no-fly zone");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // --- Terrain Elevation Endpoints ---

    /// <summary>Gets terrain elevation (AMSL) for a specific point.</summary>
    [HttpGet("terrain-elevation")]
    public async Task<ActionResult<object>> GetTerrainElevation(
        [FromQuery] double lat,
        [FromQuery] double lon)
    {
        try
        {
            var elevation = await _gisService.GetTerrainElevationAsync(lat, lon);
            return Ok(new
            {
                latitude = lat,
                longitude = lon,
                elevationAMSL = elevation,
                source = "Demo",
                queriedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get terrain elevation at ({Lat},{Lon})", lat, lon);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Gets terrain elevation profile for waypoints.</summary>
    [HttpPost("terrain-elevation/profile")]
    public async Task<ActionResult<List<TerrainElevation>>> GetTerrainProfile(
        [FromBody] List<WaypointRequest> waypoints)
    {
        try
        {
            var coords = waypoints.Select(w => (w.Latitude, w.Longitude)).ToList();
            var profile = await _gisService.GetTerrainProfileAsync(coords);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get terrain profile");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// --- Request DTOs ---

public record FlightPathRequest(string FlightPathGeoJson, DateTime OperationDate, double AltitudeAMSL);
public record WaypointRequest(double Latitude, double Longitude);
