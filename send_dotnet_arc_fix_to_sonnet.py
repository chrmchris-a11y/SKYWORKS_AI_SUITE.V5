#!/usr/bin/env python3
"""
Send .NET→Python ARC field mapping issue to Claude Sonnet for fix
"""
import anthropic
import os

api_key = os.getenv("ANTHROPIC_API_KEY")
if not api_key:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

client = anthropic.Anthropic(api_key=api_key)

problem = """# CRITICAL: .NET→Python ARC Field Mapping Mismatch

## ROOT CAUSE IDENTIFIED

**Problem:** .NET backend sends `max_height_agl_m` but Python expects DIFFERENT field names for SORA 2.0 vs 2.5!

### .NET Backend Sends (PythonARCRequest_2_0 and PythonARCRequest_2_5)
```csharp
[JsonPropertyName("max_height_agl_m")]
public double MaxHeightAglM { get; set; }

[JsonPropertyName("airspace_class")]
public string AirspaceClass { get; set; } = "G";

[JsonPropertyName("environment")]
public string Environment { get; set; } = "Rural";

// Plus other fields:
is_controlled, is_modes_veil, is_tmz, is_airport_heliport, 
is_atypical_segregated, tactical_mitigation_level
```

### Python Backend Expects

**SORA 2.0 (ARCRequest_2_0):**
```python
airspace_class: AirspaceClass = REQUIRED  # ✅ MATCH
altitude_agl_ft: float = REQUIRED (IN FEET!)  # ❌ .NET sends max_height_agl_m
environment: EnvironmentType = REQUIRED  # ✅ MATCH
distance_to_aerodrome_nm: Optional[float]  # ❌ .NET sends is_airport_heliport (bool)
is_in_ctr: bool = False  # ❌ .NET sends is_controlled
is_mode_s_veil: bool = False  # ❌ .NET sends is_modes_veil
is_tmz: bool = False  # ✅ MATCH
is_atypical_segregated: bool = False  # ✅ MATCH
strategic_mitigations: List[str] = []  # ❌ .NET sends tactical_mitigation_level (string)
```

**SORA 2.5 (ARCRequest_2_5):**
```python
airspace_class: AirspaceClass = REQUIRED  # ✅ MATCH
altitude_agl_m: float = REQUIRED (IN METERS!)  # ❌ .NET sends max_height_agl_m
environment: EnvironmentType = REQUIRED  # ✅ MATCH
distance_to_aerodrome_km: Optional[float]  # ❌ .NET sends is_airport_heliport (bool)
is_in_ctr: bool = False  # ❌ .NET sends is_controlled
is_mode_s_veil: bool = False  # ❌ .NET sends is_modes_veil
is_tmz: bool = False  # ✅ MATCH
is_atypical_segregated: bool = False  # ✅ MATCH
strategic_mitigations: List[str] = []  # ❌ .NET sends tactical_mitigation_level (string)
```

## Field Mapping Issues

| .NET Field | Python SORA 2.0 | Python SORA 2.5 | Status |
|------------|-----------------|-----------------|--------|
| `max_height_agl_m` | `altitude_agl_ft` (feet) | `altitude_agl_m` (meters) | ❌ WRONG NAME |
| `is_controlled` | `is_in_ctr` | `is_in_ctr` | ❌ WRONG NAME |
| `is_modes_veil` | `is_mode_s_veil` | `is_mode_s_veil` | ❌ TYPO (modes vs mode_s) |
| `is_airport_heliport` (bool) | `distance_to_aerodrome_nm` (float) | `distance_to_aerodrome_km` (float) | ❌ TYPE MISMATCH |
| `tactical_mitigation_level` (string) | `strategic_mitigations` (List[str]) | `strategic_mitigations` (List[str]) | ❌ TYPE MISMATCH |

## Required Fixes

### Option 1: Fix .NET DTOs (RECOMMENDED)
Update `PythonARCRequest_2_0` and `PythonARCRequest_2_5` in .NET to match Python's exact schema:

```csharp
public class PythonARCRequest_2_0
{
    [JsonPropertyName("altitude_agl_ft")]  // Changed from max_height_agl_m
    public double AltitudeAglFt { get; set; }
    
    [JsonPropertyName("airspace_class")]
    public string AirspaceClass { get; set; } = "G";
    
    [JsonPropertyName("environment")]
    public string Environment { get; set; } = "Rural";
    
    [JsonPropertyName("is_in_ctr")]  // Changed from is_controlled
    public bool IsInCtr { get; set; }
    
    [JsonPropertyName("is_mode_s_veil")]  // Changed from is_modes_veil
    public bool IsModeS Veil { get; set; }
    
    [JsonPropertyName("is_tmz")]
    public bool IsTmz { get; set; }
    
    [JsonPropertyName("is_atypical_segregated")]
    public bool IsAtypicalSegregated { get; set; }
    
    [JsonPropertyName("distance_to_aerodrome_nm")]  // Changed from is_airport_heliport
    public double? DistanceToAerodromeNm { get; set; }
    
    [JsonPropertyName("strategic_mitigations")]  // Changed from tactical_mitigation_level
    public List<string> StrategicMitigations { get; set; } = new();
}

public class PythonARCRequest_2_5
{
    [JsonPropertyName("altitude_agl_m")]  // Changed from max_height_agl_m
    public double AltitudeAglM { get; set; }
    
    [JsonPropertyName("airspace_class")]
    public string AirspaceClass { get; set; } = "G";
    
    [JsonPropertyName("environment")]
    public string Environment { get; set; } = "Rural";
    
    [JsonPropertyName("is_in_ctr")]  // Changed from is_controlled
    public bool IsInCtr { get; set; }
    
    [JsonPropertyName("is_mode_s_veil")]  // Changed from is_modes_veil
    public bool IsModeS Veil { get; set; }
    
    [JsonPropertyName("is_tmz")]
    public bool IsTmz { get; set; }
    
    [JsonPropertyName("is_atypical_segregated")]
    public bool IsAtypicalSegregated { get; set; }
    
    [JsonPropertyName("distance_to_aerodrome_km")]  // Changed from is_airport_heliport
    public double? DistanceToAerodromeKm { get; set; }
    
    [JsonPropertyName("strategic_mitigations")]  // Changed from tactical_mitigation_level
    public List<string> StrategicMitigations { get; set; } = new();
}
```

### Option 2: Add Field Aliases in Python (FALLBACK)
Add backwards compatibility in Python Pydantic models to accept .NET's field names.

## Request to Claude Sonnet

Please provide:
1. **Complete fixed C# code** for `PythonARCRequest_2_0` and `PythonARCRequest_2_5` classes
2. **Location** to apply fix: `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs` lines 358-430
3. **Any additional changes** needed in the proxy controller or elsewhere

**Goal:** Make .NET→Python ARC requests work by aligning field names and types exactly with Python's schema.
"""

print("🚀 Sending ARC field mapping issue to Claude Sonnet 4...")
print(f"📝 Prompt length: {len(problem)} chars")

try:
    message = client.messages.create(
        model="claude-sonnet-4-20250514",
        max_tokens=3000,
        messages=[{"role": "user", "content": problem}]
    )
    
    response_text = message.content[0].text
    
    # Calculate cost
    input_tokens = message.usage.input_tokens
    output_tokens = message.usage.output_tokens
    input_cost = (input_tokens / 1_000_000) * 3.0
    output_cost = (output_tokens / 1_000_000) * 15.0
    total_cost = input_cost + output_cost
    
    print(f"\n✅ Sonnet 4 responded!")
    print(f"📊 Tokens:")
    print(f"   - Input: {input_tokens:,} (${input_cost:.4f})")
    print(f"   - Output: {output_tokens:,} (${output_cost:.4f})")
    print(f"   - THIS: ${total_cost:.4f}")
    print(f"   - TOTAL: ${0.8063 + total_cost:.4f}")
    print(f"   - Remaining: ${18.47 - (0.8063 + total_cost):.4f}")
    
    # Save response
    with open("SONNET_DOTNET_ARC_FIX.txt", "w", encoding="utf-8") as f:
        f.write(response_text)
    
    print(f"\n💾 Saved to SONNET_DOTNET_ARC_FIX.txt")
    print(f"\n" + "="*80)
    print(response_text)
    print("="*80)

except Exception as e:
    print(f"\n❌ Error: {e}")
    raise
