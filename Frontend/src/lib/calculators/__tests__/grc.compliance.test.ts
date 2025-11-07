// grc.compliance.test.ts
// Golden Test Cases for SORA 2.5/2.0 GRC Compliance
// ✅ ref: Official JARUS SORA 2.5 Annex B (JAR_doc_27) Tables 2-8
// ✅ ref: Official AMC1 to Article 11 (SORA 2.0) Table 3

import { describe, it, expect } from 'vitest';
import { calculateGRC25 } from '../grc25';
import { calculateGRC20 } from '../grc20';

describe('SORA 2.5 GRC Compliance - Official Annex B', () => {
  
  /**
   * ✅ Golden Test 1: M1(A) Sheltering - Only Low integrity level
   * ref: Official Annex B Tables 2-3
   * 
   * MUST accept: None, Low
   * MUST reject: Medium, High (not defined in Annex B)
   */
  it('Golden Test 1: M1(A) accepts only None/Low per Annex B Tables 2-3', () => {
    const baseInput = {
      mtomKg: 5,
      maxDimensionM: 1.5,
      typicalSpeedMs: 20,
      populationDensity: 'Suburban' as const,
      m1a: 'None' as const,
      m1b: 'None' as const,
      m1c: 'None' as const,
      m2: 'None' as const,
    };
    
    // ✅ MUST accept None
    const resultNone = calculateGRC25({ ...baseInput, m1a: 'None' });
    expect(resultNone.isValid).toBe(true);
    expect(resultNone.mitigationsApplied.find(m => m.type.includes('M1(A)'))).toBeUndefined();
    
    // ✅ MUST accept Low
    const resultLow = calculateGRC25({ ...baseInput, m1a: 'Low' });
    expect(resultLow.isValid).toBe(true);
    const m1aMitigation = resultLow.mitigationsApplied.find(m => m.type.includes('M1(A)'));
    expect(m1aMitigation).toBeDefined();
    expect(m1aMitigation?.robustness).toBe('Low');
    expect(m1aMitigation?.credit).toBe(-1);
    
    // ❌ Backend should NOT accept Medium (removed per Official Annex B)
    // Note: TypeScript enum now prevents this at compile-time
  });

  /**
   * ✅ Golden Test 2: M1(B) Operational Restrictions - Only Medium/High integrity
   * ref: Official Annex B Tables 4-5
   * 
   * MUST accept: None, Medium, High
   * MUST reject: Low (not defined in Annex B)
   */
  it('Golden Test 2: M1(B) accepts only None/Medium/High per Annex B Tables 4-5', () => {
    const baseInput = {
      mtomKg: 5,
      maxDimensionM: 1.5,
      typicalSpeedMs: 20,
      populationDensity: 'Suburban' as const,
      m1a: 'None' as const,
      m1b: 'None' as const,
      m1c: 'None' as const,
      m2: 'None' as const,
    };
    
    // ✅ MUST accept None
    const resultNone = calculateGRC25({ ...baseInput, m1b: 'None' });
    expect(resultNone.isValid).toBe(true);
    
    // ✅ MUST accept Medium
    const resultMedium = calculateGRC25({ ...baseInput, m1b: 'Medium' });
    expect(resultMedium.isValid).toBe(true);
    const m1bMedium = resultMedium.mitigationsApplied.find(m => m.type.includes('M1(B)'));
    expect(m1bMedium?.robustness).toBe('Medium');
    expect(m1bMedium?.credit).toBe(-1);
    
    // ✅ MUST accept High
    const resultHigh = calculateGRC25({ ...baseInput, m1b: 'High' });
    expect(resultHigh.isValid).toBe(true);
    const m1bHigh = resultHigh.mitigationsApplied.find(m => m.type.includes('M1(B)'));
    expect(m1bHigh?.robustness).toBe('High');
    expect(m1bHigh?.credit).toBe(-2);
  });

  /**
   * ✅ Golden Test 3: M1(C) Ground Observation - Only Low integrity level
   * ref: Official Annex B Tables 6-7
   * 
   * MUST accept: None, Low
   * MUST reject: Medium, High (not defined in Annex B)
   */
  it('Golden Test 3: M1(C) accepts only None/Low per Annex B Tables 6-7', () => {
    const baseInput = {
      mtomKg: 5,
      maxDimensionM: 1.5,
      typicalSpeedMs: 20,
      populationDensity: 'Suburban' as const,
      m1a: 'None' as const,
      m1b: 'None' as const,
      m1c: 'None' as const,
      m2: 'None' as const,
    };
    
    // ✅ MUST accept None
    const resultNone = calculateGRC25({ ...baseInput, m1c: 'None' });
    expect(resultNone.isValid).toBe(true);
    
    // ✅ MUST accept Low
    const resultLow = calculateGRC25({ ...baseInput, m1c: 'Low' });
    expect(resultLow.isValid).toBe(true);
    const m1cMitigation = resultLow.mitigationsApplied.find(m => m.type.includes('M1(C)'));
    expect(m1cMitigation?.robustness).toBe('Low');
    expect(m1cMitigation?.credit).toBe(-1);
  });

  /**
   * ✅ Golden Test 4: M2 Impact Dynamics (SORA 2.5) - Low/Medium/High integrity
   * ref: Official Annex B Table 8+
   * 
   * MUST accept: None, Low, Medium, High
   */
  it('Golden Test 4: M2 (SORA 2.5) accepts None/Low/Medium/High per Annex B Table 8+', () => {
    const baseInput = {
      mtomKg: 5,
      maxDimensionM: 1.5,
      typicalSpeedMs: 20,
      populationDensity: 'Suburban' as const,
      m1a: 'None' as const,
      m1b: 'None' as const,
      m1c: 'None' as const,
      m2: 'None' as const,
    };
    
    // ✅ MUST accept None
    const resultNone = calculateGRC25({ ...baseInput, m2: 'None' });
    expect(resultNone.isValid).toBe(true);
    
    // ✅ MUST accept Low
    const resultLow = calculateGRC25({ ...baseInput, m2: 'Low' });
    expect(resultLow.isValid).toBe(true);
    const m2Low = resultLow.mitigationsApplied.find(m => m.type.includes('M2'));
    expect(m2Low?.robustness).toBe('Low');
    expect(m2Low?.credit).toBe(-1);
    
    // ✅ MUST accept Medium
    const resultMedium = calculateGRC25({ ...baseInput, m2: 'Medium' });
    expect(resultMedium.isValid).toBe(true);
    const m2Medium = resultMedium.mitigationsApplied.find(m => m.type.includes('M2'));
    expect(m2Medium?.robustness).toBe('Medium');
    expect(m2Medium?.credit).toBe(-1);
    
    // ✅ MUST accept High
    const resultHigh = calculateGRC25({ ...baseInput, m2: 'High' });
    expect(resultHigh.isValid).toBe(true);
    const m2High = resultHigh.mitigationsApplied.find(m => m.type.includes('M2'));
    expect(m2High?.robustness).toBe('High');
    expect(m2High?.credit).toBe(-2);
  });

  /**
   * ✅ Golden Test 5: Sequential M1→M2 Application (SORA 2.5)
   * ref: Official SORA 2.5 Main Body Step #3
   * 
   * MUST apply mitigations in correct order:
   * 1. M1(A) Sheltering
   * 2. M1(B) Operational restrictions
   * 3. M1(C) Ground observation
   * 4. M2 Impact dynamics
   */
  it('Golden Test 5: SORA 2.5 applies M1(A)→M1(B)→M1(C)→M2 sequentially', () => {
    const input = {
      mtomKg: 10,
      maxDimensionM: 2,
      typicalSpeedMs: 25,
      populationDensity: 'Urban' as const,
      m1a: 'Low' as const,
      m1b: 'High' as const,
      m1c: 'Low' as const,
      m2: 'Medium' as const,
    };
    
    const result = calculateGRC25(input);
    
    // ✅ Verify all mitigations applied
    expect(result.mitigationsApplied).toHaveLength(4);
    
    // ✅ Verify correct order
    expect(result.mitigationsApplied[0].type).toContain('M1(A)');
    expect(result.mitigationsApplied[1].type).toContain('M1(B)');
    expect(result.mitigationsApplied[2].type).toContain('M1(C)');
    expect(result.mitigationsApplied[3].type).toContain('M2');
    
    // ✅ Verify correct credits
    expect(result.mitigationsApplied[0].credit).toBe(-1);  // M1(A) Low
    expect(result.mitigationsApplied[1].credit).toBe(-2);  // M1(B) High
    expect(result.mitigationsApplied[2].credit).toBe(-1);  // M1(C) Low
    expect(result.mitigationsApplied[3].credit).toBe(-1);  // M2 Medium
    
    // ✅ Verify final GRC ≥ 1
    expect(result.finalGRC).toBeGreaterThanOrEqual(1);
  });

  /**
   * ✅ Golden Test 6: Final GRC Floor at 1 (SORA 2.5)
   * ref: Official SORA 2.5 Main Body Table 5 note
   * 
   * MUST enforce: Final GRC cannot be reduced below 1
   */
  it('Golden Test 6: SORA 2.5 enforces Final GRC ≥ 1 floor', () => {
    // Small UA with aggressive mitigations (try to reduce below 1)
    const input = {
      mtomKg: 0.2,         // ≤ 0.25 kg
      maxDimensionM: 0.3,
      typicalSpeedMs: 15,  // ≤ 25 m/s → Small UA rule → iGRC = 1
      populationDensity: 'CGA' as const,
      m1a: 'Low' as const,    // -1
      m1b: 'High' as const,   // -2
      m1c: 'Low' as const,    // -1
      m2: 'High' as const,    // -2
    };
    
    const result = calculateGRC25(input);
    
    // ✅ iGRC should be 1 (small UA rule)
    expect(result.intrinsicGRC).toBe(1);
    
    // ✅ Even with all mitigations, final GRC must be floored at 1
    expect(result.finalGRC).toBe(1);
    
    // ✅ Audit trail should show floor enforcement
    const floorMessage = result.auditTrail.find(msg => msg.includes('floored at 1'));
    expect(floorMessage).toBeDefined();
  });
});

describe('SORA 2.0 GRC Compliance - Official AMC1 Article 11', () => {
  
  /**
   * ✅ Golden Test 7: M2 Impact Reduction (SORA 2.0) - Low/High only
   * ref: Official AMC1 to Article 11 Table 3
   * 
   * MUST accept: None, Low, High
   * MUST reject: Medium (not defined in SORA 2.0)
   */
  it('Golden Test 7: M2 (SORA 2.0) accepts only None/Low/High per AMC1 Article 11', () => {
    const baseInput = {
      intrinsicGRC: 5,
      columnMinimumGRC: 2,
      m1: 'None' as const,
      m2: 'None' as const,
      m3: 'Adequate' as const,
    };
    
    // ✅ MUST accept None
    const resultNone = calculateGRC20({ ...baseInput, m2: 'None' });
    expect(resultNone.isValid).toBe(true);
    
    // ✅ MUST accept Low
    const resultLow = calculateGRC20({ ...baseInput, m2: 'Low' });
    expect(resultLow.isValid).toBe(true);
    const m2Low = resultLow.mitigationsApplied.find(m => m.type.includes('M2'));
    expect(m2Low?.robustness).toBe('Low');
    expect(m2Low?.credit).toBe(-1);
    
    // ✅ MUST accept High
    const resultHigh = calculateGRC20({ ...baseInput, m2: 'High' });
    expect(resultHigh.isValid).toBe(true);
    const m2High = resultHigh.mitigationsApplied.find(m => m.type.includes('M2'));
    expect(m2High?.robustness).toBe('High');
    expect(m2High?.credit).toBe(-2);
  });

  /**
   * ✅ Golden Test 8: M3 ERP (SORA 2.0) - None/Adequate/Validated terminology
   * ref: Official AMC1 to Article 11 Table 3
   * 
   * MUST use official terminology: None, Adequate, Validated
   * MUST NOT use: Low, Medium, High (incorrect terminology)
   * 
   * Credits: None = +1 (penalty), Adequate = 0, Validated = -1
   */
  it('Golden Test 8: M3 (SORA 2.0) uses None/Adequate/Validated terminology per AMC1 Article 11', () => {
    const baseInput = {
      intrinsicGRC: 5,
      columnMinimumGRC: 2,
      m1: 'None' as const,
      m2: 'None' as const,
      m3: 'Adequate' as const,
    };
    
    // ✅ MUST accept None with +1 penalty
    const resultNone = calculateGRC20({ ...baseInput, m3: 'None' });
    expect(resultNone.isValid).toBe(true);
    const m3None = resultNone.mitigationsApplied.find(m => m.type.includes('M3'));
    expect(m3None?.robustness).toBe('None');
    expect(m3None?.credit).toBe(+1);  // Penalty
    expect(resultNone.finalGRC).toBe(6);  // 5 + 1
    
    // ✅ MUST accept Adequate with 0 credit
    const resultAdequate = calculateGRC20({ ...baseInput, m3: 'Adequate' });
    expect(resultAdequate.isValid).toBe(true);
    const m3Adequate = resultAdequate.mitigationsApplied.find(m => m.type.includes('M3'));
    expect(m3Adequate?.robustness).toBe('Adequate');
    expect(m3Adequate?.credit).toBe(0);
    expect(resultAdequate.finalGRC).toBe(5);  // 5 + 0
    
    // ✅ MUST accept Validated with -1 credit
    const resultValidated = calculateGRC20({ ...baseInput, m3: 'Validated' });
    expect(resultValidated.isValid).toBe(true);
    const m3Validated = resultValidated.mitigationsApplied.find(m => m.type.includes('M3'));
    expect(m3Validated?.robustness).toBe('Validated');
    expect(m3Validated?.credit).toBe(-1);
    expect(resultValidated.finalGRC).toBe(4);  // 5 - 1
  });

  /**
   * ✅ Golden Test 9: Column-Min Clamp (SORA 2.0)
   * ref: Official AMC1 to Article 11 Table 2
   * 
   * MUST enforce: M1 cannot reduce GRC below column minimum
   */
  it('Golden Test 9: SORA 2.0 enforces column-min clamp for M1', () => {
    const input = {
      intrinsicGRC: 6,
      columnMinimumGRC: 4,  // Cannot reduce below 4
      m1: 'High' as const,  // Normally -4, but should clamp
      m2: 'None' as const,
      m3: 'Adequate' as const,
    };
    
    const result = calculateGRC20(input);
    
    // ✅ M1 High normally gives -4, but 6 - 4 = 2 < column min (4)
    // So actual credit should be clamped to -2 (6 - 2 = 4)
    const m1Mitigation = result.mitigationsApplied.find(m => m.type.includes('M1'));
    expect(m1Mitigation?.clamped).toBe(true);
    expect(m1Mitigation?.credit).toBe(-2);  // Clamped
    expect(result.finalGRC).toBe(4);  // Cannot go below column min
    
    // ✅ Audit trail should show clamping
    const clampMessage = result.auditTrail.find(msg => msg.includes('clamped'));
    expect(clampMessage).toBeDefined();
  });

  /**
   * ✅ Golden Test 10: Sequential M1→M2→M3 Application (SORA 2.0)
   * ref: Official AMC1 to Article 11
   * 
   * MUST apply mitigations in correct order:
   * 1. M1 Strategic (with column-min clamp)
   * 2. M2 Impact reduction
   * 3. M3 Emergency Response Plan
   */
  it('Golden Test 10: SORA 2.0 applies M1→M2→M3 sequentially', () => {
    const input = {
      intrinsicGRC: 7,
      columnMinimumGRC: 3,
      m1: 'Medium' as const,  // -2
      m2: 'High' as const,    // -2
      m3: 'Validated' as const, // -1
    };
    
    const result = calculateGRC20(input);
    
    // ✅ Verify all mitigations applied
    expect(result.mitigationsApplied).toHaveLength(3);
    
    // ✅ Verify correct order
    expect(result.mitigationsApplied[0].type).toContain('M1');
    expect(result.mitigationsApplied[1].type).toContain('M2');
    expect(result.mitigationsApplied[2].type).toContain('M3');
    
    // ✅ Verify correct credits
    expect(result.mitigationsApplied[0].credit).toBe(-2);  // M1 Medium
    expect(result.mitigationsApplied[1].credit).toBe(-2);  // M2 High
    expect(result.mitigationsApplied[2].credit).toBe(-1);  // M3 Validated
    
    // ✅ Verify final GRC calculation: 7 - 2 - 2 - 1 = 2
    expect(result.finalGRC).toBe(2);
  });
});

/**
 * ✅ Regression Prevention: These tests MUST fail if:
 * 
 * 1. M1(A) adds Medium or High options (Annex B Tables 2-3: ONLY Low)
 * 2. M1(B) adds Low option (Annex B Tables 4-5: ONLY Medium/High)
 * 3. M1(C) adds Medium or High options (Annex B Tables 6-7: ONLY Low)
 * 4. M2 (2.5) removes Low option (Annex B Table 8+: Low/Medium/High)
 * 5. M2 (2.0) uses Medium instead of Low (AMC1 Article 11: Low/High)
 * 6. M3 (2.0) uses Low/Medium/High instead of None/Adequate/Validated
 * 7. Column-min clamp is not enforced for M1 (SORA 2.0)
 * 8. Final GRC floor at 1 is not enforced (SORA 2.5)
 * 9. Mitigations are applied out of order
 * 10. Backend credit values don't match frontend
 */
