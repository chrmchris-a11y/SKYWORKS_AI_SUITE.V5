using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;
using Skyworks.Backend.Services;

namespace Skyworks.Backend.Controllers
{
    /// <summary>
    /// API Controller για OSO (Operational Safety Objectives) validation
    /// Endpoints για JARUS SORA 2.5 OSO #11, #17, #19, #23 compliance checking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OSOController : ControllerBase
    {
        private readonly IOSOService _osoService;
        private readonly ILogger<OSOController> _logger;

        public OSOController(IOSOService osoService, ILogger<OSOController> logger)
        {
            _osoService = osoService ?? throw new ArgumentNullException(nameof(osoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Επιστροφή όλων των διαθέσιμων OSO requirements
        /// </summary>
        /// <returns>Dictionary με OSO requirements</returns>
        [HttpGet("requirements")]
        public ActionResult<Dictionary<string, OSORequirement>> GetOSORequirements()
        {
            try
            {
                _logger.LogInformation("Fetching all OSO requirements");
                var requirements = _osoService.GetAllOSORequirements();
                return Ok(requirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching OSO requirements");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Επικύρωση OSO #11 (Detect & Avoid) 
        /// Strategic για ARC-a/b, Tactical για ARC-c/d
        /// </summary>
        /// <param name="request">Operation data με ARC level</param>
        /// <returns>OSO #11 validation result</returns>
        [HttpPost("validate-detect-avoid")]
        public ActionResult<OSOResult> ValidateDetectAndAvoid([FromBody] DetectAvoidRequest request)
        {
            try
            {
                _logger.LogInformation("Validating OSO #11 for ARC: {ARC}", request.ARC);

                if (request.Operation == null)
                {
                    return BadRequest(new { error = "Operation data is required" });
                }

                if (string.IsNullOrEmpty(request.ARC) || 
                    !new[] { "ARC-a", "ARC-b", "ARC-c", "ARC-d" }.Contains(request.ARC))
                {
                    return BadRequest(new { error = "Valid ARC level required (ARC-a, ARC-b, ARC-c, ARC-d)" });
                }

                var result = _osoService.ValidateDetectAndAvoid(request.Operation, request.ARC);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #11");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Επικύρωση OSO #17 (Operational Volume Definition)
        /// 3D volume validation με safety buffers
        /// </summary>
        /// <param name="operation">Operation data με operational volume</param>
        /// <returns>OSO #17 validation result</returns>
        [HttpPost("validate-operational-volume")]
        public ActionResult<OSOResult> ValidateOperationalVolume([FromBody] Operation operation)
        {
            try
            {
                _logger.LogInformation("Validating OSO #17 for operation: {OperationId}", operation?.OperationId);

                if (operation == null)
                {
                    return BadRequest(new { error = "Operation data is required" });
                }

                if (operation.OperationalVolume == null)
                {
                    return BadRequest(new { error = "Operational volume data is required" });
                }

                var result = _osoService.ValidateOperationalVolume(operation);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #17");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Επικύρωση OSO #19 (Human Performance)
        /// Crew training και certification validation
        /// </summary>
        /// <param name="operation">Operation data με crew information</param>
        /// <returns>OSO #19 validation result</returns>
        [HttpPost("validate-human-performance")]
        public ActionResult<OSOResult> ValidateHumanPerformance([FromBody] Operation operation)
        {
            try
            {
                _logger.LogInformation("Validating OSO #19 for operation: {OperationId}", operation?.OperationId);

                if (operation == null)
                {
                    return BadRequest(new { error = "Operation data is required" });
                }

                if (operation.Crew == null || operation.Crew.RemotePilot == null)
                {
                    return BadRequest(new { error = "Crew data with remote pilot information is required" });
                }

                var result = _osoService.ValidateHumanPerformance(operation);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #19");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Επικύρωση OSO #23 (Adjacent Area Analysis)
        /// Population density analysis εντός 1 NM από flight path
        /// </summary>
        /// <param name="request">Operation data με GRC level</param>
        /// <returns>OSO #23 validation result</returns>
        [HttpPost("validate-adjacent-area")]
        public ActionResult<OSOResult> ValidateAdjacentArea([FromBody] AdjacentAreaRequest request)
        {
            try
            {
                _logger.LogInformation("Validating OSO #23 for GRC: {GRC}", request.GRC);

                if (request.Operation == null)
                {
                    return BadRequest(new { error = "Operation data is required" });
                }

                if (request.GRC < 1 || request.GRC > 7)
                {
                    return BadRequest(new { error = "Valid GRC level required (1-7)" });
                }

                if (request.Operation.FlightPath?.Waypoints?.Any() != true)
                {
                    return BadRequest(new { error = "Flight path with waypoints is required" });
                }

                var result = _osoService.ValidateAdjacentArea(request.Operation, request.GRC);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #23");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Comprehensive OSO validation για όλα τα implemented OSOs
        /// </summary>
        /// <param name="request">Complete operation data με SAIL level</param>
        /// <returns>List με όλα τα OSO validation results</returns>
        [HttpPost("validate-all")]
        public ActionResult<ComprehensiveOSOResult> ValidateAllOSO([FromBody] ComprehensiveValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Performing comprehensive OSO validation for SAIL: {SAIL}", request.CurrentSAIL);

                if (request.Operation == null)
                {
                    return BadRequest(new { error = "Operation data is required" });
                }

                if (request.CurrentSAIL < 0 || request.CurrentSAIL > 6)
                {
                    return BadRequest(new { error = "Valid SAIL level required (0-6)" });
                }

                var results = _osoService.ValidateAllOSO(request.Operation, request.CurrentSAIL);
                
                var comprehensiveResult = new ComprehensiveOSOResult
                {
                    OperationId = request.Operation.OperationId,
                    CurrentSAIL = request.CurrentSAIL,
                    ValidationTimestamp = DateTime.UtcNow,
                    OSO_Results = results,
                    OverallCompliance = results.All(r => r.IsCompliant),
                    CompliancePercentage = (double)results.Count(r => r.IsCompliant) / results.Count * 100,
                    M3PenaltiesApplied = results.Count(r => r.M3PenaltyApplied),
                    TotalRequiredEvidence = results.SelectMany(r => r.RequiredEvidence).Distinct().ToList()
                };

                // Calculate final SAIL after M3 penalties
                var finalSAIL = request.CurrentSAIL - comprehensiveResult.M3PenaltiesApplied;
                comprehensiveResult.FinalSAIL = Math.Max(0, finalSAIL);

                return Ok(comprehensiveResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during comprehensive OSO validation");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint για OSO service
        /// </summary>
        /// <returns>Service status</returns>
        [HttpGet("health")]
        public ActionResult<object> HealthCheck()
        {
            try
            {
                var requirements = _osoService.GetAllOSORequirements();
                return Ok(new 
                { 
                    status = "healthy", 
                    timestamp = DateTime.UtcNow,
                    availableOSOs = requirements.Keys.ToArray(),
                    osoCount = requirements.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OSO service health check failed");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }
    }

    #region Request/Response DTOs

    /// <summary>
    /// Request για OSO #11 validation
    /// </summary>
    public class DetectAvoidRequest
    {
        public Operation Operation { get; set; } = new Operation();
        public string ARC { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request για OSO #23 validation
    /// </summary>
    public class AdjacentAreaRequest  
    {
        public Operation Operation { get; set; } = new Operation();
        public int GRC { get; set; }
    }

    /// <summary>
    /// Request για comprehensive validation
    /// </summary>
    public class ComprehensiveValidationRequest
    {
        public Operation Operation { get; set; } = new Operation();
        public int CurrentSAIL { get; set; }
    }

    /// <summary>
    /// Response για comprehensive OSO validation
    /// </summary>
    public class ComprehensiveOSOResult
    {
        public string OperationId { get; set; } = string.Empty;
        public int CurrentSAIL { get; set; }
        public int FinalSAIL { get; set; }
        public DateTime ValidationTimestamp { get; set; }
        public List<OSOResult> OSO_Results { get; set; } = new List<OSOResult>();
        public bool OverallCompliance { get; set; }
        public double CompliancePercentage { get; set; }
        public int M3PenaltiesApplied { get; set; }
        public List<string> TotalRequiredEvidence { get; set; } = new List<string>();
    }

    #endregion
}