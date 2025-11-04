# COMPLETE SYSTEM FIX REQUEST - NO EXPLANATIONS, JUST WORKING CODE

## CRITICAL INSTRUCTION TO CLAUDE SONNET 4

**DO NOT explain the problems. DO NOT give me analysis. JUST SEND ME THE COMPLETE WORKING CODE FILES.**

I need you to:
1. Read all the context below
2. Fix ALL issues
3. Send me COMPLETE, READY-TO-USE code files
4. Each file should be 100% complete - no placeholders, no "...existing code...", no comments saying "add this"

## CURRENT PROBLEMS

### Problem 1: Mission Planner UI - 400 Bad Request on /api/sora/complete
**Error:** Enum deserialization fails when frontend sends string values

**Files Already Modified (but may need completion):**
- `Backend/src/Skyworks.Api/Program.cs` - Added JsonStringEnumConverter
- `Backend/src/Skyworks.Core/Models/GRC/RobustnessLevel.cs` - Added [JsonConverter]
- `Backend/src/Skyworks.Core/Models/GRC/V2_0/SORA_2_0_Models.cs` - Added [JsonConverter]
- `Backend/src/Skyworks.Core/JsonConverters/ARCRatingConverter.cs` - Created custom converter
- `Backend/src/Skyworks.Core/Models/ARC/ARCModels.cs` - Updated to use ARCRatingConverter

### Problem 2: Drone List Not Loading
**API Endpoint:** Should be `/api/drones/catalog` (exists in DronesController.cs line 20)
**Frontend probably calling:** `/api/drones` (404)

### Problem 3: Backend Shuts Down Immediately After Startup
Last known behavior: Starts, then immediately shuts down when receiving requests

## CURRENT FILE STATES

### Program.cs (Backend/src/Skyworks.Api/Program.cs) - Lines 50-60
```csharp
// Services
builder.Services.AddControllers()
  .AddJsonOptions(options =>
  {
    // Επιτρέπουμε string τιμές για Enums (π.χ. "Medium") στα request/response bodies
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    
    // Additional JSON options for better compatibility
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
  });
```

### RobustnessLevel.cs
```csharp
using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.GRC;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RobustnessLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}
```

### OperationalScenario (in SORA_2_0_Models.cs)
```csharp
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationalScenario
{
    ControlledGroundArea = 1,
    VLOS_SparselyPopulated = 2,
    BVLOS_SparselyPopulated = 3,
    VLOS_Populated = 4,
    BVLOS_Populated = 5,
    VLOS_GatheringOfPeople = 6,
    BVLOS_GatheringOfPeople = 7
}
```

### ARCRatingConverter.cs (Backend/src/Skyworks.Core/JsonConverters/ARCRatingConverter.cs)
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Core.JsonConverters;

public class ARCRatingConverter : JsonConverter<ARCRating>
{
    public override ARCRating Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string for ARCRating, got {reader.TokenType}");
        }

        string value = reader.GetString()!;
        
        return value.Replace("-", "_").ToUpperInvariant() switch
        {
            "ARC_A" => ARCRating.ARC_a,
            "ARC_B" => ARCRating.ARC_b,
            "ARC_C" => ARCRating.ARC_c,
            "ARC_D" => ARCRating.ARC_d,
            _ => throw new JsonException($"Unknown ARCRating value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ARCRating value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            ARCRating.ARC_a => "ARC-a",
            ARCRating.ARC_b => "ARC-b",
            ARCRating.ARC_c => "ARC-c",
            ARCRating.ARC_d => "ARC-d",
            _ => throw new JsonException($"Unknown ARCRating value: {value}")
        };

        writer.WriteStringValue(stringValue);
    }
}
```

### ARCModels.cs - First 15 lines
```csharp
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Skyworks.Core.JsonConverters;

namespace Skyworks.Core.Models.ARC;

[JsonConverter(typeof(ARCRatingConverter))]
public enum ARCRating
{
    [EnumMember(Value = "ARC_a")]
    ARC_a = 0,
    [EnumMember(Value = "ARC_b")]
    ARC_b = 1,
    // ... etc
```

### DronesController.cs - Line 20
```csharp
[HttpGet("catalog")]
public async Task<ActionResult<List<DroneSpecification>>> GetCatalog()
```

## FRONTEND PAYLOAD EXAMPLE (from mission.html)
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

## WHAT I NEED FROM YOU

### Format Your Response EXACTLY Like This:

```
=== FILE: Backend/src/Skyworks.Api/Program.cs ===
[COMPLETE FILE CONTENTS - ALL LINES]

=== FILE: Backend/src/Skyworks.Core/Models/GRC/RobustnessLevel.cs ===
[COMPLETE FILE CONTENTS - ALL LINES]

=== FILE: Backend/src/Skyworks.Core/JsonConverters/ARCRatingConverter.cs ===
[COMPLETE FILE CONTENTS - ALL LINES]

... etc for ALL files that need fixes
```

### Requirements:
1. ✅ Fix ALL enum deserialization issues
2. ✅ Ensure backend doesn't shut down on requests
3. ✅ Fix any missing using statements
4. ✅ Make sure all JsonConverters are registered properly
5. ✅ Include error handling to prevent crashes
6. ✅ Send COMPLETE files - not snippets, not "add this here"

### DO NOT:
❌ Give me explanations
❌ Tell me what to do
❌ Send partial code with "...existing code..."
❌ Ask me questions
❌ Give me multiple options

### JUST:
✅ Send me ready-to-paste COMPLETE file contents
✅ Fix everything
✅ Make it work

**I will copy-paste your exact output into the files. Make sure it's 100% ready.**
