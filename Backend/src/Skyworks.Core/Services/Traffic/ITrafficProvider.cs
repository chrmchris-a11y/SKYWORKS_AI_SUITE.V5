using Skyworks.Core.Models.Traffic;

namespace Skyworks.Core.Services.Traffic;

/// <summary>
/// Traffic provider interface για air traffic data (ADS-B, FLARM, OpenSky, etc.)
/// </summary>
public interface ITrafficProvider
{
    /// <summary>
    /// Get traffic density for a specific area
    /// </summary>
    Task<TrafficQueryResult> GetTrafficDensityAsync(TrafficQuery query);

    /// <summary>
    /// Get nearby traffic targets within radius
    /// </summary>
    Task<List<TrafficTarget>> GetNearbyTargetsAsync(TrafficQuery query);
}
