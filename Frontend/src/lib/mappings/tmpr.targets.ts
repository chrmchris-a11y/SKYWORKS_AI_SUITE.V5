// tmpr.targets.ts
// TMPR (Tactical Mitigation Performance Requirement) targets per Residual ARC
// ✅ ref: JARUS SORA Annex D v1.0 Table 1 (Page 6)
// Source: KnowledgeBase/EASA DOCS SPLIT CHUNKS/EXTRACTED_SORA-Annex-D-v1.0.txt

import type { TMPRLevel } from '../skyworks.enums';

export type ARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

/**
 * TMPR Target Interface
 */
export interface TMPRTarget {
  arc: ARC;
  performance: TMPRLevel;
  systemRiskRatio: number | null;     // Target ratio (null for ARC-a)
  description: string;
  requirements: string[];
}

/**
 * TMPR Performance Requirements by Residual ARC
 * 
 * ✅ ref: JARUS SORA Annex D Table 1 - TMPR RR Requirements
 * 
 * System Risk Ratio = (Probability of MAC with mitigation) / (Probability of MAC without mitigation)
 * 
 * Key Targets:
 * - ARC-d (High Performance): System Risk Ratio ≤ 0.1 (90% reduction)
 * - ARC-c (Medium Performance): System Risk Ratio ≤ 0.33 (67% reduction)
 * - ARC-b (Low Performance): System Risk Ratio ≤ 0.66 (34% reduction)
 * - ARC-a (No Performance Req): No quantitative requirement (may still need CAA-deemed mitigation)
 * 
 * DAA Functions Required:
 * 1. Detect - Sense other aircraft (cooperative/non-cooperative)
 * 2. Decide - Assess collision risk
 * 3. Command - Generate avoidance maneuver
 * 4. Execute - Perform maneuver
 * 5. Escape - Continue safe separation
 */
export const TMPR_TARGETS: Record<ARC, TMPRTarget> = {
  "ARC-d": {
    arc: "ARC-d",
    performance: "High",
    systemRiskRatio: 0.1,
    description: "High Performance - Very high air traffic density (e.g., Class B/C/D airports, TMZ >500ft)",
    requirements: [
      "System Risk Ratio ≤ 0.1 (90% MAC risk reduction)",
      "High-integrity DAA system (Detect, Decide, Command, Execute, Escape)",
      "Cooperative detection (ADS-B, transponder) + Non-cooperative (radar, electro-optical)",
      "Automated collision avoidance with high assurance",
      "Redundant sensors and decision logic",
      "Continuous monitoring and validation",
      "Extensive evidence of performance (flight tests, simulations)",
    ],
  },
  "ARC-c": {
    arc: "ARC-c",
    performance: "Medium",
    systemRiskRatio: 0.33,
    description: "Medium Performance - Medium air traffic density (e.g., Class E/F/G airports, urban airspace)",
    requirements: [
      "System Risk Ratio ≤ 0.33 (67% MAC risk reduction)",
      "Medium-integrity DAA system",
      "Cooperative detection (ADS-B, transponder) required",
      "Non-cooperative detection (radar or electro-optical) recommended",
      "Semi-automated or manual collision avoidance",
      "Regular validation and testing",
      "Evidence of performance (flight tests or validated simulations)",
    ],
  },
  "ARC-b": {
    arc: "ARC-b",
    performance: "Low",
    systemRiskRatio: 0.66,
    description: "Low Performance - Low air traffic density (e.g., rural areas <500ft, above FL600)",
    requirements: [
      "System Risk Ratio ≤ 0.66 (34% MAC risk reduction)",
      "Low-integrity DAA system",
      "Cooperative detection (ADS-B or transponder) may suffice",
      "Visual observer or basic sensors acceptable",
      "Manual collision avoidance procedures",
      "Basic validation and testing",
      "Evidence of procedural compliance",
    ],
  },
  "ARC-a": {
    arc: "ARC-a",
    performance: "None",
    systemRiskRatio: null,
    description: "No Performance Requirement - Segregated/Atypical airspace with minimal traffic",
    requirements: [
      "No quantitative System Risk Ratio requirement",
      "CAA may require operator to demonstrate some mitigation as deemed necessary",
      "Typical mitigations: Operational restrictions, airspace segregation, visual observers",
      "Evidence of segregation or atypical conditions (e.g., NOTAMs, restricted airspace)",
    ],
  },
};

/**
 * Get TMPR target for a given Residual ARC
 * 
 * @param residualARC - Residual Air Risk Class (after strategic mitigations)
 * @returns TMPR target details
 */
export function getTMPRTarget(residualARC: ARC): TMPRTarget {
  return TMPR_TARGETS[residualARC];
}

/**
 * Get TMPR performance level from Residual ARC
 * 
 * @param residualARC - Residual Air Risk Class
 * @returns TMPR performance level (None/Low/Medium/High)
 */
export function getTMPRLevel(residualARC: ARC): TMPRLevel {
  return TMPR_TARGETS[residualARC].performance;
}

/**
 * Get System Risk Ratio target from Residual ARC
 * 
 * @param residualARC - Residual Air Risk Class
 * @returns Target System Risk Ratio (null for ARC-a)
 */
export function getSystemRiskRatioTarget(residualARC: ARC): number | null {
  return TMPR_TARGETS[residualARC].systemRiskRatio;
}

/**
 * Validate if a System Risk Ratio meets TMPR target
 * 
 * @param residualARC - Residual Air Risk Class
 * @param actualRatio - Actual System Risk Ratio achieved
 * @returns Validation result with compliance status
 */
export function validateTMPR(residualARC: ARC, actualRatio: number | null): {
  isCompliant: boolean;
  target: number | null;
  actual: number | null;
  message: string;
} {
  const target = getSystemRiskRatioTarget(residualARC);

  // ARC-a has no quantitative requirement
  if (residualARC === "ARC-a") {
    return {
      isCompliant: true,
      target: null,
      actual: actualRatio,
      message: "ARC-a: No quantitative TMPR requirement. Compliance determined by CAA review.",
    };
  }

  // Check if ratio is provided
  if (actualRatio === null || actualRatio === undefined) {
    return {
      isCompliant: false,
      target,
      actual: null,
      message: `${residualARC} requires System Risk Ratio ≤ ${target}. No ratio provided.`,
    };
  }

  // Validate ratio
  const isCompliant = actualRatio <= target!;

  return {
    isCompliant,
    target,
    actual: actualRatio,
    message: isCompliant
      ? `✅ TMPR compliant: Actual ratio ${actualRatio.toFixed(3)} ≤ target ${target}`
      : `❌ TMPR non-compliant: Actual ratio ${actualRatio.toFixed(3)} > target ${target}`,
  };
}

/**
 * Get recommended DAA capabilities for TMPR level
 */
export function getDAACapabilities(tmprLevel: TMPRLevel): {
  cooperative: string[];
  nonCooperative: string[];
  automation: string;
  assurance: string;
} {
  const capabilities = {
    High: {
      cooperative: ["ADS-B IN", "Transponder (Mode S)", "FLARM", "Remote ID"],
      nonCooperative: ["Radar (primary)", "Electro-optical (camera/LIDAR)", "Acoustic sensors"],
      automation: "Fully automated collision avoidance with redundancy",
      assurance: "High integrity (flight tests + extensive simulations + formal verification)",
    },
    Medium: {
      cooperative: ["ADS-B IN", "Transponder (Mode S)", "FLARM"],
      nonCooperative: ["Radar OR Electro-optical (recommended)"],
      automation: "Semi-automated collision avoidance (alerts + manual response)",
      assurance: "Medium integrity (flight tests OR validated simulations)",
    },
    Low: {
      cooperative: ["ADS-B IN OR Transponder"],
      nonCooperative: ["Visual observer", "Basic sensors (optional)"],
      automation: "Manual collision avoidance (visual separation + procedures)",
      assurance: "Low integrity (procedural compliance + basic testing)",
    },
    None: {
      cooperative: [],
      nonCooperative: [],
      automation: "No specific automation requirement (CAA-determined)",
      assurance: "Evidence of segregation or atypical conditions",
    },
  };

  return capabilities[tmprLevel];
}
