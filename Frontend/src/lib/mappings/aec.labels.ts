/**
 * AEC (Airspace Encounter Category) Labels & Mapping
 * 
 * ✅ 100% EASA/JARUS Compliant - SORA 2.0 Annex C Table 1
 * 
 * Source: JAR-DEL-WG6-D.04 Annex C, Edition 1.0, 30.01.2019
 * Reference: EXTRACTED_SORA-Annex-C-v1.0.txt, Lines 340-420 (Table 1, Page 12)
 * 
 * ⚠️ CRITICAL NOTES:
 * - SORA 2.0 uses "Mode-S Veil" terminology (AEC 2, AEC 7)
 * - ❌ BAN "Mode-S Veil" in SORA 2.5 implementations - use "RMZ/TMZ" instead
 * - Density ratings: 5 (very high) → 1 (very low)
 * - ARC mapping: AEC → Initial ARC (a, b, c, d)
 */

export type AEC = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12;

export type InitialARC = "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";

export type DensityRating = 1 | 2 | 3 | 5; // No density 4 in SORA

/**
 * AEC Definition Interface
 */
export interface AECDefinition {
  aec: AEC;
  label: string;
  description: string;
  densityRating: DensityRating;
  initialARC: InitialARC;
  preconditions: {
    airportEnvironment?: boolean;      // Is operation in airport/heliport environment?
    airspaceClass?: string[];           // ICAO Airspace Classes (A-G)
    controlled?: boolean;               // Controlled vs Uncontrolled airspace
    altitudeBand?: string;              // e.g., ">500ft AGL", "<500ft AGL", ">FL600"
    environment?: "Urban" | "Rural";    // Urban vs Rural area
    specialZone?: string[];             // Mode-S Veil, TMZ, etc.
    atypical?: boolean;                 // Atypical/Segregated airspace
  };
  soraVersion: "2.0" | "2.5" | "both";  // Applicable SORA version
  notes?: string;                       // Additional compliance notes
}

/**
 * Complete AEC Mapping Table
 * 
 * Source: SORA 2.0 Annex C, Table 1, Page 12
 * 
 * 12 Airspace Encounter Categories with:
 * - Density ratings (1-5)
 * - Initial ARC mapping (a, b, c, d)
 * - Operational preconditions
 */
export const AEC_MAPPING: Record<AEC, AECDefinition> = {
  // =============================================================================
  // AIRPORT/HELIPORT ENVIRONMENT
  // =============================================================================
  
  1: {
    aec: 1,
    label: "Airport/Heliport in Class B/C/D",
    description: "Operations in Airport/Heliport Environment in Class B, C or D airspace",
    densityRating: 5,
    initialARC: "ARC-d",
    preconditions: {
      airportEnvironment: true,
      airspaceClass: ["B", "C", "D"],
      controlled: true
    },
    soraVersion: "both"
  },

  6: {
    aec: 6,
    label: "Airport/Heliport in Class E/F/G",
    description: "Operations in Airport/Heliport Environment in Class E airspace or in Class F or G",
    densityRating: 3,
    initialARC: "ARC-c",
    preconditions: {
      airportEnvironment: true,
      airspaceClass: ["E", "F", "G"]
    },
    soraVersion: "both"
  },

  // =============================================================================
  // OPERATIONS > 500ft AGL (but < FL600)
  // =============================================================================

  2: {
    aec: 2,
    label: ">500ft AGL in Mode-S Veil/TMZ",
    description: "Operations >500ft AGL but <FL600 in a Mode-S Veil or Transponder Mandatory Zone (TMZ)",
    densityRating: 5,
    initialARC: "ARC-d",
    preconditions: {
      altitudeBand: ">500ft AGL",
      specialZone: ["Mode-S Veil", "TMZ"]
    },
    soraVersion: "2.0",
    notes: "⚠️ SORA 2.0 ONLY - Use 'RMZ/TMZ' terminology in SORA 2.5"
  },

  3: {
    aec: 3,
    label: ">500ft AGL in Controlled Airspace",
    description: "Operations >500ft AGL but <FL600 in controlled airspace",
    densityRating: 5,
    initialARC: "ARC-d",
    preconditions: {
      altitudeBand: ">500ft AGL",
      controlled: true
    },
    soraVersion: "both"
  },

  4: {
    aec: 4,
    label: ">500ft AGL Uncontrolled Urban",
    description: "Operations >500ft AGL but <FL600 in uncontrolled airspace over Urban Area",
    densityRating: 3,
    initialARC: "ARC-c",
    preconditions: {
      altitudeBand: ">500ft AGL",
      controlled: false,
      environment: "Urban"
    },
    soraVersion: "both"
  },

  5: {
    aec: 5,
    label: ">500ft AGL Uncontrolled Rural",
    description: "Operations >500ft AGL but <FL600 in uncontrolled airspace over Rural Area",
    densityRating: 2,
    initialARC: "ARC-c",
    preconditions: {
      altitudeBand: ">500ft AGL",
      controlled: false,
      environment: "Rural"
    },
    soraVersion: "both"
  },

  // =============================================================================
  // OPERATIONS < 500ft AGL (VLL - Very Low Level)
  // =============================================================================

  7: {
    aec: 7,
    label: "<500ft AGL in Mode-S Veil/TMZ",
    description: "Operations <500ft AGL in a Mode-S Veil or Transponder Mandatory Zone (TMZ)",
    densityRating: 3,
    initialARC: "ARC-c",
    preconditions: {
      altitudeBand: "<500ft AGL",
      specialZone: ["Mode-S Veil", "TMZ"]
    },
    soraVersion: "2.0",
    notes: "⚠️ SORA 2.0 ONLY - Use 'RMZ/TMZ' terminology in SORA 2.5"
  },

  8: {
    aec: 8,
    label: "<500ft AGL in Controlled Airspace",
    description: "Operations <500ft AGL in controlled airspace",
    densityRating: 3,
    initialARC: "ARC-c",
    preconditions: {
      altitudeBand: "<500ft AGL",
      controlled: true
    },
    soraVersion: "both"
  },

  9: {
    aec: 9,
    label: "<500ft AGL Uncontrolled Urban",
    description: "Operations <500ft AGL in uncontrolled airspace over Urban Area",
    densityRating: 2,
    initialARC: "ARC-c",
    preconditions: {
      altitudeBand: "<500ft AGL",
      controlled: false,
      environment: "Urban"
    },
    soraVersion: "both"
  },

  10: {
    aec: 10,
    label: "<500ft AGL Uncontrolled Rural",
    description: "Operations <500ft AGL in uncontrolled airspace over Rural Area",
    densityRating: 1,
    initialARC: "ARC-b",
    preconditions: {
      altitudeBand: "<500ft AGL",
      controlled: false,
      environment: "Rural"
    },
    soraVersion: "both",
    notes: "Reference density scenario for ARC reduction arguments (Annex C, Page 14)"
  },

  // =============================================================================
  // SPECIAL CASES
  // =============================================================================

  11: {
    aec: 11,
    label: ">FL600",
    description: "Operations above Flight Level 600",
    densityRating: 1,
    initialARC: "ARC-b",
    preconditions: {
      altitudeBand: ">FL600"
    },
    soraVersion: "both"
  },

  12: {
    aec: 12,
    label: "Atypical/Segregated Airspace",
    description: "Operations in Atypical/Segregated Airspace",
    densityRating: 1,
    initialARC: "ARC-a",
    preconditions: {
      atypical: true
    },
    soraVersion: "both"
  }
};

/**
 * Get AEC Definition by number
 * 
 * @param aec - AEC number (1-12)
 * @returns AEC definition object
 */
export function getAECDefinition(aec: AEC): AECDefinition {
  return AEC_MAPPING[aec];
}

/**
 * Get AEC Label (short human-readable string)
 * 
 * @param aec - AEC number (1-12)
 * @returns Short label (e.g., "Airport/Heliport in Class B/C/D")
 */
export function getAECLabel(aec: AEC): string {
  return AEC_MAPPING[aec].label;
}

/**
 * Get AEC Description (full operational environment)
 * 
 * @param aec - AEC number (1-12)
 * @returns Full description
 */
export function getAECDescription(aec: AEC): string {
  return AEC_MAPPING[aec].description;
}

/**
 * Get Initial ARC from AEC
 * 
 * Source: SORA 2.0 Annex C, Table 1, Page 12
 * 
 * @param aec - AEC number (1-12)
 * @returns Initial ARC category ("ARC-a", "ARC-b", "ARC-c", "ARC-d")
 */
export function getInitialARCFromAEC(aec: AEC): InitialARC {
  return AEC_MAPPING[aec].initialARC;
}

/**
 * Get Density Rating from AEC
 * 
 * Density scale:
 * - 5: Very high density (Airport B/C/D, Controlled >500ft, Mode-S Veil)
 * - 3: High density (Airport E/F/G, Uncontrolled Urban >500ft, VLL controlled/TMZ)
 * - 2: Medium density (Rural >500ft, Urban <500ft uncontrolled)
 * - 1: Low density (Rural <500ft, >FL600, Atypical)
 * 
 * @param aec - AEC number (1-12)
 * @returns Density rating (1, 2, 3, or 5)
 */
export function getDensityRating(aec: AEC): DensityRating {
  return AEC_MAPPING[aec].densityRating;
}

/**
 * Determine AEC from Operational Parameters
 * 
 * Decision tree logic (SORA 2.0 Annex C):
 * 1. Atypical/Segregated? → AEC 12
 * 2. >FL600? → AEC 11
 * 3. Airport environment?
 *    - Class B/C/D → AEC 1
 *    - Class E/F/G → AEC 6
 * 4. Altitude > 500ft AGL?
 *    - Mode-S Veil/TMZ → AEC 2
 *    - Controlled → AEC 3
 *    - Uncontrolled Urban → AEC 4
 *    - Uncontrolled Rural → AEC 5
 * 5. Altitude < 500ft AGL:
 *    - Mode-S Veil/TMZ → AEC 7
 *    - Controlled → AEC 8
 *    - Uncontrolled Urban → AEC 9
 *    - Uncontrolled Rural → AEC 10
 * 
 * @param params - Operational parameters
 * @returns AEC number (1-12)
 */
export function determineAEC(params: {
  altitude_ft: number;
  airspaceClass?: string;
  controlled: boolean;
  environment: "Urban" | "Rural";
  airportEnvironment: boolean;
  atypical: boolean;
  specialZone?: string;
}): AEC {
  const { altitude_ft, airspaceClass, controlled, environment, airportEnvironment, atypical, specialZone } = params;

  // Atypical/Segregated airspace
  if (atypical) {
    return 12;
  }

  // Above FL600 (60,000 ft)
  if (altitude_ft > 60000) {
    return 11;
  }

  // Airport/Heliport environment
  if (airportEnvironment) {
    if (airspaceClass && ["B", "C", "D"].includes(airspaceClass)) {
      return 1;
    } else {
      return 6; // Class E, F, or G
    }
  }

  // High altitude (>500ft AGL but <FL600)
  if (altitude_ft > 500 && altitude_ft <= 60000) {
    // Mode-S Veil / TMZ
    if (specialZone && (specialZone.includes("Mode-S") || specialZone.includes("TMZ"))) {
      return 2;
    }
    // Controlled airspace
    if (controlled) {
      return 3;
    }
    // Uncontrolled
    if (environment === "Urban") {
      return 4;
    } else {
      return 5; // Rural
    }
  }

  // Low altitude (<500ft AGL) - VLL
  if (altitude_ft <= 500) {
    // Mode-S Veil / TMZ
    if (specialZone && (specialZone.includes("Mode-S") || specialZone.includes("TMZ"))) {
      return 7;
    }
    // Controlled airspace
    if (controlled) {
      return 8;
    }
    // Uncontrolled
    if (environment === "Urban") {
      return 9;
    } else {
      return 10; // Rural
    }
  }

  // Default fallback (should not reach here)
  return 10; // Conservative default: AEC 10 (ARC-b)
}

/**
 * Validate AEC for SORA Version
 * 
 * AEC 2 and AEC 7 use "Mode-S Veil" terminology (SORA 2.0 only)
 * ❌ BAN in SORA 2.5 - use RMZ/TMZ instead
 * 
 * @param aec - AEC number (1-12)
 * @param soraVersion - SORA version ("2.0" or "2.5")
 * @returns Validation result with warnings
 */
export function validateAECForVersion(
  aec: AEC,
  soraVersion: "2.0" | "2.5"
): { valid: boolean; warnings: string[] } {
  const warnings: string[] = [];
  const definition = AEC_MAPPING[aec];

  if (soraVersion === "2.5" && definition.soraVersion === "2.0") {
    warnings.push(
      `⚠️ COMPLIANCE WARNING: ${definition.label} uses "Mode-S Veil" terminology from SORA 2.0. ` +
      `For SORA 2.5, use "RMZ/TMZ" instead. Source: SORA 2.5 Main Body, Section 4.4.4`
    );
    return { valid: false, warnings };
  }

  return { valid: true, warnings };
}

/**
 * Get AEC Preconditions Summary
 * 
 * @param aec - AEC number (1-12)
 * @returns Human-readable preconditions string
 */
export function getAECPreconditions(aec: AEC): string {
  const def = AEC_MAPPING[aec];
  const parts: string[] = [];

  if (def.preconditions.airportEnvironment) {
    parts.push("Airport/Heliport Environment");
  }
  if (def.preconditions.airspaceClass) {
    parts.push(`Class ${def.preconditions.airspaceClass.join("/")}`);
  }
  if (def.preconditions.controlled !== undefined) {
    parts.push(def.preconditions.controlled ? "Controlled" : "Uncontrolled");
  }
  if (def.preconditions.altitudeBand) {
    parts.push(def.preconditions.altitudeBand);
  }
  if (def.preconditions.environment) {
    parts.push(def.preconditions.environment);
  }
  if (def.preconditions.specialZone) {
    parts.push(def.preconditions.specialZone.join("/"));
  }
  if (def.preconditions.atypical) {
    parts.push("Atypical/Segregated");
  }

  return parts.join(", ");
}

/**
 * Export all AECs as array for dropdowns/UI
 */
export const ALL_AECS: AEC[] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

/**
 * Export AEC → ARC mapping for quick lookup
 */
export const AEC_TO_ARC_MAP: Record<AEC, InitialARC> = {
  1: "ARC-d",
  2: "ARC-d",
  3: "ARC-d",
  4: "ARC-c",
  5: "ARC-c",
  6: "ARC-c",
  7: "ARC-c",
  8: "ARC-c",
  9: "ARC-c",
  10: "ARC-b",
  11: "ARC-b",
  12: "ARC-a"
};
