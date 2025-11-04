using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Risk;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/risk")] 
public class RiskController : ControllerBase
{
    private readonly IRiskModelingService _risk;

    public RiskController(IRiskModelingService risk)
    {
        _risk = risk;
    }

    [HttpPost("assess")]
    public ActionResult<RiskAssessmentResult> Assess([FromBody] RiskAssessmentRequest request)
    {
        var res = _risk.AssessOperationalRisk(request);
        return Ok(res);
    }
}
