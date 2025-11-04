using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Airports;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/airports")]
public class AirportsController : ControllerBase
{
    private readonly IAirportDeclaredDistancesProvider _provider;

    public AirportsController(IAirportDeclaredDistancesProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Returns declared distances per runway for the given ICAO (TORA/TODA/ASDA/LDA). Static sample dataset.
    /// </summary>
    [HttpGet("{icao}/runways")]
    public ActionResult<IEnumerable<RunwayDeclaredDistances>> GetRunways(string icao)
    {
        if (string.IsNullOrWhiteSpace(icao))
            return BadRequest(new { error = "ICAO code is required" });

        var rows = _provider.GetRunwaysByIcao(icao);
        return Ok(rows);
    }
}
