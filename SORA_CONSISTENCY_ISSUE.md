# SORA 2.0 vs 2.5 Consistency Issue

## Problem Statement

Running the **same mission** (Sky Tech SC15, 32kg, Urban, 60m AGL) produces **different results** between SORA 2.0 and SORA 2.5:

### Results Comparison

| Metric | SORA 2.0 | SORA 2.5 | Expected |
|--------|----------|----------|----------|
| **Initial iGRC** | 6 | 7 | Should be SAME (based on drone+population) |
| **Final GRC** | 2 | 5 | Should be close (3-4) |
| **SAIL** | I | II | Should be SAME or I |

## Test Data

**Drone:**
- Model: Sky Tech SC15
- MTOM: 32 kg
- Max Speed: 8 m/s
- Max Dimension: 1.2 m
- Kinetic Energy: 1024 J

**Environment:**
- Population Density: 25,000 people/km² (Urban)
- Airspace: Class G (Uncontrolled)
- Height AGL: 60m
- Typicality: Atypical
- Environment: Urban

**Mitigations SORA 2.0:**
- M1 (Strategic): Medium (-2)
- M2 (Impact reduction): High (-2)
- M3 (ERP): Medium (0)
- **Total: -4**

**Mitigations SORA 2.5:**
- M1(A) (Sheltering): Low (-1)
- M1(B) (Operational): Medium (-1)
- M1(C) (Ground observation): Low (-1)
- M2 (Impact): Medium (-1)
- **Total: -4**

## Issues Identified

### Issue #1: Initial iGRC Mismatch

**SORA 2.0 calculation:**
```
iGRC = 6 (based on "VLOS_Sparsely, 1.2m")
```

**SORA 2.5 calculation:**
```
iGRC = 7 (based on Table 2: 32kg + 25,000 density)
```

**ROOT CAUSE:** SORA 2.0 is using **wrong logic** - it should use the **same Table 2** calculation as SORA 2.5 (MTOM + population density), not "scenario-based" iGRC.

### Issue #2: Final GRC Mismatch

**SORA 2.0:**
```
iGRC = 6
Mitigations = -4
Final GRC = 2 ✅ (correct)
```

**SORA 2.5:**
```
iGRC = 7
Mitigations = -4
Expected Final GRC = 3
Actual Final GRC = 5 ❌ (WRONG!)
```

**ROOT CAUSE:** SORA 2.5 backend is **not applying mitigations correctly** or is applying **additional penalties**.

### Issue #3: SAIL Level Different

**SORA 2.0:** SAIL I (GRC 2 × ARC-a)  
**SORA 2.5:** SAIL II (GRC 5 × ARC-a)

**ROOT CAUSE:** Because Final GRC is wrong (5 instead of 3), SAIL level is escalated unnecessarily.

## Expected Behavior

According to **JARUS SORA 2.5 Main Body Table 2** and **Annex B JAR_doc_27**:

### Initial iGRC Calculation (Both versions)

For **32kg drone** in **25,000 people/km² area**:

1. MTOM = 32 kg → **Category 3** (25-150 kg per Table 2)
2. Population density = 25,000 → **High Density** (>10,000)
3. **Table 2 lookup:** Category 3 + High Density = **iGRC 7**

✅ **SORA 2.5 is CORRECT (iGRC=7)**  
❌ **SORA 2.0 should ALSO be iGRC=7** (not 6)

### Final GRC Calculation

**Both versions should calculate:**
```
iGRC = 7
M1 total = -3 (or -2 depending on mitigation strength)
M2 = -1
Final GRC = 7 - 3 - 1 = 3
```

**Minimum GRC = 1** (per JARUS rules)

### Expected SAIL

With **GRC 3** and **ARC-a**:
- SAIL = **I or II** (depending on exact mitigation robustness)

Both SORA versions should produce **SAIL I or II** (not I vs II).

## Questions for Claude Sonnet

1. **Why is SORA 2.0 calculating iGRC=6 instead of iGRC=7?**
   - Is it using deprecated "scenario-based" iGRC?
   - Should we unify it to use Table 2 like SORA 2.5?

2. **Why is SORA 2.5 Final GRC=5 instead of 3?**
   - Are mitigations being applied incorrectly?
   - Is there a validation bug in the backend?

3. **Should both versions produce the same SAIL level?**
   - Given same inputs, should SORA 2.0 and 2.5 converge?
   - Or is it acceptable for SORA 2.5 to be more conservative?

## Backend Code to Review

**Files:**
- `Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs`
  - Lines 200-400 (GRC calculation for SORA 2.0)
  - Lines 450-650 (GRC calculation for SORA 2.5)
- `Backend_Python/main.py`
  - GRC calculation endpoint `/api/v1/calculate/grc/2.0`
  - GRC calculation endpoint `/api/v1/calculate/grc/2.5`

## Action Required

Please analyze the calculation logic and provide:
1. Fix for SORA 2.0 iGRC calculation (should match SORA 2.5)
2. Fix for SORA 2.5 Final GRC calculation (should be 3, not 5)
3. Explanation of expected differences between versions
4. Code patches for both backends (C# and Python)

---

**Date:** October 29, 2025  
**Reporter:** User (via GitHub Copilot)  
**Priority:** HIGH - Affects calculation accuracy
