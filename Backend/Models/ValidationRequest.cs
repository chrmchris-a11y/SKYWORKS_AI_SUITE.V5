using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Base validation request with mandatory SORA version support
    /// All validation requests must inherit from this to ensure version consistency
    /// </summary>
    public abstract class BaseValidationRequest
    {
        /// <summary>
        /// SORA version ("2.0" or "2.5") - REQUIRED
        /// Έκδοση SORA ("2.0" ή "2.5") - ΥΠΟΧΡΕΩΤΙΚΟ
        /// </summary>
        [Required(ErrorMessage = "SoraVersion is required")]
        public string SoraVersion { get; set; } = string.Empty;

        /// <summary>
        /// Operation identifier for tracking
        /// </summary>
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Validate the request parameters
        /// </summary>
        public virtual void Validate()
        {
            SoraVersionValidator.ValidateVersion(SoraVersion);
        }
    }

    /// <summary>
    /// OSO validation request with dual SORA version support
    /// </summary>
    public class OSOValidationRequest : BaseValidationRequest
    {
        /// <summary>
        /// OSO identifier (e.g., "OSO-11", "OSO-17")
        /// </summary>
        [Required(ErrorMessage = "OSOId is required")]
        public string OsoId { get; set; } = string.Empty;

        /// <summary>
        /// Operation data for validation
        /// </summary>
        [Required(ErrorMessage = "Operation is required")]
        public Operation Operation { get; set; } = new();

        /// <summary>
        /// Current ARC level for Floor Rule application
        /// </summary>
        public string CurrentARC { get; set; } = string.Empty;

        /// <summary>
        /// Target SAIL level
        /// </summary>
        public int TargetSAIL { get; set; }

        public override void Validate()
        {
            base.Validate();
            
            // Check if OSO exists for the specified version
            var availableOSOs = SoraVersionConstants.GetOSOsForVersion(SoraVersion);
            if (!availableOSOs.Contains(OsoId))
            {
                throw new ArgumentException(
                    $"OSO {OsoId} is not available in SORA {SoraVersion}. " +
                    $"Available OSOs: {string.Join(", ", availableOSOs)}"
                );
            }
        }
    }

    /// <summary>
    /// TMPR validation request with version-specific thresholds
    /// </summary>
    public class TMPRValidationRequest : BaseValidationRequest
    {
        /// <summary>
        /// OSO identifier requiring TMPR
        /// </summary>
        [Required(ErrorMessage = "OSOId is required")]
        public string OsoId { get; set; } = string.Empty;

        /// <summary>
        /// Operation data with TMPR systems
        /// </summary>
        [Required(ErrorMessage = "Operation is required")]
        public Operation Operation { get; set; } = new();

        public override void Validate()
        {
            base.Validate();
            
            // Check if OSO requires TMPR for the specified version
            var tmprRequiredOSOs = SoraVersionConstants.GetTMPRRequiredOSOs(SoraVersion);
            if (!tmprRequiredOSOs.Contains(OsoId))
            {
                throw new ArgumentException(
                    $"OSO {OsoId} does not require TMPR validation in SORA {SoraVersion}. " +
                    $"TMPR required OSOs: {string.Join(", ", tmprRequiredOSOs)}"
                );
            }
        }
    }

    /// <summary>
    /// Comprehensive validation request for full compliance check
    /// VERIFIED: JARUS SORA v2.5 - NO "TargetSAIL" concept
    /// SAIL is computed from Final GRC × Residual ARC, not targeted
    /// </summary>
    public class ComprehensiveValidationRequest : BaseValidationRequest
    {
        /// <summary>
        /// Operation data for validation
        /// </summary>
        [Required(ErrorMessage = "Operation is required")]
        public Operation Operation { get; set; } = new();

        /// <summary>
        /// Current Residual ARC level (after strategic mitigations applied)
        /// VERIFIED: JARUS Step #5 output
        /// </summary>
        [Required(ErrorMessage = "CurrentARC is required")]
        public string CurrentARC { get; set; } = string.Empty;

        // REMOVED: TargetSAIL - no such concept in JARUS SORA
        // SAIL is computed, not targeted

        /// <summary>
        /// Final Ground Risk Class (after M1/M2 mitigations applied)
        /// VERIFIED: JARUS Step #3 output - used for SAIL calculation
        /// </summary>
        [Range(0, 12, ErrorMessage = "FinalGRC must be between 0 and 12")]
        public int FinalGRC { get; set; }

        /// <summary>
        /// List of OSOs to validate (if empty, validates all applicable OSOs)
        /// </summary>
        public List<string> OSOsToValidate { get; set; } = new();

        public override void Validate()
        {
            base.Validate();
            
            // Validate ARC format
            var validARCs = new[] { "ARC-a", "ARC-b", "ARC-c", "ARC-d" };
            if (!validARCs.Contains(CurrentARC))
            {
                throw new ArgumentException(
                    $"Invalid ARC: {CurrentARC}. Valid values: {string.Join(", ", validARCs)}"
                );
            }

            // If specific OSOs requested, validate they exist for the version
            if (OSOsToValidate.Any())
            {
                var availableOSOs = SoraVersionConstants.GetOSOsForVersion(SoraVersion);
                var invalidOSOs = OSOsToValidate.Where(oso => !availableOSOs.Contains(oso)).ToList();
                
                if (invalidOSOs.Any())
                {
                    throw new ArgumentException(
                        $"Invalid OSOs for SORA {SoraVersion}: {string.Join(", ", invalidOSOs)}. " +
                        $"Available OSOs: {string.Join(", ", availableOSOs)}"
                    );
                }
            }
        }
    }

    /// <summary>
    /// Risk mitigation validation request
    /// </summary>
    public class RiskMitigationRequest : BaseValidationRequest
    {
        /// <summary>
        /// Current risk level
        /// </summary>
        [Required(ErrorMessage = "CurrentRiskLevel is required")]
        public string CurrentRiskLevel { get; set; } = string.Empty;

        /// <summary>
        /// Target risk level
        /// </summary>
        [Required(ErrorMessage = "TargetRiskLevel is required")]
        public string TargetRiskLevel { get; set; } = string.Empty;

        /// <summary>
        /// Operation context for mitigation strategy
        /// </summary>
        [Required(ErrorMessage = "Operation is required")]
        public Operation Operation { get; set; } = new();

        /// <summary>
        /// Available mitigation strategies
        /// </summary>
        public List<string> AvailableMitigations { get; set; } = new();
    }
}