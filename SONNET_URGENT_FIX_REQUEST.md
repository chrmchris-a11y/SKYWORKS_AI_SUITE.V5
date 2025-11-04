# URGENT FIX REQUEST - SKYWORKS MISSION PLANNER

## CLAUDE SONNET 4: READ THIS CAREFULLY

The Mission Planner web page has **TWO CRITICAL BUGS**:

1. ❌ **Drone list NOT loading** - Dropdown empty
2. ❌ **"Failed to fetch" error** when clicking "Execute SAIL Calculation"

User screenshot shows:
```
Status: ❌ Error
Summary: 🌐 Network Connection Failed
  • Failed to fetch
  • Verify backend is running: http://localhost:5210
  • Check network connectivity
  • Review browser console for details
```

## CURRENT STATE

### Services Status
✅ .NET Backend running on port 5210
✅ Python FastAPI running on port 8001
✅ Mission Planner UI loads at `http://localhost:5210/app/Pages/mission.html`

### What Works
- ✅ Page loads successfully
- ✅ UI renders properly
- ✅ Static file serving fixed

### What FAILS
- ❌ Drone list doesn't populate
- ❌ Calculate button gives "Failed to fetch"
- ❌ Both SORA 2.0 and 2.5 calculations fail

## BUG #1: DRONE LIST NOT LOADING

### Frontend Code (mission.html line 1360)
```javascript
const response = await fetch('http://localhost:5210/api/drones/dropdown');
```

### Backend Controller (DronesController.cs line 20)
```csharp
[HttpGet("catalog")]  // ← WRONG ENDPOINT!
public async Task<ActionResult<List<DroneSpecification>>> GetCatalog()
```

**PROBLEM:** Frontend calls `/api/drones/dropdown` but backend has `/api/drones/catalog`

### Current DronesController.cs Structure
```csharp
[ApiController]
[Route("api/drones")]
public class DronesController : ControllerBase
{
    [HttpGet("catalog")]  // Line 20 - responds to /api/drones/catalog
    public async Task<ActionResult<List<DroneSpecification>>> GetCatalog()
    
    [HttpGet("{droneId}")]  // Line 35 - responds to /api/drones/{id}
    public async Task<ActionResult<DroneSpecification>> GetDrone(string droneId)
    
    [HttpPost("search")]  // Line 48 - responds to /api/drones/search
    public async Task<ActionResult<List<DroneSpecification>>> SearchDrones([FromBody] DroneCatalogFilter filter)
```

**REQUIRED FIX:**
Add missing endpoint `/api/drones/dropdown` that returns format expected by frontend:
```json
{
  "drones": [
    {
      "id": "DJI_MINI4PRO",
      "displayName": "DJI Mini 4 Pro",
      "cClass": "C0",
      "mTOM_kg": 0.249,
      "characteristicDimension_m": 0.245,
      "maxSpeed_mps": 16
    },
    // ... more drones
  ]
}
```

## BUG #2: SORA CALCULATION "Failed to fetch"

### Frontend Code (mission.html line 2297)
```javascript
const resp = await fetch('http://localhost:5210/api/sora/complete', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body)
});
```

### Payload Example
```javascript
{
  SoraVersion: "2.0",
  GroundRisk: {
    Scenario_V2_0: "VLOS_Populated",
    MTOM_kg: 32.0,
    MaxCharacteristicDimension: 1.0,
    Mitigations: [
      { Type: "M1", Robustness: "Medium" },
      { Type: "M2", Robustness: "High" }
    ],
    IsControlledGroundArea: false
  },
  AirRisk: {
    ExplicitARC: "ARC-b",
    StrategicMitigations: []
  }
}
```

### Backend Controller (SORAController.cs line 28)
```csharp
[HttpPost("complete")]
public ActionResult<SORACompleteResult> ExecuteComplete([FromBody] SORACompleteRequest request)
{
    if (request == null)
        return BadRequest(new { error = _localizer["RequestBodyRequired"].Value });

    if (request.SoraVersion != "2.0" && request.SoraVersion != "2.5")
        return BadRequest(new { error = _localizer["InvalidSoraVersion"].Value });

    var result = _orchestration.ExecuteComplete(request);

    if (!result.IsSuccessful)
        return BadRequest(result);

    return Ok(result);
}
```

**PROBLEM:** Likely deserialization failure due to enum issues OR missing service registration

## ENUM FIXES ALREADY APPLIED

### ✅ RobustnessLevel.cs
```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RobustnessLevel { None = 0, Low = 1, Medium = 2, High = 3 }
```

### ✅ OperationalScenario (SORA_2_0_Models.cs)
```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationalScenario { ControlledGroundArea = 1, VLOS_Populated = 4, ... }
```

### ✅ ARCRating (ARCModels.cs)
```csharp
[JsonConverter(typeof(ARCRatingConverter))]
public enum ARCRating { ARC_a = 0, ARC_b = 1, ARC_c = 2, ARC_d = 3 }
```

### ✅ Program.cs (lines 50-60)
```csharp
builder.Services.AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
  });
```

## WHAT I NEED FROM YOU

### Format: COMPLETE FILE CONTENTS ONLY

Send me **EXACTLY** this format:

```
=== FILE: Backend/src/Skyworks.Api/Controllers/DronesController.cs ===
[COMPLETE FILE - ALL LINES FROM START TO END]

=== FILE: Backend/src/Skyworks.Api/Controllers/SORAController.cs ===
[COMPLETE FILE - ALL LINES FROM START TO END]

... any other files that need changes
```

### Specific Requirements

#### For DronesController.cs:
1. Add new endpoint: `[HttpGet("dropdown")]`
2. Return format: `{ "drones": [ ... ] }`
3. Include fields: `id`, `displayName`, `cClass`, `mTOM_kg`, `characteristicDimension_m`, `maxSpeed_mps`
4. Keep ALL existing endpoints intact

#### For SORAController.cs:
1. Ensure proper error handling (try-catch)
2. Log all exceptions
3. Return proper error responses
4. Make sure enum deserialization works

#### For any Service files:
1. Make sure `ISORAOrchestrationService` is properly registered in Program.cs
2. Ensure all dependencies are injected
3. Handle null checks

### DO NOT:
❌ Send explanations
❌ Send partial code
❌ Use "...existing code..." placeholders
❌ Give me multiple options
❌ Ask me questions

### JUST:
✅ Send complete, working files
✅ Fix both bugs
✅ Make it production-ready

## DIAGNOSTIC COMMANDS RUN

```powershell
# Test drones endpoint
Invoke-RestMethod -Uri 'http://localhost:5210/api/drones'
# Result: 404 Not Found

# Backend is running
Get-Process | Where { $_.ProcessName -like "*dotnet*" }
# Result: Process exists, backend is UP

# Test wrong endpoint
Invoke-RestMethod -Uri 'http://localhost:5210/api/drones/catalog'
# Expected: Should work if endpoint exists
```

## USER CONTEXT

User says (Greek):
> "δεν έχει λίστα drones, δώσε full detailed οδηγίες στο sonnet 4, με αρχεία που πρέπει να διορθωθούν, ποια είναι τα errors, δεν φορτώνεται λίστα drones και μόλις πατήσω execute sail calculation βγάζει και για sora 2 και sora 2.5"

Translation:
> "No drone list, give full detailed instructions to sonnet 4, with files that need to be fixed, what are the errors, drone list doesn't load and when I press execute sail calculation it fails for both sora 2 and sora 2.5"

**USER NEEDS WORKING SYSTEM NOW. NO THEORY. JUST CODE.**
