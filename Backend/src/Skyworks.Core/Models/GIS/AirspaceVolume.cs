// Phase 6 Step 51 — Skyworks V5
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GIS;

/// <summary>
/// Represents a 3D airspace volume with classification and ARC determination data.
/// Supports dual SORA versions for ARC calculation (12 AEC categories).
/// </summary>
public class AirspaceVolume
{
    /// <summary>Unique identifier for the airspace volume.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Airspace name (e.g., "CTR Larnaca", "TMA Nicosia").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>GeoJSON geometry (Polygon or MultiPolygon).</summary>
    public string GeometryGeoJson { get; set; } = string.Empty;

    /// <summary>Airspace class (A, B, C, D, E, F, G).</summary>
    public string AirspaceClass { get; set; } = "G";

    /// <summary>Floor altitude in meters (AMSL or AGL based on AltitudeReference).</summary>
    public double FloorAltitude { get; set; }

    /// <summary>Ceiling altitude in meters (AMSL or AGL based on AltitudeReference).</summary>
    public double CeilingAltitude { get; set; }

    /// <summary>Altitude reference: "AMSL" (Above Mean Sea Level) or "AGL" (Above Ground Level).</summary>
    public string AltitudeReference { get; set; } = "AMSL";

    /// <summary>Indicates if ATC service is available.</summary>
    public bool ATCServiceAvailable { get; set; } = false;

    /// <summary>Indicates if Mode S transponder is required (TMZ - Transponder Mandatory Zone).</summary>
    public bool ModeS_Required { get; set; } = false;

    /// <summary>Indicates if radio communication is required (RMZ - Radio Mandatory Zone).</summary>
    public bool Radio_Required { get; set; } = false;

    /// <summary>Environment classification: "Urban", "Suburban", "Rural".</summary>
    public string Environment { get; set; } = "Rural";

    /// <summary>Special zone type (if applicable): "TMZ", "RMZ", "TSA", "TRA", "D", "R", "P".</summary>
    public string? SpecialZoneType { get; set; }

    // --- ARC Determination (Dual SORA) ---

    /// <summary>Initial ARC (before strategic mitigations): "ARC-a", "ARC-b", "ARC-c", "ARC-d".</summary>
    public string InitialARC { get; set; } = "ARC-b";

    /// <summary>AEC (Airspace Encounter Category) number (1-12 from SORA decision tree).</summary>
    public int AECCategory { get; set; } = 10;

    /// <summary>Manned aircraft density (1-5, used in ARC determination).</summary>
    public int MannedAircraftDensity { get; set; } = 1;

    /// <summary>Data source (e.g., "EAD", "FAA_NASR", "ICAO").</summary>
    public string DataSource { get; set; } = "Unknown";

    /// <summary>Last data update timestamp.</summary>
    public DateTime DataUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Additional metadata.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? Metadata { get; set; }
}
