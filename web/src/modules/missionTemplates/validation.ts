import { MissionTemplate, ValidationResult } from './types';

export function validateTemplate(t: MissionTemplate): ValidationResult {
  const errors: string[] = [];
  const warnings: string[] = [];

  if (t.sora_version === '2.5') {
    const s25 = t.sora_input_block.sora_2_5;
    if (!s25) {
      errors.push('sora_2_5 block required for SORA 2.5');
    } else {
      if (!['None', 'Low', 'Medium', 'High'].includes(s25.m1a_ground_risk)) {
        errors.push('m1a_ground_risk must be one of None/Low/Medium/High');
      }
      if (!['None', 'Low'].includes(s25.m1b_ground_risk)) {
        errors.push('m1b_ground_risk must be None/Low');
      }
      if (!['None', 'Low'].includes(s25.m1c_ground_risk)) {
        errors.push('m1c_ground_risk must be None/Low');
      }
      if (!['None', 'Low'].includes(s25.m2_air_risk)) {
        errors.push('m2_air_risk must be None/Low');
      }
      if (!(Number.isFinite(s25.residual_arc_level) && s25.residual_arc_level >= 2 && s25.residual_arc_level <= 9)) {
        errors.push('residual_arc_level must be an integer between 2 and 9');
      }

      // Annex F NA rules are hard-validation (blocking)
      if (s25.m1a_ground_risk === 'High') {
        errors.push('M1A High must be mitigated to Medium (Annex F)');
      }
      if (s25.m1b_ground_risk === 'Low') {
        errors.push('M1B Low must be mitigated to None (Annex F)');
      }
      if (s25.m2_air_risk === 'Low') {
        errors.push('M2 Low must be mitigated to None (Annex F)');
      }
    }
  }

  if (t.sora_version === '2.0') {
    const s20 = t.sora_input_block.sora_2_0;
    if (!s20) {
      errors.push('sora_2_0 block required for SORA 2.0');
    } else {
      if (!(Number.isFinite(s20.intrinsic_grc) && s20.intrinsic_grc >= 1 && s20.intrinsic_grc <= 12)) {
        errors.push('intrinsic_grc must be 1..12');
      }
      const osoLimits: Record<string, number> = { class_I: 6, class_II: 10, class_III: 15, class_IV: 18, class_V: 21, class_VI: 24 };
      if (s20.oso_requirements) {
        for (const [k, v] of Object.entries(s20.oso_requirements)) {
          if (typeof v === 'number' && v > (osoLimits as any)[k]) {
            errors.push(`${k} OSO count ${v} exceeds limit ${(osoLimits as any)[k]}`);
          }
        }
      }
      if (s20.intrinsic_grc > 7) {
        warnings.push('GRC > 7 triggers Category C operations (SORA 2.0)');
      }
    }
  }

  return { errors, warnings };
}

export function validateTemplates(templates: MissionTemplate[]): ValidationResult[] {
  return templates.map(validateTemplate);
}
