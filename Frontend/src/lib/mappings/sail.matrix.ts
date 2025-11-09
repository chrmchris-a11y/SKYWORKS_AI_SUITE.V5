/**
 * SAIL Matrix Mapping - SORA 2.5
 * 
 * ✅ VERIFIED FROM OFFICIAL DOCUMENT:
 * Source: JAR-DEL-SRM-SORA-MB-2.5, Table 7, Page 47
 * Location: EXTRACTED_SORA-v2.5-Main-Body-Release-JAR_doc_25 (1).txt, lines 1393-1406
 * 
 * Table 7: SAIL Determination
 * Final GRC × Residual ARC → SAIL (I-VI)
 * 
 * SAIL = Specific Assurance and Integrity Level
 * 
 * ⚠️ CRITICAL: This table is IDENTICAL for both SORA 2.0 and SORA 2.5
 * Source: SORA 2.0 Main Body Table 5 (page 27) = SORA 2.5 Main Body Table 7 (page 47)
 */

/**
 * SAIL Categories
 * 
 * I-VI: Progressive levels of operational safety objectives (OSO) robustness
 * >7: Requires Category C (Certified) operation
 */
export type SAIL = "I" | "II" | "III" | "IV" | "V" | "VI" | "Category C";

/**
 * Residual ARC Categories
 * Source: Table 7 columns
 */
export type ResidualARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

/**
 * Final GRC Range
 * Source: Table 7 rows
 * 
 * ≤2, 3, 4, 5, 6, 7, >7
 */
export type FinalGRC = number; // 0-infinity (but >7 requires certified)

/**
 * SAIL Matrix - Complete Table 7
 * Source: JAR_doc_25 Main Body, Table 7, Page 47
 * 
 * Matrix Structure:
 * - Rows: Final GRC (≤2, 3, 4, 5, 6, 7, >7)
 * - Columns: Residual ARC (a, b, c, d)
 * 
 * @example
 * // GRC=4, ARC-c → SAIL IV
 * const sail = SAIL_MATRIX[4]["ARC-c"]; // "IV"
 * 
 * // GRC=2, ARC-a → SAIL I
 * const sail = SAIL_MATRIX[2]["ARC-a"]; // "I"
 */
export const SAIL_MATRIX: Record<number, Record<ResidualARC, SAIL>> = {
  // GRC ≤ 2
  0: { "ARC-a": "I", "ARC-b": "II", "ARC-c": "IV", "ARC-d": "VI" },
  1: { "ARC-a": "I", "ARC-b": "II", "ARC-c": "IV", "ARC-d": "VI" },
  2: { "ARC-a": "I", "ARC-b": "II", "ARC-c": "IV", "ARC-d": "VI" },

  // GRC = 3
  3: { "ARC-a": "II", "ARC-b": "II", "ARC-c": "IV", "ARC-d": "VI" },

  // GRC = 4
  4: { "ARC-a": "III", "ARC-b": "III", "ARC-c": "IV", "ARC-d": "VI" },

  // GRC = 5
  5: { "ARC-a": "IV", "ARC-b": "IV", "ARC-c": "IV", "ARC-d": "VI" },

  // GRC = 6
  6: { "ARC-a": "V", "ARC-b": "V", "ARC-c": "V", "ARC-d": "VI" },

  // GRC = 7
  7: { "ARC-a": "VI", "ARC-b": "VI", "ARC-c": "VI", "ARC-d": "VI" },
};

/**
 * Get SAIL from Final GRC and Residual ARC
 * 
 * Source: JAR_doc_25 Main Body, Table 7, Page 47
 * 
 * @param finalGRC - Final Ground Risk Class (after all mitigations)
 * @param residualARC - Residual Air Risk Class (after strategic mitigations)
 * @returns SAIL level (I-VI) or "Category C" if GRC > 7
 * 
 * @example
 * getSAIL(4, "ARC-c") // Returns: "IV"
 * getSAIL(8, "ARC-a") // Returns: "Category C"
 * getSAIL(2, "ARC-d") // Returns: "VI"
 */
export function getSAIL(finalGRC: FinalGRC, residualARC: ResidualARC): SAIL {
  // GRC > 7 requires Category C (Certified) operation
  // Source: Table 7 last row, JAR_doc_25 page 47
  if (finalGRC > 7) {
    return "Category C";
  }

  // GRC ≤ 0 defaults to minimum SAIL (treat as GRC=0)
  if (finalGRC < 0) {
    finalGRC = 0;
  }

  // GRC ≤ 2 uses row 2
  if (finalGRC <= 2) {
    return SAIL_MATRIX[2][residualARC];
  }

  // GRC 3-7 uses exact row
  return SAIL_MATRIX[finalGRC][residualARC];
}

/**
 * SAIL Descriptions
 * Source: JAR_doc_25 Main Body, Section 4.7.4, Page 47
 * 
 * SAIL represents the level of confidence that the operation will remain in control.
 * Each SAIL corresponds to:
 * - Level of OSO robustness (Table 14)
 * - Description of activities for compliance
 * - Evidence that objectives have been satisfied
 */
export const SAIL_DESCRIPTIONS: Record<SAIL, string> = {
  "I": "SAIL I: Lowest assurance level - Minimal OSO robustness requirements",
  "II": "SAIL II: Low assurance level - Basic OSO robustness requirements",
  "III": "SAIL III: Medium-low assurance level - Moderate OSO robustness requirements",
  "IV": "SAIL IV: Medium assurance level - Substantial OSO robustness requirements",
  "V": "SAIL V: Medium-high assurance level - High OSO robustness requirements",
  "VI": "SAIL VI: Highest assurance level - Maximum OSO robustness requirements",
  "Category C": "Category C: Certified operation required (outside SORA scope)",
};

/**
 * Get all possible SAIL values for a given Final GRC
 * 
 * Useful for UI dropdowns showing possible outcomes
 * 
 * @param finalGRC - Final Ground Risk Class
 * @returns Array of possible SAIL values
 * 
 * @example
 * getPossibleSAILs(4) // Returns: ["III", "IV", "VI"]
 * getPossibleSAILs(8) // Returns: ["Category C"]
 */
export function getPossibleSAILs(finalGRC: FinalGRC): SAIL[] {
  if (finalGRC > 7) {
    return ["Category C"];
  }

  if (finalGRC < 0) finalGRC = 0;
  if (finalGRC <= 2) finalGRC = 2;

  const row = SAIL_MATRIX[finalGRC];
  if (!row) return [];

  // Get unique SAIL values from this GRC row
  const sails = Object.values(row);
  return Array.from(new Set(sails));
}

/**
 * Get minimum and maximum possible SAIL for a given Final GRC
 * 
 * @param finalGRC - Final Ground Risk Class
 * @returns Object with min and max SAIL
 * 
 * @example
 * getSAILRange(4) // Returns: { min: "III", max: "VI" }
 * getSAILRange(2) // Returns: { min: "I", max: "VI" }
 */
export function getSAILRange(finalGRC: FinalGRC): { min: SAIL; max: SAIL } {
  if (finalGRC > 7) {
    return { min: "Category C", max: "Category C" };
  }

  if (finalGRC < 0) finalGRC = 0;
  if (finalGRC <= 2) finalGRC = 2;

  const row = SAIL_MATRIX[finalGRC];
  if (!row) {
    return { min: "I", max: "VI" };
  }

  // SAIL order: I < II < III < IV < V < VI
  const sailOrder: SAIL[] = ["I", "II", "III", "IV", "V", "VI"];
  const sails = Object.values(row);

  const sailIndices = sails.map((s) => sailOrder.indexOf(s as SAIL));
  const minIndex = Math.min(...sailIndices);
  const maxIndex = Math.max(...sailIndices);

  return {
    min: sailOrder[minIndex],
    max: sailOrder[maxIndex],
  };
}

/**
 * Validate SAIL Matrix lookup inputs
 * 
 * @param finalGRC - Final Ground Risk Class
 * @param residualARC - Residual Air Risk Class
 * @returns Validation result with errors if any
 */
export function validateSAILInputs(
  finalGRC: FinalGRC,
  residualARC: ResidualARC
): { valid: boolean; errors: string[] } {
  const errors: string[] = [];

  // Validate GRC
  if (typeof finalGRC !== "number" || isNaN(finalGRC)) {
    errors.push("Final GRC must be a valid number");
  }

  // Validate ARC
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
 * Get SAIL with validation and detailed result
 * 
 * @param finalGRC - Final Ground Risk Class
 * @param residualARC - Residual Air Risk Class
 * @returns Detailed SAIL result with validation
 */
export function getSAILWithValidation(
  finalGRC: FinalGRC,
  residualARC: ResidualARC
): {
  sail: SAIL | null;
  valid: boolean;
  errors: string[];
  description: string;
  reference: string;
} {
  const validation = validateSAILInputs(finalGRC, residualARC);

  if (!validation.valid) {
    return {
      sail: null,
      valid: false,
      errors: validation.errors,
      description: "",
      reference: "",
    };
  }

  const sail = getSAIL(finalGRC, residualARC);

  return {
    sail,
    valid: true,
    errors: [],
    description: SAIL_DESCRIPTIONS[sail],
    reference: "JAR-DEL-SRM-SORA-MB-2.5, Table 7, Page 47",
  };
}
