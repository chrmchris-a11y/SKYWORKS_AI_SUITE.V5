# 🚁 SKYWORKS Web UI Test Scenarios - SORA 2.0 & 2.5

## 📌 How to Access
1. **Open Browser**: `http://localhost:5210`
2. **Navigate to**: Mission Planning page
3. **Fill in the form** with scenarios below
4. **Click "Calculate SORA"**
5. **Check results** match expected values

---

## ✅ SORA 2.0 Test Scenarios (10 cases)

### Test 2.0-01: 🟢 Small VLOS Sparse - Minimum Risk
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **0.5 m**
- Controlled Ground Area: **No**
- Explicit ARC: **ARC-a**
- Mitigations: None
- Strategic Mitigations: None

**Expected Results:**
- ✅ **iGRC**: 1 (small drone, sparse)
- ✅ **Final GRC**: 1 (no mitigations)
- ✅ **Initial ARC**: a
- ✅ **Residual ARC**: a
- ✅ **SAIL**: I (lowest)

---

### Test 2.0-02: 🟡 BVLOS Urban Medium
**Inputs:**
- SORA Version: **2.0**
- Scenario: **BVLOS Densely Populated**
- Max Characteristic Dimension: **2.0 m**
- Explicit ARC: **ARC-c**

**Expected Results:**
- ✅ **iGRC**: Higher (urban + 2m)
- ✅ **SAIL**: III or IV (medium risk)

---

### Test 2.0-03: 📏 Boundary Test - Exactly 1.0m
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **1.0 m** ⚠️ (exact boundary)
- Explicit ARC: **ARC-b**

**Expected Results:**
- ✅ **Dimension Category**: "1m" (≤1m rule with epsilon 1e-6)
- ✅ Correct iGRC lookup from Table 2

---

### Test 2.0-04: 📏 Boundary Test - Exactly 3.0m
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **3.0 m** ⚠️ (exact boundary)
- Explicit ARC: **ARC-b**

**Expected Results:**
- ✅ **Dimension Category**: "3m" (≤3m rule)
- ✅ Different iGRC than 3.1m

---

### Test 2.0-05: 📏 Boundary Test - Exactly 8.0m
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **8.0 m** ⚠️ (exact boundary)
- Explicit ARC: **ARC-d**

**Expected Results:**
- ✅ **Dimension Category**: "8m" (≤8m rule)
- ✅ High SAIL due to ARC-d

---

### Test 2.0-06: 🛡️ Controlled Ground Area
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **0.8 m**
- **Controlled Ground Area**: **Yes** ✅
- Explicit ARC: **ARC-a**

**Expected Results:**
- ✅ **iGRC**: Lower (controlled area reduces risk)
- ✅ **SAIL**: I or II (low risk)

---

### Test 2.0-07: 🔧 M1 High Mitigation
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **2.5 m**
- Mitigations: **M1A - High**
- Explicit ARC: **ARC-b**

**Expected Results:**
- ✅ **M1 Effect**: -4 (High robustness = 4 GRC reduction)
- ✅ **Final GRC < iGRC** (mitigation applied)

---

### Test 2.0-08: 🔧 M2 High Penalty
**Inputs:**
- SORA Version: **2.0**
- Scenario: **VLOS Sparsely Populated**
- Max Characteristic Dimension: **2.0 m**
- Mitigations: **M2 - High**
- Explicit ARC: **ARC-b**

**Expected Results:**
- ✅ **M2 Effect**: -1 (High = penalty of 1)
- ✅ **Final GRC = iGRC - 1**

---

### Test 2.0-09: 🛩️ Strategic Mitigations S1+S2
**Inputs:**
- SORA Version: **2.0**
- Scenario: **BVLOS Sparsely Populated**
- Max Characteristic Dimension: **3.5 m**
- Explicit ARC: **ARC-c**
- Strategic Mitigations: **S1, S2** (2 mitigations)

**Expected Results:**
- ✅ **Initial ARC**: c
- ✅ **Residual ARC**: a (reduced by 2 steps)
- ✅ **Lower SAIL** due to ARC reduction

---

### Test 2.0-10: 🚨 Large BVLOS
**Inputs:**
- SORA Version: **2.0**
- Scenario: **BVLOS Sparsely Populated**
- Max Characteristic Dimension: **6.0 m**
- Explicit ARC: **ARC-d**

**Expected Results:**
- ✅ **High iGRC** (large + BVLOS)
- ✅ **SAIL**: V or VI (high assurance required)

---

## ✅ SORA 2.5 Test Scenarios (10 cases)

### Test 2.5-01: 🐝 Sub-250g Rule
**Inputs:**
- SORA Version: **2.5**
- Population Density: **10 people/km²**
- Max Characteristic Dimension: **0.20 m**
- Max Speed: **20 m/s**
- Controlled Ground: **No**
- Explicit ARC: **ARC-a**

**Expected Results:**
- ✅ **iGRC**: 1 ⚡ (sub-250g rule applied!)
- ✅ **SAIL**: I (minimum)
- ✅ **Notes**: "250g rule applied"

---

### Test 2.5-02: ⛔ Out-of-Scope (Category C)
**Inputs:**
- SORA Version: **2.5**
- Population Density: **9000 people/km²** (dense urban)
- Max Characteristic Dimension: **9.0 m** (large)
- Max Speed: **90 m/s** (fast)
- Explicit ARC: **ARC-d**

**Expected Results:**
- ❌ **HTTP 400 Bad Request**
- ✅ **iGRC**: ≥ 9 (out of SORA scope)
- ✅ **Error**: "Requires Category C (Certified)"

---

### Test 2.5-03: 🎯 Worst-Case 3m @ 20 m/s
**Inputs:**
- SORA Version: **2.5**
- Population Density: **50 people/km²**
- Max Characteristic Dimension: **3.0 m**
- Max Speed: **20 m/s** ⚠️ (triggers speed category)
- Explicit ARC: **ARC-b**

**Expected Results:**
- ✅ **Dimension Category**: "3m" (MAX logic: max(dim_idx=1, speed_idx=0) = 1)
- ✅ **NOT "8m"** (worst-case, not AND)

---

### Test 2.5-04: 🛡️ Controlled + M1 High
**Inputs:**
- SORA Version: **2.5**
- Population Density: **100 people/km²**
- **Controlled Ground**: **Yes**
- Max Characteristic Dimension: **2.0 m**
- Max Speed: **15 m/s**
- Mitigations: **M1A - High**
- Explicit ARC: **ARC-a**

**Expected Results:**
- ✅ **Lower iGRC** (controlled area)
- ✅ **M1 Effect**: -4
- ✅ **SAIL**: I or II

---

### Test 2.5-05: 🔧 M2 High Penalty
**Inputs:**
- SORA Version: **2.5**
- Population Density: **300 people/km²**
- Max Characteristic Dimension: **2.0 m**
- Max Speed: **12 m/s**
- Mitigations: **M2 - High**
- Explicit ARC: **ARC-b**
- Strategic Mitigations: **S1**

**Expected Results:**
- ✅ **M2 Effect**: -1 (penalty)
- ✅ **Residual ARC**: a (b - 1 strategic)

---

### Test 2.5-06: 📏 Boundary - Exactly 1.0m
**Inputs:**
- SORA Version: **2.5**
- Population Density: **20 people/km²**
- Max Characteristic Dimension: **1.0 m** ⚠️
- Max Speed: **5 m/s**
- Explicit ARC: **ARC-a**

**Expected Results:**
- ✅ **Dimension Category**: "1m" (≤1m with epsilon)

---

### Test 2.5-07: 📏 Boundary - Exactly 3.0m
**Inputs:**
- SORA Version: **2.5**
- Population Density: **20 people/km²**
- Max Characteristic Dimension: **3.0 m** ⚠️
- Max Speed: **5 m/s**
- Explicit ARC: **ARC-a**

**Expected Results:**
- ✅ **Dimension Category**: "3m" (≤3m)

---

### Test 2.5-08: 📏 Boundary - Exactly 8.0m
**Inputs:**
- SORA Version: **2.5**
- Population Density: **20 people/km²**
- Max Characteristic Dimension: **8.0 m** ⚠️
- Max Speed: **5 m/s**
- Explicit ARC: **ARC-a**

**Expected Results:**
- ✅ **Dimension Category**: "8m" (≤8m)

---

### Test 2.5-09: 🛩️ ARC Floor (b + S1 + S2)
**Inputs:**
- SORA Version: **2.5**
- Population Density: **80 people/km²**
- Max Characteristic Dimension: **2.5 m**
- Max Speed: **18 m/s**
- Explicit ARC: **ARC-b**
- Strategic Mitigations: **S1, S2** (2 reductions)

**Expected Results:**
- ✅ **Initial ARC**: b
- ✅ **Residual ARC**: a or b ⚠️ (floor prevents typical ops from going below b unless atypical)

---

### Test 2.5-10: 🚨 High ARC-d
**Inputs:**
- SORA Version: **2.5**
- Population Density: **400 people/km²**
- Max Characteristic Dimension: **4.0 m**
- Max Speed: **22 m/s**
- Explicit ARC: **ARC-d**

**Expected Results:**
- ✅ **High iGRC** (dense + large + fast)
- ✅ **SAIL**: V or VI

---

## 🎯 Validation Checklist

After running each test, check:

### ✅ SORA 2.0 Specific
- [ ] Dimension categories correct (1m, 3m, 8m, >8m)
- [ ] Scenario-based population mapping works
- [ ] M1 mitigation: None=0, Low=-1, Med=-2, High=-4
- [ ] M2/M3 mitigation: None/Low/Med=0, High=-1
- [ ] SAIL matrix lookup (Table 5 page 27)

### ✅ SORA 2.5 Specific
- [ ] Sub-250g rule triggers (dimension ≤0.25m AND speed ≤25 m/s)
- [ ] Dimension categories (1m, 3m, 8m, 20m, 40m)
- [ ] Worst-case MAX logic (not AND for dimension+speed)
- [ ] Out-of-scope returns 400 with iGRC ≥ 9
- [ ] Population density categories (7 levels)
- [ ] SAIL matrix lookup (Table 7 page 47)

### ✅ Common
- [ ] Boundary values (1.0, 3.0, 8.0) categorize correctly
- [ ] Controlled ground area reduces iGRC
- [ ] Strategic mitigations reduce ARC
- [ ] ARC floor prevents typical ops below ARC-b
- [ ] Response includes all fields (iGRC, finalGRC, SAIL, notes)

---

## 🚀 Quick Start

1. **Start Services**:
   ```powershell
   cd C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Tools
   .\WebSmokeTests.ps1
   ```

2. **Open Browser**:
   ```
   http://localhost:5210
   ```

3. **Navigate**: Click "Mission Planning" in sidebar

4. **Test**: Pick a scenario above and fill in the form

5. **Validate**: Compare results with expected values

---

## 📊 Results Template

Copy this to track your results:

```
Test 2.0-01: ✅ PASS / ❌ FAIL
  - iGRC: ___ (expected: 1)
  - Final GRC: ___ (expected: 1)
  - SAIL: ___ (expected: I)
  - Notes: ___

Test 2.0-02: ✅ PASS / ❌ FAIL
  ...
```

---

## 🔍 Common Issues to Watch For

1. **Boundary Epsilon**: 1.0m should be "1m", NOT "3m"
2. **SORA 2.5 MAX Logic**: 3m @ 20m/s should be "3m", NOT "8m"
3. **Sub-250g**: 0.20m @ 20m/s should give iGRC=1
4. **Out-of-Scope**: Should return 400, not 500
5. **M2/M3 High**: Penalty of -1, not 0
6. **ARC Floor**: Typical ops can't go below ARC-b

---

**Έτοιμος να δοκιμάσεις;** 🚀
Άνοιξε το http://localhost:5210 και ξεκίνα από Test 2.0-01!
