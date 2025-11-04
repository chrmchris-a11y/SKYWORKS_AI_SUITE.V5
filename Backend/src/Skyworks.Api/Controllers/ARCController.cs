using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.ARC.Validation;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services.ARC;
using Skyworks.Core.Services.Airspace;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.ARC.Composite;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/arc")] 
public class ARCController : ControllerBase
{
    private readonly IGRCCalculationService _service;
    private readonly IARCValidationService _validator;
    private readonly IAirspaceProvider _airspace;
    private readonly ILogger<ARCController> _logger;
    private readonly IARCCompositeService _composite;

    public ARCController(IGRCCalculationService service, IARCValidationService validator, IAirspaceProvider airspace, IARCCompositeService composite, ILogger<ARCController> logger)
    {
        _service = service;
        _validator = validator;
        _airspace = airspace;
        _composite = composite;
        _logger = logger;
    }

    // SORA 2.0
    [HttpPost("v2.0/initial")]
    public IActionResult Initial_V20([FromBody] ARCInitialInput input)
    {
        var result = _service.DetermineInitialARC_V2_0(input);
        return Ok(result);
    }

    // Composite Initial ARC (SORA 2.5) across segments/time windows
    [HttpPost("v2.5/composite/initial")]
    public IActionResult CompositeInitial_V25([FromBody] CompositeArcRequest request)
    {
        var res = _composite.ComputeInitial_V2_5(request);
        return Ok(res);
    }

    [HttpPost("v2.0/initial/environment")]
    public IActionResult Initial_V20_FromEnvironment([FromBody] ARCEnvironmentInput env)
    {
        var result = _service.DetermineInitialARC_V2_0(env);
        return Ok(result);
    }

    [HttpPost("v2.0/residual")]
    public IActionResult Residual_V20([FromBody] ARCResidualInput input)
    {
        var result = _service.DetermineResidualARC_V2_0(input);
        return Ok(result);
    }

    // SORA 2.5
    [HttpPost("v2.5/initial")]
    public IActionResult Initial_V25([FromBody] ARCInitialInput input)
    {
        var result = _service.DetermineInitialARC_V2_5(input);
        return Ok(result);
    }

    [HttpPost("v2.5/initial/environment")]
    public IActionResult Initial_V25_FromEnvironment([FromBody] ARCEnvironmentInput env)
    {
        var result = _service.DetermineInitialARC_V2_5(env);
        return Ok(result);
    }

    [HttpPost("v2.5/residual")]
    public IActionResult Residual_V25([FromBody] ARCResidualInput input)
    {
        var result = _service.DetermineResidualARC_V2_5(input);
        return Ok(result);
    }

    // Validation: SORA 2.5 (environment-based)
    [HttpPost("v2.5/validate/environment")]
    public IActionResult ValidateEnv_V25([FromBody] ARCEnvironmentInput env)
    {
        var res = _validator.ValidateEnvironment_V2_5(env);
        return Ok(res);
    }

    // Validation at point: builds env from airspace control (demo) assuming NonAirport Typical, maps only control
    [HttpGet("v2.5/validate/at-point")]
    public async Task<IActionResult> ValidateAtPoint_V25([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double maxHeightAGL = 60)
    {
        var point = new GeoPoint { Latitude = latitude, Longitude = longitude, AltitudeMeters = maxHeightAGL };
        var asp = await _airspace.GetAirspacesAtPointAsync(point);
        var isControlled = asp.Airspaces.Any(a => a.Type == Core.Models.Airspace.AirspaceType.CTR || a.Type == Core.Models.Airspace.AirspaceType.TMA || (a.Class.HasValue && a.Class.Value != Core.Models.Airspace.AirspaceClass.G));

        var env = new ARCEnvironmentInput
        {
            AirspaceControl = isControlled ? AirspaceControl.Controlled : AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Urban, // Default to Urban (JARUS Annex C Table 1)
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = maxHeightAGL
        };

        var res = _validator.ValidateEnvironment_V2_5(env);
        return Ok(new { Env = env, Validation = res });
    }

    // TMPR helpers
    [HttpGet("v2.0/tmpr")]
    public IActionResult TMPR_V20([FromQuery] ARCRating residualArc)
    {
        var res = _service.DetermineTMPR_V2_0(residualArc);
        return Ok(res);
    }

    [HttpGet("v2.5/tmpr")]
    public IActionResult TMPR_V25([FromQuery] ARCRating residualArc)
    {
        var res = _service.DetermineTMPR_V2_5(residualArc);
        return Ok(res);
    }

    // Annex C helper: list recognized strategic mitigations (same for 2.0/2.5)
    [HttpGet("strategic-mitigations")]
    public IActionResult StrategicMitigations()
    {
        var list = _service.GetRecognizedStrategicMitigations();
        return Ok(list);
    }
}
