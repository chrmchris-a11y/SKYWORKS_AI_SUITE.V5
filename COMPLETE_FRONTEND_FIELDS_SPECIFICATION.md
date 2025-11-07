# 🎯 COMPLETE FRONTEND FIELDS SPECIFICATION - EASA/JARUS COMPLIANCE

**Date:** 7 November 2025  
**Source:** Backend C# Models (Authoritative Implementation)  
**Status:** 100% Aligned with GRCCalculationService

---

## 📋 SORA 2.0 FIELDS (BACKEND AUTHORITATIVE)

### 1. **Scenario Dropdown** (`<select id="scenario20">`)

**Source:** `Backend/src/Skyworks.Core/Models/GRC/V2_0/SORA_2_0_Models.cs`  
**Enum:** `OperationalScenario`

| Value | Display Name | Description | iGRC Range |
|-------|--------------|-------------|------------|
| `ControlledGroundArea` | Controlled Ground Area | VLOS/BVLOS over controlled ground | 1-4 |
| `VLOS_SparselyPopulated` | VLOS · Sparsely Populated | VLOS in sparsely populated | 2-5 |
| `BVLOS_SparselyPopulated` | BVLOS · Sparsely Populated | BVLOS in sparsely populated | 3-6 |
| `VLOS_Populated` | VLOS · Populated | VLOS in populated | 4-8 |
| `BVLOS_Populated` | BVLOS · Populated | BVLOS in populated | 5-10 |
| `VLOS_GatheringOfPeople` | VLOS · Gathering of People | VLOS over assemblies | 7 |
| `BVLOS_GatheringOfPeople` | BVLOS · Gathering of People | BVLOS over assemblies | 8 |

**✅ Frontend Implementation:**
```html
<select id="scenario20">
  <option value="ControlledGroundArea">Controlled Ground Area</option>
  <option value="VLOS_SparselyPopulated">VLOS · Sparsely Populated</option>
  <option value="BVLOS_SparselyPopulated">BVLOS · Sparsely Populated</option>
  <option value="VLOS_Populated">VLOS · Populated</option>
  <option value="BVLOS_Populated">BVLOS · Populated</option>
  <option value="VLOS_GatheringOfPeople">VLOS · Gathering of People</option>
  <option value="BVLOS_GatheringOfPeople">BVLOS · Gathering of People</option>
</select>
```

---

### 2. **M1 Strategic Mitigations** (`<select id="m1_20">`)

**Source:** SORA 2.0 Table 3  
**Credits:** Low = -1, Medium = -2, High = -4  
**Floor Constraint:** Cannot reduce below lowest Table 2 column value

| Value | Display | Credit |
|-------|---------|--------|
| `0` | None (0) | 0 |
| `-1` | Low (-1) | -1 |
| `-2` | Medium (-2) | -2 |
| `-4` | High (-4) | -4 |

**✅ Frontend Implementation:**
```html
<select id="m1_20">
  <option value="0">None (0)</option>
  <option value="-1">Low (-1)</option>
  <option value="-2">Medium (-2)</option>
  <option value="-4">High (-4)</option>
</select>
```

---

### 3. **M2 Impact Reduction** (`<select id="m2_20">`)

**Source:** SORA 2.0 Table 3  
**Credits:** Medium = -1, High = -2  
**⚠️ CRITICAL:** NO "Low" option!

| Value | Display | Credit |
|-------|---------|--------|
| `0` | None (0) | 0 |
| `-1` | Medium (-1) | -1 |
| `-2` | High (-2) | -2 |

**✅ Frontend Implementation:**
```html
<select id="m2_20">
  <option value="0">None (0)</option>
  <option value="-1">Medium (-1)</option>
  <option value="-2">High (-2)</option>
</select>
```

---

### 4. **M3 Emergency Response Plan** (`<select id="m3_20">`)

**Source:** SORA 2.0 Table 3  
**Credits:** Low = +1 (penalty), Medium = 0, High = -1

| Value | Display | Credit |
|-------|---------|--------|
| `1` | None (+1 penalty) | +1 |
| `0` | Medium (0) | 0 |
| `-1` | High (-1) | -1 |

**✅ Frontend Implementation:**
```html
<select id="m3_20">
  <option value="1">None (+1 penalty)</option>
  <option value="0">Medium (0)</option>
  <option value="-1">High (-1)</option>
</select>
```

---

## 📋 SORA 2.5 FIELDS (BACKEND AUTHORITATIVE)

### 1. **Population Density Dropdown** (`<select id="populationDensity">`)

**Source:** JARUS SORA 2.5 Table 3  
**Backend Mapping:** `IGRCCalculationService.GetPopulationCategory(density, controlled)`

| Value | Display Name | Category | iGRC Impact |
|-------|--------------|----------|-------------|
| `"controlled"` | Controlled Ground Area | Controlled | Lowest iGRC |
| `4` | < 5 people/km² - Remote | Remote | Low |
| `25` | < 50 people/km² - Lightly Populated | Lightly Populated | Low-Medium |
| `250` | < 500 people/km² - Sparsely Populated | Sparsely Populated | Medium |
| `2500` | < 5,000 people/km² - Suburban | Suburban | Medium-High |
| `25000` | < 50,000 people/km² - High Density Metropolitan | Urban | High |
| `50001` | > 50,000 people/km² - Assemblies of People | Assemblies | Highest iGRC |

**✅ Frontend Implementation:**
```html
<select id="populationDensity">
  <option value="controlled">Controlled Ground Area - Ελεγχόμενη Περιοχή</option>
  <option value="4">< 5 people/km² - Remote (Απομακρυσμένη)</option>
  <option value="25">< 50 people/km² - Lightly Populated (Αραιοκατοικημένη)</option>
  <option value="250">< 500 people/km² - Sparsely Populated / Residential Lightly (Αραιή Κατοίκηση)</option>
  <option value="2500">< 5,000 people/km² - Suburban / Low Density Metropolitan (Προάστια)</option>
  <option value="25000">< 50,000 people/km² - High Density Metropolitan (Αστική Περιοχή)</option>
  <option value="50001">> 50,000 people/km² - Assemblies of People (Συγκεντρώσεις Κόσμου)</option>
</select>
```

---

### 2. **M1A Sheltering** (`<select id="m1a_25">`)

**Source:** `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs` line 286-287  
**Backend Implementation:**
```csharp
(GroundRiskMitigation.M1A_Sheltering, RobustnessLevel.Low) => -1,
(GroundRiskMitigation.M1A_Sheltering, RobustnessLevel.Medium) => -2,
```

| Value | Display | Credit | Backend Support |
|-------|---------|--------|-----------------|
| `0` | None (0) | 0 | ✅ |
| `-1` | Low (-1) | -1 | ✅ |
| `-2` | Medium (-2) | -2 | ✅ |

**✅ Frontend Implementation:**
```html
<select id="m1a_25">
  <option value="0">None (0)</option>
  <option value="-1">Low (-1)</option>
  <option value="-2">Medium (-2)</option>
</select>
```

**📚 Note:** Documentation inconsistency exists. Backend SUPPORTS M1A Medium (-2), so frontend MUST include it.

---

### 3. **M1B Operational Restrictions** (`<select id="m1b_25">`)

**Source:** Backend line 289-290  
**Credits:** Medium = -1, High = -2

| Value | Display | Credit |
|-------|---------|--------|
| `0` | None (0) | 0 |
| `-1` | Medium (-1) | -1 |
| `-2` | High (-2) | -2 |

**✅ Frontend Implementation:**
```html
<select id="m1b_25">
  <option value="0">None (0)</option>
  <option value="-1">Medium (-1)</option>
  <option value="-2">High (-2)</option>
</select>
```

---

### 4. **M1C Ground Observation** (`<select id="m1c_25">`)

**Source:** Backend line 292  
**Credits:** Low = -1 ONLY

| Value | Display | Credit |
|-------|---------|--------|
| `0` | None (0) | 0 |
| `-1` | Low (-1) | -1 |

**✅ Frontend Implementation:**
```html
<select id="m1c_25">
  <option value="0">None (0)</option>
  <option value="-1">Low (-1)</option>
</select>
```

---

### 5. **M2 Impact Dynamics** (`<select id="m2_25">`)

**Source:** Backend line 294-295  
**Credits:** Medium = -1, High = -2

| Value | Display | Credit |
|-------|---------|--------|
| `0` | None (0) | 0 |
| `-1` | Medium (-1) | -1 |
| `-2` | High (-2) | -2 |

**✅ Frontend Implementation:**
```html
<select id="m2_25">
  <option value="0">None (0)</option>
  <option value="-1">Medium (-1)</option>
  <option value="-2">High (-2)</option>
</select>
```

---

## 🔄 FIELD VISIBILITY RULES

### When `operationCategory === 'SORA-2.0'`

**SHOW:**
- ✅ `<fieldset id="grc20">` (scenario20, m1_20, m2_20, m3_20)
- ✅ Kinetic Energy display
- ✅ Max Characteristic Dimension (SORA 2.0)

**HIDE:**
- ❌ `<div id="populationDensityField">`
- ❌ `<fieldset id="grc25">` (m1a_25, m1b_25, m1c_25, m2_25)
- ❌ `<div id="arc25Fields">` (U-space, traffic density)

### When `operationCategory === 'SORA-2.5'`

**SHOW:**
- ✅ `<div id="populationDensityField">`
- ✅ `<fieldset id="grc25">` (m1a_25, m1b_25, m1c_25, m2_25)
- ✅ `<div id="arc25Fields">`
- ✅ Max Dimension / Max Speed (SORA 2.5)

**HIDE:**
- ❌ `<fieldset id="grc20">` (scenario20, m1_20, m2_20, m3_20)
- ❌ Kinetic Energy display

---

## 🧪 VALIDATION TESTS

### Test 1: SORA 2.0 Scenario Dropdown
```javascript
const scenario = document.getElementById('scenario20').value;
// Valid values: ControlledGroundArea, VLOS_SparselyPopulated, etc.
// Should map exactly to backend enum OperationalScenario
```

### Test 2: SORA 2.0 M1/M2/M3
```javascript
const m1 = parseInt(document.getElementById('m1_20').value);
// Valid: 0, -1, -2, -4
const m2 = parseInt(document.getElementById('m2_20').value);
// Valid: 0, -1, -2 (NO Low option!)
const m3 = parseInt(document.getElementById('m3_20').value);
// Valid: 1, 0, -1
```

### Test 3: SORA 2.5 Population Density
```javascript
const popDensity = document.getElementById('populationDensity').value;
// Valid: "controlled", "4", "25", "250", "2500", "25000", "50001"
```

### Test 4: SORA 2.5 M1A/M1B/M1C/M2
```javascript
const m1a = parseInt(document.getElementById('m1a_25').value);
// Valid: 0, -1, -2 (Backend SUPPORTS Medium!)
const m1b = parseInt(document.getElementById('m1b_25').value);
// Valid: 0, -1, -2
const m1c = parseInt(document.getElementById('m1c_25').value);
// Valid: 0, -1 (ONLY Low, no Medium/High)
const m2 = parseInt(document.getElementById('m2_25').value);
// Valid: 0, -1, -2
```

---

## ✅ COMPLIANCE CHECKLIST

### SORA 2.0
- [x] Scenario dropdown: 7 options (ControlledGroundArea → BVLOS_GatheringOfPeople)
- [x] M1 Strategic: None/Low/Medium/High (0, -1, -2, -4)
- [x] M2 Impact: None/Medium/High (0, -1, -2) - NO Low!
- [x] M3 ERP: None/Medium/High (+1, 0, -1)
- [x] Kinetic Energy: Auto-computed from MTOM × Speed²
- [x] Hidden when SORA 2.5 selected

### SORA 2.5
- [x] Population Density: 7 categories (Controlled → Assemblies)
- [x] M1A Sheltering: None/Low/Medium (0, -1, -2) - Backend supports Medium!
- [x] M1B Operational: None/Medium/High (0, -1, -2)
- [x] M1C Ground Obs: None/Low (0, -1) - ONLY Low!
- [x] M2 Impact: None/Medium/High (0, -1, -2)
- [x] Max Dimension/Speed: Manual entry fields
- [x] Hidden when SORA 2.0 selected

---

## 🎯 CONCLUSION

**All dropdowns now match backend implementation exactly:**
- ✅ SORA 2.0: 7 scenarios, M1 (4 levels), M2 (3 levels), M3 (3 levels)
- ✅ SORA 2.5: 7 population densities, M1A (3 levels), M1B (3 levels), M1C (2 levels), M2 (3 levels)
- ✅ Field visibility: Correctly shows/hides based on category selection
- ✅ API request mapping: Correctly maps dropdown values to backend enums

**Backend Source of Truth:**
`Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs::GetMitigationCredit_V2_5`
