# URGENT: Frontend ARC 422 Validation Error - Sonnet Fix Request

## Problem Summary
Mission Planner frontend sends SORA assessment request but **ARC endpoint returns 422 Unprocessable Content** for BOTH SORA 2.0 and 2.5.

## Error Details
```
Status: ❌ BadRequest
Summary: API Validation Error
Air Risk: Proxy-only mode: Python ARC call failed - Response status code does not indicate success: 422 (Unprocessable Content).
```

## What Works
- ✅ GRC 2.0 endpoint: Returns 200 OK
- ✅ GRC 2.5 endpoint: Returns 200 OK  
- ✅ Python backend comprehensive tests: 20/20 PASS (all GRC/ARC/SAIL tests work)

## What Fails
- ❌ ARC 2.0 endpoint: Returns 422 when called from frontend
- ❌ ARC 2.5 endpoint: Returns 422 when called from frontend

## Backend Logs
```
INFO: 127.0.0.1:49202 - "POST /api/v1/calculate/grc/2.0 HTTP/1.1" 200 OK
INFO: 127.0.0.1:49202 - "POST /api/v1/calculate/arc/2.0 HTTP/1.1" 422 Unprocessable Content
INFO: 127.0.0.1:49274 - "POST /api/v1/calculate/grc/2.5 HTTP/1.1" 200 OK
INFO: 127.0.0.1:49274 - "POST /api/v1/calculate/arc/2.5 HTTP/1.1" 422 Unprocessable Content
```

## Frontend Request Data (from Mission Planner form)
```
Operation Category: SORA 2.5
Drone: Sky Tech SC15 (C3)
- MTOM: 32 kg
- Max Dimension: 1.2m  
- Max Speed: 8 m/s
- KE: 1024 J

Airspace Control: Controlled
Location Type: Non-Airport
Environment: Urban (>1,500 people/km²)
Typicality: Typical
Max Height AGL: 60m
Airspace Class: Class G (Uncontrolled)
Special Airspace Zones: Mode-S Veil / TMZ
Strategic Mitigations: SM1
Atypical Segregated Area: True
```

## Root Cause Analysis Needed
1. **Frontend → Backend payload mismatch**: Frontend likely sends wrong field names or missing required fields to ARC endpoint
2. **ARCRequest validation**: Python Pydantic models expect specific required fields (see sora_models.py ARCRequest_2_0 and ARCRequest_2_5)
3. **No detailed error logging**: FastAPI doesn't log WHAT validation failed (only returns 422)

## Required ARCRequest Fields (from sora_models.py)

### ARCRequest_2_0
```python
airspace_class: AirspaceClass = REQUIRED
altitude_agl_ft: float = REQUIRED (in feet for SORA 2.0)
environment: EnvironmentType = REQUIRED
distance_to_aerodrome_nm: Optional[float] = optional
is_in_ctr: bool = default False
is_mode_s_veil: bool = default False
is_tmz: bool = default False
is_atypical_segregated: bool = default False
strategic_mitigations: List[str] = default []
```

### ARCRequest_2_5
```python
airspace_class: AirspaceClass = REQUIRED
altitude_agl_m: float = REQUIRED (in meters for SORA 2.5)
environment: EnvironmentType = REQUIRED
distance_to_aerodrome_nm: Optional[float] = optional
is_in_ctr: bool = default False
is_mode_s_veil: bool = default False
is_tmz: bool = default False
is_atypical_segregated: bool = default False
strategic_mitigations: List[str] = default []
```

## Expected Fix
1. **Add detailed validation error logging** to main.py ARC endpoints to see WHAT field is missing
2. **Identify frontend payload structure** that's being sent
3. **Fix payload mapping** between frontend and backend
4. **Test with actual frontend request** to verify 200 OK

## CRITICAL: This Blocks Production Use
- Users cannot complete SORA assessments
- GRC works but ARC validation fails → No SAIL calculation → No OSO determination
- 100% blocking issue for Mission Planner functionality

## Request to Claude Sonnet
Please:
1. **Analyze** the ARCRequest models and identify likely missing/incorrect fields
2. **Suggest code changes** to main.py to add detailed validation error logging
3. **Recommend frontend fixes** if you can infer what's being sent wrong
4. **Provide complete fix** to resolve 422 errors and achieve 200 OK responses

**Goal:** Make ARC endpoints return 200 OK when called from Mission Planner, just like GRC endpoints do.
