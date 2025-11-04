using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core.Services.GRC;

namespace Skyworks.Core.Services;

public interface ISAILComplianceService
{
    SAILComplianceResult CheckCompliance(SAILComplianceRequest request);
}

public class SAILComplianceService : ISAILComplianceService
{
    private readonly ISAILService _sailService;
    private readonly IOSOService _osoService;
    private readonly IGRCCalculationService _grcService;

    public SAILComplianceService(ISAILService sailService, IOSOService osoService, IGRCCalculationService grcService)
    {
        _sailService = sailService;
        _osoService = osoService;
        _grcService = grcService;
    }

    public SAILComplianceResult CheckCompliance(SAILComplianceRequest request)
    {
        // 1) Determine SAIL per official matrix
        var sailRes = _sailService.DetermineSAIL(new SAILInput
        {
            FinalGRC = request.FinalGRC,
            ResidualARC = request.ResidualARC
        });

        if (!sailRes.IsSupported || !sailRes.SAIL.HasValue)
        {
            return new SAILComplianceResult
            {
                IsCompliant = false,
                SAIL = null,
                TMPR = null,
                OSOCompliance = null,
                Notes = $"SAIL not supported: {sailRes.Notes}"
            };
        }

        var sail = sailRes.SAIL.Value;

        // 2) Determine TMPR from Residual ARC (SORA 2.0/2.5 consistent mapping)
        var tmpr = request.SoraVersion == "2.0"
            ? _grcService.DetermineTMPR_V2_0(request.ResidualARC)
            : _grcService.DetermineTMPR_V2_5(request.ResidualARC);

        // 3) Get OSO requirements and validate compliance
        var osoReqs = _osoService.GetOSORequirements(sail, request.SoraVersion);
        var osoCompliance = _osoService.ValidateOSOCompliance(
            sail,
            request.ImplementedOSOs ?? new List<OSOImplementation>(),
            request.SoraVersion
        );

        // 4) Build summary
        var ok = osoCompliance.IsCompliant;
        var notes = $"SAIL = {sail} ({sailRes.Notes.Split('\n')[0]}). TMPR: {tmpr.Level} ({tmpr.Robustness}).\n" +
                    $"OSO: required={osoReqs.RequiredCount}, missing={osoCompliance.MissingOSOs.Count}, insufficient={osoCompliance.InsufficientRobustness.Count}.";

        return new SAILComplianceResult
        {
            IsCompliant = ok,
            SAIL = sail,
            TMPR = tmpr,
            OSOCompliance = osoCompliance,
            Notes = notes,
            SoraVersion = request.SoraVersion
        };
    }
}

public class SAILComplianceRequest
{
    public int FinalGRC { get; set; }
    public ARCRating ResidualARC { get; set; } = ARCRating.ARC_b;
    public string SoraVersion { get; set; } = "2.5"; // "2.0" or "2.5"
    public List<OSOImplementation>? ImplementedOSOs { get; set; }
}

public class SAILComplianceResult
{
    public bool IsCompliant { get; set; }
    public SAILLevel? SAIL { get; set; }
    public TMPRResult? TMPR { get; set; }
    public OSOComplianceResult? OSOCompliance { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string SoraVersion { get; set; } = "2.5";
}
