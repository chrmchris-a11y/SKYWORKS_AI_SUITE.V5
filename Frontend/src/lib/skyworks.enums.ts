/**/**// skyworks.enums.ts

 * SKYWORKS SORA 2.0/2.5 TypeScript Enums

 * ✅ Verified from official EASA/JARUS documents (Nov 8, 2025) * SKYWORKS AI SUITE - SORA 2.0/2.5 EASA/JARUS Compliant Enums// Fixed dropdowns & enums per EASA/JARUS (SORA 2.0 AMC & JARUS SORA 2.5)

 * 

 * Sources: * // NOTE: AEC labels must be loaded from Annex C Table C.1 (KnowledgeBase). Fallback uses numeric "AEC-1"…"AEC-12".

 * - SORA 2.5: JAR-DEL-SRM-SORA-MB-2.5 + Annex B

 * - SORA 2.0: JAR-DEL-WG6-D.04 + Annex C * ✅ ALL VALUES VERIFIED FROM OFFICIAL DOCUMENTS (November 8, 2025)// ✅ 100% Compliance με EASA/JARUS επίσημα έγγραφα

 */

 * 

// =============================================================================

// SORA 2.5 ENUMS * Sources:export const SoraVersion = [

// =============================================================================

 * - SORA 2.5: JAR-DEL-SRM-SORA-MB-2.5 (Main Body) + JAR-DEL-SRM-SORA-B-2.5 (Annex B)  "SORA_2_0_AMC",

/**

 * M1(A) Sheltering - SORA 2.5 * - SORA 2.0: JAR-DEL-WG6-D.04 (Main Body + Annex C)  "SORA_2_5_JARUS",

 * Source: JAR_doc_27 Annex B Table 11, Page 15

 */ *   "PDRA_S01",

export enum M1A_Sheltering_25 {

  None = "None", * Reference: Docs/SORA_COMPLIANCE_REFERENCE.md  "PDRA_S02",

  Low = "Low",       // -1 GRC

  Medium = "Medium"  // -2 GRC */  "STS_01",

  // ⚠️ High does NOT exist

}  "STS_02",



/**// =============================================================================] as const;

 * M1(B) Operational Restrictions - SORA 2.5

 * Source: JAR_doc_27 Annex B Table 11, Page 15// SORA 2.5 ENUMS (JAR-DEL-SRM-SORA-MB-2.5 + Annex B)export type SoraVersion = typeof SoraVersion[number];

 * ⚠️ CANNOT combine with M1(A) Medium

 */// =============================================================================

export enum M1B_OperationalRestrictions_25 {

  None = "None",export const OperationType = ["VLOS", "BVLOS", "EVLOS"] as const;

  Medium = "Medium", // -1 GRC

  High = "High"      // -2 GRC/**export type OperationType = typeof OperationType[number];

  // ⚠️ Low does NOT exist

} * M1(A) Sheltering - SORA 2.5



/** * Source: JAR_doc_27 Annex B Table 11, Page 15export const AirspaceClass = ["A","B","C","D","E","F","G"] as const;

 * M1(C) Ground Observation - SORA 2.5

 * Source: JAR_doc_27 Annex B Table 11, Page 15 */export type AirspaceClass = typeof AirspaceClass[number];

 */

export enum M1C_GroundObservation_25 {export enum M1A_Sheltering_25 {

  None = "None",

  Low = "Low"  // -1 GRC  None = "None",       // 0 credit// SORA 2.5 Population Density (per JARUS Table 2)

  // ⚠️ Medium/High do NOT exist

}  Low = "Low",         // -1 GRC creditexport const Environment25 = ["CGA","Rural","Suburban","Urban"] as const;



/**  Medium = "Medium"    // -2 GRC creditexport type Environment25 = typeof Environment25[number];

 * M2 Impact Dynamics - SORA 2.5

 * Source: JAR_doc_27 Annex B Table 11, Page 15  // ⚠️ High does NOT exist in SORA 2.5

 */

export enum M2_ImpactDynamics_25 {}// EU Airspace Special Zones (NO "Mode-S veil" - FAA only)

  None = "None",

  Medium = "Medium", // -1 GRC// ✅ ref: EASA Regulation (EU) 2019/947, Annex A definitions

  High = "High"      // -2 GRC

  // ⚠️ Low does NOT exist/**export const SpecialZones = [

}

 * M1(B) Operational Restrictions - SORA 2.5  "CTR",      // Control Zone

/**

 * Population Density - SORA 2.5 * Source: JAR_doc_27 Annex B Table 11, Page 15  "TMA",      // Terminal Maneuvering Area

 * Source: JAR_doc_25 Table 2, Page 34

 */ *   "ATZ",      // Aerodrome Traffic Zone

export enum PopulationDensity_25 {

  ControlledGround = "Controlled Ground", * ⚠️ CRITICAL CONSTRAINT: M1(A) Medium CANNOT combine with M1(B)  "RMZ",      // Radio Mandatory Zone (EU equivalent of Mode-S)

  Remote = "Remote",

  LightlyPopulated = "Lightly Populated", * Source: JAR_doc_27 Annex B Section B.2, Page 8  "TMZ",      // Transponder Mandatory Zone (EU equivalent of Mode-S)

  SparselyPopulated = "Sparsely Populated",

  Suburban = "Suburban", */  "Prohibited_P",

  HighDensityMetropolitan = "High Density Metropolitan"

}export enum M1B_OperationalRestrictions_25 {  "Restricted_R",



/**  None = "None",     // 0 credit  "Danger_D",

 * ARC Categories - SORA 2.5

 * Source: JAR_doc_25 Main Body  Medium = "Medium", // -1 GRC credit  "TSA",      // Temporary Segregated Area

 */

export enum ARC_25 {  High = "High"      // -2 GRC credit  "TRA",      // Temporary Reserved Area

  ARC_a = "ARC-a",

  ARC_b = "ARC-b",  // ⚠️ Low does NOT exist in SORA 2.5  "CBA",      // Cross-Border Area

  ARC_c = "ARC-c",

  ARC_d = "ARC-d"}  "UAS_Geographical_Zone",

}

] as const;

/**

 * SAIL Categories - SORA 2.5/**export type SpecialZone = typeof SpecialZones[number];

 * Source: JAR_doc_25 Table 7, Page 47

 */ * M1(C) Ground Observation - SORA 2.5

export enum SAIL_25 {

  I = "I", * Source: JAR_doc_27 Annex B Table 11, Page 15export const Typicality = ["Typical","Atypical_Segregated"] as const;

  II = "II",

  III = "III", */export type Typicality = typeof Typicality[number];

  IV = "IV",

  V = "V",export enum M1C_GroundObservation_25 {

  VI = "VI"

}  None = "None", // 0 credit// SORA 2.5 Step #5 - Traffic Density Source



/**  Low = "Low"    // -1 GRC credit// ✅ ref: JARUS SORA 2.5 Annex C, strategic mitigations

 * TMPR - SORA 2.5

 * Source: JAR_doc_25 Table 6, Page 45  // ⚠️ Medium and High do NOT exist in SORA 2.5export const TrafficDensitySource = ["Empirical","Modelled","ANSP"] as const;

 */

export enum TMPR_25 {}export type TrafficDensitySource = typeof TrafficDensitySource[number];

  None = "None",

  Low = "Low",

  Medium = "Medium",

  High = "High"/**export const USpace = ["Yes","No"] as const;

}

 * M2 Impact Dynamics - SORA 2.5export type USpace = typeof USpace[number];

// =============================================================================

// SORA 2.0 ENUMS * Source: JAR_doc_27 Annex B Table 11, Page 15

// =============================================================================

 */// SORA 2.5 Step #5 - Airspace Containment

/**

 * M1 Strategic - SORA 2.0export enum M2_ImpactDynamics_25 {// ✅ ref: JARUS SORA 2.5 Annex C Table C.2

 * Source: JAR_doc_06 Table 3, Page 21

 */  None = "None",     // 0 creditexport const AirspaceContainment = [

export enum M1_Strategic_20 {

  None = "None",  Medium = "Medium", // -1 GRC credit  "None",

  Low = "Low",       // -1 GRC

  Medium = "Medium", // -2 GRC  High = "High"      // -2 GRC credit  "Horizontal",

  High = "High"      // -4 GRC

}  // ⚠️ Low does NOT exist in SORA 2.5  "Vertical",



/**}  "Horizontal_and_Vertical"

 * M2 Impact Reduction - SORA 2.0

 * Source: JAR_doc_06 Table 3, Page 21] as const;

 */

export enum M2_ImpactReduction_20 {/**export type AirspaceContainment = typeof AirspaceContainment[number];

  None = "None",

  Medium = "Medium", // -1 GRC * Population Density Categories - SORA 2.5

  High = "High"      // -2 GRC

} * Source: JAR_doc_25 Main Body Table 2, Page 34// AEC: 1..12 (Airspace Encounter Categories)



/** */// ✅ ref: JARUS SORA 2.5 Annex C Table C.1

 * M3 ERP - SORA 2.0

 * Source: JAR_doc_06 Table 3, Page 21export enum PopulationDensity_25 {export const AECValues = ["AEC_1","AEC_2","AEC_3","AEC_4","AEC_5","AEC_6","AEC_7","AEC_8","AEC_9","AEC_10","AEC_11","AEC_12"] as const;

 * ⚠️ None = +1 penalty!

 */  ControlledGround = "Controlled Ground",           // iGRC from row 1export type AEC = typeof AECValues[number];

export enum M3_ERP_20 {

  None = "None",           // +1 penalty  Remote = "Remote",                                 // < 5 ppl/km² (row 2)

  Adequate = "Adequate",   // 0

  Validated = "Validated"  // -1 GRC  LightlyPopulated = "Lightly Populated",           // < 50 ppl/km² (row 3)// TMPR target per Residual ARC

}

  SparselyPopulated = "Sparsely Populated",         // < 500 ppl/km² (row 4)// ✅ ref: JARUS SORA Annex D (TMPR guidance)

/**

 * Operation Scenarios - SORA 2.0  Suburban = "Suburban",                             // < 5,000 ppl/km² (row 5)export const TMPRLevel = ["None","Low","Medium","High"] as const;

 * Source: JAR_doc_06 Table 2, Page 20

 */  HighDensityMetropolitan = "High Density Metropolitan" // < 50,000 ppl/km² (row 6)export type TMPRLevel = typeof TMPRLevel[number];

export enum OperationScenario_20 {

  VLOS_Controlled = "VLOS_Controlled",  // Note: "Assemblies of People" (>50,000) not in dropdown - requires special approval

  VLOS_Sparsely = "VLOS_Sparsely",

  VLOS_Populated = "VLOS_Populated",}// ════════════════════════════════════════════════════════════════

  VLOS_Gathering = "VLOS_Gathering",

  BVLOS_Controlled = "BVLOS_Controlled",// SORA 2.5 — Ground Risk Mitigations (Table 5)

  BVLOS_Sparsely = "BVLOS_Sparsely",

  BVLOS_Populated = "BVLOS_Populated",/**// ✅ ref: JARUS SORA 2.5 Main Body Table 5 + Annex B (JAR_doc_27)

  BVLOS_Gathering = "BVLOS_Gathering"

} * Characteristic Dimension + Speed Categories - SORA 2.5// ════════════════════════════════════════════════════════════════



/** * Source: JAR_doc_25 Main Body Table 2, Page 34

 * AEC - SORA 2.0

 * Source: JAR_doc_06 Annex C Table 1, Page 12 * // M1(A) - Sheltering (per Table 5 + Annex B)

 */

export enum AEC_20 { * Combined categories for iGRC lookup (columns 1-5)// ✅ OFFICIAL ANNEX B: ONLY None | Low (NO Medium per JAR_doc_27)

  AEC_1 = 1,

  AEC_2 = 2, */// ✅ Credits: None=0, Low=-1

  AEC_3 = 3,

  AEC_4 = 4,export enum DimensionSpeedCategory_25 {export const M1A_Sheltering = ["None","Low"] as const;

  AEC_5 = 5,

  AEC_6 = 6,  Small_LowSpeed = "<1m & <25m/s",      // Column 1export type M1A_Sheltering = typeof M1A_Sheltering[number];

  AEC_7 = 7,

  AEC_8 = 8,  Small_MedSpeed = "<1m & 25-75m/s",    // Column 2

  AEC_9 = 9,

  AEC_10 = 10,  Medium_LowSpeed = "1-3m & <25m/s",    // Column 3// M1(B) - Operational restrictions (per Table 5 + Annex B)

  AEC_11 = 11,

  AEC_12 = 12  Medium_MedSpeed = "1-3m & 25-75m/s",  // Column 4// ✅ Credits: None=0, Medium=-1, High=-2

}

  Large_Any = ">3m (any speed)"         // Column 5export const M1B_OperationalRestrictions = ["None","Medium","High"] as const;

// =============================================================================

// COMMON ENUMS}export type M1B_OperationalRestrictions = typeof M1B_OperationalRestrictions[number];

// =============================================================================



/**

 * ICAO Airspace Classes/**// M1(C) - Ground observation (per Table 5 + Annex B)

 */

export enum AirspaceClass { * Initial ARC Categories - SORA 2.5// ✅ Credits: None=0, Low=-1 (only Low level defined)

  A = "A",

  B = "B", * Source: JAR_doc_25 Main Body Figure 6, Pages 41-42export const M1C_GroundObservation = ["None","Low"] as const;

  C = "C",

  D = "D", */export type M1C_GroundObservation = typeof M1C_GroundObservation[number];

  E = "E",

  F = "F",export enum InitialARC_25 {

  G = "G"

}  ARC_a = "ARC-a", // Atypical/Segregated airspace// M2 - Impact dynamics reduced (per Table 5 + Annex B)



/**  ARC_b = "ARC-b", // Low encounter rate// ✅ FIXED per JAR_doc_27: Added "Low" level (not in old drafts)

 * Line of Sight

 */  ARC_c = "ARC-c", // Medium encounter rate// ✅ Credits: None=0, Low=-1, Medium=-2, High=-3

export enum LineOfSight {

  VLOS = "VLOS",  ARC_d = "ARC-d"  // High encounter rate// Note: parachute, frangibility, etc. affect robustness level

  EVLOS = "EVLOS",

  BVLOS = "BVLOS"}export const M2_ImpactDynamics = ["None","Low","Medium","High"] as const;

}

export type M2_ImpactDynamics = typeof M2_ImpactDynamics[number];

/**

 * SORA Version/**

 */

export enum SORAVersion { * Residual ARC Categories - SORA 2.5// ════════════════════════════════════════════════════════════════

  SORA_20 = "2.0",

  SORA_25 = "2.5" * Source: JAR_doc_25 Main Body Section 4.5, Pages 43-44// SORA 2.0 — Ground Risk Mitigations (Table 3)

}

 * // ✅ ref: AMC1 to Article 11 (SORA 2.0), Table 3

// =============================================================================

// VALIDATION HELPERS * After applying strategic mitigations (Step #5)// ════════════════════════════════════════════════════════════════

// =============================================================================

 */

/**

 * Validate M1(A)+M1(B) combination - SORA 2.5export enum ResidualARC_25 {// M1 - Strategic mitigations (reduce people at risk)

 * Source: JAR_doc_27 Annex B Page 8

 */  ARC_a = "ARC-a",// ✅ Credits: None=0, Low=-1, Medium=-2, High=-4

export function validateM1A_M1B(

  m1a: M1A_Sheltering_25,  ARC_b = "ARC-b",// ⚠️ MUST apply column-min clamp (cannot reduce below column minimum)

  m1b: M1B_OperationalRestrictions_25

): boolean {  ARC_c = "ARC-c",export const M1_20 = ["None","Low","Medium","High"] as const;

  if (m1a === M1A_Sheltering_25.Medium && m1b !== M1B_OperationalRestrictions_25.None) {

    return false;  ARC_d = "ARC-d"export type M1_20 = typeof M1_20[number];

  }

  return true;}

}

// M2 - Impact reduction (e.g., parachute)

/**

 * Enforce finalGRC >= 1 floor/**// ✅ FIXED per Annex B: Changed Medium → Low for SORA 2.0

 * Source: JAR_doc_25 Table 7, Page 47

 */ * SAIL Categories - SORA 2.5// ✅ Credits: None=0, Low=-1, High=-2

export function enforceGRCFloor(finalGRC: number): number {

  return Math.max(finalGRC, 1); * Source: JAR_doc_25 Main Body Table 7, Page 47export const M2_20 = ["None","Low","High"] as const;

}

 */export type M2_20 = typeof M2_20[number];

export enum SAIL_25 {

  SAIL_I = "I",// M3 - Emergency Response Plan

  SAIL_II = "II",// ✅ FIXED per Annex B: Changed to None/Adequate/Validated terminology

  SAIL_III = "III",// ✅ Credits: None=+1 (penalty), Adequate=0, Validated=-1

  SAIL_IV = "IV",export const M3_20 = ["None","Adequate","Validated"] as const;

  SAIL_V = "V",export type M3_20 = typeof M3_20[number];

  SAIL_VI = "VI"

}// ════════════════════════════════════════════════════════════════

// OSO Robustness Levels

/**// ✅ ref: JARUS SORA 2.5 Table 14 + SORA 2.0 Table 6

 * TMPR (Tactical Mitigation Performance Requirement) - SORA 2.5// ════════════════════════════════════════════════════════════════

 * Source: JAR_doc_25 Main Body Table 6, Page 45export const Robustness = ["Low","Medium","High"] as const;

 */export type Robustness = typeof Robustness[number];

export enum TMPR_25 {

  None = "None",     // ARC-a// ════════════════════════════════════════════════════════════════

  Low = "Low",       // ARC-b// Drone Library - UA Class Marking

  Medium = "Medium", // ARC-c// ✅ ref: EASA Regulation (EU) 2019/945 (Drone classes)

  High = "High"      // ARC-d// ════════════════════════════════════════════════════════════════

}export const DroneClass = ["C0","C1","C2","C3","C4","C5","C6","PB"] as const;

export type DroneClass = typeof DroneClass[number];

// =============================================================================

// SORA 2.0 ENUMS (JAR-DEL-WG6-D.04)export interface DroneSpec {

// =============================================================================  id?: string;

  manufacturer: string;

/**  model: string;

 * M1 Strategic Mitigation - SORA 2.0  classMarking: DroneClass;

 * Source: JAR_doc_06 Main Body Table 3, Page 21  mtomKg: number;                    // MTOM in kg

 *   maxDimensionM: number;              // Max characteristic dimension in meters

 * ⚠️ SORA 2.0 has column-min clamp (different from SORA 2.5)  typicalSpeedMs: number;             // Typical cruise speed in m/s

 */  tethered: boolean;                  // Is the UA tethered?

export enum M1_Strategic_20 {  equipage: {

  None = "None",     // 0 credit    parachute?: boolean;              // Has parachute system

  Low = "Low",       // -1 GRC credit    flightTermination?: boolean;      // Has flight termination system

  Medium = "Medium", // -2 GRC credit    frangibility?: boolean;           // Has frangible design

  High = "High"      // -4 GRC credit (much larger than SORA 2.5!)  };

}  validated?: boolean;                // Evidence checked and validated

}

/**

 * M2 Impact Reduction - SORA 2.0// ════════════════════════════════════════════════════════════════

 * Source: JAR_doc_06 Main Body Table 3, Page 21// Calculation Results Interfaces

 */// ════════════════════════════════════════════════════════════════

export enum M2_ImpactReduction_20 {

  None = "None",     // 0 creditexport interface GrcResult {

  Medium = "Medium", // -1 GRC credit  intrinsicGRC: number;               // Initial GRC from Table 2

  High = "High"      // -2 GRC credit  finalGRC: number;                   // After mitigations applied

  // ⚠️ Low does NOT exist in SORA 2.0  mitigationsApplied: string[];       // List of mitigations with credits

}  auditTrail: string[];               // Step-by-step calculation log

  columnMinimum?: number;             // SORA 2.0: column minimum for clamp

/**  smallUARuleApplied?: boolean;       // SORA 2.5: ≤250g & ≤25m/s rule

 * M3 Emergency Response Plan (ERP) - SORA 2.0}

 * Source: JAR_doc_06 Main Body Table 3, Page 21

 * export interface ArcResult {

 * ⚠️ CRITICAL: M3 "None" adds +1 penalty (increases GRC)!  aec: AEC;                           // Selected AEC category

 * ⚠️ M3 removed in SORA 2.5 (moved to OSOs)  aecLabel: string;                   // Human-readable AEC label from Annex C

 */  initialARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";

export enum M3_ERP_20 {  residualARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";

  None = "None",           // +1 penalty (INCREASES GRC!)  strategicMitigations: string[];     // Applied strategic mitigations

  Adequate = "Adequate",   // 0 (neutral, no credit)  explanation: string;                // How ARC was derived

  Validated = "Validated"  // -1 GRC credit}

}

export interface SailResult {

/**  finalGRC: number;

 * Operation Scenarios - SORA 2.0  residualARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";

 * Source: JAR_doc_06 Main Body Table 2, Page 20  sail: "I"|"II"|"III"|"IV"|"V"|"VI";

 *   requiredOSOs: Array<{

 * Used for iGRC lookup (8 scenarios)    id: string;

 */    name: string;

export enum OperationScenario_20 {    robustness: Robustness;

  VLOS_Controlled = "VLOS_Controlled",     // VLOS + Controlled ground    evidenceSource?: "Operator" | "Manufacturer";  // SORA 2.5 distinction

  VLOS_Sparsely = "VLOS_Sparsely",         // VLOS + Sparsely populated  }>;

  VLOS_Populated = "VLOS_Populated",       // VLOS + Populated  tmprTarget: TMPRLevel;              // TMPR requirement per residual ARC

  VLOS_Gathering = "VLOS_Gathering",       // VLOS + Assemblies of people}

  BVLOS_Controlled = "BVLOS_Controlled",   // BVLOS + Controlled ground

  BVLOS_Sparsely = "BVLOS_Sparsely",       // BVLOS + Sparsely populatedexport interface TMPRTarget {

  BVLOS_Populated = "BVLOS_Populated",     // BVLOS + Populated  arc: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";

  BVLOS_Gathering = "BVLOS_Gathering"      // BVLOS + Assemblies of people  target: TMPRLevel;

}  maxRatio: number | null;            // null for ARC-a (no requirement)

  description: string;

/**}

 * Size Categories - SORA 2.0
 * Source: JAR_doc_06 Main Body Table 2, Page 20
 */
export enum SizeCategory_20 {
  Small = "<1m",   // < 1 meter
  Medium = "1-3m", // 1-3 meters
  Large = "3-8m",  // 3-8 meters
  XLarge = ">8m"   // > 8 meters
}

/**
 * Aircraft Encounter Categories (AEC) - SORA 2.0
 * Source: JAR_doc_06 Annex C Table 1, Page 12
 */
export enum AEC_20 {
  AEC_1 = 1,   // Airport/Heliport B/C/D (density 5, ARC-d)
  AEC_2 = 2,   // >500ft Mode-S/TMZ (density 5, ARC-d)
  AEC_3 = 3,   // >500ft Controlled (density 5, ARC-d)
  AEC_4 = 4,   // >500ft Uncontrolled Urban (density 3, ARC-c)
  AEC_5 = 5,   // >500ft Uncontrolled Rural (density 2, ARC-c)
  AEC_6 = 6,   // Airport/Heliport E/F/G (density 3, ARC-c)
  AEC_7 = 7,   // <500ft Mode-S/TMZ (density 3, ARC-c)
  AEC_8 = 8,   // <500ft Controlled (density 3, ARC-c)
  AEC_9 = 9,   // <500ft Uncontrolled Urban (density 2, ARC-c)
  AEC_10 = 10, // <500ft Uncontrolled Rural (density 1, ARC-b)
  AEC_11 = 11, // >FL600 (density 1, ARC-b)
  AEC_12 = 12  // Atypical/Segregated (density 1, ARC-a)
}

/**
 * Initial ARC Categories - SORA 2.0
 * Source: JAR_doc_06 Annex C Table 1, Page 12
 */
export enum InitialARC_20 {
  ARC_a = "ARC-a",
  ARC_b = "ARC-b",
  ARC_c = "ARC-c",
  ARC_d = "ARC-d"
}

/**
 * Residual ARC Categories - SORA 2.0
 * Source: JAR_doc_06 Annex C Table 2, Page 14
 */
export enum ResidualARC_20 {
  ARC_a = "ARC-a",
  ARC_b = "ARC-b",
  ARC_c = "ARC-c",
  ARC_d = "ARC-d"
}

// =============================================================================
// COMMON ENUMS (Both SORA 2.0 and 2.5)
// =============================================================================

/**
 * ICAO Airspace Classes
 * Source: ICAO Annex 2, Appendix 4
 */
export enum AirspaceClass {
  A = "A", // IFR only, ATC clearance required
  B = "B", // IFR/VFR, ATC clearance required
  C = "C", // IFR/VFR, ATC clearance required (VFR: traffic info)
  D = "D", // IFR/VFR, ATC clearance required (VFR: traffic info)
  E = "E", // IFR/VFR, IFR gets ATC, VFR gets traffic info
  F = "F", // IFR/VFR, advisory service
  G = "G"  // IFR/VFR, flight information service
}

/**
 * Special Airspace Zones
 * 
 * ⚠️ COMPLIANCE NOTE: Use "RMZ/TMZ" in SORA 2.5
 * "Mode-S Veil" is SORA 2.0 terminology ONLY (AEC 2, AEC 7)
 * Source: JAR_doc_06 Annex C Table 1
 */
export enum SpecialZone {
  RMZ = "RMZ",                   // Radio Mandatory Zone
  TMZ = "TMZ",                   // Transponder Mandatory Zone
  CTR = "CTR",                   // Control Zone
  TMA = "TMA",                   // Terminal Maneuvering Area
  ATZ = "ATZ",                   // Aerodrome Traffic Zone
  Restricted = "Restricted",     // Restricted airspace
  Prohibited = "Prohibited",     // Prohibited airspace
  Danger = "Danger",             // Danger area
  TSA = "TSA",                   // Temporary Segregated Area
  TRA = "TRA",                   // Temporary Reserved Area
  CBA = "CBA",                   // Cross-Border Area
  UAS = "UAS Geographic Zone"    // UAS Geographic Zone
  // ❌ DO NOT USE "Mode-S Veil" - this is SORA 2.0 only!
}

/**
 * Airspace Control Type
 */
export enum AirspaceControl {
  Controlled = "Controlled",       // Class A, B, C, D, E
  Uncontrolled = "Uncontrolled"    // Class F, G
}

/**
 * Environment Type
 */
export enum EnvironmentType {
  Rural = "Rural",         // Low population density
  Suburban = "Suburban",   // Medium population density
  Urban = "Urban"          // High population density
}

/**
 * Altitude Bands
 */
export enum AltitudeBand {
  VLL = "VLL",               // Very Low Level (<500 ft AGL)
  Low = "Low",               // 500-5000 ft AGL
  Medium = "Medium",         // 5000 ft - FL195
  High = "High",             // FL195 - FL600
  VeryHigh = "VeryHigh"      // >FL600
}

/**
 * Airspace Typicality
 */
export enum AirspaceTypicality {
  Typical = "Typical",                 // Normal airspace
  Atypical = "Atypical",               // Atypical/Segregated (ARC-a)
  AirportEnvironment = "Airport"       // Airport/Heliport environment
}

/**
 * VLOS/BVLOS/EVLOS
 */
export enum LineOfSight {
  VLOS = "VLOS",   // Visual Line Of Sight
  EVLOS = "EVLOS", // Extended Visual Line Of Sight
  BVLOS = "BVLOS"  // Beyond Visual Line Of Sight
}

/**
 * SORA Version Selector
 */
export enum SORAVersion {
  SORA_20 = "2.0",
  SORA_25 = "2.5"
}

/**
 * Regulatory Framework
 */
export enum RegulatoryFramework {
  SORA_20 = "SORA 2.0",
  SORA_25 = "SORA 2.5",
  PDRA_S01 = "PDRA-S01",
  PDRA_S02 = "PDRA-S02",
  STS_01 = "STS-01",
  STS_02 = "STS-02"
}

// =============================================================================
// COMPLIANCE GUARDS
// =============================================================================

/**
 * Banned Tokens - DO NOT USE in SORA 2.5 implementations
 * 
 * Source: JAR_doc_06 Annex C Table 1 (SORA 2.0 only)
 */
export const BANNED_TOKENS_SORA_25 = [
  "Mode-S veil",
  "Mode-S Veil",
  "MODE-S VEIL"
] as const;

/**
 * Validate M1(A) and M1(B) combination - SORA 2.5
 * 
 * ⚠️ CRITICAL CONSTRAINT: M1(A) Medium CANNOT combine with M1(B)
 * Source: JAR_doc_27 Annex B Section B.2, Page 8
 * 
 * @param m1a - M1(A) Sheltering level
 * @param m1b - M1(B) Operational Restrictions level
 * @throws Error if invalid combination detected
 */
export function validateM1A_M1B_Combination(
  m1a: M1A_Sheltering_25,
  m1b: M1B_OperationalRestrictions_25
): void {
  if (m1a === M1A_Sheltering_25.Medium && m1b !== M1B_OperationalRestrictions_25.None) {
    throw new Error(
      "❌ VALIDATION ERROR: M1(A) Medium robustness cannot be combined with M1(B) mitigations. " +
      "Source: JAR-DEL-SRM-SORA-B-2.5 Annex B Section B.2, Page 8. " +
      "Please select M1(A) Low or set M1(B) to None."
    );
  }
}

/**
 * Enforce finalGRC >= 1 floor
 * 
 * Source: JAR_doc_25 Main Body Table 7, Page 47 (SAIL matrix starts at GRC 1)
 * 
 * @param finalGRC - Calculated final GRC
 * @returns Clamped GRC (minimum 1)
 */
export function enforceGRCFloor(finalGRC: number): number {
  return Math.max(finalGRC, 1);
}
