# CRITICAL FIX REQUEST: 422 Unprocessable Entity Error in SORA 2.0/2.5 Calculations

## 🚨 CRITICAL ISSUE

Our SKYWORKS AI Suite V5 platform is experiencing a **PERSISTENT 422 Unprocessable Entity error** when the .NET backend calls the Python FastAPI for SORA Ground Risk Class (GRC) calculations. This is **BLOCKING ALL CALCULATIONS** and preventing the system from functioning.

---

## 📋 SYSTEM ARCHITECTURE

### Backend Components:
1. **.NET 8.0 Backend API** (Port 5210)
   - `SORAOrchestrationService.cs` - Main orchestration
   - `PythonCalculationClient.cs` - HTTP client to Python API
   - Handles SORA complete workflow: GRC → ARC → SAIL → OSO

2. **Python FastAPI** (Port 8001)
   - `main.py` - SORA 2.0/2.5 GRC calculation endpoints
   - Uses Pydantic models with enum validation
   - Implements EASA/JARUS Table 2 authoritative calculations

3. **Frontend Mission Planner** (HTML/JavaScript)
   - Sends complete SORA assessment requests to .NET API
   - Displays results: iGRC, final GRC, ARC, SAIL, OSOs

---

## 🔴 THE PROBLEM

### Current Status:
- ✅ **Python API works PERFECTLY** when called directly (200 OK, correct calculations)
- ❌ **.NET → Python integration FAILS** with 422 error (Unprocessable Content)
- ✅ **UI loads correctly**, drone catalog works (64 drones)
- ❌ **ALL SORA calculations fail** at the Python API call stage

### Error Message:
```
Ground Risk: Proxy-only mode: Python GRC 2.0 call failed - Response status code does not indicate success: 422 (Unprocessable Content).
```

### Test Results:

**✅ Direct Python API Test (WORKS):**
```powershell
Invoke-RestMethod -Uri 'http://localhost:8001/api/v1/calculate/grc/2.0' -Method POST -Body '{
  "mtom_kg": 0.249,
  "population_density": 5000,
  "m1_strategic": "Medium",
  "m2_impact": "High",
  "m3_erp": null
}'

# Response: 200 OK
{
  "version": "SORA_2.0",
  "initial_grc": 3,
  "final_grc": 1,
  "mitigation_total": -4
}
```

**❌ .NET API Test (FAILS):**
```powershell
Invoke-RestMethod -Uri 'http://localhost:5210/api/sora/complete' -Method POST -Body '{
  "SoraVersion": "2.0",
  "DroneId": "DJI_MINI4PRO",
  "GroundRisk": {
    "Scenario_V2_0": "VLOS_Populated",
    "Mitigations": [
      {"Type": "M1", "Robustness": "Medium"},
      {"Type": "M2", "Robustness": "High"}
    ]
  },
  "AirRisk": {...}
}'

# Response: 422 error, intrinsicGRC = 0
```

---

## 🔍 ROOT CAUSE ANALYSIS

### What We've Tried (ALL FAILED):

1. ❌ **Python Enum Case Sensitivity Fix**
   - Added `_missing_()` method to `MitigationLevel` enum
   - Accepts "MEDIUM", "medium", "Medium" → converts to "Medium"
   - **Result**: Direct Python tests pass, but .NET integration still 422

2. ❌ **.NET ToTitleCase Normalization**
   - Added `ToTitleCase()` helper in `PythonCalculationClient.cs`
   - Normalizes "MEDIUM" → "Medium", "HIGH" → "High"
   - Applied to both `CalculateGRC_2_0()` and `CalculateGRC_2_5()` methods
   - **Result**: Code compiles, but 422 error persists

3. ❌ **Scenario → PopulationDensity Mapping**
   - Added mapping in `SORAOrchestrationService.cs` (lines 306-330)
   - VLOS_Populated → 5000, VLOS_Sparsely → 250, etc.
   - **Result**: Build succeeds, but 422 error still occurs

4. ❌ **Multiple Clean Rebuilds**
   - `dotnet clean` + `dotnet build`
   - Killed all .NET processes
   - Restarted backend 5+ times
   - **Result**: DLL rebuilt correctly, but 422 persists

5. ❌ **Added Debug Logging**
   - `Console.WriteLine` in PythonCalculationClient
   - `print()` statements in Python main.py
   - **Result**: Cannot access logs from background VS Code tasks

### The Mystery:
- One screenshot showed **Sky Tech SC15 (32kg) WORKING** with correct GRC calculation
- All other tests with **DJI Mini 4 Pro (0.249kg) FAIL** with 422 error
- Direct Python API accepts both payloads successfully
- The .NET → Python HTTP serialization layer is sending **something different** that Python rejects

---

## 📚 AUTHORITATIVE EASA/JARUS SPECIFICATIONS

### SORA 2.0 (EASA AMC1 UAS.OPEN.040/050/060):

**Table 2: Initial GRC Determination (MTOM-based)**
```
Population Density (people/km²):
- Controlled Ground Area: 0
- ≤ 500: Sparse
- 501 - 10,000: Moderately Populated
- > 10,000: Densely Populated

MTOM Categories:
- Cat 0: ≤ 0.25 kg (250g)
- Cat 1: 0.25 - 1 kg
- Cat 2: 1 - 3 kg
- Cat 3: 3 - 8 kg
- Cat 4: 8 - 25 kg
- Cat 5: > 25 kg

Initial GRC Matrix: MTOM Category × Population Density
```

**Mitigations (SORA 2.0)**:
- **M1 Strategic** (ground contingency procedures):
  - Low: -1 GRC
  - Medium: -2 GRC
  - High: -4 GRC
  
- **M2 Impact** (design/impact reduction):
  - Medium: -1 GRC
  - High: -2 GRC
  
- **M3 ERP** (Emergency Response Plan):
  - Medium: 0 GRC (required for certain scenarios)
  - High: -1 GRC

**Final GRC Calculation**:
```
Final GRC = max(1, Initial GRC + M1 + M2 + M3)
```

### SORA 2.5 (JARUS SORA 2.5 - JAR-DEL-SRM-SORA-MB-2.5):

**Table 2: Initial GRC (Dimension + Speed based)**
```
Characteristic Dimension × Max Speed → Kinetic Energy Category
Combined with Population Density → Initial GRC (1-7)
```

**Mitigations (SORA 2.5)**:
- **M1A Sheltering**: -1 to -2 GRC
- **M1B Operational**: -1 to -2 GRC  
- **M1C Ground Observation**: -1 GRC
- **M2 Impact**: -1 to -2 GRC

---

## 🎯 WHAT WE NEED FROM YOU

### **PRIMARY OBJECTIVE**: 
**FIX THE 422 ERROR SO .NET → PYTHON INTEGRATION WORKS**

### Specific Requirements:

1. **Identify the Exact Payload Difference**
   - What is .NET sending that Python rejects?
   - Compare working direct Python payload vs. failing .NET payload
   - Check for extra fields, wrong types, incorrect enum values

2. **Fix the C# Code**
   - Provide complete fixed code for:
     - `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`
     - `Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs`
   - Ensure proper JSON serialization to match Python Pydantic models

3. **Ensure Compliance with EASA/JARUS**
   - All calculations must follow authoritative specifications
   - Initial GRC, Final GRC, ARC, SAIL must be 100% compliant
   - Sequential mitigation application per JARUS rules

4. **Complete System Alignment**
   - ✅ UI sends correct payload format
   - ✅ .NET backend processes and forwards correctly
   - ✅ Python API validates and calculates correctly
   - ✅ Results flow back through entire stack
   - ✅ All 64 drone models work (from 0.249kg to 600kg)

---

## 📦 KEY FILES TO FIX

### 1. `PythonCalculationClient.cs` (Lines 55-120)
**Current ToTitleCase implementation:**
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request)
{
    try
    {
        var normalizedRequest = new
        {
            mtom_kg = request.MTOM_kg,
            population_density = request.PopulationDensity,
            m1_strategic = ToTitleCase(request.M1Strategic),
            m2_impact = ToTitleCase(request.M2Impact),
            m3_erp = ToTitleCase(request.M3ERP),
            environment_type = ToTitleCase(request.EnvironmentType)
        };
        
        var json = JsonSerializer.Serialize(normalizedRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        Console.WriteLine($"[DEBUG] GRC 2.0 Payload: {json}");
        _logger.LogInformation("Calling Python GRC 2.0 with: {Request}", json);
        
        var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.0", content);
        response.EnsureSuccessStatusCode(); // ← THIS THROWS 422 ERROR
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, ...);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Python GRC 2.0 call failed - {Message}", ex.Message);
        return null;
    }
}
```

**Problem**: The `normalizedRequest` anonymous object is sent, but Python returns 422.

### 2. `SORAOrchestrationService.cs` (Lines 306-350)
**Current Scenario → PopulationDensity mapping:**
```csharp
private (bool isValid, int intrinsicGRC, int finalGRC, string notes, string message) 
    ExecuteGroundRisk_V2_0(GroundRiskInput input)
{
    if (_py != null)
    {
        try
        {
            int populationDensity = (int)(input.PopulationDensity ?? 0);
            if (input.Scenario_V2_0.HasValue && populationDensity == 0)
            {
                populationDensity = input.Scenario_V2_0.Value switch
                {
                    OperationalScenario.ControlledGroundArea => 0,
                    OperationalScenario.VLOS_SparselyPopulated => 250,
                    OperationalScenario.BVLOS_SparselyPopulated => 250,
                    OperationalScenario.VLOS_Populated => 5000,
                    OperationalScenario.BVLOS_Populated => 5000,
                    OperationalScenario.VLOS_GatheringOfPeople => 15000,
                    OperationalScenario.BVLOS_GatheringOfPeople => 15000,
                    _ => 1000
                };
            }
            
            var req = new PythonGRCRequest_2_0
            {
                MTOM_kg = input.MTOM_kg ?? 0,  // ← If null, sends 0!
                PopulationDensity = populationDensity,
                M1Strategic = RobustnessToString(...),
                M2Impact = RobustnessToString(...),
                M3ERP = RobustnessToString(...),
                EnvironmentType = "Urban"
            };
            
            var pyRes = _py.CalculateGRC_2_0(req).GetAwaiter().GetResult();
            // ...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python GRC 2.0 proxy failed: {Message}", ex.Message);
            return (false, 0, 0, "", $"Proxy-only mode: Python GRC 2.0 call failed - {ex.Message}");
        }
    }
}
```

**Problem**: Even with correct mapping, Python returns 422.

### 3. Python `main.py` Endpoint (Lines 356-385)
```python
@app.post("/api/v1/calculate/grc/2.0", response_model=GRCResponse)
async def calculate_grc_2_0(request: GRCRequest_2_0):
    try:
        print(f"[PYTHON GRC 2.0] Received request: {request.dict()}")
        log_request_data("GRC 2.0", request.dict())
        
        initial_result = JARUSSORATable2.calculate_initial_grc(
            request.mtom_kg, 
            request.population_density
        )
        # ... mitigation application ...
        
        return GRCResponse(...)
    except ValidationError as ve:
        raise HTTPException(status_code=422, detail=str(ve))
```

**Problem**: Pydantic validation fails with 422, but direct API calls work.

---

## 🚫 WHAT NOT TO DO

### Please DO NOT:
1. ❌ Send generic "check your JSON" advice - we've done that
2. ❌ Suggest adding logging - we already have it (just can't access it)
3. ❌ Recommend testing with Postman - direct Python API tests already pass
4. ❌ Fix only the Python side - the Python API works correctly
5. ❌ Suggest "validate your schema" - we need the actual C# fix
6. ❌ Repeat previous solutions that failed (enum fix, ToTitleCase, etc.)

### What We Need Instead:
✅ **EXACT C# code fixes** for PythonCalculationClient.cs and SORAOrchestrationService.cs
✅ **Specific explanation** of what the .NET code is doing wrong
✅ **Complete working implementation** that we can copy-paste
✅ **Verification** that payload matches Python Pydantic model exactly

---

## ✅ SUCCESS CRITERIA

The fix is successful when:

1. ✅ All direct .NET API calls return **200 OK** (not 422)
2. ✅ Mission Planner UI calculations work for **all 64 drones**
3. ✅ Both SORA 2.0 and SORA 2.5 calculations succeed
4. ✅ Initial GRC, Final GRC, ARC, SAIL are **100% EASA/JARUS compliant**
5. ✅ Sequential mitigation application works correctly
6. ✅ All test scenarios pass:
   - DJI Mini 4 Pro (0.249kg)
   - DJI Mavic 3 Enterprise (0.895kg)
   - DJI Matrice 30T (3.77kg)
   - Sky Tech SC15 (32kg)
   - Freefly Alta X (125kg)
   - Wingtra One (600kg)

---

## 🎯 DELIVERABLES NEEDED

Please provide:

1. **Complete Fixed `PythonCalculationClient.cs`**
   - Full file content (lines 1-250)
   - With all necessary using statements
   - Correct JSON serialization
   - Proper null handling

2. **Complete Fixed `SORAOrchestrationService.cs`**
   - Focus on `ExecuteGroundRisk_V2_0()` method
   - Correct MTOM_kg handling (don't send 0 if null)
   - Proper scenario mapping
   - RobustnessToString() implementation

3. **Explanation of the Fix**
   - What was wrong with the previous implementation?
   - Why does direct Python API work but .NET integration fails?
   - What exact changes make it work?

4. **Verification Steps**
   - How to test the fix
   - Expected responses for test payloads

---

## 🔥 URGENCY

This is **BLOCKING PRODUCTION**. The entire SKYWORKS AI Suite V5 platform cannot function without SORA calculations. We need:

- ✅ **DEFINITIVE AND PERMANENT SOLUTION**
- ✅ **NO MORE TRIAL-AND-ERROR FIXES**
- ✅ **100% EASA/JARUS COMPLIANT CALCULATIONS**
- ✅ **COMPLETE SYSTEM ALIGNMENT** (UI → .NET → Python → Results)

We've tried 5 different approaches over multiple sessions. We need a **comprehensive, final fix** that resolves this once and for all.

---

## 📞 CONTEXT

- Platform: SKYWORKS AI Suite V5 (SORA 2.0/2.5 Compliance)
- Backend: .NET 8.0 + Python FastAPI
- Frontend: HTML/JavaScript Mission Planner
- Standards: EASA AMC1 UAS.OPEN + JARUS SORA 2.5
- User: Aviation compliance expert requiring authoritative calculations
- Status: **CRITICAL - SYSTEM DOWN**

**Please provide the complete, working C# fixes now. Thank you!**
