# 🎯 COMPLETE SORA 2.0 / 2.5 COMPLIANCE FIX PLAN

**Date:** 7 November 2025  
**Scope:** 100% EASA/JARUS compliance per user specification  
**Authority:** Backend C# models + JARUS SORA documents

---

## 📋 CRITICAL FIXES REQUIRED (26 Total)

### **FIX #1: AEC Dropdown (SORA 2.0 & 2.5)**

**Current:** Field "ACE Categories" με λάθος ορολογία  
**Required:** Rename to "AEC (Airspace Encounter Category)" με 12 options

**Source:** `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs` lines 340-575

**12 AEC Categories (Annex C Table C.1):**

| AEC | Description | Max Height | Airspace | Environment | → ARC |
|-----|-------------|------------|----------|-------------|-------|
| **1** | Airport/Heliport in Class B/C/D | Any | Controlled | Airport | **d** |
| **2** | Above 500ft in Mode-S/TMZ | >500ft | TMZ | Any | **d** |
| **3** | Above 500ft in Controlled | >500ft | Controlled | Any | **d** |
| **4** | Above 500ft, Uncontrolled, Urban | >500ft | Uncontrolled | Urban | **c** |
| **5** | Above 500ft, Uncontrolled, Rural | >500ft | Uncontrolled | Rural | **c** |
| **6** | Airport/Heliport in Class E/F/G | Any | Uncontrolled | Airport | **c** |
| **7** | Below 500ft in Mode-S/TMZ | <500ft | TMZ | Any | **c** |
| **8** | Below 500ft in Controlled | <500ft | Controlled | Any | **c** |
| **9** | Below 500ft, Uncontrolled, Urban | <500ft | Uncontrolled | Urban | **c** |
| **10** | Below 500ft, Uncontrolled, Rural | <500ft | Uncontrolled | Rural | **b** |
| **11** | Above FL600 (18,000m) | >18,000m | Any | Any | **b** |
| **12** | Atypical/Segregated (Annex G 3.20) | Any | Segregated | Any | **a** |

**Implementation:**
```html
<label>AEC (Airspace Encounter Category - Annex C)
  <select id="aecCategory">
    <option value="">Auto-calculate from environment</option>
    <option value="1">AEC 1: Airport Class B/C/D → ARC-d</option>
    <option value="2">AEC 2: >500ft Mode-S/TMZ → ARC-d</option>
    <option value="3">AEC 3: >500ft Controlled → ARC-d</option>
    <option value="4">AEC 4: >500ft Uncontrolled Urban → ARC-c</option>
    <option value="5">AEC 5: >500ft Uncontrolled Rural → ARC-c</option>
    <option value="6">AEC 6: Airport Class E/F/G → ARC-c</option>
    <option value="7">AEC 7: <500ft Mode-S/TMZ → ARC-c</option>
    <option value="8">AEC 8: <500ft Controlled → ARC-c</option>
    <option value="9">AEC 9: <500ft Uncontrolled Urban → ARC-c</option>
    <option value="10">AEC 10: <500ft Uncontrolled Rural → ARC-b</option>
    <option value="11">AEC 11: Above FL600 → ARC-b</option>
    <option value="12">AEC 12: Atypical/Segregated → ARC-a</option>
  </select>
</label>
```

---

### **FIX #2: Remove "Mode-S Veil" (EU-Specific)**

**Current:** Checkbox "Mode-S Veil / Transponder Mandatory"  
**Required:** Replace with **TMZ (Transponder Mandatory Zone)** only

**Authority:** EU regulations use TMZ/RMZ, NOT Mode-S veil (FAA term)

**Implementation:**
```html
<!-- BEFORE (WRONG - US Term) -->
<input type="checkbox" id="isModeS"> Mode-S Veil / Transponder Mandatory

<!-- AFTER (CORRECT - EU Term) -->
<input type="checkbox" id="isTMZ"> TMZ (Transponder Mandatory Zone)
```

---

### **FIX #3: SORA 2.5 - Remove M3 from GRC Mitigations**

**Current:** M3 (ERP) shown in ground risk mitigations  
**Required:** Remove M3 from SORA 2.5; ERP moves to OSOs

**Authority:** JARUS SORA 2.5 Main Body - M3 deprecated as mitigation, now OSO evidence

**Implementation:**
```html
<!-- SORA 2.5: NO M3 dropdown -->
<fieldset id="grc25">
  <!-- M1A, M1B, M1C, M2 ONLY -->
  <!-- M3 (ERP) → OSO section -->
</fieldset>
```

---

### **FIX #4: SORA 2.5 - Split M1 into M1(A/B/C)**

**Current:** Single M1 dropdown  
**Required:** Three separate dropdowns per Table 5

**Authority:** `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs` lines 284-296

**Implementation:**
```html
<label>M1(A) - Sheltering
  <select id="m1a_25">
    <option value="0">None (0)</option>
    <option value="-1">Low (-1)</option>
    <option value="-2">Medium (-2)</option>
  </select>
</label>

<label>M1(B) - Operational Restrictions
  <select id="m1b_25">
    <option value="0">None (0)</option>
    <option value="-1">Medium (-1)</option>
    <option value="-2">High (-2)</option>
  </select>
</label>

<label>M1(C) - Ground Observation
  <select id="m1c_25">
    <option value="0">None (0)</option>
    <option value="-1">Low (-1)</option>
  </select>
</label>
```

---

### **FIX #5: Hide ≤250g Rule for Heavy Drones**

**Current:** Shows "≤250g & ≤25m/s → iGRC = 1" for ALL drones  
**Required:** Only show if MTOM ≤ 0.25 kg AND speed ≤ 25 m/s

**Authority:** JARUS SORA 2.5 Table 2 special rule (micro-UAS)

**Implementation:**
```javascript
function updateiGRC() {
  const mtom = parseFloat(document.getElementById('mtom').value);
  const speed = parseFloat(document.getElementById('maxSpeed').value);
  const specialRuleDiv = document.getElementById('microUASRule');
  
  // Only show if meets BOTH conditions
  if (mtom <= 0.25 && speed <= 25) {
    specialRuleDiv.style.display = 'block';
    specialRuleDiv.textContent = '✅ Special rule: ≤250g & ≤25m/s → iGRC = 1';
  } else {
    specialRuleDiv.style.display = 'none';
  }
}
```

---

### **FIX #6: SORA 2.0 - Correct M1/M2/M3 Credits**

**Current:** Uncertain implementation  
**Required:** Exact credits from Table 3

**Authority:** Backend lines 151-178 (SORA_2_0_Models.cs)

**Credits:**
- **M1 Strategic:** None=0, Low=-1, Medium=-2, High=-4 (with column floor clamp)
- **M2 Impact:** None=0, Medium=-1, High=-2 (NO "Low" option!)
- **M3 ERP:** None=+1, Medium=0, High=-1

**Implementation:**
```html
<select id="m1_20">
  <option value="0">None (0)</option>
  <option value="-1">Low (-1)</option>
  <option value="-2">Medium (-2)</option>
  <option value="-4">High (-4)</option>
</select>

<select id="m2_20">
  <option value="0">None (0)</option>
  <option value="-1">Medium (-1)</option>
  <option value="-2">High (-2)</option>
</select>

<select id="m3_20">
  <option value="1">None (+1 penalty)</option>
  <option value="0">Medium (0)</option>
  <option value="-1">High (-1)</option>
</select>
```

---

### **FIX #7: Sequential Mitigation Application (SORA 2.0)**

**Current:** Unknown order  
**Required:** Apply M1 → M2 → M3 sequentially with floor clamp

**Authority:** Backend GRCCalculationService lines 610-680

**Logic:**
```javascript
function calculateFinalGRC_20(iGRC, m1, m2, m3, columnMin) {
  let grc = iGRC;
  
  // Step 1: Apply M1 (clamp to column minimum)
  grc = grc + m1; // m1 is negative
  grc = Math.max(grc, columnMin);
  
  // Step 2: Apply M2
  grc = grc + m2; // m2 is negative
  
  // Step 3: Apply M3
  grc = grc + m3; // m3 can be positive (+1) or negative
  
  // Floor: Final GRC ≥ 1
  grc = Math.max(grc, 1);
  
  // Ceiling: Final GRC > 7 → OUT OF SCOPE
  if (grc > 7) {
    return { valid: false, error: "Final GRC > 7 requires CERTIFIED category" };
  }
  
  return { valid: true, finalGRC: grc };
}
```

---

### **FIX #8: Add U-space Field (SORA 2.5)**

**Current:** Missing  
**Required:** U-space availability affects ARC mitigation

**Authority:** EASA Regulation (EU) 2021/664

**Implementation:**
```html
<label>U-space Services Available?
  <select id="uspaceAvailable">
    <option value="no">No</option>
    <option value="yes">Yes (typically → ARC-b)</option>
  </select>
</label>

<label>Traffic Density Source
  <select id="trafficDensitySource">
    <option value="ansp">ANSP/USSP Map</option>
    <option value="empirical">Empirical Data</option>
    <option value="modelled">Modelled</option>
  </select>
</label>
```

---

### **FIX #9: Strategic ARC Mitigations (Structured)**

**Current:** CSV input "SM1,SM2"  
**Required:** Structured checkboxes/dropdowns

**Implementation:**
```html
<fieldset>
  <legend>Strategic ARC Mitigations (Step #5)</legend>
  
  <label>
    <input type="checkbox" id="temporalSegregation"> 
    Temporal Segregation (time-based separation)
  </label>
  
  <label>
    <input type="checkbox" id="spatialSegregation"> 
    Spatial Segregation (geographical separation)
  </label>
  
  <label>Airspace Containment
    <select id="airspaceContainment">
      <option value="none">None</option>
      <option value="lateral">Lateral Only</option>
      <option value="vertical">Vertical Only</option>
      <option value="both">Lateral & Vertical</option>
    </select>
  </label>
</fieldset>
```

---

### **FIX #10: Validation - Out of Scope Errors**

**Current:** Unknown handling  
**Required:** Clear error messages

**Authority:** SORA 2.0/2.5 Main Body acceptability criteria

**Validation Rules:**
```javascript
function validateSORACriteria(finalGRC, residualARC, sail) {
  const errors = [];
  
  // Rule 1: Final GRC > 7 → CERTIFIED category required
  if (finalGRC > 7) {
    errors.push("Final GRC exceeds 7 - operation requires CERTIFIED category (out of SPECIFIC scope)");
  }
  
  // Rule 2: SAIL VI → CERTIFIED category
  if (sail === 'VI') {
    errors.push("SAIL VI requires CERTIFIED category per EASA regulations");
  }
  
  // Rule 3: ARC-d with high GRC → typically out of scope
  if (residualARC === 'ARC_d' && finalGRC >= 5) {
    errors.push("ARC-d with GRC ≥5 typically requires CERTIFIED category (consult authority)");
  }
  
  return { valid: errors.length === 0, errors };
}
```

---

### **FIX #11-26: Additional Compliance Items**

**FIX #11:** Add RMZ (Radio Mandatory Zone) checkbox (separate from TMZ)  
**FIX #12:** Population Density qualitative descriptors (Annex F) for SORA 2.5  
**FIX #13:** MTOM input validation (numeric, > 0)  
**FIX #14:** Max Height AGL input (numeric, typically ≤ 120m for SPECIFIC)  
**FIX #15:** Operation Type dropdown (VLOS / BVLOS) - affects scenario selection  
**FIX #16:** STS/PDRA selection (dropdown with STS-01, STS-02, PDRA-S01, etc.)  
**FIX #17:** UA Class Marking (C0-C6 / Privately built) - affects STS eligibility  
**FIX #18:** Tethered checkbox (reduces iGRC if proper CGA)  
**FIX #19:** Impact mitigation equipment (multi-select for M2 credit)  
**FIX #20:** Airspace Class dropdown (A-G) for AEC calculation  
**FIX #21:** Location Type (Airport/Heliport/Non-Airport) for AEC  
**FIX #22:** CTR/TMA/ATZ proximity checkboxes  
**FIX #23:** Prohibited/Restricted/Danger area warnings  
**FIX #24:** TMPR evidence file upload per residual ARC  
**FIX #25:** OSO robustness level inputs (Low/Medium/High per OSO)  
**FIX #26:** SAIL auto-calculation from Table 7 (GRC × ARC matrix)

---

## 🧪 TESTING CHECKLIST

### SORA 2.0 Tests
- [ ] AEC 9 (<500ft Urban) → ARC-c calculation
- [ ] M1 High (-4) with column clamp to min GRC
- [ ] M2 Medium (-1) NO "Low" option available
- [ ] M3 None (+1 penalty) applied correctly
- [ ] Sequential mitigation order (M1→M2→M3)
- [ ] Final GRC > 7 → error "CERTIFIED required"
- [ ] Strategic mitigations (temporal/spatial) reduce ARC
- [ ] SAIL auto from GRC×ARC table

### SORA 2.5 Tests
- [ ] ≤250g & ≤25m/s → iGRC=1 special rule
- [ ] M1(A) Low+Medium combo allowed
- [ ] M1(B) High (-2) credit applied
- [ ] M1(C) Low only (no Medium/High)
- [ ] M3 NOT shown in mitigations (moved to OSOs)
- [ ] U-space "Yes" → typically ARC-b
- [ ] Population density qualitative mapping
- [ ] Annex F adjacent-area GRC calculation

---

## 📚 AUTHORITATIVE SOURCES

**Backend Services:**
- `GRCCalculationService.cs` → iGRC, fGRC, ARC logic
- `SORA_2_0_Models.cs` → Enums, credits, constraints
- `MitigationModels.cs` (V2_5) → M1A/M1B/M1C/M2 credits

**JARUS Documents:**
- SORA 2.0: JAR-DEL-WG6-D.04 (Edition 2.0, 30.01.2019)
- SORA 2.5: Main Body (13-May-2024)
- Annex C: AEC categories & Initial ARC determination
- Annex F: SORA 2.5 GRC calculation & adjacent areas
- Table 2: iGRC matrix (scenarios × dimensions)
- Table 3 (2.0): M1/M2/M3 mitigations & credits
- Table 5 (2.5): M1A/M1B/M1C/M2 mitigations & credits
- Table 7: SAIL matrix (GRC × ARC)

**EASA Regulations:**
- Commission Implementing Regulation (EU) 2019/947
- AMC to Article 11 (SORA methodology)
- Appendix 1: STS-01, STS-02
- AMC1 Article 11: PDRA-S01, PDRA-S02, PDRA-G01/G02/G03
- Regulation (EU) 2021/664: U-space

---

## 🎯 PRIORITY ORDER (Implementation Sequence)

**PRIORITY 1 (Critical - Breaks Calculations):**
1. Fix #6: M1/M2/M3 correct credits
2. Fix #7: Sequential mitigation application
3. Fix #1: AEC dropdown (12 categories)
4. Fix #3: Remove M3 from SORA 2.5
5. Fix #4: Split M1 → M1A/M1B/M1C

**PRIORITY 2 (Important - Compliance):**
6. Fix #2: Remove Mode-S veil (use TMZ)
7. Fix #5: Hide ≤250g rule for heavy drones
8. Fix #10: Validation error messages
9. Fix #8: U-space field (SORA 2.5)

**PRIORITY 3 (Enhancement - UX):**
10. Fix #9: Structured strategic mitigations
11. Fixes #11-26: Additional fields

---

## ✅ COMPLETION CRITERIA

- [ ] All 26 fixes implemented
- [ ] Frontend dropdowns match backend enums 100%
- [ ] SORA 2.0 tests pass (8 scenarios)
- [ ] SORA 2.5 tests pass (8 scenarios)
- [ ] Validation errors show correctly
- [ ] Documentation updated
- [ ] Git commit with detailed changelog
