# 422 Unprocessable Content Error - Fix Request for Claude Sonnet 4

## Problem Summary
SORA 2.0 calculations fail with **422 Unprocessable Content** when calling Python FastAPI from .NET backend.

**Error Message:**
```
Ground Risk: Proxy-only mode: Python GRC 2.0 call failed - Response status code does not indicate success: 422 (Unprocessable Content).
```

## What Works ✅
- **Direct Python API calls** return 200 OK:
```powershell
Invoke-RestMethod -Uri 'http://localhost:8001/api/v1/calculate/grc/2.0' -Method POST -Body '{"mtom_kg":0.249,"population_density":250,"m1_strategic":"Medium","m2_impact":"High","m3_erp":null}'
# Result: 200 OK, initial_grc=2, final_grc=1
```

- **Sky Tech SC15 (32kg)** worked from Mission Planner UI
- **Python enum** has `_missing_()` for case-insensitive matching
- **.NET ToTitleCase()** applied to all mitigation strings

## What Fails ❌
- **.NET→Python integration** returns 422 for most drones
- **DJI Mini 4 Pro (0.249kg)** fails consistently
- **Mission Planner UI** shows "Bad Request" after calculation

## Code Files

### 1. PythonCalculationClient.cs (Lines 55-80)
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request)
{
    try
    {
        // Normalize mitigation strings to TitleCase for Python API
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
        response.EnsureSuccessStatusCode();
        
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

### 2. SORAOrchestrationService.cs ExecuteGroundRisk_V2_0 (Lines 306-350)
```csharp
private (bool isValid, int intrinsicGRC, int finalGRC, string notes, string message) ExecuteGroundRisk_V2_0(GroundRiskInput input)
{
    // Try authoritative Python microservice first
    if (_py != null)
    {
        try
        {
            // Map SORA 2.0 Scenario to representative population density for Python API
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
                MTOM_kg = input.MTOM_kg ?? 0,  // ⚠️ PROBLEM: If null, sends 0!
                PopulationDensity = populationDensity,
                M1Strategic = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M1", StringComparison.OrdinalIgnoreCase))?.Robustness),
                M2Impact = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M2", StringComparison.OrdinalIgnoreCase))?.Robustness),
                M3ERP = RobustnessToString(input.Mitigations.FirstOrDefault(m => m.Type.Equals("M3", StringComparison.OrdinalIgnoreCase))?.Robustness),
                EnvironmentType = "Urban" // Default - could be parameterized later
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
    
    // Fallback...
}
```

### 3. RobustnessToString Helper Method
**Location:** SORAOrchestrationService.cs (need to verify implementation)

```csharp
private string? RobustnessToString(RobustnessLevel? level)
{
    // ⚠️ UNKNOWN: Does this return "Medium" or "MEDIUM"?
    // Need to check if it already does TitleCase conversion
}
```

## Python API Expectations

### Working Python Endpoint (Backend_Python/main.py line 356)
```python
@app.post("/api/v1/calculate/grc/2.0", response_model=GRCResponse)
async def calculate_grc_2_0(request: GRCRequest_2_0):
    print(f"[PYTHON GRC 2.0] Received request: {request.dict()}")
    # ...
```

### Python Model (Backend_Python/models/sora_models.py)
```python
class MitigationLevel(str, Enum):
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"
    
    @classmethod
    def _missing_(cls, value):
        if isinstance(value, str):
            value_upper = value.upper()
            for member in cls:
                if member.name == value_upper:
                    return member
        return None

class GRCRequest_2_0(BaseModel):
    mtom_kg: float = Field(..., ge=0, description="Maximum Take-Off Mass in kilograms")
    population_density: int = Field(..., ge=0, description="People per km²")
    m1_strategic: Optional[MitigationLevel] = None
    m2_impact: Optional[MitigationLevel] = None
    m3_erp: Optional[MitigationLevel] = None
    environment_type: Optional[str] = "Urban"
```

## Suspected Issues

### 1. MTOM_kg = 0 Problem
Line 331 in SORAOrchestrationService.cs:
```csharp
MTOM_kg = input.MTOM_kg ?? 0,  // If null, sends 0 instead of actual drone weight!
```

**Problem:** If `input.MTOM_kg` is null, it sends 0 to Python, which causes incorrect iGRC calculation.

**Test Case:**
- DJI Mini 4 Pro: MTOM = 0.249 kg
- If this is null or not populated, sends 0 → Python calculates wrong iGRC → 422 error?

### 2. RobustnessToString() Format
Unknown if this method returns:
- ✅ "Medium" (correct TitleCase)
- ❌ "MEDIUM" (uppercase - wrong)
- ❌ "medium" (lowercase - wrong)

### 3. Missing environment_type Field
Python expects `environment_type: Optional[str] = "Urban"` but .NET always sends "Urban" hardcoded.

### 4. Null Handling
Python Pydantic model has `Optional[MitigationLevel]` but .NET might be sending empty strings instead of null.

## EASA/JARUS References

### SORA 2.0 Ground Risk (Table 2)
**Source:** EASA AMC1 UAS.OPEN.040/050/060 & JARUS SORA JAR_doc_06_DEL_WG6_SORA_V2.0

**Initial GRC Determination:**
1. MTOM-based categories (Table 2):
   - Cat 0: ≤0.25 kg & ≤25 m/s → iGRC depends on PopDens
   - Cat 1: >0.25kg to 5kg
   - Cat 2: >5kg to 15kg
   - Cat 3: >15kg

2. Population Density:
   - Controlled Ground Area: 0 people/km²
   - Sparsely Populated: ≤500 people/km²
   - Populated: 501-10,000 people/km²
   - Gathering of People: >10,000 people/km²

**Mitigation Reductions (SORA 2.0):**
- M1 Strategic: Low=-1, Medium=-2, High=-4
- M2 Impact: Medium=-1, High=-2
- M3 ERP: Medium=0, High=-1

**Final GRC:** max(1, iGRC + Σmitigations)

## Request to Claude Sonnet 4

**Please provide FIXED code for:**

1. **SORAOrchestrationService.cs** - ExecuteGroundRisk_V2_0 method
   - Fix MTOM_kg null handling (line 331)
   - Ensure drone specs are populated correctly
   - Verify RobustnessToString() returns TitleCase

2. **PythonCalculationClient.cs** - CalculateGRC_2_0 method
   - Add better error logging to show exact JSON payload
   - Add validation before sending to Python
   - Handle null mitigations correctly

3. **RobustnessToString() helper method**
   - Show full implementation
   - Ensure returns "None", "Low", "Medium", "High" (TitleCase)

4. **Diagnostic improvements**
   - Add logging to show exact payload before HTTP call
   - Capture Python 422 error response body
   - Show validation errors from Pydantic

## Expected Output

Please provide:
1. ✅ Complete fixed C# code for both files
2. ✅ Explanation of what was wrong
3. ✅ Test cases to verify the fix
4. ✅ Any additional recommendations

**Goal:** All SORA 2.0 calculations should return 200 OK from Python API when called via .NET backend.
