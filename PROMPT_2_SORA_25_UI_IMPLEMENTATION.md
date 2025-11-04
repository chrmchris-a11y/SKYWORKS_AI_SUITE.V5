# PROMPT 2: SORA 2.5 Enhanced ARC Input Fields - UI Implementation & Styling

## Executive Summary
This prompt addresses the **5 new SORA 2.5-specific ARC input fields** that have been added to the mission.html UI but require:
1. Proper styling and visual consistency (especially checkboxes)
2. Complete JARUS reference documentation
3. User experience improvements
4. Verification that all fields match backend models exactly

These fields are **ONLY for SORA 2.5** and represent enhanced strategic mitigations introduced in JARUS SORA 2.5 (JAR_doc_25 + Annexes).

## Official JARUS References

### Primary Document
**JARUS SORA 2.5 Main Body** (JAR_doc_25)  
**Edition**: 2.5  
**Date**: 22.11.2024  
**Section**: Step #5 - Application of Strategic Mitigations to Reduce Initial ARC

### Supporting Annexes
1. **Annex H** (JAR_doc_34) - SORA Safety Services (U-space Services)
2. **Annex C** (v1.0) - Strategic Mitigations Collision Risk Assessment
3. **Annex D** (v1.0) - Tactical Mitigations Collision Risk Assessment

**Note**: SORA 2.5 introduced significant changes to the ARC methodology compared to SORA 1.0/2.0, particularly in strategic mitigations.

## The 5 New SORA 2.5 ARC Fields

### Field 1: U-space Services Available
**Type**: Boolean (Yes/No dropdown)  
**HTML ID**: `#uSpaceServices`  
**Backend Model**: `ARCInputs25.u_space_services_available` (bool)

**Official Reference**: JAR_doc_34 (Annex H), Section H.1.1
> "Service usage is not limited to any particular airspace or altitude constraint/band/limitation. Therefore, this Annex refers to 'Service Providers' (SP), recognizing that the competent authority may decide how and where those services may be used (e.g. via UTM/U-Space)."

**Definition**: U-space services are digital aviation services that support safe, efficient UAS operations, including:
- Geofencing and no-fly zones
- Flight planning and authorization
- Tracking and monitoring
- Conflict detection and resolution
- Weather information

**Impact on ARC**: When U-space services are available and used, they provide additional situational awareness and strategic deconfliction capabilities that can reduce Initial ARC.

**UI Implementation** (lines 1117-1122):
```html
<label>U-space Services Available
  <select id="uSpaceServices">
    <option value="false">No</option>
    <option value="true">Yes (requires U-space airspace)</option>
  </select>
</label>
```

**Status**: ✅ Implemented correctly

---

### Field 2: Traffic Density Data Source
**Type**: Enum (3 options)  
**HTML ID**: `#trafficDensitySource`  
**Backend Model**: `TrafficDensityDataSource` enum (Empirical/Statistical/Expert)

**Official Reference**: JAR_doc_25 (Main Body), Step #4 - Determination of Initial ARC  
The quality and reliability of traffic density data affects the robustness of the ARC determination.

**Data Source Types**:
1. **Empirical**: Direct observation of actual traffic (highest reliability)
   - Real-time surveillance data
   - Historical ADS-B/FLARM records
   - Visual observations with documentation
   
2. **Statistical**: Historical data analysis (medium reliability)
   - Statistical models based on flight patterns
   - Airport traffic statistics extrapolation
   - Regional aviation activity databases
   
3. **Expert**: Expert judgment (lowest reliability, only for low density)
   - Qualified expert assessment
   - Valid only for low-density airspace
   - Requires substantiation

**Impact on ARC**: More reliable data sources allow for more accurate Initial ARC classification. Expert judgment is **only valid for low traffic density** areas.

**UI Implementation** (lines 1124-1130):
```html
<label>Traffic Density Data Source
  <select id="trafficDensitySource">
    <option value="Empirical">Empirical (Direct observation)</option>
    <option value="Statistical">Statistical (Historical data)</option>
    <option value="Expert">Expert (Low density only)</option>
  </select>
</label>
```

**Status**: ✅ Implemented correctly

---

### Field 3: Airspace Containment
**Type**: Enum (3 options)  
**HTML ID**: `#airspaceContainment25`  
**Backend Model**: `AirspaceContainment25` enum (None/Operational/Certified)

**Official Reference**: JAR_doc_25 (Main Body), Step #5 - Strategic Mitigations  
Airspace containment is a strategic mitigation that restricts manned aircraft access to the operational volume.

**Containment Levels**:
1. **None**: No airspace containment
   - Standard airspace rules apply
   - Manned aircraft can enter operational volume
   - No reduction in Initial ARC
   
2. **Operational**: Operational-level containment (Medium robustness)
   - Temporary airspace restrictions (NOTAMs)
   - Coordinated with ANSP (Air Navigation Service Provider)
   - Procedures to exclude manned traffic
   - Requires ANSP approval and monitoring
   
3. **Certified**: Certified containment (High robustness)
   - Permanent segregated airspace (e.g., restricted zone)
   - Certified by competent authority
   - Technical and procedural barriers
   - Highest level of assurance

**Impact on ARC**: Higher containment levels provide greater reduction of Initial ARC by reducing the probability of manned aircraft encounter.

**UI Implementation** (lines 1132-1138):
```html
<label>Airspace Containment
  <select id="airspaceContainment25">
    <option value="None">None</option>
    <option value="Operational">Operational</option>
    <option value="Certified">Certified</option>
  </select>
</label>
```

**Status**: ✅ Implemented correctly

---

### Field 4: Temporal Segregation
**Type**: Boolean (checkbox)  
**HTML ID**: `#temporalSegregation`  
**Backend Model**: `ARCInputs25.temporal_segregation` (bool)

**Official Reference**: SORA 1.0 Annex C (Strategic Mitigations) - Concept carried forward to SORA 2.5  
"Strategic Mitigation by Operational Restrictions" - Chronology-based restrictions

**Definition**: Temporal segregation means the UAS operation is conducted during time periods when manned aviation activity is significantly reduced or absent.

**Examples**:
- Operations at night (when VFR traffic is minimal)
- Operations outside business hours (when general aviation is inactive)
- Coordination with ANSP to avoid peak traffic times
- Operations during weather conditions prohibitive to manned aviation

**Validation Requirements**:
- Time-based operational restrictions in ConOps
- Evidence of reduced manned traffic during selected times
- Procedures to ensure compliance with time windows
- Contingency plans if time window is exceeded

**Impact on ARC**: Reduces the probability of encounter with manned aircraft by ensuring temporal separation.

**UI Implementation** (lines 1140-1144):
```html
<label>
  <input type="checkbox" id="temporalSegregation" value="true" />
  <span>Temporal Segregation</span>
</label>
```

**Status**: ⚠️ **NEEDS STYLING FIX** (see Section: UI/UX Improvements)

---

### Field 5: Spatial Segregation
**Type**: Boolean (checkbox)  
**HTML ID**: `#spatialSegregation`  
**Backend Model**: `ARCInputs25.spatial_segregation` (bool)

**Official Reference**: SORA 1.0 Annex C (Strategic Mitigations) - Concept carried forward to SORA 2.5  
"Strategic Mitigation by Operational Restrictions" - Boundary-based restrictions

**Definition**: Spatial segregation means the UAS operation is conducted in airspace with defined boundaries that separate it from manned aviation traffic.

**Examples**:
- Operations in controlled ground areas (private property with restricted access)
- Operations below minimum safe altitude for manned aircraft
- Operations in designated UAS zones
- Operations with lateral separation from airways and approach paths
- Operations with vertical separation (altitude bands)

**Validation Requirements**:
- Clearly defined operational boundaries
- Analysis showing separation from manned traffic routes
- Compliance with airspace structure (CTR, ATZ, FRZ, etc.)
- Monitoring to ensure containment within boundaries

**Impact on ARC**: Reduces the probability of encounter by ensuring spatial separation from manned aircraft operations.

**UI Implementation** (lines 1146-1150):
```html
<label>
  <input type="checkbox" id="spatialSegregation" value="true" />
  <span>Spatial Segregation</span>
</label>
```

**Status**: ⚠️ **NEEDS STYLING FIX** (see Section: UI/UX Improvements)

---

## Current UI Implementation

### Container Structure (mission.html lines 1114-1169)

```html
<div id="arc25Fields" style="display:none; background:#e3f2fd; padding:12px; border-radius:8px; margin-top:12px; border:2px solid #2196f3;">
  <strong style="color:#0d47a1;">🛰️ SORA 2.5 Enhanced ARC Inputs (JARUS Annex B)</strong>
  
  <!-- Field 1: U-space Services (dropdown) -->
  <label>U-space Services Available
    <select id="uSpaceServices">
      <option value="false">No</option>
      <option value="true">Yes (requires U-space airspace)</option>
    </select>
  </label>
  
  <!-- Field 2: Traffic Density Data Source (dropdown) -->
  <label>Traffic Density Data Source
    <select id="trafficDensitySource">
      <option value="Empirical">Empirical (Direct observation)</option>
      <option value="Statistical">Statistical (Historical data)</option>
      <option value="Expert">Expert (Low density only)</option>
    </select>
  </label>
  
  <!-- Field 3: Airspace Containment (dropdown) -->
  <label>Airspace Containment
    <select id="airspaceContainment25">
      <option value="None">None</option>
      <option value="Operational">Operational</option>
      <option value="Certified">Certified</option>
    </select>
  </label>
  
  <!-- Field 4: Temporal Segregation (checkbox) ⚠️ NEEDS STYLING -->
  <label>
    <input type="checkbox" id="temporalSegregation" value="true" />
    <span>Temporal Segregation</span>
  </label>
  
  <!-- Field 5: Spatial Segregation (checkbox) ⚠️ NEEDS STYLING -->
  <label>
    <input type="checkbox" id="spatialSegregation" value="true" />
    <span>Spatial Segregation</span>
  </label>
  
  <div style="margin-top:8px; font-size:0.9em; color:#555;">
    ℹ️ These fields are SORA 2.5 specific per JARUS Annex B. Not applicable to SORA 2.0 AMC.
  </div>
</div>
```

### Version Switching Logic (mission.html lines 2222-2243)

```javascript
function onCategoryChanged(category) {
  const grc20 = document.getElementById('grc20');
  const grc25 = document.getElementById('grc25');
  const populationDensityField = document.getElementById('populationDensityField');
  const arc25Fields = document.getElementById('arc25Fields');
  
  if (category === 'SORA-2.0') {
    grc20.style.display = 'block';
    grc25.style.display = 'none';
    populationDensityField.style.display = 'none';
    if (arc25Fields) arc25Fields.style.display = 'none';
    console.log('✅ SORA 2.0: Hiding ARC 2.5 fields');
  }
  
  if (category === 'SORA-2.5') {
    grc20.style.display = 'none';
    grc25.style.display = 'block';
    populationDensityField.style.display = 'block';
    if (arc25Fields) arc25Fields.style.display = 'block';
    console.log('✅ SORA 2.5: Showing ARC 2.5 fields + population density');
  }
}
```

**Status**: ✅ Version switching works correctly (verified by user in browser)

---

## UI/UX Improvements Required

### Issue 1: Checkbox Styling (CRITICAL)

**Problem**: User reported that temporal/spatial segregation checkboxes have **poor styling**:
- Asymmetric alignment
- No visual frame/container
- Inconsistent with dropdown fields
- Difficult to see if checked

**Current Checkbox HTML**:
```html
<label>
  <input type="checkbox" id="temporalSegregation" value="true" />
  <span>Temporal Segregation</span>
</label>
```

**Required Fix** - Apply consistent styling:
```html
<div style="background:#f5f5f5; padding:10px; border-radius:6px; margin-top:8px; border:1px solid #ccc;">
  <strong style="display:block; margin-bottom:8px; color:#0d47a1;">Strategic Segregation</strong>
  
  <label style="display:flex; align-items:center; margin-bottom:8px; cursor:pointer;">
    <input type="checkbox" id="temporalSegregation" value="true" 
           style="width:18px; height:18px; margin-right:10px; cursor:pointer;" />
    <span style="font-size:14px;">
      <strong>Temporal Segregation</strong><br>
      <small style="color:#666;">Operation during low manned traffic periods</small>
    </span>
  </label>
  
  <label style="display:flex; align-items:center; cursor:pointer;">
    <input type="checkbox" id="spatialSegregation" value="true" 
           style="width:18px; height:18px; margin-right:10px; cursor:pointer;" />
    <span style="font-size:14px;">
      <strong>Spatial Segregation</strong><br>
      <small style="color:#666;">Operation in defined boundaries separate from manned traffic</small>
    </span>
  </label>
  
  <div style="margin-top:8px; padding:8px; background:#fff3cd; border-left:3px solid #ffc107; font-size:0.85em;">
    ⚠️ <strong>Validation Required:</strong> Both segregation types require documented procedures and evidence in your ConOps.
  </div>
</div>
```

**Visual Improvements**:
- ✅ Larger checkbox size (18px × 18px)
- ✅ Proper alignment with flexbox
- ✅ Visual grouping container
- ✅ Descriptive subtitles
- ✅ Warning box for validation requirements
- ✅ Consistent with dropdown styling
- ✅ Improved cursor indicators

---

### Issue 2: Dropdown Consistency

**Current State**: Dropdowns (U-space, Traffic Density, Airspace Containment) are styled correctly.

**Enhancement**: Add helper text for each dropdown to improve UX:

```html
<!-- Example for U-space Services -->
<label>U-space Services Available
  <select id="uSpaceServices">
    <option value="false">No</option>
    <option value="true">Yes (requires U-space airspace)</option>
  </select>
  <small style="display:block; color:#666; margin-top:4px;">
    💡 U-space services provide digital aviation support (JAR_doc_34 Annex H)
  </small>
</label>

<!-- Example for Traffic Density Source -->
<label>Traffic Density Data Source
  <select id="trafficDensitySource">
    <option value="Empirical">Empirical (Direct observation)</option>
    <option value="Statistical">Statistical (Historical data)</option>
    <option value="Expert">Expert (Low density only)</option>
  </select>
  <small style="display:block; color:#666; margin-top:4px;">
    💡 Expert judgment is only valid for low traffic density areas
  </small>
</label>

<!-- Example for Airspace Containment -->
<label>Airspace Containment
  <select id="airspaceContainment25">
    <option value="None">None</option>
    <option value="Operational">Operational</option>
    <option value="Certified">Certified</option>
  </select>
  <small style="display:block; color:#666; margin-top:4px;">
    💡 Higher containment levels require ANSP coordination (JAR_doc_25 Step #5)
  </small>
</label>
```

---

### Issue 3: Info Box Reference Update

**Current Text** (line 1165):
```html
<div>ℹ️ These fields are SORA 2.5 specific per JARUS Annex B. Not applicable to SORA 2.0 AMC.</div>
```

**Issue**: Reference to "Annex B" is misleading - Annex B is for **Ground Risk** (GRC), not Air Risk (ARC).

**Corrected Text**:
```html
<div style="margin-top:12px; padding:10px; background:#e8f5e9; border-left:4px solid #4caf50; border-radius:4px;">
  <strong style="color:#2e7d32;">✅ SORA 2.5 Enhanced ARC Strategic Mitigations</strong><br>
  <small style="color:#555;">
    These fields are specific to SORA 2.5 per JARUS Main Body (JAR_doc_25) Step #5 and Annex H (JAR_doc_34).<br>
    <strong>Not applicable to SORA 2.0 AMC.</strong> If you need these mitigations, you must use SORA 2.5 category.
  </small>
  <div style="margin-top:8px; font-size:0.85em; color:#666;">
    📚 <strong>References:</strong> JAR_doc_25 (Main Body 2.5), JAR_doc_34 (Annex H - U-space Services)
  </div>
</div>
```

---

## Backend Model Verification

### Python Backend: `Backend_Python/models/arc_models.py`

**Enums** (lines 51-60):
```python
class AirspaceContainment25(str, Enum):
    NONE = "None"
    OPERATIONAL = "Operational"
    CERTIFIED = "Certified"

class TrafficDensityDataSource(str, Enum):
    EMPIRICAL = "Empirical"
    STATISTICAL = "Statistical"
    EXPERT = "Expert"
```

**Model** (lines 135-143):
```python
class ARCInputs25(BaseModel):
    """SORA 2.5 Enhanced ARC Inputs"""
    u_space_services_available: bool
    traffic_density_data_source: TrafficDensityDataSource
    airspace_containment: AirspaceContainment25
    temporal_segregation: bool
    spatial_segregation: bool
```

**Status**: ✅ Backend models match UI exactly - no changes needed

---

## JavaScript Data Collection

### Required Updates to `mission.html` JavaScript (around line ~2789)

**Current Code** (needs verification):
```javascript
// Collect SORA 2.5 Enhanced ARC Inputs
const uSpaceServices = document.getElementById('uSpaceServices')?.value === 'true';
const trafficDensitySource = document.getElementById('trafficDensitySource')?.value || 'Empirical';
const airspaceContainment25 = document.getElementById('airspaceContainment25')?.value || 'None';
const temporalSegregation = document.getElementById('temporalSegregation')?.checked || false;
const spatialSegregation = document.getElementById('spatialSegregation')?.checked || false;
```

**Action Required**:
- Verify this code exists in the `executeSora()` function
- Ensure checkbox values use `.checked` property (not `.value`)
- Ensure boolean conversion for uSpaceServices is correct
- Add validation to reject empty values

**Enhanced Version with Validation**:
```javascript
function collectArc25Inputs() {
  if (soraCategory !== 'SORA-2.5') {
    return null;  // These fields don't apply to SORA 2.0
  }
  
  const uSpaceServices = document.getElementById('uSpaceServices')?.value === 'true';
  const trafficDensitySource = document.getElementById('trafficDensitySource')?.value;
  const airspaceContainment25 = document.getElementById('airspaceContainment25')?.value;
  const temporalSegregation = document.getElementById('temporalSegregation')?.checked || false;
  const spatialSegregation = document.getElementById('spatialSegregation')?.checked || false;
  
  // Validation
  if (!trafficDensitySource) {
    alert('Error: Please select a Traffic Density Data Source for SORA 2.5');
    return null;
  }
  
  if (!airspaceContainment25) {
    alert('Error: Please select an Airspace Containment level for SORA 2.5');
    return null;
  }
  
  return {
    u_space_services_available: uSpaceServices,
    traffic_density_data_source: trafficDensitySource,
    airspace_containment: airspaceContainment25,
    temporal_segregation: temporalSegregation,
    spatial_segregation: spatialSegregation
  };
}
```

---

## Testing Requirements

### Visual Testing (Browser)

**Test Scenario 1**: SORA 2.5 Field Visibility
1. Open mission.html in browser
2. Select "SORA-2.5" category
3. **Verify**:
   - ✅ Blue ARC 2.5 box appears
   - ✅ All 5 fields visible
   - ✅ Checkboxes properly styled (large, aligned, framed)
   - ✅ Dropdowns have helper text
   - ✅ Info box shows correct JARUS references

**Test Scenario 2**: SORA 2.0 Field Hiding
1. Select "SORA-2.0" category
2. **Verify**:
   - ✅ ARC 2.5 box hidden
   - ✅ Only SORA 2.0 fields visible
   - ✅ Console log: "✅ SORA 2.0: Hiding ARC 2.5 fields"

**Test Scenario 3**: Checkbox Interaction
1. Select "SORA-2.5" category
2. Click temporal segregation checkbox
3. **Verify**:
   - ✅ Checkbox visually changes (checkmark appears)
   - ✅ No layout shift
   - ✅ Tooltip or subtitle visible
4. Click spatial segregation checkbox
5. **Verify** same behavior

**Test Scenario 4**: Data Collection
1. Fill out all 5 ARC 2.5 fields
2. Click "Execute SORA Evaluation"
3. Open Browser DevTools → Network → Find POST request
4. **Verify** JSON payload:
   ```json
   {
     "category": "SORA-2.5",
     "arc_inputs_25": {
       "u_space_services_available": true,
       "traffic_density_data_source": "Empirical",
       "airspace_containment": "Operational",
       "temporal_segregation": true,
       "spatial_segregation": false
     }
   }
   ```

---

## Summary of Changes Required

| Component | Location | Action | Priority |
|-----------|----------|--------|----------|
| Checkbox Styling | mission.html lines 1140-1150 | Apply flexbox layout, larger size, visual container | **CRITICAL** |
| Dropdown Helper Text | mission.html lines 1117-1138 | Add `<small>` tags with context | HIGH |
| Info Box Text | mission.html line 1165 | Correct JARUS reference (Annex B → Main Body + Annex H) | HIGH |
| JavaScript Collection | mission.html ~line 2789 | Verify `.checked` used for checkboxes, add validation | CRITICAL |
| Visual Grouping | mission.html lines 1140-1150 | Group segregation checkboxes in styled container | CRITICAL |
| JARUS References | All field labels | Add tooltips or info icons with JAR_doc citations | MEDIUM |

---

## Expected Outcome

After implementing these changes:

1. ✅ **Checkboxes**: Large, aligned, visually consistent with dropdowns
2. ✅ **User Experience**: Clear helper text, proper tooltips, validation warnings
3. ✅ **JARUS Compliance**: Correct references (JAR_doc_25, JAR_doc_34)
4. ✅ **Visual Polish**: Professional appearance matching rest of form
5. ✅ **Data Collection**: All 5 fields correctly sent to backend
6. ✅ **Version Switching**: Clean show/hide behavior (already working)

---

## Official Reference Summary

| Field | JARUS Document | Section | Description |
|-------|----------------|---------|-------------|
| U-space Services | JAR_doc_34 (Annex H) | H.1.1 | Service Providers for digital aviation support |
| Traffic Density Source | JAR_doc_25 (Main Body 2.5) | Step #4 | Data quality for Initial ARC determination |
| Airspace Containment | JAR_doc_25 (Main Body 2.5) | Step #5 | Strategic mitigation via airspace restrictions |
| Temporal Segregation | Annex C (v1.0) | Strategic Mitigations | Time-based operational restrictions |
| Spatial Segregation | Annex C (v1.0) | Strategic Mitigations | Boundary-based operational restrictions |

---

**End of Prompt 2**

**Next Steps**: After confirming UI improvements are complete, proceed to Prompt 3 (SORA 2.5 Backend Calculations).
