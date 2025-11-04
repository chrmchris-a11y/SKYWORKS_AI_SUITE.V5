# ENUM DESERIALIZATION FIX REQUEST

## PROBLEM STATEMENT
Mission Planner UI sends JSON with string enum values but .NET backend fails to deserialize them, resulting in 400 Bad Request errors.

**Error from browser console:**
```
Failed to load resource: the server responded with a status of 400 (Bad Request)
http://localhost:5210/api/sora/complete
```

**Previous test error:**
```json
{
  "type":"https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title":"One or more validation errors occurred.",
  "status":400,
  "errors":{
    "request":["The request field is required."],
    "$.GroundRisk.Scenario_V2_0":["The JSON value could not be converted to System.Nullable`1[Skyworks.Core.Models.GRC.V2_0.OperationalScenario]. Path: $.GroundRisk.Scenario_V2_0 | LineNumber: 0 | BytePositionInLine: 57."]
  }
}
```

## ROOT CAUSE
Frontend sends enum values as strings (e.g., `"Medium"`, `"VLOS_Populated"`), but .NET backend expects numeric values by default. ASP.NET Core JSON serializer cannot deserialize string enum values without explicit configuration.

## AFFECTED ENUMS

### 1. RobustnessLevel (Skyworks.Core/Models/GRC/RobustnessLevel.cs)
**Current State:**
```csharp
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GRC;

/// <summary>
/// Mitigation Robustness Level (shared across SORA 2.0 and 2.5)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RobustnessLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}
```

**Frontend sends:** `"Robustness": "Medium"`, `"Robustness": "High"`

### 2. OperationalScenario (Skyworks.Core/Models/GRC/V2_0/SORA_2_0_Models.cs)
**Current State:**
```csharp
using Skyworks.Core.Models.GRC;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GRC.V2_0;

// ... IntrinsicGRCInput_V2_0 class ...

/// <summary>
/// SORA 2.0 operational scenarios (Table 2 rows)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationalScenario
{
    /// <summary>
    /// VLOS/BVLOS over controlled ground area → GRC 1-4
    /// </summary>
    ControlledGroundArea = 1,

    /// <summary>
    /// VLOS in sparsely populated environment → GRC 2-5
    /// </summary>
    VLOS_SparselyPopulated = 2,

    /// <summary>
    /// BVLOS in sparsely populated environment → GRC 3-6
    /// </summary>
    BVLOS_SparselyPopulated = 3,

    /// <summary>
    /// VLOS in populated environment → GRC 4-8
    /// </summary>
    VLOS_Populated = 4,

    /// <summary>
    /// BVLOS in populated environment → GRC 5-10
    /// </summary>
    BVLOS_Populated = 5,

    /// <summary>
    /// VLOS over gathering of people → GRC 7
    /// </summary>
    VLOS_GatheringOfPeople = 6,

    /// <summary>
    /// BVLOS over gathering of people → GRC 8
    /// </summary>
    BVLOS_GatheringOfPeople = 7
}
```

**Frontend sends:** `"Scenario_V2_0": "VLOS_Populated"`, `"Scenario_V2_0": "ControlledGroundArea"`

### 3. ARCRating (likely needs same fix)
**Frontend sends:** `"ExplicitARC": "ARC-b"`

## FRONTEND PAYLOAD EXAMPLE

```javascript
// From mission.html line 2250-2290
const body = {
  SoraVersion: "2.0",
  GroundRisk: {
    Scenario_V2_0: "VLOS_Populated",              // ← String enum
    MTOM_kg: 32.0,
    MaxCharacteristicDimension: 1.0,
    Mitigations: [
      { Type: "M1", Robustness: "Medium" },        // ← String enum
      { Type: "M2", Robustness: "High" }           // ← String enum
    ],
    IsControlledGroundArea: false
  },
  AirRisk: {
    ExplicitARC: "ARC-b",                          // ← String enum
    StrategicMitigations: []
  }
};
```

## ATTEMPTED FIX

Added `[JsonConverter(typeof(JsonStringEnumConverter))]` to both enums:
- ✅ `RobustnessLevel` 
- ✅ `OperationalScenario`

**Problem:** Backend still shuts down immediately after startup, suggesting runtime configuration issue.

## DIAGNOSTIC RESULTS

### Build Status
```
✅ dotnet build Skyworks.sln
  0 Warning(s)
  0 Error(s)
```

### Runtime Behavior
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5210
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\src\Skyworks.Api
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
```

Backend starts successfully but **shuts down immediately** when receiving first request.

## ADDITIONAL CONTEXT

### Program.cs Configuration (Backend/src/Skyworks.Api/Program.cs)
Need to verify if `JsonStringEnumConverter` is registered globally:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Should include:
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
```

### Related Files
- **SORAController.cs** (line 28-40): `[HttpPost("complete")]` endpoint
- **ISORAOrchestrationService.cs** (line 15-50): `SORACompleteRequest` model definition
- **MitigationInput.cs**: Uses `RobustnessLevel` enum
- **GroundRiskInput.cs**: Uses `OperationalScenario` enum

## QUESTIONS FOR CLAUDE SONNET 4

1. **Global JsonSerializerOptions:** Is per-enum `[JsonConverter]` attribute sufficient, or do we need global configuration in Program.cs?

2. **ARCRating enum:** Does this need the same fix? Format is `"ARC-b"` (with hyphen).

3. **Immediate shutdown cause:** Why does backend shut down after receiving request? Is there an unhandled exception in deserialization?

4. **Complete fix:** Provide complete corrected configuration including:
   - Program.cs `AddJsonOptions` setup
   - All enum `[JsonConverter]` attributes needed
   - Any custom converters required (e.g., for "ARC-b" format)

## SUCCESS CRITERIA

After fix applied:
1. Mission Planner UI sends calculation request
2. .NET backend successfully deserializes JSON with string enums
3. Backend returns 200 OK with SORA calculation results
4. No 400 Bad Request errors
5. Backend remains running (no immediate shutdown)

## PYTHON API STATUS

✅ **Python API working perfectly:**
- Port 8001 healthy
- Case-insensitive enum matching implemented
- Direct API tests return 200 OK
- Accepts both "medium" and "Medium"

**Issue isolated to .NET backend JSON deserialization layer.**

## VALIDATION TESTS NEEDED

```powershell
# Test 1: Complete API with string enums
POST http://localhost:5210/api/sora/complete
Body: {
  "SoraVersion": "2.0",
  "GroundRisk": {
    "Scenario_V2_0": "VLOS_Populated",
    "Mitigations": [{"Type":"M1","Robustness":"Medium"}]
  }
}
Expected: 200 OK

# Test 2: Mission Planner UI calculation
1. Open http://localhost:5210/app/Pages/mission.html
2. Select drone + scenario
3. Click "Calculate SORA"
Expected: Results display, no 400 errors
```

## PREVIOUS SUCCESSFUL FIXES

1. ✅ **Mission Planner 404 Fix:** Changed `UseStaticFiles` from `Frontend/Pages` to `Frontend/` root
2. ✅ **Python enum case-insensitive:** Added `_missing_()` method to `MitigationLevel`
3. ✅ **Claude Sonnet 4 validation:** Full SORA 2.0/2.5 compliance approved (8,260 chars)

## REQUEST TO CLAUDE SONNET 4

Please provide:
1. **Root cause diagnosis** - Why does backend shut down immediately?
2. **Complete Program.cs configuration** - Global JSON serializer options
3. **All enum fixes needed** - Which enums need `[JsonConverter]`?
4. **Custom converters** - If needed for special formats (e.g., "ARC-b")
5. **Testing strategy** - How to verify fix works end-to-end

The frontend is working correctly. Python API is working correctly. Only .NET JSON deserialization needs fixing.
