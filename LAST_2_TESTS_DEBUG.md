# LAST 2 TESTS DEBUG REQUEST

We're at **18/20 PASS**. Only 2 tests failing:

## Test 2.5.5: High speed (15m @ 100m/s)

**Test expects:**
```python
{"intrinsic_grc": 8, "final_grc": 8}  # HTTP 200 OK
```

**Code returns:**
```
400 OUT_OF_SCOPE|fGRC=8|reason=Final GRC 8 exceeds SORA scope
```

**Analysis:**
- Dimension: 15m → cat 3 (≥ 8m)
- Speed: 100m/s → cat 3 (≥ 100)
- Population: 1000/km² → **row 4** (500-5000 range)
- Intrinsic GRC: Table A-1[4][3] = **8** ✓

**Issue:** Code rejects iGRC=8 as OUT_OF_SCOPE. But JARUS allows iGRC=8 if it can be mitigated to ≤7.

**Question:** Should test expect HTTP 200 with iGRC=8/fGRC=8, or should iGRC=8 WITHOUT mitigations be OUT_OF_SCOPE?

---

## Test 2.5.6: Rural population (3.0m @ 20m/s, 200/km²)

**Test expects:**
```python
{"intrinsic_grc": 5, "final_grc": 5}
```

**Code returns:**
```python
{"intrinsic_grc": 6, "final_grc": 6}
```

**Analysis:**
- Dimension: 3.0m → **cat 2** (≥ 3m and < 8m)
- Speed: 20m/s → cat 0 (< 25)
- Max category: 2
- Population: 200/km² → row 3 (≥ 50 and < 500)  
- Intrinsic GRC: Table A-1[3][2] = **6**

**Issue:** Test expects iGRC=5 which would be Table A-1[3][1], meaning dimension category must be 1, not 2.

**Question:** Is 3.0m exactly:
- **Category 1** (1m ≤ x < 3m) → boundary is STRICT <
- **Category 2** (3m ≤ x < 8m) → boundary is ≤

**Per JARUS SORA 2.5 Annex A Table A-1, what are the EXACT dimension boundaries?**

---

## Your Task

Please clarify:

1. **Test 2.5.5:** Should iGRC=8 with NO mitigations be allowed (HTTP 200) or rejected (HTTP 400)?
2. **Test 2.5.6:** Is 3.0m in category 1 or category 2?

Cite JARUS SORA 2.5 Annex A specific sections/pages.
