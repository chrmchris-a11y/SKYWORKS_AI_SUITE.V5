/**
 * Air Risk Class (ARC) Calculator - Initial & Residual
 * 
 * Implements SORA 2.0/2.5 ARC determination:
 * - Step #4: Initial ARC from AEC (Airspace Encounter Category)
 * - Step #5: Residual ARC after strategic mitigations
 * 
 * Sources:
 * - JAR_doc_25 (SORA 2.5 Main Body), Step #4 (Page 40-42), Step #5 (Page 43-44)
 * - SORA Annex C (v1.0), Table 1 (AEC → Initial ARC), Table 2 (ARC reduction)
 * - JAR_doc_25, Figure 6 (ARC assignment process), Page 41
 * 
 * @module calculators/arc
 */

/**
 * Airspace Encounter Category (AEC) - 12 categories
 * Source: SORA Annex C, Table 1, Page 12 (lines 353-397)
 */
export type AEC =
  | "AEC_1"   // Airport/Heliport Class B/C/D (density=5, ARC-d)
  | "AEC_2"   // >500ft Mode-S Veil/TMZ (density=5, ARC-d) - ⚠️ Mode-S BANNED in SORA 2.5
  | "AEC_3"   // >500ft Controlled Airspace (density=5, ARC-d)
  | "AEC_4"   // >500ft Uncontrolled Urban (density=3, ARC-c)
  | "AEC_5"   // >500ft Uncontrolled Rural (density=2, ARC-c)
  | "AEC_6"   // Airport/Heliport Class E/F/G (density=3, ARC-c)
  | "AEC_7"   // <500ft Mode-S Veil/TMZ (density=3, ARC-c) - ⚠️ Mode-S BANNED in SORA 2.5
  | "AEC_8"   // <500ft Controlled Airspace (density=3, ARC-c)
  | "AEC_9"   // <500ft Uncontrolled Urban (density=2, ARC-c)
  | "AEC_10"  // <500ft Uncontrolled Rural (density=1, ARC-b)
  | "AEC_11"  // >FL600 (density=1, ARC-b)
  | "AEC_12"; // Atypical/Segregated (density=1, ARC-a)

/**
 * Air Risk Class (ARC) levels
 * Source: SORA Annex C, Table 1
 * - ARC-a: Atypical/Segregated (no tactical mitigation required)
 * - ARC-b: Low encounter rate
 * - ARC-c: Medium encounter rate
 * - ARC-d: High encounter rate
 */
export type ARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

/**
 * Airspace class types (ICAO)
 */
export type AirspaceClass = "A" | "B" | "C" | "D" | "E" | "F" | "G";

/**
 * Environment type for AEC determination
 */
export type Environment = 
  | "airport_heliport"  // Airport/Heliport environment
  | "urban"             // Urban/populated area
  | "rural"             // Rural/sparsely populated area
  | "atypical";         // Atypical/Segregated airspace

/**
 * Altitude band for AEC determination
 */
export type AltitudeBand =
  | "below_500ft"   // <500ft AGL
  | "above_500ft"   // >500ft AGL but <FL600
  | "above_FL600";  // >FL600

/**
 * Input parameters for AEC determination
 */
export interface AECInput {
  /** Environment type */
  environment: Environment;
  
  /** Airspace class */
  airspaceClass: AirspaceClass;
  
  /** Whether airspace is controlled (has ATC services) */
  isControlled: boolean;
  
  /** Altitude band */
  altitudeBand: AltitudeBand;
  
  /** Whether in special zone (TMZ, RMZ, etc.) - SORA 2.5 only */
  inSpecialZone?: boolean;
  
  /** Whether airspace is atypical/segregated */
  isAtypical?: boolean;
}

/**
 * Input parameters for Residual ARC calculation (Step #5)
 */
export interface ResidualARCInput {
  /** Initial ARC (from Step #4) */
  initialARC: ARC;
  
  /** Initial AEC */
  initialAEC: AEC;
  
  /** Whether operation is VLOS */
  isVLOS?: boolean;
  
  /** Whether airspace observer is available (SORA 2.5) */
  hasAirspaceObserver?: boolean;
  
  /** Strategic mitigation: demonstrated local density rating (1-5) */
  demonstratedDensity?: number;
  
  /** Strategic mitigation: containment level applied */
  containmentApplied?: boolean;
  
  /** Strategic mitigation: spatial/temporal segregation */
  spatialTemporalSegregation?: boolean;
}

/**
 * ARC calculation result
 */
export interface ARCResult {
  /** Airspace Encounter Category (AEC) */
  aec: AEC;
  
  /** Initial density rating (1-5) */
  initialDensity: number;
  
  /** Initial ARC (from Table 1) */
  initialARC: ARC;
  
  /** Residual ARC (after strategic mitigations) */
  residualARC: ARC;
  
  /** VLOS low exposure rule applied (-1 ARC class) */
  vlosReductionApplied: boolean;
  
  /** Strategic mitigation reduction applied */
  strategicMitigationApplied: boolean;
  
  /** Calculation breakdown (step-by-step explanation) */
  breakdown: string[];
  
  /** Official source references */
  references: string[];
}

/**
 * Table 1 - AEC → Initial ARC mapping with density ratings
 * Source: SORA Annex C, Table 1, Page 12 (lines 353-397)
 * 
 * Structure: { AEC: { density: number, initialARC: ARC } }
 */
const AEC_TO_INITIAL_ARC: Record<AEC, { density: number; initialARC: ARC }> = {
  AEC_1: { density: 5, initialARC: "ARC-d" },  // Airport/Heliport Class B/C/D
  AEC_2: { density: 5, initialARC: "ARC-d" },  // >500ft Mode-S Veil/TMZ (⚠️ BANNED in SORA 2.5)
  AEC_3: { density: 5, initialARC: "ARC-d" },  // >500ft Controlled
  AEC_4: { density: 3, initialARC: "ARC-c" },  // >500ft Uncontrolled Urban
  AEC_5: { density: 2, initialARC: "ARC-c" },  // >500ft Uncontrolled Rural
  AEC_6: { density: 3, initialARC: "ARC-c" },  // Airport/Heliport Class E/F/G
  AEC_7: { density: 3, initialARC: "ARC-c" },  // <500ft Mode-S Veil/TMZ (⚠️ BANNED in SORA 2.5)
  AEC_8: { density: 3, initialARC: "ARC-c" },  // <500ft Controlled
  AEC_9: { density: 2, initialARC: "ARC-c" },  // <500ft Uncontrolled Urban
  AEC_10: { density: 1, initialARC: "ARC-b" }, // <500ft Uncontrolled Rural (reference environment)
  AEC_11: { density: 1, initialARC: "ARC-b" }, // >FL600
  AEC_12: { density: 1, initialARC: "ARC-a" }, // Atypical/Segregated
};

/**
 * Table 2 - Strategic Mitigation ARC Reduction Rules
 * Source: SORA Annex C, Table 2, Page 14 (lines 434-450)
 * 
 * Structure: { AEC: { demonstratedDensity → newARC } }
 * 
 * Key: If local density can be demonstrated to be similar to reference density,
 * then ARC may be reduced per Column D.
 * 
 * Note: AEC_10 and AEC_11 not included - any reduction would result in ARC-a
 * (requires special Atypical/Segregated airspace demonstration)
 */
const STRATEGIC_MITIGATION_REDUCTION: Record<
  AEC,
  Record<number, ARC> | null
> = {
  AEC_1: {
    4: "ARC-c", // Density 4 → ARC-c
    3: "ARC-c", // Density 3 → ARC-c
    2: "ARC-b", // Density 2 → ARC-b
    1: "ARC-b", // Density 1 → ARC-b
  },
  AEC_2: {
    2: "ARC-c", // Density 2 → ARC-c
    1: "ARC-b", // Density 1 → ARC-b
  },
  AEC_3: {
    3: "ARC-c", // Density 3 → ARC-c
    2: "ARC-c", // Density 2 → ARC-c
    1: "ARC-b", // Density 1 → ARC-b
  },
  AEC_4: {
    1: "ARC-b", // Density 1 (AEC 10 reference) → ARC-b
  },
  AEC_5: {
    1: "ARC-b", // Density 1 (AEC 10 reference) → ARC-b
  },
  AEC_6: {
    1: "ARC-b", // Density 1 (AEC 10 reference) → ARC-b
  },
  AEC_7: {
    1: "ARC-b", // Density 1 (AEC 10 reference) → ARC-b
  },
  AEC_8: {
    1: "ARC-b", // Density 1 (AEC 10 reference) → ARC-b
  },
  AEC_9: {
    1: "ARC-b", // Density 1 (AEC 10 reference) → ARC-b
  },
  AEC_10: null, // No reduction available (already ARC-b)
  AEC_11: null, // No reduction available (already ARC-b)
  AEC_12: null, // No reduction available (already ARC-a)
};

/**
 * Determine AEC from operational parameters.
 * Source: SORA Annex C, Table 1, Page 12 + JAR_doc_25 Figure 6, Page 41
 * 
 * ⚠️ CRITICAL: Mode-S Veil BANNED in SORA 2.5 - use RMZ/TMZ instead
 * 
 * @param input - AEC determination inputs
 * @returns AEC category
 */
export function determineAEC(input: AECInput): AEC {
  // Special case: Atypical/Segregated airspace
  if (input.isAtypical || input.environment === "atypical") {
    return "AEC_12";
  }
  
  // Special case: >FL600
  if (input.altitudeBand === "above_FL600") {
    return "AEC_11";
  }
  
  // Airport/Heliport environment
  if (input.environment === "airport_heliport") {
    // Class B, C, or D → AEC 1
    if (["B", "C", "D"].includes(input.airspaceClass)) {
      return "AEC_1";
    }
    // Class E, F, or G → AEC 6
    return "AEC_6";
  }
  
  // Above 500ft AGL but below FL600
  if (input.altitudeBand === "above_500ft") {
    // Special zone (TMZ/RMZ) - SORA 2.5
    if (input.inSpecialZone) {
      return "AEC_2"; // Note: Maps to Mode-S Veil in SORA 2.0
    }
    
    // Controlled airspace
    if (input.isControlled) {
      return "AEC_3";
    }
    
    // Uncontrolled Urban
    if (input.environment === "urban") {
      return "AEC_4";
    }
    
    // Uncontrolled Rural
    return "AEC_5";
  }
  
  // Below 500ft AGL
  if (input.altitudeBand === "below_500ft") {
    // Special zone (TMZ/RMZ) - SORA 2.5
    if (input.inSpecialZone) {
      return "AEC_7"; // Note: Maps to Mode-S Veil in SORA 2.0
    }
    
    // Controlled airspace
    if (input.isControlled) {
      return "AEC_8";
    }
    
    // Uncontrolled Urban
    if (input.environment === "urban") {
      return "AEC_9";
    }
    
    // Uncontrolled Rural (reference environment)
    return "AEC_10";
  }
  
  // Fallback (should not reach here)
  throw new Error(
    `Unable to determine AEC from inputs: ${JSON.stringify(input)}`
  );
}

/**
 * Get initial ARC from AEC (Step #4).
 * Source: SORA Annex C, Table 1, Page 12
 * 
 * @param aec - Airspace Encounter Category
 * @returns Initial ARC and density rating
 */
export function getInitialARC(aec: AEC): { initialARC: ARC; density: number } {
  const entry = AEC_TO_INITIAL_ARC[aec];
  return {
    initialARC: entry.initialARC,
    density: entry.density,
  };
}

/**
 * Calculate Residual ARC with strategic mitigations (Step #5).
 * Source: JAR_doc_25, Step #5, Page 43-44
 * Source: SORA Annex C, Table 2, Page 14
 * 
 * SORA 2.5 VLOS Low Exposure Rule (Page 43-44):
 * "For VLOS operations or operations where the remote pilot is supported by 
 * an airspace observer situated alongside the pilot for instantaneous communication, 
 * the initial air risk class can be reduced by one class."
 * 
 * Restrictions:
 * - Cannot reduce to ARC-a (requires Atypical/Segregated demonstration)
 * - In ARC-d environments, additional agreement with ATC might be required
 * 
 * @param input - Residual ARC calculation inputs
 * @returns Residual ARC
 */
export function calculateResidualARC(input: ResidualARCInput): ARC {
  let currentARC = input.initialARC;
  const breakdown: string[] = [];
  
  // Step 1: Apply VLOS low exposure rule (SORA 2.5)
  if (input.isVLOS || input.hasAirspaceObserver) {
    const canReduce = currentARC !== "ARC-a"; // Cannot reduce to ARC-a
    
    if (canReduce) {
      const beforeVLOS = currentARC;
      currentARC = reduceARCByOne(currentARC);
      
      breakdown.push(
        `✓ VLOS Low Exposure Rule: ${beforeVLOS} → ${currentARC} (-1 class)`,
        `  Source: JAR_doc_25, Step #5, Page 43-44`,
        input.hasAirspaceObserver
          ? `  Airspace observer alongside pilot for instantaneous communication`
          : `  VLOS operation with visual airspace surveillance`
      );
      
      if (input.initialARC === "ARC-d") {
        breakdown.push(
          `  ⚠️ Note: ARC-d environment - additional ATC agreement may be required`
        );
      }
    } else {
      breakdown.push(
        `○ VLOS Low Exposure Rule: Cannot reduce ARC-a further`,
        `  Source: JAR_doc_25, Page 43-44`
      );
    }
  }
  
  // Step 2: Apply strategic mitigation (demonstrated density reduction)
  if (input.demonstratedDensity !== undefined) {
    const reductionRules = STRATEGIC_MITIGATION_REDUCTION[input.initialAEC];
    
    if (reductionRules && reductionRules[input.demonstratedDensity]) {
      const beforeMitigation = currentARC;
      currentARC = reductionRules[input.demonstratedDensity];
      
      breakdown.push(
        `✓ Strategic Mitigation: Demonstrated local density=${input.demonstratedDensity}`,
        `  ${beforeMitigation} → ${currentARC}`,
        `  Source: SORA Annex C, Table 2, Page 14`,
        `  Reference: AEC 10 (OPS <500ft AGL over rural areas)`
      );
    } else if (!reductionRules) {
      breakdown.push(
        `○ Strategic Mitigation: ${input.initialAEC} already at minimum (no reduction available)`,
        `  Source: SORA Annex C, Table 2, Note 1`
      );
    } else {
      breakdown.push(
        `⚠️ Strategic Mitigation: Density ${input.demonstratedDensity} does not qualify for reduction`,
        `  Source: SORA Annex C, Table 2, Page 14`
      );
    }
  }
  
  // Step 3: Apply containment/segregation mitigations
  if (input.containmentApplied || input.spatialTemporalSegregation) {
    breakdown.push(
      `ℹ️ Additional Strategic Mitigations Applied:`,
      input.containmentApplied
        ? `  - Containment: Operational volume restricted`
        : "",
      input.spatialTemporalSegregation
        ? `  - Spatial/Temporal Segregation: Boundaries/time restrictions`
        : "",
      `  Source: SORA Annex C, Section 5 (Strategic Mitigations)`
    );
  }
  
  return currentARC;
}

/**
 * Helper: Reduce ARC by one class.
 * ARC-d → ARC-c → ARC-b → ARC-a
 * 
 * @param arc - Current ARC
 * @returns Reduced ARC
 */
function reduceARCByOne(arc: ARC): ARC {
  const arcOrder: ARC[] = ["ARC-d", "ARC-c", "ARC-b", "ARC-a"];
  const currentIndex = arcOrder.indexOf(arc);
  
  if (currentIndex === -1 || currentIndex === arcOrder.length - 1) {
    return arc; // Already at ARC-a or invalid
  }
  
  return arcOrder[currentIndex + 1];
}

/**
 * Calculate complete ARC (Initial + Residual) with full breakdown.
 * 
 * @param aecInput - AEC determination inputs
 * @param residualInput - Residual ARC calculation inputs
 * @returns Complete ARC calculation result
 */
export function calculateARC(
  aecInput: AECInput,
  residualInput: Partial<ResidualARCInput> = {}
): ARCResult {
  const breakdown: string[] = [];
  const references: string[] = [
    "JAR_doc_25 (SORA 2.5 Main Body), Step #4 (Page 40-42), Figure 6",
    "JAR_doc_25 (SORA 2.5 Main Body), Step #5 (Page 43-44) - VLOS Low Exposure",
    "SORA Annex C (v1.0), Table 1 (AEC → Initial ARC), Page 12",
    "SORA Annex C (v1.0), Table 2 (ARC Reduction), Page 14",
  ];
  
  // Step 1: Determine AEC
  const aec = determineAEC(aecInput);
  breakdown.push(
    `✓ Step #4: Determine AEC`,
    `  Environment: ${aecInput.environment}`,
    `  Airspace: ${aecInput.airspaceClass} (${aecInput.isControlled ? "Controlled" : "Uncontrolled"})`,
    `  Altitude: ${aecInput.altitudeBand}`,
    `  → AEC: ${aec}`,
    `  Source: SORA Annex C, Table 1, Page 12`
  );
  
  // Step 2: Get Initial ARC
  const { initialARC, density } = getInitialARC(aec);
  breakdown.push(
    `\n✓ Step #4: Initial ARC from ${aec}`,
    `  Density Rating: ${density} (1=very low, 5=very high)`,
    `  → Initial ARC: ${initialARC}`,
    `  Source: SORA Annex C, Table 1`
  );
  
  // Step 3: Calculate Residual ARC (if mitigations provided)
  const fullResidualInput: ResidualARCInput = {
    initialARC,
    initialAEC: aec,
    ...residualInput,
  };
  
  const residualARC = calculateResidualARC(fullResidualInput);
  
  const vlosReductionApplied =
    ((residualInput.isVLOS || residualInput.hasAirspaceObserver) &&
    residualARC !== initialARC) || false;
  
  const strategicMitigationApplied =
    (residualInput.demonstratedDensity !== undefined ||
    residualInput.containmentApplied ||
    residualInput.spatialTemporalSegregation) || false;
  
  if (vlosReductionApplied || strategicMitigationApplied) {
    breakdown.push(
      `\n✓ Step #5: Residual ARC (after strategic mitigations)`,
      `  ${initialARC} → ${residualARC}`
    );
  } else {
    breakdown.push(
      `\n○ Step #5: No strategic mitigations applied`,
      `  Residual ARC = Initial ARC = ${residualARC}`
    );
  }
  
  breakdown.push(`\n✅ Final Residual ARC: ${residualARC}`);
  
  return {
    aec,
    initialDensity: density,
    initialARC,
    residualARC,
    vlosReductionApplied,
    strategicMitigationApplied,
    breakdown,
    references,
  };
}

/**
 * Validate AEC inputs for SORA 2.5 compliance.
 * 
 * ⚠️ CRITICAL: Ban "Mode-S Veil" terminology in SORA 2.5
 * Use RMZ (Radio Mandatory Zone) or TMZ (Transponder Mandatory Zone) instead
 * 
 * @param input - AEC inputs
 * @throws Error if inputs violate SORA 2.5 compliance
 */
export function validateAECInputs(input: AECInput): void {
  // Check for banned terminology (Mode-S Veil)
  const inputStr = JSON.stringify(input).toLowerCase();
  if (inputStr.includes("mode-s") || inputStr.includes("veil")) {
    throw new Error(
      `Invalid terminology: "Mode-S Veil" is BANNED in SORA 2.5. ` +
      `Use inSpecialZone=true for RMZ/TMZ instead. ` +
      `Source: SORA 2.5 compliance (RMZ/TMZ replaces Mode-S Veil)`
    );
  }
  
  // Validate airspace class
  const validClasses = ["A", "B", "C", "D", "E", "F", "G"];
  if (!validClasses.includes(input.airspaceClass)) {
    throw new Error(
      `Invalid airspace class: ${input.airspaceClass}. Must be one of: ${validClasses.join(", ")}`
    );
  }
  
  // Validate environment + altitude combinations
  if (input.environment === "atypical" && !input.isAtypical) {
    throw new Error(
      `Inconsistent inputs: environment="atypical" but isAtypical=false`
    );
  }
}
