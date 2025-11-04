using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Orchestration;
using Skyworks.Core.Services.Reports;
using Microsoft.Extensions.Localization;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/sora/explanatory-note")]
public class ExplanatoryNoteController : ControllerBase
{
    private readonly ISORAOrchestrationService _orchestration;
    private readonly IExplanatoryNoteVerificationService _verification;
    private readonly IStringLocalizer<Resources.SharedResources> _localizer;

    public ExplanatoryNoteController(
        ISORAOrchestrationService orchestration,
        IExplanatoryNoteVerificationService verification,
        IStringLocalizer<Resources.SharedResources> localizer)
    {
        _orchestration = orchestration;
        _verification = verification;
        _localizer = localizer;
    }

    /// <summary>
    /// Verify Explanatory Note checklist against the computed SORA pipeline outputs.
    /// Returns a structured checklist with SORA references and pass/open status.
    /// </summary>
    [HttpPost("verify")]
    public ActionResult<ExplanatoryNoteVerificationResult> Verify([FromBody] SORACompleteRequest request)
    {
        if (request == null)
            return BadRequest(new { error = _localizer["RequestBodyRequired"].Value });
        if (request.SoraVersion != "2.0" && request.SoraVersion != "2.5")
            return BadRequest(new { error = _localizer["InvalidSoraVersion"].Value });

        var result = _orchestration.ExecuteComplete(request);
        if (!result.IsSuccessful)
            return BadRequest(result);

        var verification = _verification.Verify(request, result);
        return Ok(verification);
    }
}
