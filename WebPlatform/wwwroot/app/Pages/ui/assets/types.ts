/**
 * SKYWORKS SORA 2.5 Annex B & SORA 2.0 AMC1 Art.11 Type Definitions
 * Source of Truth for UI Dropdowns & Validation
 * DO NOT MODIFY WITHOUT SPEC APPROVAL
 */

// ═══════════════════════════════════════════════════════════════════
// SORA 2.5 (JARUS SORA 2.5 Annex B)
// ═══════════════════════════════════════════════════════════════════

export type M1A_Sheltering_25 = "None" | "Low";
export type M1B_OperationalRestrictions_25 = "None" | "Medium" | "High";
export type M1C_GroundObservation_25 = "None" | "Low";
export type M2_ImpactDynamics_25 = "None" | "Low" | "Medium" | "High";

export const M1A_VALUES_25: readonly M1A_Sheltering_25[] = ["None", "Low"];
export const M1B_VALUES_25: readonly M1B_OperationalRestrictions_25[] = ["None", "Medium", "High"];
export const M1C_VALUES_25: readonly M1C_GroundObservation_25[] = ["None", "Low"];
export const M2_VALUES_25: readonly M2_ImpactDynamics_25[] = ["None", "Low", "Medium", "High"];

// ═══════════════════════════════════════════════════════════════════
// SORA 2.0 (EASA AMC1 Article 11)
// ═══════════════════════════════════════════════════════════════════

export type M1_Strategic_20 = "None" | "Low" | "Medium" | "High";
export type M2_ImpactReduction_20 = "None" | "Low" | "High"; // NO MEDIUM
export type M3_ERP_20 = "None" | "Adequate" | "Validated";

export const M1_VALUES_20: readonly M1_Strategic_20[] = ["None", "Low", "Medium", "High"];
export const M2_VALUES_20: readonly M2_ImpactReduction_20[] = ["None", "Low", "High"];
export const M3_VALUES_20: readonly M3_ERP_20[] = ["None", "Adequate", "Validated"];

// ═══════════════════════════════════════════════════════════════════
// Common Airspace & Context (EU ONLY)
// ═══════════════════════════════════════════════════════════════════

export type AirspaceClass = "A" | "B" | "C" | "D" | "E" | "F" | "G";
export type SpecialZone =
  | "CTR"
  | "TMA"
  | "ATZ"
  | "RMZ"
  | "TMZ"
  | "Prohibited"
  | "Restricted"
  | "Danger"
  | "TSA"
  | "TRA"
  | "CBA"
  | "UAS_Geo_Zone";

export type Typicality = "Typical" | "Atypical";
export type USpace = "Yes" | "No";
export type TrafficDensitySource = "Empirical" | "Modelled" | "ANSP";
export type AirspaceContainment = "None" | "Horizontal" | "Vertical" | "Horizontal_and_Vertical";
export type AEC = 
  | "AEC_1" | "AEC_2" | "AEC_3" | "AEC_4" | "AEC_5" | "AEC_6"
  | "AEC_7" | "AEC_8" | "AEC_9" | "AEC_10" | "AEC_11" | "AEC_12";

export const AIRSPACE_CLASS_VALUES: readonly AirspaceClass[] = ["A", "B", "C", "D", "E", "F", "G"];
export const SPECIAL_ZONE_VALUES: readonly SpecialZone[] = [
  "CTR", "TMA", "ATZ", "RMZ", "TMZ",
  "Prohibited", "Restricted", "Danger",
  "TSA", "TRA", "CBA", "UAS_Geo_Zone"
];
export const TYPICALITY_VALUES: readonly Typicality[] = ["Typical", "Atypical"];
export const USPACE_VALUES: readonly USpace[] = ["Yes", "No"];
export const TRAFFIC_DENSITY_VALUES: readonly TrafficDensitySource[] = ["Empirical", "Modelled", "ANSP"];
export const AIRSPACE_CONTAINMENT_VALUES: readonly AirspaceContainment[] = [
  "None", "Horizontal", "Vertical", "Horizontal_and_Vertical"
];
export const AEC_VALUES: readonly AEC[] = [
  "AEC_1", "AEC_2", "AEC_3", "AEC_4", "AEC_5", "AEC_6",
  "AEC_7", "AEC_8", "AEC_9", "AEC_10", "AEC_11", "AEC_12"
];

// ═══════════════════════════════════════════════════════════════════
// Payload Structures
// ═══════════════════════════════════════════════════════════════════

export interface CommonPayload {
  operationType: string; // "VLOS" | "BVLOS" | "EVLOS"
  airspaceClass: AirspaceClass;
  typicality: Typicality;
  maxHeightAGL_m: number;
  specialZones: SpecialZone[];
  aec: AEC;
  uSpace: boolean;
  trafficDensitySource: TrafficDensitySource;
  airspaceContainment: AirspaceContainment;
}

export interface Grc25Payload {
  m1a: M1A_Sheltering_25;
  m1b: M1B_OperationalRestrictions_25;
  m1c: M1C_GroundObservation_25;
  m2: M2_ImpactDynamics_25;
  smallUARuleApplies: boolean;
}

export interface Grc20Payload {
  m1: M1_Strategic_20;
  m2: M2_ImpactReduction_20;
  m3: M3_ERP_20;
}

export interface SoraRequest {
  soraVersion: "2.5" | "2.0";
  common: CommonPayload;
  grc: Grc25Payload | Grc20Payload;
}

export interface SoraResponse {
  initialGrc: number;
  finalGrc: number;
  arc: {
    initial: string;
    residual: string;
  };
  sail: string;
  warnings: string[];
  errors: string[];
}
