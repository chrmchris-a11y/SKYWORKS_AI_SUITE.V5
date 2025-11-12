using System;

namespace Skyworks.Core.Models;

/// <summary>
/// Operational Safety Objectives (OSO) coverage analysis for the mission.
/// Determines which OSOs are required (by SAIL level), which are covered (by operator/mission/ERP),
/// and which are missing (gaps to address before operation).
/// 
/// Based on SORA Table 6 (OSO requirements per SAIL) and TMPR (Technical Mitigation Performance Requirements).
/// </summary>
public class MissionOsoCoverage
{
    /// <summary>
    /// Unique OSO coverage identifier
    /// </summary>
    public Guid MissionOsoCoverageId { get; set; }
    
    /// <summary>
    /// Foreign key to parent Mission
    /// </summary>
    public Guid MissionId { get; set; }
    
    /// <summary>
    /// Required OSOs as JSON array (based on SAIL level).
    /// 
    /// Schema:
    /// [
    ///   {
    ///     "id": "OSO#01",
    ///     "title": "Operational Procedures designed, validated and adhered to",
    ///     "robustnessLevel": "Medium",
    ///     "sailRequirement": "SAIL II"
    ///   },
    ///   {
    ///     "id": "OSO#02",
    ///     "title": "UAS operator competencies and training",
    ///     "robustnessLevel": "Medium",
    ///     "sailRequirement": "SAIL II"
    ///   },
    ///   ...
    /// ]
    /// 
    /// Mapping (SORA Table 6):
    /// - SAIL I: OSO#01 (Low), OSO#02 (Low)
    /// - SAIL II: OSO#01 (Medium), OSO#02 (Medium), OSO#03 (Medium), ...
    /// - SAIL III-VI: Increasing robustness levels (High/Very High) and additional OSOs
    /// </summary>
    public string RequiredOsosJson { get; set; } = "[]";
    
    /// <summary>
    /// Covered OSOs as JSON array (OSOs already satisfied by operator profile, mission design, or ERP).
    /// 
    /// Schema:
    /// [
    ///   {
    ///     "id": "OSO#01",
    ///     "coveredBy": "Operations Manual v2.1, approved by NAA",
    ///     "evidenceType": "Document",
    ///     "evidenceLink": "https://example.com/om_v2.1.pdf"
    ///   },
    ///   {
    ///     "id": "OSO#02",
    ///     "coveredBy": "Remote Pilot Certificate No. 12345, valid until 2026-12-31",
    ///     "evidenceType": "Certificate",
    ///     "evidenceLink": "https://example.com/cert_12345.pdf"
    ///   },
    ///   ...
    /// ]
    /// 
    /// Coverage determined by:
    /// - Operator profile (certificates, approved manuals, insurance)
    /// - Mission design (geofence, CGA, no-fly zones)
    /// - ERP (emergency procedures, landing sites, ATC coordination)
    /// - Drone capabilities (parachute, detect-and-avoid, redundancy)
    /// </summary>
    public string CoveredOsosJson { get; set; } = "[]";
    
    /// <summary>
    /// Missing OSOs as JSON array (OSOs required but NOT yet covered - must be addressed).
    /// 
    /// Schema:
    /// [
    ///   {
    ///     "id": "OSO#08",
    ///     "title": "Safe recovery from technical issues",
    ///     "robustnessLevel": "High",
    ///     "reason": "No parachute system installed on drone",
    ///     "recommendation": "Install ballistic parachute OR reduce SAIL by adding M1/M2 mitigations"
    ///   },
    ///   {
    ///     "id": "OSO#23",
    ///     "title": "Environmental conditions for safe operations",
    ///     "robustnessLevel": "Medium",
    ///     "reason": "No weather monitoring system in place",
    ///     "recommendation": "Integrate METAR/TAF monitoring, set wind limits (< 10 m/s)"
    ///   },
    ///   ...
    /// ]
    /// 
    /// Computed as: RequiredOsos - CoveredOsos = MissingOsos
    /// </summary>
    public string MissingOsosJson { get; set; } = "[]";
    
    // Navigation property
    public Mission? Mission { get; set; }
}
