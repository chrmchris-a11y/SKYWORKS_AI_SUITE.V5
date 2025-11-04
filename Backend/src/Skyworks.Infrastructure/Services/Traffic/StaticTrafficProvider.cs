using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Traffic;
using Skyworks.Core.Services.Traffic;

namespace Skyworks.Infrastructure.Services.Traffic;

/// <summary>
/// Static traffic provider με synthetic demo data
/// Simulates ADS-B traffic για Cyprus και Athens regions
/// </summary>
public class StaticTrafficProvider : ITrafficProvider
{
    private readonly List<TrafficTarget> _demoTargets;

    public StaticTrafficProvider()
    {
        _demoTargets = LoadDemoTraffic();
    }

    public Task<TrafficQueryResult> GetTrafficDensityAsync(TrafficQuery query)
    {
        var targets = GetNearbyTargetsSync(query);

        var density = new TrafficDensity
        {
            Center = query.Position,
            RadiusMeters = query.RadiusMeters,
            TotalTargets = targets.Count,
            SnapshotTime = DateTime.UtcNow
        };

        // Count by type
        density.TargetsByType = targets
            .GroupBy(t => t.TargetType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Average altitude
        if (targets.Any(t => t.Position.AltitudeMeters.HasValue))
        {
            density.AverageAltitudeMsl = targets
                .Where(t => t.Position.AltitudeMeters.HasValue)
                .Average(t => t.Position.AltitudeMeters!.Value);
        }

        // Classify density
        density.DensityLevel = TrafficDensityThresholds.ClassifyDensity(targets.Count);

        // Add notes
        if (density.TotalTargets > 0)
        {
            density.Notes.Add($"{density.TotalTargets} traffic target(s) detected within {query.RadiusMeters}m radius");
        }

        if (density.DensityLevel == "High" || density.DensityLevel == "Very High")
        {
            density.Notes.Add("Elevated air traffic density - enhanced vigilance recommended");
        }

        var result = new TrafficQueryResult
        {
            Density = density,
            Targets = targets.Take(50).ToList(), // Limit to 50 targets
            QueryTime = DateTime.UtcNow
        };

        // Risk notes for ARC integration
        result.RiskNotes.Add(TrafficDensityThresholds.GetRiskNote(density.DensityLevel));

        if (density.TotalTargets > 5)
        {
            result.RiskNotes.Add("Consider TMPR enhancement: DAA system recommended for high-density airspace");
        }

        return Task.FromResult(result);
    }

    public Task<List<TrafficTarget>> GetNearbyTargetsAsync(TrafficQuery query)
    {
        var targets = GetNearbyTargetsSync(query);
        return Task.FromResult(targets);
    }

    #region Private Helpers

    private List<TrafficTarget> GetNearbyTargetsSync(TrafficQuery query)
    {
        var targets = new List<TrafficTarget>();

        foreach (var target in _demoTargets)
        {
            var distance = CalculateDistance(query.Position, target.Position);
            
            if (distance > query.RadiusMeters)
                continue;

            // Altitude filters
            if (query.MinAltitudeMsl.HasValue && target.Position.AltitudeMeters < query.MinAltitudeMsl)
                continue;

            if (query.MaxAltitudeMsl.HasValue && target.Position.AltitudeMeters > query.MaxAltitudeMsl)
                continue;

            // Type filter
            if (query.TargetTypes != null && query.TargetTypes.Any() && 
                !query.TargetTypes.Contains(target.TargetType))
                continue;

            // Clone and add distance
            var targetCopy = new TrafficTarget
            {
                TargetId = target.TargetId,
                Callsign = target.Callsign,
                Position = target.Position,
                VelocityMps = target.VelocityMps,
                TrackDegrees = target.TrackDegrees,
                VerticalRateMps = target.VerticalRateMps,
                TargetType = target.TargetType,
                DistanceMeters = distance,
                LastUpdateUtc = target.LastUpdateUtc,
                Source = target.Source
            };

            targets.Add(targetCopy);
        }

        // Sort by distance
        return targets.OrderBy(t => t.DistanceMeters).ToList();
    }

    private List<TrafficTarget> LoadDemoTraffic()
    {
        var targets = new List<TrafficTarget>();
        var now = DateTime.UtcNow;

        // Cyprus Region Traffic
        // 1. Commercial flight approaching LCLK (Larnaca)
        targets.Add(new TrafficTarget
        {
            TargetId = "4CA9A2",
            Callsign = "CYP123",
            Position = new GeoPoint { Latitude = 34.9, Longitude = 33.7, AltitudeMeters = 2500 },
            VelocityMps = 72.2, // ~140 knots
            TrackDegrees = 95,
            VerticalRateMps = -2.5, // Descending
            TargetType = "Aircraft",
            LastUpdateUtc = now.AddSeconds(-5),
            Source = "ADS-B"
        });

        // 2. GA aircraft near Paphos (LCPH)
        targets.Add(new TrafficTarget
        {
            TargetId = "4CA123",
            Callsign = "5BFLY",
            Position = new GeoPoint { Latitude = 34.72, Longitude = 32.48, AltitudeMeters = 1200 },
            VelocityMps = 51.4, // ~100 knots
            TrackDegrees = 180,
            VerticalRateMps = 0,
            TargetType = "Aircraft",
            LastUpdateUtc = now.AddSeconds(-3),
            Source = "ADS-B"
        });

        // 3. Helicopter near Limassol
        targets.Add(new TrafficTarget
        {
            TargetId = "4CA456",
            Callsign = "HELI01",
            Position = new GeoPoint { Latitude = 34.7, Longitude = 33.05, AltitudeMeters = 300 },
            VelocityMps = 41.2, // ~80 knots
            TrackDegrees = 45,
            VerticalRateMps = 0,
            TargetType = "Helicopter",
            LastUpdateUtc = now.AddSeconds(-2),
            Source = "ADS-B"
        });

        // 4. Another drone (simulated) near service zone
        targets.Add(new TrafficTarget
        {
            TargetId = "DRONE01",
            Position = new GeoPoint { Latitude = 34.68, Longitude = 33.04, AltitudeMeters = 80 },
            VelocityMps = 10.3, // ~20 knots
            TrackDegrees = 270,
            VerticalRateMps = 0.5,
            TargetType = "Drone",
            LastUpdateUtc = now.AddSeconds(-1),
            Source = "Synthetic"
        });

        // Athens Region Traffic
        // 5. Commercial at LGAV (Athens)
        targets.Add(new TrafficTarget
        {
            TargetId = "468A12",
            Callsign = "AEE345",
            Position = new GeoPoint { Latitude = 37.95, Longitude = 23.95, AltitudeMeters = 3000 },
            VelocityMps = 102.8, // ~200 knots
            TrackDegrees = 120,
            VerticalRateMps = -3.5,
            TargetType = "Aircraft",
            LastUpdateUtc = now.AddSeconds(-4),
            Source = "ADS-B"
        });

        // 6. GA training flight near Athens
        targets.Add(new TrafficTarget
        {
            TargetId = "468B45",
            Callsign = "SXFLY",
            Position = new GeoPoint { Latitude = 37.88, Longitude = 23.73, AltitudeMeters = 800 },
            VelocityMps = 46.3, // ~90 knots
            TrackDegrees = 90,
            VerticalRateMps = 1.0,
            TargetType = "Aircraft",
            LastUpdateUtc = now.AddSeconds(-6),
            Source = "ADS-B"
        });

        // 7. Glider (sailplane)
        targets.Add(new TrafficTarget
        {
            TargetId = "GLIDE1",
            Position = new GeoPoint { Latitude = 34.85, Longitude = 33.5, AltitudeMeters = 1500 },
            VelocityMps = 25.7, // ~50 knots
            TrackDegrees = 315,
            VerticalRateMps = 2.0, // Thermalling
            TargetType = "Glider",
            LastUpdateUtc = now.AddSeconds(-8),
            Source = "FLARM"
        });

        // 8. High-altitude cruise (out of typical drone airspace)
        targets.Add(new TrafficTarget
        {
            TargetId = "4CA999",
            Callsign = "WZZ1234",
            Position = new GeoPoint { Latitude = 35.0, Longitude = 33.3, AltitudeMeters = 10500 },
            VelocityMps = 128.6, // ~250 knots
            TrackDegrees = 270,
            VerticalRateMps = 0,
            TargetType = "Aircraft",
            LastUpdateUtc = now.AddSeconds(-10),
            Source = "ADS-B"
        });

        return targets;
    }

    private double CalculateDistance(GeoPoint p1, GeoPoint p2)
    {
        // Simple Euclidean distance approximation (good enough for small areas)
        const double metersPerDegreeLat = 111320.0;
        var metersPerDegreeLon = 111320.0 * Math.Cos(p1.Latitude * Math.PI / 180.0);

        var dx = (p2.Longitude - p1.Longitude) * metersPerDegreeLon;
        var dy = (p2.Latitude - p1.Latitude) * metersPerDegreeLat;

        return Math.Sqrt(dx * dx + dy * dy);
    }

    #endregion
}
