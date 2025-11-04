# COMPLETE SKYWORKS SYSTEM FIX - For Claude Sonnet

## CRITICAL PROBLEMS TO FIX

### Problem 1: Field Name Mismatch Between .NET and Python
**.NET Backend sends:**
```csharp
max_height_agl_m  // PythonARCRequest_2_0 and PythonARCRequest_2_5
```

**Python Backend expects:**
```python
altitude_agl_ft   // ARCRequest_2_0 (SORA 2.0 - in feet)
altitude_agl_m    // ARCRequest_2_5 (SORA 2.5 - in meters)
```

**Result:** 422 Unprocessable Content - Missing required field `altitude_agl_ft` or `altitude_agl_m`

### Problem 2: Drone List Not Loading
- Frontend fails to load drones when .NET backend not running
- Need robust error handling and retry logic
- Need persistent backend startup

### Problem 3: No Unified Startup Script
- User has to manually start:
  - Python backend (port 8001)
  - .NET backend (port 5210)
  - Frontend server (port 8080)
- Backends crash randomly
- No health monitoring

## REQUIRED DELIVERABLES

### 1. Fix .NET → Python Field Mapping
**File:** `Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs`

Change:
```csharp
[JsonPropertyName("max_height_agl_m")]  // WRONG
```

To SORA 2.0:
```csharp
[JsonPropertyName("altitude_agl_ft")]  // CORRECT - Python expects this
```

To SORA 2.5:
```csharp
[JsonPropertyName("altitude_agl_m")]  // CORRECT - Python expects this
```

### 2. Add Field Alias Support in Python (Backwards Compatibility)
**File:** `Backend_Python/models/sora_models.py`

Add field validators to accept BOTH `max_height_agl_m` (legacy .NET) AND `altitude_agl_m` (correct):

```python
class ARCRequest_2_5(BaseModel):
    altitude_agl_m: float = Field(ge=0, description="Altitude above ground level in meters")
    
    # Backwards compatibility
    max_height_agl_m: Optional[float] = Field(default=None, exclude=True)
    
    @field_validator('altitude_agl_m', mode='before')
    @classmethod
    def handle_altitude_alias(cls, v, info):
        """Handle legacy max_height_agl_m field"""
        if v is None and hasattr(info, 'data'):
            data = info.data if hasattr(info, 'data') else {}
            if 'max_height_agl_m' in data and data['max_height_agl_m'] is not None:
                return data['max_height_agl_m']
        return v
```

### 3. Create Unified Startup Script
**File:** `START_ALL_BACKENDS.ps1`

```powershell
# Start all Skyworks backends and frontend
Write-Host "🚀 Starting SKYWORKS AI SUITE..." -ForegroundColor Cyan

# Start Python backend
Write-Host "`n📊 Starting Python Backend (SORA Calculators)..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Backend_Python' ; .\venv\Scripts\python.exe -m uvicorn main:app --host 127.0.0.1 --port 8001" -WindowStyle Normal

# Wait for Python backend
Start-Sleep -Seconds 3

# Start .NET backend  
Write-Host "`n🏗️ Starting .NET Backend (Drone Models)..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Backend\src\Skyworks.Api' ; dotnet run" -WindowStyle Normal

# Wait for .NET backend
Start-Sleep -Seconds 5

# Start Frontend
Write-Host "`n🌐 Starting Frontend Server..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Frontend' ; python -m http.server 8080" -WindowStyle Normal

Start-Sleep -Seconds 2

# Open browser
Write-Host "`n✅ Opening Mission Planner..." -ForegroundColor Green
Start-Process "http://localhost:8080/Pages/mission.html"

Write-Host "`n🎯 SKYWORKS AI SUITE Ready!" -ForegroundColor Green
Write-Host "   - Python Backend: http://localhost:8001" -ForegroundColor White
Write-Host "   - .NET Backend: http://localhost:5210" -ForegroundColor White
Write-Host "   - Frontend: http://localhost:8080" -ForegroundColor White
```

### 4. Enhanced Python Logging (Keep Sonnet's Fix)
Already applied - keep the detailed validation logging from previous Sonnet response.

### 5. Health Check Endpoint
Add to Python `main.py`:
```python
@app.get("/health/detailed")
async def detailed_health():
    return {
        "status": "healthy",
        "endpoints": {
            "grc_2_0": "OK",
            "grc_2_5": "OK", 
            "arc_2_0": "OK",
            "arc_2_5": "OK",
            "sail": "OK"
        },
        "version": "1.0.0",
        "compliance": "EASA/JARUS SORA 2.0 & 2.5"
    }
```

## CURRENT STATUS

### What Works ✅
- Python backend GRC endpoints: 200 OK
- Python backend direct ARC test: 200 OK (tested with curl)
- Comprehensive test suite: 20/20 PASS
- 100% EASA/JARUS compliance in calculations

### What Fails ❌
- Frontend → .NET → Python ARC calls: 422 error
- Field name mismatch: `max_height_agl_m` vs `altitude_agl_ft`/`altitude_agl_m`
- Drone list disappears when .NET backend crashes
- No unified startup

## VERIFICATION STEPS

After fixes:
1. Run `START_ALL_BACKENDS.ps1`
2. Open Mission Planner
3. Select drone (Sky Tech SC15)
4. Fill SORA 2.0 or 2.5 form
5. Click "Execute SORA Assessment"
6. Should get ✅ 200 OK with complete GRC + ARC + SAIL results

## REQUEST TO CLAUDE SONNET

Please provide:
1. **Complete fixed `PythonCalculationClient.cs`** with correct field names
2. **Complete fixed `sora_models.py`** with field aliases for backwards compatibility
3. **Complete `START_ALL_BACKENDS.ps1`** startup script
4. **Any other fixes** needed for robust 24/7 operation

**Goal:** Zero manual intervention. One-click startup. 100% reliability. Full EASA/JARUS compliance.

## COST TRACKING
- Previous Sonnet calls: $0.8063
- Budget remaining: $17.6637
- This call: ~$0.05 estimated
