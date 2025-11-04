using Skyworks.Core.Models.GRC;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GRC.V2_0;

/// <summary>
/// SORA 2.0 Table 2 — Intrinsic Ground Risk Class (iGRC) input parameters (7 scenarios × 4 dimension categories → iGRC 1–10)
/// Based on JAR-DEL-WG6-D.04 (Edition 2.0, 30.01.2019), Section 2.3.1 and Table 2
/// 
/// <para><b>Table 2 structure:</b></para>
/// <list type="bullet">
/// <item><description>Rows: 7 operational scenarios (controlled ground area → gathering of people)</description></item>
/// <item><description>Columns: 4 UA dimension categories (1 m, 3 m, 8 m, &gt; 8 m) with typical kinetic energies</description></item>
/// <item><description>Cells: iGRC values 1–10 (some combinations out of scope/grey cells)</description></item>
/// </list>
/// </summary>
public class IntrinsicGRCInput_V2_0
{
    /// <summary>
    /// Max UAS characteristic dimension in meters (wingspan, blade diameter, max dimension)
    /// <para>Maps to SORA 2.0 Table 2 columns: 1m, 3m, 8m, >8m</para>
    /// </summary>
    public double MaxCharacteristicDimension { get; set; }

    /// <summary>
    /// Typical kinetic energy expected in Joules (optional, for validation against Table 2 reference energies)
    /// <para>Reference energies (from Table 2): ~700 J (1 m), ~34 kJ (3 m), ~1084 kJ (8 m), &gt; 1084 kJ (&gt; 8 m)</para>
    /// <para>Per §2.3.1(d): if dimension and typical kinetic energy mismatch, provide substantiation for chosen column</para>
    /// </summary>
    public double? KineticEnergy { get; set; }

    /// <summary>
    /// Operational scenario category (SORA 2.0 Table 2 rows: 1-7)
    /// </summary>
    public OperationalScenario Scenario { get; set; }
}

/// <summary>
/// SORA 2.0 operational scenarios (Table 2 rows: 1-7)
/// <para>Each scenario maps to a row in SORA 2.0 Table 2 with progressively higher risk levels</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationalScenario
{
    /// <summary>
    /// VLOS/BVLOS over controlled ground area → GRC 1-4
    /// </summary>
    ControlledGroundArea = 1,

    /// <summary>
    /// VLOS in sparsely populated environment → GRC 2-5
    /// </summary>
    VLOS_SparselyPopulated = 2,

    /// <summary>
    /// BVLOS in sparsely populated environment → GRC 3-6
    /// </summary>
    BVLOS_SparselyPopulated = 3,

    /// <summary>
    /// VLOS in populated environment → GRC 4-8
    /// </summary>
    VLOS_Populated = 4,

    /// <summary>
    /// BVLOS in populated environment → GRC 5-10
    /// </summary>
    BVLOS_Populated = 5,

    /// <summary>
    /// VLOS over gathering of people → GRC 7
    /// </summary>
    VLOS_GatheringOfPeople = 6,

    /// <summary>
    /// BVLOS over gathering of people → GRC 8
    /// </summary>
    BVLOS_GatheringOfPeople = 7
}

/// <summary>
/// SORA 2.0 UA dimension categories (Table 2 columns: 1–4)
/// <para>Categories based on max characteristic dimension and typical kinetic energy (Table 2 column headers)</para>
/// </summary>
public enum UASDimensionCategory_V2_0
{
    /// <summary>
    /// 1m / ~700J (~529 Ft·Lb)
    /// </summary>
    Small_1m = 1,

    /// <summary>
    /// 3m / ~34 kJ (~25,000 Ft·Lb)
    /// </summary>
    Medium_3m = 2,

    /// <summary>
    /// 8m / ~1084 kJ (~800,000 Ft·Lb)
    /// </summary>
    Large_8m = 3,

    /// <summary>
    /// >8m / >1084 kJ (>800,000 Ft·Lb)
    /// </summary>
    VeryLarge_Above8m = 4
}

/// <summary>
/// SORA 2.0 intrinsic GRC result from Table 2 lookup
/// <para>Result of mapping scenario (row) and dimension category (column) to iGRC value (1-10)</para>
/// </summary>
public class IntrinsicGRCResult_V2_0
{
    /// <summary>
    /// Intrinsic Ground Risk Class (1-10) from SORA 2.0 Table 2
    /// </summary>
    public int IGRC { get; set; }

    /// <summary>
    /// UAS dimension category used (Table 2 column)
    /// </summary>
    public UASDimensionCategory_V2_0 DimensionCategory { get; set; }

    /// <summary>
    /// Operational scenario used (Table 2 row)
    /// </summary>
    public OperationalScenario Scenario { get; set; }

    /// <summary>
    /// True if operation is not supported by SORA (grey/out-of-scope cells in Table 2)
    /// </summary>
    public bool IsOutOfScope { get; set; }

    /// <summary>
    /// Additional notes or warnings (e.g., boundary conditions, validation messages)
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// SORA 2.0 Ground Risk Mitigations (M1, M2, M3)
/// Based on JAR-DEL-WG6-D.04 (Edition 2.0), Section 2.3.2 and Table 3 — Mitigations for Final GRC determination
/// 
/// <para><b>Mitigation credits (GRC adjustment):</b></para>
/// <list type="bullet">
/// <item><description><b>M1 (Strategic mitigations):</b> Low = −1, Medium = −2, High = −4 — with floor: cannot reduce below the lowest value in the applicable Table 2 column (note (d))</description></item>
/// <item><description><b>M2 (Impact reduction):</b> Medium = −1, High = −2</description></item>
/// <item><description><b>M3 (ERP in place and effective):</b> Low = +1, Medium = 0, High = −1</description></item>
/// </list>
/// <para><b>Final GRC constraint:</b> If Final GRC &gt; 7 the operation is not supported by SORA (§2.3.2(g))</para>
/// </summary>
public enum GroundRiskMitigation_V2_0
{
    /// <summary>
    /// M1: Strategic mitigations for ground risk (reduce number of people at risk)
    /// <para>Robustness: Low = −1, Medium = −2, High = −4</para>
    /// <para>Constraint: Cannot reduce below lowest value in applicable Table 2 column (Table 3 note (d))</para>
    /// <para>Reference: SORA 2.0 Table 3</para>
    /// </summary>
    M1_StrategicMitigations = 1,

    /// <summary>
    /// M2: Effects of ground impact are reduced (reduce energy absorbed)
    /// <para>Robustness: Medium = −1, High = −2</para>
    /// <para>Reference: SORA 2.0 Table 3</para>
    /// </summary>
    M2_ImpactReduction = 2,

    /// <summary>
    /// M3: Emergency Response Plan (ERP) in place, operator validated and effective
    /// <para>Robustness: Low = +1, Medium = 0, High = −1</para>
    /// <para>Reference: SORA 2.0 Table 3</para>
    /// </summary>
    M3_EmergencyResponsePlan = 3
}

/// <summary>
/// Applied mitigation with robustness level (SORA 2.0)
/// <para>Represents a single mitigation (M1/M2/M3) applied with specific robustness (Low/Medium/High)</para>
/// <para>GRCReduction is computed per Tables 6-8 based on mitigation type and robustness level</para>
/// </summary>
public class AppliedMitigation_V2_0
{
    public GroundRiskMitigation_V2_0 Type { get; set; }
    public RobustnessLevel Robustness { get; set; }
    
    /// <summary>
    /// GRC adjustment credit per Table 3 (negative = reduction, positive = increase)
    /// </summary>
    public int GRCReduction { get; set; }
}

/// <summary>
/// Final GRC calculation input (SORA 2.0)
/// <para>Applies mitigations (Tables 6-8) to intrinsic GRC (Table 2) to compute Final GRC ≤ 7</para>
/// </summary>
public class FinalGRCInput_V2_0
{
    /// <summary>
    /// Intrinsic GRC from Table 2 lookup
    /// </summary>
    public int IntrinsicGRC { get; set; }
    
    /// <summary>
    /// List of mitigations to apply (M1/M2/M3 with robustness levels)
    /// </summary>
    public List<AppliedMitigation_V2_0> Mitigations { get; set; } = new();
    
    /// <summary>
    /// Applicable Table 2 column minimum (enforces M1 floor constraint per Table 3 note (d))
    /// </summary>
    public int ColumnMinimumGRC { get; set; }
}

/// <summary>
/// Final GRC calculation result (SORA 2.0)
/// <para>Final GRC must be ≤ 7 per SORA 2.0 AMC acceptability criteria</para>
/// </summary>
public class FinalGRCResult_V2_0
{
    /// <summary>
    /// Final Ground Risk Class after applying mitigations (must be ≤ 7)
    /// </summary>
    public int FinalGRC { get; set; }
    
    /// <summary>
    /// Total GRC reduction from all mitigations (sum of M1/M2/M3 credits)
    /// </summary>
    public int TotalGRCReduction { get; set; }
    
    /// <summary>
    /// List of mitigations actually applied with computed reductions
    /// </summary>
    public List<AppliedMitigation_V2_0> AppliedMitigations { get; set; } = new();
    
    /// <summary>
    /// True if Final GRC ≤ 7 and all mitigation constraints satisfied
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Validation message (e.g., "Final GRC exceeds limit of 7", "M1 floor constraint violated")
    /// </summary>
    public string ValidationMessage { get; set; } = string.Empty;
}
