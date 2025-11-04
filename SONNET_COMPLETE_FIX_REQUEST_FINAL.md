# 🚨 URGENT: Complete SORA 2.0/2.5 Integration Fix Request for Claude Sonnet 4.5

## Executive Summary
**Mission-critical bug affecting EASA/JARUS compliance system**. User confirmed M2 display error as "megalo sfalma" (critical error). After multiple iterations with Opus 4.1, we've fixed Backend API but **mission.html UI integration still fails with 422 Unprocessable Entity**.

**Current Status:**
- ✅ Backend API works perfectly (PowerShell tests pass)
- ✅ Python API works perfectly (direct tests pass)
- ❌ mission.html UI → Backend → Python chain fails
- ❌ Empty error object returned: `{ "error": {} }`

---

## System Architecture

```
┌──────────────┐     POST /api/SoraProxy/complete     ┌────────────────┐
│              │────────────────────────────────────────▶│                │
│ mission.html │                                        │   Backend API  │
│  (Frontend)  │     JSON: { SoraVersion, GroundRisk }  │  (C# .NET 8.0) │
│              │◀────────────────────────────────────────│   Port 5210    │
└──────────────┘     JSON Response or Error            └────────┬───────┘
                                                                │
                                                                │ POST /api/grc/calculate-v20
                                                                │ snake_case JSON
                                                                ▼
                                                        ┌────────────────┐
                                                        │  Python API    │
                                                        │  (FastAPI)     │
                                                        │  Port 8001     │
                                                        └────────────────┘
```

---

## Detailed Problem Analysis

### Problem 1: UI Sends Request, Gets 422 Error

**Evidence from Console Log:**
```javascript
// Request sent by mission.html:
[SORA Request Body] {
  "SoraVersion": "2.0",
  "MissionId": "M-20251031-0922-012",
  "DroneId": "skytech-sc15",
  "GroundRisk": {
    "Scenario_V2_0": "ControlledGroundArea",
    "MaxCharacteristicDimension": 1.2,
    "KineticEnergy": 1024,
    "Mitigations": [
      { "Type": "M1", "Robustness": "Medium" },
      { "Type": "M2", "Robustness": "Medium" },  // ← Should be "Low" for -1 test!
      { "Type": "M3", "Robustness": "Medium" }
    ],
    "IsControlledGroundArea": false
  },
  "AirRisk": {
    "AirspaceControl": "Uncontrolled",
    "AirspaceClass": "G",
    "LocationType": "NonAirport",
    "Environment": "Urban",
    "Typicality": "Typical",
    "MaxHeightAGL": 60,
    "MaxHeightAMSL": 0,
    "IsModeS_Veil": false,
    "IsTMZ": false,
    "StrategicMitigations": [],
    "IsAtypicalSegregated": true,
    "MaxDimension": 1.2,
    "CruiseSpeed": 8
  },
  "ImplementedOSOs": []
}

// Backend response:
Failed to load resource: the server responded with a status of 422 (Unprocessable Entity)

[SORA Response] {
  "error": {}  // ← EMPTY ERROR OBJECT!
}
```

**However, PowerShell test with SAME payload works:**
```powershell
$body = @{
    SoraVersion = "2.0"
    GroundRisk = @{
        Scenario_V2_0 = "ControlledGroundArea"
        Mitigations = @(
            @{ Type = "M1"; Robustness = "Medium" }
            @{ Type = "M2"; Robustness = "Low" }
            @{ Type = "M3"; Robustness = "Medium" }
        )
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5210/api/SoraProxy/complete" ...

# Result: SUCCESS! M2=Low / m2_value=-1 ✅
```

**Conclusion:** Backend API works when called from PowerShell but fails when called from mission.html UI.

---

### Problem 2: Missing Fields in Request

**Mission.html sends:**
- ✅ `SoraVersion: "2.0"`
- ✅ `GroundRisk.Mitigations[]`
- ❌ **MISSING**: `GroundRisk.PopulationDensity` (required by Backend)
- ❌ **MISSING**: `GroundRisk.Sheltering` (required by Backend)

**Backend expects** (from `ExtractSora20FromMissionHtml`):
```csharp
if (!json.TryGetProperty("GroundRisk", out var groundRisk))
{
    throw new ArgumentException("GroundRisk is required for SORA 2.0");
}

// These are REQUIRED but mission.html doesn't send them:
if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity)) { ... }
request.Sheltering = ExtractSheltering(groundRisk) ?? "High";
```

**Why PowerShell works:**
```json
{
  "GroundRisk": {
    "PopulationDensity": 5000,     // ← PowerShell includes this
    "Sheltering": "High",          // ← PowerShell includes this
    "Scenario_V2_0": "ControlledGroundArea",
    "Mitigations": [...]
  }
}
```

---

### Problem 3: JavaScript Error Before Request

**From Console:**
```javascript
mission.html:1730 Uncaught ReferenceError: soraResult is not defined
    at calculateGRC20 (mission.html:1730:21)
    at HTMLSelectElement.onchange (mission.html:898:62)
```

**Location (Line 1730):**
```javascript
calcDetails.innerHTML = `
  M2 (${soraResult.m2 || 'Impact reduction'}): <strong>${m2Value}</strong> GRC<br>
  //    ^^^^^^^^^^^ ERROR: soraResult not defined yet!
`;
```

**Fix Applied:**
```javascript
const m2Label = (window.soraResult && window.soraResult.m2) ? window.soraResult.m2 : 'Impact reduction';
calcDetails.innerHTML = `
  M2 (${m2Label}): <strong>${m2Value}</strong> GRC<br>
`;
```

**However:** This error happens BEFORE the API call, suggesting the `calculateGRC20()` function is called on page load when dropdowns change, but `window.soraResult` isn't set until AFTER the first successful API call.

---

### Problem 4: M2 Dropdown Mapping Issue

**From mission.html lines 2464-2478:**
```javascript
const m2 = parseInt(document.getElementById('m2_20')?.value || '0');

// Map numeric values back to robustness levels
if (m2 === -1) mitigations20.push({ Type: 'M2', Robustness: 'Medium' });
else if (m2 === -2) mitigations20.push({ Type: 'M2', Robustness: 'High' });
// ❌ NO MAPPING FOR m2 === -1 to "Low"!
```

**JARUS SORA 2.0 Table 3 - M2 (Impact Reduction) Correct Mapping:**
- **Low robustness**: -1 GRC reduction
- **Medium robustness**: -2 GRC reduction  
- **High robustness**: -3 GRC reduction

**Current dropdown mapping is WRONG:**
```
Dropdown Value    Currently Maps To    Should Map To
-1                "Medium"             "Low"  ❌
-2                "High"               "Medium" ❌
-3                (missing)            "High" ❌
```

**This is THE ROOT CAUSE of the "megalo sfalma" bug!**

User sees M2 displaying as -2 instead of -1 because:
1. Dropdown has value `-1`
2. JavaScript maps `-1` → `"Medium"` (WRONG!)
3. Backend receives `"Medium"`
4. Python API calculates M2=Medium = **-2** (correct for Medium)
5. UI displays **-2** but user expected **-1** for Low robustness

---

## Current Code State

### Backend: SoraProxyController.cs (Lines 153-274)

**Status:** ✅ Fixed by previous iterations
- ✅ M1/M3 initialized to 0
- ✅ SnakeCaseNamingPolicy fixed for underscores
- ✅ M2 value extracted correctly

```csharp
private SoraCompleteRequest ExtractSora20FromMissionHtml(JsonElement json)
{
    var request = new SoraCompleteRequest { Category = "SORA-2.0" };
    
    // Initialize defaults to prevent "Field required" errors
    request.M1 = 0;  // ✅ Fixed
    request.M3 = 0;  // ✅ Fixed

    if (!json.TryGetProperty("GroundRisk", out var groundRisk))
    {
        throw new ArgumentException("GroundRisk is required for SORA 2.0");
    }

    // Population Density - ❌ PROBLEM: mission.html doesn't send this!
    if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
    {
        var popValue = popDensity.GetDouble();
        request.PopulationDensity = popValue < 500 ? "Low" : 
                                  popValue < 10000 ? "Medium" : "High";
    }
    else
    {
        // ❌ FALLBACK: Should this throw error or use default?
        _logger.LogWarning("PopulationDensity missing, defaulting to Low");
        request.PopulationDensity = "Low";
    }

    // Sheltering - ❌ PROBLEM: mission.html doesn't send this!
    request.Sheltering = ExtractSheltering(groundRisk) ?? "High";

    // ... rest of extraction code works fine
}
```

### Frontend: mission.html (Lines 2464-2500)

**Status:** ❌ BROKEN - Wrong M2 mapping

```javascript
// Build SORA 2.0 mitigations from structured dropdowns
const mitigations20 = [];
const m1 = parseInt(document.getElementById('m1_20')?.value || '0');
const m2 = parseInt(document.getElementById('m2_20')?.value || '0');
const m3 = parseInt(document.getElementById('m3_20')?.value || '1');

// Map numeric values back to robustness levels for API
if (m1 === -1) mitigations20.push({ Type: 'M1', Robustness: 'Low' });
else if (m1 === -2) mitigations20.push({ Type: 'M1', Robustness: 'Medium' });
else if (m1 === -4) mitigations20.push({ Type: 'M1', Robustness: 'High' });

// ❌ WRONG MAPPING FOR M2!
if (m2 === -1) mitigations20.push({ Type: 'M2', Robustness: 'Medium' });  // Should be 'Low'!
else if (m2 === -2) mitigations20.push({ Type: 'M2', Robustness: 'High' }); // Should be 'Medium'!
// Missing: m2 === -3 → 'High'

if (m3 === 0) mitigations20.push({ Type: 'M3', Robustness: 'Medium' });
else if (m3 === -1) mitigations20.push({ Type: 'M3', Robustness: 'High' });

body.GroundRisk = {
    Scenario_V2_0: document.getElementById('scenario20').value,
    MaxCharacteristicDimension: Number(document.getElementById('maxDimension20').value || 0),
    KineticEnergy: ke20 !== undefined ? Math.round(ke20) : undefined,
    Mitigations: mitigations20,
    IsControlledGroundArea: document.getElementById('controlledGround20')?.value === 'true' || false
    // ❌ MISSING: PopulationDensity (required by Backend)
    // ❌ MISSING: Sheltering (required by Backend)
};
```

---

## JARUS SORA 2.0 Authoritative Reference

**Table 3: Ground Risk Mitigations (JAR_DEL_WG6_D.04)**

### M1 - Strategic Mitigation (Reduce people at risk)
- **Low**: -1 GRC
- **Medium**: -2 GRC  
- **High**: -4 GRC

### M2 - Tactical Mitigation (Impact reduction - e.g., parachute)
- **Low**: -1 GRC
- **Medium**: -2 GRC
- **High**: -3 GRC

### M3 - Emergency Response Plan
- **None**: +1 GRC penalty
- **Medium**: 0 GRC
- **High**: -1 GRC

**Source:** JARUS SORA 2.0 (AMC) Table 3 - Ground Risk Mitigations

---

## Test Results Summary

### ✅ Tests That Work

**1. Direct Python API Test:**
```bash
curl -X POST http://localhost:8001/api/grc/calculate-v20 \
  -H "Content-Type: application/json" \
  -d '{
    "category":"SORA-2.0",
    "population_density":"Medium",
    "sheltering":"High",
    "scenario_v2_0":"VLOS_SparselyPopulated",
    "mtom_kg":15,
    "initial_grc":2,
    "m1":0,
    "m2":"Low",
    "m3":0
  }'

# Result: 
{
  "final_grc": 1,
  "m2": "Low",
  "m2_value": -1,  ✅
  "calculation": "2 + 0 + -1 + 0 = 1"
}
```

**2. PowerShell → Backend → Python Chain:**
```powershell
$body = @{
    SoraVersion = "2.0"
    GroundRisk = @{
        PopulationDensity = 5000
        Sheltering = "High"
        KineticEnergy = 750
        MaxCharacteristicDimension = 1.2
        Scenario_V2_0 = "VLOS_SparselyPopulated"
        Mitigations = @(
            @{ Type = "M1"; Robustness = "Medium" }
            @{ Type = "M2"; Robustness = "Low" }
            @{ Type = "M3"; Robustness = "Medium" }
        )
    }
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri "http://localhost:5210/api/SoraProxy/complete" ...

# Result:
SUCCESS: M2=Low / m2_value=-1 ✅
```

### ❌ Tests That Fail

**1. mission.html UI → Backend:**
```
Status: 422 Unprocessable Entity
Response: { "error": {} }
```

**Console Errors:**
- `ReferenceError: soraResult is not defined` (before API call)
- Empty error object (after API call)

---

## Required Fixes

### Fix 1: Correct M2 Dropdown Mapping in mission.html

**Location:** Lines 2472-2475

**Current (WRONG):**
```javascript
if (m2 === -1) mitigations20.push({ Type: 'M2', Robustness: 'Medium' });
else if (m2 === -2) mitigations20.push({ Type: 'M2', Robustness: 'High' });
```

**Should be:**
```javascript
if (m2 === -1) mitigations20.push({ Type: 'M2', Robustness: 'Low' });    // ✅ FIXED
else if (m2 === -2) mitigations20.push({ Type: 'M2', Robustness: 'Medium' }); // ✅ FIXED
else if (m2 === -3) mitigations20.push({ Type: 'M2', Robustness: 'High' });   // ✅ ADDED
```

### Fix 2: Add Missing PopulationDensity to GroundRisk

**Location:** Lines 2490-2500

**Current:**
```javascript
body.GroundRisk = {
    Scenario_V2_0: document.getElementById('scenario20').value,
    MaxCharacteristicDimension: Number(document.getElementById('maxDimension20').value || 0),
    KineticEnergy: ke20,
    Mitigations: mitigations20,
    IsControlledGroundArea: false
    // ❌ MISSING: PopulationDensity
};
```

**Should be:**
```javascript
body.GroundRisk = {
    Scenario_V2_0: document.getElementById('scenario20').value,
    MaxCharacteristicDimension: Number(document.getElementById('maxDimension20').value || 0),
    KineticEnergy: ke20,
    Mitigations: mitigations20,
    IsControlledGroundArea: false,
    
    // ✅ ADD: PopulationDensity from SORA 2.5 field
    PopulationDensity: Number(document.getElementById('populationDensity')?.value || 5000),
    
    // ✅ ADD: Sheltering (default or from dropdown if exists)
    Sheltering: document.getElementById('sheltering20')?.value || "High"
};
```

**Alternative:** Create a hidden Sheltering dropdown for SORA 2.0 (similar to SORA 2.5 fields)

### Fix 3: Better Error Handling in Backend

**Location:** SoraProxyController.cs, `ExtractSora20FromMissionHtml`

**Current:**
```csharp
if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
{
    // ... extract
}
else
{
    _logger.LogWarning("PopulationDensity missing, defaulting to Low");
    request.PopulationDensity = "Low";  // Silent fallback
}
```

**Should be:**
```csharp
if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
{
    var popValue = popDensity.GetDouble();
    request.PopulationDensity = popValue < 500 ? "Low" : 
                              popValue < 10000 ? "Medium" : "High";
}
else
{
    // ✅ OPTION A: Throw error for UI to handle
    throw new ArgumentException("PopulationDensity is required for SORA 2.0 assessment");
    
    // ✅ OPTION B: Use safer default with warning
    _logger.LogWarning("PopulationDensity missing, using 'Medium' as safe default");
    request.PopulationDensity = "Medium"; // Safer than "Low"
}
```

### Fix 4: Initialize window.soraResult Early

**Location:** mission.html, around line 1695

**Current:**
```javascript
// window.soraResult only set AFTER successful API call
```

**Should be:**
```javascript
// Initialize early to prevent ReferenceError
window.soraResult = null;

// Later in calculateGRC20():
const m2Label = (window.soraResult && window.soraResult.m2) ? window.soraResult.m2 : 'Impact reduction';
```

### Fix 5: Return Proper Error Messages

**Location:** SoraProxyController.cs, catch blocks

**Current:**
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing");
    return StatusCode(500, new { error = "Internal server error" });
    // ❌ Frontend sees { "error": {} } - empty object!
}
```

**Should be:**
```csharp
catch (ArgumentException argEx)
{
    _logger.LogWarning(argEx, "Validation error");
    return BadRequest(new 
    { 
        error = argEx.Message,
        type = "ValidationError",
        field = "GroundRisk"
    });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unhandled error");
    return StatusCode(500, new 
    { 
        error = ex.Message,
        type = ex.GetType().Name,
        stackTrace = ex.StackTrace  // Only in development
    });
}
```

---

## Critical User Requirement

**From user (translated from Greek):**
> "nai tou estiles kai afto pu vgenei -? dioti einai megalo sfalma afto!"
> "Yes, you sent it and what comes out is -? because this is a BIG ERROR!"

**Translation:** User sees M2 displaying as **-2** instead of **-1** when selecting "Low" robustness. This is a **CRITICAL ERROR** for EASA/JARUS compliance.

**Expected Behavior:**
- User selects **M2 = Low robustness** from dropdown
- UI should display: **"M2 (Low): -1 GRC"**
- Total reduction: **M1(-2) + M2(-1) + M3(0) = -3 GRC**

**Current Broken Behavior:**
- User selects dropdown value `-1`
- JavaScript maps `-1` → `"Medium"` (WRONG!)
- Backend calculates Medium = -2
- UI displays: **"M2 (Medium): -2 GRC"** ❌
- Total shows -4 instead of -3 ❌

---

## Request to Claude Sonnet 4.5

**PLEASE PROVIDE COMPLETE, PRODUCTION-READY CODE FOR:**

1. **Fixed mission.html M2 mapping** (lines 2472-2475)
   - Correct mapping: -1→Low, -2→Medium, -3→High
   - Include PopulationDensity and Sheltering in GroundRisk object

2. **Fixed calculateGRC20() function** (around line 1690)
   - Proper m2Value calculation from API response
   - Safe handling when window.soraResult is not yet defined

3. **Enhanced error handling in SoraProxyController.cs**
   - Return proper error messages (not empty objects)
   - Distinguish between validation errors and server errors

4. **Optional: Add Sheltering dropdown for SORA 2.0**
   - Similar to SORA 2.5 fields
   - Default to "High" if not specified

**Requirements:**
- ✅ Must maintain null-safety throughout
- ✅ Must work with existing Python API (no Python changes)
- ✅ Must display M2=-1 for "Low" robustness
- ✅ Must be 100% JARUS SORA 2.0 compliant
- ✅ Must handle both SORA 2.0 and SORA 2.5 requests
- ✅ Must provide clear error messages to user

**Deliverables:**
1. Complete JavaScript code snippets (ready to paste into mission.html)
2. Complete C# code snippets (ready to paste into SoraProxyController.cs)
3. Step-by-step verification tests
4. Any HTML changes needed (dropdown additions, etc.)

**CRITICAL:** This is blocking production deployment for EASA compliance system. Need immediate, working solution!

---

## Files Attached (for reference)

1. **Backend/src/Skyworks.Api/Controllers/SoraProxyController.cs** (729 lines)
   - Current state: M1/M3 defaults fixed, snake_case naming fixed
   - Needs: Better error handling, PopulationDensity fallback logic

2. **Frontend/Pages/mission.html** (3576 lines)
   - Current state: M2 mapping broken, missing fields in request
   - Needs: Fixed mapping, added fields, early initialization

3. **Console logs showing exact errors and request/response**

4. **PowerShell test proving Backend API works when called correctly**

---

## Success Criteria

**Test Case 1: M2 = Low Robustness**
```
Input: M1=Medium (-2), M2=Low (-1), M3=Medium (0)
Expected Output:
  - M2 display: "M2 (Low): -1 GRC" ✅
  - Total: -3 GRC ✅
  - Final GRC: depends on initial GRC
```

**Test Case 2: UI → Backend → Python Chain**
```
1. Open mission.html
2. Select SORA 2.0
3. Choose M2 = Low (-1)
4. Click "Execute SORA Assessment"
5. Verify console shows successful response
6. Verify UI displays M2=-1 (not -2!)
```

**Test Case 3: Error Handling**
```
1. Send invalid request
2. Backend returns clear error message
3. UI displays error to user (not empty object)
```

---

**END OF REQUEST - PLEASE PROVIDE COMPLETE FIXES NOW!** 🚨
