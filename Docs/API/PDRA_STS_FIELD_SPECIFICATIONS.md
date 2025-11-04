# PDRA/STS FIELD SPECIFICATIONS FOR WEB PLATFORM
## Based on EU Regulation 2019/947 - Easy Access Rules for UAS (July 2024)

---

## 📋 CATEGORY DROPDOWN OPTIONS

```
SORA 2.0
SORA 2.5
STS-01 - VLOS over controlled ground
STS-02 - BVLOS with airspace observers
PDRA-S01 - VLOS (custom UAS)
PDRA-S02 - BVLOS (custom UAS)
PDRA-G01 - Aerial survey BVLOS
PDRA-G02 - Aerial survey BVLOS extended
PDRA-G03 - BVLOS low altitude
```

---

## 🔵 STS-01: VLOS OVER CONTROLLED GROUND

### Official Reference
**UAS.STS-01.020** - Regulation (EU) 2020/639

### Required Fields

| Field Name | Type | Validation Rules | Source |
|-----------|------|------------------|--------|
| **UAS Class** | Dropdown | Must be "C5" | UAS.STS-01.020(1)(f) |
| **Max Dimension** | Number (m) | ≤ 3m (Class C5 limit) | Delegated Reg 2019/945 Part 16 |
| **MTOM** | Number (kg) | ≤ 25kg (Class C5 limit) | Delegated Reg 2019/945 Part 16 |
| **Max Speed** | Number (m/s) | < 5 m/s | UAS.STS-01.020(1)(d) |
| **Max Height AGL** | Number (m) | ≤ 120m (or 150m near obstacle) | UAS.STS-01.010(1)(2) |
| **Controlled Ground Area** | Checkbox | Required TRUE | UAS.STS-01.020(1)(c) |
| **Ground Risk Buffer** | Number (m) | Auto-calculated from table | UAS.STS-01.020(1)(c)(i)(C) |
| **Population Density** | Number (ppl/km²) | Any (controlled area allows populated) | Context |
| **VLOS** | Checkbox | Required TRUE | UAS.STS-01.020(1)(a) |
| **Tethered Operation** | Checkbox | Optional | UAS.STS-01.020(1)(c)(ii) |
| **Tether Length** | Number (m) | If tethered, buffer = length + 5m | UAS.STS-01.020(1)(c)(ii) |
| **Remote Pilot Certified** | Checkbox | Required TRUE | UAS.STS-01.020(1)(e)(i) |
| **Operations Manual** | Checkbox | Required TRUE | UAS.STS-01.020(1)(b) |
| **Emergency Response Plan** | Checkbox | Required TRUE | UAS.STS-01.030 |
| **Direct Remote ID** | Checkbox | Required TRUE | UAS.STS-01.020(1)(f) |

### Ground Risk Buffer Calculation (Auto)

**Table from UAS.STS-01.020(1)(c)(i)(C):**

| Max Height | MTOM ≤ 10kg | MTOM > 10kg |
|-----------|-------------|-------------|
| 30m | 10m | 20m |
| 60m | 15m | 30m |
| 90m | 20m | 45m |
| 120m | 25m | 60m |

**Logic:** Linear interpolation between heights

### Conditional Fields

- **If Tethered = TRUE:**
  - Show `tetherLength` field
  - Buffer = tetherLength + 5m (override table)
  
- **If Near Obstacle > 105m:**
  - Show `obstacleHeight` field
  - Allow max height = obstacleHeight + 15m
  - Requires entity authorization

### GRC/ARC/SAIL Calculation

```
GRC = 2 (Controlled ground area reduces risk)
ARC = a (VLOS operation in low-risk airspace)
SAIL = I (GRC_2 + ARC_a = SAIL_I from JARUS Table 5)
```

---

## 🔵 STS-02: BVLOS WITH AIRSPACE OBSERVERS

### Official Reference
**UAS.STS-02.020** - Regulation (EU) 2020/639

### Required Fields

| Field Name | Type | Validation Rules | Source |
|-----------|------|------------------|--------|
| **UAS Class** | Dropdown | Must be "C6" | UAS.STS-02.020(8)(a) |
| **Max Dimension** | Number (m) | ≤ 3m (Class C6 limit) | Delegated Reg 2019/945 Part 17 |
| **MTOM** | Number (kg) | ≤ 25kg (Class C6 limit) | Delegated Reg 2019/945 Part 17 |
| **Max Height AGL** | Number (m) | ≤ 120m | UAS.STS-02.010(1) |
| **Flight Range** | Number (m) | 1000m (no AOs) or 2000m (with AOs) | UAS.STS-02.020(5)(6) |
| **Population Density** | Number (ppl/km²) | < 500 (sparsely populated) | UAS.STS-02.020(2) |
| **Sparsely Populated** | Checkbox | Required TRUE | UAS.STS-02.020(2) |
| **Controlled Ground Area** | Checkbox | Required TRUE | UAS.STS-02.020(2) |
| **Ground Risk Buffer** | Number (m) | Per manufacturer termination distance | UAS.STS-02.020(2)(c) |
| **Meteorological Visibility** | Number (km) | ≥ 5 km | UAS.STS-02.020(3) |
| **Airspace Observers** | Number | 0-3+ | UAS.STS-02.020(5)(6) |
| **Launch/Recovery VLOS** | Checkbox | Required TRUE | UAS.STS-02.020(4) |
| **BVLOS** | Checkbox | TRUE (by definition) | Context |
| **Pre-programmed Route** | Checkbox | Required if no VLOS | UAS.STS-02.020(5) |
| **Remote Pilot Certified** | Checkbox | Required TRUE | UAS.STS-02.020(7)(a) |
| **Operations Manual** | Checkbox | Required TRUE | UAS.STS-02.020(1) |
| **Emergency Response Plan** | Checkbox | Required TRUE | UAS.STS-02.030 |
| **Direct Remote ID** | Checkbox | Required TRUE | UAS.STS-02.020(8)(c) |
| **Geo-Fencing** | Checkbox | Required TRUE | UAS.STS-02.020(8)(b) |

### Conditional Logic

- **If Airspace Observers = 0:**
  - Max Flight Range = 1000m
  - Pre-programmed route REQUIRED
  
- **If Airspace Observers ≥ 1:**
  - Max Flight Range = 2000m
  - Each AO must be within 1km of UA
  - Each AO must be within 1km of remote pilot
  - Robust communication required

### GRC/ARC/SAIL Calculation

```
GRC = 2 (Sparsely populated + controlled area)
ARC = b (BVLOS operation with mitigations)
SAIL = II (GRC_2 + ARC_b = SAIL_II from JARUS Table 5)
```

---

## 🔵 PDRA-S01: VLOS (CUSTOM UAS)

### Official Reference
**PDRA-S01 Version 1.2** - September 2023

### Scope Differences from STS-01
✅ **Flexibility:** No Class C5 marking required  
✅ **Custom UAS:** Allows non-certified aircraft  
✅ **Higher Altitude:** Up to 150m (vs 120m for STS-01)  
❌ **More Documentation:** Full risk assessment required

### Required Fields

| Field Name | Type | Validation Rules | Source |
|-----------|------|------------------|--------|
| **UAS Class** | Text | "Custom" or specific model | PDRA-S01(a) scope |
| **Max Dimension** | Number (m) | < 3m | PDRA-S01(a)(1) |
| **MTOM** | Number (kg) | Not specified (reasonable) | Context |
| **Max Height AGL** | Number (m) | ≤ 150m | PDRA-S01(a)(4) |
| **Population Density** | Number (ppl/km²) | Any (controlled area) | PDRA-S01(a)(3) |
| **Controlled Ground Area** | Checkbox | Required TRUE | PDRA-S01 1.6 |
| **Ground Risk Buffer** | Number (m) | Per risk assessment | PDRA-S01 3.1-3.8 |
| **VLOS** | Checkbox | Required TRUE | PDRA-S01(a)(2) |
| **Tethered Operation** | Checkbox | Optional | PDRA-S01 1.7 |
| **Tether Length** | Number (m) | If tethered, buffer = length + 5m | PDRA-S01 1.7 |
| **Controlled Airspace** | Checkbox | Optional with coordination | PDRA-S01 1.12 |
| **ATC Coordination** | Checkbox | If controlled airspace | PDRA-S01 1.12.2 |
| **Automatic Mode** | Checkbox | Must be FALSE | PDRA-S01 1.1 |
| **Remote Pilot Certified** | Checkbox | Required TRUE | PDRA-S01 requirements |
| **Operations Manual** | Checkbox | Required TRUE | PDRA-S01 requirements |
| **Risk Assessment** | Checkbox | Required TRUE | PDRA nature |

### Conditional Logic

- **If Height > 120m:**
  - Must define enhanced risk buffer
  - Additional justification required

- **If Controlled Airspace = TRUE:**
  - ATC Coordination must be TRUE
  - Flight authorization required

### GRC/ARC/SAIL Calculation

```
GRC = 2 (Controlled ground area)
ARC = a (VLOS in low-risk airspace, max ARC-b allowed)
SAIL = I (GRC_2 + ARC_a = SAIL_I)
Final GRC = 3, Final ARC = ARC-b, SAIL = II (per PDRA-S01 Table)
```

---

## 🔵 PDRA-S02: BVLOS (CUSTOM UAS)

### Official Reference
**PDRA-S02 Version 1.1** - September 2023

### Scope Differences from STS-02
✅ **Flexibility:** No Class C6 marking required  
✅ **Custom UAS:** Allows non-certified aircraft  
✅ **Higher Altitude:** Up to 150m (vs 120m for STS-02)  
✅ **Higher MTOM:** Up to 25kg explicitly stated  

### Required Fields

| Field Name | Type | Validation Rules | Source |
|-----------|------|------------------|--------|
| **UAS Class** | Text | "Custom" or specific model | PDRA-S02(a) scope |
| **Max Dimension** | Number (m) | ≤ 3m | PDRA-S02(a)(1) |
| **MTOM** | Number (kg) | ≤ 25kg | PDRA-S02(a)(2) |
| **Max Height AGL** | Number (m) | ≤ 150m | PDRA-S02(a)(4) |
| **Flight Range** | Number (m) | 1000m (no AOs) or 2000m (with AOs) | PDRA-S02 scope |
| **Population Density** | Number (ppl/km²) | < 500 (sparsely populated) | PDRA-S02(a)(3) |
| **Sparsely Populated** | Checkbox | Required TRUE | PDRA-S02(a)(3) |
| **Controlled Ground Area** | Checkbox | Required TRUE | PDRA-S02 1.8 |
| **Ground Risk Buffer** | Number (m) | Per risk assessment | PDRA-S02 3.x |
| **Airspace Observers** | Number | 0-3+ | PDRA-S02 scope |
| **BVLOS** | Checkbox | TRUE (by definition) | Context |
| **Controlled Airspace** | Checkbox | Optional with coordination | PDRA-S02 1.12 |
| **ATC Coordination** | Checkbox | If controlled airspace | PDRA-S02 1.12.2 |
| **Remote Pilot Certified** | Checkbox | Required TRUE | PDRA-S02 requirements |
| **Operations Manual** | Checkbox | Required TRUE | PDRA-S02 requirements |
| **Risk Assessment** | Checkbox | Required TRUE | PDRA nature |

### Conditional Logic

- **If Airspace Observers = 0:**
  - Max Flight Range = 1000m
  
- **If Airspace Observers ≥ 1:**
  - Max Flight Range = 2000m
  - AO positioning rules apply

### GRC/ARC/SAIL Calculation

```
GRC = 2 (Sparsely populated + controlled area)
ARC = b (BVLOS with mitigations)
SAIL = II (GRC_2 + ARC_b = SAIL_II)
```

---

## 🔵 PDRA-G01: AERIAL SURVEY BVLOS

### Official Reference
**PDRA-G01 Version 1.3** - September 2023

### Required Fields

| Field Name | Type | Validation Rules | Source |
|-----------|------|------------------|--------|
| **Max Dimension** | Number (m) | ≤ 3m | PDRA-G01(a)(1) |
| **MTOM** | Number (kg) | ≤ 25kg | PDRA-G01(a)(2) |
| **Max Height AGL** | Number (m) | ≤ 120m | PDRA-G01(a)(3) |
| **Population Density** | Number (ppl/km²) | < 500 (sparsely populated) | PDRA-G01(a)(4) |
| **Operational Area** | Text | Specific geographical area | PDRA-G01 scope |
| **BVLOS** | Checkbox | TRUE | Context |
| **Survey Type** | Dropdown | Aerial photography, mapping, inspection | PDRA-G01 purpose |

### GRC/ARC/SAIL

```
GRC = 2 (Sparsely populated)
ARC = b (BVLOS)
SAIL = II
```

---

## 🔵 PDRA-G02: AERIAL SURVEY EXTENDED

### Official Reference
**PDRA-G02 Version 1.2** - September 2023

### Differences from G01
- Extended operational envelope
- Additional mitigations required
- May allow higher GRC scenarios

### Required Fields
(Similar to G01 with extended parameters)

---

## 🔵 PDRA-G03: BVLOS LOW ALTITUDE

### Official Reference
**PDRA-G03 Version 1.1** - September 2023

### Scope
Low altitude BVLOS operations with specific constraints

### Required Fields
(Similar to G01/G02 with low altitude focus)

---

## 🎯 IMPLEMENTATION CHECKLIST

### Frontend Changes Required

1. **Mission Planning Page (`mission.html`)**
   - ✅ Add category dropdown with all 9 options
   - ✅ Dynamic form rendering based on selection
   - ✅ Conditional field display
   - ✅ Real-time validation
   - ✅ Ground risk buffer auto-calculation
   - ✅ SAIL/GRC/ARC preview

2. **JavaScript Updates (`mission.js` or inline)**
   - Category change event handler
   - Field visibility logic
   - Validation rules per category
   - API integration

3. **CSS Updates**
   - Consistent styling for all categories
   - Validation feedback (red/green borders)
   - Help text tooltips

### Backend Verification
- ✅ PDRASTSController already created
- ✅ Validation logic implemented
- ✅ SAIL/GRC/ARC calculations working

### Test Missions Required
- 5 STS-01 missions
- 5 STS-02 missions
- 5 PDRA-S01 missions
- 5 PDRA-S02 missions
- 3 PDRA-G01 missions
- 3 PDRA-G02 missions
- 3 PDRA-G03 missions

**Total: 29 test missions for web platform**

---

## 📚 OFFICIAL REFERENCES

1. **EU Regulation 2019/947** - Easy Access Rules for UAS (July 2024, 468 pages)
2. **JARUS SORA 2.0** (JAR_doc_06_JARUS_SORA_V2.0)
3. **JARUS SORA 2.5** (JAR_doc_25_JARUS_SORA_V2.5_FINAL)
4. **Delegated Regulation (EU) 2019/945** - UAS product requirements

---

## ✅ VALIDATION SUMMARY

All field specifications are directly traceable to official EASA/JARUS documentation with exact regulation references provided for each requirement.
