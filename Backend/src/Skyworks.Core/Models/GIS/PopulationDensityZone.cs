// Phase 6 Step 51 — Skyworks V5
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GIS;

/// <summary>
/// Represents a geographic zone with population density information for GRC calculation.
/// Supports dual SORA versions: EASA AMC 2.0 (scenario-based) and JARUS 2.5 (density-based).
/// </summary>
public class PopulationDensityZone
{
    /// <summary>Unique identifier for the zone.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Zone name or description.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>GeoJSON geometry (Polygon or MultiPolygon).</summary>
    public string GeometryGeoJson { get; set; } = string.Empty;

    /// <summary>Population density in people per square kilometer.</summary>
    public double PopulationDensity { get; set; }

    /// <summary>Data source (e.g., "Eurostat", "WorldPop", "Census").</summary>
    public string DataSource { get; set; } = "Unknown";

    /// <summary>Data resolution in meters.</summary>
    public double ResolutionMeters { get; set; } = 10.0;

    /// <summary>Last data update timestamp.</summary>
    public DateTime DataUpdatedAt { get; set; } = DateTime.UtcNow;

    // --- SORA 2.0 Properties (Scenario-based) ---

    /// <summary>SORA 2.0 operational scenario (e.g., "VLOS_Controlled", "BVLOS_Sparsely", "VLOS_Populated").</summary>
    public string? Scenario_2_0 { get; set; }

    /// <summary>SORA 2.0 Intrinsic GRC (1-10, from Table 2).</summary>
    public int? IntrinsicGRC_2_0 { get; set; }

    // --- SORA 2.5 Properties (Density-based) ---

    /// <summary>
    /// SORA 2.5 population density category:
    /// "Controlled" (0), "&lt;5", "&lt;50", "&lt;500", "&lt;5000", "&lt;50000", "&gt;50000"
    /// </summary>
    public string? Category_2_5 { get; set; }

    /// <summary>SORA 2.5 Intrinsic GRC (1-10, from Table 2).</summary>
    public int? IntrinsicGRC_2_5 { get; set; }

    // --- Common Properties ---

    /// <summary>Sheltering factor (0.0-1.0, based on building density and roof strength).</summary>
    public double ShelteringFactor { get; set; } = 0.0;

    /// <summary>Indicates if M1 (2.0) or M1A/M1B/M1C (2.5) strategic mitigations are applicable.</summary>
    public bool M1Applicability { get; set; } = true;

    /// <summary>Additional metadata.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Metadata { get; set; }
}
