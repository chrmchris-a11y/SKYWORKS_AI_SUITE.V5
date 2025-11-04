"""
GRC Calculation Verification Script
====================================
Comprehensive manual verification of GRC calculations for SORA 2.0 and 2.5.

This script tests 6 critical scenarios:
1. Low population with M3 penalty (SORA 2.0)
2. M1 floor cap enforcement (SORA 2.0) - MOST CRITICAL TEST
3. All mitigations maximum (SORA 2.0)
4. SORA 2.5 N/A validation (should reject)
5. SORA 2.5 quantitative model (basic)
6. Trace completeness verification

Run: python verify_grc_calculations.py
"""

import sys
from pathlib import Path

# Add grc module to path
sys.path.insert(0, str(Path(__file__).parent))

from grc.models.grc_models import GRCInputs20, GRCInputs25
from grc.calculators.grc_calculator import GRCCalculator20, GRCCalculator25

def test_case_1_low_pop_m3_penalty():
    """
    Test Case 1: SORA 2.0 - Low Population, No Mitigations (M3 Penalty)
    Expected:
    - iGRC = 2 (50 people/km² → range [10, 100])
    - M1 = None → 0
    - M2 = None → 0
    - M3 = None → +1 (PENALTY!)
    - Final GRC = 2 + 0 + 0 + 1 = 3
    """
    print("\n" + "="*80)
    print("TEST CASE 1: Low Population + M3 Penalty (SORA 2.0)")
    print("="*80)
    
    calc = GRCCalculator20()
    inputs = GRCInputs20(
        population_density_p_km2=50,
        containment_quality="Good",
        m1_strategic="None",
        m2_impact="None",
        m3_erp="None"  # Should add +1 penalty
    )
    
    result = calc.calculate(inputs)
    
    print(f"Inputs: population=50, M1=None, M2=None, M3=None")
    print(f"Results:")
    print(f"  Initial GRC: {result.initial_grc}")
    print(f"  M1 reduction: {result.m1_reduction}")
    print(f"  M2 reduction: {result.m2_reduction}")
    print(f"  M3 adjustment: {result.m3_adjustment}")
    print(f"  Final GRC: {result.residual_grc}")
    print(f"  Trace entries: {len(result.calculation_trace)}")
    
    # Verify
    assert result.initial_grc == 2, f"Expected iGRC=2, got {result.initial_grc}"
    assert result.m1_reduction == 0, f"Expected M1=0, got {result.m1_reduction}"
    assert result.m2_reduction == 0, f"Expected M2=0, got {result.m2_reduction}"
    assert result.m3_adjustment == 1, f"Expected M3=+1 (penalty), got {result.m3_adjustment}"
    assert result.residual_grc == 3, f"Expected final=3, got {result.residual_grc}"
    
    print("✅ PASSED - M3 penalty working correctly!")
    return True

def test_case_2_m1_floor_cap():
    """
    Test Case 2: SORA 2.0 - M1 Floor Cap (MOST CRITICAL!)
    Expected:
    - iGRC = 5 (1500 people/km² → range [1000, 2500])
    - M1 High = -4
    - Without floor: 5 + (-4) = 1
    - Floor for Good/iGRC=5 = 2
    - After M1 capped: 2
    - M2 High = -2 → 0
    - M3 High = -1 → -1, but floor at 1 → 1
    - Final GRC = 1
    - CRITICAL: M1 reduction should be -3 (not -4) due to floor cap!
    """
    print("\n" + "="*80)
    print("TEST CASE 2: M1 Floor Cap Enforcement (SORA 2.0) - CRITICAL!")
    print("="*80)
    
    calc = GRCCalculator20()
    inputs = GRCInputs20(
        population_density_p_km2=1500,
        containment_quality="Good",
        m1_strategic="High",  # -4 but floor will limit it
        m2_impact="High",
        m3_erp="High"
    )
    
    result = calc.calculate(inputs)
    
    print(f"Inputs: population=1500, containment=Good, M1=High, M2=High, M3=High")
    print(f"Results:")
    print(f"  Initial GRC: {result.initial_grc}")
    print(f"  M1 reduction (nominal=-4, actual after floor): {result.m1_reduction}")
    print(f"  M2 reduction: {result.m2_reduction}")
    print(f"  M3 adjustment: {result.m3_adjustment}")
    print(f"  Final GRC: {result.residual_grc}")
    
    # Check trace for floor cap
    m1_trace = [t for t in result.calculation_trace if "m1" in t.step.lower()][0]
    print(f"\nM1 Trace:")
    print(f"  Step: {m1_trace.step}")
    print(f"  Result: {m1_trace.result}")
    print(f"  Delta: {m1_trace.delta}")
    if m1_trace.notes:
        print(f"  Notes: {m1_trace.notes}")
    
    # Verify
    assert result.initial_grc == 5, f"Expected iGRC=5, got {result.initial_grc}"
    assert result.m1_reduction == -3, f"Expected M1=-3 (floor cap applied), got {result.m1_reduction}"
    assert result.residual_grc >= 1, f"Expected final>=1, got {result.residual_grc}"
    
    # Verify floor cap is mentioned in trace
    floor_mentioned = any("floor" in t.notes.lower() if t.notes else False 
                          for t in result.calculation_trace)
    assert floor_mentioned, "Floor cap not mentioned in trace!"
    
    print("✅ PASSED - M1 floor cap working correctly!")
    return True

def test_case_3_all_mitigations():
    """
    Test Case 3: SORA 2.0 - All Mitigations Maximum
    Expected:
    - iGRC = 6 (3500 people/km²)
    - M1 High = -4, floor for Adequate/iGRC=6 = 4 → after M1: 4
    - M2 High = -2 → 2
    - M3 High = -1 → 1
    - Final GRC = 1
    """
    print("\n" + "="*80)
    print("TEST CASE 3: All Mitigations Maximum (SORA 2.0)")
    print("="*80)
    
    calc = GRCCalculator20()
    inputs = GRCInputs20(
        population_density_p_km2=3500,
        containment_quality="Adequate",
        m1_strategic="High",
        m2_impact="High",
        m3_erp="High"
    )
    
    result = calc.calculate(inputs)
    
    print(f"Inputs: population=3500, containment=Adequate, all High")
    print(f"Results:")
    print(f"  Initial GRC: {result.initial_grc}")
    print(f"  M1 reduction: {result.m1_reduction}")
    print(f"  M2 reduction: {result.m2_reduction}")
    print(f"  M3 adjustment: {result.m3_adjustment}")
    print(f"  Total reduction: {result.total_reduction}")
    print(f"  Final GRC: {result.residual_grc}")
    
    # Verify
    assert result.initial_grc == 6, f"Expected iGRC=6, got {result.initial_grc}"
    assert result.residual_grc == 1, f"Expected final=1, got {result.residual_grc}"
    assert result.m1_reduction <= -2, f"Expected M1<=-2 (with floor), got {result.m1_reduction}"
    assert result.m2_reduction == -2, f"Expected M2=-2, got {result.m2_reduction}"
    assert result.m3_adjustment == -1, f"Expected M3=-1, got {result.m3_adjustment}"
    
    print("✅ PASSED - All mitigations working!")
    return True

def test_case_4_sora25_na_validation():
    """
    Test Case 4: SORA 2.5 - N/A Validation (Should REJECT)
    Expected:
    - M1A High → N/A → Should raise ValidationError
    """
    print("\n" + "="*80)
    print("TEST CASE 4: SORA 2.5 N/A Validation (Should Reject)")
    print("="*80)
    
    calc = GRCCalculator25()
    
    # Test M1A High (N/A)
    print("Testing M1A High (should reject)...")
    try:
        inputs = GRCInputs25(
            characteristic_dimension_m=2.0,
            max_speed_mps=15.0,
            population_density_p_km2=1000,
            containment_quality="Good",
            m1a_sheltering="High",  # N/A!
            m1b_operational="None",
            m1c_ground_observation="None",
            m2_impact="None"
        )
        print("❌ FAILED - Should have rejected M1A High!")
        return False
    except ValueError as e:
        print(f"✅ Correctly rejected: {e}")
    
    # Test M1B Low (N/A)
    print("\nTesting M1B Low (should reject)...")
    try:
        inputs = GRCInputs25(
            characteristic_dimension_m=2.0,
            max_speed_mps=15.0,
            population_density_p_km2=1000,
            containment_quality="Good",
            m1a_sheltering="None",
            m1b_operational="Low",  # N/A!
            m1c_ground_observation="None",
            m2_impact="None"
        )
        print("❌ FAILED - Should have rejected M1B Low!")
        return False
    except ValueError as e:
        print(f"✅ Correctly rejected: {e}")
    
    print("\n✅ PASSED - N/A validation working!")
    return True

def test_case_5_sora25_quantitative():
    """
    Test Case 5: SORA 2.5 - Quantitative Model Basic
    Expected:
    - iGRC calculated from dimension, speed, population
    - M1A Low = -1, M1B Medium = -1, M1C Low = -1
    - M2 High = -2
    - Final GRC >= 1
    """
    print("\n" + "="*80)
    print("TEST CASE 5: SORA 2.5 Quantitative Model (Basic)")
    print("="*80)
    
    calc = GRCCalculator25()
    inputs = GRCInputs25(
        characteristic_dimension_m=1.5,
        max_speed_mps=20.0,
        population_density_p_km2=500,
        containment_quality="Good",
        m1a_sheltering="Low",
        m1b_operational="Medium",
        m1c_ground_observation="Low",
        m2_impact="High"
    )
    
    result = calc.calculate(inputs)
    
    print(f"Inputs: dim=1.5m, speed=20m/s, pop=500, M1A=Low, M1B=Med, M1C=Low, M2=High")
    print(f"Results:")
    print(f"  Initial GRC: {result.initial_grc}")
    print(f"  M1A reduction: {result.m1a_reduction}")
    print(f"  M1B reduction: {result.m1b_reduction}")
    print(f"  M1C reduction: {result.m1c_reduction}")
    print(f"  M2 reduction: {result.m2_reduction}")
    print(f"  Total reduction: {result.total_reduction}")
    print(f"  Final GRC: {result.residual_grc}")
    
    # Verify
    assert 1 <= result.initial_grc <= 8, f"iGRC out of range: {result.initial_grc}"
    assert result.m1a_reduction == -1, f"Expected M1A=-1, got {result.m1a_reduction}"
    assert result.m1b_reduction == -1, f"Expected M1B=-1, got {result.m1b_reduction}"
    assert result.m1c_reduction == -1, f"Expected M1C=-1, got {result.m1c_reduction}"
    assert result.m2_reduction == -2, f"Expected M2=-2, got {result.m2_reduction}"
    assert result.residual_grc >= 1, f"Final GRC must be >=1, got {result.residual_grc}"
    
    print("✅ PASSED - Quantitative model working!")
    return True

def test_case_6_trace_completeness():
    """
    Test Case 6: Trace Completeness
    Expected:
    - All trace entries have doc_ref with doc_id, standard, section
    - Trace shows sequential application (iGRC → M1 → M2 → M3)
    """
    print("\n" + "="*80)
    print("TEST CASE 6: Trace Completeness")
    print("="*80)
    
    calc = GRCCalculator20()
    inputs = GRCInputs20(
        population_density_p_km2=800,
        containment_quality="Good",
        m1_strategic="Medium",
        m2_impact="Low",
        m3_erp="Medium"
    )
    
    result = calc.calculate(inputs)
    
    print(f"Trace entries: {len(result.calculation_trace)}")
    
    # Check each trace entry
    for i, trace in enumerate(result.calculation_trace):
        print(f"\nTrace {i+1}:")
        print(f"  Step: {trace.step}")
        print(f"  Result: {trace.result}")
        print(f"  Rule ref: {trace.rule_ref}")
        print(f"  Doc ref: {trace.doc_ref}")
        
        # Verify doc_ref completeness (convert to dict if Pydantic model)
        doc_ref = trace.doc_ref if isinstance(trace.doc_ref, dict) else trace.doc_ref.model_dump()
        
        assert "doc_id" in doc_ref, f"Trace {i+1} missing doc_id"
        assert "standard" in doc_ref, f"Trace {i+1} missing standard"
        assert "section" in doc_ref, f"Trace {i+1} missing section"
        assert doc_ref["doc_id"], f"Trace {i+1} doc_id is empty"
        assert doc_ref["standard"], f"Trace {i+1} standard is empty"
        assert doc_ref["section"], f"Trace {i+1} section is empty"
    
    # Verify sequential steps present
    steps = [t.step for t in result.calculation_trace]
    assert any("initial" in s.lower() for s in steps), "Missing initial GRC step"
    assert any("m1" in s.lower() for s in steps), "Missing M1 step"
    assert any("m2" in s.lower() for s in steps), "Missing M2 step"
    assert any("m3" in s.lower() for s in steps), "Missing M3 step"
    
    print("\n✅ PASSED - Trace completeness verified!")
    return True

def main():
    """Run all verification tests"""
    print("="*80)
    print("GRC CALCULATION VERIFICATION")
    print("="*80)
    print("\nRunning 6 comprehensive verification tests...")
    
    tests = [
        ("Low Population + M3 Penalty", test_case_1_low_pop_m3_penalty),
        ("M1 Floor Cap (CRITICAL)", test_case_2_m1_floor_cap),
        ("All Mitigations Maximum", test_case_3_all_mitigations),
        ("SORA 2.5 N/A Validation", test_case_4_sora25_na_validation),
        ("SORA 2.5 Quantitative Model", test_case_5_sora25_quantitative),
        ("Trace Completeness", test_case_6_trace_completeness),
    ]
    
    results = []
    for name, test_func in tests:
        try:
            passed = test_func()
            results.append((name, passed))
        except Exception as e:
            print(f"\n❌ FAILED: {e}")
            import traceback
            traceback.print_exc()
            results.append((name, False))
    
    # Summary
    print("\n" + "="*80)
    print("VERIFICATION SUMMARY")
    print("="*80)
    
    passed_count = sum(1 for _, passed in results if passed)
    total_count = len(results)
    
    for name, passed in results:
        status = "✅ PASSED" if passed else "❌ FAILED"
        print(f"{status}: {name}")
    
    print("\n" + "="*80)
    print(f"TOTAL: {passed_count}/{total_count} tests passed")
    
    if passed_count == total_count:
        print("="*80)
        print("🎉 ALL VERIFICATION TESTS PASSED!")
        print("✅ GRC module is working correctly!")
        print("✅ SORA 2.0 (EASA AMC/GM) - Verified: iGRC, M1/M2/M3, floor cap")
        print("✅ SORA 2.5 (JARUS) - Verified: quantitative model, M1A/M1B/M1C/M2, N/A validation")
        print("✅ Ready to proceed with SAIL implementation!")
        print("="*80)
        return 0
    else:
        print("="*80)
        print("❌ SOME TESTS FAILED - Please review and fix!")
        print("="*80)
        return 1

if __name__ == "__main__":
    sys.exit(main())
