using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Orchestration;
using Microsoft.Extensions.Localization;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/sora")]
public class SORAController : ControllerBase
{
    private readonly ISORAOrchestrationService _orchestration;
    private readonly IStringLocalizer<Resources.SharedResources> _localizer;

    public SORAController(
        ISORAOrchestrationService orchestration,
        IStringLocalizer<Resources.SharedResources> localizer)
    {
        _orchestration = orchestration;
        _localizer = localizer;
    }

    /// <summary>
    /// Execute complete SORA assessment pipeline: GRC → ARC → SAIL → OSO → TMPR
    /// Supports both SORA 2.0 AMC and SORA 2.5
    /// </summary>
    /// <param name="request">Complete SORA assessment request with mission details, ground/air risk inputs, and OSO implementations</param>
    /// <returns>Comprehensive SORA assessment result with all intermediate and final outputs</returns>
    [HttpPost("complete")]
    public ActionResult<SORACompleteResult> ExecuteComplete([FromBody] SORACompleteRequest request)
    {
        if (request == null)
            return BadRequest(new { error = _localizer["RequestBodyRequired"].Value });

        if (request.SoraVersion != "2.0" && request.SoraVersion != "2.5")
            return BadRequest(new { error = _localizer["InvalidSoraVersion"].Value });

        var result = _orchestration.ExecuteComplete(request);

        // FIX #9: Return HTTP 400 for out-of-scope operations (SAIL VI, GRC≥6, GRC≥5+ARC_d)
        // FIX #9: Per EASA Easy Access Rules GM1 Article 11: Operations beyond SPECIFIC category scope
        if (!result.IsSuccessful)
        {
            // Uniform error envelope for 400 responses, plus keep flat fields for compatibility
            var errorPayload = new
            {
                error = "SORA pipeline rejected",
                reason = string.Join(" | ", result.Errors ?? new List<string>()),
                reasonCode = result.ReasonCode ?? "UNKNOWN",
                // Mirror the successful schema inside 'result' for traceability
                result,
                // Keep key fields flat to preserve backward compatibility
                isSuccessful = result.IsSuccessful,
                soraVersion = result.SoraVersion,
                intrinsicGRC = result.IntrinsicGRC,
                finalGRC = result.FinalGRC,
                initialARC = result.InitialARC,
                residualARC = result.ResidualARC,
                SAIL = result.SAIL,
                errors = result.Errors
            };
            return BadRequest(errorPayload);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get SORA process information and supported versions
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetInfo()
    {
        return Ok(new
        {
            name = _localizer["SoraPipelineName"].Value,
            description = _localizer["SoraPipelineDescription"].Value,
            supportedVersions = new[] { "2.0", "2.5" },
            pipeline = new[]
            {
                _localizer["SoraPipelineStep1"].Value,
                _localizer["SoraPipelineStep2"].Value,
                _localizer["SoraPipelineStep3"].Value,
                _localizer["SoraPipelineStep4"].Value,
                _localizer["SoraPipelineStep5"].Value,
                _localizer["SoraPipelineStep6"].Value
            },
            endpoints = new
            {
                complete = "POST /api/sora/complete",
                info = "GET /api/sora/info"
            }
        });
    }
}
