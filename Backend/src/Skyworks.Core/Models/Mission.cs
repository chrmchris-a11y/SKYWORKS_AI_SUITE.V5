using System;

namespace Skyworks.Core.Models;

/// <summary>
/// Main mission entity representing a specific UAS operation.
/// Holds metadata and links to geometry, SORA assessment, ERP, and OSO coverage.
/// </summary>
public class Mission
{
    /// <summary>
    /// Unique mission identifier
    /// </summary>
    public Guid MissionId { get; set; }
    
    /// <summary>
    /// Mission name/title (e.g. "Solar Farm Inspection - Smithville")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of mission (Facade/Roof/Solar/Linear/Agriculture/Custom)
    /// </summary>
    public MissionType Type { get; set; }
    
    /// <summary>
    /// Client name (operator or end customer)
    /// </summary>
    public string? ClientName { get; set; }
    
    /// <summary>
    /// Street address or location description
    /// </summary>
    public string? LocationAddress { get; set; }
    
    /// <summary>
    /// City/municipality
    /// </summary>
    public string? LocationCity { get; set; }
    
    /// <summary>
    /// Country (ISO 3166-1 alpha-2 code recommended, e.g. "DE", "FR", "GR")
    /// </summary>
    public string? LocationCountry { get; set; }
    
    /// <summary>
    /// SORA version used for risk assessment ("2.0" or "2.5")
    /// </summary>
    public string SoraVersion { get; set; } = "2.5";
    
    /// <summary>
    /// Mission creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who created the mission (username or ID)
    /// </summary>
    public string? CreatedBy { get; set; }
    
    // Navigation properties
    public MissionGeometry? Geometry { get; set; }
    public MissionSoraAssessment? SoraAssessment { get; set; }
    public MissionErp? Erp { get; set; }
    public MissionOsoCoverage? OsoCoverage { get; set; }
}
