using System.Collections.Generic;

namespace Skyworks.Core.Models;

/// <summary>
/// Structured Emergency Response Plan (ERP) following EASA/JARUS SORA guidance.
/// Defines contingency procedures for various failure scenarios.
/// </summary>
public class ErpPlan
{
    /// <summary>
    /// Loss of C2 link procedures
    /// </summary>
    public ErpLossOfC2 LossOfC2 { get; set; } = new();
    
    /// <summary>
    /// Fly-away / loss of control procedures
    /// </summary>
    public ErpFlyAway FlyAway { get; set; } = new();
    
    /// <summary>
    /// Emergency landing procedures
    /// </summary>
    public ErpEmergencyLanding EmergencyLanding { get; set; } = new();
    
    /// <summary>
    /// Ground notification requirements
    /// </summary>
    public ErpGroundNotification GroundNotification { get; set; } = new();
    
    /// <summary>
    /// ATS coordination requirements
    /// </summary>
    public ErpAtsCoordination AtsCoordination { get; set; } = new();
}

/// <summary>
/// Loss of C2 link contingency procedures
/// </summary>
public class ErpLossOfC2
{
    /// <summary>
    /// Return-to-Home altitude in meters AGL
    /// </summary>
    public double RthAltitude_m { get; set; }
    
    /// <summary>
    /// Maximum time to execute RTH in seconds
    /// </summary>
    public int MaxTimeToRth_s { get; set; }
    
    /// <summary>
    /// Required operator actions
    /// </summary>
    public List<string> Actions { get; set; } = new();
}

/// <summary>
/// Fly-away / loss of control contingency procedures
/// </summary>
public class ErpFlyAway
{
    /// <summary>
    /// Whether a safe area is defined for fly-away scenarios
    /// </summary>
    public bool DefinedSafeArea { get; set; }
    
    /// <summary>
    /// Safe area center latitude (WGS84)
    /// </summary>
    public double? SafeAreaLat { get; set; }
    
    /// <summary>
    /// Safe area center longitude (WGS84)
    /// </summary>
    public double? SafeAreaLon { get; set; }
    
    /// <summary>
    /// Safe area radius in meters
    /// </summary>
    public double? SafeAreaRadius_m { get; set; }
    
    /// <summary>
    /// Required operator actions
    /// </summary>
    public List<string> Actions { get; set; } = new();
}

/// <summary>
/// Emergency landing contingency procedures
/// </summary>
public class ErpEmergencyLanding
{
    /// <summary>
    /// Pre-identified emergency landing sites
    /// </summary>
    public List<ErpLandingSite> Sites { get; set; } = new();
    
    /// <summary>
    /// Required operator actions
    /// </summary>
    public List<string> Actions { get; set; } = new();
}

/// <summary>
/// Emergency landing site definition
/// </summary>
public class ErpLandingSite
{
    /// <summary>
    /// Site latitude (WGS84)
    /// </summary>
    public double Lat { get; set; }
    
    /// <summary>
    /// Site longitude (WGS84)
    /// </summary>
    public double Lon { get; set; }
    
    /// <summary>
    /// Site description (e.g., "CGA open area", "field NE of site")
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Ground notification requirements
/// </summary>
public class ErpGroundNotification
{
    /// <summary>
    /// Whether ground notification is required
    /// </summary>
    public bool Required { get; set; }
    
    /// <summary>
    /// Contacts to notify (e.g., "Client", "Local Authorities")
    /// </summary>
    public List<string> Contacts { get; set; } = new();
    
    /// <summary>
    /// Notification methods (e.g., "Phone", "Email", "Radio")
    /// </summary>
    public List<string> Methods { get; set; } = new();
}

/// <summary>
/// ATS coordination requirements
/// </summary>
public class ErpAtsCoordination
{
    /// <summary>
    /// Whether ATS coordination is required
    /// </summary>
    public bool Required { get; set; }
    
    /// <summary>
    /// ATS contacts (e.g., "ATS", "Aerodrome", "FIS")
    /// </summary>
    public List<string> Contacts { get; set; } = new();
    
    /// <summary>
    /// Coordination procedure summary
    /// </summary>
    public string ProcedureSummary { get; set; } = string.Empty;
}
