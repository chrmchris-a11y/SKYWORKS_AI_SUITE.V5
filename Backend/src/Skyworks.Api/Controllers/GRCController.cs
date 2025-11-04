using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Services.GRC;

namespace Skyworks.Api.Controllers;

/// <summary>
/// Ground Risk Classification (GRC) API — EASA/JARUS aligned (SORA 2.0 AMC & SORA 2.5).
/// </summary>
/// <remarks>
/// • SORA 2.0 AMC: Table 2 (iGRC), Tables 6–8 (M1/M2/M3) → Final GRC ≤ 7.
/// • SORA 2.5: Table 2 (population × dimension/speed), M1A/M1B/M1C/M2 → Final GRC ≤ 7.
/// • Special rule (SORA 2.5): ≤250 g and ≤25 m/s ⇒ iGRC = 1.
/// These endpoints delegate to <see cref="IGRCCalculationService"/> which encapsulates the
/// authoritative logic and returns DTOs with explanatory notes for auditability.
/// </remarks>
[ApiController]
[Route("api/grc")]
public class GRCController : ControllerBase
{
    private readonly IGRCCalculationService _grcService;
    private readonly ILogger<GRCController> _logger;

    public GRCController(IGRCCalculationService grcService, ILogger<GRCController> logger)
    {
        _grcService = grcService;
        _logger = logger;
    }

    #region SORA 2.0 AMC Endpoints

    /// <summary>
    /// Calculate Intrinsic GRC using SORA 2.0 AMC methodology.
    /// </summary>
    /// <remarks>
    /// SORA 2.0 AMC Table 2: 7 operational scenarios × 4 UAS dimensions.
    /// </remarks>
    [HttpPost("v2.0/intrinsic")]
    [ProducesResponseType(typeof(IntrinsicGRCResult_V2_0), StatusCodes.Status200OK)]
    public IActionResult CalculateIntrinsicGRC_V2_0([FromBody] IntrinsicGRCInput_V2_0 input)
    {
        try
        {
            var result = _grcService.CalculateIntrinsicGRC_V2_0(input);
            _logger.LogInformation("SORA 2.0 iGRC calculated: {IGRC} for scenario {Scenario}, dimension {Dimension}",
                result.IGRC, result.Scenario, result.DimensionCategory);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SORA 2.0 iGRC");
            return StatusCode(500, new { error = "Failed to calculate intrinsic GRC", details = ex.Message });
        }
    }

    /// <summary>
    /// Calculate Final GRC with mitigations using SORA 2.0 AMC (M1/M2/M3)
    /// </summary>
    [HttpPost("v2.0/final")]
    [ProducesResponseType(typeof(FinalGRCResult_V2_0), StatusCodes.Status200OK)]
    public IActionResult CalculateFinalGRC_V2_0([FromBody] FinalGRCInput_V2_0 input)
    {
        try
        {
            var result = _grcService.CalculateFinalGRC_V2_0(input);
            _logger.LogInformation("SORA 2.0 Final GRC: {FinalGRC} (iGRC={IGRC}, reduction={Reduction})",
                result.FinalGRC, input.IntrinsicGRC, result.TotalGRCReduction);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SORA 2.0 final GRC");
            return StatusCode(500, new { error = "Failed to calculate final GRC", details = ex.Message });
        }
    }

    #endregion

    #region SORA 2.5 Endpoints

    /// <summary>
    /// Calculate Intrinsic GRC using SORA 2.5 methodology.
    /// </summary>
    /// <remarks>
    /// SORA 2.5 Table 2: 7 population density categories × 5 UAS dimensions.
    /// Special case: ≤250 g & ≤25 m/s → iGRC = 1.
    /// </remarks>
    [HttpPost("v2.5/intrinsic")]
    [ProducesResponseType(typeof(IntrinsicGRCResult), StatusCodes.Status200OK)]
    public IActionResult CalculateIntrinsicGRC_V2_5([FromBody] IntrinsicGRCInput input)
    {
        try
        {
            var result = _grcService.CalculateIntrinsicGRC_V2_5(input);
            _logger.LogInformation("SORA 2.5 iGRC calculated: {IGRC} for population {PopCat}, dimension {DimCat}",
                result.IGRC, result.PopulationCategory, result.DimensionCategory);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SORA 2.5 iGRC");
            return StatusCode(500, new { error = "Failed to calculate intrinsic GRC", details = ex.Message });
        }
    }

    /// <summary>
    /// Calculate Final GRC with mitigations using SORA 2.5 (M1A/M1B/M1C/M2)
    /// </summary>
    [HttpPost("v2.5/final")]
    [ProducesResponseType(typeof(FinalGRCResult), StatusCodes.Status200OK)]
    public IActionResult CalculateFinalGRC_V2_5([FromBody] FinalGRCInput input)
    {
        try
        {
            var result = _grcService.CalculateFinalGRC_V2_5(input);
            _logger.LogInformation("SORA 2.5 Final GRC: {FinalGRC} (iGRC={IGRC}, reduction={Reduction}, valid={IsValid})",
                result.FinalGRC, input.IntrinsicGRC, result.TotalGRCReduction, result.IsValid);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SORA 2.5 final GRC");
            return StatusCode(500, new { error = "Failed to calculate final GRC", details = ex.Message });
        }
    }

    /// <summary>
    /// Get population density category (SORA 2.5)
    /// </summary>
    [HttpGet("v2.5/population-category")]
    public IActionResult GetPopulationCategory([FromQuery] double density, [FromQuery] bool controlled = false)
    {
        try
        {
            var category = _grcService.GetPopulationCategory(density, controlled);
            return Ok(new { density, controlled, category = category.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining population category");
            return StatusCode(500, new { error = "Failed to determine population category", details = ex.Message });
        }
    }

    #endregion

    #region Helper Endpoints

    /// <summary>
    /// Get supported SORA versions
    /// </summary>
    [HttpGet("versions")]
    public IActionResult GetVersions()
    {
        return Ok(new
        {
            versions = new object[]
            {
                new
                {
                    version = "2.0",
                    name = "SORA 2.0 AMC",
                    reference = "JAR-DEL-WG6-D.04, Edition 2.0, 30.01.2019",
                    scenarios = 7,
                    dimensions = 4,
                    mitigations = new[] { "M1_Strategic", "M2_ImpactReduction", "M3_ERP" }
                },
                new
                {
                    version = "2.5",
                    name = "JARUS SORA 2.5",
                    reference = "JAR-DEL-SRM-SORA-MB-2.5",
                    populationBands = 7,
                    dimensions = 5,
                    mitigations = new[] { "M1A_Sheltering", "M1B_OperationalRestrictions", "M1C_GroundObservation", "M2_ImpactDynamics" },
                    specialRules = new[] { "≤250g & ≤25m/s → iGRC=1" }
                }
            }
        });
    }

    #endregion
}
