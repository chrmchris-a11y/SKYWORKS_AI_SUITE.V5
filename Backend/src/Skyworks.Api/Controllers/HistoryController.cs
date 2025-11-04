using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.History;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly IHistoricalAnalysisService _history;
    public HistoryController(IHistoricalAnalysisService history)
    {
        _history = history;
    }

    /// <summary>
    /// Analyze mission history for trends and statistics.
    /// Supports both SORA 2.0 and 2.5 (filter via SoraVersion field or request.SoraVersion).
    /// </summary>
    [HttpPost("analyze")]
    public ActionResult<HistoricalAnalysisResult> Analyze([FromBody] HistoricalAnalysisRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required" });
        var res = _history.Analyze(request);
        return Ok(res);
    }
}
