# SORA API Correct Usage Examples
# This script shows the correct format for both SORA 2.0 and 2.5 requests

Write-Host "SORA API Request Examples" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

# Test SORA 2.5 format
Write-Host "Testing SORA 2.5 format..." -ForegroundColor Yellow

$sora25_body = @{
    soraVersion = "2.5"
    missionId = "TEST_SORA25"
    missionName = "SORA 2.5 Test Mission"
    groundRisk = @{
        populationDensity = 50.0
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

try {
    $response25 = Invoke-WebRequest -Uri "http://localhost:5210/api/sora/complete" -Method POST -Body $sora25_body -ContentType "application/json"
    $result25 = $response25.Content | ConvertFrom-Json
    
    Write-Host "SORA 2.5 SUCCESS!" -ForegroundColor Green
    Write-Host "  iGRC: $($result25.intrinsicGRC), Final GRC: $($result25.finalGRC)" -ForegroundColor Green
    Write-Host "  ARC: $($result25.initialARC) -> $($result25.residualARC)" -ForegroundColor Green
    Write-Host "  SAIL: $($result25.sail), TMPR: $($result25.tmpr)" -ForegroundColor Green
} catch {
    Write-Host "SORA 2.5 ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Summary of Correct Format:" -ForegroundColor Cyan
Write-Host "1. Use nested objects: groundRisk{} and airRisk{}" -ForegroundColor Yellow
Write-Host "2. SORA 2.0: use scenario_V2_0 (enum values)" -ForegroundColor Yellow  
Write-Host "3. SORA 2.5: use populationDensity + isControlledGroundArea" -ForegroundColor Yellow
Write-Host "4. Always include arrays even if empty" -ForegroundColor Yellow