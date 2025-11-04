# Test corrected SORA request format

Write-Host "Testing corrected SORA 2.0 request format..." -ForegroundColor Yellow
Write-Host ""

# Corrected SORA 2.0 request format
$body = @{
    soraVersion = "2.0"
    missionId = "TEST_001"
    missionName = "Test Mission"
    groundRisk = @{
        scenario_V2_0 = "VLOS_SparselyPopulated"
        maxCharacteristicDimension = 0.5
        kineticEnergy = 300
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

Write-Host "Request body:" -ForegroundColor Cyan
Write-Host $body
Write-Host ""

try {
    Write-Host "Sending request..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "http://localhost:5210/api/sora/complete" -Method POST -Body $body -ContentType "application/json"
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "SUCCESS! Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Results:" -ForegroundColor Green
    Write-Host "  Intrinsic GRC: $($result.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($result.finalGRC)" -ForegroundColor Green  
    Write-Host "  Initial ARC: $($result.initialARC)" -ForegroundColor Green
    Write-Host "  Residual ARC: $($result.residualARC)" -ForegroundColor Green
    Write-Host "  SAIL: $($result.sail)" -ForegroundColor Green
    Write-Host "  TMPR: $($result.tmpr)" -ForegroundColor Green
    Write-Host "  Is Compliant: $($result.isCompliant)" -ForegroundColor Green
    
    if ($result.errors -and $result.errors.Count -gt 0) {
        Write-Host ""
        Write-Host "Errors:" -ForegroundColor Red
        foreach ($error in $result.errors) {
            Write-Host "  - $error" -ForegroundColor Red
        }
    }
    
    if ($result.warnings -and $result.warnings.Count -gt 0) {
        Write-Host ""
        Write-Host "Warnings:" -ForegroundColor Yellow
        foreach ($warning in $result.warnings) {
            Write-Host "  - $warning" -ForegroundColor Yellow
        }
    }
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = [System.IO.StreamReader]::new($errorStream)
            $errorContent = $reader.ReadToEnd()
            Write-Host "Error details: $errorContent" -ForegroundColor Red
        } catch {
            Write-Host "Could not read error details" -ForegroundColor Red
        }
    }
}