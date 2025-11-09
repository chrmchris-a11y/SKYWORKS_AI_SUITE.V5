using System;

namespace Skyworks.Core.Models;

/// <summary>
/// 2D/3D mission geometry: waypoints, route, CGA (Contingency Geographic Area), geofence.
/// Stored as GeoJSON for compatibility with frontend mapping libraries (Leaflet, Cesium, MapLibre).
/// </summary>
public class MissionGeometry
{
    /// <summary>
    /// Unique geometry identifier
    /// </summary>
    public Guid MissionGeometryId { get; set; }
    
    /// <summary>
    /// Foreign key to parent Mission
    /// </summary>
    public Guid MissionId { get; set; }
    
    /// <summary>
    /// Full mission geometry as GeoJSON FeatureCollection.
    /// Includes:
    /// - "waypoints": MultiPoint or LineString of navigation points
    /// - "route": LineString connecting waypoints
    /// - "cga": Polygon defining Contingency Geographic Area (ERP landing zones)
    /// - "geofence": Polygon defining operational boundary
    /// </summary>
    public string GeoJson { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum height Above Ground Level in meters (AGL).
    /// Used for ARC/SAIL calculation (airspace intrusion risk).
    /// </summary>
    public decimal MaxHeightAGL_m { get; set; }
    
    /// <summary>
    /// Total route length in meters (sum of all waypoint-to-waypoint distances)
    /// </summary>
    public decimal RouteLength_m { get; set; }
    
    /// <summary>
    /// CGA area in square meters (m²).
    /// Larger CGA = more ground risk (SORA Table 2 consideration).
    /// </summary>
    public decimal CgaArea_m2 { get; set; }
    
    // Navigation property
    public Mission? Mission { get; set; }
}
