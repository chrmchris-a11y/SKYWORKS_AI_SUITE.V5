using Skyworks.Core.Models;
using System;

namespace Skyworks.Core.Services.DTOs;

/// <summary>
/// Comprehensive overview response for a specific mission.
/// Contains all mission data needed for UI display (Maps, SORA badges, ERP, OSO).
/// </summary>
public class MissionOverviewResponse
{
    // ═══════════════════════════════════════════════════════════════
    // MISSION BASIC INFO
    // ═══════════════════════════════════════════════════════════════
    
    public Guid MissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string MissionType { get; set; } = string.Empty;
    public string SoraVersion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // ═══════════════════════════════════════════════════════════════
    // GEOMETRY
    // ═══════════════════════════════════════════════════════════════
    
    public GeometryData? Geometry { get; set; }
    
    public class GeometryData
    {
        public string RouteGeoJson { get; set; } = string.Empty;
        public double RouteLength_m { get; set; }
        public double CgaArea_m2 { get; set; }
        public double MaxHeightAGL_m { get; set; }
        public string? ControlledGroundAreaGeoJson { get; set; }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SORA ASSESSMENT
    // ═══════════════════════════════════════════════════════════════
    
    public SoraData? Sora { get; set; }
    
    public class SoraData
    {
        public int InitialGrc { get; set; }
        public int FinalGrc { get; set; }
        public string InitialArc { get; set; } = string.Empty;
        public string ResidualArc { get; set; } = string.Empty;
        public string Sail { get; set; } = string.Empty;
        public string? InputsJson { get; set; }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EMERGENCY RESPONSE PLAN
    // ═══════════════════════════════════════════════════════════════
    
    public ErpData? Erp { get; set; }
    
    public class ErpData
    {
        public string ErpJson { get; set; } = string.Empty;
        public string ErpText { get; set; } = string.Empty;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // OSO COVERAGE
    // ═══════════════════════════════════════════════════════════════
    
    public OsoData? Oso { get; set; }
    
    public class OsoData
    {
        public string RequiredOsosJson { get; set; } = string.Empty;
        public string CoveredOsosJson { get; set; } = string.Empty;
        public string MissingOsosJson { get; set; } = string.Empty;
        
        // Convenience counts for UI
        public int RequiredCount { get; set; }
        public int CoveredCount { get; set; }
        public int MissingCount { get; set; }
    }
}
