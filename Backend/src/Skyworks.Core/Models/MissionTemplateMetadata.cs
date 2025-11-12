namespace Skyworks.Core.Models;

/// <summary>
/// Metadata for a mission template defining default parameters and characteristics.
/// Used by MissionOrchestratorService to auto-populate mission configuration.
/// </summary>
public class MissionTemplateMetadata
{
    /// <summary>
    /// Template code (one of 48 predefined types)
    /// </summary>
    public MissionTemplateCode Code { get; set; }
    
    /// <summary>
    /// High-level category (Buildings, Energy, Infrastructure, etc.)
    /// </summary>
    public MissionCategory Category { get; set; }
    
    /// <summary>
    /// Default mission type for geometry generation (Facade, Roof, Solar, Linear, Agriculture, Custom)
    /// </summary>
    public MissionType DefaultMissionType { get; set; }
    
    /// <summary>
    /// Default environment: "urban", "suburban", or "rural".
    /// Used for SORA Table 2 (population density) initial guess.
    /// </summary>
    public string DefaultEnvironment { get; set; } = "suburban";
    
    /// <summary>
    /// Default SORA version: "2.0" or "2.5"
    /// </summary>
    public string DefaultSoraVersion { get; set; } = "2.5";
    
    /// <summary>
    /// True if mission follows a linear path (corridor inspection)
    /// </summary>
    public bool IsLinear { get; set; }
    
    /// <summary>
    /// True if mission covers large area (>1 hectare typically)
    /// </summary>
    public bool IsLargeArea { get; set; }
    
    /// <summary>
    /// True if mission involves high structures (>30m AGL)
    /// </summary>
    public bool IsHighStructure { get; set; }
}
