import { MissionTemplate } from './types';

// Helper to map Sonnet's residual_arc_numeric (~0.0-4.0) to integer residual_arc_level (2..9)
// This is a rough representative mapping aligned with our SAIL 2.5 expectations
const mapResidualNumericToLevel = (x: number): number => {
  if (!isFinite(x)) return 2;
  if (x <= 0.5) return 2;
  if (x <= 1.5) return 3;
  if (x <= 2.0) return 4;
  if (x <= 2.5) return 5;
  if (x <= 3.0) return 6;
  if (x <= 3.5) return 7;
  if (x <= 3.9) return 8;
  return 9;
};

export const seedTemplates: MissionTemplate[] = [
  {
    template_id: 'urban-bvlos-facade-001',
    name: 'Urban BVLOS Building Facade Inspection/Cleaning',
    sora_version: '2.5',
    scenario_tag: 'CUSTOM',
    operational_constraints: {
      altitude: { max_agl_ft: 400, buffer_ft: 50 },
      geofence: { type: 'polygon', buffer_m: 30 },
      speed: { max_groundspeed_ms: 5.0 },
    },
    sora_input_block: {
      sora_2_5: {
        m1a_ground_risk: 'High',
        m1b_ground_risk: 'Low',
        m1c_ground_risk: 'Low',
        m2_air_risk: 'Low',
        residual_arc_level: mapResidualNumericToLevel(2.5),
        final_sail: 'IV',
      },
    },
  },
  {
    template_id: 'suburban-vlos-solar-001',
    name: 'Suburban VLOS Solar Panel Inspection',
    sora_version: '2.0',
    scenario_tag: 'CUSTOM',
    operational_constraints: {
      altitude: { max_agl_ft: 200 },
      speed: { max_groundspeed_ms: 8.0 },
      temporal: { daylight_only: true },
    },
    sora_input_block: {
      sora_2_0: {
        intrinsic_grc: 4,
        initial_arc: 'b',
        oso_requirements: { class_I: 2, class_II: 4, class_III: 0 },
        final_arc: 'a',
        final_sail: 'II',
      },
    },
  },
  {
    template_id: 'sts01-roof-001',
    name: 'STS-01 Commercial Roof Inspection',
    sora_version: '2.5',
    scenario_tag: 'STS-01',
    operational_constraints: {
      altitude: { max_agl_ft: 120 },
      speed: { max_groundspeed_ms: 5.0 },
      geofence: { type: 'polygon', buffer_m: 10 },
    },
    sora_input_block: {
      sora_2_5: {
        m1a_ground_risk: 'Medium',
        m1b_ground_risk: 'None',
        m1c_ground_risk: 'None',
        m2_air_risk: 'None',
        residual_arc_level: mapResidualNumericToLevel(1.0),
        final_sail: 'I',
      },
    },
  },
  {
    template_id: 'linear-bvlos-utility-001',
    name: 'Linear BVLOS Utility Line Inspection',
    sora_version: '2.0',
    scenario_tag: 'CUSTOM',
    operational_constraints: {
      altitude: { max_agl_ft: 300 },
      geofence: { type: 'corridor', buffer_m: 50 },
      speed: { max_groundspeed_ms: 12.0 },
    },
    sora_input_block: {
      sora_2_0: {
        intrinsic_grc: 6,
        initial_arc: 'c',
        oso_requirements: { class_I: 4, class_II: 8, class_III: 10, class_IV: 5 },
        final_arc: 'b',
        final_sail: 'III',
      },
    },
    mitigations_profile: undefined,
  } as any, // keep structure close to narrative while focusing on core fields
  {
    template_id: 'night-vlos-urban-001',
    name: 'Night VLOS Urban Area Surveillance',
    sora_version: '2.5',
    scenario_tag: 'CUSTOM',
    operational_constraints: {
      altitude: { max_agl_ft: 150 },
      speed: { max_groundspeed_ms: 3.0 },
      temporal: { daylight_only: false, weather_minima: { visibility_m: 3000 } },
    },
    sora_input_block: {
      sora_2_5: {
        m1a_ground_risk: 'High',
        m1b_ground_risk: 'Low',
        m1c_ground_risk: 'Low',
        m2_air_risk: 'Low',
        residual_arc_level: mapResidualNumericToLevel(3.0),
        final_sail: 'V',
      },
    },
  },
  {
    template_id: 'harbor-vlos-rmz-001',
    name: 'Harbor VLOS Operations near RMZ/TMZ',
    sora_version: '2.0',
    scenario_tag: 'CUSTOM',
    operational_constraints: {
      altitude: { max_agl_ft: 100, buffer_ft: 20 },
      speed: { max_groundspeed_ms: 6.0 },
      geofence: { type: 'polygon', buffer_m: 100 },
    },
    sora_input_block: {
      sora_2_0: {
        intrinsic_grc: 8,
        initial_arc: 'd',
        oso_requirements: { class_I: 6, class_II: 10, class_III: 15, class_IV: 12, class_V: 8 },
        final_arc: 'c',
        final_sail: 'IV',
      },
    },
  },
];

export default seedTemplates;
