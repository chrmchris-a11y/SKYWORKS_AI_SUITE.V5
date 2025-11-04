# LATEST TEST RESULTS - 14/20 PASSING

Generated: 2025-10-29 10:05:35

## Summary
- ✅ PASSED: 14/20
- ❌ FAILED: 6/20
- ⚠️ ERRORS: 0/20

## Detailed Results

### GRC SORA 2.0 Tests (4/5 PASS)

✅ GRC 2.0.1: Minimum (0.8m VLOS_Controlled)
❌ GRC 2.0.2: Floor enforcement (3.0m BVLOS_Populated + M1 High)
   - Expected: iGRC=7, fGRC=3
   - Got: iGRC=7, fGRC=5
   - Issue: Floor not enforced! 7-2=5 but floor=3 should make it 3

✅ GRC 2.0.3: Dimension boundary (0.99m VLOS_Sparsely)
✅ GRC 2.0.4: OUT_OF_SCOPE (8.0m BVLOS_Populated)
✅ GRC 2.0.5: All mitigations High (7.5m BVLOS_Sparsely)

### GRC SORA 2.5 Tests (2/7 PASS)

✅ GRC 2.5.1: 250g rule (0.2m @ 20m/s, 0.2kg)

❌ GRC 2.5.2: 250g boundary violation (0.26kg @ 25m/s)
   - Expected: iGRC=5, fGRC=5
   - Got: iGRC=4, fGRC=4
   - Issue: row=3, col=1 → table[3][1] should be 5

❌ GRC 2.5.3: Controlled ground area
   - Expected: iGRC=2, fGRC=2
   - Got: iGRC=3, fGRC=3
   - Issue: row=0, col=2 → table[0][2] should be 2 but returns 3

❌ GRC 2.5.4: Column floor enforcement (5.0m @ 30m/s + M1 High)
   - Expected: iGRC=6, fGRC=3
   - Got: iGRC=5, fGRC=3
   - Issue: row=3, col=2 → table[3][2] should be 6

❌ GRC 2.5.5: High speed (15m @ 100m/s)
   - Expected: iGRC=8, fGRC=8
   - Got: iGRC=7, fGRC=7
   - Issue: row=4, col=3 → table[4][3] should be 8

✅ GRC 2.5.6: Rural population (3.0m @ 20m/s, 200/km²)

❌ GRC 2.5.7: All mitigations High (10m @ 50m/s)
   - Expected: iGRC=8, fGRC=4
   - Got: iGRC=7, fGRC=4
   - Issue: row=4, col=3 → table[4][3] should be 8

### ARC SORA 2.0 & 2.5 Tests (4/4 PASS) ✅

✅ ARC 2.0.1: Minimum (Class G, 100ft, Rural)
✅ ARC 2.0.2: Typical urban constraints (Class D CTR)
✅ ARC 2.5.1: Minimum (Class G, 30m, Rural)
✅ ARC 2.5.2: Typical suburban constraints

### SAIL Calculator Tests (4/4 PASS) ✅

✅ SAIL 1: GRC=1, ARC-a → SAIL I
✅ SAIL 2: GRC=3, ARC-b → SAIL II
✅ SAIL 3: GRC=5, ARC-c → SAIL V
✅ SAIL 4: GRC=7, ARC-d → SAIL VI

## Root Cause Analysis

### Issue 1: GRC 2.5 Table Values Wrong
The table definition LOOKS correct but returns wrong values:
- Row 0, Col 2: Returns 3, should be 2
- Row 3, Col 1: Returns 4, should be 5
- Row 3, Col 2: Returns 5, should be 6
- Row 4, Col 3: Returns 7, should be 8

**Hypothesis:** Category calculation is off by one!
- Either dimension/speed categories are 1-indexed (1,2,3,4,5) instead of 0-indexed (0,1,2,3,4)
- OR table lookup is using wrong index

### Issue 2: GRC 2.0 Floor Not Enforced
Test 2.0.2: 7 - 2 = 5, but floor=3 should make it max(5,3)=3
- Current result: 5 (floor ignored!)
- Expected: 3 (floor applied)

**Hypothesis:** `_apply_mitigation_step()` not using floor parameter correctly

## What Sonnet Needs to Fix

1. **Debug category calculation:**
   - Verify `_get_dimension_category_2_5()` returns 0-indexed values (0,1,2,3,4)
   - Verify `_get_speed_category_2_5()` returns 0-indexed values (0,1,2,3)
   - Add debug prints to confirm

2. **Check table lookup:**
   - Verify `INTRINSIC_GRC_2_5[row][col]` uses correct indices
   - Add debug print: `print(f"table[{row}][{col}] = {value}")`

3. **Fix floor application:**
   - Check `_apply_mitigation_step()` implementation
   - Ensure it calls `max(new_grc, floor)`

4. **Verify final table values:**
   ```python
   INTRINSIC_GRC_2_5 = [
       [1, 2, 2, 3, 4],      # Row 0: Controlled (col 2 MUST be 2!)
       [1, 3, 4, 5, 6],      # Row 1
       [2, 4, 5, 6, 7],      # Row 2
       [3, 5, 6, 7, 8],      # Row 3 (col 1=5, col 2=6!)
       [4, 6, 7, 8, -1],     # Row 4 (col 3=8!)
       [5, 7, 8, -1, -1],    # Row 5
       [6, 8, -1, -1, -1],   # Row 6
   ]
   ```

## Next Steps

1. Add debug logging to failing tests
2. Verify category functions return correct values
3. Fix any off-by-one errors
4. Re-run tests and achieve 20/20 pass!
