using Skyworks.Core.Services.GRC;

namespace Skyworks.Infrastructure.Services;

/// <summary>
/// Static/demo population density provider using approximate EU density zones
/// TODO: Replace with real GIS service (e.g. WorldPop, Eurostat GISCO, commercial API)
/// </summary>
public class StaticPopulationDensityProvider : IPopulationDensityProvider
{
    // Demo static data: lat/lon bounding boxes with rough density estimates (people/km²)
    // Based on major European urban/rural zones for demonstration
    private readonly List<DensityZone> _zones = new()
    {
        // Athens metropolitan area
        new DensityZone(37.9, 38.1, 23.6, 23.9, 17000),
        // Athens suburbs
        new DensityZone(37.8, 38.2, 23.5, 24.0, 5000),
        // Madrid
        new DensityZone(40.3, 40.5, -3.8, -3.6, 14000),
        // Paris
        new DensityZone(48.8, 48.9, 2.2, 2.4, 21000),
        // London
        new DensityZone(51.4, 51.6, -0.2, 0.1, 15000),
        // Berlin
        new DensityZone(52.4, 52.6, 13.3, 13.5, 12000),
        // Rome
        new DensityZone(41.8, 42.0, 12.4, 12.6, 8500),
        // Rural Greece (Peloponnese)
        new DensityZone(36.5, 38.0, 21.5, 23.0, 50),
        // Rural Spain (Castile)
        new DensityZone(40.0, 42.0, -5.0, -3.0, 30),
        // Rural France (Massif Central)
        new DensityZone(44.5, 46.0, 2.0, 4.0, 35),
        // Default European suburban/peri-urban fallback
        new DensityZone(35.0, 70.0, -10.0, 30.0, 300)
    };

    public Task<double?> GetDensityAsync(double latitude, double longitude)
    {
        // Find first matching zone (ordered by specificity: urban > suburban > rural > default)
        var zone = _zones.FirstOrDefault(z =>
            latitude >= z.LatMin && latitude <= z.LatMax &&
            longitude >= z.LonMin && longitude <= z.LonMax);

        return Task.FromResult<double?>(zone?.Density);
    }

    public Task<double?> GetMaxDensityInAreaAsync(IEnumerable<(double lat, double lon)> boundingPolygon)
    {
        // Conservative: sample multiple points and return max
        var points = boundingPolygon.ToList();
        if (!points.Any())
            return Task.FromResult<double?>(null);

        // Sample the bounding box with a simple grid
        var lats = points.Select(p => p.lat).ToList();
        var lons = points.Select(p => p.lon).ToList();
        double latMin = lats.Min(), latMax = lats.Max();
        double lonMin = lons.Min(), lonMax = lons.Max();

        var samples = new List<double>();
        for (double lat = latMin; lat <= latMax; lat += (latMax - latMin) / 5.0)
        {
            for (double lon = lonMin; lon <= lonMax; lon += (lonMax - lonMin) / 5.0)
            {
                var d = GetDensityAsync(lat, lon).Result;
                if (d.HasValue)
                    samples.Add(d.Value);
            }
        }

        return Task.FromResult<double?>(samples.Any() ? samples.Max() : null);
    }

    private record DensityZone(double LatMin, double LatMax, double LonMin, double LonMax, double Density);
}
