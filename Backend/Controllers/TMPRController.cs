using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;
using Skyworks.Backend.Services;
using System;
using System.Threading.Tasks;

namespace Skyworks.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TMPRController : ControllerBase
    {
        private readonly ITMPRService _tmprService;
        private readonly ILogger<TMPRController> _logger;

        public TMPRController(ITMPRService tmprService, ILogger<TMPRController> logger)
        {
            _tmprService = tmprService;
            _logger = logger;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateTMPR([FromBody] TMPRValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Validating TMPR for OSO {OsoId}", request.OsoId);

                var result = await _tmprService.ValidateTMPR(
                    request.Operation, 
                    request.OsoId, 
                    request.SoraVersion
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TMPR validation error");
                return StatusCode(500, new { error = "TMPR validation failed", details = ex.Message });
            }
        }

        [HttpGet("systems/{soraVersion}")]
        public IActionResult GetTMPRSystems(string soraVersion)
        {
            try
            {
                _logger.LogInformation("Retrieving TMPR systems for SORA {Version}", soraVersion);

                var systems = _tmprService.GetAvailableTMPRSystems(soraVersion);
                return Ok(systems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TMPR systems");
                return StatusCode(500, new { error = "TMPR systems retrieval failed", details = ex.Message });
            }
        }

        [HttpGet("requirements/{osoId}/{soraVersion}")]
        public IActionResult GetTMPRRequirements(string osoId, string soraVersion)
        {
            try
            {
                _logger.LogInformation("Retrieving TMPR requirements for OSO {OsoId}", osoId);

                // Placeholder logic - expand as needed
                var requirements = new 
                {
                    OsoId = osoId,
                    SoraVersion = soraVersion,
                    ReliabilityThreshold = soraVersion == "2.0" ? 0.9 : 0.95,
                    RequiredSystems = soraVersion == "2.0" 
                        ? new[] { "Parachute", "Flight Termination" }
                        : new[] { "Advanced Parachute", "Geo-Fencing" }
                };

                return Ok(requirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TMPR requirements");
                return StatusCode(500, new { error = "TMPR requirements retrieval failed", details = ex.Message });
            }
        }
    }

    // DTO for TMPR validation request
    public class TMPRValidationRequest
    {
        public Operation Operation { get; set; }
        public string OsoId { get; set; }
        public string SoraVersion { get; set; }
    }
}