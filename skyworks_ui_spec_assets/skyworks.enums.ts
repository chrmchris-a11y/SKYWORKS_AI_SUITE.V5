// skyworks.enums.ts
// Fixed dropdowns & enums per EASA/JARUS (SORA 2.0 AMC & JARUS SORA 2.5)
// NOTE: AEC labels must be loaded from Annex C Table C.1 (KnowledgeBase). Fallback uses numeric "AEC-1"…"AEC-12".

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

export const Environment25 = ["CGA","Rural","Suburban","Urban"] as const; // per SORA 2.5 qualitative descriptors
export type Environment25 = typeof Environment25[number];

export const SpecialZones = [
  "CTR",
  "TMA",
  "ATZ",
  "RMZ",
  "TMZ",
  "Prohibited_P",
  "Restricted_R",
  "Danger_D",
  "TSA",
  "TRA",
  "CBA",
  "UAS_Geographical_Zone",
] as const;
export type SpecialZone = typeof SpecialZones[number];

export const Typicality = ["Typical","Atypical_Segregated"] as const;
export type Typicality = typeof Typicality[number];

export const TrafficDensitySource = ["Empirical","Modelled","ANSP"] as const;

export const USpace = ["Yes","No"] as const;

export const AirspaceContainment = [
  "None",
  "Horizontal",
  "Vertical",
  "Horizontal_and_Vertical"
] as const;

// AEC: 1..12 (labels to be loaded from KB at runtime)
export const AECValues = Array.from({length:12}, (_,i)=>`AEC_${i+1}`) as const;
export type AEC = typeof AECValues[number];

// TMPR target per Residual ARC (Annex D guidance mapping convenience keys)
export const TMPRLevel = ["None","Low","Medium","High"] as const;

// SORA 2.5 — Ground Mitigations (Table 5)
export const M1A_Sheltering = ["None","Low"] as const;  // High sheltering is evidence-only; no extra dropdown credit
export const M1B_OperationalRestrictions = ["None","Low"] as const;
export const M1C_GroundObservation = ["None","Low"] as const;
export const M2_ImpactDynamics = ["None","Low","Medium","High"] as const;

// SORA 2.0 — Ground Mitigations (Table 3)
export const M1_20 = ["None","Low","Medium","High"] as const; // clamp to column minimum
export const M2_20 = ["None","Low","High"] as const;
export const M3_20 = ["None","Adequate","Validated"] as const;

// OSO robustness levels
export const Robustness = ["Low","Medium","High"] as const;

// Drone library
export const DroneClass = ["C0","C1","C2","C3","C4","C5","C6","PB"] as const; // PB = privately built
export type DroneClass = typeof DroneClass[number];

export interface DroneSpec {
  manufacturer: string;
  model: string;
  classMarking: DroneClass;
  mtomKg: number;
  maxDimensionM: number;
  typicalSpeedMs: number;
  tethered: boolean;
  equipage: {
    parachute?: boolean;
    flightTermination?: boolean;
    frangibility?: boolean;
  };
  validated?: boolean; // evidence checked
}

// --- ZOD SCHEMAS (skeletons) ---
import { z } from "zod";

export const SoraCommonSchema = z.object({
  version: z.enum(SoraVersion),
  operationType: z.enum(OperationType),
  airspaceClass: z.enum(AirspaceClass),
  environment25: z.enum(Environment25).optional(), // used in 2.5
  typicality: z.enum(Typicality),
  maxHeightAGL_m: z.number().min(1).max(600), // scope; EU usually <=120m
  specialZones: z.array(z.enum(SpecialZones)).default([]),
  aec: z.enum(AECValues),
  uSpace: z.enum(USpace),
  trafficDensitySource: z.enum(TrafficDensitySource),
  airspaceContainment: z.enum(AirspaceContainment),
});

export const Grc20Schema = z.object({
  intrinsicGRC: z.number().int().min(1).max(7),  // from Table 2
  m1: z.enum(M1_20),
  m2: z.enum(M2_20),
  m3: z.enum(M3_20),
});

export const Grc25Schema = z.object({
  intrinsicGRC: z.number().int().min(1).max(7),  // from Table 2
  m1a: z.enum(M1A_Sheltering),
  m1b: z.enum(M1B_OperationalRestrictions),
  m1c: z.enum(M1C_GroundObservation),
  m2: z.enum(M2_ImpactDynamics),
  smallUARuleApplies: z.boolean().default(false), // MTOM<=0.25kg && speed<=25m/s
});

export const DroneSpecSchema = z.object({
  manufacturer: z.string(),
  model: z.string(),
  classMarking: z.enum(DroneClass),
  mtomKg: z.number().positive(),
  maxDimensionM: z.number().positive(),
  typicalSpeedMs: z.number().positive(),
  tethered: z.boolean().default(false),
  equipage: z.object({
    parachute: z.boolean().optional(),
    flightTermination: z.boolean().optional(),
    frangibility: z.boolean().optional(),
  }),
  validated: z.boolean().optional(),
});

// Calculation contracts (interfaces) to wire with your Engine
export interface GrcResult {
  initialGRC: number;
  finalGRC: number;
  auditTrail: string[];
}

export interface ArcResult {
  initialARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
  residualARC: "ARC-a"|"ARC-b"|"ARC-c"|"ARC-d";
}

export interface SailResult {
  sail: "I"|"II"|"III"|"IV"|"V"|"VI";
  requiredOSOs: Array<{ id: string; robustness: typeof Robustness[number] }>;
}
