// Phase 6 Step 51 — Skyworks V5
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GIS;

/// <summary>
/// Represents a 3D operational volume for UAS operations (OSO #17 compliance).
/// Supports dual SORA versions: EASA AMC 2.0 and JARUS 2.5.
/// </summary>
public class OperationalVolume
{
    /// <summary>Unique identifier for the operational volume.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Name/description of the operational volume.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>GeoJSON geometry representing the horizontal boundaries (2D polygon).</summary>
    /// <remarks>
    /// Format: GeoJSON Polygon with coordinates as [longitude, latitude] pairs.
    /// Example: {"type":"Polygon","coordinates":[[[lon1,lat1],[lon2,lat2],...]]}
    /// </remarks>
    public string GeometryGeoJson { get; set; } = string.Empty;

    /// <summary>Minimum altitude Above Ground Level (AGL) in meters.</summary>
    public double MinAltitudeAGL { get; set; }

    /// <summary>Maximum altitude Above Ground Level (AGL) in meters.</summary>
    public double MaxAltitudeAGL { get; set; }

    /// <summary>Minimum altitude Above Mean Sea Level (AMSL) in meters.</summary>
    public double MinAltitudeAMSL { get; set; }

    /// <summary>Maximum altitude Above Mean Sea Level (AMSL) in meters.</summary>
    public double MaxAltitudeAMSL { get; set; }

    /// <summary>Horizontal safety margin in meters (GPS/GNSS accuracy buffer).</summary>
    public double SafetyMarginHorizontal { get; set; } = 10.0;

    /// <summary>Vertical safety margin in meters.</summary>
    public double SafetyMarginVertical { get; set; } = 5.0;

    /// <summary>SORA version for compliance ("2.0" or "2.5").</summary>
    public string SoraVersion { get; set; } = "2.5";

    /// <summary>Volume type: operational, contingency, buffer.</summary>
    public string VolumeType { get; set; } = "operational";

    /// <summary>Contingency zones (emergency containment areas) as GeoJSON Points.</summary>
    public List<string> ContingencyZonesGeoJson { get; set; } = new();

    /// <summary>Associated mission ID.</summary>
    public Guid? MissionId { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Additional metadata (flexible key-value pairs).</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Metadata { get; set; }
}
