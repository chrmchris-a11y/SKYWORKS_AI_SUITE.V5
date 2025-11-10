using System.ComponentModel.DataAnnotations;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services.DTOs;

/// <summary>
/// Request DTO for auto-creating a complete mission with geometry, SORA, ERP, and OSO checklist.
/// </summary>
public class CreateMissionRequest
{
    /// <summary>
    /// Mission template code (e.g. PhotovoltaicParkInspection, FacadeInspection)
    /// Determines category, mission type, and default parameters.
    /// </summary>
    [Required]
    public MissionTemplateCode TemplateCode { get; set; }
    
    /// <summary>
    /// Center latitude for mission area (WGS84 decimal degrees)
    /// </summary>
    [Required]
    [Range(-90, 90)]
    public double CenterLat { get; set; }
    
    /// <summary>
    /// Center longitude for mission area (WGS84 decimal degrees)
    /// </summary>
    [Required]
    [Range(-180, 180)]
    public double CenterLon { get; set; }
    
    /// <summary>
    /// Maximum height above ground level in meters
    /// </summary>
    [Required]
    [Range(5, 120)]
    public double MaxHeightAGL_m { get; set; }
    
    /// <summary>
    /// Drone model name (e.g. "DJI Matrice 300 RTK")
    /// </summary>
    [Required]
    public string DroneModel { get; set; } = string.Empty;
    
    /// <summary>
    /// Drone maximum takeoff mass in kilograms
    /// </summary>
    [Required]
    [Range(0.25, 150)]
    public double DroneMtom_kg { get; set; }
    
    /// <summary>
    /// Drone CE class (C0, C1, C2, C3, C4, C5, C6, or "Legacy")
    /// </summary>
    [Required]
    public string DroneClass { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional SORA version override (defaults to template metadata default)
    /// </summary>
    public string? SoraVersion { get; set; }
    
    /// <summary>
    /// Optional mission name (auto-generated if not provided)
    /// </summary>
    public string? MissionName { get; set; }
    
    /// <summary>
    /// Optional client name
    /// </summary>
    public string? ClientName { get; set; }
    
    /// <summary>
    /// Optional area width in meters (for structured patterns like facade/roof)
    /// If not provided, defaults will be used based on template type.
    /// </summary>
    public double? AreaWidth_m { get; set; }
    
    /// <summary>
    /// Optional area length in meters (for structured patterns)
    /// </summary>
    public double? AreaLength_m { get; set; }
}
