// sail.matrix.ts
// SAIL Determination Matrix: Final GRC × Residual ARC → SAIL (I-VI)
// ✅ ref: JARUS SORA 2.0 Table 5 (Page 27) + SORA 2.5 Table 7 (Page 47)
// ✅ NOTE: SAIL matrix is IDENTICAL in SORA 2.0 and SORA 2.5
// Source: KnowledgeBase/PERMANENT_MEMORY/SORA_2_0_TABLES_REFERENCE.md

export type SAIL = "I" | "II" | "III" | "IV" | "V" | "VI";
export type ARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

/**
 * SAIL Determination Matrix
 * 
 * ✅ ref: JARUS SORA Table 5 (v2.0) / Table 7 (v2.5)
 * 
 * Formula: SAIL = f(Final GRC, Residual ARC)
 * 
 * Key Rules:
 * - ARC-d ALWAYS results in SAIL VI (highest risk)
 * - GRC ≤2 with ARC-a = SAIL I (lowest risk)
 * - GRC >7 = Out of SORA scope → Certified category (requires Part 21/EASA cert)
 * 
 * Matrix Layout:
 * 
 * | Final GRC | ARC-a | ARC-b | ARC-c | ARC-d |
 * |-----------|-------|-------|-------|-------|
 * | ≤2        | I     | II    | IV    | VI    |
 * | 3         | II    | II    | IV    | VI    |
 * | 4         | III   | III   | IV    | VI    |
 * | 5         | IV    | IV    | IV    | VI    |
 * | 6         | V     | V     | V     | VI    |
 * | 7         | VI    | VI    | VI    | VI    |
 * | >7        | Out of SORA scope (Certified) |
 */
export const SAIL_MATRIX: Record<number, Record<ARC, SAIL>> = {
  1: {
    "ARC-a": "I",
    "ARC-b": "II",
    "ARC-c": "IV",
    "ARC-d": "VI",
  },
  2: {
    "ARC-a": "I",
    "ARC-b": "II",
    "ARC-c": "IV",
    "ARC-d": "VI",
  },
  3: {
    "ARC-a": "II",
    "ARC-b": "II",
    "ARC-c": "IV",
    "ARC-d": "VI",
  },
  4: {
    "ARC-a": "III",
    "ARC-b": "III",
    "ARC-c": "IV",
    "ARC-d": "VI",
  },
  5: {
    "ARC-a": "IV",
    "ARC-b": "IV",
    "ARC-c": "IV",
    "ARC-d": "VI",
  },
  6: {
    "ARC-a": "V",
    "ARC-b": "V",
    "ARC-c": "V",
    "ARC-d": "VI",
  },
  7: {
    "ARC-a": "VI",
    "ARC-b": "VI",
    "ARC-c": "VI",
    "ARC-d": "VI",
  },
};

/**
 * Get SAIL from Final GRC and Residual ARC
 * 
 * @param finalGRC - Final Ground Risk Class (1-7, or >7 for out-of-scope)
 * @param residualARC - Residual Air Risk Class (a/b/c/d)
 * @returns SAIL level (I-VI) or null if out of scope
 */
export function getSAIL(finalGRC: number, residualARC: ARC): SAIL | null {
  // Validate inputs
  if (finalGRC < 1) {
    throw new Error(`Invalid Final GRC: ${finalGRC} (must be ≥1)`);
  }

  if (finalGRC > 7) {
    // Out of SORA scope - requires Certified category
    return null;
  }

  // GRC 1-2 use same row
  const grcKey = finalGRC <= 2 ? 1 : finalGRC;

  return SAIL_MATRIX[grcKey][residualARC];
}

/**
 * Validate if operation is within SORA scope
 * 
 * @param finalGRC - Final Ground Risk Class
 * @returns true if within SORA scope (GRC 1-7), false if Certified required
 */
export function isWithinSORAScope(finalGRC: number): boolean {
  return finalGRC >= 1 && finalGRC <= 7;
}

/**
 * Get SAIL with validation and error handling
 * 
 * @param finalGRC - Final Ground Risk Class
 * @param residualARC - Residual Air Risk Class
 * @returns Object with SAIL, validation status, and messages
 */
export function determineSAIL(finalGRC: number, residualARC: ARC): {
  sail: SAIL | null;
  isValid: boolean;
  message: string;
  requiresCertified: boolean;
} {
  // Check if GRC is valid
  if (finalGRC < 1) {
    return {
      sail: null,
      isValid: false,
      message: `Invalid Final GRC: ${finalGRC}. GRC must be ≥1 after mitigations.`,
      requiresCertified: false,
    };
  }

  // Check if out of SORA scope
  if (finalGRC > 7) {
    return {
      sail: null,
      isValid: false,
      message: `Final GRC ${finalGRC} exceeds SORA scope (max 7). Operation requires Certified category (EASA Part 21).`,
      requiresCertified: true,
    };
  }

  // Get SAIL
  const sail = getSAIL(finalGRC, residualARC);

  return {
    sail,
    isValid: true,
    message: `SAIL ${sail} determined from Final GRC ${finalGRC} × Residual ${residualARC}`,
    requiresCertified: false,
  };
}

/**
 * Get description of SAIL level
 */
export function getSAILDescription(sail: SAIL): string {
  const descriptions: Record<SAIL, string> = {
    I: "SAIL I - Lowest risk: Minimal OSO requirements (operator competency, basic procedures)",
    II: "SAIL II - Low risk: Low-Medium OSO robustness (training, maintenance, inspections)",
    III: "SAIL III - Medium-Low risk: Medium OSO robustness (design standards, procedures validation)",
    IV: "SAIL IV - Medium risk: Medium-High OSO robustness (reliability, C3 link, human error mitigation)",
    V: "SAIL V - High risk: High OSO robustness (all OSOs at high level, extensive evidence)",
    VI: "SAIL VI - Highest risk: Maximum OSO requirements (all OSOs at high level, comprehensive safety case)",
  };

  return descriptions[sail];
}
