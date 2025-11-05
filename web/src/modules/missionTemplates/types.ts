export type SoraVersion = '2.0' | '2.5';

export type SailLevel = 'I' | 'II' | 'III' | 'IV' | 'V' | 'VI';

export type ArcLetter = 'a' | 'b' | 'c' | 'd';

export type RiskLevelWide = 'None' | 'Low' | 'Medium' | 'High';
export type RiskLevelNarrow = 'None' | 'Low';

export interface Sora20Block {
  intrinsic_grc: number; // 1-12
  initial_arc: ArcLetter;
  oso_requirements?: Partial<Record<
    'class_I' | 'class_II' | 'class_III' | 'class_IV' | 'class_V' | 'class_VI',
    number
  >>;
  final_arc?: ArcLetter;
  final_sail?: SailLevel | null;
}

export interface Sora25Block {
  m1a_ground_risk: RiskLevelWide; // Annex F: High must be mitigated to Medium
  m1b_ground_risk: RiskLevelNarrow; // Low→None
  m1c_ground_risk: RiskLevelNarrow; // None/Low only
  m2_air_risk: RiskLevelNarrow; // Low→None
  // Our stack expects integer residual ARC level (2..9) when calling SAIL in 2.5
  residual_arc_level: number; // 2..9
  final_sail?: SailLevel | null;
}

export interface MissionTemplate {
  template_id: string;
  name: string;
  description?: string;
  sora_version: SoraVersion;
  scenario_tag?: 'STS-01' | 'STS-02' | 'PDRA-S01' | 'CUSTOM';
  operational_constraints?: {
    altitude?: { max_agl_ft?: number; max_amsl_ft?: number; buffer_ft?: number };
    geofence?: { type?: 'polygon' | 'circle' | 'corridor'; buffer_m?: number };
    speed?: { max_groundspeed_ms?: number; max_vertical_ms?: number };
    temporal?: { daylight_only?: boolean; weather_minima?: { visibility_m?: number; ceiling_ft?: number; wind_max_ms?: number } };
  };
  sora_input_block: {
    sora_2_0?: Sora20Block;
    sora_2_5?: Sora25Block;
  };
}

export interface ValidationResult {
  errors: string[];
  warnings: string[];
}
