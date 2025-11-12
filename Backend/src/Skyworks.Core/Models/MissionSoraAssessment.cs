using System;

namespace Skyworks.Core.Models;

/// <summary>
/// SORA risk assessment results for the mission.
/// Stores calculated GRC, ARC, SAIL levels and all input parameters (snapshot for auditability).
/// Uses existing SORA API (/api/v1/sora/calculate) - DO NOT duplicate calculator logic.
/// </summary>
public class MissionSoraAssessment
{
    /// <summary>
    /// Unique assessment identifier
    /// </summary>
    public Guid MissionSoraAssessmentId { get; set; }
    
    /// <summary>
    /// Foreign key to parent Mission
    /// </summary>
    public Guid MissionId { get; set; }
    
    /// <summary>
    /// SORA version used ("2.0" or "2.5")
    /// </summary>
    public string SoraVersion { get; set; } = "2.5";
    
    /// <summary>
    /// Initial Ground Risk Class (before mitigations).
    /// Range: 0-8 (SORA 2.5), 1-8 (SORA 2.0).
    /// From SORA Table 2 (2.5) or Table 3 (2.0).
    /// </summary>
    public int InitialGrc { get; set; }
    
    /// <summary>
    /// Final Ground Risk Class (after M1/M2 mitigations).
    /// Range: 0-8 (SORA 2.5), 0-8 (SORA 2.0).
    /// Used for SAIL determination (SORA Table 5).
    /// </summary>
    public int FinalGrc { get; set; }
    
    /// <summary>
    /// Initial Air Risk Class (before tactical mitigations).
    /// Values: "a", "b", "c", "d" or "ARC-a", "ARC-b", "ARC-c", "ARC-d".
    /// From SORA Annex C: AEC (Air Environment Complexity) → ARC mapping.
    /// </summary>
    public string InitialArc { get; set; } = string.Empty;
    
    /// <summary>
    /// Residual Air Risk Class (after tactical mitigations like VLOS, strategic deconfliction).
    /// Values: "a", "b", "c", "d" or "ARC-a", "ARC-b", "ARC-c", "ARC-d".
    /// Determines OSO#23 robustness level (SORA Table 7).
    /// </summary>
    public string ResidualArc { get; set; } = string.Empty;
    
    /// <summary>
    /// Specific Assurance and Integrity Level (SAIL).
    /// Values: "I", "II", "III", "IV", "V", "VI", "Category C".
    /// From SORA Table 5 (fGRC, residualARC cross-reference).
    /// Determines required OSOs (Operational Safety Objectives).
    /// </summary>
    public string Sail { get; set; } = string.Empty;
    
    /// <summary>
    /// Snapshot of ALL SORA calculation inputs as JSON (for auditability and reproducibility).
    /// Includes:
    /// - Drone specs (MTOM, max speed, characteristic dimension)
    /// - Environment (population density, airspace class, operation type VLOS/BVLOS)
    /// - Mitigations (M1A, M1B, M1C, M2, M3 values)
    /// - Operational parameters (max height AGL, u-space, typicality)
    /// 
    /// Example:
    /// {
    ///   "drone": { "mtom_kg": 0.249, "maxSpeed_ms": 16, "charDim_m": 0.213 },
    ///   "environment": { "populationDensity": "<500", "airspaceClass": "G", "operationType": "VLOS" },
    ///   "mitigations": { "m1a": "Low", "m1b": "None", "m2": "Medium" },
    ///   "operational": { "maxHeightAGL_m": 400, "uSpace": false, "typicality": "Typical" }
    /// }
    /// </summary>
    public string InputsSnapshotJson { get; set; } = "{}";
    
    // Navigation property
    public Mission? Mission { get; set; }
}
