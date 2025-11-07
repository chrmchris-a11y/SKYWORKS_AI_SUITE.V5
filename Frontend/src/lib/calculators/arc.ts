// arc.ts
// ARC Calculator: AEC → Initial ARC → Strategic Mitigations → Residual ARC
// ✅ ref: JARUS SORA 2.5 Annex C Table C.1 + Table C.2 (Strategic mitigations)
// Source: Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs (lines 344-575)

import type { AEC, USpace, TrafficDensitySource, AirspaceContainment } from '../skyworks.enums';
import { AEC_TO_INITIAL_ARC, AEC_LABELS } from '../aec.labels';

export type ARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

/**
 * Strategic Mitigation Types
 * ✅ ref: JARUS SORA 2.5 Annex C Step #5 + Operational limitation guidance
 */
export interface StrategicMitigations {
  // Temporal mitigation (time-based restrictions)
  temporal: boolean;                    // e.g., fly only at night when traffic low
  
  // Spatial/Containment mitigation
  containment: AirspaceContainment;     // None | Horizontal | Vertical | Both
  
  // U-Space services
  uSpace: USpace;                       // Yes | No
  
  // Traffic density source (for validation)
  trafficDensitySource: TrafficDensitySource;  // Empirical | Modelled | ANSP
  
  // ✅ NEW: Step #5 - VLOS with low time exposure
  // Allows -1 ARC class reduction via operational limitation
  // Requires: VLOS operation, boundary definition, chronological documentation
  vlosLowTimeExposure?: {
    enabled: boolean;
    boundaryDefined: boolean;           // Operational volume defined
    chronologyDocumented: boolean;      // Time exposure evidence
  };
}

/**
 * ARC Calculation Input
 */
export interface ARCInput {
  aec: AEC;                             // Airspace Encounter Category (1-12)
  strategicMitigations?: StrategicMitigations;  // Optional mitigations
}

/**
 * ARC Calculation Result
 */
export interface ARCResult {
  aec: AEC;
  aecLabel: string;
  initialARC: ARC;
  residualARC: ARC;
  strategicMitigationsApplied: string[];
  arcReduction: number;                 // 0-2 levels
  explanation: string;
  auditTrail: string[];
}

/**
 * Calculate Initial ARC from AEC
 * ✅ ref: JARUS SORA 2.5 Annex C Table C.1
 * 
 * @param aec - Airspace Encounter Category
 * @returns Initial ARC (a/b/c/d)
 */
export function getInitialARC(aec: AEC): ARC {
  return AEC_TO_INITIAL_ARC[aec];
}

/**
 * Apply Strategic Mitigations to reduce ARC
 * ✅ ref: JARUS SORA 2.5 Annex C Table C.2
 * 
 * Maximum reduction: 2 levels (e.g., ARC-d → ARC-b)
 * 
 * Mitigation Effects:
 * - Temporal (time restrictions): -1 level
 * - Spatial/Containment: -1 level
 * - U-Space services: -1 level
 * - Traffic density source: Enables validation of mitigations
 * 
 * @param initialARC - Initial ARC from AEC
 * @param mitigations - Strategic mitigations applied
 * @returns Residual ARC after mitigations
 */
export function applyStrategicMitigations(
  initialARC: ARC,
  mitigations: StrategicMitigations
): {
  residualARC: ARC;
  reduction: number;
  mitigationsApplied: string[];
} {
  const arcLevels: ARC[] = ["ARC-a", "ARC-b", "ARC-c", "ARC-d"];
  let currentLevel = arcLevels.indexOf(initialARC);
  let reduction = 0;
  const mitigationsApplied: string[] = [];
  
  // Temporal mitigation (time-based restrictions)
  if (mitigations.temporal) {
    reduction += 1;
    mitigationsApplied.push("Temporal (time restrictions)");
  }
  
  // Spatial/Containment mitigation
  if (mitigations.containment !== "None") {
    reduction += 1;
    mitigationsApplied.push(`Containment (${mitigations.containment})`);
  }
  
  // U-Space services
  if (mitigations.uSpace === "Yes") {
    reduction += 1;
    mitigationsApplied.push("U-Space services");
  }
  
  // ✅ Step #5: VLOS low time exposure operational limitation
  // ref: EASA guidance - VLOS with documented low exposure → -1 ARC
  if (mitigations.vlosLowTimeExposure?.enabled) {
    const vlos = mitigations.vlosLowTimeExposure;
    if (vlos.boundaryDefined && vlos.chronologyDocumented) {
      reduction += 1;
      mitigationsApplied.push("VLOS low time exposure (operational limitation per Step #5)");
    }
  }
  
  // Cap reduction at 2 levels maximum
  if (reduction > 2) {
    reduction = 2;
  }
  
  // Apply reduction (lower index = better ARC)
  const newLevel = Math.max(0, currentLevel - reduction);
  const residualARC = arcLevels[newLevel];
  
  return {
    residualARC,
    reduction,
    mitigationsApplied,
  };
}

/**
 * Calculate ARC (Initial + Residual after mitigations)
 * 
 * @param input - ARC calculation input
 * @returns ARC calculation result
 */
export function calculateARC(input: ARCInput): ARCResult {
  const { aec, strategicMitigations } = input;
  const auditTrail: string[] = [];
  
  // Step 1: Get Initial ARC from AEC
  const initialARC = getInitialARC(aec);
  const aecLabel = AEC_LABELS[aec];
  
  auditTrail.push(`AEC: ${aec} - ${aecLabel}`);
  auditTrail.push(`Initial ARC: ${initialARC}`);
  
  // Step 2: Apply Strategic Mitigations (if any)
  let residualARC = initialARC;
  let arcReduction = 0;
  let strategicMitigationsApplied: string[] = [];
  
  if (strategicMitigations) {
    const result = applyStrategicMitigations(initialARC, strategicMitigations);
    residualARC = result.residualARC;
    arcReduction = result.reduction;
    strategicMitigationsApplied = result.mitigationsApplied;
    
    if (arcReduction > 0) {
      auditTrail.push(`Strategic mitigations applied: ${strategicMitigationsApplied.join(", ")}`);
      auditTrail.push(`ARC reduced by ${arcReduction} level(s): ${initialARC} → ${residualARC}`);
    }
  }
  
  auditTrail.push(`Residual ARC: ${residualARC}`);
  
  const explanation = arcReduction > 0
    ? `Initial ${initialARC} reduced to ${residualARC} via ${strategicMitigationsApplied.join(" + ")}`
    : `No strategic mitigations applied - ${initialARC} maintained`;
  
  return {
    aec,
    aecLabel,
    initialARC,
    residualARC,
    strategicMitigationsApplied,
    arcReduction,
    explanation,
    auditTrail,
  };
}

/**
 * Validate strategic mitigations require evidence
 * ✅ ref: JARUS SORA 2.5 Annex C - strategic mitigations need documentation
 */
export function validateStrategicMitigations(mitigations: StrategicMitigations): {
  isValid: boolean;
  warnings: string[];
} {
  const warnings: string[] = [];
  
  // Temporal mitigation requires evidence
  if (mitigations.temporal) {
    warnings.push("⚠️ Temporal mitigation requires documented time restrictions and traffic data");
  }
  
  // Containment requires evidence
  if (mitigations.containment !== "None") {
    warnings.push("⚠️ Containment mitigation requires documented operational volume and barriers");
  }
  
  // U-Space requires specific services
  if (mitigations.uSpace === "Yes") {
    warnings.push("⚠️ U-Space mitigation requires subscription to U-Space services and ANSP coordination");
  }
  
  // Traffic density source validation
  if (mitigations.trafficDensitySource === "Empirical") {
    warnings.push("ℹ️ Empirical traffic data requires operator-collected measurements");
  } else if (mitigations.trafficDensitySource === "Modelled") {
    warnings.push("ℹ️ Modelled traffic data requires validated simulation tools");
  } else if (mitigations.trafficDensitySource === "ANSP") {
    warnings.push("ℹ️ ANSP traffic data requires coordination with Air Navigation Service Provider");
  }
  
  return {
    isValid: true,  // Warnings are informational, not blocking
    warnings,
  };
}

/**
 * Get recommended strategic mitigations for AEC
 */
export function getRecommendedMitigations(aec: AEC): {
  temporal: boolean;
  containment: boolean;
  uSpace: boolean;
  explanation: string;
} {
  const initialARC = getInitialARC(aec);
  
  // High-risk ARC (ARC-d) - recommend all mitigations
  if (initialARC === "ARC-d") {
    return {
      temporal: true,
      containment: true,
      uSpace: true,
      explanation: "ARC-d is very high risk - recommend all strategic mitigations to reduce to ARC-b minimum",
    };
  }
  
  // Medium-risk ARC (ARC-c) - recommend some mitigations
  if (initialARC === "ARC-c") {
    return {
      temporal: true,
      containment: false,
      uSpace: true,
      explanation: "ARC-c is medium risk - recommend temporal + U-Space to reduce to ARC-a or ARC-b",
    };
  }
  
  // Low-risk ARC (ARC-b) - minimal mitigations
  if (initialARC === "ARC-b") {
    return {
      temporal: false,
      containment: false,
      uSpace: false,
      explanation: "ARC-b is low risk - strategic mitigations optional (can reduce to ARC-a if desired)",
    };
  }
  
  // Minimal-risk ARC (ARC-a) - no mitigations needed
  return {
    temporal: false,
    containment: false,
    uSpace: false,
    explanation: "ARC-a is minimal risk - no strategic mitigations required",
  };
}
