namespace Skyworks.Core.Models;

/// <summary>
/// Request for auto-generating mission geometry based on mission type and parameters.
/// Used by MissionGeometryService to create waypoints, routes, CGA, and geofence.
/// </summary>
public class GeometryGenerationRequest
{
    /// <summary>
    /// Type of mission (determines pattern algorithm)
    /// </summary>
    public MissionType MissionType { get; set; }
    
    /// <summary>
    /// Center latitude of mission area (WGS84)
    /// </summary>
    public double CenterLat { get; set; }
    
    /// <summary>
    /// Center longitude of mission area (WGS84)
    /// </summary>
    public double CenterLon { get; set; }
    
    /// <summary>
    /// Maximum flight height Above Ground Level in meters
    /// </summary>
    public double MaxHeightAGL_m { get; set; }
    
    /// <summary>
    /// Optional: Width of area in meters (for area-based missions like Roof, Solar, Agriculture)
    /// </summary>
    public double? AreaWidth_m { get; set; }
    
    /// <summary>
    /// Optional: Length of area in meters (for area-based missions)
    /// </summary>
    public double? AreaLength_m { get; set; }
    
    /// <summary>
    /// Optional: User-provided GeoJSON (for Custom missions)
    /// </summary>
    public string? ImportedGeoJson { get; set; }
    
    /// <summary>
    /// Optional: User-provided KML (for Custom missions)
    /// </summary>
    public string? ImportedKml { get; set; }
    
    /// <summary>
    /// Optional: User-provided CSV waypoints (for Custom missions)
    /// </summary>
    public string? ImportedCsv { get; set; }
    
    /// <summary>
    /// If true, use imported geometry instead of generating patterns.
    /// Only valid for MissionType.Custom.
    /// </summary>
    public bool UseImportedGeometry { get; set; }
}
