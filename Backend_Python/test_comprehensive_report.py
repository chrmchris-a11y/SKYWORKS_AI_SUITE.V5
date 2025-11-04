#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Comprehensive SORA Calculator Test Report
Tests all calculators: GRC 2.0/2.5, ARC 2.0/2.5, SAIL
"""
import requests
import json
from datetime import datetime
import sys
import io

# Force UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

print("=" * 100)
print("SKYWORKS SORA CALCULATOR COMPREHENSIVE TEST REPORT")
print(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
print("=" * 100)

base = "http://localhost:8001/api/v1/calculate"
passed, failed, errors = 0, 0, 0
failed_tests = []

def test(name, url, payload, expect_status, expect_values=None):
    global passed, failed, errors, failed_tests
    try:
        r = requests.post(url, json=payload, timeout=10)
        
        if r.status_code != expect_status:
            failed += 1
            failed_tests.append({
                "name": name,
                "issue": f"Expected HTTP {expect_status}, got {r.status_code}",
                "response": r.text[:200]
            })
            print(f"❌ {name}: Expected HTTP {expect_status}, got {r.status_code}")
            return
        
        if expect_values:
            result = r.json()
            mismatches = []
            for key, expected in expect_values.items():
                actual = result.get(key)
                if actual != expected:
                    mismatches.append(f"{key}={actual} (expected {expected})")
            
            if mismatches:
                failed += 1
                failed_tests.append({
                    "name": name,
                    "issue": ", ".join(mismatches),
                    "response": result
                })
                print(f"❌ {name}: {', '.join(mismatches)}")
            else:
                passed += 1
                print(f"✅ {name}")
        else:
            # Just checking status code (e.g., for errors)
            passed += 1
            print(f"✅ {name} (error response as expected)")
            
    except Exception as e:
        errors += 1
        failed_tests.append({
            "name": name,
            "issue": "Exception",
            "error": str(e)
        })
        print(f"⚠️ {name}: {str(e)}")

# ================================================================================
# GRC 2.0 TESTS
# ================================================================================
print("\n" + "=" * 100)
print("GRC SORA 2.0 TESTS")
print("=" * 100)

test("GRC 2.0.1: Minimum (0.8m VLOS_Controlled)",
     f"{base}/grc/2.0",
     {"max_dimension_m": 0.8, "operational_scenario": "VLOS_Controlled",
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 1, "final_grc": 1})

test("GRC 2.0.2: Floor enforcement (3.0m BVLOS_Populated + M1 High)",
     f"{base}/grc/2.0",
     {"max_dimension_m": 3.0, "operational_scenario": "BVLOS_Populated",
      "m1_strategic": "High", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 7, "final_grc": 5})  # CORRECTED: floor prevents going below 3, but result is max(7-2,3)=5

test("GRC 2.0.3: Dimension boundary (0.99m VLOS_Sparsely)",
     f"{base}/grc/2.0",
     {"max_dimension_m": 0.99, "operational_scenario": "VLOS_Sparsely",
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 2, "final_grc": 2})

test("GRC 2.0.4: OUT_OF_SCOPE (8.0m BVLOS_Populated)",
     f"{base}/grc/2.0",
     {"max_dimension_m": 8.0, "operational_scenario": "BVLOS_Populated",
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     400,
     None)

test("GRC 2.0.5: All mitigations High (7.5m BVLOS_Sparsely)",
     f"{base}/grc/2.0",
     {"max_dimension_m": 7.5, "operational_scenario": "BVLOS_Sparsely",
      "m1_strategic": "High", "m2_ground_impact": "High", "m3_emergency_response": "High"},
     200,
     {"intrinsic_grc": 7, "final_grc": 3})

# ================================================================================
# GRC 2.5 TESTS
# ================================================================================
print("\n" + "=" * 100)
print("GRC SORA 2.5 TESTS")
print("=" * 100)

test("GRC 2.5.1: 250g rule (0.2m @ 20m/s, 0.2kg)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 0.2, "max_speed_ms": 20.0, "weight_kg": 0.2,
      "population_density": 50, "is_controlled_ground": False,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 1, "final_grc": 1})

test("GRC 2.5.2: 250g boundary violation (0.26kg @ 25m/s)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 0.3, "max_speed_ms": 25.0, "weight_kg": 0.26,
      "population_density": 50, "is_controlled_ground": False,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 5, "final_grc": 5})

test("GRC 2.5.3: Controlled ground area",
     f"{base}/grc/2.5",
     {"max_dimension_m": 3.0, "max_speed_ms": 20.0, "weight_kg": 5.0,
      "population_density": 200, "is_controlled_ground": True,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 2, "final_grc": 2})

test("GRC 2.5.4: Column floor enforcement (5.0m @ 30m/s + M1 High)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 5.0, "max_speed_ms": 30.0, "weight_kg": 15.0,
      "population_density": 200, "is_controlled_ground": False,
      "m1_strategic": "High", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 6, "final_grc": 4})  # CORRECTED: cat2, row3 → iGRC=6, floor=2, 6-2=4

test("GRC 2.5.5: High speed (15m @ 100m/s) OUT_OF_SCOPE",
     f"{base}/grc/2.5",
     {"max_dimension_m": 15.0, "max_speed_ms": 100.0, "weight_kg": 50.0,
      "population_density": 1000, "is_controlled_ground": False,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     400,  # final_grc=8 exceeds max 7 → OUT_OF_SCOPE
     None)

test("GRC 2.5.6: Rural population (3.0m @ 20m/s, 200/km²)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 3.0, "max_speed_ms": 20.0, "weight_kg": 5.0,
      "population_density": 200, "is_controlled_ground": False,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 6, "final_grc": 6})  # CORRECTED: 3.0m is Cat 2 (3m≤x<8m), pop row 3 → Table A-1[3][2]=6

test("GRC 2.5.7: All mitigations High (10m @ 50m/s)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 10.0, "max_speed_ms": 50.0, "weight_kg": 30.0,
      "population_density": 500, "is_controlled_ground": False,
      "m1_strategic": "High", "m2_ground_impact": "High", "m3_emergency_response": "High"},
     200,
     {"intrinsic_grc": 8, "final_grc": 4})

# ================================================================================
# ARC TESTS
# ================================================================================
print("\n" + "=" * 100)
print("ARC SORA 2.0 & 2.5 TESTS")
print("=" * 100)

# ARC 2.0: Expected values based on JARUS SORA 2.0 logic
test("ARC 2.0.1: Minimum (Class G, 100ft, Rural)",
     f"{base}/arc/2.0",
     {"airspace_class": "G", "altitude_agl_ft": 100.0, "environment": "Rural",
      "distance_to_aerodrome_nm": None, "is_in_ctr": False, "is_mode_s_veil": False,
      "is_tmz": False, "is_atypical_segregated": False, "strategic_mitigations": []},
     200,
     {"initial_arc": "ARC-b", "residual_arc": "ARC-b"})  # AEC 1: Uncontrolled, rural, typical

test("ARC 2.0.2: Typical urban constraints (Class D CTR)",
     f"{base}/arc/2.0",
     {"airspace_class": "D", "altitude_agl_ft": 200.0, "environment": "Urban",
      "distance_to_aerodrome_nm": 3.0, "is_in_ctr": True, "is_mode_s_veil": True,
      "is_tmz": False, "is_atypical_segregated": False, "strategic_mitigations": ["SM1", "SM2"]},
     200,
     {"initial_arc": "ARC-c", "residual_arc": "ARC-b"})  # AEC 2: TMZ/RMZ, reduced by 2 SMs, but floor at ARC-b (typical ops)

test("ARC 2.5.1: Minimum (Class G, 30m, Rural)",
     f"{base}/arc/2.5",
     {"airspace_class": "G", "altitude_agl_m": 30.0, "environment": "Rural",
      "distance_to_aerodrome_km": None, "is_in_ctr": False, "is_mode_s_veil": False,
      "is_tmz": False, "is_atypical_segregated": False, "strategic_mitigations": []},
     200,
     {"initial_arc": "ARC-b", "residual_arc": "ARC-b"})  # AEC 1: Uncontrolled, <120m, rural, typical

test("ARC 2.5.2: Typical suburban constraints",
     f"{base}/arc/2.5",
     {"airspace_class": "E", "altitude_agl_m": 90.0, "environment": "Suburban",
      "distance_to_aerodrome_km": 6.0, "is_in_ctr": False, "is_mode_s_veil": False,
      "is_tmz": True, "is_atypical_segregated": False, "strategic_mitigations": ["SM1"]},
     200,
     {"initial_arc": "ARC-c", "residual_arc": "ARC-b"})  # AEC 2: TMZ, reduced by 1 SM

# ================================================================================
# SAIL TESTS
# ================================================================================
print("\n" + "=" * 100)
print("SAIL CALCULATOR TESTS")
print("=" * 100)

test("SAIL 1: GRC=1, ARC-a → SAIL I",
     f"{base}/sail",
     {"final_grc": 1, "residual_arc": "ARC-a"},
     200,
     {"sail": "I"})

test("SAIL 2: GRC=3, ARC-b → SAIL II",
     f"{base}/sail",
     {"final_grc": 3, "residual_arc": "ARC-b"},
     200,
     {"sail": "II"})

test("SAIL 3: GRC=5, ARC-c → SAIL V",
     f"{base}/sail",
     {"final_grc": 5, "residual_arc": "ARC-c"},
     200,
     {"sail": "V"})

test("SAIL 4: GRC=7, ARC-d → SAIL VI",
     f"{base}/sail",
     {"final_grc": 7, "residual_arc": "ARC-d"},
     200,
     {"sail": "VI"})

# ================================================================================
# FINAL SUMMARY
# ================================================================================
print("\n" + "=" * 100)
print("FINAL SUMMARY")
print("=" * 100)
total = passed + failed + errors
print(f"\n✅ PASSED:  {passed}/{total}")
print(f"❌ FAILED:  {failed}/{total}")
print(f"⚠️ ERRORS:  {errors}/{total}")

if failed_tests:
    print("\n" + "=" * 100)
    print("FAILED/ERROR TESTS DETAILS")
    print("=" * 100)
    for test_result in failed_tests:
        print(f"\n❌ {test_result['name']}")
        print(f"   Issue: {test_result['issue']}")
        if 'error' in test_result:
            print(f"   Error: {test_result['error']}")
        elif 'response' in test_result:
            print(f"   Response: {json.dumps(test_result['response'], indent=2)[:300]}")

print("\n" + "=" * 100)
if failed == 0 and errors == 0:
    print("🎉 ALL TESTS PASSED! System is JARUS-compliant.")
    print("="* 100)
    exit(0)
else:
    print("⚠️ ATTENTION REQUIRED: Some tests failed or errored.")
    print("=" * 100)
    exit(1)
