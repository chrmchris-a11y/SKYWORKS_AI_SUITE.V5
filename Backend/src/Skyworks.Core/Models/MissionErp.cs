using System;

namespace Skyworks.Core.Models;

/// <summary>
/// Emergency Response Plan (ERP) for the mission.
/// Generated based on SAIL level, mission type, airspace, and environment.
/// Uses rule-based logic from EASA/JARUS guidance (NOT free-form generation).
/// </summary>
public class MissionErp
{
    /// <summary>
    /// Unique ERP identifier
    /// </summary>
    public Guid MissionErpId { get; set; }
    
    /// <summary>
    /// Foreign key to parent Mission
    /// </summary>
    public Guid MissionId { get; set; }
    
    /// <summary>
    /// Structured ERP data as JSON (for programmatic access and UI rendering).
    /// 
    /// Schema:
    /// {
    ///   "lossOfC2": {
    ///     "rthAltitude_m": 60,
    ///     "maxTime_s": 15,
    ///     "actions": ["Initiate RTH", "Notify Remote Pilot", "Monitor descent"]
    ///   },
    ///   "flyAway": {
    ///     "safeArea": { "type": "Polygon", "coordinates": [...] },
    ///     "actions": ["Execute emergency landing", "Alert ATC if applicable"]
    ///   },
    ///   "emergencyLanding": {
    ///     "sites": [{ "lat": 50.123, "lon": 8.456, "name": "Field A" }],
    ///     "criteria": ["Clear of people", "No obstacles > 2m", "Soft surface preferred"]
    ///   },
    ///   "groundNotification": {
    ///     "who": "Local Police, NAA",
    ///     "how": "Phone: +49 123 456 789, Email: ops@naa.de",
    ///     "when": "Immediately upon incident"
    ///   },
    ///   "atsCoordination": {
    ///     "required": true,
    ///     "contacts": ["ATC Tower: +49 987 654 321"],
    ///     "procedure": "NOTAM filed 24h prior, ATC clearance required"
    ///   }
    /// }
    /// 
    /// Rules (based on SAIL level):
    /// - SAIL I-II: Simple RTH, no ATC coordination required (unless CTR/ATZ)
    /// - SAIL III-IV: Detailed ERP, emergency landing sites, ground notification
    /// - SAIL V-VI: Full ERP with redundancy, ATC coordination, real-time monitoring
    /// </summary>
    public string ErpJson { get; set; } = "{}";
    
    /// <summary>
    /// Human-readable ERP narrative (for Operations Manual, briefings, reports).
    /// 
    /// Example:
    /// "In the event of loss of C2 link, the UAS shall initiate Return-to-Home (RTH) procedure 
    /// at 60m AGL within 15 seconds. The Remote Pilot shall be notified immediately via telemetry.
    /// If RTH fails, the UAS shall execute emergency landing at designated site (Field A, 50.123° N, 8.456° E).
    /// Local police and NAA shall be notified by phone (+49 123 456 789) within 5 minutes of incident.
    /// ATC coordination: NOTAM filed 24h prior, clearance required from Tower (+49 987 654 321)."
    /// 
    /// Generated via:
    /// - Template-based approach (string interpolation with structured data), OR
    /// - LLM-based approach (using ErpJson as input, constrained by EASA/JARUS rules).
    /// </summary>
    public string ErpText { get; set; } = string.Empty;
    
    // Navigation property
    public Mission? Mission { get; set; }
}
