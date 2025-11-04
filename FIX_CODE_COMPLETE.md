# SKYWORKS 422 FIX - COMPLETE CODE PACKAGE

## CURRENT STATE (BROKEN CODE)

### PythonCalculationClient.cs - CalculateGRC_2_0 Method (Line 55)
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_0(PythonGRCRequest_2_0 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);
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

### PythonCalculationClient.cs - CalculateGRC_2_5 Method (Line 73)
```csharp
public async Task<PythonGRCResponse?> CalculateGRC_2_5(PythonGRCRequest_2_5 request)
{
    try
    {
        var json = JsonSerializer.Serialize(request);
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
                // If can't parse, use raw response
                throw new HttpRequestException($"Python GRC 2.5 returned {(int)response.StatusCode}: {responseJson}");
            }
            
            // Generic error if no detail found
            response.EnsureSuccessStatusCode();
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

## FIXED CODE (TO IMPLEMENT)

### PythonCalculationClient.cs - CalculateGRC_2_0 Method (FIXED)
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

### PythonCalculationClient.cs - CalculateGRC_2_5 Method (FIXED)
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
                // If can't parse, use raw response
                throw new HttpRequestException($"Python GRC 2.5 returned {(int)response.StatusCode}: {responseJson}");
            }
            
            // Generic error if no detail found
            response.EnsureSuccessStatusCode();
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

## REFERENCE: WORKING PATTERN (ARC Method)

### This pattern ALREADY WORKS in the same file (Line 128)
```csharp
public async Task<PythonARCResponse?> CalculateARC_2_0(PythonARCRequest_2_0 request)
{
    try
    {
        // Create normalized request object with TitleCase mitigations
        var normalizedRequest = new
        {
            scenario = request.Scenario,
            environment = ToTitleCase(request.Environment),
            m1_strategic = ToTitleCase(request.M1Strategic),
            m2_effects = ToTitleCase(request.M2Effects)
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

## HELPER METHOD (ALREADY EXISTS - Line 214)

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

## COMPLETE FILE PATH

```
Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs
```

**Total file size**: 470 lines
**Lines to modify**: 
- Lines 55-72 (CalculateGRC_2_0)
- Lines 73-115 (CalculateGRC_2_5)

---

## VALIDATION PROOF

### Direct Python API Test - WORKS PERFECTLY ✅
```powershell
$headers = @{
    "Content-Type" = "application/json"
}
$body = @{
    mtom_kg = 32.0
    population_density = 2000
    m1_strategic = "Medium"
    m2_impact = "High"
    m3_erp = "Medium"
} | ConvertTo-Json

Invoke-RestMethod -Uri 'http://localhost:8001/api/v1/calculate/grc/2.0' -Method POST -Headers $headers -Body $body

# RESULT:
version          : SORA_2.0
initial_grc      : 6
final_grc        : 2
mitigation_total : -4
M1_strategic     : @{level=Medium; reduction=-2}
M2_impact        : @{level=High; reduction=-2}
M3_erp           : @{level=Medium; reduction=0}
calculation_steps: 6 + (-4) = 2
```

This proves Python API accepts "Medium", "High", "Low" format perfectly.

---

## IMPLEMENTATION INSTRUCTIONS

1. **Open file**: `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`

2. **Find method** `CalculateGRC_2_0` (starts at line 55)

3. **Replace lines 55-72** with the FIXED code shown above

4. **Find method** `CalculateGRC_2_5` (starts at line 73)

5. **Replace lines 73-115** with the FIXED code shown above

6. **Save file**

7. **Rebuild**:
   ```powershell
   cd Backend
   dotnet build Skyworks.sln
   ```

8. **Restart .NET Backend**:
   ```powershell
   dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210
   ```

9. **Test in Mission Planner**: http://localhost:5210/app/Pages/mission.html

---

**SIMPLE SUMMARY**: Copy the normalization pattern from `CalculateARC_2_0` (line 128) and apply it to `CalculateGRC_2_0` and `CalculateGRC_2_5`. The `ToTitleCase()` helper already exists and works perfectly.
