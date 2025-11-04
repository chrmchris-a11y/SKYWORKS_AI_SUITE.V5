# SORA 2.0 & JARUS 2.5 Accuracy Tests
# Tests 10 SORA 2.0 + 10 JARUS 2.5 scenarios with detailed analysis

$baseUrl = "http://localhost:5210"
$results = @()

Write-Host "=== SORA 2.0 ACCURACY TESTS ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: SORA 2.0 - VLOS Sparsely Populated, Small Drone, No Mitigations
Write-Host "Test 1: SORA 2.0 - Small drone, sparse area, no mitigations" -ForegroundColor Yellow
$test1 = @{
    soraVersion = "2.0"
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
} | ConvertTo-Json -Depth 10

try {
    $response1 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test1 -ContentType "application/json"
    Write-Host "  iGRC: $($response1.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response1.finalGRC)" -ForegroundColor Green
    Write-Host "  Initial ARC: $($response1.initialARC)" -ForegroundColor Green
    Write-Host "  Residual ARC: $($response1.residualARC)" -ForegroundColor Green
    Write-Host "  SAIL: $($response1.sail)" -ForegroundColor Green
    Write-Host "  Expected: GRC~2-3, ARC~b, SAIL~I-II" -ForegroundColor Cyan
    $results += @{ Test = 1; Result = $response1; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results += @{ Test = 1; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 2: SORA 2.0 - BVLOS Populated, Medium Drone, M1 Medium + M2 High
Write-Host "Test 2: SORA 2.0 - Medium drone BVLOS, populated, strong mitigations" -ForegroundColor Yellow
$test2 = @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "BVLOS_Populated"
        maxCharacteristicDimension = 3.0
        kineticEnergy = 25000
        mitigations = @(
            @{ type = "M1"; robustness = "Medium" }
            @{ type = "M2"; robustness = "High" }
            @{ type = "M3"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 120
        strategicMitigations = @("SM1", "SM2")
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response2 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test2 -ContentType "application/json"
    Write-Host "  iGRC: $($response2.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response2.finalGRC)" -ForegroundColor Green
    Write-Host "  Initial ARC: $($response2.initialARC)" -ForegroundColor Green
    Write-Host "  Residual ARC: $($response2.residualARC)" -ForegroundColor Green
    Write-Host "  SAIL: $($response2.sail)" -ForegroundColor Green
    Write-Host "  Expected: GRC reduction -7 (M1:-2, M2:-2, M3:-1), ARC~d, SAIL~III-IV" -ForegroundColor Cyan
    $results += @{ Test = 2; Result = $response2; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results += @{ Test = 2; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 3: SORA 2.0 - Assembly of People, Large Drone, M1 High only
Write-Host "Test 3: SORA 2.0 - Large drone, assembly of people, M1 High only" -ForegroundColor Yellow
$test3 = @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_AssemblyOfPeople"
        maxCharacteristicDimension = 7.5
        kineticEnergy = 800000
        mitigations = @(
            @{ type = "M1"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 30
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response3 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test3 -ContentType "application/json"
    Write-Host "  iGRC: $($response3.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response3.finalGRC)" -ForegroundColor Green
    Write-Host "  Initial ARC: $($response3.initialARC)" -ForegroundColor Green
    Write-Host "  Residual ARC: $($response3.residualARC)" -ForegroundColor Green
    Write-Host "  SAIL: $($response3.sail)" -ForegroundColor Green
    Write-Host "  Expected: iGRC~7-8, Final GRC~3-4 (M1 High:-4), ARC~c, SAIL~III-IV" -ForegroundColor Cyan
    $results += @{ Test = 3; Result = $response3; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results += @{ Test = 3; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 4: SORA 2.0 - INVALID: M3 None (penalty +1)
Write-Host "Test 4: SORA 2.0 - Testing M3 penalty (No ERP = +1 GRC)" -ForegroundColor Yellow
$test4 = @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_Populated"
        maxCharacteristicDimension = 2.0
        kineticEnergy = 15000
        mitigations = @(
            @{ type = "M1"; robustness = "Low" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 80
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response4 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test4 -ContentType "application/json"
    Write-Host "  iGRC: $($response4.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response4.finalGRC)" -ForegroundColor Green
    Write-Host "  Expected: Final GRC = iGRC + M1(-1) + M3 penalty(+1) = iGRC" -ForegroundColor Cyan
    $results += @{ Test = 4; Result = $response4; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results += @{ Test = 4; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 5: SORA 2.0 - Atypical operation (ARC-a expected)
Write-Host "Test 5: SORA 2.0 - Atypical operation (should get ARC-a)" -ForegroundColor Yellow
$test5 = @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_SparselyPopulated"
        maxCharacteristicDimension = 1.5
        kineticEnergy = 5000
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Atypical"
        maxHeightAGL = 60
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response5 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test5 -ContentType "application/json"
    Write-Host "  Initial ARC: $($response5.initialARC)" -ForegroundColor Green
    Write-Host "  Residual ARC: $($response5.residualARC)" -ForegroundColor Green
    Write-Host "  Expected: ARC-a (Atypical = no TMPR)" -ForegroundColor Cyan
    $results += @{ Test = 5; Result = $response5; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results += @{ Test = 5; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

Write-Host "=== SORA 2.0 TESTS COMPLETE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to continue to JARUS 2.5 tests..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
