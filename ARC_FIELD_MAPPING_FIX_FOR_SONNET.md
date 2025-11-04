# CRITICAL: Frontend→Backend Field Name Mismatch for ARC Endpoints

## Problem Found!
The detailed logging from Sonnet's fix revealed the exact issue: **Frontend sends different field names than Backend expects!**

## Frontend Payload (What's Being Sent)
```json
{
  "max_height_agl_m": 60,
  "max_height_amsl_m": 500,
  "airspace_class": "G",
  "is_controlled": true,
  "is_modes_veil": false,
  "is_tmz": false,
  "environment": "Urban",
  "is_airport_heliport": false,
  "is_atypical_segregated": false,
  "tactical_mitigation_level": "None"
}
```

## Backend Expected Fields (What Backend Needs)

### SORA 2.0 (ARCRequest_2_0)
```python
airspace_class: AirspaceClass = REQUIRED
altitude_agl_ft: float = REQUIRED ← Frontend sends "max_height_agl_m" instead!
environment: EnvironmentType = REQUIRED
distance_to_aerodrome_nm: Optional[float]
is_in_ctr: bool = default False
is_mode_s_veil: bool = default False ← Frontend sends "is_modes_veil"
is_tmz: bool = default False
is_atypical_segregated: bool = default False
strategic_mitigations: List[str] = default []
```

### SORA 2.5 (ARCRequest_2_5)
```python
airspace_class: AirspaceClass = REQUIRED
altitude_agl_m: float = REQUIRED ← Frontend sends "max_height_agl_m" instead!
environment: EnvironmentType = REQUIRED
distance_to_aerodrome_km: Optional[float]
is_in_ctr: bool = default False
is_mode_s_veil: bool = default False ← Frontend sends "is_modes_veil"
is_tmz: bool = default False
is_atypical_segregated: bool = default False
strategic_mitigations: List[str] = default []
```

## Field Mapping Issues

| Frontend Field | Backend Expects | Issue |
|---|---|---|
| `max_height_agl_m` | `altitude_agl_ft` (2.0) or `altitude_agl_m` (2.5) | Different name! |
| `is_modes_veil` | `is_mode_s_veil` | Typo: "modes" vs "mode_s" |
| `is_controlled` | `is_in_ctr` | Different name! |
| `is_airport_heliport` | N/A | Not in backend schema |
| `tactical_mitigation_level` | N/A | Not in backend schema |
| `max_height_amsl_m` | N/A | Not used |
| `max_speed_ms` | N/A | Not used for ARC |

## Solution Options

### Option 1: Fix Backend to Accept Frontend Field Names (RECOMMENDED)
Add field aliases in Pydantic models to accept both naming conventions:

```python
class ARCRequest_2_0(BaseModel):
    airspace_class: AirspaceClass
    altitude_agl_ft: float = Field(alias="max_height_agl_m")  # Accept frontend name
    environment: EnvironmentType
    is_in_ctr: bool = Field(default=False, alias="is_controlled")
    is_mode_s_veil: bool = Field(default=False, alias="is_modes_veil")
    # ... rest
```

### Option 2: Add Field Mapping in main.py (QUICK FIX)
Map frontend fields to backend fields in the ARC endpoints before validation:

```python
# In calculate_arc_2_0()
if "max_height_agl_m" in raw_data:
    raw_data["altitude_agl_ft"] = raw_data["max_height_agl_m"] * 3.28084  # m to ft
if "is_controlled" in raw_data:
    raw_data["is_in_ctr"] = raw_data["is_controlled"]
if "is_modes_veil" in raw_data:
    raw_data["is_mode_s_veil"] = raw_data["is_modes_veil"]
```

## Request to Claude Sonnet
Please provide **complete fixed code** for main.py that:
1. Maps frontend field names to backend field names
2. Handles unit conversions (meters to feet for SORA 2.0)
3. Preserves all the detailed logging you added
4. Makes ARC endpoints return 200 OK

**Goal:** Accept frontend payload as-is and map it to backend schema correctly.
