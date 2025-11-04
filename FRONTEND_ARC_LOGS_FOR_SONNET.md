# Frontend ARC 422 Error - Complete Diagnostic

## What We Know

### Backend Test (Direct API Call) ✅ WORKS
```json
Request: {
  "airspace_class": "G",
  "altitude_agl_m": 60,
  "environment": "Urban"
}

Response: HTTP 200 OK
{
  "initial_arc": "ARC-c",
  "residual_arc": "ARC-c",
  ...
}
```

Logs show:
```
INFO:main:ARC 2.5 REQUEST RECEIVED
INFO:main:Raw frontend data: {
  "airspace_class": "G",
  "altitude_agl_m": 60,
  "environment": "Urban"
}
INFO:main:ARC 2.5 - Validation successful
INFO:main:ARC 2.5 - Success: ARC-c
INFO:     127.0.0.1:61115 - "POST /api/v1/calculate/arc/2.5 HTTP/1.1" 200 OK
```

### Frontend Call (via Mission Planner) ❌ FAILS
```
Status: ❌ BadRequest
Summary: API Validation Error
Air Risk: Proxy-only mode: Python ARC call failed - Response status code does not indicate success: 422 (Unprocessable Content).
```

**NO LOGS IN PYTHON TERMINAL** - Request never reaches Python backend!

## Root Cause Analysis

The 422 error is coming from the **.NET Backend proxy**, NOT from Python!

### Architecture
```
Frontend (mission.html)
    ↓ POST request
.NET Backend (localhost:5210)
    ↓ Proxies to Python
Python Backend (localhost:8001)
```

### The Issue
Frontend → .NET backend receives request and tries to proxy to Python, but:
1. .NET backend validates the request FIRST (before proxying)
2. .NET backend's validation schema doesn't match Python's schema
3. Request gets rejected at .NET level with 422
4. Python never sees the request (no logs!)

## Evidence
- ✅ Direct curl to Python: Works perfectly
- ❌ Frontend through .NET: Gets 422
- 🔍 Python terminal: NO LOGS when frontend calls (request never arrives)
- 🔍 .NET is returning "Proxy-only mode: Python ARC call failed"

## What Needs Fixing

**Option 1: Fix .NET Proxy Validation**
- Update .NET backend's ARCRequest DTO to match Python's schema
- Ensure .NET doesn't do validation, just proxies raw JSON

**Option 2: Fix .NET to Python Field Mapping**
- .NET might be transforming field names before proxying
- Need to check what .NET sends vs what Python expects

## Required Files to Analyze

1. **.NET Backend ARC Proxy Code**
   - Location: `Backend/src/Skyworks.Api/Controllers/*`
   - Look for: ARC endpoint proxy logic
   - Check: Field mapping, validation, transformation

2. **.NET Backend DTO Models**
   - Location: `Backend/Models/*`
   - Look for: ARCRequest models
   - Check: Field names match Python's

3. **Frontend JavaScript**
   - Location: `Frontend/Pages/*.js` or `Frontend/assets/js/*.js`
   - Look for: ARC API call construction
   - Check: What fields are being sent

## Request to Claude Sonnet

Please identify the .NET proxy issue causing 422 errors. The Python backend works perfectly with direct calls but fails when requests come through .NET proxy.

Need to either:
1. Fix .NET validation to accept Python's expected fields
2. Fix .NET field mapping to transform frontend → Python correctly
3. Make .NET a true passthrough proxy without validation

**Critical:** Python logs show NOTHING when frontend calls - proves .NET is blocking requests before they reach Python.
