using Skyworks.Core.Services.Orchestration;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core;

namespace Skyworks.Core.Services.Reports;

public interface IExplanatoryNoteVerificationService
{
    ExplanatoryNoteVerificationResult Verify(SORACompleteRequest request, SORACompleteResult result);
}

public class ExplanatoryNoteVerificationResult
{
    public string SoraVersion { get; set; } = "2.5";
    public string? MissionId { get; set; }
    public string? MissionName { get; set; }
    public bool Pass { get; set; }
    public int PassedCount { get; set; }
    public int OpenCount { get; set; }
    public List<VerificationItem> Items { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class VerificationItem
{
    public string Code { get; set; } = string.Empty; // e.g., EN-01
    public string Title { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty; // SORA reference string
    public string Status { get; set; } = "Open"; // Pass/Open
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Lightweight verification against the SORA Explanatory Note content expectations.
/// Uses the computed SORA pipeline output to infer checklist status and references
/// to the relevant SORA main body / annex sections available in the local KnowledgeBase.
/// </summary>
public class ExplanatoryNoteVerificationService : IExplanatoryNoteVerificationService
{
    private readonly ISAILService _sailService;
    private readonly IOSOService _osoService;

    public ExplanatoryNoteVerificationService(ISAILService sailService, IOSOService osoService)
    {
        _sailService = sailService;
        _osoService = osoService;
    }

    public ExplanatoryNoteVerificationResult Verify(SORACompleteRequest request, SORACompleteResult result)
    {
        var items = new List<VerificationItem>();

        // EN-01: ConOps summary present
        items.Add(new VerificationItem
        {
            Code = "EN-01",
            Title = "Concept of Operations (ConOps) described",
            Reference = "SORA Main Body §3–4 (Concept of Operations)",
            Status = string.IsNullOrWhiteSpace(request.MissionDescription) ? "Open" : "Pass",
            Details = string.IsNullOrWhiteSpace(request.MissionDescription)
                ? "MissionDescription is empty. Provide a short ConOps: location, timeframe, participants, command & control."
                : "ConOps text provided in MissionDescription."
        });

        // EN-02: GRC path documented
        var grcOk = result.IntrinsicGRC.HasValue && result.FinalGRC.HasValue && string.IsNullOrWhiteSpace(result.GroundRiskNotes) == false;
        items.Add(new VerificationItem
        {
            Code = "EN-02",
            Title = "Ground Risk (iGRC → Final GRC) documented with mitigations",
            Reference = request.SoraVersion == "2.0" ? "JARUS SORA AMC 2.0 – M1/M2/M3; Table 5" : "JARUS SORA 2.5 – Annex B (M1A/M1B/M1C, M2); Table 3",
            Status = grcOk ? "Pass" : "Open",
            Details = grcOk ? $"iGRC={result.IntrinsicGRC}, FinalGRC={result.FinalGRC}. Notes: {result.GroundRiskNotes}" : "Provide intrinsic and final GRC plus justification of applied mitigations."
        });

        // EN-03: ARC path documented
        var arcOk = result.InitialARC.HasValue && result.ResidualARC.HasValue && string.IsNullOrWhiteSpace(result.AirRiskNotes) == false;
        items.Add(new VerificationItem
        {
            Code = "EN-03",
            Title = "Air Risk (Initial ARC → Residual ARC) with strategic mitigations",
            Reference = request.SoraVersion == "2.0" ? "SORA 2.0 – Annex C (strategic mitigations)" : "SORA 2.5 – Annex C (strategic mitigations)",
            Status = arcOk ? "Pass" : "Open",
            Details = arcOk ? $"Initial={result.InitialARC}, Residual={result.ResidualARC}. Notes: {result.AirRiskNotes}" : "Provide initial/residual ARC and the applied strategic mitigations with rationale."
        });

        // EN-04: SAIL + TMPR captured
        var sailOk = result.SAIL.HasValue && result.TMPRDetails != null;
        items.Add(new VerificationItem
        {
            Code = "EN-04",
            Title = "SAIL and TMPR determination recorded",
            Reference = request.SoraVersion == "2.0" ? "SORA 2.0 – Table 5 (SAIL) / Annex D (TMPR)" : "SORA 2.5 – Table 7 (SAIL) / Annex D (TMPR)",
            Status = sailOk ? "Pass" : "Open",
            Details = sailOk ? $"SAIL={result.SAIL}, TMPR={result.TMPRDetails?.Level} ({result.TMPRDetails?.Robustness})" : "Provide SAIL and corresponding TMPR level with justification."
        });

        // EN-05: OSO evidence readiness
        var required = result.SAIL.HasValue ? _osoService.GetOSORequirements(result.SAIL.Value, request.SoraVersion) : null;
        var osoOk = result.IsCompliant && required != null && result.ImplementedOSOCount >= required.RequiredCount;
        items.Add(new VerificationItem
        {
            Code = "EN-05",
            Title = "OSO evidence plan matches SAIL requirements",
            Reference = request.SoraVersion == "2.0" ? "SORA 2.0 – Table 6 (OSO per SAIL)" : "SORA 2.5 – Table 14 (OSO per SAIL)",
            Status = osoOk ? "Pass" : "Open",
            Details = required == null
                ? "SAIL not available to map OSO requirements."
                : osoOk
                    ? $"ImplementedOSO={result.ImplementedOSOCount} / Required={required.RequiredCount}"
                    : $"Missing OSO or insufficient robustness. Implemented={result.ImplementedOSOCount}, Required={required.RequiredCount}"
        });

        // EN-06: Residual risk and ALARP statement (proxy via warnings/errors)
        var alarpOk = (result.Errors?.Count ?? 0) == 0; // if errors exist, ALARP assertion is not yet credible
        items.Add(new VerificationItem
        {
            Code = "EN-06",
            Title = "Residual risk statement (ALARP) and limitations",
            Reference = "SORA Main Body §7–8 (Residual risk & ALARP)",
            Status = alarpOk ? "Pass" : "Open",
            Details = alarpOk ? "No blocking errors; proceed to authoring explicit ALARP text in the note." : "Resolve errors before asserting ALARP; list limitations and remaining risks."
        });

        var passed = items.Count(i => i.Status == "Pass");
        var open = items.Count - passed;

        return new ExplanatoryNoteVerificationResult
        {
            SoraVersion = request.SoraVersion,
            MissionId = request.MissionId,
            MissionName = request.MissionName,
            Pass = open == 0,
            PassedCount = passed,
            OpenCount = open,
            Items = items,
            Summary = $"Explanatory Note checklist: {passed}/{items.Count} passed, {open} open."
        };
    }
}
