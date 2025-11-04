using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.Traffic;
using Skyworks.Core.Services.Traffic;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/traffic")]
public class TrafficController : ControllerBase
{
    private readonly ITrafficProvider _trafficProvider;

    public TrafficController(ITrafficProvider trafficProvider)
    {
        _trafficProvider = trafficProvider;
    }

    /// <summary>
    /// Get traffic density for a specific area
    /// </summary>
    [HttpPost("density")]
    public async Task<ActionResult<TrafficQueryResult>> GetDensity([FromBody] TrafficQuery query)
    {
        var result = await _trafficProvider.GetTrafficDensityAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get nearby traffic targets within radius
    /// </summary>
    [HttpPost("nearby")]
    public async Task<ActionResult<List<TrafficTarget>>> GetNearby([FromBody] TrafficQuery query)
    {
        var targets = await _trafficProvider.GetNearbyTargetsAsync(query);
        return Ok(targets);
    }
}
