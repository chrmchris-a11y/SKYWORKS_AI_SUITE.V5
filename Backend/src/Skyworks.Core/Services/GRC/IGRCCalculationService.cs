using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services.GRC;

/// <summary>
/// Ενιαία υπηρεσία υπολογισμού GRC (Ground Risk Class) σύμφωνα με EASA/JARUS.
/// Unified GRC calculation service covering both JARUS SORA 2.0 AMC and SORA 2.5.
/// 
/// Contract highlights (EASA/JARUS references):
/// - SORA 2.0 AMC: Table 2 (iGRC), Tables 6–8 (M1/M2/M3) → Final GRC ≤ 7
/// - SORA 2.5: Table 2 (iGRC: πληθυσμιακές ζώνες × κατηγορίες διαστάσεων),
///             Annex F (ποσοτικό μοντέλο – εκτός του παρόντος interface),
///             M1A/M1B/M1C/M2 → Final GRC ≤ 7
/// Additionally exposes helpers for ARC/SAIL/TMPR mapping used by orchestration services.
/// </summary>
public interface IGRCCalculationService
{
    /// <summary>
    /// Υπολογισμός Intrinsic GRC με βάση SORA 2.0 AMC (Table 2: 7 σενάρια × 4 διαστάσεις).
    /// Calculate Intrinsic GRC using SORA 2.0 AMC Table 2.
    /// Input: <see cref="IntrinsicGRCInput_V2_0"/> (σενάριο, μέγιστη χαρακτηριστική διάσταση, προαιρετικά ενέργεια).
    /// Output: <see cref="IntrinsicGRCResult_V2_0"/> (iGRC 1–10, κατηγορία διάστασης, scope flags).
    /// </summary>
    IntrinsicGRCResult_V2_0 CalculateIntrinsicGRC_V2_0(IntrinsicGRCInput_V2_0 input);

    /// <summary>
    /// Υπολογισμός Final GRC SORA 2.0 AMC εφαρμόζοντας M1/M2/M3 (Tables 6–8) με ανώτατο όριο GRC=7.
    /// Calculate Final GRC with mitigations using SORA 2.0 AMC (M1/M2/M3). Final GRC must be ≤ 7.
    /// </summary>
    FinalGRCResult_V2_0 CalculateFinalGRC_V2_0(FinalGRCInput_V2_0 input);

    /// <summary>
    /// Υπολογισμός Intrinsic GRC SORA 2.5 (Table 2: πληθυσμιακή κατηγορία × κατηγορία διάστασης/ταχύτητας).
    /// Includes special case: micro‑UAS ≤ 250 g & ≤ 25 m/s → iGRC = 1 (Table 2 note).
    /// </summary>
    IntrinsicGRCResult CalculateIntrinsicGRC_V2_5(IntrinsicGRCInput input);

    /// <summary>
    /// Υπολογισμός Final GRC SORA 2.5 εφαρμόζοντας M1A/M1B/M1C/M2 με κατώφλι GRC=1 και ανώτατο όριο 7.
    /// Calculate Final GRC with mitigations using SORA 2.5 (M1A/M1B/M1C/M2). Final GRC must be ≤ 7.
    /// </summary>
    FinalGRCResult CalculateFinalGRC_V2_5(FinalGRCInput input);

    /// <summary>
    /// Χαρτογράφηση πυκνότητας πληθυσμού → κατηγορία πληθυσμού (SORA 2.5 Table 2 rows).
    /// Get population density category (SORA 2.5 Table 2 rows). Controlled ground area handled explicitly.
    /// </summary>
    PopulationDensityCategory GetPopulationCategory(double density, bool isControlledArea);

    /// <summary>
    /// Χαρτογράφηση μέγιστης χαρακτηριστικής διάστασης/ταχύτητας → κατηγορία διάστασης SORA 2.5 (Table 2 columns).
    /// Get UAS dimension category (SORA 2.5 Table 2 columns), speed used as secondary constraint.
    /// </summary>
    UADimensionCategory GetDimensionCategory(double dimension, double speed);

    /// <summary>
    /// Χαρτογράφηση διάστασης UA → κατηγορία διάστασης SORA 2.0 (Table 2 columns: 1m/3m/8m/>8m).
    /// Get UAS dimension category (SORA 2.0 Table 2 columns).
    /// </summary>
    UASDimensionCategory_V2_0 GetDimensionCategory_V2_0(double dimension, double? kineticEnergy = null);

    // ARC determination
    /// <summary>
    /// [SORA 2.0 Annex C Table 1] Determine Initial ARC (deprecated input overload kept for compatibility).
    /// </summary>
    ARCResult DetermineInitialARC_V2_0(ARCInitialInput input);
    /// <summary>
    /// [SORA 2.0 Annex C] Determine Residual ARC μετά στρατηγικές μετριάσεις (χωρίς διπλή μέτρηση).
    /// </summary>
    ARCResult DetermineResidualARC_V2_0(ARCResidualInput input);
    /// <summary>
    /// [SORA 2.5 Figure 6] Determine Initial ARC (deprecated input overload kept for compatibility).
    /// </summary>
    ARCResult DetermineInitialARC_V2_5(ARCInitialInput input);
    /// <summary>
    /// [SORA 2.5 Figure 6] Determine Residual ARC μετά στρατηγικές μετριάσεις (χωρίς διπλή μέτρηση).
    /// </summary>
    ARCResult DetermineResidualARC_V2_5(ARCResidualInput input);
    /// <summary>
    /// [SORA 2.0 Annex C Table 1] Authoritative AEC decision-tree based Initial ARC (environmental parameters).
    /// </summary>
    ARCResult DetermineInitialARC_V2_0(ARCEnvironmentInput env);
    /// <summary>
    /// [SORA 2.5 Figure 6] Authoritative AEC decision-tree based Initial ARC (environmental parameters).
    /// </summary>
    ARCResult DetermineInitialARC_V2_5(ARCEnvironmentInput env);

    // SAIL determination
    /// <summary>
    /// [SORA 2.0 Table 5] Final GRC × Residual ARC → SAIL I..VI.
    /// </summary>
    SAILResult DetermineSAIL_V2_0(SAILInput input);
    /// <summary>
    /// [SORA 2.5] Mapping aligned with SORA 2.0 structure pending official deltas.
    /// </summary>
    SAILResult DetermineSAIL_V2_5(SAILInput input);

    // TMPR mapping from Residual ARC
    /// <summary>
    /// [SORA 2.0 Table 4] Residual ARC → TMPR level & robustness requirements.
    /// </summary>
    Skyworks.Core.Models.ARC.TMPRResult DetermineTMPR_V2_0(Skyworks.Core.Models.ARC.ARCRating residualArc);
    /// <summary>
    /// [SORA 2.5 Table 4] Residual ARC → TMPR level & robustness requirements.
    /// </summary>
    Skyworks.Core.Models.ARC.TMPRResult DetermineTMPR_V2_5(Skyworks.Core.Models.ARC.ARCRating residualArc);

    // Annex C strategic mitigations (helper)
    /// <summary>
    /// Επιστρέφει τη λίστα αναγνωρισμένων στρατηγικών μετριάσεων (Annex C) που χρησιμοποιούνται από orchestration/UI.
    /// Returns recognized strategic mitigations for ARC residual mapping and UI.
    /// </summary>
    IEnumerable<Skyworks.Core.Models.ARC.StrategicMitigationInfo> GetRecognizedStrategicMitigations();
}
