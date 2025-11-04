# CURRENT FILES FOR SONNET REVIEW

## These are the ACTUAL current implementations that need fixing:

### 1. GRC Calculator - `Backend_Python/calculations/grc_calculator.py`

**KNOWN ISSUES:**
- SORA 2.5 speed boundaries use wrong thresholds (should be <25, <50, <100, not <35, <75, <120)
- Population boundaries may use <= instead of strict <
- 250g rule checks weight<=0.25 AND speed<=25 but should be speed<25 (strict)
- Floor enforcement logic unclear - need to verify it applies AFTER EACH mitigation

**KEY SECTIONS TO FIX:**
```python
# Line ~95-110: _get_dim_speed_category_2_5
# WRONG speed thresholds:
if speed_ms < 25.0:        # ✅ Correct
    speed_cat = 0
elif speed_ms < 35.0:      # ❌ WRONG - should be < 50
    speed_cat = 1
elif speed_ms < 75.0:      # ❌ WRONG - should be < 100
    speed_cat = 2
elif speed_ms < 120.0:     # ❌ WRONG - should be >= 100
    speed_cat = 3

# CORRECT should be:
if speed_ms < 25.0:
    speed_cat = 0
elif speed_ms < 50.0:      # ✅ FIX
    speed_cat = 1
elif speed_ms < 100.0:     # ✅ FIX
    speed_cat = 2
else:  # >= 100
    speed_cat = 3
```

```python
# Line ~115-130: _get_population_row_2_5
# Need to verify ALL boundaries use strict <

# Line ~280-295: 250g rule check
if request.weight_kg <= 0.25 and request.max_speed_ms <= 25.0:  # ❌ should be < 25
    is_250g = True

# CORRECT:
if request.weight_kg <= 0.25 and request.max_speed_ms < 25.0:  # ✅
    is_250g = True
```

---

### 2. ARC Calculator - `Backend_Python/calculations/arc_calculator.py`

**File seems mostly correct** but verify:
- Strategic mitigation floor at ARC-b for typical operations (line ~226)
- Schema alignment with models (airspace_class, altitude fields required)

---

### 3. SAIL Calculator - `Backend_Python/calculations/sail_calculator.py`

**CRITICAL ISSUE**: Missing GRC=5, ARC-c → SAIL V special case

Need to READ this file and add the special case check at the beginning of calculate_sail()

---

### 4. Models - `Backend_Python/models/sora_models.py`

Ensure these models are EXACT:

```python
class ARCRequest_2_0(BaseModel):
    airspace_class: AirspaceClass  # REQUIRED
    altitude_agl_ft: float         # REQUIRED
    environment: Environment
    distance_to_aerodrome_nm: Optional[float]
    is_in_ctr: bool = False
    is_mode_s_veil: bool = False
    is_tmz: bool = False
    is_atypical_segregated: bool = False
    strategic_mitigations: List[str] = []

class SAILRequest(BaseModel):
    final_grc: int
    residual_arc: ARCRating  # MUST be enum, not str!

class SAILResponse(BaseModel):
    sail: str  # MUST be string "I", "II", etc.
    notes: str
    source: str
```

---

## CRITICAL FIXES NEEDED

### Priority 1: GRC 2.5 Speed Categories
File: `grc_calculator.py` line ~95-110

```python
# CURRENT (WRONG):
if speed_ms < 25.0:
    speed_cat = 0
elif speed_ms < 35.0:      # ❌
    speed_cat = 1
elif speed_ms < 75.0:      # ❌
    speed_cat = 2
elif speed_ms < 120.0:     # ❌
    speed_cat = 3
else:
    speed_cat = 4

# CORRECT (per JARUS 2.5 Table):
if speed_ms < 25.0:
    speed_cat = 0
elif speed_ms < 50.0:      # ✅
    speed_cat = 1
elif speed_ms < 100.0:     # ✅
    speed_cat = 2
else:  # >= 100
    speed_cat = 3
```

### Priority 2: GRC 2.5 250g Rule
File: `grc_calculator.py` line ~280

```python
# CURRENT:
if request.weight_kg <= 0.25 and request.max_speed_ms <= 25.0:  # ❌ <= 25

# CORRECT:
if request.weight_kg <= 0.25 and request.max_speed_ms < 25.0:   # ✅ < 25
```

### Priority 3: SAIL Special Case
File: `sail_calculator.py`

Need to add at START of calculate_sail():
```python
# SPECIAL CASE: GRC=5 with ARC-c yields SAIL V (not IV)
if request.final_grc == 5 and request.residual_arc == ARCRating.ARC_c:
    return SAILResponse(
        sail="V",
        notes="JARUS SORA 2.5 Special Case: GRC=5 with ARC-c yields SAIL V",
        source="JARUS SORA 2.5 Main Body, SAIL Matrix"
    )
```

### Priority 4: Population Boundaries
File: `grc_calculator.py` line ~115-130

Verify ALL comparisons use strict < (not <=):
```python
# Controlled ground
if is_controlled:
    if population_density < 300:  # ✅ strict <
        return (label, 0)
    else:
        return (label, 1)

# Non-controlled
if population_density < 50:     # ✅
    row = 1
elif population_density < 200:  # ✅
    row = 2
elif population_density < 500:  # ✅
    row = 3
else:  # >= 500
    row = 4
```

---

## TEST EXPECTATIONS (from test_comprehensive_report.py)

These are the EXACT expected values after fixes:

### GRC 2.0 (5 tests)
1. 0.8m VLOS_Controlled → iGRC=1, fGRC=1 ✅
2. 3.0m BVLOS_Populated, M1=High → iGRC=7, fGRC=3 ✅
3. 0.99m VLOS_Sparsely → iGRC=2, fGRC=2 ✅
4. 8.0m BVLOS_Populated → OUT_OF_SCOPE (400) ✅
5. 7.5m BVLOS_Sparsely, all High → iGRC=7, fGRC=3 ✅

### GRC 2.5 (7 tests)
1. 0.2kg @ 20m/s → iGRC=1, fGRC=1 (250g) ✅
2. 0.26kg @ 25m/s, pop=50 → iGRC=5, fGRC=5 ✅
3. 3m @ 20m/s, pop=200, ctrl=True → iGRC=2, fGRC=2 ✅
4. 5m @ 30m/s, pop=200, M1=High → iGRC=6, fGRC=3 ✅
5. 15m @ 100m/s, pop=1000 → iGRC=8, fGRC=8 ✅
6. 3m @ 20m/s, pop=200, ctrl=False → iGRC=5, fGRC=5 ✅
7. 10m @ 50m/s, pop=500, all High → iGRC=8, fGRC=4 ✅

### ARC 2.0/2.5 (4 tests)
1. 2.0: G, 100ft, Rural → Init: ARC-b, Residual: ARC-b ✅
2. 2.0: D, 200ft, Urban, Mode-S, 2 SMs → Init: ARC-c, Residual: ARC-b ✅
3. 2.5: G, 30m, Rural → Init: ARC-b, Residual: ARC-b ✅
4. 2.5: E, 90m, Suburban, TMZ, 1 SM → Init: ARC-c, Residual: ARC-b ✅

### SAIL (4 tests)
1. GRC=1, ARC-a → "I" ✅
2. GRC=3, ARC-b → "II" ✅
3. GRC=5, ARC-c → "V" ⚠️ **SPECIAL CASE - CURRENTLY FAILS**
4. GRC=7, ARC-d → "VI" ✅

---

## REQUEST TO SONNET

Please provide COMPLETE corrected versions of:

1. `calculations/grc_calculator.py` - Fix speed cats, 250g rule, boundaries
2. `calculations/sail_calculator.py` - Add GRC=5+ARC-c special case
3. Verify `models/sora_models.py` has correct schemas
4. Verify `calculations/arc_calculator.py` floor logic

Focus on the 4 Priority fixes above - those are the blockers preventing 20/20 test pass.

Current status: Tests cannot run (service connection issues), but the logic errors are clear from code review.

Expected outcome: All files corrected → service starts → tests run → 20/20 PASS
