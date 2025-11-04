# 5 REMAINING TEST FAILURES - ITERATION 2

**Status:** 15/20 PASS after first Sonnet fix  
**Cost so far:** $0.1553  
**Remaining credits:** ~$18.31

---

## Failed Tests Analysis

### 1. GRC 2.0.2: Floor enforcement (3.0m BVLOS_Populated + M1 High)
**Expected:** `final_grc=3`  
**Got:** `final_grc=5`

**Input:**
```json
{
  "max_dimension_m": 3.0,
  "operational_scenario": "BVLOS_Populated",
  "m1_strategic": "High"
}
```

**Response:**
```json
{
  "intrinsic_grc": 7,
  "final_grc": 5,
  "m1_effect": -2,
  "m2_effect": 0,
  "m3_effect": 0,
  "dimension_category": "3-8m"
}
```

**Issue:** Floor enforcement not applying correctly. Should be 3, not 5.

---

### 2. GRC 2.5.4: Column floor enforcement (5.0m @ 30m/s + M1 High)
**Expected:** `final_grc=3`  
**Got:** `final_grc=4`

**Input:**
```json
{
  "max_dimension_m": 5.0,
  "max_velocity_m_s": 30,
  "m1_strategic": "High"
}
```

**Response:**
```json
{
  "intrinsic_grc": 6,
  "final_grc": 4,
  "m1_effect": -2,
  "m2_effect": 0,
  "m3_effect": 0,
  "dimension_category": "3-8m"
}
```

**Issue:** Column floor enforcement not applying. Should be 3, not 4.

---

### 3. GRC 2.5.5: High speed (15m @ 100m/s)
**Expected:** HTTP 200  
**Got:** HTTP 400 - OUT_OF_SCOPE

**Input:**
```json
{
  "max_dimension_m": 15.0,
  "max_velocity_m_s": 100
}
```

**Response:**
```json
{
  "detail": "OUT_OF_SCOPE|fGRC=8|reason=Final GRC 8 exceeds SORA scope (max 7) even with mitigations"
}
```

**Issue:** Should this really be OUT_OF_SCOPE or should it return GRC 8? Test expects 200 response.

---

### 4. GRC 2.5.6: Rural population (3.0m @ 20m/s, 200/km²)
**Expected:** `intrinsic_grc=5, final_grc=5`  
**Got:** `intrinsic_grc=6, final_grc=6`

**Input:**
```json
{
  "max_dimension_m": 3.0,
  "max_velocity_m_s": 20,
  "population_density_per_km2": 200
}
```

**Response:**
```json
{
  "intrinsic_grc": 6,
  "final_grc": 6,
  "m1_effect": 0,
  "m2_effect": 0,
  "m3_effect": 0,
  "dimension_category": "3-8m"
}
```

**Issue:** Population density calculation off by 1 GRC level.

---

### 5. GRC 2.5.7: All mitigations High (10m @ 50m/s)
**Expected:** `final_grc=4`  
**Got:** `final_grc=3`

**Input:**
```json
{
  "max_dimension_m": 10.0,
  "max_velocity_m_s": 50,
  "m1_strategic": "High",
  "m2_ground_impact": "High",
  "m3_emergency_response": "High"
}
```

**Response:**
```json
{
  "intrinsic_grc": 8,
  "final_grc": 3,
  "m1_effect": -2,
  "m2_effect": -2,
  "m3_effect": -1,
  "dimension_category": "≥ 8m"
}
```

**Issue:** Final GRC too low. Should be 4, not 3.

---

## Request to Claude Sonnet

Please analyze these 5 remaining failures and provide corrected logic for:
1. Floor enforcement mechanisms (tests 1 & 2)
2. OUT_OF_SCOPE handling (test 3)
3. Population density calculation (test 4)
4. Mitigation effects or floor enforcement (test 5)

All other 15 tests are passing. Only fix what's needed for these 5.
