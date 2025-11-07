// grc20.ts
// SORA 2.0 GRC Calculator (iGRC → Final GRC via M1/M2/M3 mitigations with column-min clamp)
// ✅ ref: AMC1 to Article 11 (SORA 2.0) Table 2 + Table 3
// Source: Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs (lines 51-93, 127-138)

import type {
  M1_20,
  M2_20,
  M3_20,
} from '../skyworks.enums';

/**
 * GRC 2.0 Input Parameters
 */
export interface GRC20Input {
  intrinsicGRC: number;                 // From Table 2 (1-7)
  columnMinimumGRC: number;             // Column minimum (cannot reduce below this)
  m1: M1_20;                            // None | Low | Medium | High
  m2: M2_20;                            // None | Low | High (✅ FIXED per Annex B)
  m3: M3_20;                            // None | Adequate | Validated (✅ FIXED per Annex B)
}

/**
 * GRC 2.0 Calculation Result
 */
export interface GRC20Result {
  intrinsicGRC: number;
  finalGRC: number;
  columnMinimum: number;
  totalReduction: number;
  mitigationsApplied: Array<{
    type: string;
    robustness: string;
    credit: number;
    clamped?: boolean;
  }>;
  auditTrail: string[];
  isValid: boolean;
  validationMessage: string;
}

/**
 * Mitigation Credits (SORA 2.0 Table 3)
 * ✅ ref: Backend GRCCalculationService.cs GetMitigationCredit_V2_0 (lines 127-138)
 */
const MITIGATION_CREDITS_20 = {
  // M1 - Strategic mitigations (reduce people at risk)
  M1_Low: -1,
  M1_Medium: -2,
  M1_High: -4,
  
  // M2 - Impact reduction (e.g., parachute)
  // ✅ FIXED per Annex B: Changed Medium → Low
  M2_Low: -1,
  M2_High: -2,
  
  // M3 - Emergency Response Plan
  // ✅ FIXED per Annex B: Changed Low/Medium/High → None/Adequate/Validated
  M3_None: +1,          // Penalty if no ERP
  M3_Adequate: 0,       // Neutral (basic ERP)
  M3_Validated: -1,     // Credit for validated ERP
};

/**
 * Calculate Final GRC (SORA 2.0)
 * 
 * ⚠️ CRITICAL: Must apply mitigations in sequence: M1 → M2 → M3
 * ⚠️ CRITICAL: M1 cannot reduce below column minimum (clamp)
 * ⚠️ CRITICAL: Final GRC ≥ 1 (floor)
 * 
 * @param input - GRC 2.0 input parameters
 * @returns GRC 2.0 calculation result
 */
export function calculateGRC20(input: GRC20Input): GRC20Result {
  const { intrinsicGRC, columnMinimumGRC, m1, m2, m3 } = input;
  const auditTrail: string[] = [];
  const mitigationsApplied: Array<{ type: string; robustness: string; credit: number; clamped?: boolean }> = [];
  
  let currentGRC = intrinsicGRC;
  let totalReduction = 0;
  
  auditTrail.push(`Intrinsic GRC = ${intrinsicGRC}`);
  auditTrail.push(`Column Minimum = ${columnMinimumGRC}`);
  
  // Step 1: Apply M1 (Strategic mitigations) with column-min clamp
  // ✅ ref: Backend lines 67-73
  if (m1 !== "None") {
    let credit = 0;
    if (m1 === "Low") credit = MITIGATION_CREDITS_20.M1_Low;
    else if (m1 === "Medium") credit = MITIGATION_CREDITS_20.M1_Medium;
    else if (m1 === "High") credit = MITIGATION_CREDITS_20.M1_High;
    
    const proposedGRC = currentGRC + credit;
    let actualCredit = credit;
    let clamped = false;
    
    // COLUMN-MIN CLAMP: Cannot reduce below column minimum
    if (proposedGRC < columnMinimumGRC) {
      actualCredit = columnMinimumGRC - currentGRC;
      clamped = true;
      auditTrail.push(`⚠️ M1 ${m1}: Proposed GRC ${proposedGRC} < column minimum ${columnMinimumGRC} → clamped to ${actualCredit}`);
    }
    
    currentGRC += actualCredit;
    totalReduction -= actualCredit;  // Credits are negative, so subtract
    
    mitigationsApplied.push({
      type: "M1 Strategic",
      robustness: m1,
      credit: actualCredit,
      clamped,
    });
    
    auditTrail.push(`M1 ${m1}: ${actualCredit} (${clamped ? 'clamped' : 'full credit'}) → GRC = ${currentGRC}`);
  }
  
  // Step 2: Apply M2 (Impact reduction)
  if (m2 !== "None") {
    let credit = 0;
    if (m2 === "Low") credit = MITIGATION_CREDITS_20.M2_Low;
    else if (m2 === "High") credit = MITIGATION_CREDITS_20.M2_High;
    
    currentGRC += credit;
    totalReduction -= credit;
    
    mitigationsApplied.push({
      type: "M2 Impact",
      robustness: m2,
      credit,
    });
    
    auditTrail.push(`M2 ${m2}: ${credit} → GRC = ${currentGRC}`);
  }
  
  // Step 3: Apply M3 (Emergency Response Plan)
  let credit = 0;
  if (m3 === "None") credit = MITIGATION_CREDITS_20.M3_None;           // +1 penalty
  else if (m3 === "Adequate") credit = MITIGATION_CREDITS_20.M3_Adequate;  // 0
  else if (m3 === "Validated") credit = MITIGATION_CREDITS_20.M3_Validated; // -1
  
  currentGRC += credit;
  totalReduction -= credit;
  
  mitigationsApplied.push({
    type: "M3 ERP",
    robustness: m3,
    credit,
  });
  
  auditTrail.push(`M3 ${m3}: ${credit} → GRC = ${currentGRC}`);
  
  // Step 4: Floor at GRC = 1
  // ✅ ref: Backend lines 85-88
  if (currentGRC < 1) {
    auditTrail.push(`⚠️ GRC floored at 1 (was ${currentGRC})`);
    currentGRC = 1;
  }
  
  const finalGRC = currentGRC;
  auditTrail.push(`Final GRC = ${finalGRC}`);
  
  // Step 5: Validation
  // ✅ ref: Backend lines 91-94
  const isValid = finalGRC >= 1 && finalGRC <= 7;
  let validationMessage = "";
  
  if (finalGRC > 7) {
    validationMessage = `❌ Final GRC ${finalGRC} exceeds SORA 2.0 scope (max 7). Operation requires Certified category.`;
    auditTrail.push(validationMessage);
  } else if (finalGRC < 1) {
    validationMessage = `❌ Final GRC ${finalGRC} < 1 (invalid). Check calculation logic.`;
    auditTrail.push(validationMessage);
  } else {
    validationMessage = `✅ Final GRC ${finalGRC} within SORA 2.0 scope (1-7)`;
  }
  
  return {
    intrinsicGRC,
    finalGRC,
    columnMinimum: columnMinimumGRC,
    totalReduction,
    mitigationsApplied,
    auditTrail,
    isValid,
    validationMessage,
  };
}

/**
 * Validate M2 level (SORA 2.0 has NO "Low" level for M2)
 * ✅ ref: JARUS SORA 2.0 Table 3 - M2 only has None/Medium/High
 */
export function validateM2Level(m2: string): { isValid: boolean; message: string } {
  if (m2 === "Low") {
    return {
      isValid: false,
      message: "❌ M2 'Low' is not valid in SORA 2.0. Use None/Medium/High only.",
    };
  }
  
  return {
    isValid: true,
    message: "✅ M2 level valid",
  };
}

/**
 * Get M1 column minimum from Table 2
 * ✅ ref: SORA 2.0 Table 2 - each column has a minimum GRC that M1 cannot reduce below
 * 
 * Simplified mapping (full backend has complete Table 2 logic):
 * - Column 1 (1m): min GRC = 1
 * - Column 2 (3m): min GRC = 2
 * - Column 3 (8m): min GRC = 3
 * - Column 4 (>8m): min GRC = 4
 */
export function getColumnMinimum(dimensionM: number): number {
  if (dimensionM <= 1) return 1;
  if (dimensionM <= 3) return 2;
  if (dimensionM <= 8) return 3;
  return 4;  // >8m
}
