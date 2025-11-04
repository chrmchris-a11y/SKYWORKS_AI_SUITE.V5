using Skyworks.Core.Models.GRC;

namespace Skyworks.Core.Models.GRC.V2_5;

/// <summary>
/// Ground Risk Mitigation Types per SORA 2.5 Table 5
/// <para>Reference: JARUS SORA v2.5 Main Body, Section 4.3, Table 5 - Ground Risk Mitigations</para>
/// 
/// <para><b>Mitigation Credits (GRC reduction) as implemented:</b></para>
/// <list type="bullet">
/// <item><description><b>M1(A) - Sheltering:</b> Low = -1, Medium = -2</description></item>
/// <item><description><b>M1(B) - Operational restrictions:</b> Medium = -1, High = -2</description></item>
/// <item><description><b>M1(C) - Ground observation:</b> Low = -1</description></item>
/// <item><description><b>M2 - Impact dynamics:</b> Medium = -1, High = -2</description></item>
/// </list>
/// <para>These credits reflect the current interpretation and automated tests (see GRCCalculationService.GetMitigationCredit_V2_5).</para>
/// <para><b>Final GRC constraint:</b> Must be ≤ 7 per SORA 2.5 acceptability criteria</para>
/// </summary>
public enum GroundRiskMitigation
{
    None = 0,

    /// <summary>
    /// M1(A) - Strategic mitigation - Sheltering (reduce number of people at risk)
    /// <para>Robustness credits: Low=-1, Medium=-2</para>
    /// <para>Reference: SORA 2.5 Table 5, row M1(A)</para>
    /// </summary>
    M1A_Sheltering,

    /// <summary>
    /// M1(B) - Strategic mitigation - Operational restrictions (time/place constraints)
    /// <para>Robustness credits: Medium=-1, High=-2</para>
    /// <para>Reference: SORA 2.5 Table 5, row M1(B)</para>
    /// </summary>
    M1B_OperationalRestrictions,

    /// <summary>
    /// M1(C) - Tactical mitigation - Ground observation (detect and avoid people)
    /// <para>Robustness credits: Low=-1</para>
    /// <para>Reference: SORA 2.5 Table 5, row M1(C)</para>
    /// </summary>
    M1C_GroundObservation,

    /// <summary>
    /// M2 - Effects of UA impact dynamics are reduced (e.g., parachute, deformable structure)
    /// <para>Robustness credits: Medium=-1, High=-2</para>
    /// <para>Reference: SORA 2.5 Table 5, row M2</para>
    /// </summary>
    M2_ImpactDynamics
}

/// <summary>
/// Mitigation Robustness Level per SORA 2.5 (Low, Medium, High)
/// </summary>
// RobustnessLevel is shared in Skyworks.Core.Models.GRC.RobustnessLevel

/// <summary>
/// Applied Mitigation with robustness level (SORA 2.5)
/// <para>Represents a single mitigation (M1A/M1B/M1C/M2) applied with specific robustness (Low/Medium/High)</para>
/// <para>GRCReduction is computed per Table 5 based on mitigation type and robustness level</para>
/// </summary>
public class AppliedMitigation
{
    public GroundRiskMitigation Type { get; set; }
    public RobustnessLevel Robustness { get; set; }

    /// <summary>
    /// GRC reduction credit (-1, -2) per SORA 2.5 Table 5
    /// <para>As implemented: M1A Low=-1, Medium=-2; M1B Medium=-1, High=-2; M1C Low=-1; M2 Medium=-1, High=-2; others 0.</para>
    /// </summary>
    public int GRCReduction { get; set; }
}

/// <summary>
/// Final GRC calculation input (SORA 2.5)
/// <para>Applies mitigations (Table 5) to intrinsic GRC (Table 2) to compute Final GRC ≤ 7</para>
/// </summary>
public class FinalGRCInput
{
    /// <summary>
    /// Intrinsic GRC from Table 2 lookup (includes micro-UAS special rule if applicable)
    /// </summary>
    public required int IntrinsicGRC { get; set; }
    
    /// <summary>
    /// List of mitigations to apply (M1A/M1B/M1C/M2 with robustness levels)
    /// </summary>
    public required List<AppliedMitigation> Mitigations { get; set; }
}

/// <summary>
/// Final GRC calculation result (SORA 2.5)
/// <para>Final GRC must be ≤ 7 per SORA 2.5 acceptability criteria</para>
/// </summary>
public class FinalGRCResult
{
    /// <summary>
    /// Final Ground Risk Class after applying mitigations (must be ≤ 7)
    /// </summary>
    public int FinalGRC { get; set; }
    
    /// <summary>
    /// Total GRC reduction from all mitigations (sum of M1A/M1B/M1C/M2 credits)
    /// </summary>
    public int TotalGRCReduction { get; set; }
    
    /// <summary>
    /// List of mitigations actually applied with computed reductions
    /// </summary>
    public List<AppliedMitigation> AppliedMitigations { get; set; } = new();
    
    /// <summary>
    /// True if Final GRC ≤ 7 and all mitigation constraints satisfied
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Validation message (e.g., "Final GRC exceeds limit of 7")
    /// </summary>
    public string? ValidationMessage { get; set; }
}
