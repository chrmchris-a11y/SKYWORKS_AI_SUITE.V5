// sail.ts
// SAIL Calculator: Final GRC × Residual ARC → SAIL (I-VI) + OSO Requirements
// ✅ ref: JARUS SORA 2.0 Table 5 + Table 6 | SORA 2.5 Table 7 + Table 14
// Source: Frontend/src/lib/mappings/sail.matrix.ts

import { SAIL_MATRIX, type SAIL, type ARC, getSAIL, determineSAIL } from '../mappings/sail.matrix';
import { getTMPRTarget, type TMPRTarget } from '../mappings/tmpr.targets';

/**
 * SAIL Calculation Input
 */
export interface SAILInput {
  finalGRC: number;                     // After GRC mitigations (1-7)
  residualARC: ARC;                     // After ARC strategic mitigations
  soraVersion: "SORA_2_0_AMC" | "SORA_2_5_JARUS";  // For OSO selection
}

/**
 * OSO (Operational Safety Objective) Requirement
 */
export interface OSORequirement {
  id: string;                           // e.g., "OSO-01"
  name: string;
  robustness: "Low" | "Medium" | "High" | "O";  // O = Optional
  evidenceSource?: "Operator" | "Manufacturer";  // SORA 2.5 distinction
  description: string;
}

/**
 * SAIL Calculation Result
 */
export interface SAILResult {
  finalGRC: number;
  residualARC: ARC;
  sail: SAIL | null;
  tmprTarget: TMPRTarget;
  requiredOSOs: OSORequirement[];
  isValid: boolean;
  validationMessage: string;
  requiresCertified: boolean;
  auditTrail: string[];
}

/**
 * Calculate SAIL from Final GRC and Residual ARC
 * 
 * @param input - SAIL calculation input
 * @returns SAIL result with OSO requirements
 */
export function calculateSAIL(input: SAILInput): SAILResult {
  const { finalGRC, residualARC, soraVersion } = input;
  const auditTrail: string[] = [];
  
  auditTrail.push(`Final GRC: ${finalGRC}`);
  auditTrail.push(`Residual ARC: ${residualARC}`);
  
  // Step 1: Determine SAIL from matrix
  const sailResult = determineSAIL(finalGRC, residualARC);
  
  auditTrail.push(sailResult.message);
  
  // Step 2: Get TMPR target for residual ARC
  const tmprTarget = getTMPRTarget(residualARC);
  auditTrail.push(`TMPR Target: ${tmprTarget.performance} (System Risk Ratio ${tmprTarget.systemRiskRatio ?? 'N/A'})`);
  
  // Step 3: Get required OSOs for SAIL
  const requiredOSOs = sailResult.sail
    ? getRequiredOSOs(sailResult.sail, soraVersion)
    : [];
  
  if (requiredOSOs.length > 0) {
    auditTrail.push(`Required OSOs: ${requiredOSOs.length} objectives`);
  }
  
  return {
    finalGRC,
    residualARC,
    sail: sailResult.sail,
    tmprTarget,
    requiredOSOs,
    isValid: sailResult.isValid,
    validationMessage: sailResult.message,
    requiresCertified: sailResult.requiresCertified,
    auditTrail,
  };
}

/**
 * Get required OSOs for a SAIL level
 * ✅ ref: JARUS SORA 2.0 Table 6 (24 OSOs) | SORA 2.5 Table 14
 * 
 * @param sail - SAIL level (I-VI)
 * @param soraVersion - SORA version (2.0 or 2.5)
 * @returns List of required OSOs with robustness levels
 */
export function getRequiredOSOs(sail: SAIL, soraVersion: string): OSORequirement[] {
  // Common OSOs for all SORA versions (based on SORA 2.0 Table 6)
  // Full OSO list would be imported from separate file
  
  const osoRequirements: Record<SAIL, Record<string, "Low" | "Medium" | "High" | "O">> = {
    "I": {
      "OSO-01": "O",
      "OSO-02": "O",
      "OSO-03": "Low",
      "OSO-07": "Low",
      "OSO-08": "Low",
      "OSO-09": "Low",
      "OSO-10": "Low",
      "OSO-11": "Low",
      "OSO-13": "Low",
      "OSO-14": "Low",
      "OSO-19": "Low",
    },
    "II": {
      "OSO-01": "Low",
      "OSO-02": "O",
      "OSO-03": "Low",
      "OSO-06": "Low",
      "OSO-07": "Low",
      "OSO-08": "Medium",
      "OSO-09": "Low",
      "OSO-10": "Low",
      "OSO-11": "Medium",
      "OSO-12": "Low",
      "OSO-13": "Low",
      "OSO-14": "Medium",
      "OSO-19": "Low",
    },
    "III": {
      "OSO-01": "Medium",
      "OSO-02": "Low",
      "OSO-03": "Medium",
      "OSO-04": "O",
      "OSO-05": "Low",
      "OSO-06": "Low",
      "OSO-07": "Medium",
      "OSO-08": "High",
      "OSO-09": "Medium",
      "OSO-10": "Medium",
      "OSO-11": "High",
      "OSO-12": "Medium",
      "OSO-13": "Medium",
      "OSO-14": "High",
      "OSO-18": "Low",
      "OSO-19": "Medium",
      "OSO-20": "Medium",
    },
    "IV": {
      "OSO-01": "High",
      "OSO-02": "Medium",
      "OSO-03": "Medium",
      "OSO-04": "Low",
      "OSO-05": "Medium",
      "OSO-06": "Medium",
      "OSO-07": "Medium",
      "OSO-08": "High",
      "OSO-09": "Medium",
      "OSO-10": "Medium",
      "OSO-11": "High",
      "OSO-12": "Medium",
      "OSO-13": "High",
      "OSO-14": "High",
      "OSO-15": "Medium",
      "OSO-16": "Low",
      "OSO-17": "Low",
      "OSO-18": "Medium",
      "OSO-19": "High",
      "OSO-20": "High",
      "OSO-21": "Medium",
      "OSO-22": "Medium",
      "OSO-23": "Medium",
      "OSO-24": "Medium",
    },
    "V": {
      "OSO-01": "High",
      "OSO-02": "High",
      "OSO-03": "High",
      "OSO-04": "Medium",
      "OSO-05": "High",
      "OSO-06": "High",
      "OSO-07": "High",
      "OSO-08": "High",
      "OSO-09": "High",
      "OSO-10": "High",
      "OSO-11": "High",
      "OSO-12": "High",
      "OSO-13": "High",
      "OSO-14": "High",
      "OSO-15": "High",
      "OSO-16": "Medium",
      "OSO-17": "Medium",
      "OSO-18": "High",
      "OSO-19": "High",
      "OSO-20": "High",
      "OSO-21": "High",
      "OSO-22": "High",
      "OSO-23": "High",
      "OSO-24": "High",
    },
    "VI": {
      "OSO-01": "High",
      "OSO-02": "High",
      "OSO-03": "High",
      "OSO-04": "High",
      "OSO-05": "High",
      "OSO-06": "High",
      "OSO-07": "High",
      "OSO-08": "High",
      "OSO-09": "High",
      "OSO-10": "High",
      "OSO-11": "High",
      "OSO-12": "High",
      "OSO-13": "High",
      "OSO-14": "High",
      "OSO-15": "High",
      "OSO-16": "High",
      "OSO-17": "High",
      "OSO-18": "High",
      "OSO-19": "High",
      "OSO-20": "High",
      "OSO-21": "High",
      "OSO-22": "High",
      "OSO-23": "High",
      "OSO-24": "High",
    },
  };
  
  const osoNames: Record<string, string> = {
    "OSO-01": "Operator competent/proven",
    "OSO-02": "UAS manufactured by competent entity",
    "OSO-03": "UAS maintained by competent entity",
    "OSO-04": "UAS designed to recognized design standards",
    "OSO-05": "UAS system/reliability",
    "OSO-06": "C3 link performance appropriate",
    "OSO-07": "Inspection of UAS",
    "OSO-08": "Operational procedures defined/validated",
    "OSO-09": "Remote crew trained/current",
    "OSO-10": "Safe recovery from technical issue",
    "OSO-11": "External systems deterioration procedures",
    "OSO-12": "UAS designed for external systems deterioration",
    "OSO-13": "External services adequate",
    "OSO-14": "Operational procedures (Human Error)",
    "OSO-15": "Operational procedures validated",
    "OSO-16": "Multi-crew coordination",
    "OSO-17": "Human-Machine Interface",
    "OSO-18": "Operating instructions clear/complete",
    "OSO-19": "Adverse operating conditions",
    "OSO-20": "Multi-UAS interference",
    "OSO-21": "Security",
    "OSO-22": "Detect & avoid performance",
    "OSO-23": "Ground control station siting",
    "OSO-24": "Automatic protection of flight envelope",
  };
  
  const requirements = osoRequirements[sail] || {};
  
  return Object.entries(requirements)
    .filter(([_, robustness]) => robustness !== "O")  // Exclude optional OSOs
    .map(([id, robustness]) => ({
      id,
      name: osoNames[id] || id,
      robustness: robustness as "Low" | "Medium" | "High",
      description: `${id}: ${osoNames[id] || id} - ${robustness} robustness required`,
    }));
}

/**
 * Validate SAIL result is achievable
 */
export function validateSAILAchievability(result: SAILResult): {
  isAchievable: boolean;
  blockers: string[];
  recommendations: string[];
} {
  const blockers: string[] = [];
  const recommendations: string[] = [];
  
  // Check if out of scope
  if (result.requiresCertified) {
    blockers.push("Final GRC > 7 - requires Certified category (EASA Part 21)");
    return { isAchievable: false, blockers, recommendations };
  }
  
  // Check SAIL VI (highest risk)
  if (result.sail === "VI") {
    recommendations.push("SAIL VI requires all OSOs at High robustness - extensive evidence needed");
    recommendations.push("Consider reducing Final GRC or improving Residual ARC to lower SAIL");
  }
  
  // Check ARC-d (always results in SAIL VI unless GRC very low)
  if (result.residualARC === "ARC-d" && result.finalGRC > 2) {
    recommendations.push("ARC-d with GRC > 2 results in SAIL VI - apply strategic mitigations to reduce ARC");
  }
  
  // Check TMPR requirements
  if (result.tmprTarget.systemRiskRatio !== null) {
    recommendations.push(`TMPR requires System Risk Ratio ≤ ${result.tmprTarget.systemRiskRatio} (${result.tmprTarget.performance} performance)`);
  }
  
  return {
    isAchievable: blockers.length === 0,
    blockers,
    recommendations,
  };
}
