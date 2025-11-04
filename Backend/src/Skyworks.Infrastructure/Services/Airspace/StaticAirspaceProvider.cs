using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Services.Airspace;
using Skyworks.Core.Utils.Geometry;

namespace Skyworks.Infrastructure.Services;

/// <summary>
/// Στατικός demo provider με 2-3 αντιπροσωπευτικά πολύγωνα (π.χ. CTR/ATZ στην Αττική).
/// Δεν αντικαθιστά επίσημα AIXM δεδομένα· απλώς για ανάπτυξη της μηχανής ARC.
/// </summary>
public class StaticAirspaceProvider : IAirspaceProvider
{
    private readonly List<AirspaceInfo> _airspaces;

    public StaticAirspaceProvider()
    {
        _airspaces = BuildDemoAirspaces();
    }

    public Task<AirspaceQueryResult> GetAirspacesAtPointAsync(GeoPoint point, CancellationToken ct = default)
    {
        var matched = _airspaces.Where(a =>
            GeometryUtils.IsPointInPolygon(point, a.Boundary) &&
            GeometryUtils.AltitudeWithinBand(point.AltitudeMeters, a.LowerLimitMeters, a.UpperLimitMeters))
            .ToList();

        return Task.FromResult(new AirspaceQueryResult
        {
            Airspaces = matched,
            Notes = "Static demo provider: approximate polygons"
        });
    }

    public Task<List<AirspaceInfo>> GetAirspacesIntersectingPolygonAsync(IEnumerable<GeoPoint> polygon, double? minAltitudeMeters = null, double? maxAltitudeMeters = null, CancellationToken ct = default)
    {
        var poly = polygon.ToList();
        var centroid = new GeoPoint
        {
            Latitude = poly.Average(p => p.Latitude),
            Longitude = poly.Average(p => p.Longitude),
            AltitudeMeters = maxAltitudeMeters
        };
        // Simple heuristic: return airspaces that contain the centroid and overlap altitude band
        var matched = _airspaces.Where(a =>
            GeometryUtils.IsPointInPolygon(centroid, a.Boundary) &&
            !(a.UpperLimitMeters.HasValue && minAltitudeMeters.HasValue && a.UpperLimitMeters.Value < minAltitudeMeters.Value) &&
            !(a.LowerLimitMeters.HasValue && maxAltitudeMeters.HasValue && a.LowerLimitMeters.Value > maxAltitudeMeters.Value))
            .ToList();
        return Task.FromResult(matched);
    }

    private static List<AirspaceInfo> BuildDemoAirspaces()
    {
        var list = new List<AirspaceInfo>();

        // Demo CTR Athens (LGAV) — very rough rectangle
        list.Add(new AirspaceInfo
        {
            Id = "CTR_LGAV",
            Name = "Athens CTR (demo)",
            Type = AirspaceType.CTR,
            Class = AirspaceClass.D,
            LowerLimitMeters = 0,
            UpperLimitMeters = 2500,
            Notes = "Approximate polygon for demo only",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 37.85, Longitude = 23.70 },
                new GeoPoint{ Latitude = 37.85, Longitude = 24.10 },
                new GeoPoint{ Latitude = 38.10, Longitude = 24.10 },
                new GeoPoint{ Latitude = 38.10, Longitude = 23.70 },
            }
        });

        // Demo ATZ Megara (LGMG) — small box
        list.Add(new AirspaceInfo
        {
            Id = "ATZ_LGMG",
            Name = "Megara ATZ (demo)",
            Type = AirspaceType.ATZ,
            Class = AirspaceClass.G,
            LowerLimitMeters = 0,
            UpperLimitMeters = 900, // ~3000 ft
            Notes = "Approximate polygon for demo only",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 38.05, Longitude = 23.30 },
                new GeoPoint{ Latitude = 38.05, Longitude = 23.40 },
                new GeoPoint{ Latitude = 38.12, Longitude = 23.40 },
                new GeoPoint{ Latitude = 38.12, Longitude = 23.30 },
            }
        });

        // Demo Restricted Area — small patch
        list.Add(new AirspaceInfo
        {
            Id = "R_TEST",
            Name = "Restricted Area R-TEST (demo)",
            Type = AirspaceType.Restricted,
            Class = null,
            LowerLimitMeters = 300,
            UpperLimitMeters = 1200,
            Notes = "Approximate polygon for demo only",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 38.00, Longitude = 23.90 },
                new GeoPoint{ Latitude = 38.00, Longitude = 24.00 },
                new GeoPoint{ Latitude = 38.06, Longitude = 24.00 },
                new GeoPoint{ Latitude = 38.06, Longitude = 23.90 },
            }
        });

        // ------------------------------
        // Cyprus (approximate demo polygons)
        // ------------------------------
        // Larnaca CTR (LCLK) — rough rectangle covering Larnaca area
        list.Add(new AirspaceInfo
        {
            Id = "CTR_LCLK",
            Name = "Larnaca CTR (demo)",
            Type = AirspaceType.CTR,
            Class = AirspaceClass.D,
            LowerLimitMeters = 0,
            UpperLimitMeters = 2500,
            Notes = "Approximate polygon for demo only (CY)",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 34.80, Longitude = 33.50 },
                new GeoPoint{ Latitude = 34.80, Longitude = 33.75 },
                new GeoPoint{ Latitude = 34.96, Longitude = 33.75 },
                new GeoPoint{ Latitude = 34.96, Longitude = 33.50 },
            }
        });

        // Paphos CTR (LCPH)
        list.Add(new AirspaceInfo
        {
            Id = "CTR_LCPH",
            Name = "Paphos CTR (demo)",
            Type = AirspaceType.CTR,
            Class = AirspaceClass.D,
            LowerLimitMeters = 0,
            UpperLimitMeters = 2500,
            Notes = "Approximate polygon for demo only (CY)",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 34.60, Longitude = 32.35 },
                new GeoPoint{ Latitude = 34.60, Longitude = 32.65 },
                new GeoPoint{ Latitude = 34.85, Longitude = 32.65 },
                new GeoPoint{ Latitude = 34.85, Longitude = 32.35 },
            }
        });

        // Akrotiri CTR (LCRA) — covers Limassol/RAF Akrotiri vicinity
        list.Add(new AirspaceInfo
        {
            Id = "CTR_LCRA",
            Name = "Akrotiri CTR (demo)",
            Type = AirspaceType.CTR,
            Class = AirspaceClass.D,
            LowerLimitMeters = 0,
            UpperLimitMeters = 2500,
            Notes = "Approximate polygon for demo only (CY)",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 34.55, Longitude = 32.90 },
                new GeoPoint{ Latitude = 34.55, Longitude = 33.08 },
                new GeoPoint{ Latitude = 34.73, Longitude = 33.08 },
                new GeoPoint{ Latitude = 34.73, Longitude = 32.90 },
            }
        });

        // Larnaca TMA — larger box (higher floor)
        list.Add(new AirspaceInfo
        {
            Id = "TMA_LCLK",
            Name = "Larnaca TMA (demo)",
            Type = AirspaceType.TMA,
            Class = AirspaceClass.C,
            LowerLimitMeters = 900,  // ~3000 ft
            UpperLimitMeters = 5000, // ~FL165
            Notes = "Approximate polygon for demo only (CY)",
            Boundary = new List<GeoPoint>
            {
                new GeoPoint{ Latitude = 34.70, Longitude = 33.35 },
                new GeoPoint{ Latitude = 34.70, Longitude = 33.95 },
                new GeoPoint{ Latitude = 35.10, Longitude = 33.95 },
                new GeoPoint{ Latitude = 35.10, Longitude = 33.35 },
            }
        });

        return list;
    }
}
