using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;
using Skyworks.Backend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skyworks.Backend.Controllers
{
    /// <summary>
    /// Ελεγκτής API για επικύρωση επιχειρησιακών απαιτήσεων UAS
    /// Comprehensive validation of UAS operational safety requirements
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ValidationController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly ILogger<ValidationController> _logger;

        /// <summary>
        /// Κατασκευαστής ελεγκτή επικύρωσης
        /// Validation controller constructor with dependency injection
        /// </summary>
        /// <param name="validationService">Υπηρεσία επικύρωσης επιχειρησιακών απαιτήσεων</param>
        /// <param name="logger">Καταγραφέας συμβάντων</param>
        public ValidationController(
            IValidationService validationService, 
            ILogger<ValidationController> logger)
        {
            _validationService = validationService 
                ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger 
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Λήψη κατάστασης συμμόρφωσης OSO για συγκεκριμένη επιχείρηση
        /// Retrieve OSO compliance status for a specific operation
        /// </summary>
        /// <param name="operationId">Μοναδικό αναγνωριστικό επιχείρησης</param>
        /// <returns>Αποτέλεσμα συμμόρφωσης OSO</returns>
        [HttpGet("oso-compliance")]
        [ProducesResponseType(typeof(OSOComplianceResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetOSOCompliance([FromQuery] string operationId)
        {
            try
            {
                _logger.LogInformation("Retrieving OSO compliance for Operation: {OperationId}", operationId);

                if (string.IsNullOrWhiteSpace(operationId))
                {
                    return BadRequest(new { error = "Operation ID is required" });
                }

                var result = await _validationService.GetOSOComplianceAsync(operationId);

                return result != null 
                    ? Ok(result) 
                    : NotFound(new { error = "Operation not found or no compliance data available" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OSO compliance for Operation: {OperationId}", operationId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Πλήρης επικύρωση επιχείρησης με συστάσεις
        /// Comprehensive operation validation with recommendations
        /// </summary>
        /// <param name="validationRequest">Αίτημα επικύρωσης επιχείρησης</param>
        /// <returns>Πλήρες αποτέλεσμα επικύρωσης</returns>
        [HttpPost("validate-operation")]
        [ProducesResponseType(typeof(OperationValidationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateOperation([FromBody] OperationValidationRequest validationRequest)
        {
            try
            {
                _logger.LogInformation("Validating operation: {OperationId}", validationRequest?.Operation?.OperationId);

                if (validationRequest?.Operation == null)
                {
                    return BadRequest(new { error = "Operation data is required" });
                }

                var result = await _validationService.ValidateOperationAsync(
                    validationRequest.Operation, 
                    validationRequest.ARC, 
                    validationRequest.SAIL
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating operation");
                return StatusCode(500, new { error = "Operation validation failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Εφαρμογή κανόνα Floor Rule και υπολογισμός προσαρμοσμένου ARC
        /// Apply Floor Rule and calculate adjusted ARC
        /// </summary>
        /// <param name="floorRuleRequest">Αίτημα εφαρμογής Floor Rule</param>
        /// <returns>Αποτέλεσμα εφαρμογής Floor Rule</returns>
        [HttpPost("floor-rule")]
        [ProducesResponseType(typeof(FloorRuleResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ApplyFloorRule([FromBody] FloorRuleRequest floorRuleRequest)
        {
            try
            {
                _logger.LogInformation("Applying Floor Rule for Operation: {OperationId}", floorRuleRequest?.OperationId);

                if (floorRuleRequest?.OperationId == null)
                {
                    return BadRequest(new { error = "Operation ID is required" });
                }

                var result = await _validationService.ApplyFloorRuleAsync(
                    floorRuleRequest.OperationId, 
                    floorRuleRequest.ARC, 
                    floorRuleRequest.M3Penalties
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying Floor Rule");
                return StatusCode(500, new { error = "Floor Rule application failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Λήψη συστάσεων συμμόρφωσης για συγκεκριμένη επιχείρηση
        /// Retrieve compliance recommendations for a specific operation
        /// </summary>
        /// <param name="operationId">Μοναδικό αναγνωριστικό επιχείρησης</param>
        /// <returns>Λίστα συστάσεων</returns>
        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetComplianceRecommendations([FromQuery] string operationId)
        {
            try
            {
                _logger.LogInformation("Retrieving compliance recommendations for Operation: {OperationId}", operationId);

                if (string.IsNullOrWhiteSpace(operationId))
                {
                    return BadRequest(new { error = "Operation ID is required" });
                }

                var recommendations = await _validationService.GetComplianceRecommendationsAsync(operationId);

                return recommendations?.Count > 0 
                    ? Ok(recommendations) 
                    : NotFound(new { error = "No recommendations available" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance recommendations");
                return StatusCode(500, new { error = "Recommendations retrieval failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Λήψη τρέχουσας κατάστασης επικύρωσης επιχείρησης
        /// Retrieve current validation status for an operation
        /// </summary>
        /// <param name="operationId">Μοναδικό αναγνωριστικό επιχείρησης</param>
        /// <returns>Κατάσταση επικύρωσης</returns>
        [HttpGet("status/{operationId}")]
        [ProducesResponseType(typeof(ValidationStatusResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetValidationStatus(string operationId)
        {
            try
            {
                _logger.LogInformation("Retrieving validation status for Operation: {OperationId}", operationId);

                if (string.IsNullOrWhiteSpace(operationId))
                {
                    return BadRequest(new { error = "Operation ID is required" });
                }

                var status = await _validationService.GetValidationStatusAsync(operationId);

                return status != null 
                    ? Ok(status) 
                    : NotFound(new { error = "No validation status found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving validation status");
                return StatusCode(500, new { error = "Validation status retrieval failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Υγεία της υπηρεσίας επικύρωσης
        /// Validation service health check
        /// </summary>
        /// <returns>Κατάσταση υπηρεσίας</returns>
        [HttpGet("health")]
        [ProducesResponseType(200)]
        public IActionResult HealthCheck()
        {
            try
            {
                _logger.LogInformation("Performing validation service health check");
                return Ok(new 
                { 
                    status = "healthy", 
                    timestamp = DateTime.UtcNow,
                    serviceVersion = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation service health check failed");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }
    }

    #region DTOs (Data Transfer Objects)

    /// <summary>
    /// Αίτημα επικύρωσης επιχείρησης
    /// Operation validation request DTO
    /// </summary>
    public class OperationValidationRequest
    {
        /// <summary>
        /// Στοιχεία επιχείρησης
        /// Operation details
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// Επίπεδο Air Risk Class (ARC)
        /// Air Risk Class level
        /// </summary>
        public string ARC { get; set; }

        /// <summary>
        /// Επίπεδο Safety Assurance Integrity Level (SAIL)
        /// Safety Assurance Integrity Level
        /// </summary>
        public int SAIL { get; set; }
    }

    /// <summary>
    /// Αίτημα εφαρμογής Floor Rule
    /// Floor Rule application request DTO
    /// </summary>
    public class FloorRuleRequest
    {
        /// <summary>
        /// Μοναδικό αναγνωριστικό επιχείρησης
        /// Operation unique identifier
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Επίπεδο Air Risk Class (ARC)
        /// Air Risk Class level
        /// </summary>
        public string ARC { get; set; }

        /// <summary>
        /// Εφαρμοζόμενες ποινές M3
        /// Applied M3 penalties
        /// </summary>
        public List<string> M3Penalties { get; set; } = new List<string>();
    }

    #endregion
}