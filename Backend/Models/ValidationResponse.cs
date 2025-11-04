using System;
using System.Collections.Generic;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Base validation response with mandatory SORA version labeling
    /// All validation responses must inherit from this to ensure version tracking
    /// </summary>
    public abstract class BaseValidationResponse
    {
        /// <summary>
        /// SORA version used for validation ("2.0" or "2.5")
        /// Έκδοση SORA που χρησιμοποιήθηκε για επικύρωση
        /// </summary>
        public string SoraVersion { get; set; } = string.Empty;

        /// <summary>
        /// Overall validation result
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Validation timestamp
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Operation identifier for tracking
        /// </summary>
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Risk mitigation matrix used for validation
        /// </summary>
        public string RiskMitigationMatrix { get; set; } = string.Empty;

        /// <summary>
        /// Validation messages and warnings
        /// </summary>
        public List<string> ValidationMessages { get; set; } = new();

        /// <summary>
        /// Overall pass/fail summary
        /// </summary>
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// OSO validation response with per-OSO results
    /// </summary>
    public class OSOValidationResponse : BaseValidationResponse
    {
        /// <summary>
        /// OSO identifier that was validated
        /// </summary>
        public string OsoId { get; set; } = string.Empty;

        /// <summary>
        /// OSO-specific validation result
        /// </summary>
        public bool OSOCompliant { get; set; }

        /// <summary>
        /// Evidence collected for this OSO
        /// </summary>
        public List<string> EvidenceCollected { get; set; } = new();

        /// <summary>
        /// Mitigation measures applied
        /// </summary>
        public List<string> MitigationMeasures { get; set; } = new();

        /// <summary>
        /// Whether M3 penalty was applied
        /// </summary>
        public bool M3PenaltyApplied { get; set; }

        /// <summary>
        /// TMPR validation result (if applicable)
        /// </summary>
        public TMPRValidationResult? TMPRResult { get; set; }

        /// <summary>
        /// ARC before Floor Rule application
        /// </summary>
        public string ARCBefore { get; set; } = string.Empty;

        /// <summary>
        /// ARC after Floor Rule and M3 penalties
        /// </summary>
        public string ARCAfter { get; set; } = string.Empty;

        /// <summary>
        /// List of penalties applied with descriptions
        /// </summary>
        public List<string> PenaltiesApplied { get; set; } = new();
    }

    /// <summary>
    /// TMPR validation response with version-specific thresholds
    /// </summary>
    public class TMPRValidationResponse : BaseValidationResponse
    {
        /// <summary>
        /// OSO requiring TMPR validation
        /// </summary>
        public string OsoId { get; set; } = string.Empty;

        /// <summary>
        /// TMPR compliance status
        /// </summary>
        public bool TMPRCompliant { get; set; }

        /// <summary>
        /// Reliability threshold used for validation
        /// </summary>
        public double ReliabilityThreshold { get; set; }

        /// <summary>
        /// Actual reliability achieved
        /// </summary>
        public double ActualReliability { get; set; }

        /// <summary>
        /// TMPR systems evaluated
        /// </summary>
        public List<TMPRSystemEvaluation> SystemEvaluations { get; set; } = new();

        /// <summary>
        /// TMPR requirements for the specific version
        /// </summary>
        public string TMPRRequirements { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comprehensive validation response with full compliance details
    /// VERIFIED: JARUS SORA v2.5 - Aligned with SORA method sequence
    /// </summary>
    public class ComprehensiveValidationResponse : BaseValidationResponse
    {
        /// <summary>
        /// Per-OSO validation results with LoR evidence
        /// VERIFIED: JARUS Step #9 output
        /// </summary>
        public List<OSOResult> OSOResults { get; set; } = new();

        /// <summary>
        /// Initial ARC (before strategic mitigations)
        /// VERIFIED: JARUS Step #4 output
        /// </summary>
        public string OriginalARC { get; set; } = string.Empty;

        /// <summary>
        /// Residual ARC (after strategic mitigations, input to SAIL calculation)
        /// VERIFIED: JARUS Step #5 output - OSOs do NOT modify this
        /// </summary>
        public string FinalARC { get; set; } = string.Empty;

        // REMOVED: TargetSAIL - no such concept in JARUS SORA
        
        /// <summary>
        /// Final Ground Risk Class (after M1/M2 mitigations)
        /// VERIFIED: JARUS Step #3 output - used for SAIL calculation
        /// </summary>
        public int FinalGRC { get; set; }

        /// <summary>
        /// Computed SAIL from Final GRC × Residual ARC matrix
        /// VERIFIED: JARUS Step #6 - SAIL = f(Final GRC, Residual ARC)
        /// </summary>
        public int RecalculatedSAIL { get; set; }

        // REMOVED: M3PenaltiesApplied - M3 doesn't exist in SORA v2.5

        /// <summary>
        /// TMPR validation summary (if tactical mitigations claimed)
        /// VERIFIED: JARUS Annex D - mitigation-specific Risk Ratio validation
        /// </summary>
        public TMPRSummary? TMPRSummary { get; set; }

        /// <summary>
        /// Applied tactical mitigation path (e.g., "DAA", "ATC_SEPARATION")
        /// VERIFIED: JARUS Annex D - TMPR attaches to mitigation path
        /// </summary>
        public string? AppliedTacticalMitigation { get; set; }

        /// <summary>
        /// TMPR metrics for tactical mitigation (Risk Ratio, detection rate, etc.)
        /// VERIFIED: JARUS Annex D Table 1 - mitigation-specific metrics
        /// </summary>
        public Dictionary<string, double> TMPRMetrics { get; set; } = new();

        /// <summary>
        /// OSO evidence artifacts (procedures, training records, audits, certifications)
        /// VERIFIED: JARUS Annex E - LoR = Integrity × Assurance evidence
        /// </summary>
        public List<OSOEvidence> OSOEvidenceArtifacts { get; set; } = new();

        /// <summary>
        /// Mitigation recommendations
        /// </summary>
        public List<string> Recommendations { get; set; } = new();

        /// <summary>
        /// Overall compliance status (all required OSOs met for computed SAIL)
        /// </summary>
        public bool OverallCompliant { get; set; }
    }

    /// <summary>
    /// Risk mitigation response with version-specific matrix results
    /// </summary>
    public class RiskMitigationResponse : BaseValidationResponse
    {
        /// <summary>
        /// Risk level before mitigation
        /// </summary>
        public string RiskLevelBefore { get; set; } = string.Empty;

        /// <summary>
        /// Risk level after mitigation
        /// </summary>
        public string RiskLevelAfter { get; set; } = string.Empty;

        /// <summary>
        /// Mitigation strategies applied
        /// </summary>
        public List<MitigationStrategy> MitigationStrategies { get; set; } = new();

        /// <summary>
        /// Effectiveness of mitigation measures
        /// </summary>
        public double MitigationEffectiveness { get; set; }

        /// <summary>
        /// Version-specific mitigation matrix used
        /// </summary>
        public string MitigationMatrixVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Supporting classes for detailed responses
    /// </summary>
    
    /// <summary>
    /// OSO evidence artifact for LoR evaluation
    /// VERIFIED: JARUS Annex E - Integrity × Assurance evidence
    /// </summary>
    public class OSOEvidence
    {
        /// <summary>
        /// OSO identifier
        /// </summary>
        public string OSOId { get; set; } = string.Empty;
        
        /// <summary>
        /// Evidence type (e.g., "Procedure", "Training", "Audit", "Certification")
        /// </summary>
        public string EvidenceType { get; set; } = string.Empty;
        
        /// <summary>
        /// Evidence artifact reference (document ID, file path, etc.)
        /// </summary>
        public string ArtifactReference { get; set; } = string.Empty;
        
        /// <summary>
        /// Integrity score contribution (0.0-1.0)
        /// </summary>
        public double IntegrityScore { get; set; }
        
        /// <summary>
        /// Assurance score contribution (0.0-1.0)
        /// </summary>
        public double AssuranceScore { get; set; }
        
        /// <summary>
        /// Computed LoR = Integrity × Assurance
        /// </summary>
        public double ComputedLoR => IntegrityScore * AssuranceScore;
    }

    // REMOVED: M3Penalty class - M3 doesn't exist in SORA v2.5

    /// <summary>
    /// TMPR validation summary for tactical mitigations
    /// VERIFIED: JARUS Annex D - mitigation-specific Risk Ratio validation
    /// </summary>
    public class TMPRSummary
    {
        /// <summary>
        /// Tactical mitigation path validated (e.g., "DAA", "ATC_SEPARATION")
        /// </summary>
        public string TacticalMitigationPath { get; set; } = string.Empty;
        
        /// <summary>
        /// ARC class for which TMPR is validated
        /// </summary>
        public string ARCClass { get; set; } = string.Empty;
        
        /// <summary>
        /// Required Risk Ratio threshold per Annex D Table 1
        /// ARC-d: ≤ 0.1, ARC-c: ≤ 0.33, ARC-b: ≤ 0.66
        /// </summary>
        public double RequiredRiskRatio { get; set; }
        
        /// <summary>
        /// Measured/actual Risk Ratio for the tactical mitigation
        /// </summary>
        public double ActualRiskRatio { get; set; }
        
        /// <summary>
        /// TMPR compliance (ActualRiskRatio ≤ RequiredRiskRatio)
        /// </summary>
        public bool IsCompliant { get; set; }
        
        /// <summary>
        /// Validation messages for non-compliance
        /// </summary>
        public List<string> ValidationMessages { get; set; } = new();
        
        /// <summary>
        /// Failed TMPR details
        /// </summary>
        public List<string> FailedTMPRs { get; set; } = new();
    }

    public class TMPRSystemEvaluation
    {
        public string SystemType { get; set; } = string.Empty;
        public double ReliabilityFactor { get; set; }
        public bool MeetsThreshold { get; set; }
        public string EvaluationNotes { get; set; } = string.Empty;
    }

    public class MitigationStrategy
    {
        public string StrategyId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double EffectivenessRating { get; set; }
        public bool Applied { get; set; }
        public string ApplicationNotes { get; set; } = string.Empty;
    }
}