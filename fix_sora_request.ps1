# SORA API Request Fix - Correct Format Examples
# The issue is that your request used a flat structure instead of nested objects

Write-Host "🔧 SORA API Request Format Fix" -ForegroundColor Cyan
Write-Host ""

# ❌ WRONG FORMAT (what you used):
Write-Host "❌ INCORRECT Format (flat structure):" -ForegroundColor Red
$wrongFormat = @{
    sora_version = "2.0"
    scenario = "VLOS_Sparsely"
    dimension = 0.5
    controlled_ground = $false
    arc = "a"
} | ConvertTo-Json
Write-Host $wrongFormat
Write-Host ""

# ✅ CORRECT FORMAT (nested structure):
Write-Host "✅ CORRECT Format (nested structure):" -ForegroundColor Green

# For SORA 2.0 requests, use this format:
$correctFormat_SORA20 = @{
    soraVersion = "2.0"
    missionId = "TEST_001"
    missionName = "Test Mission"
    groundRisk = @{
        scenario_V2_0 = "VLOS_SparselyPopulated"  # Note: full enum name
        maxCharacteristicDimension = 0.5
        kineticEnergy = 300
        mitigations = @()  # Empty array for no mitigations
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport" 
        environment = "Rural"
        typicality = "Typical"
        maxHeightAGL = 50
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
    implementedOSOs = @()  # Empty array
} | ConvertTo-Json -Depth 10

Write-Host $correctFormat_SORA20
Write-Host ""

# For SORA 2.5 requests:
Write-Host "✅ SORA 2.5 Format:" -ForegroundColor Green
$correctFormat_SORA25 = @{
    soraVersion = "2.5"
    missionId = "TEST_002"
    missionName = "Test Mission 2.5"
    groundRisk = @{
        populationDensity = 50.0  # people per km²
        isControlledGroundArea = $false
        maxCharacteristicDimension = 0.5
        maxSpeed = 15.0
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural" 
        typicality = "Typical"
        maxHeightAGL = 50
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
    implementedOSOs = @()
} | ConvertTo-Json -Depth 10

Write-Host $correctFormat_SORA25
Write-Host ""

# Test the corrected SORA 2.0 request:
Write-Host "🧪 Testing corrected SORA 2.0 request..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5210/api/sora/complete" -Method POST -Body $correctFormat_SORA20 -ContentType "application/json"
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ SUCCESS!" -ForegroundColor Green
    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "iGRC: $($result.intrinsicGRC)" -ForegroundColor Green
    Write-Host "Final GRC: $($result.finalGRC)" -ForegroundColor Green  
    Write-Host "Initial ARC: $($result.initialARC)" -ForegroundColor Green
    Write-Host "Residual ARC: $($result.residualARC)" -ForegroundColor Green
    Write-Host "SAIL: $($result.sail)" -ForegroundColor Green
    Write-Host "TMPR: $($result.tmpr)" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorBody = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($errorBody)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error details: $errorContent" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📝 Key Differences:" -ForegroundColor Cyan
Write-Host "1. Use 'soraVersion' not 'sora_version'" -ForegroundColor Yellow
Write-Host "2. Nest parameters under 'groundRisk' and 'airRisk' objects" -ForegroundColor Yellow  
Write-Host "3. Use full enum names: 'VLOS_SparselyPopulated' not 'VLOS_Sparsely'" -ForegroundColor Yellow
Write-Host "4. Use 'maxCharacteristicDimension' not 'dimension'" -ForegroundColor Yellow
Write-Host "5. Use 'isControlledGroundArea' not 'controlled_ground'" -ForegroundColor Yellow
Write-Host "6. Include required arrays even if empty: mitigations, implementedOSOs" -ForegroundColor Yellow