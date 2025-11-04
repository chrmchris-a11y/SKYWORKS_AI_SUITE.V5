# SKYWORKS AI SUITE - COMPLETE 422 ERROR FIX REQUEST

## CRITICAL PROBLEM

**STATUS**: SORA calculations fail with **422 Unprocessable Content** error from Python API

**ERROR MESSAGE**: 
```
Python GRC 2.0 call failed - Response status code does not indicate success: 422 (Unprocessable Content)
```

## ROOT CAUSE ANALYSIS

### ✅ PROVEN WORKING: Direct Python API Test
```powershell
$body = '{"mtom_kg": 32.0, "population_density": 2000, "m1_strategic": "Medium", "m2_impact": "High", "m3_erp": "Medium"}'
Invoke-RestMethod -Uri 'http://localhost:8001/api/v1/calculate/grc/2.0' -Method POST -Body $body -ContentType 'application/json'

# RESULT: ✅ SUCCESS 200 OK
# Response:
# version: SORA_2.0
# initial_grc: 6
# final_grc: 2
# mitigation_total: -4
```

### ❌ BROKEN: .NET Backend → Python API Call

**Problem**: .NET `PythonCalculationClient.cs` sends mitigation values WITHOUT `ToTitleCase()` normalization

**Current Flow**:
1. Frontend sends: `{"M1Strategic": "Medium", "M2Impact": "High"}`
2. .NET deserializes to `PythonGRCRequest_2_0` object
3. .NET serializes DIRECTLY without normalization: `JsonSerializer.Serialize(request)`
4. Python receives: `{"m1_strategic": "Medium", ...}` **← This works!**
5. OR Python receives enum names like `{"m1_strategic": "MEDIUM", ...}` **← This fails!**

**KEY FINDING**: 
- `ToTitleCase()` helper EXISTS but is ONLY used in ARC calculations (lines 132, 135, 166, 169)
- GRC calculations (lines 55, 73) use DIRECT serialization: `var json = JsonSerializer.Serialize(request);`
- NO normalization applied to mitigation strings before sending to Python

## AFFECTED FILES

### 1. Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs

**PROBLEM METHODS**:

#### Method 1: `CalculateGRC_2_0` (Line 55)
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);  // ❌ NO NORMALIZATION
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
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

**Request Model** (Line 264):
```csharp
public class PythonGRCRequest_2_0
{
    [JsonPropertyName("mtom_kg")]
    public double MTOM_kg { get; set; }

    [JsonPropertyName("population_density")]
    public double PopulationDensity { get; set; }

    [JsonPropertyName("m1_strategic")]
    public string? M1Strategic { get; set; }  // ❌ RAW STRING - NO NORMALIZATION

    [JsonPropertyName("m2_impact")]
    public string? M2Impact { get; set; }  // ❌ RAW STRING - NO NORMALIZATION

    [JsonPropertyName("m3_erp")]
    public string? M3ERP { get; set; }  // ❌ RAW STRING - NO NORMALIZATION
    
    [JsonPropertyName("environment_type")]
    public string? EnvironmentType { get; set; }
}
```

#### Method 2: `CalculateGRC_2_5` (Line 73)
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);  // ❌ NO NORMALIZATION
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python GRC 2.5 with: {Request}", json);
        
        var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.5", content);
        // ... rest of method
    }
}
```

**WORKING EXAMPLE** - ARC Calculation (Lines 128-145):
```csharp
public async Task<PythonARCResponse?> CalculateARC_2_0(PythonARCRequest_2_0 request)
{
    try
    {
        // ✅ CORRECT: Creates normalized object before serialization
        var normalizedRequest = new
        {
            scenario = request.Scenario,
            environment = ToTitleCase(request.Environment),  // ✅ NORMALIZED
            m1_strategic = ToTitleCase(request.M1Strategic), // ✅ NORMALIZED
            m2_effects = ToTitleCase(request.M2Effects)      // ✅ NORMALIZED
        };
        
        var json = JsonSerializer.Serialize(normalizedRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python ARC 2.0 with: {Request}", json);
        
        var response = await _httpClient.PostAsync("/api/v1/calculate/arc/2.0", content);
        response.EnsureSuccessStatusCode();
        // ... rest of method
    }
}
```

**ToTitleCase Helper** (Line 214):
```csharp
// Normalize uppercase enum strings to title-case for Python
private string ToTitleCase(string s)
{
    if (string.IsNullOrWhiteSpace(s)) return s ?? string.Empty;
    var lower = s.ToLowerInvariant();
    if (lower == "none") return "None";
    if (lower == "low") return "Low";
    if (lower == "medium") return "Medium";
    if (lower == "high") return "High";
    // Environments
    if (lower == "rural") return "Rural";
    if (lower == "urban") return "Urban";
    if (lower == "suburban") return "Suburban";
    if (lower == "controlled") return "Controlled";
    if (lower == "industrial") return "Industrial";
    // Default: capitalize first letter
    return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
}
```

### 2. Backend_Python/models/sora_models.py

**ALREADY FIXED** ✅ (Lines 20-34):
```python
class MitigationLevel(str, Enum):
    """Mitigation robustness levels - Case-insensitive (accepts low/Low, medium/Medium, high/High)"""
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"
    
    @classmethod
    def _missing_(cls, value):
        """Allow case-insensitive enum matching"""
        if isinstance(value, str):
            value_upper = value.upper()
            for member in cls:
                if member.name == value_upper:
                    return member
        return None
```

**VALIDATION**: Direct Python API test proves this works perfectly.

## REQUIRED FIXES

### Fix 1: Normalize GRC 2.0 Request (CRITICAL)

**File**: `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`

**Method**: `CalculateGRC_2_0` (Line 55)

**REPLACE THIS**:
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python GRC 2.0 with: {Request}", json);
```

**WITH THIS**:
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
        
        _logger.LogInformation("Calling Python GRC 2.0 with: {Request}", json);
```

### Fix 2: Normalize GRC 2.5 Request (CRITICAL)

**File**: `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`

**Method**: `CalculateGRC_2_5` (Line 73)

**REPLACE THIS**:
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python GRC 2.5 with: {Request}", json);
```

**WITH THIS**:
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
{
    try
    {
        // Normalize mitigation strings to TitleCase for Python API
        var normalizedRequest = new
        {
            mtom_kg = request.MTOM_kg,
            characteristic_dimension_m = request.CharacteristicDimension_m,
            max_speed_mps = request.MaxSpeed_mps,
            population_density = request.PopulationDensity,
            m1a_sheltering = ToTitleCase(request.M1A_Sheltering),
            m1b_impact_angle = ToTitleCase(request.M1B_ImpactAngle),
            m2_containment = ToTitleCase(request.M2Containment),
            m3_erp = ToTitleCase(request.M3ERP),
            environment_type = ToTitleCase(request.EnvironmentType)
        };
        
        var json = JsonSerializer.Serialize(normalizedRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python GRC 2.5 with: {Request}", json);
```

## VALIDATION STEPS

### Step 1: Apply Fixes
1. Open `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`
2. Apply Fix 1 to `CalculateGRC_2_0` method (line 55)
3. Apply Fix 2 to `CalculateGRC_2_5` method (line 73)
4. Save file

### Step 2: Rebuild Backend
```powershell
cd Backend
dotnet build Skyworks.sln
```

### Step 3: Restart Services
```powershell
# Stop existing services (Ctrl+C in terminals)
# Start .NET Backend
cd Backend
dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210

# Start Python FastAPI (in another terminal)
cd Backend_Python
.\venv\Scripts\python.exe -m uvicorn main:app --host 0.0.0.0 --port 8001
```

### Step 4: Test Mission Planner UI
1. Open browser: `http://localhost:5210/app/Pages/mission.html`
2. Select drone: "DJI Mini 4 Pro" (MTOM: 249g)
3. Choose SORA 2.0 scenario: "VLOS - Populated Area"
4. Set mitigations:
   - M1 Strategic: Medium
   - M2 Impact: High
   - M3 ERP: Medium
5. Click "Execute SORA Assessment"
6. **Expected Result**: 
   ```json
   {
     "version": "SORA_2.0",
     "initial_grc": 4,
     "final_grc": 1,
     "mitigation_total": -3,
     "result": "SUCCESS"
   }
   ```

### Step 5: Run 20 Test Scenarios
Complete user requirement: 10 SORA 2.0 + 10 SORA 2.5 calculations via Mission Planner UI.

## TECHNICAL SUMMARY

**Architecture**:
- **Frontend**: HTML/JavaScript Mission Planner (`Frontend/Pages/mission.html`)
- **Backend**: .NET 8.0 API (port 5210) - Serves UI, proxies calculations
- **Python**: FastAPI (port 8001) - SORA calculation engine

**Issue**: .NET→Python integration layer missing mitigation string normalization

**Solution**: Apply `ToTitleCase()` helper (already exists in codebase) to all mitigation strings before sending to Python API, following pattern used successfully in ARC calculations.

**Impact**: 
- ✅ Python API: WORKING (proven via direct test)
- ✅ Python enum fix: WORKING (case-insensitive matching)
- ❌ .NET serialization: BROKEN (missing normalization)
- 🎯 **FIX REQUIRED**: 2 methods, ~10 lines of code each

**Validation**: Claude Sonnet 4 previously validated entire SORA implementation as "100% COMPLIANT" with EASA/JARUS specifications.

## FILES TO FIX

**ONLY ONE FILE NEEDS MODIFICATION**:
```
Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs
```

**Lines to modify**:
- Line 55-62: `CalculateGRC_2_0` method
- Line 73-80: `CalculateGRC_2_5` method

**Pattern to follow**: Lines 128-145 (`CalculateARC_2_0` method)

**Helper to use**: Line 214 (`ToTitleCase` method) - ALREADY EXISTS

---

**END OF FIX REQUEST**

**REQUEST**: Please apply these specific fixes to `PythonCalculationClient.cs` following the exact pattern shown in the working ARC calculation methods.
