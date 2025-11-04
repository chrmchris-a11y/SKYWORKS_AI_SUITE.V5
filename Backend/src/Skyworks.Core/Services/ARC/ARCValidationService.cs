using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.ARC.Validation;

namespace Skyworks.Core.Services.ARC;

public class ARCValidationService : IARCValidationService
{
    public ARCValidationResult ValidateEnvironment_V2_5(ARCEnvironmentInput env)
    {
        var res = new ARCValidationResult();

        // Height guidance (typical 120 m AGL limit)
        if (env.MaxHeightAGL > 120.0)
        {
            res.Issues.Add(new ValidationIssue
            {
                Severity = RuleSeverity.Warning,
                Code = "HEIGHT.AGL.>120",
                Message = "MaxHeightAGL > 120 m. Επιβεβαιώστε ότι επιτρέπεται από την Αρχή (SORA specific category)."
            });
        }

        // Controlled airspace => coordination typically required
        if (env.AirspaceControl == AirspaceControl.Controlled)
        {
            res.Issues.Add(new ValidationIssue
            {
                Severity = RuleSeverity.Warning,
                Code = "AIRSPACE.CONTROLLED",
                Message = "Ελεγχόμενος εναέριος χώρος: πιθανή απαίτηση συντονισμού με ANSP/ATC."
            });
        }

        // Airport/Heliport locations => stricter constraints
        if (env.LocationType == LocationType.Airport || env.LocationType == LocationType.Heliport)
        {
            res.Issues.Add(new ValidationIssue
            {
                Severity = RuleSeverity.Warning,
                Code = "LOCATION.AERODROME",
                Message = "Περιοχή αεροδρομίου/ελικοδρομίου: απαιτούνται ειδικές εγκρίσεις/διαδικασίες."
            });
        }

        // Atypical/Segregated claim => explicit acceptance
        if (env.Typicality == AirspaceTypicality.AtypicalSegregated)
        {
            res.Issues.Add(new ValidationIssue
            {
                Severity = RuleSeverity.Info,
                Code = "TYPICALITY.ATYPICAL",
                Message = "Δηλώθηκε Atypical/Segregated: απαιτείται αποδοχή από Αρχή (Annex G 3.20)."
            });
        }

        res.Notes = "Heuristic validation (Annex B oriented). Προσθήκη τοπικών κανόνων ανά Αρχή επιτρέπεται.";
        return res;
    }
}
