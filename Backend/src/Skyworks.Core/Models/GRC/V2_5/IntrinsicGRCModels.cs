using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GRC.V2_5;

/// <summary>
/// SORA 2.5 Table 2 — Intrinsic Ground Risk Class (iGRC) Input
/// Βασισμένο στο JARUS SORA v2.5 Main Body, Section 4.2 (Step #2), Table 2 (7 bands πληθυσμιακής πυκνότητας × 5 κατηγορίες διαστάσεων/ταχύτητας → iGRC 1–10)
/// 
/// <para><b>Δομή Table 2:</b></para>
/// <list type="bullet">
/// <item><description>Γραμμές: 7 κατηγορίες πληθυσμιακής πυκνότητας (Controlled Ground Area → &gt;50k/km²), με ποιοτικούς περιγραφείς στο Table 3</description></item>
/// <item><description>Στήλες: 5 κατηγορίες UA (1 m / 25 m/s, 3 m / 35 m/s, 8 m / 75 m/s, 20 m / 120 m/s, 40 m+ / 200 m/s+)</description></item>
/// <item><description>Κελιά: τιμές iGRC 1–10 (γκρι κελιά = εκτός πεδίου SORA)</description></item>
/// </list>
/// <para><b>Κανόνας micro-UAS (Table 2 σημείωση):</b> UA με MTOM ≤ 250 g και μέγιστη ταχύτητα ≤ 25 m/s → iGRC = 1 ανεξαρτήτως πυκνότητας</para>
/// </summary>
public class IntrinsicGRCInput
{
    /// <summary>
    /// Maximum UA characteristic dimension (meters) - maps to SORA 2.5 Table 2 columns
    /// <para>Examples: wingspan for fixed wing, blade diameter for rotorcraft, max tip distance for multicopters</para>
    /// <para>Categories: 1m (25m/s), 3m (35m/s), 8m (75m/s), 20m (120m/s), 40m+ (200m/s+)</para>
    /// </summary>
    public double MaxCharacteristicDimension { get; set; }

    /// <summary>
    /// Maximum commanded airspeed (m/s) όπως ορίζεται από τον σχεδιαστή — δευτερεύων περιορισμός για επιλογή στήλης στον Πίνακα 2
    /// <para>Όρια ταχύτητας: 25, 35, 75, 120, 200 m/s κατά SORA 2.5 Table 2</para>
    /// <para>Κανόνας micro-UAS: αν MTOM ≤ 250 g και ταχύτητα ≤ 25 m/s → iGRC = 1</para>
    /// </summary>
    public double MaxSpeed { get; set; }

    /// <summary>
    /// Μέγιστη μάζα απογείωσης (MTOM) σε κιλά (προαιρετικό πεδίο για έλεγχο micro-UAS)
    /// <para>Χρησιμοποιείται για εφαρμογή του κανόνα: MTOM ≤ 0.25 kg και MaxSpeed ≤ 25 m/s → iGRC = 1 (Table 2 σημείωση)</para>
    /// </summary>
    [JsonPropertyName("mtom_kg")]
    public double? MTOM_Kg { get; set; }

    /// <summary>
    /// Maximum population density in the iGRC footprint (people/km²) - maps to SORA 2.5 Table 2 rows via Table 3
    /// <para>Footprint = Flight Geography + Contingency Volume + Initial Ground Risk Buffer</para>
    /// <para>Categories: &lt;5, &lt;50, &lt;500, &lt;5k, &lt;50k, ≥50k people/km² per SORA 2.5 Table 3</para>
    /// </summary>
    public double PopulationDensity { get; set; }

    /// <summary>
    /// Is this a Controlled Ground Area? (only involved persons present) - maps to SORA 2.5 Table 2 row 1
    /// <para>If true, population density category is "Controlled Ground Area" regardless of actual density</para>
    /// </summary>
    public bool IsControlledGroundArea { get; set; }
}

/// <summary>
/// Population density categories per SORA 2.5 Table 3 (maps to Table 2 rows: 1-7)
/// <para>Reference: JARUS SORA v2.5 Section 4.2, Table 3 - Population Density Categories</para>
/// </summary>
public enum PopulationDensityCategory
{
    /// <summary>
    /// Controlled Ground Area (only involved persons)
    /// </summary>
    ControlledGroundArea = 0,

    /// <summary>
    /// &lt; 5 people/km² (Remote/Extremely remote)
    /// </summary>
    Remote = 1,

    /// <summary>
    /// &lt; 50 people/km² (Lightly populated)
    /// </summary>
    LightlyPopulated = 2,

    /// <summary>
    /// &lt; 500 people/km² (Sparsely populated / Residential lightly populated)
    /// </summary>
    SparselyPopulated = 3,

    /// <summary>
    /// &lt; 5,000 people/km² (Suburban / Low density metropolitan)
    /// </summary>
    Suburban = 4,

    /// <summary>
    /// &lt; 50,000 people/km² (High density metropolitan)
    /// </summary>
    HighDensityMetro = 5,

    /// <summary>
    /// &gt; 50,000 people/km² (Assemblies of people)
    /// </summary>
    AssembliesOfPeople = 6
}

/// <summary>
/// UA Dimension Category κατά SORA 2.5 Table 2 (στήλες 1–5) με περιορισμούς μέγιστης ταχύτητας
/// <para>Κάθε κατηγορία έχει όριο διάστασης και όριο ταχύτητας (π.χ. 1 m / 25 m/s, 3 m / 35 m/s, κ.ο.κ.)</para>
/// <para>Αναφορά: JARUS SORA v2.5 Section 4.2, Table 2</para>
/// </summary>
public enum UADimensionCategory
{
    /// <summary>
    /// 1m / approx. 3 ft; max speed 25 m/s
    /// </summary>
    Small = 1,

    /// <summary>
    /// 3m / approx. 10 ft; max speed 35 m/s
    /// </summary>
    Medium = 2,

    /// <summary>
    /// 8m / approx. 25 ft; max speed 75 m/s
    /// </summary>
    Large = 3,

    /// <summary>
    /// 20m / approx. 65 ft; max speed 120 m/s
    /// </summary>
    VeryLarge = 4,

    /// <summary>
    /// 40m+ / approx. 130 ft+; max speed 200 m/s+
    /// </summary>
    ExtraLarge = 5
}

/// <summary>
/// SORA 2.5 intrinsic GRC result από Table 2
/// <para>Χαρτογράφηση κατηγορίας πληθυσμιακής πυκνότητας (Table 3) και κατηγορίας διάστασης/ταχύτητας (Table 2) σε iGRC (1–10)</para>
/// <para>Κανόνας micro-UAS: MTOM ≤ 250 g και MaxSpeed ≤ 25 m/s ⇒ iGRC = 1</para>
/// </summary>
public class IntrinsicGRCResult
{
    /// <summary>
    /// Intrinsic Ground Risk Class (1-10) from SORA 2.5 Table 2
    /// <para>Micro-UAS (≤250g MTOM and ≤25m/s) always result in iGRC=1</para>
    /// </summary>
    public int IGRC { get; set; }
    
    /// <summary>
    /// UAS dimension category used (Table 2 column: 1-5)
    /// </summary>
    public UADimensionCategory DimensionCategory { get; set; }
    
    /// <summary>
    /// Population density category used (Table 2 row: 0-6, where 0=controlled area)
    /// </summary>
    public PopulationDensityCategory PopulationCategory { get; set; }
    
    /// <summary>
    /// True if operation is not supported by SORA (grey/out-of-scope cells in Table 2)
    /// </summary>
    public bool IsOutOfScope { get; set; }
    
    /// <summary>
    /// Additional notes or warnings (e.g., micro-UAS rule applied, speed exceeds category limit)
    /// </summary>
    public string? Notes { get; set; }
}
