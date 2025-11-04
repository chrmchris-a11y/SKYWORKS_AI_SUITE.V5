# C# 422 Error Fix Request - .NET Backend Only

## Problem
.NET backend returns **422 Unprocessable Content** when calling Python API for SORA 2.0 GRC calculations.

## What Works
- ✅ **Direct Python API calls** return 200 OK
- ✅ Python has case-insensitive enum matching
- ✅ Python API tested and working with payloads like:
```json
{"mtom_kg":0.249,"population_density":250,"m1_strategic":"Medium","m2_impact":"High","m3_erp":null}
```

## What Fails
- ❌ **.NET→Python integration** returns 422
- ❌ Most drones fail (DJI Mini 4 Pro 0.249kg consistently fails)
- ❌ Mission Planner UI shows "Bad Request"

## Root Cause Analysis

### Issue 1: MTOM_kg = 0 when null
**File:** `SORAOrchestrationService.cs` line 331

```csharp
var req = new Skyworks.Core.Services.Python.PythonGRCRequest_2_0
{
    MTOM_kg = input.MTOM_kg ?? 0,  // ⚠️ PROBLEM: Sends 0 instead of actual weight!
    PopulationDensity = populationDensity,
    // ...
};
```

**Problem:** If `input.MTOM_kg` is null, sends 0 to Python. This breaks iGRC calculation.

### Issue 2: Unknown RobustnessToString() format
**File:** `SORAOrchestrationService.cs` (unknown line)

```csharp
M1Strategic = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M1", StringComparison.OrdinalIgnoreCase))?.Robustness),
```

**Problem:** Don't know if this returns "Medium" (correct) or "MEDIUM" (wrong).

### Issue 3: No error response logging
**File:** `PythonCalculationClient.cs` line 74

```csharp
var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.0", content);
response.EnsureSuccessStatusCode();  // ⚠️ Throws exception without showing Python error!
```

**Problem:** When Python returns 422, can't see Pydantic validation errors.

## Python API Expectations

```python
class GRCRequest_2_0(BaseModel):
    mtom_kg: float = Field(..., ge=0, description="Maximum Take-Off Mass")
    population_density: int = Field(..., ge=0, description="People per km²")
    m1_strategic: Optional[MitigationLevel] = None
    m2_impact: Optional[MitigationLevel] = None
    m3_erp: Optional[MitigationLevel] = None
    environment_type: Optional[str] = "Urban"

class MitigationLevel(str, Enum):
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"
```

## Request: Please Fix These C# Files

### 1. SORAOrchestrationService.cs - ExecuteGroundRisk_V2_0

**Current code (lines 306-350):**
```csharp
private (bool isValid, int intrinsicGRC, int finalGRC, string notes, string message) ExecuteGroundRisk_V2_0(GroundRiskInput input)
{
    if (_py != null)
    {
        try
        {
            // Map SORA 2.0 Scenario to representative population density
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
            
            var req = new Skyworks.Core.Services.Python.PythonGRCRequest_2_0
            {
                MTOM_kg = input.MTOM_kg ?? 0,  // ⚠️ FIX THIS
                PopulationDensity = populationDensity,
                M1Strategic = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M1", StringComparison.OrdinalIgnoreCase))?.Robustness),
                M2Impact = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M2", StringComparison.OrdinalIgnoreCase))?.Robustness),
                M3ERP = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M3", StringComparison.OrdinalIgnoreCase))?.Robustness),
                EnvironmentType = "Urban"
            };
            
            _logger?.LogInformation("Calling Python GRC 2.0: MTOM={MTOM}kg, PopDens={PopDens}, M1={M1}, M2={M2}, M3={M3}", 
                req.MTOM_kg, req.PopulationDensity, req.M1Strategic, req.M2Impact, req.M3ERP);
            
            var pyRes = _py.CalculateGRC_2_0(req).GetAwaiter().GetResult();
            if (pyRes != null)
            {
                var notes = string.IsNullOrWhiteSpace(pyRes.Notes) ? "Python authoritative GRC 2.0 (Table 2)" : pyRes.Notes;
                var final = Math.Min(Math.Max(pyRes.FinalGRC, 1), 7);
                _logger?.LogInformation("Python GRC 2.0 result: iGRC={IGRC}, Final GRC={FinalGRC}", pyRes.IntrinsicGRC, final);
                return (true, pyRes.IntrinsicGRC, final, notes, "");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Proxy-only mode: Python GRC 2.0 call failed - {Message}", ex.Message);
            return (false, 0, 0, "", $"Proxy-only mode: Python GRC 2.0 call failed - {ex.Message}");
        }
    }
    // ... fallback code ...
}
```

**Fixes needed:**
1. ✅ Validate `input.MTOM_kg` is not null/0 before sending
2. ✅ Add better error message if MTOM_kg missing
3. ✅ Show exact JSON being sent in logs

### 2. PythonCalculationClient.cs - CalculateGRC_2_0

**Current code (lines 55-82):**
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
        response.EnsureSuccessStatusCode();  // ⚠️ FIX THIS
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calling Python GRC 2.0 calculation");
        throw;
    }
}
```

**Fixes needed:**
1. ✅ Capture Python 422 error response body before throwing
2. ✅ Log Pydantic validation errors
3. ✅ Validate request fields before sending

### 3. Show RobustnessToString() Implementation

**Need to see full implementation to verify it returns TitleCase:**
```csharp
private string? RobustnessToString(RobustnessLevel? level)
{
    // SHOW FULL IMPLEMENTATION HERE
}
```

## Expected Output

Please provide:

1. ✅ **Complete FIXED SORAOrchestrationService.cs ExecuteGroundRisk_V2_0 method**
2. ✅ **Complete FIXED PythonCalculationClient.cs CalculateGRC_2_0 method**
3. ✅ **Full RobustnessToString() helper method** (with fix if needed)
4. ✅ **Explanation** of what was wrong
5. ✅ **Test command** to verify the fix

## Test Case

After fixes, this should return 200 OK:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5210/api/sora/complete' `
  -Method POST `
  -ContentType 'application/json' `
  -Body '{
    "SoraVersion":"2.0",
    "DroneId":"DJI_MINI4PRO",
    "GroundRisk":{
      "Scenario_V2_0":"VLOS_SparselyPopulated",
      "Mitigations":[
        {"Type":"M1","Robustness":"Medium"},
        {"Type":"M2","Robustness":"High"}
      ]
    },
    "AirRisk":{
      "AirspaceControl":"Uncontrolled",
      "LocationType":"NonAirport",
      "Environment":"Urban",
      "Typicality":"Typical"
    },
    "ImplementedOSOs":[]
  }'
```

Expected result:
```json
{
  "isSuccessful": true,
  "intrinsicGRC": 2,
  "finalGRC": 1,
  "groundRiskNotes": "Python authoritative GRC 2.0...",
  "errors": []
}
```
