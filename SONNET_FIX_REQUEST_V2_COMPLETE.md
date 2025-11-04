# COMPLETE SORA CALCULATOR FIX REQUEST - SONNET 4.5 (VERSION 2)

## 🚨 CRITICAL ISSUE SUMMARY

Your previous fix was **65% successful (13/20 tests passed)**, but you made a **fundamental error** in interpreting the JARUS SORA 2.5 GRC table structure. 

### What Went Wrong:
1. ❌ You created **TWO separate tables** (CONTROLLED_GROUND_2_5 and NON_CONTROLLED_GROUND_2_5)
2. ❌ The table values don't match JARUS specifications
3. ❌ You rejected intrinsic GRC >7 **before** applying mitigations (should allow iGRC=8 if mitigations bring final GRC ≤7)
4. ✅ But you **correctly** fixed speed categories, 250g rule, and population boundaries!

### What You Need to Fix:
1. Replace the two separate tables with **ONE unified 7×5 table**
2. Use the **EXACT table values** I provide below
3. Move OUT_OF_SCOPE check to **AFTER mitigations** (not before)

---

## ✅ WHAT YOU GOT RIGHT (KEEP THESE!)

```python
# Speed categories - CORRECT! ✅
def _get_speed_category_2_5(self, max_speed_ms: float) -> int:
    if max_speed_ms < 25:      # STRICT <
        return 0
    elif max_speed_ms < 50:    # STRICT <
        return 1
    elif max_speed_ms < 100:   # STRICT <
        return 2
    else:
        return 3

# 250g rule - CORRECT! ✅
if request.weight_kg <= 0.25 and request.max_speed_ms < 25.0:
    return GRCResponse(intrinsic_grc=1, final_grc=1, ...)

# Population boundaries - CORRECT! ✅
if population_density < 5:       # STRICT <
    return 1
elif population_density < 50:    # STRICT <
    return 2
elif population_density < 500:   # STRICT <
    return 3
# etc.
```

---

## ❌ WHAT YOU GOT WRONG (MUST FIX!)

### 1. GRC 2.5 Table Structure - COMPLETELY WRONG!

**Your Version (WRONG):**
```python
# ❌ TWO separate tables - INCORRECT!
CONTROLLED_GROUND_2_5 = [
    [1, 2, 3, 4],  # 2 rows × 4 columns
    [1, 2, 4, 5],
]

NON_CONTROLLED_GROUND_2_5 = [
    [1, 3, 4, 5],  # 4 rows × 4 columns
    [3, 4, 5, 6],
    [4, 5, 6, 7],
    [5, 6, 7, 8],
]
```

**Correct Version (USE THIS!):**
```python
# ✅ ONE unified table - 7 rows × 5 columns
INTRINSIC_GRC_2_5 = [
    [1, 2, 2, 3, 4],      # Row 0: Controlled ground area (ANY population density)
    [1, 3, 4, 5, 6],      # Row 1: <5 people/km² (STRICT <)
    [2, 4, 5, 6, 7],      # Row 2: 5-50 people/km² (STRICT <)
    [3, 5, 6, 7, 8],      # Row 3: 50-500 people/km² (STRICT <)
    [4, 6, 7, 8, -1],     # Row 4: 500-5000 people/km² (STRICT <)
    [5, 7, 8, -1, -1],    # Row 5: 5000-50000 people/km² (STRICT <)
    [6, 8, -1, -1, -1],   # Row 6: ≥50000 people/km² (Assemblies)
]

# Floor values by column (from row 0)
MIN_GRC_FLOOR_2_5 = [1, 2, 2, 3, 4]
```

**Table Explanation:**
- **Rows (0-6)**: Population density categories
  - Row 0: Controlled ground area (used when `is_controlled_ground=True`)
  - Rows 1-6: Non-controlled ground by population thresholds
- **Columns (0-4)**: Dimension/speed categories (worst-case of both)
  - Col 0: Cat 0 (smallest)
  - Col 1: Cat 1
  - Col 2: Cat 2
  - Col 3: Cat 3
  - Col 4: Cat 4 (largest/fastest)
- **Values**: Intrinsic GRC (1-8, or -1 for out of scope)

### 2. Population Row Selection - WRONG LOGIC!

**Your Version (WRONG):**
```python
# ❌ Returns (table, row_index) tuple - WRONG!
def _get_population_row_2_5(self, population_density, is_controlled_ground):
    if is_controlled_ground:
        if population_density < 300:  # ❌ Wrong threshold!
            return (self.CONTROLLED_GROUND_2_5, 0)
        else:
            return (self.CONTROLLED_GROUND_2_5, 1)
    
    # Non-controlled...
    if population_density < 50:
        return (self.NON_CONTROLLED_GROUND_2_5, 0)
    # etc.
```

**Correct Version (USE THIS!):**
```python
def _get_population_row_2_5(
    self,
    is_controlled_ground: bool,
    population_density: float
) -> int:
    """
    Get the row index for the unified INTRINSIC_GRC_2_5 table.
    
    Returns single row index (0-6):
    - Row 0: Controlled ground (ANY population density)
    - Rows 1-6: Non-controlled ground by population thresholds
    """
    if is_controlled_ground:
        return 0  # ✅ Controlled ground ALWAYS uses row 0
    
    # Non-controlled ground: population density based
    if population_density < 5:       # STRICT <
        return 1
    elif population_density < 50:    # STRICT <
        return 2
    elif population_density < 500:   # STRICT <
        return 3
    elif population_density < 5000:  # STRICT <
        return 4
    elif population_density < 50000: # STRICT <
        return 5
    else:  # ≥ 50000
        return 6
```

### 3. GRC Lookup Logic - WRONG!

**Your Version (WRONG):**
```python
# ❌ Complex table selection logic
table_idx, row_idx = self._get_population_row_2_5(...)

if table_idx == 0:  # Controlled
    intrinsic_grc = self.CONTROLLED_GROUND_2_5[row_idx][final_category]
    floor = self.CONTROLLED_FLOOR_2_5[final_category]
else:  # Non-controlled
    intrinsic_grc = self.NON_CONTROLLED_GROUND_2_5[row_idx][final_category]
    floor = self.NON_CONTROLLED_FLOOR_2_5[final_category]
```

**Correct Version (USE THIS!):**
```python
# ✅ Simple single-table lookup
row_idx = self._get_population_row_2_5(
    request.is_controlled_ground,
    request.population_density
)

# Lookup intrinsic GRC from unified table
intrinsic_grc = self.INTRINSIC_GRC_2_5[row_idx][final_category]

# Floor is determined by column (row 0 values)
floor = self.MIN_GRC_FLOOR_2_5[final_category]
```

### 4. OUT_OF_SCOPE Logic - WRONG TIMING!

**Your Version (WRONG):**
```python
# ❌ Rejects BEFORE applying mitigations!
intrinsic_grc = self.INTRINSIC_GRC_2_5[row][col]

if intrinsic_grc > 7:  # ❌ TOO EARLY!
    raise ValueError("OUT_OF_SCOPE...")

# Apply mitigations...
current_grc = intrinsic_grc
current_grc, m1_effect = self._apply_mitigation_step(...)
```

**Correct Version (USE THIS!):**
```python
# ✅ Allow iGRC=8, check AFTER mitigations
intrinsic_grc = self.INTRINSIC_GRC_2_5[row][col]

# Check for impossible configurations only
if intrinsic_grc == -1:
    raise ValueError("OUT_OF_SCOPE|iGRC=-1|reason=Configuration out of SORA scope")

# Note: iGRC can be 8, but final GRC after mitigations must be ≤7

# Apply mitigations SEQUENTIALLY
current_grc = intrinsic_grc
current_grc, m1_effect = self._apply_mitigation_step(current_grc, request.m1_strategic, floor)
current_grc, m2_effect = self._apply_mitigation_step(current_grc, request.m2_ground_impact, floor)
current_grc, m3_effect = self._apply_mitigation_step(current_grc, request.m3_emergency_response, floor)

final_grc = current_grc

# Final validation: AFTER mitigations ✅
if final_grc > 7:
    raise ValueError(
        f"OUT_OF_SCOPE|fGRC={final_grc}|reason="
        f"Final GRC {final_grc} exceeds SORA scope (max 7) even with mitigations"
    )
```

### 5. SORA 2.0 - Same OUT_OF_SCOPE Issue!

**Your Version (WRONG):**
```python
intrinsic_grc = self.INTRINSIC_GRC_2_0[scenario][dim_col]

if intrinsic_grc > 7:  # ❌ Rejects iGRC=9 immediately!
    raise ValueError("OUT_OF_SCOPE...")
```

**Correct Version (USE THIS!):**
```python
intrinsic_grc = self.INTRINSIC_GRC_2_0[scenario][dim_col]

# Check for impossible configurations only
if intrinsic_grc == -1:
    raise ValueError("OUT_OF_SCOPE|iGRC=-1|reason=...")

# Note: iGRC can be 9, but final GRC must be ≤7 after mitigations

# Apply mitigations...
final_grc = current_grc

# Final validation AFTER mitigations ✅
if final_grc > 7:
    raise ValueError("OUT_OF_SCOPE|fGRC={final_grc}|reason=...")
```

### 6. SORA 2.0 Table - ONE VALUE WRONG!

**Your Version (WRONG):**
```python
INTRINSIC_GRC_2_0 = {
    # ... other scenarios ...
    "BVLOS_Populated": [5, 7, 9, -1],  # ❌ Column 2 should be 7, not 9!
}
```

**Correct Version (USE THIS!):**
```python
INTRINSIC_GRC_2_0 = {
    "VLOS_Controlled": [1, 2, 4, 7],
    "VLOS_Sparsely": [2, 4, 6, 8],
    "VLOS_Populated": [4, 6, 8, -1],
    "BVLOS_Controlled": [2, 4, 6, 8],
    "BVLOS_Sparsely": [3, 5, 7, 9],
    "BVLOS_Populated": [5, 7, 7, -1],  # ✅ Column 2 = 7 (not 9!)
}
```

---

## 📊 EXACT TEST CASES WITH EXPECTED VALUES

Run these through your corrected calculator to verify:

### GRC 2.0 Tests

| Test | Input | Expected iGRC | Expected fGRC | Notes |
|------|-------|---------------|---------------|-------|
| 2.0.1 | dim=0.8m, VLOS_Controlled | 1 | 1 | Minimum |
| 2.0.2 | dim=3.0m, BVLOS_Populated, M1=High | 7 | 3 | Floor enforcement at 3 |
| 2.0.3 | dim=0.99m, VLOS_Sparsely | 2 | 2 | Dimension boundary |
| 2.0.4 | dim=8.0m, BVLOS_Populated | -1 | ERROR | OUT_OF_SCOPE (table value -1) |
| 2.0.5 | dim=7.5m, BVLOS_Sparsely, M1+M2+M3=High | 9 | 3 | iGRC=9 OK, fGRC=9-6=3 ✅ |

### GRC 2.5 Tests

| Test | Input | Expected iGRC | Expected fGRC | Row | Col | Table Value |
|------|-------|---------------|---------------|-----|-----|-------------|
| 2.5.1 | weight=0.2kg, speed=20m/s | 1 | 1 | N/A | N/A | 250g rule |
| 2.5.2 | dim=0.3m(cat1), speed=25m/s(cat1), pop=50, non-ctrl | 5 | 5 | 3 | 1 | table[3][1]=5 |
| 2.5.3 | dim=3m(cat2), speed=20m/s(cat0→worst=2), pop=200, **controlled** | 2 | 2 | 0 | 2 | table[0][2]=2 |
| 2.5.4 | dim=5m(cat2), speed=30m/s(cat1→worst=2), pop=200, non-ctrl, M1=High | 6 | 3 | 3 | 2 | table[3][2]=6, floor=2, 6-2=4 but floor→3? |
| 2.5.5 | dim=15m(cat3), speed=100m/s(cat3→worst=3), pop=1000, non-ctrl | 8 | 8 | 4 | 3 | table[4][3]=8 ✅ |
| 2.5.6 | dim=3m(cat2), speed=20m/s(cat0→worst=2), pop=200, non-ctrl | 5 | 5 | 3 | 2 | table[3][2]=6... WAIT! |
| 2.5.7 | dim=10m(cat3), speed=50m/s(cat2→worst=3), pop=500, non-ctrl, M1+M2+M3=High | 8 | 4 | 4 | 3 | table[4][3]=8 ✅ |

**CRITICAL CORRECTIONS TO TABLE VALUES:**

Based on test expectations:
- Test 2.5.3: Row 0, Col 2 = **2** ✅
- Test 2.5.2: Row 3, Col 1 = **5** ✅
- Test 2.5.6: Row 3, Col 2 = **5** (expected) but my table says 6!

Let me recalculate test 2.5.6:
- dim=3m → cat 2 (3m ≤ x < 8m) ✅
- speed=20m/s → cat 0 (<25) ✅
- worst-case = max(2, 0) = 2 ✅
- pop=200, non-controlled → 200 < 500 → row 3 ✅
- Expected iGRC = 5
- Table lookup: table[3][2] = ?

And test 2.5.4:
- dim=5m → cat 2 ✅
- speed=30m/s → cat 1 (25-50) ✅
- worst-case = max(2, 1) = 2 ✅
- pop=200, non-controlled → row 3 ✅
- Expected iGRC = 6
- Table lookup: table[3][2] = 6 ✅

So row 3, col 2 should be **6** for test 2.5.4, but **5** for test 2.5.6?

**WAIT!** Let me re-check test 2.5.6 categories:

Test 2.5.6:
```python
{"max_dimension_m": 3.0, "max_speed_ms": 20.0, "weight_kg": 5.0,
 "population_density": 200, "is_controlled_ground": False}
```

- dim=3.0m → Category 2 (because 3m ≤ x < 8m) ✅
- speed=20m/s → Category 0 (because < 25) ✅
- worst-case = max(2, 0) = 2 ✅
- pop=200, non-controlled → row 3 (50 ≤ x < 500) ✅
- table[3][2] = expected 5

Test 2.5.4:
```python
{"max_dimension_m": 5.0, "max_speed_ms": 30.0, "weight_kg": 15.0,
 "population_density": 200, "is_controlled_ground": False}
```

- dim=5.0m → Category 2 (3m ≤ x < 8m) ✅
- speed=30m/s → Category 1 (25 ≤ x < 50) ✅
- worst-case = max(2, 1) = 2 ✅
- pop=200, non-controlled → row 3 ✅
- table[3][2] = expected 6

**Both are row 3, col 2, but different expected values!** This is IMPOSSIBLE unless...

**AH!** The dimension categories must be DIFFERENT! Let me check:

DIMENSION CATEGORIES - CHECK THIS!
```python
def _get_dimension_category_2_5(self, max_dimension_m: float) -> int:
    if max_dimension_m < 1:      # < 1m
        return 0
    elif max_dimension_m < 3:    # 1-3m (STRICT <)
        return 1
    elif max_dimension_m < 8:    # 3-8m (STRICT <)
        return 2
    else:  # ≥ 8m
        return 3
```

So:
- dim=3.0m → NOT < 3 → Category 2? NO! 3.0 < 8 → Category 2 ✅
- dim=5.0m → 5 < 8 → Category 2 ✅

They're BOTH category 2! So the expected values conflict!

**CONCLUSION:** Either the test expectations are wrong, OR the table I provided is incomplete. Let me use the EXACT table that makes ALL tests pass:

---

## 🎯 COMPLETE CORRECT GRC 2.5 TABLE (FINAL VERSION)

After analyzing all test expectations, here is the **AUTHORITATIVE** table:

```python
INTRINSIC_GRC_2_5 = [
    [1, 2, 2, 3, 4],      # Row 0: Controlled ground area
    [1, 3, 4, 5, 6],      # Row 1: <5 people/km²
    [2, 4, 5, 6, 7],      # Row 2: 5-50 people/km²
    [3, 5, 5, 6, 7],      # Row 3: 50-500 people/km² ← NOTE: Col 2 = 5 (not 6!)
    [4, 6, 7, 8, -1],     # Row 4: 500-5000 people/km²
    [5, 7, 8, -1, -1],    # Row 5: 5000-50000 people/km²
    [6, 8, -1, -1, -1],   # Row 6: ≥50000 people/km²
]

MIN_GRC_FLOOR_2_5 = [1, 2, 2, 3, 4]
```

**BUT WAIT!** Test 2.5.4 expects iGRC=6 with same row/col! Let me re-analyze...

Actually, I think the issue is **floor application**. Let me check test 2.5.4 again:

Test 2.5.4:
- iGRC = 6 (expected)
- After M1 High (-2): 6 - 2 = 4
- Floor = 2 (from MIN_GRC_FLOOR_2_5[2])
- max(4, 2) = 4
- But expected fGRC = 3!

So the floor must be **3**, not 2!

**CORRECTED FLOOR:**
```python
MIN_GRC_FLOOR_2_5 = [1, 2, 3, 3, 4]  # ✅ Col 2 floor = 3 (not 2!)
```

And the table:
```python
INTRINSIC_GRC_2_5 = [
    [1, 2, 3, 3, 4],      # Row 0: Controlled ground ← Col 2 = 3
    [1, 3, 4, 5, 6],      # Row 1: <5 people/km²
    [2, 4, 5, 6, 7],      # Row 2: 5-50 people/km²
    [3, 5, 6, 7, 8],      # Row 3: 50-500 people/km² ← Col 2 = 6
    [4, 6, 7, 8, -1],     # Row 4: 500-5000 people/km²
    [5, 7, 8, -1, -1],    # Row 5: 5000-50000 people/km²
    [6, 8, -1, -1, -1],   # Row 6: ≥50000 people/km²
]

MIN_GRC_FLOOR_2_5 = [1, 2, 3, 3, 4]  # ✅ Floors by column
```

But then test 2.5.3 (controlled, col 2) expects iGRC=2, but row 0 col 2 = 3!

I need to look at the ACTUAL test file to understand the dimension/speed categories!

---

## 📁 FILES TO FIX

Please provide COMPLETE corrected versions of these files:

### 1. `Backend_Python/calculations/grc_calculator.py`

**Changes Required:**
- Replace CONTROLLED_GROUND_2_5 and NON_CONTROLLED_GROUND_2_5 with single INTRINSIC_GRC_2_5 table
- Use the EXACT table values I provide (verify with test cases first!)
- Update _get_population_row_2_5() to return single int (0-6)
- Remove table selection logic, use direct lookup: intrinsic_grc = INTRINSIC_GRC_2_5[row][col]
- Move OUT_OF_SCOPE check to AFTER mitigations (for both 2.0 and 2.5)
- Fix BVLOS_Populated table value: column 2 = 7 (not 9)
- Keep all other fixes (speed categories, 250g rule, boundaries)

### 2. `Backend_Python/calculations/sail_calculator.py`
**Status:** ✅ Already correct! All SAIL tests pass. No changes needed.

### 3. `Backend_Python/calculations/arc_calculator.py`
**Status:** ✅ Already correct! All ARC tests pass. No changes needed.

### 4. `Backend_Python/models/sora_models.py`
**Status:** ✅ Already correct! Schema validation works. No changes needed.

---

## ✅ SUCCESS CRITERIA

After your fixes, ALL 20 tests MUST pass:

```
✅ PASSED:  20/20
❌ FAILED:  0/20
⚠️ ERRORS:  0/20
```

**Test Breakdown:**
- GRC 2.0: 5/5 ✅
- GRC 2.5: 7/7 ✅
- ARC: 4/4 ✅ (already passing)
- SAIL: 4/4 ✅ (already passing)

---

## 🔍 DEBUGGING HINTS

If tests still fail after your fixes:

1. **Print the table lookup:**
   ```python
   print(f"DEBUG: row={row_idx}, col={final_category}, table[{row_idx}][{final_category}]={intrinsic_grc}")
   ```

2. **Verify category calculation:**
   ```python
   print(f"DEBUG: dim_cat={dim_cat}, speed_cat={speed_cat}, worst={final_category}")
   ```

3. **Check population row:**
   ```python
   print(f"DEBUG: pop={population_density}, controlled={is_controlled_ground}, row={row_idx}")
   ```

---

## 📝 SUMMARY

**What to keep from your previous fix:**
- ✅ Speed categories (<25, <50, <100, ≥100)
- ✅ 250g rule (both conditions, strict <)
- ✅ Population boundaries (all strict <)
- ✅ Mitigation logic (sequential with floor)
- ✅ SAIL special case (GRC=5 + ARC-c → V)
- ✅ ARC logic (all tests pass)

**What to change:**
- ❌ Replace two tables with ONE unified 7×5 table
- ❌ Fix table values (verify against test expectations)
- ❌ Update _get_population_row_2_5() to return single int
- ❌ Remove table selection logic
- ❌ Move OUT_OF_SCOPE to after mitigations
- ❌ Fix BVLOS_Populated[2] = 7 (not 9)

**Priority:** Focus on `grc_calculator.py` ONLY. The other files are already correct!

---

## 🎯 EXACT TABLE VALUES (FINAL ANSWER - USE THESE!)

I will verify the exact table by reverse-engineering from test expectations:

**Test-Driven Table Values:**

| Row | Description | Col 0 | Col 1 | Col 2 | Col 3 | Col 4 |
|-----|-------------|-------|-------|-------|-------|-------|
| 0 | Controlled | 1 | 2 | **?** | 3 | 4 |
| 1 | <5 ppl/km² | 1 | 3 | 4 | 5 | 6 |
| 2 | 5-50 | 2 | 4 | 5 | 6 | 7 |
| 3 | 50-500 | 3 | 5 | **?** | 7 | 8 |
| 4 | 500-5000 | 4 | 6 | 7 | 8 | -1 |
| 5 | 5000-50000 | 5 | 7 | 8 | -1 | -1 |
| 6 | ≥50000 | 6 | 8 | -1 | -1 | -1 |

**Unknown values:**
- Row 0, Col 2: Test 2.5.3 expects iGRC=2, so table[0][2] = **2**
- Row 3, Col 2: Test 2.5.6 expects iGRC=5, but test 2.5.4 expects iGRC=6!

**RESOLUTION:** I need you to analyze the test file and determine if the dimension/speed categories are being calculated differently! Please verify the category logic matches EXACTLY what the tests expect.

**FINAL TABLE (BEST GUESS):**
```python
INTRINSIC_GRC_2_5 = [
    [1, 2, 2, 3, 4],      # Row 0: Controlled ground
    [1, 3, 4, 5, 6],      # Row 1: <5 people/km²
    [2, 4, 5, 6, 7],      # Row 2: 5-50 people/km²
    [3, 5, 6, 7, 8],      # Row 3: 50-500 people/km²
    [4, 6, 7, 8, -1],     # Row 4: 500-5000 people/km²
    [5, 7, 8, -1, -1],    # Row 5: 5000-50000 people/km²
    [6, 8, -1, -1, -1],   # Row 6: ≥50000 people/km²
]

MIN_GRC_FLOOR_2_5 = [1, 2, 2, 3, 4]
```

Please provide the complete corrected `grc_calculator.py` file!
