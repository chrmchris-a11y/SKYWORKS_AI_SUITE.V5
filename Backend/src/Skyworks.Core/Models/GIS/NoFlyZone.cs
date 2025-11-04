// Phase 6 Step 51 — Skyworks V5
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GIS;

/// <summary>
/// Represents a no-fly zone (permanent, temporary, or conditional restriction).
/// Supports OSO #18 compliance (adjacency to controlled ground areas).
/// </summary>
public class NoFlyZone
{
    /// <summary>Unique identifier for the no-fly zone.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Zone name or description.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>GeoJSON geometry (Polygon or MultiPolygon).</summary>
    public string GeometryGeoJson { get; set; } = string.Empty;

    /// <summary>Restriction type: "permanent", "temporary", "conditional".</summary>
    public string RestrictionType { get; set; } = "permanent";

    /// <summary>Category: "airport", "military", "critical_infrastructure", "event", "weather", "privacy", "protected_area".</summary>
    public string Category { get; set; } = "airport";

    /// <summary>Severity: "prohibited" (no flight allowed), "restricted" (authorization required), "caution" (advisory).</summary>
    public string Severity { get; set; } = "prohibited";

    /// <summary>Valid from timestamp (null for permanent restrictions).</summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>Valid to timestamp (null for permanent restrictions).</summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>Minimum altitude restriction in meters AMSL (null = ground level).</summary>
    public double? AltitudeRestrictionMin { get; set; }

    /// <summary>Maximum altitude restriction in meters AMSL (null = unlimited).</summary>
    public double? AltitudeRestrictionMax { get; set; }

    /// <summary>Indicates if geofence enforcement is active (hard geofence).</summary>
    public bool EnforceGeofence { get; set; } = true;

    /// <summary>NOTAM reference number (for temporary restrictions).</summary>
    public string? NOTAMReference { get; set; }

    /// <summary>Authority issuing the restriction (e.g., "DCA Cyprus", "EASA", "Local CAA").</summary>
    public string Authority { get; set; } = "Unknown";

    /// <summary>Contact information for authorization requests.</summary>
    public string? ContactInfo { get; set; }

    /// <summary>Reason for restriction.</summary>
    public string? Reason { get; set; }

    /// <summary>Data source.</summary>
    public string DataSource { get; set; } = "Unknown";

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Additional metadata.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Metadata { get; set; }
}
