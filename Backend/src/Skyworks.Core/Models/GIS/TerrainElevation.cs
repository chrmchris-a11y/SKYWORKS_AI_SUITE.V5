// Phase 6 Step 51 — Skyworks V5
using System;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GIS;

/// <summary>
/// Represents terrain elevation at a specific geographic point.
/// Used for AGL/AMSL conversion and obstacle avoidance.
/// </summary>
public class TerrainElevation
{
    /// <summary>Latitude in decimal degrees (-90 to 90).</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude in decimal degrees (-180 to 180).</summary>
    public double Longitude { get; set; }

    /// <summary>Elevation Above Mean Sea Level (AMSL) in meters.</summary>
    public double ElevationAMSL { get; set; }

    /// <summary>Data source (e.g., "SRTM", "EU-DEM", "ASTER_GDEM").</summary>
    public string Source { get; set; } = "SRTM";

    /// <summary>Vertical accuracy in meters (e.g., ±10m for SRTM).</summary>
    public double Accuracy { get; set; } = 10.0;

    /// <summary>Horizontal resolution in meters (e.g., 30m for SRTM).</summary>
    public double ResolutionMeters { get; set; } = 30.0;

    /// <summary>Data collection timestamp.</summary>
    public DateTime DataCollectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Indicates if this is an interpolated value.</summary>
    public bool IsInterpolated { get; set; } = false;
}
