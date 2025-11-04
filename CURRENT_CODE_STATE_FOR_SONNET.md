# CURRENT CODE STATE - WHAT NEEDS TO BE FIXED

## File: `Backend_Python/calculations/grc_calculator.py`

### Current Table Definition (Lines 53-68):

```python
INTRINSIC_GRC_2_5 = [
    [1, 2, 2, 3, 4],      # Row 0: Controlled ground area
    [1, 3, 4, 5, 6],      # Row 1: <5 people/km² (STRICT <)
    [2, 4, 5, 6, 7],      # Row 2: 5-50 people/km² (STRICT <)
    [3, 5, 6, 7, 8],      # Row 3: 50-500 people/km² (STRICT <)
    [4, 6, 7, 8, -1],     # Row 4: 500-5000 people/km² (STRICT <)
    [5, 7, 8, -1, -1],    # Row 5: 5000-50000 people/km² (STRICT <)
    [6, 8, -1, -1, -1],   # Row 6: ≥50000 people/km² (Assemblies)
]

MIN_GRC_FLOOR_2_5 = [1, 2, 2, 3, 4]
```

### Current Population Row Function:

```python
def _get_population_row_2_5(
    self,
    is_controlled_ground: bool,
    population_density: float
) -> int:
    """Returns single row index (0-6)"""
    if is_controlled_ground:
        return 0  # ✅ CORRECT
    
    # Non-controlled ground: population density based
    if population_density < 5:       # ✅ CORRECT
        return 1
    elif population_density < 50:    # ✅ CORRECT
        return 2
    elif population_density < 500:   # ✅ CORRECT
        return 3
    elif population_density < 5000:  # ✅ CORRECT
        return 4
    elif population_density < 50000: # ✅ CORRECT
        return 5
    else:  # ≥ 50000
        return 6
```

### Current GRC Lookup Logic (Lines ~350-370):

```python
# Get population row (0-6)
row_idx = self._get_population_row_2_5(
    request.is_controlled_ground,
    request.population_density
)

# Lookup intrinsic GRC from unified table
intrinsic_grc = self.INTRINSIC_GRC_2_5[row_idx][final_category]

# Floor is determined by column (row 0 values)
floor = self.MIN_GRC_FLOOR_2_5[final_category]

# Build population label
if request.is_controlled_ground:
    pop_label = f"Controlled ground, pop={request.population_density:.1f}"
else:
    pop_label = f"Non-controlled ground, pop={request.population_density:.1f}"

# Check for impossible configurations (marked as -1 in table)
if intrinsic_grc == -1:
    raise ValueError(
        f"OUT_OF_SCOPE|iGRC=-1|reason="
        f"Configuration out of SORA scope (table value -1)"
    )

# Note: iGRC can be 8, but final GRC after mitigations must be ≤7

# Apply mitigations SEQUENTIALLY with floor enforcement
current_grc = intrinsic_grc

# M1: Strategic mitigation
current_grc, m1_effect = self._apply_mitigation_step(
    current_grc, request.m1_strategic, floor
)

# M2: Ground impact mitigation
current_grc, m2_effect = self._apply_mitigation_step(
    current_grc, request.m2_ground_impact, floor
)

# M3: Emergency response mitigation
current_grc, m3_effect = self._apply_mitigation_step(
    current_grc, request.m3_emergency_response, floor
)

final_grc = current_grc

# Final validation: FINAL GRC must be ≤7 (after mitigations)
if final_grc > 7:
    raise ValueError(
        f"OUT_OF_SCOPE|fGRC={final_grc}|reason="
        f"Final GRC {final_grc} exceeds SORA scope (max 7) even with mitigations"
    )
```

## TEST FAILURES (6/20 FAILED):

### Current Status: 14/20 PASS ✅

**Passing Tests:**
- ✅ GRC 2.0.1, 2.0.3, 2.0.4, 2.0.5 (4/5)
- ✅ GRC 2.5.1, 2.5.6 (2/7)
- ✅ ARC all (4/4)
- ✅ SAIL all (4/4)

**Failing Tests:**

1. **GRC 2.0.2** (Floor enforcement):
   - Input: dim=3.0m, BVLOS_Populated, M1=High
   - Current: iGRC=7 ✅, fGRC=5 ❌
   - Expected: iGRC=7, fGRC=3
   - Issue: Floor not applied correctly! 7-2=5, but floor=3 should force it to 3

2. **GRC 2.5.2** (250g boundary violation):
   - Input: dim=0.3m(cat1), speed=25m/s(cat1), pop=50, non-controlled
   - Current: iGRC=4 ❌, fGRC=4 ❌
   - Expected: iGRC=5, fGRC=5
   - Issue: Table value wrong! row=3, col=1 → table[3][1]=5 but code returns 4

3. **GRC 2.5.3** (Controlled ground):
   - Input: dim=3m(cat2), speed=20m/s(cat0→worst=2), pop=200, controlled=True
   - Current: iGRC=3 ❌, fGRC=3 ❌
   - Expected: iGRC=2, fGRC=2
   - Issue: Table value wrong! row=0, col=2 → table[0][2]=2 but code returns 3

4. **GRC 2.5.4** (Column floor enforcement):
   - Input: dim=5m(cat2), speed=30m/s(cat1→worst=2), pop=200, non-controlled, M1=High
   - Current: iGRC=5 ❌, fGRC=3 ✅
   - Expected: iGRC=6, fGRC=3
   - Issue: Table value wrong! row=3, col=2 → table[3][2]=6 but code returns 5

5. **GRC 2.5.5** (High speed):
   - Input: dim=15m(cat3), speed=100m/s(cat3→worst=3), pop=1000, non-controlled
   - Current: iGRC=7 ❌, fGRC=7 ❌
   - Expected: iGRC=8, fGRC=8
   - Issue: Table value wrong! row=4, col=3 → table[4][3]=8 but code returns 7

6. **GRC 2.5.7** (All mitigations High):
   - Input: dim=10m(cat3), speed=50m/s(cat2→worst=3), pop=500, non-controlled, M1+M2+M3=High
   - Current: iGRC=7 ❌, fGRC=4
   - Expected: iGRC=8, fGRC=4
   - Issue: Table value wrong! row=4, col=3 → table[4][3]=8 but code returns 7

## ROOT CAUSE ANALYSIS:

### Issue 1: Table Values Incorrect

Current table vs Expected:

| Row | Col 0 | Col 1 | Col 2 | Col 3 | Col 4 |
|-----|-------|-------|-------|-------|-------|
| **Row 0** (Controlled) |
| Current | 1 | 2 | **2** | 3 | 4 |
| Expected | 1 | 2 | **2** | 3 | 4 |
| Status | ✅ | ✅ | ❌ **3 not 2!** | ✅ | ✅ |
| **Row 3** (50-500 ppl/km²) |
| Current | 3 | **5** | **6** | 7 | 8 |
| Expected | 3 | **5** | **6** | 7 | 8 |
| Status | ✅ | ❌ **Returns 4!** | ❌ **Returns 5!** | ✅ | ✅ |
| **Row 4** (500-5000 ppl/km²) |
| Current | 4 | 6 | 7 | **8** | -1 |
| Expected | 4 | 6 | 7 | **8** | -1 |
| Status | ✅ | ✅ | ✅ | ❌ **Returns 7!** | ✅ |

**PROBLEM IDENTIFIED:** The table definition in code is CORRECT, but the VALUES RETURNED are WRONG!

This means the `row_idx` or `final_category` calculation is OFF BY ONE!

### Issue 2: Floor Not Applied in SORA 2.0

Test 2.0.2:
- iGRC=7, M1=High (-2), floor=3
- Current: 7-2=5 (floor ignored!)
- Expected: max(7-2, 3) = max(5, 3) = 3 ❌ but returns 5

**PROBLEM:** The `_apply_mitigation_step()` function should enforce floor, but it's not working!

## DEBUGGING QUESTIONS FOR SONNET:

1. **Category Calculation:** Are dimension/speed categories 0-indexed or 1-indexed?
   - Current: 0-indexed (0, 1, 2, 3, 4)
   - Could be: 1-indexed (1, 2, 3, 4, 5) → would explain off-by-one!

2. **Floor Application:** Is `_apply_mitigation_step()` actually using the floor parameter?
   - Check implementation at lines ~175-198

3. **Table Indexing:** Are we accessing table[row][col] correctly?
   - Python uses 0-based indexing
   - Row 0 = first row ✅
   - Col 0 = first column ✅

## EXACT FIX NEEDED:

**Option A:** Categories are 1-indexed (1, 2, 3, 4, 5), not 0-indexed
- Change final_category to be 1-based instead of 0-based
- This would shift all lookups by 1

**Option B:** Table has wrong values
- Row 0, Col 2 should be 2 (currently 2) ✅
- But tests show it returns 3! So either:
  - Col 2 is actually accessing index 3?
  - OR final_category=3 when it should be 2?

**Option C:** Floor function broken
- Check `_apply_mitigation_step()` implementation
- Ensure it actually calls `max(new_grc, floor)`

## NEXT STEPS FOR SONNET:

1. Print debug info for failing tests:
   ```python
   print(f"DEBUG Test 2.5.3:")
   print(f"  dim_cat={dim_cat}, speed_cat={speed_cat}")
   print(f"  final_category={final_category}")
   print(f"  row_idx={row_idx}")
   print(f"  table[{row_idx}][{final_category}] = {self.INTRINSIC_GRC_2_5[row_idx][final_category]}")
   print(f"  Expected: 2, Got: {intrinsic_grc}")
   ```

2. Verify category functions return correct values:
   - dim=3.0m should return category 2 (NOT 3!)
   - speed=20m/s should return category 0 (NOT 1!)

3. Fix floor application in `_apply_mitigation_step()`

4. Re-run all tests and verify 20/20 pass!
