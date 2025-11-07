// skyworks.enums.ts
// Fixed dropdowns & enums per EASA/JARUS (SORA 2.0 AMC & JARUS SORA 2.5)
// NOTE: AEC labels must be loaded from Annex C Table C.1 (KnowledgeBase). Fallback uses numeric "AEC-1"…"AEC-12".
// ✅ 100% Compliance με EASA/JARUS επίσημα έγγραφα

export const SoraVersion = [
  "SORA_2_0_AMC",
  "SORA_2_5_JARUS",
  "PDRA_S01",
  "PDRA_S02",
  "STS_01",
  "STS_02",
] as const;
export type SoraVersion = typeof SoraVersion[number];

export const OperationType = ["VLOS", "BVLOS", "EVLOS"] as const;
export type OperationType = typeof OperationType[number];

export const AirspaceClass = ["A","B","C","D","E","F","G"] as const;
export type AirspaceClass = typeof AirspaceClass[number];

// SORA 2.5 Population Density (per JARUS Table 2)
export const Environment25 = ["CGA","Rural","Suburban","Urban"] as const;
export type Environment25 = typeof Environment25[number];

// EU Airspace Special Zones (NO "Mode-S veil" - FAA only)
// ✅ ref: EASA Regulation (EU) 2019/947, Annex A definitions
export const SpecialZones = [
  "CTR",      // Control Zone
  "TMA",      // Terminal Maneuvering Area
  "ATZ",      // Aerodrome Traffic Zone
  "RMZ",      // Radio Mandatory Zone (EU equivalent of Mode-S)
  "TMZ",      // Transponder Mandatory Zone (EU equivalent of Mode-S)
  "Prohibited_P",
  "Restricted_R",
  "Danger_D",
  "TSA",      // Temporary Segregated Area
  "TRA",      // Temporary Reserved Area
  "CBA",      // Cross-Border Area
  "UAS_Geographical_Zone",
] as const;
export type SpecialZone = typeof SpecialZones[number];

export const Typicality = ["Typical","Atypical_Segregated"] as const;
export type Typicality = typeof Typicality[number];

// SORA 2.5 Step #5 - Traffic Density Source
// ✅ ref: JARUS SORA 2.5 Annex C, strategic mitigations
export const TrafficDensitySource = ["Empirical","Modelled","ANSP"] as const;
export type TrafficDensitySource = typeof TrafficDensitySource[number];

export const USpace = ["Yes","No"] as const;
export type USpace = typeof USpace[number];

// SORA 2.5 Step #5 - Airspace Containment
// ✅ ref: JARUS SORA 2.5 Annex C Table C.2
export const AirspaceContainment = [
  "None",
  "Horizontal",
  "Vertical",
  "Horizontal_and_Vertical"
] as const;
export type AirspaceContainment = typeof AirspaceContainment[number];

// AEC: 1..12 (Airspace Encounter Categories)
// ✅ ref: JARUS SORA 2.5 Annex C Table C.1
export const AECValues = ["AEC_1","AEC_2","AEC_3","AEC_4","AEC_5","AEC_6","AEC_7","AEC_8","AEC_9","AEC_10","AEC_11","AEC_12"] as const;
export type AEC = typeof AECValues[number];

// TMPR target per Residual ARC
// ✅ ref: JARUS SORA Annex D (TMPR guidance)
export const TMPRLevel = ["None","Low","Medium","High"] as const;
export type TMPRLevel = typeof TMPRLevel[number];

// ════════════════════════════════════════════════════════════════
// SORA 2.5 — Ground Risk Mitigations (Table 5)
// ✅ ref: JARUS SORA 2.5 Main Body Table 5 + Annex B (JAR_doc_27)
// ════════════════════════════════════════════════════════════════

// M1(A) - Sheltering (per Table 5 + Annex B)
// ✅ Credits: None=0, Low=-1, Medium=-2 (Medium requires high evidence/assurance)
export const M1A_Sheltering = ["None","Low","Medium"] as const;
export type M1A_Sheltering = typeof M1A_Sheltering[number];

// M1(B) - Operational restrictions (per Table 5 + Annex B)
// ✅ Credits: None=0, Medium=-1, High=-2
export const M1B_OperationalRestrictions = ["None","Medium","High"] as const;
export type M1B_OperationalRestrictions = typeof M1B_OperationalRestrictions[number];

// M1(C) - Ground observation (per Table 5 + Annex B)
// ✅ Credits: None=0, Low=-1 (only Low level defined)
export const M1C_GroundObservation = ["None","Low"] as const;
export type M1C_GroundObservation = typeof M1C_GroundObservation[number];

// M2 - Impact dynamics reduced (per Table 5 + Annex B)
// ✅ Credits: None=0, Medium=-1, High=-2
// Note: parachute, frangibility, etc. affect robustness level
export const M2_ImpactDynamics = ["None","Medium","High"] as const;
export type M2_ImpactDynamics = typeof M2_ImpactDynamics[number];

// ════════════════════════════════════════════════════════════════
// SORA 2.0 — Ground Risk Mitigations (Table 3)
// ✅ ref: AMC1 to Article 11 (SORA 2.0), Table 3
// ════════════════════════════════════════════════════════════════

// M1 - Strategic mitigations (reduce people at risk)
// ✅ Credits: None=0, Low=-1, Medium=-2, High=-4
// ⚠️ MUST apply column-min clamp (cannot reduce below column minimum)
export const M1_20 = ["None","Low","Medium","High"] as const;
export type M1_20 = typeof M1_20[number];

// M2 - Impact reduction (e.g., parachute)
// ✅ Credits: None=0, Medium=-1, High=-2
// ⚠️ NO "Low" level for M2 in SORA 2.0!
export const M2_20 = ["None","Medium","High"] as const;
export type M2_20 = typeof M2_20[number];

// M3 - Emergency Response Plan
// ✅ Credits: Low=+1 (penalty if inadequate), Medium=0, High=-1
export const M3_20 = ["Low","Medium","High"] as const;
export type M3_20 = typeof M3_20[number];

// ════════════════════════════════════════════════════════════════
// OSO Robustness Levels
// ✅ ref: JARUS SORA 2.5 Table 14 + SORA 2.0 Table 6
// ════════════════════════════════════════════════════════════════
export const Robustness = ["Low","Medium","High"] as const;
export type Robustness = typeof Robustness[number];

// ════════════════════════════════════════════════════════════════
// Drone Library - UA Class Marking
// ✅ ref: EASA Regulation (EU) 2019/945 (Drone classes)
// ════════════════════════════════════════════════════════════════
export const DroneClass = ["C0","C1","C2","C3","C4","C5","C6","PB"] as const;
export type DroneClass = typeof DroneClass[number];

export interface DroneSpec {
  id?: string;
  manufacturer: string;
  model: string;
  classMarking: DroneClass;
  mtomKg: number;                    // MTOM in kg
  maxDimensionM: number;              // Max characteristic dimension in meters
  typicalSpeedMs: number;             // Typical cruise speed in m/s
  tethered: boolean;                  // Is the UA tethered?
  equipage: {
    parachute?: boolean;              // Has parachute system
    flightTermination?: boolean;      // Has flight termination system
    frangibility?: boolean;           // Has frangible design
  };
  validated?: boolean;                // Evidence checked and validated
}

// ════════════════════════════════════════════════════════════════
// Calculation Results Interfaces
// ════════════════════════════════════════════════════════════════

export interface GrcResult {
  intrinsicGRC: number;               // Initial GRC from Table 2
  finalGRC: number;                   // After mitigations applied
  mitigationsApplied: string[];       // List of mitigations with credits
  auditTrail: string[];               // Step-by-step calculation log
  columnMinimum?: number;             // SORA 2.0: column minimum for clamp
  smallUARuleApplied?: boolean;       // SORA 2.5: ≤250g & ≤25m/s rule
}

export interface ArcResult {
  aec: AEC;                           // Selected AEC category
  aecLabel: string;                   // Human-readable AEC label from Annex C
  initialARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
  residualARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
  strategicMitigations: string[];     // Applied strategic mitigations
  explanation: string;                // How ARC was derived
}

export interface SailResult {
  finalGRC: number;
  residualARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
  sail: "I"|"II"|"III"|"IV"|"V"|"VI";
  requiredOSOs: Array<{
    id: string;
    name: string;
    robustness: Robustness;
    evidenceSource?: "Operator" | "Manufacturer";  // SORA 2.5 distinction
  }>;
  tmprTarget: TMPRLevel;              // TMPR requirement per residual ARC
}

export interface TMPRTarget {
  arc: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
  target: TMPRLevel;
  maxRatio: number | null;            // null for ARC-a (no requirement)
  description: string;
}
