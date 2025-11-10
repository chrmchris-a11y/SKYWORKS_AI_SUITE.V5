using System.Threading.Tasks;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Service for auto-generating mission geometry (waypoints, routes, CGA, geofence).
/// Generates patterns based on MissionType: Facade, Roof, Solar, Linear, Agriculture, Custom.
/// </summary>
public interface IMissionGeometryService
{
    /// <summary>
    /// Generate mission geometry from request parameters
    /// </summary>
    /// <param name="request">Geometry generation request with mission type and area parameters</param>
    /// <returns>MissionGeometry with GeoJSON, route length, CGA area, max height</returns>
    /// <exception cref="System.InvalidOperationException">If Custom mission requested without imported geometry</exception>
    Task<MissionGeometry> GenerateAsync(GeometryGenerationRequest request);
}
