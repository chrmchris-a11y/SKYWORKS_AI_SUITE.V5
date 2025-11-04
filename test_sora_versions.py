#!/usr/bin/env python3
"""
SKYWORKS AI SUITE - SORA Version-Specific Tests
Tests both SORA 2.0 and JARUS SORA 2.5 implementations
"""

import sys
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'Backend_Python'))

from main import SORAv2_0_Table2, JARUSSORAv2_5_Table2

def test_sora_2_0():
    """Test SORA 2.0 MTOM-based calculations"""
    print("=== SORA 2.0 Tests ===")
    
    tests = [
        # (test_name, mtom_kg, pop_density, expected_igrc)
        ("Micro drone, controlled area", 0.1, 1, 1),
        ("Small drone, sparsely populated", 0.8, 500, 2),
        ("Small drone, suburban", 0.8, 8000, 4),  # This was failing before
        ("Medium drone, high density", 5.0, 15000, 6),
        ("Large drone, controlled area", 30.0, 1, 3),
    ]
    
    passed = 0
    for test_name, mtom_kg, pop_density, expected in tests:
        result = SORAv2_0_Table2.calculate_initial_grc(mtom_kg, pop_density)
        actual = result['initial_grc']
        
        if actual == expected:
            print(f"✅ {test_name}: iGRC={actual}")
            passed += 1
        else:
            print(f"❌ {test_name}: Expected {expected}, got {actual}")
    
    print(f"SORA 2.0: {passed}/{len(tests)} tests passed\n")
    return passed == len(tests)

def test_sora_2_5():
    """Test JARUS SORA 2.5 Dimension+Speed-based calculations"""
    print("=== JARUS SORA 2.5 Tests ===")
    
    tests = [
        # (test_name, dimension_m, speed_ms, pop_density, mtom_kg, expected_igrc)
        ("Micro drone, special rule", 0.5, 20, 1000, 0.2, 1),  # ≤250g + ≤25m/s override
        ("Small drone, remote area", 0.8, 25, 3, None, 2),     # Small, Row 1
        ("Small drone, suburban", 0.8, 25, 8000, None, 6),     # Small (Col 0), Row 5 (≥5k) → 6
        ("Medium drone, lightly populated", 2.5, 30, 25, None, 4),  # Medium, Row 2
        ("Large drone, sparsely populated", 6.0, 60, 200, None, 6),  # Large, Row 3
        ("Very large drone, remote", 15.0, 100, 3, None, 5),   # VeryLarge, Row 1
        ("Extra large drone, suburban", 25.0, 150, 2000, None, 9),  # ExtraLarge, Row 4
    ]
    
    passed = 0
    for test_name, dim_m, speed_ms, pop_density, mtom_kg, expected in tests:
        result = JARUSSORAv2_5_Table2.calculate_initial_grc(dim_m, speed_ms, pop_density, mtom_kg)
        
        if result.get("error"):
            print(f"❌ {test_name}: {result['error']}")
            continue
            
        actual = result['initial_grc']
        
        if actual == expected:
            print(f"✅ {test_name}: iGRC={actual}")
            passed += 1
        else:
            print(f"❌ {test_name}: Expected {expected}, got {actual}")
            print(f"   Debug: {result.get('debug_info', {})}")
    
    print(f"JARUS SORA 2.5: {passed}/{len(tests)} tests passed\n")
    return passed == len(tests)

def test_sora_2_5_edge_cases():
    """Test JARUS SORA 2.5 edge cases and special rules"""
    print("=== JARUS SORA 2.5 Edge Cases ===")
    
    # Test grey cells (out of scope)
    result = JARUSSORAv2_5_Table2.calculate_initial_grc(10.0, 80.0, 60000, None)
    if result.get("error") == "OUT_OF_SORA_2_5_SCOPE":
        print("✅ Grey cell detection: Out of scope correctly identified")
        edge_passed = 1
    else:
        print("❌ Grey cell detection failed")
        edge_passed = 0
    
    # Test boundary conditions
    boundary_tests = [
        ("Boundary: 1m, 25m/s", 1.0, 25.0, 1000, None, 5),    # Exactly Small
        ("Boundary: 3m, 35m/s", 3.0, 35.0, 1000, None, 6),    # Exactly Medium
        ("Boundary: 3m, 36m/s", 3.0, 36.0, 1000, None, 7),    # Just over Medium → Large
    ]
    
    for test_name, dim_m, speed_ms, pop_density, mtom_kg, expected in boundary_tests:
        result = JARUSSORAv2_5_Table2.calculate_initial_grc(dim_m, speed_ms, pop_density, mtom_kg)
        actual = result['initial_grc']
        
        if actual == expected:
            print(f"✅ {test_name}: iGRC={actual}")
            edge_passed += 1
        else:
            print(f"❌ {test_name}: Expected {expected}, got {actual}")
    
    print(f"Edge cases: {edge_passed}/4 tests passed\n")
    return edge_passed == 4

if __name__ == "__main__":
    print("╔═══════════════════════════════════════════════════════════════════════════╗")
    print("  SKYWORKS AI SUITE - SORA VERSION TESTS")
    print("  Testing both SORA 2.0 and JARUS SORA 2.5 implementations")
    print("╚═══════════════════════════════════════════════════════════════════════════╝\n")
    
    sora_2_0_pass = test_sora_2_0()
    sora_2_5_pass = test_sora_2_5()
    edge_cases_pass = test_sora_2_5_edge_cases()
    
    print("="*79)
    if sora_2_0_pass and sora_2_5_pass and edge_cases_pass:
        print("🎉 ALL TESTS PASSED! Both SORA versions working correctly.")
        print("✅ SORA 2.0: MTOM-based calculations validated")
        print("✅ JARUS SORA 2.5: Dimension+Speed-based calculations validated")
        print("✅ Edge cases: Special rules and boundaries verified")
        sys.exit(0)
    else:
        print("❌ Some tests failed. Implementation needed:")
        if not sora_2_0_pass:
            print("   - SORA 2.0 tests failed (rename JARUSSORATable2 → SORAv2_0_Table2)")
        if not sora_2_5_pass:
            print("   - JARUS SORA 2.5 tests failed (implement JARUSSORAv2_5_Table2)")
        if not edge_cases_pass:
            print("   - Edge cases failed (implement special rules and grey cell detection)")
        sys.exit(1)
