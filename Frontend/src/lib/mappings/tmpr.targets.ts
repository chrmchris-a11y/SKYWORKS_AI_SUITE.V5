/**
 * TMPR (Tactical Mitigation Performance Requirement) Targets - SORA 2.5
 * 
 * ✅ VERIFIED FROM OFFICIAL DOCUMENT:
 * Source: JAR-DEL-SRM-SORA-MB-2.5, Table 6, Page 45
 * Location: EXTRACTED_SORA-v2.5-Main-Body-Release-JAR_doc_25 (1).txt, lines 1297-1303
 * 
 * Table 6: Tactical Mitigation Performance Requirement (TMPR) and TMPR Level of Robustness Assignment
 * 
 * Residual ARC → TMPR → TMPR Level of Robustness
 * 
 * ⚠️ CRITICAL NOTES:
 * - TMPR only applies to BVLOS operations (Source: Section 4.6.3, Page 44)
 * - VLOS is considered acceptable tactical mitigation for all ARC levels
 * - TMPR defines requirements for DAA (Detect and Avoid) systems
 */

/**
 * TMPR Levels
 * Source: Table 6, Page 45
 */
export type TMPRLevel = "No requirement" | "Low" | "Medium" | "High";

/**
 * Residual ARC Categories
 * Source: Table 6 rows
 */
export type ResidualARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

/**
 * TMPR Target Definition
 */
export interface TMPRTarget {
  arc: ResidualARC;
  tmpr: TMPRLevel;
  robustness: TMPRLevel;
  description: string;
  applicability: "BVLOS only" | "Not required for VLOS";
  reference: string;
}

/**
 * TMPR Mapping Table
 * Source: JAR_doc_25 Main Body, Table 6, Page 45
 * 
 * Complete mapping from Residual ARC to TMPR requirements
 * 
 * Table structure:
 * | Residual ARC | TMPR           | TMPR Level of Robustness |
 * |--------------|----------------|--------------------------|
 * | ARC-d        | High           | High                     |
 * | ARC-c        | Medium         | Medium                   |
 * | ARC-b        | Low            | Low                      |
 * | ARC-a        | No requirement | No requirement           |
 */
export const TMPR_TARGETS: Record<ResidualARC, TMPRTarget> = {
  "ARC-d": {
    arc: "ARC-d",
    tmpr: "High",
    robustness: "High",
    description:
      "High encounter rate airspace - Requires high robustness DAA system for BVLOS",
    applicability: "BVLOS only",
    reference: "JAR-DEL-SRM-SORA-MB-2.5, Table 6, Page 45, Row 1",
  },
  "ARC-c": {
    arc: "ARC-c",
    tmpr: "Medium",
    robustness: "Medium",
    description:
      "Medium encounter rate airspace - Requires medium robustness DAA system for BVLOS",
    applicability: "BVLOS only",
    reference: "JAR-DEL-SRM-SORA-MB-2.5, Table 6, Page 45, Row 2",
  },
  "ARC-b": {
    arc: "ARC-b",
    tmpr: "Low",
    robustness: "Low",
    description:
      "Low encounter rate airspace - Requires low robustness DAA system for BVLOS",
    applicability: "BVLOS only",
    reference: "JAR-DEL-SRM-SORA-MB-2.5, Table 6, Page 45, Row 3",
  },
  "ARC-a": {
    arc: "ARC-a",
    tmpr: "No requirement",
    robustness: "No requirement",
    description:
      "Atypical/Segregated airspace - No TMPR required (even for BVLOS)",
    applicability: "Not required for VLOS",
    reference: "JAR-DEL-SRM-SORA-MB-2.5, Table 6, Page 45, Row 4",
  },
};

/**
 * Get TMPR target from Residual ARC
 * 
 * Source: JAR_doc_25 Main Body, Table 6, Page 45
 * 
 * @param residualARC - Residual Air Risk Class (after strategic mitigations)
 * @returns TMPR target definition
 * 
 * @example
 * getTMPRTarget("ARC-d") // Returns: { tmpr: "High", robustness: "High", ... }
 * getTMPRTarget("ARC-a") // Returns: { tmpr: "No requirement", ... }
 */
export function getTMPRTarget(residualARC: ResidualARC): TMPRTarget {
  return TMPR_TARGETS[residualARC];
}

/**
 * Get TMPR level only (simplified)
 * 
 * @param residualARC - Residual Air Risk Class
 * @returns TMPR level string
 * 
 * @example
 * getTMPRLevel("ARC-c") // Returns: "Medium"
 * getTMPRLevel("ARC-a") // Returns: "No requirement"
 */
export function getTMPRLevel(residualARC: ResidualARC): TMPRLevel {
  return TMPR_TARGETS[residualARC].tmpr;
}

/**
 * Check if TMPR is required for a given operation
 * 
 * Source: JAR_doc_25 Main Body, Section 4.6.3, Page 44
 * 
 * VLOS operations:
 * - VLOS is considered acceptable tactical mitigation for all ARC levels
 * - No TMPR/DAA system required for VLOS
 * 
 * BVLOS operations:
 * - Must meet TMPR requirements per Table 6
 * - Exception: ARC-a requires no TMPR even for BVLOS
 * 
 * @param residualARC - Residual Air Risk Class
 * @param isVLOS - Is operation VLOS?
 * @returns Whether TMPR is required
 * 
 * @example
 * isTMPRRequired("ARC-d", true)  // Returns: false (VLOS acceptable)
 * isTMPRRequired("ARC-d", false) // Returns: true (BVLOS needs High TMPR)
 * isTMPRRequired("ARC-a", false) // Returns: false (ARC-a no requirement)
 */
export function isTMPRRequired(
  residualARC: ResidualARC,
  isVLOS: boolean
): boolean {
  // VLOS is acceptable for all ARC levels
  // Source: Section 4.6.4, Page 45, Paragraph (a)
  if (isVLOS) {
    return false;
  }

  // ARC-a has no TMPR requirement even for BVLOS
  if (residualARC === "ARC-a") {
    return false;
  }

  // All other ARCs require TMPR for BVLOS
  return true;
}

/**
 * TMPR Robustness Descriptions
 * Source: Annex D (referenced in Table 6)
 * 
 * Detailed requirements for each robustness level
 */
export const TMPR_ROBUSTNESS_DESCRIPTIONS: Record<TMPRLevel, string> = {
  "No requirement": "No tactical mitigation system required (VLOS or ARC-a)",
  Low: "Low robustness DAA - Basic detect and avoid capability for low-density airspace",
  Medium:
    "Medium robustness DAA - Enhanced detect and avoid for medium-density airspace",
  High: "High robustness DAA - Advanced detect and avoid for high-density airspace",
};

/**
 * Get TMPR with detailed requirements
 * 
 * @param residualARC - Residual Air Risk Class
 * @param isVLOS - Is operation VLOS?
 * @returns Detailed TMPR requirements
 */
export function getTMPRRequirements(
  residualARC: ResidualARC,
  isVLOS: boolean
): {
  required: boolean;
  tmpr: TMPRLevel;
  robustness: TMPRLevel;
  description: string;
  applicabilityNote: string;
  reference: string;
} {
  const target = getTMPRTarget(residualARC);
  const required = isTMPRRequired(residualARC, isVLOS);

  let applicabilityNote: string;
  if (isVLOS) {
    applicabilityNote =
      "VLOS is acceptable tactical mitigation - No TMPR/DAA system required";
  } else if (residualARC === "ARC-a") {
    applicabilityNote =
      "Atypical/Segregated airspace - No TMPR required even for BVLOS";
  } else {
    applicabilityNote = `BVLOS operation - ${target.tmpr} TMPR required`;
  }

  return {
    required,
    tmpr: required ? target.tmpr : "No requirement",
    robustness: required ? target.robustness : "No requirement",
    description: target.description,
    applicabilityNote,
    reference: target.reference,
  };
}

/**
 * Validate TMPR inputs
 * 
 * @param residualARC - Residual Air Risk Class
 * @returns Validation result
 */
export function validateTMPRInputs(residualARC: ResidualARC): {
  valid: boolean;
  errors: string[];
} {
  const errors: string[] = [];

  const validARCs: ResidualARC[] = ["ARC-a", "ARC-b", "ARC-c", "ARC-d"];
  if (!validARCs.includes(residualARC)) {
    errors.push(
      `Residual ARC must be one of: ${validARCs.join(", ")}. Got: ${residualARC}`
    );
  }

  return {
    valid: errors.length === 0,
    errors,
  };
}

/**
 * TMPR Application Guidance
 * Source: JAR_doc_25 Main Body, Section 4.6.4, Pages 44-45
 */
export const TMPR_GUIDANCE = {
  vlos: {
    title: "VLOS/EVLOS Operations",
    points: [
      "VLOS is considered acceptable Tactical Mitigation for all ARC levels",
      "Operator advised to consider additional means to increase situational awareness",
      "EVLOS: All VLOS requirements apply, plus airspace observer must communicate continuously",
      "Multiple segments: VLOS segments don't need TMPR, BVLOS segments do",
    ],
    reference: "Section 4.6.4, Page 45, Paragraphs (a)-(d)",
  },
  bvlos: {
    title: "BVLOS Operations",
    points: [
      "Identify applicable TMPR level from Residual ARC using Table 6",
      "Identify applicable TMPR according to Annex D – Section 5",
      "Utilise Annex A, Section A.3 for presenting data to authority",
      "Tactical mitigations: DAA systems providing alternate means of achieving airspace safety",
    ],
    reference: "Section 4.6.3, Page 44, Paragraphs (a)-(c)",
  },
};

/**
 * Get all TMPR targets (for UI display)
 * 
 * @returns Array of all TMPR targets
 */
export function getAllTMPRTargets(): TMPRTarget[] {
  return Object.values(TMPR_TARGETS);
}

/**
 * Get TMPR targets sorted by robustness level
 * 
 * @returns Sorted array (No requirement → Low → Medium → High)
 */
export function getTMPRTargetsSorted(): TMPRTarget[] {
  const order: ResidualARC[] = ["ARC-a", "ARC-b", "ARC-c", "ARC-d"];
  return order.map((arc) => TMPR_TARGETS[arc]);
}
