# 🚨 URGENT: Python API Validation Error - M2 Bug Fix Not Working in UI

## Problem Summary
After fixing the M2 bug in Backend (PowerShell tests pass ✅), the **mission.html UI still fails** with "API Validation Error" when submitting SORA 2.0 assessment.

---

## Current Status

### ✅ What Works (PowerShell Direct Test)
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

# Result: ✅ SUCCESS
# M2 VALUE: -1 (Expected: -1) ✅
# M1 VALUE: -2 (Expected: -2) ✅
# M3 VALUE: 0 (Expected: 0) ✅
# TOTAL REDUCTION: -3 (Expected: -3) ✅
```

### ❌ What Fails (mission.html UI)
When clicking "Run SORA 2.0 Assessment" in mission.html:
- **Error**: "❌ BadRequest - API Validation Error - Request failed"
- **Backend returns**: 503 Service Unavailable or validation error from Python API

---

## Python API Validation Error (Latest Test)

```json
{
  "detail": [
    {
      "type": "missing",
      "loc": ["body", "m1"],
      "msg": "Field required",
      "input": {
        "category": "SORA-2.0",
        "population_density": "Medium",
        "sheltering": "High",
        "scenario__v2_0": "VLOS_SparselyPopulated",
        "mtom_kg": 15,
        "initial_grc": 2,
        "m2": "Low"
      }
    },
    {
      "type": "missing",
      "loc": ["body", "m3"],
      "msg": "Field required",
      "input": {
        "category": "SORA-2.0",
        "population_density": "Medium",
        "sheltering": "High",
        "scenario__v2_0": "VLOS_SparselyPopulated",
        "mtom_kg": 15,
        "initial_grc": 2,
        "m2": "Low"
      }
    }
  ]
}
```

**Key Issues:**
1. ❌ `m1` field is **MISSING** from Python API request
2. ❌ `m3` field is **MISSING** from Python API request
3. ⚠️ `m2` is sent as **string** ("Low") but Python API expects **integer** (-1)
4. ⚠️ Field name transformed to `scenario__v2_0` (double underscore!) instead of `scenario_v2_0`

---

## System Architecture

```
mission.html (Frontend)
    ↓ POST /api/SoraProxy/complete
Backend (C# .NET 8.0 - Port 5210)
    ↓ POST /api/grc/calculate-v20
Python API (FastAPI - Port 8001)
    ↓ Returns JSON response
Backend transforms response
    ↓ Returns to Frontend
mission.html displays results
```

---

## Backend Code (SoraProxyController.cs)

### ExtractSora20FromMissionHtml Method (Lines 153-274)
```csharp
private SoraCompleteRequest ExtractSora20FromMissionHtml(JsonElement json)
{
    var request = new SoraCompleteRequest { Category = "SORA-2.0" };

    try
    {
        if (!json.TryGetProperty("GroundRisk", out var groundRisk))
        {
            _logger.LogWarning("GroundRisk not found in request");
            throw new ArgumentException("GroundRisk is required for SORA 2.0");
        }

        // Population Density
        if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
        {
            var popValue = popDensity.GetDouble();
            request.PopulationDensity = popValue < 500 ? "Low" : 
                                      popValue < 10000 ? "Medium" : "High";
            _logger.LogDebug("Population density {Value} mapped to {Category}", popValue, request.PopulationDensity);
        }
        else
        {
            _logger.LogWarning("PopulationDensity missing, defaulting to Low");
            request.PopulationDensity = "Low";
        }

        // Sheltering - with proper fallback
        request.Sheltering = ExtractSheltering(groundRisk) ?? "High";
        _logger.LogDebug("Sheltering set to: {Sheltering}", request.Sheltering);

        // ... (Scenario, MTOM, KE, Dimension extraction - all working)

        // Calculate initial GRC
        request.InitialGrc = CalculateInitialGrc(request.PopulationDensity, request.Sheltering);
        _logger.LogDebug("Initial GRC calculated: {GRC}", request.InitialGrc);

        // Extract mitigations ← HERE IS THE PROBLEM!
        if (groundRisk.TryGetProperty("Mitigations", out var mitigations) && mitigations.ValueKind == JsonValueKind.Array)
        {
            foreach (var mitigation in mitigations.EnumerateArray())
            {
                if (!mitigation.TryGetProperty("Type", out var typeElem) ||
                    !mitigation.TryGetProperty("Robustness", out var robustnessElem))
                {
                    continue;
                }

                var type = typeElem.GetString();
                var robustness = robustnessElem.GetString();

                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(robustness))
                {
                    continue;
                }

                switch (type.ToUpper())
                {
                    case "M1":
                        request.M1 = MapRobustnessToGrcReduction(robustness);  // ← Converts to int (-1, -2, -3)
                        _logger.LogDebug("M1 set to {Value} for robustness {Robustness}", request.M1, robustness);
                        break;
                    case "M2":
                        request.M2 = robustness;  // ← Keeps as STRING! This is wrong for Python API!
                        _logger.LogDebug("M2 set to {Robustness}", robustness);
                        break;
                    case "M3":
                        request.M3 = MapM3RobustnessToReduction(robustness);  // ← Converts to int (0, -1)
                        _logger.LogDebug("M3 set to {Value} for robustness {Robustness}", request.M3, robustness);
                        break;
                }
            }
        }

        return request;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error extracting SORA 2.0 data from mission.html format");
        throw;
    }
}
```

### SoraCompleteRequest Model (Lines 680-709)
```csharp
private class SoraCompleteRequest
{
    public string? Category { get; set; }
    public string? PopulationDensity { get; set; }
    public string? Sheltering { get; set; }
    public string? Scenario_V2_0 { get; set; }
    public double? MtomKg { get; set; }
    public double? KineticEnergy { get; set; }
    public double? MaxCharacteristicDimension { get; set; }
    public int? InitialGrc { get; set; }
    public int? M1 { get; set; }          // ← INTEGER ✅
    public string? M2 { get; set; }       // ← STRING ❌ (Should be int!)
    public int? M3 { get; set; }          // ← INTEGER ✅
    public OperationalVolume? OperationalVolume { get; set; }
    public string? TrafficDensity { get; set; }
    public ArcInputs25? ArcInputs25 { get; set; }
    public GrcInputs? GrcInputs { get; set; }
}
```

### SnakeCaseNamingPolicy (Lines 7-33)
```csharp
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        
        var result = new StringBuilder();
        result.Append(char.ToLowerInvariant(name[0]));
        
        for (int i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(name[i]));
            }
            else
            {
                result.Append(name[i]);
            }
        }
        
        return result.ToString();
    }
}
```

**BUG:** `Scenario_V2_0` converts to `scenario__v2_0` (double underscore!)
- Input: `Scenario_V2_0`
- Expected: `scenario_v2_0`
- Actual: `scenario__v2_0` ❌

---

## Python API Expected Input (SORA 2.0)

```python
# From Backend_Python/main.py
class SORA20Request(BaseModel):
    category: str = "SORA-2.0"
    population_density: str  # "Low", "Medium", "High"
    sheltering: str          # "Low", "Medium", "High"
    scenario_v2_0: str       # "VLOS_SparselyPopulated", etc.
    mtom_kg: float = 15.0
    kinetic_energy: Optional[float] = None
    max_characteristic_dimension: Optional[float] = None
    initial_grc: int         # Calculated by Backend
    m1: int                  # ← REQUIRED INTEGER! Not optional!
    m2: str                  # ← STRING (but should calculate m2_value internally)
    m3: int                  # ← REQUIRED INTEGER! Not optional!
```

---

## What mission.html Sends

From `Frontend/Pages/mission.html` lines 2464-2500:

```javascript
const mitigations20 = [];
const m1 = parseInt(document.getElementById('m1_20')?.value || '0');
const m2 = parseInt(document.getElementById('m2_20')?.value || '0');
const m3 = parseInt(document.getElementById('m3_20')?.value || '1');

// Map numeric values back to robustness levels for API
if (m1 === -1) mitigations20.push({ Type: 'M1', Robustness: 'Low' });
else if (m1 === -2) mitigations20.push({ Type: 'M1', Robustness: 'Medium' });
else if (m1 === -4) mitigations20.push({ Type: 'M1', Robustness: 'High' });

if (m2 === -1) mitigations20.push({ Type: 'M2', Robustness: 'Medium' });
else if (m2 === -2) mitigations20.push({ Type: 'M2', Robustness: 'High' });

if (m3 === 0) mitigations20.push({ Type: 'M3', Robustness: 'Medium' });
else if (m3 === -1) mitigations20.push({ Type: 'M3', Robustness: 'High' });

body.GroundRisk = {
    Scenario_V2_0: document.getElementById('scenario20').value,
    MaxCharacteristicDimension: Number(document.getElementById('maxDimension20').value || 0),
    KineticEnergy: ke20 !== undefined ? Math.round(ke20) : undefined,
    Mitigations: mitigations20,  // ← Array of {Type, Robustness}
    IsControlledGroundArea: document.getElementById('controlledGround20')?.value === 'true' || false
};
```

**Issue:** If dropdown is set to default (0), NO mitigation is added to array!
- M1 dropdown default = 0 → No M1 in array → Backend M1 = null → Python API error "m1 required"
- M3 dropdown default = 1 → No M3 in array → Backend M3 = null → Python API error "m3 required"

---

## Root Causes

### 1. M2 Data Type Mismatch
- **Backend stores M2 as string**: `public string? M2 { get; set; }`
- **Python API expects**: `m2: str` (correct) BUT also needs to calculate `m2_value: int` internally
- **Problem**: Backend SHOULD store M2 as string for display, but Python needs both

### 2. Missing M1/M3 When No Mitigation Selected
- **If user doesn't select mitigation**, mission.html doesn't add it to Mitigations array
- **Backend receives empty or partial Mitigations array**
- **Backend leaves M1/M3 as null**
- **Python API rejects request**: "Field required"

### 3. Snake Case Naming Bug
- `Scenario_V2_0` → `scenario__v2_0` (double underscore!)
- Should be: `scenario_v2_0`
- **Fix needed in SnakeCaseNamingPolicy**

### 4. M1/M3 Not Initialized to Zero
- If no mitigation provided, M1/M3 should default to **0** (no reduction)
- Currently defaults to **null**
- Python API requires these fields

---

## Required Fixes

### Fix 1: Initialize M1/M3 to Zero by Default
```csharp
// In ExtractSora20FromMissionHtml, BEFORE processing mitigations:
request.M1 = 0;  // Default: no M1 mitigation
request.M3 = 0;  // Default: no M3 mitigation

// Then extract mitigations (will override if present)
if (groundRisk.TryGetProperty("Mitigations", out var mitigations) && mitigations.ValueKind == JsonValueKind.Array)
{
    // ... existing code
}
```

### Fix 2: Fix SnakeCaseNamingPolicy for Underscores
```csharp
public override string ConvertName(string name)
{
    if (string.IsNullOrEmpty(name)) return name;
    
    var result = new StringBuilder();
    result.Append(char.ToLowerInvariant(name[0]));
    
    for (int i = 1; i < name.Length; i++)
    {
        if (name[i] == '_')
        {
            // Already an underscore, keep it lowercase
            result.Append('_');
        }
        else if (char.IsUpper(name[i]))
        {
            result.Append('_');
            result.Append(char.ToLowerInvariant(name[i]));
        }
        else
        {
            result.Append(name[i]);
        }
    }
    
    return result.ToString();
}
```

### Fix 3: Ensure M2 is Properly Handled
M2 can stay as string in Backend model, but ensure Python API handles it:
- Backend sends: `"m2": "Low"`
- Python calculates: `m2_value = -1`
- Python returns both: `"m2": "Low"` AND `"m2_value": -1`

---

## Testing Commands

### Test 1: PowerShell Direct (Currently Works ✅)
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

Invoke-RestMethod -Uri "http://localhost:5210/api/SoraProxy/complete" -Method Post -Body $body -ContentType "application/json"
```

### Test 2: After Fix (Should Work from UI)
1. Open http://localhost:5210/app/Pages/mission.html
2. Go to SORA 2.0 tab
3. Set M2 = Low
4. Click "Run SORA 2.0 Assessment"
5. **Expected**: M2 (Low): -1 GRC, Total: -3 GRC

---

## Files to Modify

1. **Backend/src/Skyworks.Api/Controllers/SoraProxyController.cs**
   - Line 7-33: Fix SnakeCaseNamingPolicy
   - Line 230: Add `request.M1 = 0;` before mitigation loop
   - Line 231: Add `request.M3 = 0;` before mitigation loop

2. **Test and verify Python API is running**
   - Port 8001 should be listening
   - Health check: http://localhost:8001/health

---

## Current System State

✅ Backend API running on port 5210
✅ Python API running on port 8001  
✅ M2 fix implemented in Backend response transformation
✅ mission.html updated to use m2_value
❌ Python API validation fails when mission.html submits request
❌ M1/M3 missing from Python API request
❌ Snake case conversion creates double underscores

---

## Priority: URGENT 🚨

User confirmed M2 display bug is "megalo sfalma" (big error). PowerShell tests show fix works, but UI integration broken due to validation errors.

**Need immediate fix for snake_case conversion and M1/M3 initialization!**
