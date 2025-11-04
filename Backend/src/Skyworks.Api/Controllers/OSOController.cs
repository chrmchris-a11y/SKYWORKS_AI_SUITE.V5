using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core.Services;

namespace Skyworks.Api.Controllers;

/// <summary>
/// OSO (Operational Safety Objectives) Controller - Step 32
/// JARUS SORA 2.0 Table 6 (24 OSOs) and SORA 2.5 Table 14 (17 OSOs)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OSOController : ControllerBase
{
    private readonly IOSOService _osoService;

    public OSOController(IOSOService osoService)
    {
        _osoService = osoService;
    }

    /// <summary>
    /// Get OSO requirements for a SAIL level
    /// </summary>
    /// <param name="sail">SAIL level (I-VI or 1-6)</param>
    /// <param name="soraVersion">SORA version (2.0 or 2.5)</param>
    [HttpGet("requirements/{sail}")]
    public ActionResult<OSORequirementsResult> GetOSORequirements(string sail, [FromQuery] string soraVersion = "2.5")
    {
        try
        {
            var sailLevel = ParseSAIL(sail);
            var result = _osoService.GetOSORequirements(sailLevel, soraVersion);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate OSO compliance for an operation
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<OSOComplianceResult> ValidateCompliance([FromBody] OSOComplianceRequest request)
    {
        try
        {
            var result = _osoService.ValidateOSOCompliance(
                request.SAIL,
                request.ImplementedOSOs,
                request.SORAVersion ?? "2.5"
            );
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed information about a specific OSO
    /// </summary>
    /// <param name="osoId">OSO ID (1-24)</param>
    /// <param name="soraVersion">SORA version (2.0 or 2.5)</param>
    [HttpGet("{osoId}")]
    public ActionResult<OSODetails> GetOSODetails(int osoId, [FromQuery] string soraVersion = "2.5")
    {
        if (osoId < 1 || osoId > 24)
            return BadRequest(new { error = "OSO ID must be between 1 and 24" });

        var details = _osoService.GetOSODetails(osoId, soraVersion);
        
        if (details == null)
            return NotFound(new { error = $"OSO#{osoId} not found in SORA {soraVersion}" });

        return Ok(details);
    }

    /// <summary>
    /// Compare OSO requirements between SORA 2.0 and 2.5
    /// </summary>
    [HttpGet("compare/{sail}")]
    public ActionResult<OSOComparisonResult> CompareVersions(string sail)
    {
        try
        {
            var sailLevel = ParseSAIL(sail);
            var sora20 = _osoService.GetOSORequirements(sailLevel, "2.0");
            var sora25 = _osoService.GetOSORequirements(sailLevel, "2.5");

            var comparison = new OSOComparisonResult
            {
                SAIL = sailLevel,
                SORA20Count = sora20.TotalCount,
                SORA25Count = sora25.TotalCount,
                RemovedInSora25 = new List<string>
                {
                    "OSO#10 (merged into OSO#05)",
                    "OSO#11 (procedures for external systems)",
                    "OSO#12 (design for external systems)",
                    "OSO#14 (human error procedures - covered by OSO#08)",
                    "OSO#15 (human error training - covered by OSO#09)",
                    "OSO#21 (adverse conditions procedures - covered by OSO#08)",
                    "OSO#22 (adverse conditions training - covered by OSO#09)"
                },
                KeyChanges = new List<string>
                {
                    "Optional (O) changed to Not Required (NR)",
                    "Added Dependencies column (Operator/Designer/Training org)",
                    "Added Annex E Critical References",
                    "Consolidated duplicate OSOs"
                }
            };

            return Ok(comparison);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private SAILLevel ParseSAIL(string sail)
    {
        return sail.ToUpper() switch
        {
            "I" or "1" => SAILLevel.I,
            "II" or "2" => SAILLevel.II,
            "III" or "3" => SAILLevel.III,
            "IV" or "4" => SAILLevel.IV,
            "V" or "5" => SAILLevel.V,
            "VI" or "6" => SAILLevel.VI,
            _ => throw new ArgumentException($"Invalid SAIL level: {sail}. Use I-VI or 1-6.")
        };
    }
}

/// <summary>
/// OSO Compliance Request
/// </summary>
public class OSOComplianceRequest
{
    public SAILLevel SAIL { get; set; }
    public List<OSOImplementation> ImplementedOSOs { get; set; } = new();
    public string? SORAVersion { get; set; }
}

/// <summary>
/// OSO Version Comparison Result
/// </summary>
public class OSOComparisonResult
{
    public SAILLevel SAIL { get; set; }
    public int SORA20Count { get; set; }
    public int SORA25Count { get; set; }
    public List<string> RemovedInSora25 { get; set; } = new();
    public List<string> KeyChanges { get; set; } = new();
}
