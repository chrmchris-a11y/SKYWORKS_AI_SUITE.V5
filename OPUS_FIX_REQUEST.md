# URGENT: SORA 2.0 & 2.5 Integration Issues - Complete Fix Request for Claude Opus 4.1

## Executive Summary
We have successfully implemented **SORA 2.5 backend calculations** (Python) with all 5 new ARC fields, but the **Frontend-Backend integration is broken**. The Python API works perfectly in isolation, but mission.html cannot communicate properly with either the .NET Backend (5210) or Python Backend (8001).

---

## 🎯 What Works (VERIFIED ✅)

### Python Backend API (Port 8001)
- **Status**: Fully functional when called directly via curl/Postman
- **SORA 2.0 Endpoint**: `POST /api/grc/calculate-v20` ✅
- **SORA 2.5 Endpoint**: `POST /api/sora/complete-v25` ✅
- **All 5 SORA 2.5 Fields**: Working correctly
  1. U-space Services Available (-1 ARC credit) ✅
  2. Traffic Density Data Source (Expert validation) ✅
  3. Airspace Containment (-1/-2 ARC credits) ✅
  4. Temporal Segregation (-1 ARC credit) ✅
  5. Spatial Segregation (-1 ARC credit) ✅

**Test Results**:
```json
// SORA 2.0 Test - SUCCESS
{
  "initial_grc": 5,
  "m2": "Low",
  "m2_value": -1,
  "final_grc": 6,
  "reference": "JAR_doc_06 Table 3"
}

// SORA 2.5 Test - SUCCESS
{
  "initial_grc": 1,
  "final_grc": 1,
  "initial_arc": 1,
  "residual_arc": 1,
  "sail": "I",
  "strategic_mitigations": [
    "✓ U-space Services: -1 ARC",
    "✓ Operational Containment: -1 ARC",
    "✓ Temporal Segregation: -1 ARC"
  ]
}
```

### Frontend UI
- **SORA 2.5 Fields**: All 5 fields styled correctly with 20x20px checkboxes ✅
- **JavaScript Validation**: arc25-validation.js working ✅
- **Helper Text**: JARUS references displayed ✅

---

## ❌ What's Broken (CRITICAL ISSUES)

### Issue #1: .NET Backend API (Port 5210) Won't Start
**Symptoms**:
- Task "Start Backend API (5210)" exits with code 1
- `/health` endpoint returns 404
- Frontend gets "BadRequest" errors when calling API

**Error Messages**:
```
SORA 2.0: "Ground Risk: Invalid drone mass (MTOM_kg) for GRC calculation"
SORA 2.5: "Ground Risk: Python GRC 2.5 calculation failed - null response"
```

**Attempted Fixes**:
1. Killed process on port 5210 and restarted → Still fails
2. Tried running from `Backend/src/Skyworks.Api` folder → Gets 404 on /health
3. Checked if project file exists → EXISTS at `Backend/src/Skyworks.Api/Skyworks.Api.csproj`

**Root Cause Hypothesis**:
- The .NET Backend expects to call Python Backend (8001) for SORA calculations
- Python Backend is not running when .NET starts
- .NET Backend has no `/health` endpoint configured (404 error)
- Frontend sends wrong JSON structure to .NET Backend

### Issue #2: Frontend → Backend Communication Broken
**Symptoms**:
- Frontend sends requests to `http://localhost:5210/api/sora/complete`
- Backend expects different JSON structure than what Frontend sends
- Missing fields in request: `MTOM_kg`, `grc_inputs` structure mismatch

**Current Frontend Request Structure** (from mission.html line ~2500):
```javascript
{
  AirspaceControl: "...",
  AirspaceClass: "...",
  // ... other fields ...
  MaxDimension: 1.2,
  CruiseSpeed: 8
  // MISSING: MTOM_kg field that backend expects
}
```

**Expected Backend Structure** (for SORA 2.5):
```json
{
  "operational_volume": { ... },
  "traffic_density": "Low",
  "arc_inputs_25": {
    "u_space_services_available": true,
    "traffic_density_data_source": "Empirical",
    "airspace_containment": "Operational",
    "temporal_segregation": true,
    "spatial_segregation": false
  },
  "grc_inputs": {
    "population_density": "Low",
    "sheltering": "High",
    "m1": 0,
    "m3": 0
  },
  "category": "SORA-2.5"
}
```

### Issue #3: Python Backend Not Auto-Starting
**Symptoms**:
- Python service (8001) must be started manually
- No automatic startup when Backend API starts
- Frontend assumes Python is always running

---

## 📋 Required Fixes (Priority Order)

### FIX #1: .NET Backend Health Endpoint
**File**: `Backend/src/Skyworks.Api/Program.cs` or relevant controller
**Required**:
```csharp
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    service = "Skyworks Backend API", 
    port = 5210,
    python_api = "http://localhost:8001"
}));
```

### FIX #2: Frontend Request Mapping
**File**: `Frontend/Pages/mission.html` (around line 2470-2520)
**Required**: Add proper SORA 2.5 field mapping when category is "SORA-2.5"
```javascript
if (operationCategory === 'SORA-2.5') {
  body.arc_inputs_25 = {
    u_space_services_available: document.getElementById('uSpaceServices')?.value === 'true',
    traffic_density_data_source: document.getElementById('trafficDensitySource')?.value || 'Empirical',
    airspace_containment: document.getElementById('airspaceContainment25')?.value || 'None',
    temporal_segregation: document.getElementById('temporalSegregation')?.checked || false,
    spatial_segregation: document.getElementById('spatialSegregation')?.checked || false
  };
  body.grc_inputs = {
    population_density: /* extract from UI */,
    sheltering: /* extract from UI */,
    m1: 0,
    m3: 0
  };
}
```

### FIX #3: .NET Backend → Python Backend Proxy
**File**: `Backend/src/Skyworks.Api/Controllers/SoraController.cs` (or create if missing)
**Required**: Add endpoint that forwards SORA 2.5 requests to Python API
```csharp
[HttpPost("api/sora/complete")]
public async Task<IActionResult> CalculateSora([FromBody] SoraRequest request)
{
    if (request.Category == "SORA-2.5")
    {
        // Forward to Python API at http://localhost:8001/api/sora/complete-v25
        using var httpClient = new HttpClient();
        var pythonResponse = await httpClient.PostAsJsonAsync(
            "http://localhost:8001/api/sora/complete-v25", 
            request
        );
        return Ok(await pythonResponse.Content.ReadAsStringAsync());
    }
    // Handle SORA 2.0 internally
    // ...
}
```

### FIX #4: Python Backend Auto-Start
**Options**:
1. Add Python startup to .NET Backend initialization
2. Create a startup script that launches both services
3. Use Docker Compose (if applicable)

---

## 📁 File Locations

### Python Backend (Working ✅)
- `Backend_Python/main.py` (406 lines)
- `Backend_Python/models/arc_models.py` (88 lines)
- `Backend_Python/arc/calculators/initial_arc_calculator_v25.py` (227 lines)
- `Backend_Python/arc/calculators/strategic_mitigations_v25.py` (213 lines)
- `Backend_Python/sail/sail_calculator_v25.py` (94 lines)

### .NET Backend (Broken ❌)
- `Backend/src/Skyworks.Api/Skyworks.Api.csproj` (exists)
- `Backend/src/Skyworks.Api/Program.cs` (exists)
- `Backend/src/Skyworks.Api/Controllers/` (need to check)

### Frontend (Partially Working ⚠️)
- `Frontend/Pages/mission.html` (3563 lines)
- `Frontend/Pages/arc25-validation.js` (271 lines)

---

## 🧪 Test Cases (for validation after fix)

### Test Case 1: SORA 2.0 via UI
1. Open mission.html
2. Select "SORA-2.0" from category dropdown
3. Fill in drone (MTOM: 32kg, Speed: 8m/s)
4. Set GRC: Medium Pop + Medium Shelter
5. Set M1=1, M2=Low, M3=1
6. Click "Calculate SORA"
7. **Expected**: Final GRC calculated, no 400 errors

### Test Case 2: SORA 2.5 via UI
1. Open mission.html
2. Select "SORA-2.5" from category dropdown
3. Fill in GRC: Low Pop + High Shelter
4. Check 5 SORA 2.5 fields:
   - U-space: Yes
   - Data Source: Empirical
   - Containment: Operational
   - Temporal: Yes
   - Spatial: No
5. Click "Calculate SORA"
6. **Expected**: 
   - Initial GRC: 1
   - Initial ARC: 1
   - Residual ARC: 1 (after -3 credits)
   - SAIL: I
   - Strategic mitigations listed

### Test Case 3: Expert Judgment Validation
1. Select SORA 2.5
2. Set Traffic Density: Medium
3. Set Data Source: Expert
4. Click Calculate
5. **Expected**: Error "Expert judgment only valid for LOW traffic density"

---

## 🎯 Success Criteria
- [ ] .NET Backend starts successfully on port 5210
- [ ] `/health` endpoint returns 200 OK
- [ ] Frontend can call `/api/sora/complete` without 400 errors
- [ ] SORA 2.0 calculations work end-to-end (UI → .NET → Result)
- [ ] SORA 2.5 calculations work end-to-end (UI → .NET → Python → Result)
- [ ] All 5 SORA 2.5 fields properly transmitted from UI to Python
- [ ] Strategic mitigations correctly applied and displayed
- [ ] JARUS references present in all responses

---

## 🔑 JARUS Compliance Requirements (DO NOT BREAK)
- **JAR_doc_06**: SORA 2.0 M2 levels (None/Low/High only)
- **JAR_doc_25**: SORA 2.5 Steps #4, #5, Table 7
- **JAR_doc_34**: Annex H (U-space services)
- **Annex C**: Strategic Mitigations

---

## 📦 Request for Claude Opus 4.1

**Please provide:**
1. Complete fixed version of `Backend/src/Skyworks.Api/Program.cs`
2. Complete fixed version of `Backend/src/Skyworks.Api/Controllers/SoraController.cs` (create if missing)
3. Fixed JavaScript code for `mission.html` (lines ~2470-2600) to properly map SORA 2.5 fields
4. PowerShell startup script that launches both .NET and Python backends in correct order
5. Step-by-step integration instructions

**Key Constraints:**
- Preserve all existing Python backend code (it works perfectly)
- Preserve all existing Frontend UI styling (SORA 2.5 fields look good)
- Maintain 100% JARUS compliance
- No breaking changes to existing SORA 2.0 functionality

---

## Current Environment
- **OS**: Windows 11
- **.NET SDK**: 8.0.403
- **Python**: 3.14
- **FastAPI**: Latest
- **PowerShell**: 7.x
- **Browser**: Microsoft Edge
- **Workspace**: C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5

---

**Thank you for your help in fixing this integration! The core logic is solid, we just need the plumbing to work correctly.**
