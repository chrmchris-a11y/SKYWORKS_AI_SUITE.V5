# FINAL REQUEST: 100% EASA/JARUS ALIGNMENT

**Status:** 15/20 tests passing after 2 iterations  
**Cost so far:** $0.41  
**Goal:** 20/20 PASS with FULL EASA/JARUS compliance

---

## Executive Summary

After 2 iterations with Claude Sonnet 4, we have:
- ✅ **15/20 tests PASSING**
- ❌ **5/20 tests FAILING**

Your analysis (from iteration 2 response) suggests that **some test expectations may be incorrect**, not the code.

---

## The 5 Failing Tests

### Test 1: GRC 2.0.2 - Floor enforcement (3.0m BVLOS_Populated + M1 High)
**Current Result:**
- Intrinsic GRC: 7
- Final GRC: 5 (after M1 High = -2)
- Floor: 3

**Test expects:** `final_grc = 3`  
**Code returns:** `final_grc = 5`

**Your previous analysis:** *"BVLOS_Populated with 3-8m has intrinsic GRC = 7. With M1 High (-2) and floor of 3, it should be max(7-2, 3) = 5, not 3."*

**QUESTION:** Should the test expect 5, or should floor enforcement work differently?

---

### Test 2: GRC 2.5.4 - Column floor enforcement (5.0m @ 30m/s + M1 High)
**Current Result:**
- Dimension: 5.0m → Cat 2
- Speed: 30m/s → Cat 1
- Combined: Cat 2 (max)
- Population: Default (row 1)
- Intrinsic GRC: 6
- Final GRC: 4 (after M1 High = -2)
- Floor: 2

**Test expects:** `final_grc = 3`  
**Code returns:** `final_grc = 4`

**Your previous analysis:** *"Test expects final GRC = 3, but the calculation should be 2 based on floor enforcement."*

**QUESTION:** What should the correct final GRC be according to JARUS 2.5 Annex A?

---

### Test 3: GRC 2.5.5 - High speed (15m @ 100m/s)
**Current Result:**
- Dimension: 15m → Cat 3
- Speed: 100m/s → Cat 3
- Combined: Cat 3 (≥ 8m)
- Population: Default (row 1)
- Intrinsic GRC: 6
- Final GRC: 6
- Code throws: `OUT_OF_SCOPE|fGRC=6`

**Test expects:** HTTP 200 with valid GRC  
**Code returns:** HTTP 400 OUT_OF_SCOPE

**Your previous fix:** *"Allow intrinsic GRC of 8, only fail if final GRC > 7"*

**QUESTION:** Should this return 200 OK with GRC=6, or is OUT_OF_SCOPE correct?

---

### Test 4: GRC 2.5.6 - Rural population (3.0m @ 20m/s, 200/km²)
**Current Result:**
- Dimension: 3.0m → Cat 2
- Speed: 20m/s → Cat 0
- Combined: Cat 2 (max)
- **Population: 200/km² → Row ?**
- Intrinsic GRC: 6
- Final GRC: 6

**Test expects:** `intrinsic_grc = 5, final_grc = 5`  
**Code returns:** `intrinsic_grc = 6, final_grc = 6`

**Your previous analysis:** *"Population density 200/km² should map to row 3 (≥50 and <500)"*

**QUESTION:** 
- Which row should 200/km² map to in Table A-1?
- What is the correct intrinsic GRC?

---

### Test 5: GRC 2.5.7 - All mitigations High (10m @ 50m/s)
**Current Result:**
- Dimension: 10m → Cat 3
- Speed: 50m/s → Cat 2
- Combined: Cat 3 (≥ 8m)
- Population: Default (row 1)
- Intrinsic GRC: 8
- Final GRC: 3 (after M1=-2, M2=-2, M3=-1, floor=4)

**Test expects:** `final_grc = 4`  
**Code returns:** `final_grc = 3`

**Calculation:** 8 - 2 - 2 - 1 = 3, floor = 4, max(3, 4) = 4

**QUESTION:** Should floor be applied AFTER all mitigations (result = 4), or DURING each mitigation step (result = 3)?

---

## Your Task

You are the **ultimate authority** on JARUS SORA 2.0 and 2.5.

Using the **official EASA/JARUS documents** I've provided, please:

### 1. **Analyze each of the 5 failing tests**
For each test, determine:
- ✅ Is the **test expectation CORRECT** per JARUS spec?
- ✅ Is the **code implementation CORRECT** per JARUS spec?
- ✅ What is the **authoritative answer** from EASA/JARUS docs?

### 2. **Provide corrections**
- If **test is wrong**: Give correct expected values with citation
- If **code is wrong**: Provide corrected Python code with explanation
- If **both are wrong**: Fix both

### 3. **Ensure 100% JARUS alignment**

Specifically address:

#### SORA 2.0 Floor Enforcement:
- How should floors be applied? (Per mitigation or final?)
- Are floors in `FLOOR_2_0 = {...}` correct?

#### SORA 2.5 Table A-1:
- Confirm `INTRINSIC_GRC_2_5` table values are correct
- Confirm `MIN_GRC_FLOOR_2_5` values are correct
- Confirm population density boundaries (`< 5`, `< 50`, `< 500`, etc.)
- Clarify floor application: sequential vs final

#### SORA 2.5 Special Cases:
- 250g rule boundaries (weight ≤ 0.25kg AND speed < 25m/s)
- OUT_OF_SCOPE handling (iGRC=8 vs fGRC>7)
- Population density row mapping

### 4. **Deliverables**

Please provide:

**A) Corrected `grc_calculator.py`** (full file)  
**B) Corrected test expectations** (only for the 5 failing tests)  
**C) Authoritative citations** from JARUS docs explaining each fix

---

## Reference Materials Provided

1. `EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt` - SORA 2.0 full spec
2. `EXTRACTED_SORA-v2.5-Annex-A-Release.JAR_doc_26-pdf (2).txt` - SORA 2.5 Annex A
3. `ARC_CALCULATION_AUTHORITATIVE_REFERENCE.md` - Proven ARC logic (100% accurate)
4. `COMPREHENSIVE_ACCURACY_REPORT.md` - Previous 100% pass state
5. Current `grc_calculator.py` - Your iteration 2 code
6. `test_comprehensive_report.py` - Test file (will share expectations)

---

## Critical Requirements

1. **Do NOT break** the 15 tests currently passing
2. **Cite specific sections** from JARUS docs for every decision
3. **Preserve code structure** and comments
4. **Be definitive** - no "maybe" or "appears to be"
5. **100% JARUS compliance** - if spec says floor=3, code must enforce floor=3

---

## Expected Output Format

```markdown
# Analysis Results

## Test 1: GRC 2.0.2
**Authoritative Answer:** [from JARUS SORA 2.0 Section X]
**Correct Expectation:** final_grc = X
**Code Status:** ✅ CORRECT / ❌ NEEDS FIX
**Fix Required:** [if needed]

## Test 2: GRC 2.5.4
...

# Corrected Code

\```python
[full grc_calculator.py]
\```

# Corrected Test Expectations

\```python
# Test 1: GRC 2.0.2
expected = {"final_grc": X, ...}

# Test 2: GRC 2.5.4
...
\```
```

---

**Budget remaining:** $17.90  
**This is the FINAL iteration** - we need 20/20 PASS with full compliance.

Thank you for your expertise!
