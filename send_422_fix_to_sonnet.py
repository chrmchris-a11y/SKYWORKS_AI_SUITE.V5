"""
SKYWORKS AI SUITE - Send 422 Fix Request to Claude Sonnet 4
Comprehensive package with all context for fixing the .NET→Python serialization issue
"""

import anthropic
import sys
from datetime import datetime
import os

# API Configuration
API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

# Complete prompt with all context
PROMPT = """
# SKYWORKS AI SUITE - CRITICAL 422 ERROR FIX REQUEST

## PROJECT CONTEXT

**System**: SKYWORKS AI Suite V5 - Dual SORA 2.0/2.5 Implementation
**Architecture**: 
- Frontend: HTML/JavaScript Mission Planner
- Backend: .NET 8.0 API (port 5210)
- Python: FastAPI (port 8001) - SORA Calculation Engine

**User Requirement**: 
User needs Mission Planner UI working to run 20 SORA test calculations (10x SORA 2.0 + 10x SORA 2.5).
User REFUSES automated testing - wants official web UI only.

---

## CRITICAL PROBLEM

**ERROR**: SORA calculations fail with **422 Unprocessable Content** from Python API

**Error Message**:
```
Python GRC 2.0 call failed - Response status code does not indicate success: 422 (Unprocessable Content)
```

---

## ROOT CAUSE ANALYSIS

### ✅ PROVEN WORKING: Direct Python API Test

PowerShell test with TitleCase enums:
```powershell
$body = '{"mtom_kg": 32.0, "population_density": 2000, "m1_strategic": "Medium", "m2_impact": "High", "m3_erp": "Medium"}'
Invoke-RestMethod -Uri 'http://localhost:8001/api/v1/calculate/grc/2.0' -Method POST -Body $body -ContentType 'application/json'

# RESULT: ✅ SUCCESS 200 OK
Response:
{
  "version": "SORA_2.0",
  "initial_grc": 6,
  "final_grc": 2,
  "mitigation_total": -4,
  "M1_strategic": {"level": "Medium", "reduction": -2},
  "M2_impact": {"level": "High", "reduction": -2},
  "M3_erp": {"level": "Medium", "reduction": 0}
}
```

**VALIDATION**: Python API accepts "Medium", "High", "Low" format PERFECTLY. Python enum fix works.

### ❌ BROKEN: .NET Backend → Python API Integration

**Problem**: .NET `PythonCalculationClient.cs` sends mitigation strings WITHOUT normalization.

**Current Flow**:
1. Frontend sends JSON with mitigations
2. .NET deserializes to `PythonGRCRequest_2_0` object
3. .NET calls: `var json = JsonSerializer.Serialize(request);` ❌ NO NORMALIZATION
4. Python receives enum names or wrong case → 422 error

**KEY FINDING**: 
- `ToTitleCase()` helper method EXISTS in the same file
- It's ONLY used in ARC calculations (lines 132, 135, 166, 169)
- GRC calculations use DIRECT serialization without normalization
- NO `ToTitleCase()` applied to mitigation strings

---

## AFFECTED FILE

**File**: `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`
**Total Lines**: 470
**Methods to Fix**: 2
  - `CalculateGRC_2_0` (line 55)
  - `CalculateGRC_2_5` (line 73)

---

## CURRENT CODE (BROKEN)

### Method 1: CalculateGRC_2_0 (Line 55-72)

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

### Method 2: CalculateGRC_2_5 (Line 73-110)

```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);  // ❌ NO NORMALIZATION
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python GRC 2.5 with: {Request}", json);
        
        var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.5", content);
        
        // Read response body before checking status (for error message extraction)
        var responseJson = await response.Content.ReadAsStringAsync();
        
        // If error response, try to extract structured error message
        if (!response.IsSuccessStatusCode)
        {
            // Try to parse error detail
            try
            {
                var errorDoc = JsonDocument.Parse(responseJson);
                if (errorDoc.RootElement.TryGetProperty("detail", out var detailProp))
                {
                    var detailMsg = detailProp.GetString() ?? string.Empty;
                    throw new HttpRequestException($"Python GRC 2.5 returned {(int)response.StatusCode}: {detailMsg}");
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw response
            }
            response.EnsureSuccessStatusCode(); // Will throw with status code message
        }
        
        return JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calling Python GRC 2.5 calculation");
        throw;
    }
}
```

---

## WORKING REFERENCE PATTERN (Same File, Line 128)

### CalculateARC_2_0 - THIS WORKS PERFECTLY ✅

```csharp
public async Task<PythonARCResponse?> CalculateARC_2_0(PythonARCRequest_2_0 request)
{
    try
    {
        // ✅ CORRECT: Creates normalized object before serialization
        var normalizedRequest = new
        {
            scenario = request.Scenario,
            environment = ToTitleCase(request.Environment),      // ✅ NORMALIZED
            m1_strategic = ToTitleCase(request.M1Strategic),     // ✅ NORMALIZED
            m2_effects = ToTitleCase(request.M2Effects)          // ✅ NORMALIZED
        };
        
        var json = JsonSerializer.Serialize(normalizedRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Calling Python ARC 2.0 with: {Request}", json);
        
        var response = await _httpClient.PostAsync("/api/v1/calculate/arc/2.0", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PythonARCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calling Python ARC 2.0 calculation");
        throw;
    }
}
```

---

## HELPER METHOD (Already Exists - Line 214)

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

---

## REQUEST MODEL STRUCTURES

### PythonGRCRequest_2_0 (Line 264)
```csharp
public class PythonGRCRequest_2_0
{
    [JsonPropertyName("mtom_kg")]
    public double MTOM_kg { get; set; }

    [JsonPropertyName("population_density")]
    public double PopulationDensity { get; set; }

    [JsonPropertyName("m1_strategic")]
    public string? M1Strategic { get; set; }

    [JsonPropertyName("m2_impact")]
    public string? M2Impact { get; set; }

    [JsonPropertyName("m3_erp")]
    public string? M3ERP { get; set; }
    
    [JsonPropertyName("environment_type")]
    public string? EnvironmentType { get; set; }
}
```

### PythonGRCRequest_2_5 (Similar structure with more fields)
```csharp
public class PythonGRCRequest_2_5
{
    [JsonPropertyName("mtom_kg")]
    public double MTOM_kg { get; set; }
    
    [JsonPropertyName("characteristic_dimension_m")]
    public double CharacteristicDimension_m { get; set; }
    
    [JsonPropertyName("max_speed_mps")]
    public double MaxSpeed_mps { get; set; }

    [JsonPropertyName("population_density")]
    public double PopulationDensity { get; set; }

    [JsonPropertyName("m1a_sheltering")]
    public string? M1A_Sheltering { get; set; }

    [JsonPropertyName("m1b_impact_angle")]
    public string? M1B_ImpactAngle { get; set; }

    [JsonPropertyName("m2_containment")]
    public string? M2Containment { get; set; }

    [JsonPropertyName("m3_erp")]
    public string? M3ERP { get; set; }
    
    [JsonPropertyName("environment_type")]
    public string? EnvironmentType { get; set; }
}
```

---

## REQUIRED FIXES

### Fix 1: CalculateGRC_2_0 Method

**REPLACE** lines 55-72 with:

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

### Fix 2: CalculateGRC_2_5 Method

**REPLACE** lines 73-110 with:

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
        
        var response = await _httpClient.PostAsync("/api/v1/calculate/grc/2.5", content);
        
        // Read response body before checking status (for error message extraction)
        var responseJson = await response.Content.ReadAsStringAsync();
        
        // If error response, try to extract structured error message
        if (!response.IsSuccessStatusCode)
        {
            // Try to parse error detail
            try
            {
                var errorDoc = JsonDocument.Parse(responseJson);
                if (errorDoc.RootElement.TryGetProperty("detail", out var detailProp))
                {
                    var detailMsg = detailProp.GetString() ?? string.Empty;
                    throw new HttpRequestException($"Python GRC 2.5 returned {(int)response.StatusCode}: {detailMsg}");
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw response
            }
            response.EnsureSuccessStatusCode(); // Will throw with status code message
        }
        
        return JsonSerializer.Deserialize<PythonGRCResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calling Python GRC 2.5 calculation");
        throw;
    }
}
```

---

## VALIDATION

**After applying fixes**:

1. Rebuild: `dotnet build Skyworks.sln`
2. Restart .NET backend (port 5210)
3. Open Mission Planner: http://localhost:5210/app/Pages/mission.html
4. Test SORA 2.0 calculation:
   - Drone: DJI Mini 4 Pro (249g)
   - Scenario: VLOS - Populated Area
   - Mitigations: M1=Medium, M2=High, M3=Medium
   - Expected: iGRC=4, Final GRC=1 ✅

5. Run 20 test scenarios (user requirement)

---

## TECHNICAL SUMMARY

**Issue**: .NET→Python integration missing TitleCase normalization for mitigation strings

**Solution**: Apply `ToTitleCase()` helper (already exists in same file) to ALL mitigation string properties before serialization

**Pattern**: Copy normalization approach from `CalculateARC_2_0` (line 128) which WORKS PERFECTLY

**Impact**: 
- Python API: ✅ WORKING (proven via direct test)
- Python enum fix: ✅ WORKING (case-insensitive)
- .NET serialization: ❌ BROKEN (missing normalization) ← FIX THIS
- Fix complexity: 2 methods, ~10 lines each

**Previous Validation**: You (Claude Sonnet 4) previously validated entire SORA implementation as "100% COMPLIANT" with EASA/JARUS specifications.

---

## REQUEST

Please provide the COMPLETE FIXED CODE for both methods:

1. `CalculateGRC_2_0` (with normalization like ARC method)
2. `CalculateGRC_2_5` (with normalization like ARC method)

Follow the exact pattern from `CalculateARC_2_0` (line 128) which uses:
```csharp
var normalizedRequest = new { ... fields with ToTitleCase() ... };
var json = JsonSerializer.Serialize(normalizedRequest);
```

This is a .NET 8.0 project. The `ToTitleCase()` helper is private and exists in the same class.

Thank you for your help! 🎯
"""

def send_to_sonnet():
    """Send the complete fix request to Claude Sonnet 4"""
    print("=" * 80)
    print("SKYWORKS AI SUITE - Sending 422 Fix Request to Claude Sonnet 4")
    print("=" * 80)
    print(f"Timestamp: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"API Key: {API_KEY[:20]}...{API_KEY[-10:]}")
    print(f"Model: claude-sonnet-4-20250514")
    print(f"Prompt Length: {len(PROMPT)} characters")
    print()
    
    try:
        client = anthropic.Anthropic(api_key=API_KEY)
        
        print("Sending request to Claude Sonnet 4...")
        print()
        
        message = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=4000,
            temperature=0,
            messages=[
                {
                    "role": "user",
                    "content": PROMPT
                }
            ]
        )
        
        response_text = message.content[0].text
        
        print("=" * 80)
        print("CLAUDE SONNET 4 RESPONSE")
        print("=" * 80)
        print()
        print(response_text)
        print()
        print("=" * 80)
        
        # Save response to file
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"SONNET_422_FIX_RESPONSE_{timestamp}.txt"
        
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(f"SKYWORKS AI SUITE - Claude Sonnet 4 Response\n")
            f.write(f"Timestamp: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"Request: 422 Error Fix for .NET→Python Serialization\n")
            f.write("=" * 80 + "\n\n")
            f.write(response_text)
        
        print(f"Response saved to: {filename}")
        print()
        print("✅ SUCCESS - Claude Sonnet 4 response received!")
        
        return response_text
        
    except Exception as e:
        print(f"❌ ERROR: {str(e)}")
        import traceback
        traceback.print_exc()
        return None

if __name__ == "__main__":
    result = send_to_sonnet()
    if result:
        sys.exit(0)
    else:
        sys.exit(1)
