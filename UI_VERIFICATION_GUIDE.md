# 🎯 MISSION.HTML UI VERIFICATION GUIDE
**Live Testing Instructions - SORA 2.0 vs 2.5 Fields**

Date: October 30, 2025  
URL: http://localhost:5210/app/Pages/mission.html

---

## 📋 TESTING CHECKLIST

### **Step 1: Select SORA 2.0**

1. **Open mission.html** (should now be open in browser)
2. **Find "Operation Category" dropdown** (top of page)
3. **Select: "SORA 2.0 (Legacy AMC)"**

---

### ✅ **VERIFY SORA 2.0 FIELDS ARE VISIBLE:**

#### **GRC Section (Ground Risk):**

**📍 Location:** Scroll to "SORA 2.0 Fields" section (green background)

| Field | Element ID | Dropdown Options | Status |
|-------|------------|------------------|--------|
| **M1 - Strategic** | `#m1_20` | None (0), Low (-1), Medium (-2), High (-4) | ✅ Should see 4 options |
| **M2 - Impact** | `#m2_20` | None (0), **Low (-1)**, High (-2) | ✅ **CRITICAL: Must say "Low" NOT "Medium"** |
| **M3 - ERP** | `#m3_20` | None (+1), Medium (0), High (-1) | ✅ Should see 3 options |

**🔍 CRITICAL CHECK:**
```
M2 dropdown MUST show:
  • None (0)
  • Low (-1)     ← THIS IS CORRECT (was "Medium" before fix)
  • High (-2)
```

#### **ARC Section (Air Risk):**

**📍 Location:** Scroll to "Air Risk" section

| Field | Element ID | What to Verify |
|-------|------------|----------------|
| **Airspace Class** | `#airspaceClass` | Should have: G, E, D, C, CTR, TMA, TIZ, ATZ, RMZ, TMZ (NO U-space) |
| **Airspace Segregation** | `#airspaceSegregation` | None, Partial, Full_Certified |

**🔍 VERIFY HIDDEN:**
- ❌ **SORA 2.5 Enhanced ARC box** (blue box with U-space fields) should be **HIDDEN**
- ❌ Population Density field should be **HIDDEN**

---

### **Step 2: Switch to SORA 2.5**

1. **Change "Operation Category"** dropdown
2. **Select: "SORA 2.5 (JARUS Latest Version)"**
3. **Watch the page change!**

---

### ✅ **VERIFY SORA 2.5 FIELDS ARE VISIBLE:**

#### **GRC Section (Ground Risk):**

**📍 Location:** Scroll to "SORA 2.5 Fields" section (blue background)

| Field | Element ID | Dropdown Options | N/A Levels | Status |
|-------|------------|------------------|------------|--------|
| **M1(A) - Sheltering** | `#m1a_25` | None (0), Low (-1), Medium (-2) | High is N/A | ✅ Should see 3 options |
| **M1(B) - Operational** | `#m1b_25` | None (0), Medium (-1), High (-2) | Low is N/A | ✅ Should see 3 options |
| **M1(C) - Ground Obs** | `#m1c_25` | None (0), Low (-1) | Med/High N/A | ✅ Should see 2 options |
| **M2 - Impact** | `#m2_25` | None (0), Medium (-1), High (-2) | Low is N/A | ✅ Should see 3 options |

**🔍 NOTE:** M3 (Emergency Response Plan) does NOT exist in SORA 2.5 - should be hidden!

#### **Required Input Fields:**

**📍 Location:** Top of SORA 2.5 section

| Field | Element ID | Required? | Status |
|-------|------------|-----------|--------|
| **Population Density** | `#populationDensity` | ✅ YES | Should be **VISIBLE** now |
| **Max Characteristic Dimension (m)** | `#maxDimension` | ✅ YES | Should be visible |
| **Max Speed (m/s)** | `#maxSpeed` | ✅ YES | Should be visible |

**🔍 CRITICAL CHECK:**
```
Population Density dropdown should NOW be visible and show:
  • Controlled Ground Area
  • < 5 people/km²
  • < 50 people/km²
  • < 500 people/km²
  • < 5,000 people/km²
  • < 50,000 people/km²
  • > 50,000 people/km²
```

#### **ARC Section - SORA 2.5 Enhanced Fields:**

**📍 Location:** Scroll to Air Risk section - look for BLUE BOX with title:
**"🛰️ SORA 2.5 Enhanced ARC Inputs (JARUS Annex B)"**

**🔍 VERIFY THIS BOX IS NOW VISIBLE:**

| Field | Element ID | Options | Status |
|-------|------------|---------|--------|
| **U-space Services Available** | `#uSpaceServices` | No, Yes (requires U-space airspace) | ✅ NEW |
| **Traffic Density Data Source** | `#trafficDensitySource` | Empirical, Statistical, Expert (Low only) | ✅ NEW |
| **Airspace Containment** | `#airspaceContainment25` | None, Operational, Certified | ✅ NEW |
| **Temporal Segregation** | `#temporalSegregation` | Checkbox | ✅ NEW |
| **Spatial Segregation** | `#spatialSegregation` | Checkbox | ✅ NEW |

**🔍 CRITICAL CHECK:**
```
The blue box at the bottom should say:
"ℹ️ These fields are SORA 2.5 specific per JARUS Annex B. 
    Not applicable to SORA 2.0 AMC."
```

---

## 🎬 **LIVE TESTING SCENARIOS**

### **Scenario 1: SORA 2.0 → 2.5 Switching**

```
1. Start with SORA 2.0 selected
2. Verify M2 shows "Low" (not "Medium") ✅
3. Verify blue ARC box is HIDDEN ❌
4. Switch to SORA 2.5
5. Watch fields change!
6. Verify blue ARC box appears ✅
7. Verify Population Density appears ✅
8. Verify M1 split into M1A/M1B/M1C ✅
9. Verify M3 disappears ❌
```

### **Scenario 2: SORA 2.5 Field Validation**

```
1. Select SORA 2.5
2. Try to select M1A = Medium
3. Try to select M1B = Medium
4. Watch for validation warning:
   "⚠️ M1(A) Medium cannot be combined with M1(B)"
```

### **Scenario 3: U-space Services**

```
1. Select SORA 2.5
2. Scroll to blue ARC box
3. Find "U-space Services Available"
4. Select "Yes"
5. Verify help tooltip says:
   "Detect-and-Avoid (DAA) or tracking services per JARUS Annex B"
```

### **Scenario 4: Traffic Density Validation**

```
1. Select SORA 2.5
2. In ARC section, find "Traffic Density Data Source"
3. Notice the options:
   - Empirical (Direct observation)
   - Statistical (Historical data)
   - Expert (Low density only) ← Note the warning!
4. Tooltip should say:
   "Expert NOT valid for Medium/High density"
```

---

## 📸 **VISUAL CHECKPOINTS**

### **SORA 2.0 Layout:**

```
┌─────────────────────────────────────────┐
│ Operation Category: SORA 2.0 (Legacy)   │
└─────────────────────────────────────────┘

┌─ SORA 2.0 Fields (GREEN BOX) ──────────┐
│                                         │
│ M1 - Strategic    [None/Low/Med/High]  │
│ M2 - Impact       [None/Low/High] ✅   │
│ M3 - ERP          [None/Med/High]      │
│                                         │
└─────────────────────────────────────────┘

┌─ Air Risk ──────────────────────────────┐
│                                         │
│ Airspace Class    [G-TMZ, no U-space]  │
│ Airspace Segregation [None/Part/Full]  │
│                                         │
│ ❌ NO BLUE BOX (SORA 2.5 fields hidden)│
└─────────────────────────────────────────┘
```

### **SORA 2.5 Layout:**

```
┌─────────────────────────────────────────┐
│ Operation Category: SORA 2.5 (JARUS)    │
└─────────────────────────────────────────┘

┌─ SORA 2.5 Fields (BLUE BOX) ───────────┐
│                                         │
│ Population Density ✅ [VISIBLE NOW]    │
│ Max Dimension (m)  [Required]          │
│ Max Speed (m/s)    [Required]          │
│                                         │
│ M1(A) - Sheltering [None/Low/Med]      │
│ M1(B) - Operational [None/Med/High]    │
│ M1(C) - Ground Obs  [None/Low]         │
│ M2 - Impact        [None/Med/High]     │
│                                         │
│ ⚠️ M1A Med + M1B validation warning    │
└─────────────────────────────────────────┘

┌─ Air Risk ──────────────────────────────┐
│                                         │
│ Airspace Class [G-TMZ + U-space] ✅    │
│                                         │
│ ┌─ 🛰️ SORA 2.5 Enhanced ARC (BLUE) ─┐│
│ │ ✅ U-space Services    [Yes/No]    ││
│ │ ✅ Traffic Density     [Emp/Stat]  ││
│ │ ✅ Airspace Containment [None/Oper]││
│ │ ✅ Temporal Segregation ☑️         ││
│ │ ✅ Spatial Segregation  ☑️         ││
│ │                                    ││
│ │ ℹ️ SORA 2.5 specific per Annex B   ││
│ └────────────────────────────────────┘│
└─────────────────────────────────────────┘
```

---

## ✅ **COMPLIANCE VERIFICATION CHECKLIST**

### **SORA 2.0 Compliance:**

- [ ] M1 has 4 levels (None/Low/Medium/High)
- [ ] M2 has 3 levels (None/**Low**/High) ← **CRITICAL: Must be "Low" not "Medium"**
- [ ] M3 has 3 levels (None/Medium/High)
- [ ] Airspace Segregation visible (None/Partial/Full_Certified)
- [ ] Population Density field **HIDDEN**
- [ ] Blue SORA 2.5 ARC box **HIDDEN**

### **SORA 2.5 Compliance:**

- [ ] Population Density field **VISIBLE**
- [ ] M1 split into M1A/M1B/M1C (3 separate dropdowns)
- [ ] M1A has 3 levels (None/Low/Medium) - High N/A
- [ ] M1B has 3 levels (None/Medium/High) - Low N/A
- [ ] M1C has 2 levels (None/Low) - Med/High N/A
- [ ] M2 has 3 levels (None/Medium/High) - Low N/A
- [ ] M3 **REMOVED** (not in SORA 2.5)
- [ ] Blue ARC box **VISIBLE** with 5 new fields:
  - [ ] U-space Services dropdown
  - [ ] Traffic Density Source dropdown
  - [ ] Airspace Containment dropdown
  - [ ] Temporal Segregation checkbox
  - [ ] Spatial Segregation checkbox
- [ ] Info message: "SORA 2.5 specific per JARUS Annex B"

---

## 🚨 **KNOWN ISSUES TO CHECK FOR:**

### ❌ **Bug #1: M2 showing "Medium" instead of "Low" (SORA 2.0)**
**Status:** ✅ **FIXED** (should now show "Low")

**How to verify:**
1. Select SORA 2.0
2. Look at M2 dropdown
3. Should see: None (0), **Low (-1)**, High (-2)
4. If you see "Medium" - BUG NOT FIXED!

### ❌ **Bug #2: SORA 2.5 fields not showing**
**Status:** ✅ **FIXED** (should now appear)

**How to verify:**
1. Select SORA 2.5
2. Scroll to bottom of Air Risk section
3. Should see blue box with "🛰️ SORA 2.5 Enhanced ARC Inputs"
4. If box is missing - BUG NOT FIXED!

### ❌ **Bug #3: Population Density not visible (SORA 2.5)**
**Status:** ✅ **FIXED** (should now appear)

**How to verify:**
1. Select SORA 2.5
2. Look in GRC section
3. Should see "Population Density" dropdown
4. If hidden - BUG NOT FIXED!

---

## 📊 **BACKEND VERIFICATION**

After testing the UI, verify backend receives correct data:

1. Fill out a mission with SORA 2.5
2. Click "Run Evaluation"
3. Open Browser DevTools (F12) → Network tab
4. Find POST request to `/api/sora/evaluate`
5. Check Request Payload should include:

```json
{
  "version": "SORA_2.5",
  "grc_inputs": {
    "population_density_p_km2": 100,
    "characteristic_dimension_m": 1.5,
    "cruise_speed_m_s": 20.0,
    "m1a_sheltering": "Low",
    "m1b_operational": "None",
    "m1c_ground_observation": "None",
    "m2_impact": "Medium"
  },
  "arc_inputs": {
    "u_space_services_available": false,
    "traffic_density_data_source": "Statistical",
    "airspace_containment": "Operational",
    "temporal_segregation": true,
    "spatial_segregation": false
  }
}
```

---

## 🎯 **SUCCESS CRITERIA**

### ✅ **All Tests Pass If:**

1. **SORA 2.0:**
   - M2 shows "Low" (not "Medium")
   - 3 mitigations visible (M1/M2/M3)
   - No blue SORA 2.5 box
   - Population density hidden

2. **SORA 2.5:**
   - Population density visible
   - 4 mitigations visible (M1A/M1B/M1C/M2)
   - Blue ARC box with 5 new fields
   - M3 removed

3. **Switching:**
   - Fields show/hide correctly
   - No JavaScript errors in console
   - Smooth transitions

4. **Backend:**
   - API accepts payloads
   - Validation works correctly
   - Returns proper SAIL/OSO results

---

## 📞 **HELP & DEBUGGING**

### **If fields don't show/hide correctly:**

1. Open Browser Console (F12)
2. Look for JavaScript errors
3. Check `onCategoryChanged()` function is called
4. Verify element IDs match:
   - `#grc20` (SORA 2.0 container)
   - `#grc25` (SORA 2.5 container)
   - `#arc25Fields` (SORA 2.5 ARC box)
   - `#populationDensityField`

### **If dropdowns have wrong options:**

1. View page source
2. Search for dropdown ID (e.g., `#m2_20`)
3. Verify `<option>` values match backend enum
4. Compare with `grc_models.py` and `arc_models.py`

### **If validation doesn't work:**

1. Check backend logs
2. Verify API endpoint is running (http://localhost:5210)
3. Check network requests in DevTools
4. Look for 400/422 validation errors

---

## ✅ **FINAL VERIFICATION COMMAND**

Run automated verification:
```bash
python Tools/verify_dropdowns_detailed.py
```

Expected output:
```
✅ 16/16 CHECKS PASSING
📈 SUCCESS RATE: 100.0%
✅ ALL DROPDOWNS VERIFIED - 100% COMPLIANT!
```

---

**Date Created:** October 30, 2025  
**Last Updated:** October 30, 2025  
**Status:** ✅ Ready for live testing  
**URL:** http://localhost:5210/app/Pages/mission.html
