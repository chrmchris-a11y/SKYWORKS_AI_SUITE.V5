# CRITICAL: SORA 2.0 vs 2.5 Calculation Inconsistency - URGENT FIX REQUIRED

## Executive Summary

**PROBLEM:** Running the **SAME mission** with **SAME inputs** produces **DRASTICALLY DIFFERENT results** between SORA 2.0 and SORA 2.5. This violates JARUS SORA principles where methodology updates should maintain consistency.

**IMPACT:** Users cannot trust calculations. Regulatory compliance at risk.

**PRIORITY:** 🔴 CRITICAL - Fix required immediately

---

## Test Case: Identical Mission Parameters

### Mission Configuration
```json
{
  "drone": {
    "model": "Sky Tech SC15",
    "mtom_kg": 32,
    "max_speed_ms": 8,
    "max_dimension_m": 1.2,
    "kinetic_energy_j": 1024
  },
  "environment": {
    "population_density": 25000,
    "population_descriptor": "< 50,000 people/km² - High Density Metropolitan",
    "environment_type": "Urban",
    "airspace_class": "G",
    "typicality": "Atypical",
    "max_height_agl_m": 60,
    "controlled_ground_area": false
  },
  "mitigations_sora20": {
    "M1_strategic": "Medium (-2)",
    "M2_impact": "High (-2)",
    "M3_erp": "Medium (0)"
  },
  "mitigations_sora25": {
    "M1A_sheltering": "Low (-1)",
    "M1B_operational": "Medium (-1)",
    "M1C_ground_observation": "Low (-1)",
    "M2_impact": "Medium (-1)"
  }
}
```

---

## Actual Results (BROKEN)

| Metric | SORA 2.0 | SORA 2.5 | ✅ Expected | ❌ Status |
|--------|----------|----------|------------|-----------|
| **Initial iGRC** | 6 | 7 | **7** (both) | ❌ SORA 2.0 WRONG |
| **Mitigation Total** | -4 | -4 | -4 (both) | ✅ Correct |
| **Final GRC** | 2 | 5 | **3** (both) | ❌ BOTH WRONG |
| **Initial ARC** | ARC-b | ARC-b | ARC-b (both) | ✅ Correct |
| **Residual ARC** | ARC-a | ARC-a | ARC-a (both) | ✅ Correct |
| **SAIL Level** | I | II | **I** (both) | ❌ SORA 2.5 WRONG |
| **OSOs Required** | 15/24 | 11/17 | Different OK | ✅ Expected |

---

## Detailed Analysis

### 🔴 BUG #1: SORA 2.0 Initial iGRC Calculation

**Current Behavior:**
```
SORA 2.0 frontend shows: "iGRC = 3 (VLOS_Sparsely, 1.2m)"
SORA 2.0 backend calculates: iGRC = 6
```

**Root Cause:** SORA 2.0 is using **deprecated scenario-based iGRC** logic instead of **JARUS SORA 2.5 Table 2** unified approach.

**Expected Behavior:**
```
MTOM = 32 kg → Category 3 (25-150 kg per Table 2)
Population = 25,000 → "Controlled ground area > 1 person/km²" → High density
Table 2 Lookup: Category 3 × High Density = iGRC 7
```

**AUTHORITATIVE REFERENCE:**
- **JARUS SORA 2.5 Main Body (JAR_doc_25)** - Table 2, Page 24
- **JARUS SORA 2.0 (JAR_doc_06)** - Table 2 (should be same!)

### 🔴 BUG #2: SORA 2.5 Final GRC Calculation

**Current Behavior:**
```
iGRC = 7 ✅
Mitigations applied = -4 ✅
Final GRC shown = 5 ❌ (WRONG!)
```

**Expected Calculation:**
```
iGRC = 7
M1(A) Low = -1
M1(B) Medium = -1  
M1(C) Low = -1
M2 Medium = -1
─────────────
Final GRC = 7 - 1 - 1 - 1 - 1 = 3 ✅
```

**Root Cause:** Backend is either:
1. Not applying all mitigations
2. Applying additional penalties
3. Has validation logic overriding calculation

**CRITICAL FILES TO CHECK:**
- `Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs` (lines 450-650)
- `Backend_Python/main.py` - GRC 2.5 endpoint
- `Backend_Python/models/sora_models.py` - Mitigation application logic

### 🔴 BUG #3: SAIL Level Mismatch

**Current:**
- SORA 2.0: GRC 2 × ARC-a = SAIL I ✅
- SORA 2.5: GRC 5 × ARC-a = SAIL II ❌

**Expected:**
- SORA 2.0: GRC 3 × ARC-a = SAIL I ✅
- SORA 2.5: GRC 3 × ARC-a = SAIL I ✅

**JARUS SORA 2.5 Table 1 (SAIL Matrix):**
```
       ARC-a  ARC-b  ARC-c  ARC-d
GRC 1    I      I      II     III
GRC 2    I      II     II     III
GRC 3    I      II     III    IV    ← Expected for both versions
GRC 4    II     II     III    IV
GRC 5    II     III    IV     IV    ← Current SORA 2.5 (WRONG)
```

---

## Root Cause Analysis

### Python Backend (Backend_Python/main.py)

**Suspected Issues:**
1. **GRC 2.0 calculation** may use old logic instead of Table 2
2. **GRC 2.5 mitigation application** may have bugs
3. **Validation rules** may override correct calculations

**Code to Review:**
```python
# Backend_Python/main.py

@app.post("/api/v1/calculate/grc/2.0")
async def calculate_grc_2_0(request: GRCRequest_2_0):
    # CHECK: Is this using Table 2 or scenario-based iGRC?
    # SHOULD USE: Table 2 (MTOM + population density)
    pass

@app.post("/api/v1/calculate/grc/2.5")
async def calculate_grc_2_5(request: GRCRequest_2_5):
    # CHECK: Are all mitigations being applied?
    # SHOULD: iGRC - M1A - M1B - M1C - M2 = Final GRC
    pass
```

### C# Backend (SORAOrchestrationService.cs)

**Suspected Issues:**
1. Passing wrong parameters to Python backend
2. Post-processing results incorrectly
3. Validation logic overriding Python calculations

---

## JARUS SORA Authoritative Tables

### Table 2: Initial GRC (Both SORA 2.0 and 2.5)

**Source:** JAR_doc_25 Page 24, JAR_doc_27 Annex B

| MTOM Category | Controlled (≤1 p/km²) | Sparsely (≤10k p/km²) | Moderately (≤10k p/km²) | High Density (>10k p/km²) |
|---------------|----------------------|----------------------|------------------------|--------------------------|
| **Cat 0** (<0.25kg) | 1 | 1 | 1 | 2 |
| **Cat 1** (0.25-1kg) | 2 | 2 | 3 | 4 |
| **Cat 2** (1-25kg) | 3 | 4 | 5 | 6 |
| **Cat 3** (25-150kg) | 4 | 5 | 6 | **7** ← **32kg, 25k density** |
| **Cat 4** (>150kg) | 5 | 6 | 7 | 8 |

**For our test case:**
- MTOM = 32 kg → **Category 3**
- Population = 25,000 → **High Density**
- **Initial iGRC = 7** ✅

### Table 5: Mitigations (SORA 2.5)

**Source:** JAR_doc_25 Table 5, JAR_doc_27 Annex B

| Mitigation | Robustness | GRC Reduction |
|------------|------------|---------------|
| M1(A) - Sheltering | Low | -1 |
| M1(A) - Sheltering | Medium | -2 |
| M1(B) - Operational restrictions | Low | 0 |
| M1(B) - Operational restrictions | Medium | -1 |
| M1(C) - Ground observation | Low | -1 |
| M2 - Impact dynamics | Low | 0 |
| M2 - Impact dynamics | Medium | -1 |

**Applied in our test:**
```
M1(A) Low: -1
M1(B) Medium: -1
M1(C) Low: -1
M2 Medium: -1
Total: -4 ✅
```

### Table 3: Mitigations (SORA 2.0)

**Source:** JAR_doc_06 Table 3

| Mitigation | Robustness | GRC Reduction |
|------------|------------|---------------|
| M1 - Strategic | Low | -1 |
| M1 - Strategic | Medium | -2 |
| M1 - Strategic | High | -2 (limited by column min) |
| M2 - Impact reduction | Low | 0 |
| M2 - Impact reduction | Medium | -1 |
| M2 - Impact reduction | High | -2 |
| M3 - ERP | Low/Medium/High | 0 |

**Applied in our test:**
```
M1 Medium: -2
M2 High: -2
M3 Medium: 0
Total: -4 ✅
```

---

## Expected vs Actual Calculations

### ✅ SORA 2.0 (CORRECTED)

```
Step 1: Initial iGRC
  MTOM 32kg + Population 25k → Table 2 → iGRC = 7 ✅

Step 2: Apply Mitigations
  M1 Medium = -2
  M2 High = -2
  M3 Medium = 0
  Total = -4

Step 3: Final GRC
  7 - 4 = 3 ✅
  Minimum GRC = 1 (not applied, 3 > 1)
  Final GRC = 3 ✅

Step 4: SAIL Determination
  GRC 3 × ARC-a → Table 1 → SAIL I ✅
```

### ✅ SORA 2.5 (CORRECTED)

```
Step 1: Initial iGRC
  MTOM 32kg + Population 25k → Table 2 → iGRC = 7 ✅

Step 2: Apply Mitigations
  M1(A) Low = -1
  M1(B) Medium = -1
  M1(C) Low = -1
  M2 Medium = -1
  Total = -4

Step 3: Final GRC
  7 - 1 - 1 - 1 - 1 = 3 ✅
  Minimum GRC = 1 (not applied, 3 > 1)
  Final GRC = 3 ✅

Step 4: SAIL Determination
  GRC 3 × ARC-a → Table 1 → SAIL I ✅
```

---

## Required Fixes

### Fix #1: SORA 2.0 Initial iGRC Calculation

**Location:** `Backend_Python/main.py` - `/api/v1/calculate/grc/2.0` endpoint

**Change:**
```python
# BEFORE (scenario-based logic - WRONG)
if scenario == "VLOS_Sparsely":
    igrc = calculate_scenario_grc(dimension)  # Returns 3-6

# AFTER (Table 2 unified approach - CORRECT)
def calculate_igrc_table2(mtom_kg: float, population_density: int) -> int:
    """Calculate iGRC using JARUS SORA Table 2 (unified for 2.0 and 2.5)"""
    # Determine MTOM category
    if mtom_kg < 0.25:
        category = 0
    elif mtom_kg < 1:
        category = 1
    elif mtom_kg < 25:
        category = 2
    elif mtom_kg < 150:
        category = 3  # 32kg falls here
    else:
        category = 4
    
    # Determine population density category
    if population_density <= 1:
        density_col = 0  # Controlled
    elif population_density <= 500:
        density_col = 1  # Sparsely
    elif population_density <= 10000:
        density_col = 2  # Moderately
    else:
        density_col = 3  # High density (25,000 falls here)
    
    # Table 2 lookup
    table2 = [
        [1, 1, 1, 2],  # Cat 0
        [2, 2, 3, 4],  # Cat 1
        [3, 4, 5, 6],  # Cat 2
        [4, 5, 6, 7],  # Cat 3 ← Our case
        [5, 6, 7, 8]   # Cat 4
    ]
    
    return table2[category][density_col]

# Use this for BOTH SORA 2.0 and 2.5
igrc = calculate_igrc_table2(mtom_kg, population_density)
```

### Fix #2: SORA 2.5 Final GRC Calculation

**Location:** `Backend_Python/main.py` - `/api/v1/calculate/grc/2.5` endpoint

**Verify:**
```python
@app.post("/api/v1/calculate/grc/2.5")
async def calculate_grc_2_5(request: GRCRequest_2_5):
    # Step 1: Get iGRC
    igrc = calculate_igrc_table2(request.mtom_kg, request.population_density)
    
    # Step 2: Apply mitigations (CHECK THIS LOGIC!)
    final_grc = igrc
    final_grc += request.m1a_mitigation  # Should be -1, -2, or 0
    final_grc += request.m1b_mitigation  # Should be -1 or 0
    final_grc += request.m1c_mitigation  # Should be -1 or 0
    final_grc += request.m2_mitigation   # Should be -1 or 0
    
    # Step 3: Apply minimum (GRC cannot be less than 1)
    final_grc = max(1, final_grc)
    
    # CRITICAL: Ensure no additional penalties are added!
    # CHECK: Are there validation rules overriding this?
    
    return {
        "initial_grc": igrc,
        "final_grc": final_grc,
        "mitigations_applied": {
            "m1a": request.m1a_mitigation,
            "m1b": request.m1b_mitigation,
            "m1c": request.m1c_mitigation,
            "m2": request.m2_mitigation
        }
    }
```

### Fix #3: C# Backend Request Building

**Location:** `Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs`

**Verify:**
```csharp
// Ensure C# sends correct parameters to Python
var grcRequest = new PythonGRCRequest_2_5
{
    MTOM_kg = drone.MTOM_kg,  // 32
    PopulationDensity = environment.PopulationDensity,  // 25000
    M1A_Mitigation = ConvertMitigationValue(input.M1A),  // -1
    M1B_Mitigation = ConvertMitigationValue(input.M1B),  // -1
    M1C_Mitigation = ConvertMitigationValue(input.M1C),  // -1
    M2_Mitigation = ConvertMitigationValue(input.M2)     // -1
};

// CHECK: Are mitigation values correctly mapped?
// Low = -1, Medium = -2 (for M1A), Medium = -1 (for M1B, M2)
```

---

## Validation Rules to Check

### ⚠️ CRITICAL: Do NOT override calculations with validation

**BAD (Don't do this):**
```python
# WRONG: Overriding calculated GRC
if environment == "Urban" and final_grc < 5:
    final_grc = 5  # Force higher GRC for urban ❌ WRONG!
```

**GOOD (Correct approach):**
```python
# CORRECT: Warn user but don't override
if environment == "Urban" and final_grc < 3:
    warnings.append("Urban environment - verify mitigations are adequate")
# But keep final_grc as calculated ✅
```

---

## Testing Instructions

### Test Case 1: Basic Calculation

**Input:**
```json
{
  "mtom_kg": 32,
  "population_density": 25000,
  "m1a": -1,
  "m1b": -1,
  "m1c": -1,
  "m2": -1
}
```

**Expected Output (BOTH SORA 2.0 and 2.5):**
```json
{
  "initial_grc": 7,
  "final_grc": 3,
  "sail": "I"
}
```

### Test Case 2: Minimum GRC Clamping

**Input:**
```json
{
  "mtom_kg": 0.2,
  "population_density": 100,
  "m1a": -2,
  "m1b": -1,
  "m1c": -1,
  "m2": -1
}
```

**Expected Output:**
```json
{
  "initial_grc": 1,
  "final_grc": 1,
  "comment": "GRC cannot be less than 1 (clamped)"
}
```

---

## Key Points for Claude Sonnet 4

### 🚨 CRITICAL REQUIREMENTS

1. **CONSISTENCY:** SORA 2.0 and SORA 2.5 **MUST use the same Table 2** for initial iGRC
2. **NO OVERRIDES:** Validation rules should **WARN** but **NOT override** calculated GRC
3. **TRANSPARENCY:** Show all calculation steps in response for debugging
4. **MINIMUM GRC:** Both versions apply `max(1, calculated_grc)` rule

### 📊 Expected Behavior

For **identical inputs** (same drone, same environment):
- Initial iGRC should be **IDENTICAL**
- Final GRC should be **NEARLY IDENTICAL** (±1 max due to mitigation differences)
- SAIL level should be **SAME or ±1 level**

### 🎯 Success Criteria

After fixes, this test must pass:
```
SORA 2.0: iGRC=7, Final GRC=3, SAIL=I ✅
SORA 2.5: iGRC=7, Final GRC=3, SAIL=I ✅
```

---

## Files to Modify

1. `Backend_Python/main.py` - GRC calculation endpoints
2. `Backend_Python/models/sora_models.py` - Request/response models
3. `Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs` - C# orchestration
4. `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs` - Python client

---

## Budget

Estimated cost for Sonnet analysis + fix: **$0.15 - $0.20**  
Current budget remaining: **$17.51**  
Authorization: ✅ **APPROVED - CRITICAL FIX**

---

**Generated:** October 29, 2025  
**Reporter:** User via GitHub Copilot  
**Status:** 🔴 CRITICAL - Awaiting Claude Sonnet 4 analysis
