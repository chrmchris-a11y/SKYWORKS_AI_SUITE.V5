using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skyworks.Backend.Services
{
    /// <summary>
    /// Υπηρεσία για OSO Compliance Validation - Step 43
    /// Ενσωματώνει Floor Rule validation, M3 penalties, και comprehensive compliance reporting
    /// Βάσει JARUS SORA 2.5 Annex B & Floor Rule specifications
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IOSOService _osoService;
        private readonly ISAILCalculator _sailCalculator;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IOSOService osoService,
            ISAILCalculator sailCalculator,
            ILogger<ValidationService> logger)
        {
            _osoService = osoService ?? throw new ArgumentNullException(nameof(osoService));
            _sailCalculator = sailCalculator ?? throw new ArgumentNullException(nameof(sailCalculator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region OSO Compliance Validation

                /// <summary>
        /// Ολοκληρωμένη επικύρωση συμμόρφωσης με OSO requirements
        /// Comprehensive OSO compliance validation with dual SORA version support
        /// </summary>
        /// <param name="operation">Operation data</param>
        /// <param name="arc">Current ARC level</param>
        /// <param name="sail">Target SAIL level</param>
        /// <param name="soraVersion">SORA version ("2.0" or "2.5")</param>
        /// <returns>Comprehensive validation result με recommendations</returns>
        public async Task<OSOComplianceResult> ValidateOSOCompliance(
            Operation operation, 
            string arc, 
            int sail,
            string soraVersion)
                {
            // Validate SORA version first
            SoraVersionValidator.ValidateVersion(soraVersion);
            
            _logger.LogInformation("Starting OSO compliance validation for Operation: {OperationId}, ARC: {ARC}, SAIL: {SAIL}, SORA: {SoraVersion}", 
                operation.OperationId, arc, sail, soraVersion);

                        var result = new OSOComplianceResult
            {
                OperationId = operation.OperationId,
                SoraVersion = soraVersion,
                OriginalARC = arc,
                TargetSAIL = sail,
                ValidationTimestamp = DateTime.UtcNow,
                OSOResults = new List<OSOResult>(),
                M3PenaltiesApplied = new List<string>(),
                IsCompliant = false,
                RiskMitigationMatrix = SoraVersionConstants.GetRiskMitigationMatrix(soraVersion)
            };

                        try
            {
                // Get applicable OSOs for the specified SORA version
                var applicableOSOs = SoraVersionConstants.GetOSOsForVersion(soraVersion);
                _logger.LogInformation("Validating {Count} OSOs for SORA {SoraVersion}", applicableOSOs.Length, soraVersion);
                
                // Validate key OSOs based on version
                var keyOSOs = GetKeyOSOsForValidation(soraVersion);
                foreach (var osoId in keyOSOs)
                {
                    var osoResult = await ValidateOSOByVersion(osoId, operation, arc, soraVersion);
                    result.OSOResults.Add(osoResult);
                    
                    if (osoResult.M3PenaltyApplied)
                    {
                        var penaltyDescription = GetM3PenaltyDescription(osoId, soraVersion);
                        result.M3PenaltiesApplied.Add(penaltyDescription);
                        _logger.LogWarning("M3 Penalty applied for {OSOId} in SORA {SoraVersion}: {Description}", 
                            osoId, soraVersion, penaltyDescription);
                    }
                }

                // 5. Apply Floor Rule (M3 penalties reduce ARC)
                var adjustedARC = ApplyFloorRule(arc, result.M3PenaltiesApplied.Count);
                result.AdjustedARC = adjustedARC;

                _logger.LogInformation("Floor Rule applied: {OriginalARC} → {AdjustedARC} ({M3Count} M3 penalties)", 
                    arc, adjustedARC, result.M3PenaltiesApplied.Count);

                // 6. Recalculate SAIL with adjusted ARC
                var recalculatedSAIL = await RecalculateSAIL(operation, adjustedARC);
                result.RecalculatedSAIL = recalculatedSAIL;

                                // 7. Determine overall compliance
                result.IsCompliant = DetermineCompliance(result);

                // 8. Generate version-specific recommendations
                result.Recommendations = GenerateRecommendations(result, soraVersion);

                _logger.LogInformation("OSO compliance validation completed for SORA {SoraVersion}. Compliant: {IsCompliant}, M3 Penalties: {M3Count}", 
                    soraVersion, result.IsCompliant, result.M3PenaltiesApplied.Count);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OSO compliance validation");
                result.ValidationMessages.Add($"Validation error: {ex.Message}");
                result.IsCompliant = false;
            }

            return result;
        }

        #endregion

        #region Floor Rule Implementation

        /// <summary>
        /// Εφαρμογή Floor Rule: Κάθε M3 penalty μειώνει το ARC κατά 1 επίπεδο
        /// JARUS SORA 2.5 Floor Rule specification
        /// </summary>
        /// <param name="originalARC">Original ARC (ARC-a, ARC-b, ARC-c, ARC-d)</param>
        /// <param name="m3PenaltyCount">Number of M3 penalties applied</param>
        /// <returns>Adjusted ARC level</returns>
        public string ApplyFloorRule(string originalARC, int m3PenaltyCount)
        {
            if (m3PenaltyCount == 0)
            {
                return originalARC;
            }

            var arcLevels = new[] { "ARC-a", "ARC-b", "ARC-c", "ARC-d" };
            var currentIndex = Array.IndexOf(arcLevels, originalARC);

            if (currentIndex == -1)
            {
                _logger.LogWarning("Invalid ARC level: {ARC}", originalARC);
                return originalARC;
            }

            // Μειώνουμε το ARC κατά 1 για κάθε M3 penalty (Floor Rule)
            var adjustedIndex = Math.Max(0, currentIndex - m3PenaltyCount);
            var adjustedARC = arcLevels[adjustedIndex];

            _logger.LogInformation("Floor Rule: {OriginalARC} with {M3Count} M3 penalties → {AdjustedARC}", 
                originalARC, m3PenaltyCount, adjustedARC);

            return adjustedARC;
        }

        #endregion

        #region SAIL Recalculation

        /// <summary>
        /// Επανυπολογισμός SAIL με το adjusted ARC (μετά τα M3 penalties)
        /// </summary>
        private async Task<int> RecalculateSAIL(Operation operation, string adjustedARC)
        {
            try
            {
                // Χρησιμοποιούμε το SAILCalculator για να υπολογίσουμε το νέο SAIL
                var sailResult = await _sailCalculator.CalculateSAIL(operation, adjustedARC);
                return sailResult.SAIL;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating SAIL with adjusted ARC: {AdjustedARC}", adjustedARC);
                return -1; // Invalid SAIL
            }
        }

        #endregion

        #region Compliance Determination

        /// <summary>
        /// Καθορισμός συνολικής συμμόρφωσης βάσει OSO results
        /// </summary>
        private bool DetermineCompliance(OSOComplianceResult result)
        {
            // Όλα τα OSO πρέπει να είναι compliant ή να έχει εφαρμοστεί TMPR
            var allCompliant = result.OSOResults.All(oso => 
                oso.IsCompliant || oso.TMPRApplied);

            // Το recalculated SAIL πρέπει να είναι <= target SAIL
            var sailAcceptable = result.RecalculatedSAIL <= result.TargetSAIL;

            return allCompliant && sailAcceptable;
        }

        #endregion

        #region Recommendations Generation

        /// <summary>
        /// Δημιουργία συστάσεων για επίτευξη συμμόρφωσης
        /// </summary>
        private List<string> GenerateRecommendations(OSOComplianceResult result)
        {
            var recommendations = new List<string>();

            // Έλεγχος για μη-compliant OSOs
            foreach (var oso in result.OSOResults.Where(o => !o.IsCompliant))
            {
                switch (oso.OSOId)
                {
                    case "OSO-11":
                        recommendations.Add("Install tactical Detect & Avoid system (e.g., ADS-B, radar)");
                        recommendations.Add("Alternative: Add 2+ visual observers with radio communication");
                        break;

                    case "OSO-17":
                        recommendations.Add("Reduce operational volume safety margins");
                        recommendations.Add("Improve GPS accuracy with RTK or DGPS");
                        recommendations.Add("Coordinate with ATC for controlled airspace");
                        break;

                    case "OSO-19":
                        recommendations.Add("Complete EASA A1/A2/A3 pilot certification");
                        recommendations.Add("Accumulate minimum 50 flight hours");
                        recommendations.Add("Provide ground crew emergency training (valid 12 months)");
                        break;

                    case "OSO-23":
                        recommendations.Add("Increase flight altitude above 150m AGL");
                        recommendations.Add("Select flight path avoiding high population areas");
                        recommendations.Add("Implement controlled ground area (CGR) procedures");
                        break;
                }
            }

            // Έλεγχος για SAIL mismatch
            if (result.RecalculatedSAIL > result.TargetSAIL)
            {
                recommendations.Add($"Target SAIL {result.TargetSAIL} not achievable. Current: SAIL {result.RecalculatedSAIL}");
                recommendations.Add("Reduce GRC (ground risk) or ARC (air risk) through operational changes");
                recommendations.Add("Consider applying additional OSO mitigations");
            }

            // Προτάσεις για M3 penalties
            if (result.M3PenaltiesApplied.Any())
            {
                recommendations.Add($"Address {result.M3PenaltiesApplied.Count} M3 penalties to improve ARC level:");
                foreach (var penalty in result.M3PenaltiesApplied)
                {
                    recommendations.Add($"  - {penalty}");
                }
            }

            return recommendations;
        }

        #endregion

        #region Public Validation Methods

        /// <summary>
        /// Επικύρωση Floor Rule για συγκεκριμένο operation
        /// MCP Tool integration: validate_floor_rule
        /// </summary>
        public FloorRuleResult ValidateFloorRule(string operationId, string arc, List<string> m3Penalties)
        {
            _logger.LogInformation("Validating Floor Rule for operation: {OperationId}, ARC: {ARC}, M3 Count: {M3Count}", 
                operationId, arc, m3Penalties.Count);

            var adjustedARC = ApplyFloorRule(arc, m3Penalties.Count);

            return new FloorRuleResult
            {
                OperationId = operationId,
                OriginalARC = arc,
                AdjustedARC = adjustedARC,
                M3PenaltiesApplied = m3Penalties,
                ARCReduction = CalculateARCReduction(arc, adjustedARC),
                IsValid = true,
                ValidationTimestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Υπολογισμός μείωσης ARC σε αριθμητικό επίπεδο
        /// </summary>
        private int CalculateARCReduction(string originalARC, string adjustedARC)
        {
            var arcLevels = new[] { "ARC-a", "ARC-b", "ARC-c", "ARC-d" };
            var originalIndex = Array.IndexOf(arcLevels, originalARC);
            var adjustedIndex = Array.IndexOf(arcLevels, adjustedARC);

            return Math.Max(0, originalIndex - adjustedIndex);
        }

        #endregion
    }

    #region Supporting Interfaces

        public interface IValidationService
    {
        Task<OSOComplianceResult> ValidateOSOCompliance(Operation operation, string arc, int sail, string soraVersion);
        string ApplyFloorRule(string originalARC, int m3PenaltyCount);
        FloorRuleResult ValidateFloorRule(string operationId, string arc, List<string> m3Penalties);
    }

    #endregion

    #region Result Models

    /// <summary>
    /// Comprehensive OSO compliance validation result
    /// </summary>
    public class OSOComplianceResult
    {
        public string OperationId { get; set; } = string.Empty;
        public string OriginalARC { get; set; } = string.Empty;
        public string AdjustedARC { get; set; } = string.Empty;
        public int TargetSAIL { get; set; }
        public int RecalculatedSAIL { get; set; }
        public bool IsCompliant { get; set; }
        public List<OSOResult> OSOResults { get; set; } = new List<OSOResult>();
        public List<string> M3PenaltiesApplied { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> ValidationMessages { get; set; } = new List<string>();
        public DateTime ValidationTimestamp { get; set; }
    }

    /// <summary>
    /// Floor Rule validation result
    /// </summary>
    public class FloorRuleResult
    {
        public string OperationId { get; set; } = string.Empty;
        public string OriginalARC { get; set; } = string.Empty;
        public string AdjustedARC { get; set; } = string.Empty;
        public List<string> M3PenaltiesApplied { get; set; } = new List<string>();
        public int ARCReduction { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidationTimestamp { get; set; }
    }

    #endregion
}
