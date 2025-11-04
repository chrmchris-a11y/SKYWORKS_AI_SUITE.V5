using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GRC;

/// <summary>
/// Επίπεδο Robustness (Αξιοπιστίας) για μετριασμούς GRC — κοινό για SORA 2.0 AMC και SORA 2.5.
/// </summary>
/// <remarks>
/// Αναφορά σε επίσημα κείμενα:
/// • SORA 2.0 AMC: Πίνακες 6–8 (M1, M2, M3) ορίζουν credits ανά Low/Medium/High.
/// • SORA 2.5: Πίνακας 5 (M1A, M1B, M1C, M2) ορίζει credits ανά Low/Medium/High.
/// 
/// Σημαντικό: Τα credits (πόσο μειώνεται το GRC) ΔΕΝ είναι ιδιότητα του enum· εξαρτώνται από τον τύπο μετριασμού
/// και την έκδοση SORA. Η χαρτογράφηση υλοποιείται στις μεθόδους του service:
/// • GetMitigationCredit_V2_0 για SORA 2.0 AMC
/// • GetMitigationCredit_V2_5 για SORA 2.5
/// 
/// Βλ. επίσης τα DTOs: AppliedMitigation(_V2_0), FinalGRCInput(_V2_0), FinalGRCResult(_V2_0) και αντίστοιχα για v2.5.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RobustnessLevel
{
    /// <summary>
    /// Καμία εφαρμογή/μη διαθέσιμο. Δεν αποδίδονται credits.
    /// </summary>
    None = 0,

    /// <summary>
    /// Χαμηλή αξιοπιστία μετριασμού (Low). Τα credits εξαρτώνται από τον τύπο μετριασμού και την έκδοση SORA.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Μεσαία αξιοπιστία μετριασμού (Medium). Τα credits εξαρτώνται από τον τύπο μετριασμού και την έκδοση SORA.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Υψηλή αξιοπιστία μετριασμού (High). Τα credits εξαρτώνται από τον τύπο μετριασμού και την έκδοση SORA.
    /// </summary>
    High = 3
}
