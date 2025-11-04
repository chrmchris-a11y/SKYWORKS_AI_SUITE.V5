# 🚨 CRITICAL: SORA 2.0 Calculation Error - Request for Claude Sonnet 4

**Date:** October 29, 2025  
**Priority:** URGENT - Live Production Issue  
**Affected System:** SKYWORKS Mission Planner - SORA 2.0 Calculations

---

## 🔴 PROBLEM STATEMENT

**Mission Planner returns 422 Unprocessable Content when calculating SORA 2.0 GRC.**

**Error Message:**
```
Ground Risk: Proxy-only mode: Python GRC 2.0 call failed - 
Response status code does not indicate success: 422 (Unprocessable Content).
```

---

## 📋 FAILED TEST CASE

**From Mission Planner UI (http://localhost:5210/app/Pages/mission.html):**

```yaml
Mission: M-20251029-2140-021
Category: SORA 2.0 (Legacy AMC)
Drone: Sky Tech SC15 (C3)
  - MTOM: 32 kg
  - Max Dimension: 1.2 m
  - Max Speed: 8 m/s
  - Kinetic Energy: 1024 J

Scenario: VLOS · Gathering of People
Environment: Urban (>1,500 people/km²)
Max Height AGL: 60 m

Ground Risk Mitigations:
  - M1 (Strategic): Medium (-2)
  - M2 (Impact reduction): High (-2)
  - M3 (Emergency Response Plan): Medium (0)

Expected Result:
  - Initial GRC: 3
  - Final GRC: 1 (after mitigations)
  
Actual Result:
  - API Error 422 (Unprocessable Content)
```

---

## 🎯 REQUIREMENTS

### **Primary Goal:**
Fix Python backend (`Backend_Python/main.py`) to correctly handle SORA 2.0 GRC calculations without returning 422 errors.

### **Secondary Goal:**
Ensure SORA 2.5 calculations continue to work (already validated with 16/16 tests passing).

### **Critical Constraint:**
**BOTH SORA 2.0 AND SORA 2.5 MUST WORK SIMULTANEOUSLY!**
- Do NOT remove SORA 2.0 support
- Do NOT break existing SORA 2.5 implementation
- Maintain dual Table 2 classes: `SORAv2_0_Table2` + `JARUSSORAv2_5_Table2`

---

## 📂 RELEVANT FILES TO ANALYZE

### **1. Python Backend (Calculation Engine)**
**File:** `Backend_Python/main.py` (617 lines)
- Contains `SORAv2_0_Table2` class (lines 32-95)
- Contains `JARUSSORAv2_5_Table2` class (lines 96-196)
- API endpoints for GRC calculation

### **2. SORA 2.0 Specification**
**Source:** EASA AMC1 UAS.OPEN.040/050/060
- Table 2: 5×4 matrix (MTOM × Population density)
- MTOM categories: <0.25kg, 0.25-1kg, 1-25kg, 25-150kg, >150kg
- Population boundaries: ≤1, ≤500, ≤10k, >10k ppl/km²
- Max iGRC: 8

### **3. JARUS SORA 2.5 Specification**
**Source:** JAR-DEL-SRM-SORA-MB-2.5 (May 2024)
- Table 2: 7×5 matrix (Dimension+Speed × Population density)
- Special rule: ≤250g + ≤25m/s → iGRC=1
- Grey cells: Out of scope detection
- Max iGRC: 10

### **4. Request Payload Structure**
Expected API request format for SORA 2.0:
```json
{
  "version": "2.0",
  "mtom_kg": 32.0,
  "population_density": 2000,
  "mitigations": {
    "M1": "medium",
    "M2": "high",
    "M3": "medium"
  }
}
```

---

## 🔍 DIAGNOSTIC QUESTIONS

Please analyze and answer:

1. **What validation is causing the 422 error?**
   - Missing required fields?
   - Invalid field values?
   - Schema mismatch?

2. **Does `SORAv2_0_Table2.calculate_initial_grc()` accept correct parameters?**
   - Should it accept `mtom_kg` + `population_density`?
   - Or does it expect different input structure?

3. **Are mitigations being validated correctly?**
   - Valid mitigation codes: M1, M2, M3
   - Valid levels: low, medium, high
   - Credit values: M1 Medium=-2, M2 High=-2, M3 Medium=0

4. **Is there a FastAPI Pydantic model causing validation failure?**
   - Check request model definition
   - Verify field types and constraints

---

## 📊 EXPECTED BEHAVIOR (REFERENCE)

### **SORA 2.0 Calculation Steps:**

**Step 1: Get MTOM Category**
```python
MTOM = 32 kg
→ Category = 3 (25-150kg range)
```

**Step 2: Get Population Category**
```python
Population Density = 2000 ppl/km² (Urban)
→ Category = 2 (≤10k range, Moderately populated)
```

**Step 3: Lookup iGRC from Table 2**
```python
IGRC_MATRIX = [
  [1, 2, 3, 4],  # <0.25kg
  [1, 2, 4, 5],  # 0.25-1kg
  [2, 3, 5, 6],  # 1-25kg
  [3, 4, 6, 7],  # 25-150kg  ← Row 3
  [4, 5, 7, 8]   # >150kg
]

iGRC = IGRC_MATRIX[3][2] = 6
```

**Step 4: Apply Mitigations**
```python
Initial GRC = 6
M1 Medium: -2 → GRC = 4
M2 High: -2 → GRC = 2
M3 Medium: 0 → GRC = 2
Final GRC = max(1, 2) = 2  # Floor at 1
```

**Wait, the UI shows Initial GRC=3, not 6!** 
→ This suggests the calculation is already wrong or the scenario is different.

---

## 🚨 SUSPECTED ROOT CAUSE

**Hypothesis:** The API endpoint for SORA 2.0 is either:
1. **Missing or incorrectly defined** (wrong path/method)
2. **Has strict Pydantic validation** that rejects the payload
3. **Expects different field names** than what .NET backend sends
4. **Has a bug in parameter mapping** (e.g., expects `mtom` instead of `mtom_kg`)

---

## 📝 DELIVERABLES NEEDED

### **1. Root Cause Analysis**
- Identify exact validation error causing 422
- Show which field/value is rejected

### **2. Fixed Code**
- Updated `Backend_Python/main.py` with corrections
- Ensure SORA 2.0 endpoint works
- Verify SORA 2.5 endpoint still works

### **3. Validation**
- Provide test request payload that should work
- Show expected vs actual API response
- Confirm both versions operational

### **4. Test Suite**
- Update or create test cases for SORA 2.0 API
- Verify edge cases (micro drones, heavy drones, different populations)

---

## 📚 CONTEXT: Previous Work

### **Your Previous Analysis (October 29, 2025 20:52):**
You provided comprehensive analysis of SORA 2.0 vs 2.5 differences and recommended:

✅ **"Option A - Dual Implementation"** (COMPLETED)
- Created `SORAv2_0_Table2` class ✅
- Created `JARUSSORAv2_5_Table2` class ✅
- Test suite: 16/16 tests passing ✅

**Current Issue:** API endpoint integration is broken for SORA 2.0

---

## 🎯 SUCCESS CRITERIA

✅ **SORA 2.0 calculations return 200 OK** (not 422)  
✅ **Mission Planner UI shows correct iGRC/Final GRC**  
✅ **SORA 2.5 calculations continue to work**  
✅ **All 20 test scenarios can execute** (10 for 2.0 + 10 for 2.5)  
✅ **No breaking changes to existing API contracts**

---

## 🔧 DEBUGGING STEPS TAKEN

1. ✅ Verified Backend API running (port 5210)
2. ✅ Verified Python FastAPI running (port 8001)
3. ✅ Confirmed Mission Planner loads successfully
4. ✅ Confirmed drone database loads
5. ❌ **SORA 2.0 calculation fails with 422**
6. ⚠️ SORA 2.5 calculation not yet tested in UI

---

## 📞 REQUEST TO CLAUDE SONNET 4

**Dear Sonnet 4,**

Please analyze the Python backend code and SORA specifications to:

1. **Identify why SORA 2.0 API returns 422 Unprocessable Content**
2. **Provide corrected `Backend_Python/main.py` code**
3. **Ensure dual version support remains intact**
4. **Validate against EASA/JARUS official specifications**

**Attached Files:**
- Current `Backend_Python/main.py` (617 lines)
- SORA 2.0 specification reference
- JARUS SORA 2.5 specification reference
- Failed test case details
- API error logs

**Expected Response:**
- Root cause explanation
- Complete fixed code
- Test validation proof
- Migration instructions (if needed)

Thank you for your expert analysis! 🙏

---

**Prepared by:** GitHub Copilot  
**For Review by:** Claude Sonnet 4 (claude-sonnet-4-20250514)  
**Urgency:** CRITICAL - Blocking production testing
