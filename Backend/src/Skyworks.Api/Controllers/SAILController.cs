using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core.Services;
using Skyworks.Core.Services.Orchestration;
using System.Text.Json.Serialization;

namespace Skyworks.Api.Controllers;

/// <summary>
/// SAIL Determination API - JARUS SORA 2.0 Step #7
/// Determines Specific Assurance and Integrity Level (SAIL) based on Final GRC and Residual ARC
/// </summary>
[ApiController]
[Route("api/sail")] 
public class SAILController : ControllerBase
{
    private readonly ISAILService _sailService;
    private readonly ILogger<SAILController> _logger;
    private readonly ISORAOrchestrationService _orchestration;

    public SAILController(ISAILService sailService, ILogger<SAILController> logger, ISORAOrchestrationService orchestration)
    {
        _sailService = sailService;
        _logger = logger;
        _orchestration = orchestration;
    }

    /// <summary>
    /// Determine SAIL level from Final GRC and Residual ARC (SORA 2.0 Table 5)
    /// </summary>
    /// <param name="input">Final GRC (1-7) and Residual ARC (a-d)</param>
    /// <returns>SAIL level (I-VI) with compliance requirements</returns>
    [HttpPost("determine")]
    [ProducesResponseType(typeof(SAILResult), 200)]
    [ProducesResponseType(400)]
    public IActionResult DetermineSAIL([FromBody] SAILInput input)
    {
        try
        {
            _logger.LogInformation("SAIL determination requested: GRC={GRC}, ARC={ARC}", 
                input.FinalGRC, input.ResidualARC);
            
            var result = _sailService.DetermineSAIL(input);
            
            if (!result.IsSupported)
            {
                _logger.LogWarning("SAIL determination failed: {Notes}", result.Notes);
                return BadRequest(new { error = result.Notes });
            }
            
            _logger.LogInformation("SAIL determined: {SAIL} for GRC={GRC}, ARC={ARC}", 
                result.SAIL, input.FinalGRC, input.ResidualARC);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining SAIL");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Determine composite SAIL for multi-segment missions
    /// Rule: Composite SAIL = Highest segment SAIL (SORA 2.0 Section 2.4.2(g))
    /// </summary>
    /// <param name="segments">List of mission segments with individual GRC/ARC values</param>
    /// <returns>Composite SAIL with segment breakdown</returns>
    [HttpPost("composite")]
    [ProducesResponseType(typeof(CompositeSAILResult), 200)]
    [ProducesResponseType(400)]
    public IActionResult DetermineCompositeSAIL([FromBody] List<MissionSegment> segments)
    {
        try
        {
            if (segments == null || segments.Count == 0)
            {
                return BadRequest(new { error = "At least one segment required" });
            }
            
            _logger.LogInformation("Composite SAIL determination requested for {Count} segments", segments.Count);
            
            var result = _sailService.DetermineCompositeSAIL(segments);
            
            _logger.LogInformation("Composite SAIL determined: {SAIL} (critical segment: {Segment})", 
                result.CompositeSAIL, result.CriticalSegment);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining composite SAIL");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get required OSO count for a given SAIL level
    /// </summary>
    /// <param name="sail">SAIL level (I-VI)</param>
    /// <returns>Number of required Operational Safety Objectives</returns>
    [HttpGet("oso-count/{sail}")]
    [ProducesResponseType(typeof(OSOCountResult), 200)]
    public IActionResult GetOSOCount(SAILLevel sail)
    {
        var count = _sailService.GetRequiredOSOCount(sail);
        var requirements = _sailService.GetOSORobustnessRequirements(sail);
        
        return Ok(new OSOCountResult
        {
            SAIL = sail,
            RequiredOSOCount = count,
            OSORequirements = requirements,
            Reference = "JARUS SORA 2.0 Table 6 - Operational Safety Objectives"
        });
    }

    /// <summary>
    /// Calculate iGRC, Final GRC, ARC (initial/residual) and SAIL from full inputs.
    /// Thin wrapper over the SORA orchestration pipeline; returns a concise summary for UI live validation.
    /// </summary>
    /// <param name="request">Minimal SORA input payload (version, drone/ground risk, air risk). OSOs optional.</param>
    /// <returns>Calculated values and advisory notes</returns>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(CalculateSAILResponse), 200)]
    [ProducesResponseType(400)]
    public IActionResult Calculate([FromBody] SORACompleteRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required" });

            if (request.SoraVersion != "2.0" && request.SoraVersion != "2.5")
                return BadRequest(new { error = "Invalid SORA version. Use '2.0' or '2.5'" });

            var result = _orchestration.ExecuteComplete(request);
            if (!result.IsSuccessful)
                return BadRequest(new { error = string.Join("; ", result.Errors ?? new()), warnings = result.Warnings });

            var response = new CalculateSAILResponse
            {
                SoraVersion = result.SoraVersion,
                IGRC = result.IntrinsicGRC,
                FinalGRC = result.FinalGRC,
                InitialARC = result.InitialARC?.ToString().Replace("ARC_", string.Empty),
                ResidualARC = result.ResidualARC?.ToString().Replace("ARC_", string.Empty),
                SAIL = result.SAILLabel,
                Steps = new List<string>()
                {
                    result.GroundRiskNotes,
                    result.AirRiskNotes,
                    result.SAILNotes
                }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
                Warnings = result.Warnings ?? new()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating SAIL");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

/// <summary>
/// OSO count result
/// </summary>
public class OSOCountResult
{
    public SAILLevel SAIL { get; set; }
    public int RequiredOSOCount { get; set; }
    public Dictionary<string, string> OSORequirements { get; set; } = new();
    public string Reference { get; set; } = string.Empty;
}

/// <summary>
/// Concise response for SAIL calculation suitable for UI live panel
/// </summary>
public class CalculateSAILResponse
{
    public string SoraVersion { get; set; } = "2.5";
    [JsonPropertyName("iGRC")] public int? IGRC { get; set; }
    public int? FinalGRC { get; set; }
    public string? InitialARC { get; set; } // a..d
    public string? ResidualARC { get; set; } // a..d
    public string? SAIL { get; set; } // I..VI
    public List<string> Steps { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
