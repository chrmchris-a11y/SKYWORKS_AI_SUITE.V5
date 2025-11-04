using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Models.Traffic;

/// <summary>
/// Traffic target (aircraft, drone, helicopter) από ADS-B/FLARM/OpenSky
/// </summary>
public class TrafficTarget
{
    /// <summary>
    /// Target ID (e.g., ICAO24 hex code για ADS-B)
    /// </summary>
    public required string TargetId { get; set; }

    /// <summary>
    /// Callsign (if available)
    /// </summary>
    public string? Callsign { get; set; }

    /// <summary>
    /// Position (lat/lon/altitude)
    /// </summary>
    public required GeoPoint Position { get; set; }

    /// <summary>
    /// Velocity (m/s)
    /// </summary>
    public double? VelocityMps { get; set; }

    /// <summary>
    /// Track/Heading (degrees, 0-360)
    /// </summary>
    public double? TrackDegrees { get; set; }

    /// <summary>
    /// Vertical rate (m/s, positive=climbing)
    /// </summary>
    public double? VerticalRateMps { get; set; }

    /// <summary>
    /// Target type: "Aircraft", "Helicopter", "Drone", "Glider", "Unknown"
    /// </summary>
    public string TargetType { get; set; } = "Unknown";

    /// <summary>
    /// Distance to query point (meters) - computed field
    /// </summary>
    public double? DistanceMeters { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdateUtc { get; set; }

    /// <summary>
    /// Data source: "ADS-B", "FLARM", "OpenSky", "Synthetic"
    /// </summary>
    public string Source { get; set; } = "Synthetic";
}

/// <summary>
/// Traffic density για μια περιοχή
/// </summary>
public class TrafficDensity
{
    /// <summary>
    /// Center position of query area
    /// </summary>
    public required GeoPoint Center { get; set; }

    /// <summary>
    /// Radius (meters) of query area
    /// </summary>
    public required double RadiusMeters { get; set; }

    /// <summary>
    /// Total number of targets detected
    /// </summary>
    public int TotalTargets { get; set; }

    /// <summary>
    /// Targets by type breakdown
    /// </summary>
    public Dictionary<string, int> TargetsByType { get; set; } = new();

    /// <summary>
    /// Density classification: "Low", "Medium", "High", "Very High"
    /// </summary>
    public string DensityLevel { get; set; } = "Low";

    /// <summary>
    /// Average altitude of traffic (meters MSL)
    /// </summary>
    public double? AverageAltitudeMsl { get; set; }

    /// <summary>
    /// Timestamp of density snapshot
    /// </summary>
    public DateTime SnapshotTime { get; set; }

    /// <summary>
    /// Notes (e.g., "CTR active", "High GA traffic")
    /// </summary>
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Request για traffic query
/// </summary>
public class TrafficQuery
{
    /// <summary>
    /// Center position of search area
    /// </summary>
    public required GeoPoint Position { get; set; }

    /// <summary>
    /// Search radius (meters)
    /// </summary>
    public double RadiusMeters { get; set; } = 5000; // Default 5km

    /// <summary>
    /// Minimum altitude filter (meters MSL, optional)
    /// </summary>
    public double? MinAltitudeMsl { get; set; }

    /// <summary>
    /// Maximum altitude filter (meters MSL, optional)
    /// </summary>
    public double? MaxAltitudeMsl { get; set; }

    /// <summary>
    /// Filter by target type (optional)
    /// </summary>
    public List<string>? TargetTypes { get; set; }

    /// <summary>
    /// Include historical data (για trend analysis)
    /// </summary>
    public bool IncludeHistorical { get; set; } = false;
}

/// <summary>
/// Response από traffic provider
/// </summary>
public class TrafficQueryResult
{
    /// <summary>
    /// Traffic density summary
    /// </summary>
    public required TrafficDensity Density { get; set; }

    /// <summary>
    /// List of individual targets (up to MaxTargets limit)
    /// </summary>
    public List<TrafficTarget> Targets { get; set; } = new();

    /// <summary>
    /// Risk assessment notes for ARC integration
    /// </summary>
    public List<string> RiskNotes { get; set; } = new();

    /// <summary>
    /// Data timestamp
    /// </summary>
    public DateTime QueryTime { get; set; }
}

/// <summary>
/// Traffic density thresholds για classification
/// </summary>
public static class TrafficDensityThresholds
{
    /// <summary>
    /// Low density: 0-2 targets per 5km radius
    /// </summary>
    public const int LowThreshold = 2;

    /// <summary>
    /// Medium density: 3-5 targets per 5km radius
    /// </summary>
    public const int MediumThreshold = 5;

    /// <summary>
    /// High density: 6-10 targets per 5km radius
    /// </summary>
    public const int HighThreshold = 10;

    /// <summary>
    /// Very High density: >10 targets per 5km radius
    /// </summary>
    public const int VeryHighThreshold = 10;

    /// <summary>
    /// Classify density level based on target count
    /// </summary>
    public static string ClassifyDensity(int targetCount)
    {
        if (targetCount <= LowThreshold) return "Low";
        if (targetCount <= MediumThreshold) return "Medium";
        if (targetCount <= HighThreshold) return "High";
        return "Very High";
    }

    /// <summary>
    /// Get risk note based on density
    /// </summary>
    public static string GetRiskNote(string densityLevel)
    {
        return densityLevel switch
        {
            "Very High" => "Very high air traffic density detected. Enhanced vigilance and TMPR required.",
            "High" => "High air traffic density. Consider tactical mitigations (DAA, see-and-avoid).",
            "Medium" => "Moderate air traffic. Standard TMPR applies.",
            "Low" => "Low air traffic density.",
            _ => "Unknown traffic density."
        };
    }
}
