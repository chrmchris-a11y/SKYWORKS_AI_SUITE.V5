# SKYWORKS AI SUITE V5 - Feature Development TODO

**Last Updated**: November 8, 2025  
**Current Status**: Phase 6 skeleton merged, implementing complete features

---

## 🎯 COMPLETED ✅

- [x] PR #1: Mission Planner UI + Phase 6 Airspace Maps (skeleton)
  - 12-page Mission Planner UI structure
  - Phase 6 Airspace Maps HTML/CSS/JS
  - TypeScript errors fixed (111 → 0)
  - E2E tests (Playwright) configured
  - 9 commits merged to main

---

## 🔴 HIGH PRIORITY (Next Sprint)

### 1. SORA 2.0 vs 2.5 Dynamic Toggle
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/mission-01-basic.html`
- `WebPlatform/wwwroot/app/Pages/ui/assets/mission.js`

**Requirements**:
- Radio buttons: SORA 2.0 | SORA 2.5 | PDRA | STS
- When SORA 2.0 selected → Show M1 (4 levels), M2 (3 levels), M3 (3 levels)
- When SORA 2.5 selected → Show M1A, M1B, M1C, M2, Small-UA rule
- When PDRA selected → Show only PDRA fields (S01, S02)
- When STS selected → Show only STS fields (G01-G03, STS-01/02)
- Dynamic form generation based on selection

**Acceptance Criteria**:
- Toggle switches forms without page reload
- Previous selections preserved when switching back
- Validation rules change per SORA version

---

### 2. PDRA UI Pages
**Status**: ❌ Not Started  
**Files to Create**:
- `WebPlatform/wwwroot/app/Pages/ui/pdra-s01.html` (PDRA-S01: Inspection UAS ≤25kg VLOS)
- `WebPlatform/wwwroot/app/Pages/ui/pdra-s02.html` (PDRA-S02: Aerial Work UAS ≤4kg ≤120m)

**Requirements**:
- Forms for PDRA-S01 and PDRA-S02 scenarios
- Pre-filled mitigation measures (M1, M2, M3, etc.)
- Automatic compliance checks
- Export PDRA declaration (PDF)

**Acceptance Criteria**:
- User can fill PDRA form
- System validates PDRA eligibility
- Generates PDRA declaration document

---

### 3. STS UI Pages
**Status**: ❌ Not Started  
**Files to Create**:
- `WebPlatform/wwwroot/app/Pages/ui/sts-g01.html` (Ground Risk: G01 Controlled Ground Area)
- `WebPlatform/wwwroot/app/Pages/ui/sts-g02.html` (Ground Risk: G02 Sparsely Populated)
- `WebPlatform/wwwroot/app/Pages/ui/sts-g03.html` (Ground Risk: G03 Populated)
- `WebPlatform/wwwroot/app/Pages/ui/sts-01.html` (STS-01: VLOS over controlled ground area)
- `WebPlatform/wwwroot/app/Pages/ui/sts-02.html` (STS-02: BVLOS with airspace observers)

**Requirements**:
- Forms for STS-01 and STS-02 scenarios
- Ground risk category selection (G01/G02/G03)
- Automatic TMPR calculation
- Export STS declaration (PDF)

**Acceptance Criteria**:
- User can select STS scenario
- System validates STS eligibility
- Generates STS declaration document

---

### 4. Job Type Dropdown
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/assets/job-types.json` (NEW)
- `WebPlatform/wwwroot/app/Pages/ui/mission-01-basic.html` (ADD dropdown)

**Job Types**:
```json
[
  {"id": "facade-cleaning", "label": "Facade Cleaning", "category": "cleaning"},
  {"id": "photovoltaic-cleaning", "label": "Photovoltaic Park Cleaning", "category": "cleaning"},
  {"id": "solar-panel-cleaning", "label": "Solar Panels Cleaning", "category": "cleaning"},
  {"id": "roof-cleaning", "label": "Roof Cleaning", "category": "cleaning"},
  {"id": "window-cleaning", "label": "Windows Cleaning", "category": "cleaning"},
  {"id": "inspection-general", "label": "General Inspection", "category": "inspection"},
  {"id": "inspection-infrastructure", "label": "Infrastructure Inspection", "category": "inspection"},
  {"id": "inspection-wind-turbine", "label": "Wind Turbine Inspection", "category": "inspection"},
  {"id": "videography-aerial", "label": "Aerial Videography", "category": "media"},
  {"id": "videography-real-estate", "label": "Real Estate Videography", "category": "media"},
  {"id": "photography-aerial", "label": "Aerial Photography", "category": "media"},
  {"id": "mapping-orthophoto", "label": "Orthophoto Mapping", "category": "mapping"},
  {"id": "mapping-3d-model", "label": "3D Model Mapping", "category": "mapping"},
  {"id": "surveillance", "label": "Surveillance", "category": "security"},
  {"id": "delivery", "label": "Delivery", "category": "logistics"}
]
```

**Acceptance Criteria**:
- Dropdown with all job types
- Filters drone list by job type
- Pre-fills operational parameters

---

### 5. Complete Drone List
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/assets/drones.json` (EXPAND)

**Missing Drones**:
- DJI Mavic 3 Series (Mavic 3, 3 Pro, 3 Cine, 3T, 3E)
- DJI Mini Series (Mini 3 Pro, Mini 4 Pro)
- DJI Air Series (Air 2S, Air 3)
- DJI FPV
- Autel Robotics (EVO II, EVO Lite)
- Parrot Anafi
- Skydio (Skydio 2+, X2)
- Freefly Alta X
- Custom drones (user-defined)

**Acceptance Criteria**:
- At least 50 drones in database
- Filterable by MTOM, category, job type
- User can add custom drone

---

### 6. GRC/ARC/SAIL Calculations (Backend API)
**Status**: ❌ Not Started  
**Files**:
- `Backend/src/Skyworks.Api/Controllers/SoraController.cs` (IMPLEMENT)
- `Backend/src/Skyworks.Core/Services/SoraCalculationService.cs` (NEW)

**API Endpoint**:
```
POST /api/v1/sora/calculate
Body: {
  "version": "2.5",
  "m1a": "Low",
  "m1b": "Medium",
  "m1c": "Low",
  "m2": "Medium",
  "smallUaRule": false,
  "m3": "Medium",
  "tmprSelected": ["TMPR1", "TMPR2"],
  "erpSelected": ["ERP1", "ERP2"]
}
Response: {
  "initialGrc": 3,
  "finalGrc": 2,
  "arc": { "initial": 4, "residual": 2 },
  "sail": 3,
  "oso": ["i", "ii", "iii"]
}
```

**Acceptance Criteria**:
- API calculates GRC/ARC/SAIL correctly
- Validates input parameters
- Returns OSO requirements
- Unit tests: 19/19 passing

---

### 7. Initial/Final Badges (Right Panel)
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/airspace-maps.html` (UPDATE right panel)
- `WebPlatform/wwwroot/app/Pages/ui/assets/airspace.js` (ADD API call)

**Requirements**:
- Display iGRC badge (1-12)
- Display Final GRC badge (1-12)
- Display Initial ARC badge (A/B/C/D)
- Display Residual ARC badge (A/B/C/D)
- Display SAIL badge (I-VI)
- Color-coded by risk level

**Acceptance Criteria**:
- Badges update after SORA calculation
- Color scheme: Green (low), Yellow (medium), Red (high)
- Tooltip with explanation

---

### 8. OSO (i-vi) Selector
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/mission-07-oso.html` (UPDATE)

**Requirements**:
- Checkboxes for OSO#1 through OSO#24
- Auto-select required OSOs based on SAIL
- Mark optional OSOs
- Validation: minimum required OSOs met

**Acceptance Criteria**:
- OSOs auto-populate from SAIL calculation
- User can select additional OSOs
- System validates completeness

---

## 🟡 MEDIUM PRIORITY

### 9. Print/PDF/Email Buttons
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/assets/export.js` (NEW)

**Requirements**:
- Print button → Browser print dialog
- PDF button → Generate PDF with jsPDF
- Email button → Open mailto: with PDF attached
- Include: Mission summary, GRC/ARC/SAIL, Map screenshot

**Acceptance Criteria**:
- PDF includes all mission data
- Map renders in PDF
- Email pre-fills subject/body

---

### 10. Map Responsive Layout
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/assets/styles.css` (FIX)
- `WebPlatform/wwwroot/app/Pages/ui/airspace-maps.html` (ADD full-screen toggle)

**Requirements**:
- Map container should fill available space
- Full-screen toggle button
- Responsive breakpoints (mobile/tablet/desktop)
- Collapsible sidebar

**Acceptance Criteria**:
- Map resizes on window resize
- Full-screen mode works
- Mobile-friendly layout

---

### 11. SORA Calculation Display
**Status**: ❌ Not Started  
**Files**:
- `WebPlatform/wwwroot/app/Pages/ui/assets/airspace.js` (ADD result rendering)

**Requirements**:
- Show calculation steps
- Display rationale (why GRC=X, ARC=Y)
- Link to SORA annexes
- Export calculation report

**Acceptance Criteria**:
- User understands calculation logic
- Can export calculation to PDF
- Links to EASA AMC/GM docs

---

## 🟢 LOW PRIORITY (Future)

### 12. Offline Tile Caching
**Status**: ❌ Not Started  
**Requirements**: Cache map tiles for offline use

### 13. NOTAM Integration
**Status**: ❌ Not Started  
**Requirements**: Fetch NOTAMs from EUROCONTROL API

---

## 📊 Progress Tracking

**Total Tasks**: 13  
**Completed**: 1 (PR #1 skeleton)  
**In Progress**: 0  
**Not Started**: 12  

**Estimated Completion**: Phase 7 (December 2025)

---

## 🚀 Next Actions

1. ✅ Merge PR #1
2. ⏳ Create new branch `feat/complete-ui-features`
3. ⏳ Start with HIGH priority #1 (SORA toggle)
4. ⏳ Implement features 1-8 (HIGH priority)
5. ⏳ Create PR #2 with complete features
6. ⏳ Merge to main
7. ⏳ Tag v0.7.0

---

**End of TODO**
