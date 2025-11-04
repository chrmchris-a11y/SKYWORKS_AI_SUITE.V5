namespace Skyworks.Core.Services.GRC;

/// <summary>
/// Interface for retrieving population density data for SORA 2.5 Table 3 classifications
/// </summary>
public interface IPopulationDensityProvider
{
    /// <summary>
    /// Get population density (people/km²) for a given location
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees</param>
    /// <param name="longitude">Longitude in decimal degrees</param>
    /// <returns>Population density in people per km², or null if data unavailable</returns>
    Task<double?> GetDensityAsync(double latitude, double longitude);

    /// <summary>
    /// Get population density (people/km²) for a polygon/operational volume
    /// Returns the maximum density within the area (conservative approach)
    /// </summary>
    Task<double?> GetMaxDensityInAreaAsync(IEnumerable<(double lat, double lon)> boundingPolygon);
}
