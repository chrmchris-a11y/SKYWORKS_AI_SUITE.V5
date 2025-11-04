#!/usr/bin/env python3
"""
SKYWORKS Mission Planner - Quick API Test
Tests the fixed case-insensitive enum with real API calls
"""

import requests
import json

API_BASE = "http://localhost:8001"

print("=" * 70)
print("🧪 SKYWORKS MISSION PLANNER - API TEST")
print("Testing case-insensitive MitigationLevel enum fix")
print("=" * 70)

# Test #1: SORA 2.0 with lowercase mitigations (THIS WAS FAILING BEFORE)
print("\n📋 TEST #1: SORA 2.0 - Sky Tech SC15 (32kg)")
print("-" * 70)

test_payload = {
    "mtom_kg": 32.0,
    "population_density": 2000,
    "m1_strategic": "medium",  # lowercase - was causing 422 before
    "m2_impact": "high",       # lowercase - was causing 422 before
    "m3_erp": "medium",        # lowercase - was causing 422 before
    "environment_type": "Urban"
}

print(f"📤 Request: {json.dumps(test_payload, indent=2)}")

try:
    response = requests.post(
        f"{API_BASE}/api/v1/calculate/grc/2.0",
        json=test_payload,
        timeout=10
    )
    
    print(f"\n📥 Response Status: {response.status_code}")
    
    if response.status_code == 200:
        result = response.json()
        print("✅ SUCCESS! API accepted lowercase mitigations!")
        print(f"\n📊 Results:")
        print(f"   Initial GRC: {result['initial_grc']}")
        print(f"   Final GRC: {result['final_grc']}")
        print(f"   Mitigation Total: {result['mitigation_total']}")
        print(f"   Version: {result['version']}")
        
        # Validate expected values
        expected_final_grc = 2  # 32kg, 2000 ppl/km² → initial ~6, mitigations -4 = 2
        if result['final_grc'] == expected_final_grc:
            print(f"\n🎉 CALCULATION CORRECT! Final GRC matches expected ({expected_final_grc})")
        else:
            print(f"\n⚠️  Final GRC {result['final_grc']} differs from expected {expected_final_grc}")
            
    elif response.status_code == 422:
        print("❌ FAILED! Still getting 422 (enum not accepting lowercase)")
        print(f"Error: {response.text}")
    else:
        print(f"❌ ERROR {response.status_code}")
        print(response.text)
        
except requests.exceptions.ConnectionError:
    print("❌ CONNECTION ERROR")
    print("Python FastAPI is not running on port 8001")
    print("Start it with: python -m uvicorn main:app --host 0.0.0.0 --port 8001")
except Exception as e:
    print(f"❌ EXCEPTION: {e}")

# Test #2: SORA 2.5 with special rule
print("\n" + "=" * 70)
print("📋 TEST #2: SORA 2.5 - DJI Mini 2 (Special Rule)")
print("-" * 70)

test_payload_2_5 = {
    "mtom_kg": 0.249,
    "dimension_m": 0.245,
    "speed_ms": 16.0,
    "population_density": 1000,
    "m1a_sheltering": "low",  # lowercase test
    "m2_impact": "medium",    # lowercase test
    "environment_type": "Suburban"
}

print(f"📤 Request: {json.dumps(test_payload_2_5, indent=2)}")

try:
    response = requests.post(
        f"{API_BASE}/api/v1/calculate/grc/2.5",
        json=test_payload_2_5,
        timeout=10
    )
    
    print(f"\n📥 Response Status: {response.status_code}")
    
    if response.status_code == 200:
        result = response.json()
        print("✅ SUCCESS! SORA 2.5 API working!")
        print(f"\n📊 Results:")
        print(f"   Initial GRC: {result['initial_grc']}")
        print(f"   Final GRC: {result['final_grc']}")
        print(f"   Version: {result['version']}")
        
        # Check if special rule was applied
        if result['initial_grc'] == 1:
            print(f"\n🎯 SPECIAL RULE APPLIED! ≤250g + ≤25m/s → iGRC=1")
        
    else:
        print(f"❌ ERROR {response.status_code}")
        print(response.text)
        
except Exception as e:
    print(f"❌ EXCEPTION: {e}")

print("\n" + "=" * 70)
print("🏁 TEST COMPLETE")
print("=" * 70)
