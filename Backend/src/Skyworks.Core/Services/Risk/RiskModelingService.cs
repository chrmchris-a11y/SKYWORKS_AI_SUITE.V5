using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services.Risk;

public interface IRiskModelingService
{
    RiskAssessmentResult AssessOperationalRisk(RiskAssessmentRequest request);
}

public class RiskAssessmentRequest
{
    public string SoraVersion { get; set; } = "2.5";
    public int FinalGRC { get; set; }
    public ARCRating ResidualARC { get; set; }
    public SAILLevel SAIL { get; set; }
    public string OperationType { get; set; } = "VLOS"; // VLOS/BVLOS
    public string Environment { get; set; } = "NonUrban"; // Urban/NonUrban
    public int MissionDurationMinutes { get; set; } = 10;
}

public class RiskAssessmentResult
{
    public int Score { get; set; } // 1..10
    public string Band { get; set; } = "Low"; // Low/Moderate/High/VeryHigh
    public List<string> Factors { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Heuristic risk assessment correlating SAIL, ARC, GRC και επιχειρησιακά χαρακτηριστικά.
/// Δεν αντικαθιστά επίσημες απαιτήσεις SORA/OSO· είναι συμβουλευτική έξοδος.
/// </summary>
public class RiskModelingService : IRiskModelingService
{
    public RiskAssessmentResult AssessOperationalRisk(RiskAssessmentRequest r)
    {
        var factors = new List<string>();

        // Baseline από SAIL (I..VI -> 1..6)
        var baseline = r.SAIL switch
        {
            SAILLevel.I => 1,
            SAILLevel.II => 2,
            SAILLevel.III => 3,
            SAILLevel.IV => 4,
            SAILLevel.V => 5,
            SAILLevel.VI => 6,
            _ => 3
        };
        var score = baseline;
        factors.Add($"Baseline από SAIL {r.SAIL}: {baseline}");

        // ARC επίδραση
        switch (r.ResidualARC)
        {
            case ARCRating.ARC_b: score += 1; factors.Add("ARC-b: +1"); break;
            case ARCRating.ARC_c: score += 2; factors.Add("ARC-c: +2"); break;
            case ARCRating.ARC_d: score += 3; factors.Add("ARC-d: +3"); break;
        }

        // GRC υψηλές τιμές
        if (r.FinalGRC >= 6) { score += 2; factors.Add("Final GRC ≥6: +2"); }
        else if (r.FinalGRC == 5) { score += 1; factors.Add("Final GRC 5: +1"); }

        // Περιβάλλον
        if (string.Equals(r.Environment, "Urban", StringComparison.OrdinalIgnoreCase))
        { score += 1; factors.Add("Urban περιβάλλον: +1"); }

        // Τύπος λειτουργίας
        if (string.Equals(r.OperationType, "BVLOS", StringComparison.OrdinalIgnoreCase))
        { score += 1; factors.Add("BVLOS: +1"); }

        // Διάρκεια αποστολής
        if (r.MissionDurationMinutes > 20) { score += 1; factors.Add(">20’ διάρκεια: +1"); }
        if (r.MissionDurationMinutes > 60) { score += 1; factors.Add(">60’ διάρκεια: +1"); }

        // Clamp 1..10
        score = Math.Max(1, Math.Min(10, score));

        var band = score <= 3 ? "Low" : score <= 6 ? "Moderate" : score <= 8 ? "High" : "VeryHigh";

        var notes = "Συμβουλευτική εκτίμηση κινδύνου βάσει SAIL/ARC/GRC και επιχειρησιακών παραγόντων. " +
                    "Δεν αποτελεί επίσημη μετρική EASA/JARUS. Χρησιμοποιείται για εσωτερική ιεράρχηση και τεκμηρίωση.";

        return new RiskAssessmentResult
        {
            Score = score,
            Band = band,
            Factors = factors,
            Notes = notes
        };
    }
}
