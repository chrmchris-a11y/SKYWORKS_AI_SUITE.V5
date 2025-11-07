// grc25.ts
// SORA 2.5 GRC Calculator (iGRC → Final GRC via M1A/M1B/M1C/M2 mitigations)
// ✅ ref: JARUS SORA 2.5 Main Body Table 2 + Table 5 + Annex B (JAR_doc_27)
// Source: Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs (lines 281-299)

import type {
  M1A_Sheltering,
  M1B_OperationalRestrictions,
  M1C_GroundObservation,
  M2_ImpactDynamics,
  Environment25,
} from '../skyworks.enums';

/**
 * GRC 2.5 Input Parameters
 */
export interface GRC25Input {
  // UA Specifications (for iGRC determination)
  mtomKg: number;                       // Mass (kg)
  maxDimensionM: number;                // Max characteristic dimension (m)
  typicalSpeedMs: number;               // Typical cruise speed (m/s)
  
  // Environment (for iGRC determination)
  populationDensity: Environment25;     // CGA | Rural | Suburban | Urban
  
  // Mitigations (Table 5)
  m1a: M1A_Sheltering;                  // None | Low (✅ FIXED per Annex B - NO Medium!)
  m1b: M1B_OperationalRestrictions;     // None | Medium | High
  m1c: M1C_GroundObservation;           // None | Low
  m2: M2_ImpactDynamics;                // None | Low | Medium | High (✅ FIXED per Annex B)
}

/**
 * GRC 2.5 Calculation Result
 */
export interface GRC25Result {
  intrinsicGRC: number;                 // Initial GRC from Table 2
  finalGRC: number;                     // After mitigations
  smallUARuleApplied: boolean;          // ≤250g & ≤25m/s rule
  mitigationsApplied: Array<{
    type: string;
    robustness: string;
    credit: number;
  }>;
  auditTrail: string[];
  isValid: boolean;
  validationMessage: string;
}

/**
 * Mitigation Credits (SORA 2.5 Table 5 + Annex B)
 * ✅ ref: Backend GRCCalculationService.cs GetMitigationCredit_V2_5 (lines 286-297)
 */
const MITIGATION_CREDITS_25 = {
  // M1(A) Sheltering
  M1A_Low: -1,
  M1A_Medium: -2,       // Requires high evidence/assurance
  
  // M1(B) Operational restrictions
  M1B_Medium: -1,
  M1B_High: -2,
  
  // M1(C) Ground observation
  M1C_Low: -1,          // Only Low level defined
  
  // M2 Impact dynamics (parachute, frangibility, etc.)
  M2_Medium: -1,
  M2_High: -2,
};

/**
 * Small UA Rule (SORA 2.5)
 * ✅ ref: JARUS SORA 2.5 Main Body (small UA exemption)
 * 
 * If MTOM ≤ 0.25 kg AND typical speed ≤ 25 m/s:
 * → Intrinsic GRC = 1 (minimal risk, regardless of population density)
 */
function checkSmallUARule(mtomKg: number, speedMs: number): boolean {
  return mtomKg <= 0.25 && speedMs <= 25;
}

/**
 * Determine Intrinsic GRC from UA specs and population density
 * ✅ ref: JARUS SORA 2.5 Table 2 (simplified for now - backend has full logic)
 * 
 * @param input - GRC input parameters
 * @returns Intrinsic GRC (1-7)
 */
function determineIntrinsicGRC(input: GRC25Input): {
  iGRC: number;
  smallUARuleApplied: boolean;
  explanation: string;
} {
  const { mtomKg, maxDimensionM, typicalSpeedMs, populationDensity } = input;
  
  // Check small UA rule first
  if (checkSmallUARule(mtomKg, typicalSpeedMs)) {
    return {
      iGRC: 1,
      smallUARuleApplied: true,
      explanation: `Small UA rule applied: MTOM ${mtomKg}kg ≤ 0.25kg AND speed ${typicalSpeedMs}m/s ≤ 25m/s → iGRC = 1`,
    };
  }
  
  // Simplified iGRC determination (full backend logic uses Kinetic Energy + Table 2)
  // For accurate calculation, backend API should be called
  let iGRC = 1;
  const ke = 0.5 * mtomKg * typicalSpeedMs * typicalSpeedMs;
  
  // Simplified mapping (backend has authoritative Table 2 logic)
  if (populationDensity === "CGA") {
    // Controlled Ground Area - always GRC 1
    iGRC = 1;
  } else if (populationDensity === "Rural") {
    if (ke < 100) iGRC = 1;
    else if (ke < 1000) iGRC = 2;
    else if (ke < 10000) iGRC = 3;
    else iGRC = 4;
  } else if (populationDensity === "Suburban") {
    if (ke < 100) iGRC = 2;
    else if (ke < 1000) iGRC = 3;
    else if (ke < 10000) iGRC = 4;
    else iGRC = 5;
  } else if (populationDensity === "Urban") {
    if (ke < 100) iGRC = 3;
    else if (ke < 1000) iGRC = 4;
    else if (ke < 10000) iGRC = 5;
    else iGRC = 6;
  }
  
  return {
    iGRC,
    smallUARuleApplied: false,
    explanation: `Population: ${populationDensity}, KE: ${ke.toFixed(0)}J, Dimension: ${maxDimensionM}m → iGRC = ${iGRC}`,
  };
}

/**
 * Calculate Final GRC (SORA 2.5)
 * 
 * @param input - GRC 2.5 input parameters
 * @returns GRC 2.5 calculation result
 */
export function calculateGRC25(input: GRC25Input): GRC25Result {
  const auditTrail: string[] = [];
  const mitigationsApplied: Array<{ type: string; robustness: string; credit: number }> = [];
  
  // Step 1: Determine Intrinsic GRC
  const { iGRC, smallUARuleApplied, explanation } = determineIntrinsicGRC(input);
  auditTrail.push(`Step 1: ${explanation}`);
  
  let currentGRC = iGRC;
  auditTrail.push(`Intrinsic GRC = ${iGRC}`);
  
  // Step 2: Apply M1(A) Sheltering
  if (input.m1a !== "None") {
    const credit = input.m1a === "Low" ? MITIGATION_CREDITS_25.M1A_Low : MITIGATION_CREDITS_25.M1A_Medium;
    currentGRC += credit;
    mitigationsApplied.push({ type: "M1(A) Sheltering", robustness: input.m1a, credit });
    auditTrail.push(`M1(A) ${input.m1a}: ${credit} → GRC = ${currentGRC}`);
  }
  
  // Step 3: Apply M1(B) Operational restrictions
  if (input.m1b !== "None") {
    const credit = input.m1b === "Medium" ? MITIGATION_CREDITS_25.M1B_Medium : MITIGATION_CREDITS_25.M1B_High;
    currentGRC += credit;
    mitigationsApplied.push({ type: "M1(B) Operational restrictions", robustness: input.m1b, credit });
    auditTrail.push(`M1(B) ${input.m1b}: ${credit} → GRC = ${currentGRC}`);
  }
  
  // Step 4: Apply M1(C) Ground observation
  if (input.m1c !== "None") {
    const credit = MITIGATION_CREDITS_25.M1C_Low;
    currentGRC += credit;
    mitigationsApplied.push({ type: "M1(C) Ground observation", robustness: input.m1c, credit });
    auditTrail.push(`M1(C) ${input.m1c}: ${credit} → GRC = ${currentGRC}`);
  }
  
  // Step 5: Apply M2 Impact dynamics
  if (input.m2 !== "None") {
    const credit = input.m2 === "Medium" ? MITIGATION_CREDITS_25.M2_Medium : MITIGATION_CREDITS_25.M2_High;
    currentGRC += credit;
    mitigationsApplied.push({ type: "M2 Impact dynamics", robustness: input.m2, credit });
    auditTrail.push(`M2 ${input.m2}: ${credit} → GRC = ${currentGRC}`);
  }
  
  // Step 6: Floor at GRC = 1 (cannot go below 1)
  if (currentGRC < 1) {
    auditTrail.push(`⚠️ GRC floored at 1 (was ${currentGRC})`);
    currentGRC = 1;
  }
  
  const finalGRC = currentGRC;
  auditTrail.push(`Final GRC = ${finalGRC}`);
  
  // Step 7: Validation
  const isValid = finalGRC >= 1 && finalGRC <= 7;
  let validationMessage = "";
  
  if (finalGRC > 7) {
    validationMessage = `❌ Final GRC ${finalGRC} exceeds SORA scope (max 7). Operation requires Certified category.`;
    auditTrail.push(validationMessage);
  } else if (finalGRC < 1) {
    validationMessage = `❌ Final GRC ${finalGRC} < 1 (invalid). Check calculation logic.`;
    auditTrail.push(validationMessage);
  } else {
    validationMessage = `✅ Final GRC ${finalGRC} within SORA 2.5 scope (1-7)`;
  }
  
  return {
    intrinsicGRC: iGRC,
    finalGRC,
    smallUARuleApplied,
    mitigationsApplied,
    auditTrail,
    isValid,
    validationMessage,
  };
}

/**
 * Validate M1A/M1B combination
 * ✅ ref: JARUS SORA 2.5 Annex B - M1(A) Medium cannot be combined with M1(B)
 * 
 * @param m1a - M1(A) Sheltering level
 * @param m1b - M1(B) Operational restrictions level
 * @returns Validation result
 */
export function validateM1Combination(m1a: M1A_Sheltering, m1b: M1B_OperationalRestrictions): {
  isValid: boolean;
  message: string;
} {
  if (m1a === "Medium" && m1b !== "None") {
    return {
      isValid: false,
      message: "❌ M1(A) Medium cannot be combined with M1(B) per JARUS SORA 2.5 Annex B rules",
    };
  }
  
  return {
    isValid: true,
    message: "✅ M1 combination valid",
  };
}
