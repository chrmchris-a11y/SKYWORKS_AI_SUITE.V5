/**
 * SORA 2.0 Ground Risk Class (GRC) Calculator
 * 
 * Implements SORA 2.0 GRC calculation following JAR_doc_06 (JARUS SORA v2.0) specification.
 * 
 * CRITICAL DIFFERENCES from SORA 2.5:
 * - Table 2: 8 operational scenarios (not 6 density rows)
 * - Table 3: M1/M2/M3 structure (not M1A/M1B/M1C/M2)
 * - M1 has column-minimum clamp ENABLED (lines 685-687)
 * - M3 ERP: None=+1 penalty, Adequate=0, Validated=-1
 * - 4 UA categories (not 5): 1m, 3m, 8m, >8m
 * 
 * Source: JAR_doc_06 (SORA 2.0 Main Body), Pages 20-22
 * - Table 2 (iGRC scenarios): Page 20, Lines 605-620
 * - Table 3 (Mitigations): Page 21, Lines 676-684
 * - Column-minimum clamp: Page 21, Lines 685-687
 * - Example calculation: Page 21-22, Lines 690-699
 * 
 * @module calculators/grc20
 */

/**
 * Operational scenarios for SORA 2.0 iGRC determination.
 * Source: JAR_doc_06, Table 2, Page 20, Lines 609-619
 */
export type OperationalScenario20 =
  | "VLOS_CONTROLLED"           // VLOS/BVLOS over controlled ground area
  | "VLOS_SPARSE"               // VLOS in sparsely populated environment
  | "BVLOS_SPARSE"              // BVLOS in sparsely populated environment
  | "VLOS_POPULATED"            // VLOS in populated environment
  | "BVLOS_POPULATED"           // BVLOS in populated environment
  | "VLOS_GATHERING"            // VLOS over gathering of people
  | "BVLOS_GATHERING";          // BVLOS over gathering of people

/**
 * UA dimension categories (columns in SORA 2.0 Table 2).
 * Source: JAR_doc_06, Table 2, Page 20, Lines 606-608
 * 
 * Note: SORA 2.0 has only 4 categories (vs 5 in SORA 2.5)
 */
export type UACategory20 =
  | "TINY"    // ≤1m (approx. 3 ft), <700 J kinetic energy
  | "SMALL"   // ≤3m (approx. 10 ft), <34 kJ
  | "MEDIUM"  // ≤8m (approx. 25 ft), <1084 kJ
  | "LARGE";  // >8m (approx. 25 ft), >1084 kJ

/**
 * M1 Strategic Mitigations robustness levels.
 * Source: JAR_doc_06, Table 3, Page 21, Line 677
 * 
 * CRITICAL: SORA 2.0 M1 has 4 levels (None/Low/Medium/High)
 * Credits: None=0, Low=-1, Medium=-2, High=-4
 */
export type M1_Level = "None" | "Low" | "Medium" | "High";

/**
 * M2 Impact Reduction robustness levels.
 * Source: JAR_doc_06, Table 3, Page 21, Line 679
 */
export type M2_Level_20 = "None" | "Medium" | "High";

/**
 * M3 Emergency Response Plan (ERP) robustness levels.
 * Source: JAR_doc_06, Table 3, Page 21, Line 681
 * 
 * CRITICAL: M3 has PENALTY for None (+1 GRC)
 * Credits: None=+1, Adequate=0, Validated=-1
 */
export type M3_Level = "None" | "Adequate" | "Validated";

/**
 * Input parameters for SORA 2.0 GRC calculation.
 */
export interface GRC20Input {
  /** Operational scenario category */
  scenario: OperationalScenario20;
  
  /** UA dimension/energy category */
  uaCategory: UACategory20;
  
  /** UA maximum dimension in meters (for validation) */
  maxDimension: number;
  
  /** M1 Strategic Mitigations robustness level */
  m1: M1_Level;
  
  /** M2 Impact Reduction robustness level */
  m2: M2_Level_20;
  
  /** M3 Emergency Response Plan (ERP) robustness level */
  m3: M3_Level;
}

/**
 * GRC calculation result with detailed breakdown.
 */
export interface GRC20Result {
  /** Initial GRC (from Table 2) */
  iGRC: number;
  
  /** Final GRC (after mitigations) */
  fGRC: number;
  
  /** M1 credit applied */
  m1Credit: number;
  
  /** M2 credit applied */
  m2Credit: number;
  
  /** M3 credit applied (can be PENALTY +1) */
  m3Credit: number;
  
  /** Total credits applied */
  totalCredits: number;
  
  /** Column-minimum floor value (from "Controlled Ground Area" row) */
  columnMinimum: number;
  
  /** Whether M1 column-minimum clamp was applied */
  columnMinimumApplied: boolean;
  
  /** Calculation breakdown (step-by-step explanation) */
  breakdown: string[];
  
  /** Official source references */
  references: string[];
}

/**
 * Table 2 – Intrinsic Ground Risk Classes (GRC) Determination
 * Source: JAR_doc_06 (SORA 2.0), Table 2, Page 20, Lines 605-620
 * 
 * Matrix structure: [OperationalScenario][UACategory] → iGRC
 * 
 * Rows (Operational Scenarios):
 * - VLOS/BVLOS over controlled ground area
 * - VLOS in sparsely populated environment
 * - BVLOS in sparsely populated environment
 * - VLOS in populated environment
 * - BVLOS in populated environment
 * - VLOS over gathering of people
 * - BVLOS over gathering of people
 * 
 * Columns (UA Categories):
 * - 1m / 3ft / <700 J
 * - 3m / 10ft / <34 kJ
 * - 8m / 25ft / <1084 kJ
 * - >8m / 25ft / >1084 kJ
 * 
 * Note: "Grey cells" (undefined combinations) throw error per line 643-644
 */
const IGRC_MATRIX_20: Record<OperationalScenario20, Record<UACategory20, number>> = {
  VLOS_CONTROLLED: {
    TINY: 1,
    SMALL: 2,
    MEDIUM: 3,
    LARGE: 4,
  },
  VLOS_SPARSE: {
    TINY: 2,
    SMALL: 3,
    MEDIUM: 4,
    LARGE: 5,
  },
  BVLOS_SPARSE: {
    TINY: 3,
    SMALL: 4,
    MEDIUM: 5,
    LARGE: 6,
  },
  VLOS_POPULATED: {
    TINY: 4,
    SMALL: 5,
    MEDIUM: 6,
    LARGE: 8,
  },
  BVLOS_POPULATED: {
    TINY: 5,
    SMALL: 6,
    MEDIUM: 8,
    LARGE: 10,
  },
  VLOS_GATHERING: {
    TINY: 7,
    SMALL: Number.NaN, // Grey cell - not supported
    MEDIUM: Number.NaN,
    LARGE: Number.NaN,
  },
  BVLOS_GATHERING: {
    TINY: 8,
    SMALL: Number.NaN, // Grey cell - not supported
    MEDIUM: Number.NaN,
    LARGE: Number.NaN,
  },
};

/**
 * Table 3 – Mitigations for Final GRC determination
 * Source: JAR_doc_06 (SORA 2.0), Table 3, Page 21, Lines 676-684
 * 
 * CRITICAL DIFFERENCES from SORA 2.5:
 * - M1 has 4 levels: None=0, Low=-1, Medium=-2, High=-4
 * - M2 has 3 levels: None=0, Medium=-1, High=-2
 * - M3 has PENALTY: None=+1, Adequate=0, Validated=-1
 * 
 * Sequential application: M1 → M2 → M3 (line 674-675)
 * M1 column-minimum clamp ENABLED (lines 685-687)
 */
const MITIGATION_CREDITS_20 = {
  M1: {
    None: 0,
    Low: -1,
    Medium: -2,
    High: -4,
  },
  M2: {
    None: 0,
    Medium: -1,
    High: -2,
  },
  M3: {
    None: 1,       // PENALTY: +1 GRC if no ERP
    Adequate: 0,
    Validated: -1,
  },
} as const;

/**
 * Get initial GRC from Table 2.
 * Source: JAR_doc_06, Table 2, Page 20
 * 
 * @param scenario - Operational scenario category
 * @param uaCategory - UA dimension/energy category
 * @returns Initial GRC value
 * @throws Error if combination is not supported (grey cell)
 */
export function getInitialGRC20(
  scenario: OperationalScenario20,
  uaCategory: UACategory20
): number {
  const iGRC = IGRC_MATRIX_20[scenario]?.[uaCategory];
  
  if (iGRC === undefined) {
    throw new Error(
      `Invalid combination: ${scenario} × ${uaCategory}`
    );
  }
  
  if (isNaN(iGRC)) {
    throw new Error(
      `Operation not supported by SORA 2.0: ${scenario} × ${uaCategory}. ` +
      `Source: JAR_doc_06, Table 2, Page 20, Lines 643-644 ("grey cells on the table are not currently supported").`
    );
  }
  
  return iGRC;
}

/**
 * Get column-minimum floor value (Controlled Ground Area iGRC).
 * Source: JAR_doc_06, Page 21, Lines 685-687
 * 
 * "When applying mitigation M1, the GRC cannot be reduced to a value 
 * lower than the lowest value in the applicable column in Table 2. 
 * This is because it is not possible to reduce the number of people 
 * at risk below that of a controlled area."
 * 
 * CRITICAL: Column-minimum clamp applies ONLY to M1 in SORA 2.0
 * 
 * @param uaCategory - UA dimension/energy category
 * @returns Column-minimum iGRC value
 */
export function getColumnMinimum20(uaCategory: UACategory20): number {
  return IGRC_MATRIX_20.VLOS_CONTROLLED[uaCategory];
}

/**
 * Calculate SORA 2.0 GRC with full breakdown.
 * 
 * Applies mitigations in sequential order:
 * 1. M1 - Strategic Mitigations (with column-minimum clamp)
 * 2. M2 - Impact Reduction
 * 3. M3 - Emergency Response Plan (ERP)
 * 
 * Source: JAR_doc_06, Page 21, Lines 674-675
 * "All mitigations must be applied in numeric sequence to perform the assessment"
 * 
 * CRITICAL: M1 column-minimum clamp ENABLED (lines 685-687)
 * CRITICAL: M3 None = +1 PENALTY (Table 3, line 681)
 * 
 * @param input - GRC calculation input parameters
 * @returns GRC calculation result with breakdown
 */
export function calculateGRC20(input: GRC20Input): GRC20Result {
  const breakdown: string[] = [];
  const references: string[] = [
    "JAR_doc_06 (SORA 2.0), Table 2 (iGRC scenarios), Page 20",
    "JAR_doc_06 (SORA 2.0), Table 3 (Mitigations), Page 21",
    "JAR_doc_06 (SORA 2.0), Column-minimum clamp, Page 21, Lines 685-687",
  ];
  
  // Step 1: Get initial GRC from Table 2
  const iGRC = getInitialGRC20(input.scenario, input.uaCategory);
  breakdown.push(
    `✓ Initial GRC: ${input.scenario} × ${input.uaCategory} → iGRC=${iGRC}`,
    `  Source: JAR_doc_06, Table 2, Page 20`
  );
  
  let currentGRC = iGRC;
  const columnMinimum = getColumnMinimum20(input.uaCategory);
  
  // Step 2: Apply M1 - Strategic Mitigations (with column-minimum clamp)
  const m1Credit = MITIGATION_CREDITS_20.M1[input.m1];
  let columnMinimumApplied = false;
  
  if (m1Credit !== 0) {
    const beforeM1 = currentGRC;
    const afterM1 = currentGRC + m1Credit;
    
    // Apply column-minimum clamp for M1 ONLY
    if (afterM1 < columnMinimum) {
      currentGRC = columnMinimum;
      columnMinimumApplied = true;
      breakdown.push(
        `✓ M1 Strategic Mitigations: ${input.m1} → ${m1Credit} credit`,
        `  ${beforeM1} + (${m1Credit}) = ${afterM1}`,
        `  ⚠ Column-Minimum Clamp: ${afterM1} < ${columnMinimum} (Controlled Ground Area)`,
        `  Applying M1 clamp: GRC = ${columnMinimum}`,
        `  Source: JAR_doc_06, Page 21, Lines 685-687`
      );
    } else {
      currentGRC = afterM1;
      breakdown.push(
        `✓ M1 Strategic Mitigations: ${input.m1} → ${m1Credit} credit`,
        `  ${beforeM1} + (${m1Credit}) = ${currentGRC}`,
        `  Source: JAR_doc_06, Table 3, Line 677`
      );
    }
  } else {
    breakdown.push(`○ M1 Strategic Mitigations: None (0 credit)`);
  }
  
  // Step 3: Apply M2 - Impact Reduction (NO column-minimum clamp)
  const m2Credit = MITIGATION_CREDITS_20.M2[input.m2];
  if (m2Credit !== 0) {
    const beforeM2 = currentGRC;
    currentGRC += m2Credit;
    breakdown.push(
      `✓ M2 Impact Reduction: ${input.m2} → ${m2Credit} credit`,
      `  ${beforeM2} + (${m2Credit}) = ${currentGRC}`,
      `  Source: JAR_doc_06, Table 3, Line 679`
    );
  } else {
    breakdown.push(`○ M2 Impact Reduction: None (0 credit)`);
  }
  
  // Step 4: Apply M3 - Emergency Response Plan (ERP) (NO column-minimum clamp)
  const m3Credit = MITIGATION_CREDITS_20.M3[input.m3];
  if (m3Credit !== 0) {
    const beforeM3 = currentGRC;
    currentGRC += m3Credit;
    
    if (m3Credit > 0) {
      breakdown.push(
        `⚠ M3 Emergency Response Plan: ${input.m3} → +${m3Credit} PENALTY`,
        `  ${beforeM3} + ${m3Credit} = ${currentGRC}`,
        `  Note: No ERP results in +1 GRC penalty`,
        `  Source: JAR_doc_06, Table 3, Line 681`
      );
    } else {
      breakdown.push(
        `✓ M3 Emergency Response Plan: ${input.m3} → ${m3Credit} credit`,
        `  ${beforeM3} + (${m3Credit}) = ${currentGRC}`,
        `  Source: JAR_doc_06, Table 3, Line 681`
      );
    }
  } else {
    breakdown.push(`○ M3 Emergency Response Plan: Adequate (0 credit)`);
  }
  
  const totalCredits = m1Credit + m2Credit + m3Credit;
  
  // Step 5: Apply absolute floor (fGRC ≥ 1)
  if (currentGRC < 1) {
    breakdown.push(
      `⚠ Absolute Floor: fGRC=${currentGRC} < 1`,
      `  Applying floor: fGRC = 1`,
      `  Source: JAR_doc_06, SORA 2.0 specification`
    );
    currentGRC = 1;
  }
  
  // Step 6: Check maximum GRC limit
  if (currentGRC > 7) {
    breakdown.push(
      `❌ Final GRC > 7: Operation not supported by SORA 2.0`,
      `  Source: JAR_doc_06, Page 22, Line 700`
    );
  }
  
  breakdown.push(`\n✅ Final GRC: ${currentGRC}`);
  
  // Add example reference if relevant
  if (input.m1 === "Medium" && columnMinimumApplied) {
    breakdown.push(
      `\n📖 Example: JAR_doc_06, Page 21-22, Lines 690-699`,
      `   "2.5m UAS in VLOS sparse: iGRC=3, M1 Medium (-2) → GRC=2 (clamped),`,
      `   M2 High (-1) → GRC=1, M3 Adequate (0) → fGRC=1"`
    );
  }
  
  return {
    iGRC,
    fGRC: currentGRC,
    m1Credit,
    m2Credit,
    m3Credit,
    totalCredits,
    columnMinimum,
    columnMinimumApplied,
    breakdown,
    references,
  };
}

/**
 * Helper: Get UA category from dimension.
 * Source: JAR_doc_06, Table 2, Page 20
 * 
 * Note: SORA 2.0 uses kinetic energy thresholds, but dimension is primary indicator
 * 
 * @param maxDimension - Maximum UA characteristic dimension in meters
 * @returns UA category
 */
export function getUACategory20(maxDimension: number): UACategory20 {
  if (maxDimension <= 1) return "TINY";
  if (maxDimension <= 3) return "SMALL";
  if (maxDimension <= 8) return "MEDIUM";
  return "LARGE"; // >8m
}

/**
 * Helper: Determine operational scenario from environment and line-of-sight.
 * Source: JAR_doc_06, Table 2, Page 20
 * 
 * @param isVLOS - Whether operation is VLOS (false = BVLOS/EVLOS)
 * @param environment - Environment type
 * @returns Operational scenario
 */
export function getOperationalScenario20(
  isVLOS: boolean,
  environment: "controlled" | "sparse" | "populated" | "gathering"
): OperationalScenario20 {
  if (environment === "controlled") {
    return "VLOS_CONTROLLED"; // Note: VLOS/BVLOS same GRC for controlled
  }
  
  if (environment === "gathering") {
    return isVLOS ? "VLOS_GATHERING" : "BVLOS_GATHERING";
  }
  
  if (environment === "sparse") {
    return isVLOS ? "VLOS_SPARSE" : "BVLOS_SPARSE";
  }
  
  // populated
  return isVLOS ? "VLOS_POPULATED" : "BVLOS_POPULATED";
}

/**
 * Calculate GRC 2.0 with validation.
 * 
 * @param input - GRC calculation input
 * @returns GRC calculation result
 * @throws Error if inputs are invalid or operation not supported
 */
export function calculateGRC20WithValidation(input: GRC20Input): GRC20Result {
  // Validate scenario/category combination
  const result = calculateGRC20(input);
  
  // Check if final GRC exceeds SORA 2.0 limit
  if (result.fGRC > 7) {
    throw new Error(
      `Final GRC (${result.fGRC}) exceeds SORA 2.0 maximum (7). ` +
      `Operation not supported by SORA 2.0 process. ` +
      `Source: JAR_doc_06, Page 22, Line 700.`
    );
  }
  
  return result;
}
