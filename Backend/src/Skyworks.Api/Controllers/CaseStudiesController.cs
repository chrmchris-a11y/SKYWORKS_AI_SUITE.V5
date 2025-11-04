using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Orchestration;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core.Services;
using Microsoft.Extensions.Localization;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/sora/case-studies")]
public class CaseStudiesController : ControllerBase
{
    private readonly ISORAOrchestrationService _orchestration;
    private readonly IStringLocalizer<Resources.SharedResources> _localizer;

    public CaseStudiesController(
        ISORAOrchestrationService orchestration,
        IStringLocalizer<Resources.SharedResources> localizer)
    {
        _orchestration = orchestration;
        _localizer = localizer;
    }

    private static readonly Dictionary<string, SORACompleteRequest> Presets = new(StringComparer.OrdinalIgnoreCase)
    {
        // SORA 2.5 – Low exposure suburban BVLOS with strategic mitigations leading to low SAIL
        ["2.5_low_suburban_bvlos"] = new SORACompleteRequest
        {
            SoraVersion = "2.5",
            MissionId = "CS-25-LOW",
            MissionName = "BVLOS Suburban Inspection",
            MissionDescription = "Linear BVLOS inspection over suburban area with NOTAM and geofencing.",
            GroundRisk = new GroundRiskInput
            {
                MTOM_kg = 1.2,
                PopulationDensity = 600,
                IsControlledGroundArea = true,
                MaxCharacteristicDimension = 1.0,
                MaxSpeed = 15,
                Mitigations = new List<MitigationInput>
                {
                    new MitigationInput{ Type="M1A", Robustness=RobustnessLevel.Medium },
                    new MitigationInput{ Type="M2", Robustness=RobustnessLevel.Medium }
                }
            },
            AirRisk = new AirRiskInput
            {
                AirspaceControl = "Uncontrolled",
                LocationType = "NonAirport",
                Environment = "Suburban",
                Typicality = "Typical",
                MaxHeightAGL = 100,
                StrategicMitigations = new List<string>{"S1","S2"},
                LocalDensityRating = 2
            },
            ImplementedOSOs = new List<OSOImplementation>{
                new OSOImplementation{ OSOId = 1, AchievedRobustness = OSORobustnessLevel.Medium },
                new OSOImplementation{ OSOId = 2, AchievedRobustness = OSORobustnessLevel.Medium }
            }
        },

        // SORA 2.0 – Medium VLOS scenario with legacy reductions path
        ["2.0_medium_vlos"] = new SORACompleteRequest
        {
            SoraVersion = "2.0",
            MissionId = "CS-20-MED",
            MissionName = "VLOS Medium Risk",
            MissionDescription = "VLOS operation over sparsely populated area.",
            GroundRisk = new GroundRiskInput
            {
                Scenario_V2_0 = OperationalScenario.VLOS_SparselyPopulated,
                MaxCharacteristicDimension = 0.7,
                KineticEnergy = 50,
                Mitigations = new List<MitigationInput>
                {
                    new MitigationInput{ Type="M1", Robustness=RobustnessLevel.Low },
                    new MitigationInput{ Type="M2", Robustness=RobustnessLevel.Low }
                }
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = ARCRating.ARC_c,
                StrategicMitigations = new List<string>{"S1","S3"}
            },
            ImplementedOSOs = new List<OSOImplementation>{ new OSOImplementation{ OSOId = 1, AchievedRobustness = OSORobustnessLevel.Low } }
        }
    };

    [HttpGet]
    public IActionResult List() => Ok(Presets.Keys.OrderBy(k => k).ToArray());

    [HttpGet("{id}")]
    public ActionResult<SORACompleteRequest> Get(string id)
    {
        if (!Presets.TryGetValue(id, out var req))
            return NotFound(new { error = _localizer["CaseStudyNotFound"].Value });
        return Ok(req);
    }

    [HttpPost("{id}/run")]
    public ActionResult<object> Run(string id)
    {
        if (!Presets.TryGetValue(id, out var req))
            return NotFound(new { error = _localizer["CaseStudyNotFound"].Value });
        var result = _orchestration.ExecuteComplete(req);
        return result.IsSuccessful ? Ok(result) : BadRequest(result);
    }
}
