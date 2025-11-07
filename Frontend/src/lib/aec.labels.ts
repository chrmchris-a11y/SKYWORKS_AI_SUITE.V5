// aec.labels.ts
// AEC (Airspace Encounter Category) Labels from JARUS SORA Annex C Table C.1
// ✅ ref: JARUS SORA 2.5 Annex C Table C.1 (Page 12)
// Source: KnowledgeBase/PERMANENT_MEMORY/AIR_RISK_ARC_TMPR_REFERENCE.md

import type { AEC } from './skyworks.enums';

/**
 * AEC Labels - Official descriptions from Annex C Table C.1
 * 
 * Each AEC (1-12) maps to an operational environment with:
 * - Density Rating (1-5): 1 = Very low, 5 = Very high
 * - Initial ARC: a/b/c/d based on encounter risk
 * 
 * ⚠️ NOTE: "Mode-C Veil" in AEC 2/7 is US/FAA terminology.
 * EU equivalent: TMZ (Transponder Mandatory Zone) or RMZ (Radio Mandatory Zone)
 */
export const AEC_LABELS: Record<AEC, string> = {
  AEC_1: "Airport/Heliport in Class B/C/D → ARC-d",
  AEC_2: ">500ft in TMZ/Mode-C Veil → ARC-d",
  AEC_3: ">500ft Controlled airspace → ARC-d",
  AEC_4: ">500ft Uncontrolled Urban → ARC-c",
  AEC_5: ">500ft Uncontrolled Rural → ARC-c",
  AEC_6: "Airport/Heliport in Class E/F/G → ARC-c",
  AEC_7: "<500ft in TMZ/Mode-C Veil → ARC-c",
  AEC_8: "<500ft Controlled airspace → ARC-c",
  AEC_9: "<500ft Uncontrolled Urban → ARC-c",
  AEC_10: "<500ft Uncontrolled Rural → ARC-b",
  AEC_11: ">FL600 (above 18,000m) → ARC-b",
  AEC_12: "Atypical/Segregated airspace → ARC-a",
};

/**
 * AEC Density Ratings (1-5)
 * ✅ ref: Annex C Table C.1, Column "Density Rating"
 */
export const AEC_DENSITY: Record<AEC, number> = {
  AEC_1: 5,   // Very high (Class B/C/D airport)
  AEC_2: 5,   // Very high (TMZ/Mode-C >500ft)
  AEC_3: 5,   // Very high (Controlled >500ft)
  AEC_4: 3,   // Medium (Uncontrolled Urban >500ft)
  AEC_5: 2,   // Low-Medium (Uncontrolled Rural >500ft)
  AEC_6: 3,   // Medium (Class E/F/G airport)
  AEC_7: 3,   // Medium (TMZ/Mode-C <500ft)
  AEC_8: 3,   // Medium (Controlled <500ft)
  AEC_9: 2,   // Low-Medium (Uncontrolled Urban <500ft)
  AEC_10: 1,  // Very low (Uncontrolled Rural <500ft)
  AEC_11: 1,  // Very low (Above FL600)
  AEC_12: 1,  // Very low (Atypical/Segregated)
};

/**
 * Initial ARC mapping from AEC
 * ✅ ref: Annex C Table C.1, Column "Initial ARC"
 */
export const AEC_TO_INITIAL_ARC: Record<AEC, "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d"> = {
  AEC_1: "ARC-d",   // Airport Class B/C/D
  AEC_2: "ARC-d",   // >500ft TMZ/Mode-C
  AEC_3: "ARC-d",   // >500ft Controlled
  AEC_4: "ARC-c",   // >500ft Uncontrolled Urban
  AEC_5: "ARC-c",   // >500ft Uncontrolled Rural
  AEC_6: "ARC-c",   // Airport Class E/F/G
  AEC_7: "ARC-c",   // <500ft TMZ/Mode-C
  AEC_8: "ARC-c",   // <500ft Controlled
  AEC_9: "ARC-c",   // <500ft Uncontrolled Urban
  AEC_10: "ARC-b",  // <500ft Uncontrolled Rural
  AEC_11: "ARC-b",  // >FL600
  AEC_12: "ARC-a",  // Atypical/Segregated
};

/**
 * Helper: Get full AEC description with density and ARC
 */
export function getAECDescription(aec: AEC): {
  label: string;
  density: number;
  initialARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
} {
  return {
    label: AEC_LABELS[aec],
    density: AEC_DENSITY[aec],
    initialARC: AEC_TO_INITIAL_ARC[aec],
  };
}

/**
 * Helper: Get AEC from operational parameters (auto-detection)
 * ✅ ref: Backend implementation - GRCCalculationService.cs DetermineInitialARC_V2_0
 */
export function detectAECFromParams(params: {
  maxHeightAGL: number;           // in meters
  airspaceClass: string;          // A-G
  airspaceControl: string;        // Controlled/Uncontrolled
  locationType: string;           // Airport/NonAirport
  environment: string;            // Urban/Rural/Suburban
  isTMZ: boolean;                 // In TMZ/RMZ zone
  isAtypicalSegregated: boolean;  // Segregated airspace
}): AEC {
  const { maxHeightAGL, airspaceClass, airspaceControl, locationType, environment, isTMZ, isAtypicalSegregated } = params;

  // AEC 12: Atypical/Segregated (highest priority)
  if (isAtypicalSegregated) return "AEC_12";

  // AEC 11: Above FL600 (>18,000m AMSL)
  if (maxHeightAGL > 18000) return "AEC_11";

  const isAbove500ft = maxHeightAGL > 152.4; // 500ft = 152.4m
  const isControlled = airspaceControl === "Controlled";
  const isAirport = locationType === "Airport";
  const isUrban = environment === "Urban" || environment === "Suburban";

  // Airport operations
  if (isAirport) {
    if (["B", "C", "D"].includes(airspaceClass)) {
      return "AEC_1";  // Airport Class B/C/D
    } else {
      return "AEC_6";  // Airport Class E/F/G
    }
  }

  // Above 500ft
  if (isAbove500ft) {
    if (isTMZ) return "AEC_2";                    // >500ft TMZ
    if (isControlled) return "AEC_3";              // >500ft Controlled
    if (isUrban) return "AEC_4";                   // >500ft Uncontrolled Urban
    return "AEC_5";                                 // >500ft Uncontrolled Rural
  }

  // Below 500ft
  if (isTMZ) return "AEC_7";                       // <500ft TMZ
  if (isControlled) return "AEC_8";                 // <500ft Controlled
  if (isUrban) return "AEC_9";                      // <500ft Uncontrolled Urban
  return "AEC_10";                                  // <500ft Uncontrolled Rural (default)
}
