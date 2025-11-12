/**
 * SORA 2.5 Ground Risk Class (GRC) Calculator
 * 
 * Implements SORA 2.5 GRC calculation following JAR-DEL-SRM-SORA-MB-2.5 specification.
 * 
 * Source: JAR-DEL-SRM-SORA-MB-2.5, Main Body, Pages 34-39
 * - Table 2 (iGRC matrix): Page 34, Lines 925-944
 * - Table 5 (Mitigations): Page 38, Lines 1104-1107
 * - Sequential application rule: Lines 1101, 1123
 * - Column-minimum floor rule: Lines 1133-1135
 * - Small-UA rule: Line 945
 * 
 * @module calculators/grc25
 */

/**
 * Population density categories for iGRC determination.
 * Source: JAR_doc_25, Table 2, Page 34
 */
export type PopulationDensity =
  | "CONTROLLED"      // Controlled Ground Area
  | "SPARSE"          // < 5 people/km²
  | "LOW"             // < 50 people/km²
  | "MEDIUM"          // < 500 people/km²
  | "HIGH"            // < 5,000 people/km²
  | "VERY_HIGH"       // < 50,000 people/km²
  | "EXTREMELY_HIGH"; // > 50,000 people/km²

/**
 * UA dimension/speed categories (columns in Table 2).
 * Source: JAR_doc_25, Table 2, Page 34, Lines 926-930
 */
export type UACategory =
  | "TINY"    // ≤1m, ≤25 m/s (approx. 3 ft)
  | "SMALL"   // ≤3m, ≤35 m/s (approx. 10 ft)
  | "MEDIUM"  // ≤8m, ≤75 m/s (approx. 25 ft)
  | "LARGE"   // ≤20m, ≤120 m/s (approx. 65 ft)
  | "XLARGE"; // ≤40m, ≤200 m/s (approx. 130 ft)

/**
 * M1(A) Sheltering mitigation levels.
 * Source: JAR_doc_25, Table 5, Page 38, Line 1104
 * Source: JAR_doc_27 (Annex B), Table 11, Page 15
 */
export type M1A_Level = "None" | "Low" | "Medium";

/**
 * M1(B) Operational Restrictions mitigation levels.
 * Source: JAR_doc_25, Table 5, Page 38, Line 1105
 * Source: JAR_doc_27 (Annex B), Table 11, Page 15
 */
export type M1B_Level = "None" | "Medium" | "High";

/**
 * M1(C) Ground Observation mitigation levels.
 * Source: JAR_doc_25, Table 5, Page 38, Line 1106
 * Source: JAR_doc_27 (Annex B), Table 11, Page 15
 */
export type M1C_Level = "None" | "Low";

/**
 * M2 Impact Dynamics mitigation levels.
 * Source: JAR_doc_25, Table 5, Page 38, Line 1107
 * Source: JAR_doc_27 (Annex B), Table 11, Page 15
 */
export type M2_Level = "None" | "Medium" | "High";

/**
 * Input parameters for SORA 2.5 GRC calculation.
 */
export interface GRC25Input {
  /** Population density category */
  populationDensity: PopulationDensity;
  
  /** UA dimension/speed category */
  uaCategory: UACategory;
  
  /** UA maximum dimension in meters (for validation) */
  maxDimension: number;
  
  /** UA maximum speed in m/s (for validation) */
  maxSpeed: number;
  
  /** UA weight in grams (for Small-UA Rule check) */
  weight: number;
  
  /** M1(A) Sheltering mitigation level */
  m1a: M1A_Level;
  
  /** M1(B) Operational Restrictions mitigation level */
  m1b: M1B_Level;
  
  /** M1(C) Ground Observation mitigation level */
  m1c: M1C_Level;
  
  /** M2 Impact Dynamics mitigation level */
  m2: M2_Level;
}

/**
 * GRC calculation result with detailed breakdown.
 */
export interface GRC25Result {
  /** Initial GRC (from Table 2) */
  iGRC: number;
  
  /** Final GRC (after mitigations) */
  fGRC: number;
  
  /** Small-UA Rule applied (≤250g AND ≤25m/s → iGRC=1) */
  smallUARuleApplied: boolean;
  
  /** M1(A) credit applied */
  m1aCredit: number;
  
  /** M1(B) credit applied */
  m1bCredit: number;
  
  /** M1(C) credit applied */
  m1cCredit: number;
  
  /** M2 credit applied */
  m2Credit: number;
  
  /** Total credits applied */
  totalCredits: number;
  
  /** Column-minimum floor value (from "Controlled Ground Area" row) */
  columnMinimum: number;
  
  /** Whether column-minimum floor was applied */
  columnMinimumApplied: boolean;
  
  /** Calculation breakdown (step-by-step explanation) */
  breakdown: string[];
  
  /** Official source references */
  references: string[];
}

/**
 * Table 2 - Intrinsic Ground Risk Class (iGRC) determination
 * Source: JAR-DEL-SRM-SORA-MB-2.5, Table 2, Page 34, Lines 925-944
 * 
 * Matrix structure: [PopulationDensity][UACategory] → iGRC
 * 
 * Rows (Population Density):
 * - Controlled Ground Area
 * - < 5 people/km²
 * - < 50 people/km²
 * - < 500 people/km²
 * - < 5,000 people/km²
 * - < 50,000 people/km²
 * - > 50,000 people/km²
 * 
 * Columns (UA Category):
 * - 1m / 25 m/s (approx. 3 ft)
 * - 3m / 35 m/s (approx. 10 ft)
 * - 8m / 75 m/s (approx. 25 ft)
 * - 20m / 120 m/s (approx. 65 ft)
 * - 40m / 200 m/s (approx. 130 ft)
 */
const IGRC_MATRIX: Record<PopulationDensity, Record<UACategory, number>> = {
  CONTROLLED: {
    TINY: 1,
    SMALL: 1,
    MEDIUM: 2,
    LARGE: 3,
    XLARGE: 3,
  },
  SPARSE: {
    TINY: 2,
    SMALL: 3,
    MEDIUM: 4,
    LARGE: 5,
    XLARGE: 6,
  },
  LOW: {
    TINY: 3,
    SMALL: 4,
    MEDIUM: 5,
    LARGE: 6,
    XLARGE: 7,
  },
  MEDIUM: {
    TINY: 4,
    SMALL: 5,
    MEDIUM: 6,
    LARGE: 7,
    XLARGE: 8,
  },
  HIGH: {
    TINY: 5,
    SMALL: 6,
    MEDIUM: 7,
    LARGE: 8,
    XLARGE: 9,
  },
  VERY_HIGH: {
    TINY: 6,
    SMALL: 7,
    MEDIUM: 8,
    LARGE: 9,
    XLARGE: 10,
  },
  EXTREMELY_HIGH: {
    TINY: 7,
    SMALL: 8,
    MEDIUM: Number.NaN, // "Not part of SORA" per Table 2
    LARGE: Number.NaN,
    XLARGE: Number.NaN,
  },
};

/**
 * Table 5 - Mitigations for Final GRC determination
 * Source: JAR-DEL-SRM-SORA-MB-2.5, Table 5, Page 38, Lines 1104-1107
 * Source: JAR_doc_27 (Annex B), Table 11, Page 15 (detailed requirements)
 * 
 * Credit values:
 * - M1(A) Sheltering: Low=-1, Medium=-2, High=N/A
 * - M1(B) Operational Restrictions: Low=N/A, Medium=-1, High=-2
 * - M1(C) Ground Observation: Low=-1, Medium=N/A, High=N/A
 * - M2 Impact Dynamics: Low=N/A, Medium=-1, High=-2
 */
const MITIGATION_CREDITS = {
  M1A: {
    None: 0,
    Low: -1,
    Medium: -2,
  },
  M1B: {
    None: 0,
    Medium: -1,
    High: -2,
  },
  M1C: {
    None: 0,
    Low: -1,
  },
  M2: {
    None: 0,
    Medium: -1,
    High: -2,
  },
} as const;

/**
 * Small-UA Rule check.
 * Source: JAR_doc_25, Page 34, Line 945
 * 
 * "A UA weighing less than or equal to 250 g and having a maximum speed 
 * less than or equal to 25 m/s is considered to have an iGRC of 1 
 * regardless of population density."
 * 
 * @param weight - UA weight in grams
 * @param maxSpeed - UA maximum speed in m/s
 * @returns true if Small-UA Rule applies
 */
export function isSmallUA(weight: number, maxSpeed: number): boolean {
  return weight <= 250 && maxSpeed <= 25;
}

/**
 * Get initial GRC from Table 2.
 * Source: JAR_doc_25, Table 2, Page 34
 * 
 * @param populationDensity - Population density category
 * @param uaCategory - UA dimension/speed category
 * @returns Initial GRC value
 * @throws Error if combination is not in SORA scope
 */
export function getInitialGRC(
  populationDensity: PopulationDensity,
  uaCategory: UACategory
): number {
  const iGRC = IGRC_MATRIX[populationDensity]?.[uaCategory];
  
  if (iGRC === undefined) {
    throw new Error(
      `Invalid combination: ${populationDensity} × ${uaCategory}`
    );
  }
  
  if (isNaN(iGRC)) {
    throw new Error(
      `Operation not part of SORA scope: ${populationDensity} × ${uaCategory}. ` +
      `Consider certified category (JAR_doc_25, Table 2, Page 34).`
    );
  }
  
  return iGRC;
}

/**
 * Get column-minimum floor value (Controlled Ground Area iGRC).
 * Source: JAR_doc_25, Page 38-39, Lines 1133-1135
 * 
 * "When applying all the M1 mitigations, the final GRC cannot be reduced 
 * to a value lower than the lowest value in the applicable column in Table 2. 
 * This is because it is not possible to reduce the number of people at risk 
 * below that of a controlled ground area."
 * 
 * @param uaCategory - UA dimension/speed category
 * @returns Column-minimum iGRC value
 */
export function getColumnMinimum(uaCategory: UACategory): number {
  return IGRC_MATRIX.CONTROLLED[uaCategory];
}

/**
 * Calculate SORA 2.5 GRC with full breakdown.
 * 
 * Applies mitigations in sequential order:
 * 1. M1(A) - Sheltering
 * 2. M1(B) - Operational Restrictions
 * 3. M1(C) - Ground Observation
 * 4. M2 - Impact Dynamics
 * 
 * Source: JAR_doc_25, Page 38, Lines 1101, 1123
 * "All mitigations must be applied in numerical sequence"
 * 
 * @param input - GRC calculation input parameters
 * @returns GRC calculation result with breakdown
 */
export function calculateGRC25(input: GRC25Input): GRC25Result {
  const breakdown: string[] = [];
  const references: string[] = [
    "JAR-DEL-SRM-SORA-MB-2.5, Table 2 (iGRC matrix), Page 34",
    "JAR-DEL-SRM-SORA-MB-2.5, Table 5 (Mitigations), Page 38",
    "JAR_doc_27 (Annex B), Table 11 (Mitigation details), Page 15",
  ];
  
  // Step 1: Check Small-UA Rule
  const smallUARuleApplied = isSmallUA(input.weight, input.maxSpeed);
  
  if (smallUARuleApplied) {
    breakdown.push(
      `✓ Small-UA Rule: weight=${input.weight}g ≤250g AND speed=${input.maxSpeed}m/s ≤25m/s → iGRC=1`,
      `  Source: JAR_doc_25, Page 34, Line 945`
    );
    
    return {
      iGRC: 1,
      fGRC: 1,
      smallUARuleApplied: true,
      m1aCredit: 0,
      m1bCredit: 0,
      m1cCredit: 0,
      m2Credit: 0,
      totalCredits: 0,
      columnMinimum: getColumnMinimum(input.uaCategory),
      columnMinimumApplied: false,
      breakdown,
      references,
    };
  }
  
  // Step 2: Get initial GRC from Table 2
  const iGRC = getInitialGRC(input.populationDensity, input.uaCategory);
  breakdown.push(
    `✓ Initial GRC: ${input.populationDensity} × ${input.uaCategory} → iGRC=${iGRC}`,
    `  Source: JAR_doc_25, Table 2, Page 34`
  );
  
  let currentGRC = iGRC;
  
  // Step 3: Apply M1(A) - Sheltering
  const m1aCredit = MITIGATION_CREDITS.M1A[input.m1a];
  if (m1aCredit !== 0) {
    currentGRC += m1aCredit;
    breakdown.push(
      `✓ M1(A) Sheltering: ${input.m1a} → ${m1aCredit} credit`,
      `  ${iGRC} + (${m1aCredit}) = ${currentGRC}`,
      `  Source: JAR_doc_25, Table 5, Line 1104; JAR_doc_27, Table 11`
    );
  } else {
    breakdown.push(`○ M1(A) Sheltering: None (0 credit)`);
  }
  
  // Step 4: Apply M1(B) - Operational Restrictions
  const m1bCredit = MITIGATION_CREDITS.M1B[input.m1b];
  if (m1bCredit !== 0) {
    const beforeM1B = currentGRC;
    currentGRC += m1bCredit;
    breakdown.push(
      `✓ M1(B) Operational Restrictions: ${input.m1b} → ${m1bCredit} credit`,
      `  ${beforeM1B} + (${m1bCredit}) = ${currentGRC}`,
      `  Source: JAR_doc_25, Table 5, Line 1105; JAR_doc_27, Table 11`
    );
  } else {
    breakdown.push(`○ M1(B) Operational Restrictions: None (0 credit)`);
  }
  
  // Step 5: Apply M1(C) - Ground Observation
  const m1cCredit = MITIGATION_CREDITS.M1C[input.m1c];
  if (m1cCredit !== 0) {
    const beforeM1C = currentGRC;
    currentGRC += m1cCredit;
    breakdown.push(
      `✓ M1(C) Ground Observation: ${input.m1c} → ${m1cCredit} credit`,
      `  ${beforeM1C} + (${m1cCredit}) = ${currentGRC}`,
      `  Source: JAR_doc_25, Table 5, Line 1106; JAR_doc_27, Table 11`
    );
  } else {
    breakdown.push(`○ M1(C) Ground Observation: None (0 credit)`);
  }
  
  // Step 6: Apply M2 - Impact Dynamics
  const m2Credit = MITIGATION_CREDITS.M2[input.m2];
  if (m2Credit !== 0) {
    const beforeM2 = currentGRC;
    currentGRC += m2Credit;
    breakdown.push(
      `✓ M2 Impact Dynamics: ${input.m2} → ${m2Credit} credit`,
      `  ${beforeM2} + (${m2Credit}) = ${currentGRC}`,
      `  Source: JAR_doc_25, Table 5, Line 1107; JAR_doc_27, Table 11`
    );
  } else {
    breakdown.push(`○ M2 Impact Dynamics: None (0 credit)`);
  }
  
  const totalCredits = m1aCredit + m1bCredit + m1cCredit + m2Credit;
  
  // Step 7: Apply column-minimum floor
  const columnMinimum = getColumnMinimum(input.uaCategory);
  let columnMinimumApplied = false;
  
  if (currentGRC < columnMinimum) {
    breakdown.push(
      `⚠ Column-Minimum Floor: fGRC=${currentGRC} < ${columnMinimum} (Controlled Ground Area)`,
      `  Applying floor: fGRC = ${columnMinimum}`,
      `  Source: JAR_doc_25, Page 38-39, Lines 1133-1135`
    );
    currentGRC = columnMinimum;
    columnMinimumApplied = true;
  }
  
  // Step 8: Apply absolute floor (fGRC ≥ 1)
  if (currentGRC < 1) {
    breakdown.push(
      `⚠ Absolute Floor: fGRC=${currentGRC} < 1`,
      `  Applying floor: fGRC = 1`,
      `  Source: JAR_doc_25, SORA 2.5 specification`
    );
    currentGRC = 1;
  }
  
  breakdown.push(`\n✅ Final GRC: ${currentGRC}`);
  
  return {
    iGRC,
    fGRC: currentGRC,
    smallUARuleApplied: false,
    m1aCredit,
    m1bCredit,
    m1cCredit,
    m2Credit,
    totalCredits,
    columnMinimum,
    columnMinimumApplied,
    breakdown,
    references,
  };
}

/**
 * Helper: Get UA category from dimensions and speed.
 * Source: JAR_doc_25, Table 2, Page 34
 * 
 * @param maxDimension - Maximum UA characteristic dimension in meters
 * @param maxSpeed - Maximum UA speed in m/s
 * @returns UA category
 */
export function getUACategory(maxDimension: number, maxSpeed: number): UACategory {
  if (maxDimension <= 1 && maxSpeed <= 25) return "TINY";
  if (maxDimension <= 3 && maxSpeed <= 35) return "SMALL";
  if (maxDimension <= 8 && maxSpeed <= 75) return "MEDIUM";
  if (maxDimension <= 20 && maxSpeed <= 120) return "LARGE";
  if (maxDimension <= 40 && maxSpeed <= 200) return "XLARGE";
  
  throw new Error(
    `UA exceeds SORA 2.5 scope: dimension=${maxDimension}m, speed=${maxSpeed}m/s. ` +
    `Max: 40m @ 200m/s. Consider certified category or Annex F guidance (JAR_doc_25, Page 34).`
  );
}

/**
 * Helper: Validate mitigation combination.
 * Source: JAR_doc_27 (Annex B), Page 8, Footnote
 * 
 * CRITICAL: M1(A) Medium CANNOT combine with M1(B)
 * "The medium level of M1(A) sheltering may not be combined with 
 * operational restrictions M1(B)"
 * 
 * @param m1a - M1(A) level
 * @param m1b - M1(B) level
 * @throws Error if combination is invalid
 */
export function validateMitigationCombination(m1a: M1A_Level, m1b: M1B_Level): void {
  if (m1a === "Medium" && m1b !== "None") {
    throw new Error(
      `Invalid mitigation combination: M1(A)=Medium cannot combine with M1(B)=${m1b}. ` +
      `Source: JAR_doc_27 (Annex B), Page 8, Footnote.`
    );
  }
}

/**
 * Calculate GRC 2.5 with validation.
 * 
 * @param input - GRC calculation input
 * @returns GRC calculation result
 * @throws Error if inputs are invalid or combination not allowed
 */
export function calculateGRC25WithValidation(input: GRC25Input): GRC25Result {
  // Validate mitigation combination
  validateMitigationCombination(input.m1a, input.m1b);
  
  // Perform calculation
  return calculateGRC25(input);
}
