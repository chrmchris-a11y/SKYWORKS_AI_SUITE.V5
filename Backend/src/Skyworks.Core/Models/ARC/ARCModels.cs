using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Skyworks.Core.JsonConverters;

namespace Skyworks.Core.Models.ARC;

/// <summary>
/// Air Risk Class (ARC) categories per JARUS SORA v2.0/v2.5
/// Official notation: ARC-a (lowest risk) to ARC-d (highest risk)
/// Reference: JAR-DEL-WG6-D.04 Section 2.4.2, JAR-DEL-SRM-SORA-MB-2.5 Section 4.4
/// </summary>
[JsonConverter(typeof(ARCRatingConverter))]
public enum ARCRating
{
    /// <summary>
    /// ARC-a: Atypical/Segregated airspace - No TMPR required
    /// Risk of collision between UAS and manned aircraft is acceptable without tactical mitigation
    /// </summary>
    [EnumMember(Value = "ARC_a")]
    ARC_a = 0,
    
    /// <summary>
    /// ARC-b: Low collision risk - Low TMPR required (System Risk Ratio ≤ 0.66)
    /// </summary>
    [EnumMember(Value = "ARC_b")]
    ARC_b = 1,
    
    /// <summary>
    /// ARC-c: Medium collision risk - Medium TMPR required (System Risk Ratio ≤ 0.33)
    /// </summary>
    [EnumMember(Value = "ARC_c")]
    ARC_c = 2,
    
    /// <summary>
    /// ARC-d: High collision risk - High TMPR required (System Risk Ratio ≤ 0.1)
    /// Operations in integrated airspace with full DAA compliance
    /// </summary>
    [EnumMember(Value = "ARC_d")]
    ARC_d = 3
}

/// <summary>
/// Initial ARC determination input (version-agnostic contract)
/// For 2.0/2.5, the system may accept explicit ARC override or environment descriptors.
/// </summary>
public class ARCInitialInput
{
    /// <summary>
    /// Optional explicit ARC override (if decision tree or authority map already used by the operator)
    /// </summary>
    public ARCRating? ExplicitARC { get; set; }

    /// <summary>
    /// Placeholder for future environment descriptors (airspace class, altitude, proximity to aerodromes, etc.)
    /// </summary>
    public string? EnvironmentCategory { get; set; }
}

/// <summary>
/// Residual ARC determination input (post strategic mitigations)
/// </summary>
public class ARCResidualInput
{
    /// <summary>
    /// Initial ARC (from Step #4)
    /// </summary>
    public required ARCRating InitialARC { get; set; }

    /// <summary>
    /// Optional: explicit residual ARC override (e.g., when authority map or Annex C process applied externally)
    /// </summary>
    public ARCRating? ExplicitResidualARC { get; set; }

    /// <summary>
    /// Optional: list of applied strategic mitigations identifiers (to be detailed per Annex C)
    /// </summary>
    public List<string>? StrategicMitigations { get; set; }

    /// <summary>
    /// Optional: Demonstrated local manned-aircraft density rating per Annex C Table 2 (1 very low .. 5 very high)
    /// Used for Strategic Mitigation by Operational Restrictions (density-based reduction).
    /// </summary>
    public int? LocalDensityRating { get; set; }

    /// <summary>
    /// Optional: Claim that Operational Volume meets Atypical/Segregated Airspace definition (Annex G 3.20)
    /// If accepted by the authority, residual ARC may be ARC-a without tactical mitigations.
    /// </summary>
    public bool? IsAtypicalSegregated { get; set; }
}

/// <summary>
/// Result of ARC determination with full auditability
/// Includes AEC category and density rating for traceability
/// Reference: SORA 2.0 Annex C Table 1 (AEC categories), Table 2 (density ratings)
/// </summary>
public class ARCResult
{
    /// <summary>
    /// Determined Air Risk Class (ARC-a, ARC-b, ARC-c, or ARC-d)
    /// </summary>
    public required ARCRating ARC { get; set; }
    
    /// <summary>
    /// Airspace Encounter Category (1-12) per SORA 2.0 Annex C Table 1
    /// AEC 1:  Airport B/C/D              → ARC-d (Density 5)
    /// AEC 2:  >500ft Mode-S/TMZ          → ARC-d (Density 5)
    /// AEC 3:  >500ft Controlled          → ARC-d (Density 5)
    /// AEC 4:  >500ft Uncontrolled Urban  → ARC-c (Density 3)
    /// AEC 5:  >500ft Uncontrolled Rural  → ARC-c (Density 2)
    /// AEC 6:  Airport E/F/G              → ARC-c (Density 3)
    /// AEC 7:  <500ft Mode-S/TMZ          → ARC-c (Density 3)
    /// AEC 8:  <500ft Controlled          → ARC-c (Density 3)
    /// AEC 9:  <500ft Uncontrolled Urban  → ARC-c (Density 2)
    /// AEC 10: <500ft Uncontrolled Rural  → ARC-b (Density 1)
    /// AEC 11: >FL600                     → ARC-b (Density 1)
    /// AEC 12: Atypical/Segregated        → ARC-a (Density 1)
    /// </summary>
    public int? AEC { get; set; }
    
    /// <summary>
    /// Manned aircraft density rating (1-5) per SORA 2.0 Annex C Table 1
    /// 1 = Very Low (typical for >FL600, rural VLL, segregated)
    /// 2 = Low (rural low-level)
    /// 3 = Medium (urban VLL, controlled VLL, airport E/F/G)
    /// 4 = High (reserved)
    /// 5 = Very High (airport B/C/D, Mode-S veil, controlled >500ft)
    /// Used for strategic mitigation calculations
    /// </summary>
    public int? DensityRating { get; set; }
    
    /// <summary>
    /// Detailed notes explaining the ARC determination rationale
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
