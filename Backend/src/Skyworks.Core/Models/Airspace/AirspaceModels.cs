namespace Skyworks.Core.Models.Airspace;

/// <summary>
/// Γεωγραφικό σημείο (WGS-84)
/// </summary>
public class GeoPoint
{
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public double? AltitudeMeters { get; set; }
}

/// <summary>
/// Τύπος εναέριου χώρου (Annex B)
/// </summary>
public enum AirspaceType
{
    CTR,           // Control Zone
    ATZ,           // Aerodrome Traffic Zone
    TMA,           // Terminal Control Area
    Restricted,    // Restricted Area
    Danger,        // Danger Area
    Prohibited,    // Prohibited Area
    USpace,        // U-space
    Uncontrolled   // Μη ελεγχόμενος
}

/// <summary>
/// Κλάση εναέριου χώρου ICAO
/// </summary>
public enum AirspaceClass
{
    A, B, C, D, E, F, G
}

/// <summary>
/// Πληροφορίες εναέριου χώρου
/// </summary>
public class AirspaceInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required AirspaceType Type { get; set; }
    public AirspaceClass? Class { get; set; }
    public double? LowerLimitMeters { get; set; }
    public double? UpperLimitMeters { get; set; }
    public List<GeoPoint> Boundary { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// Αποτέλεσμα ερωτήματος εναέριου χώρου
/// </summary>
public class AirspaceQueryResult
{
    public List<AirspaceInfo> Airspaces { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
