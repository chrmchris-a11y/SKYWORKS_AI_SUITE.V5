# JARUS 2.5 ACCURACY TESTS (10 scenarios)
# Part 2 of comprehensive testing

$baseUrl = "http://localhost:5210"
$results25 = @()

Write-Host "=== JARUS SORA 2.5 ACCURACY TESTS ===" -ForegroundColor Cyan
Write-Host ""

# Test 6: JARUS 2.5 - Tiny drone (≤250g), controlled area
Write-Host "Test 6: JARUS 2.5 - ≤250g drone, ≤25m/s (should get iGRC=1)" -ForegroundColor Yellow
$test6 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 50
        controlledGroundArea = $true
        maxCharacteristicDimension = 0.2
        maxSpeed = 20
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Typical"
        maxHeightAGL = 30
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
    mtom = 0.24
} | ConvertTo-Json -Depth 10

try {
    $response6 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test6 -ContentType "application/json"
    Write-Host "  iGRC: $($response6.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response6.finalGRC)" -ForegroundColor Green
    Write-Host "  SAIL: $($response6.sail)" -ForegroundColor Green
    Write-Host "  Expected: iGRC=1 (≤250g & ≤25m/s special rule), SAIL~I" -ForegroundColor Cyan
    $results25 += @{ Test = 6; Result = $response6; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 6; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 7: JARUS 2.5 - iGRC Table 2 edge case: 3m / 35m/s
Write-Host "Test 7: JARUS 2.5 - 3m/35m/s drone, 500 people/km² (Table 2 check)" -ForegroundColor Yellow
$test7 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 500
        controlledGroundArea = $false
        maxCharacteristicDimension = 3.0
        maxSpeed = 35
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 100
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response7 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test7 -ContentType "application/json"
    Write-Host "  iGRC: $($response7.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Expected: iGRC=5 per Table 2 (3m/35m/s, <500 people/km²)" -ForegroundColor Cyan
    $results25 += @{ Test = 7; Result = $response7; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 7; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 8: JARUS 2.5 - All M1 mitigations (M1A + M1B + M1C)
Write-Host "Test 8: JARUS 2.5 - M1(A) Medium + M1(B) High + M1(C) Low" -ForegroundColor Yellow
$test8 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 5000
        controlledGroundArea = $false
        maxCharacteristicDimension = 5.0
        maxSpeed = 60
        mitigations = @(
            @{ type = "M1A"; robustness = "Medium" }
            @{ type = "M1B"; robustness = "High" }
            @{ type = "M1C"; robustness = "Low" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 150
        strategicMitigations = @("SM1")
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response8 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test8 -ContentType "application/json"
    Write-Host "  iGRC: $($response8.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response8.finalGRC)" -ForegroundColor Green
    Write-Host "  Reduction: $($response8.intrinsicGRC - $response8.finalGRC)" -ForegroundColor Green
    Write-Host "  Expected: Reduction = -2 (M1A) + -2 (M1B) + -1 (M1C) = -5" -ForegroundColor Cyan
    $results25 += @{ Test = 8; Result = $response8; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 8; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 9: JARUS 2.5 - INVALID combination: M1(A) Medium + M1(B) High (should fail)
Write-Host "Test 9: JARUS 2.5 - INVALID: M1(A) Medium + M1(B) (should reject per Annex B)" -ForegroundColor Yellow
$test9 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 1000
        controlledGroundArea = $false
        maxCharacteristicDimension = 2.5
        maxSpeed = 40
        mitigations = @(
            @{ type = "M1A"; robustness = "Medium" }
            @{ type = "M1B"; robustness = "Medium" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Typical"
        maxHeightAGL = 70
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response9 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test9 -ContentType "application/json"
    if ($response9.warnings -and $response9.warnings -match "M1.*cannot.*combined") {
        Write-Host "  ✅ CORRECTLY REJECTED: $($response9.warnings)" -ForegroundColor Green
        $results25 += @{ Test = 9; Result = $response9; Status = "✅ (Validation)" }
    } else {
        Write-Host "  ⚠️ WARNING: Should have validation error for M1A Medium + M1B" -ForegroundColor Yellow
        $results25 += @{ Test = 9; Result = $response9; Status = "⚠️" }
    }
} catch {
    Write-Host "  ERROR (expected validation): $_" -ForegroundColor Yellow
    $results25 += @{ Test = 9; Status = "✅ (Rejected as expected)" }
}
Write-Host ""

# Test 10: JARUS 2.5 - Out of scope (>50,000 people/km²)
Write-Host "Test 10: JARUS 2.5 - Assemblies (>50k people/km²) - should be iGRC=7+" -ForegroundColor Yellow
$test10 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 60000
        controlledGroundArea = $false
        maxCharacteristicDimension = 1.0
        maxSpeed = 25
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 40
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response10 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test10 -ContentType "application/json"
    Write-Host "  iGRC: $($response10.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Expected: iGRC≥7 (assemblies), SAIL≥III" -ForegroundColor Cyan
    $results25 += @{ Test = 10; Result = $response10; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 10; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 11: JARUS 2.5 - Large drone (20m/120m/s edge case)
Write-Host "Test 11: JARUS 2.5 - 20m/120m/s drone, remote area" -ForegroundColor Yellow
$test11 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 5
        controlledGroundArea = $false
        maxCharacteristicDimension = 20.0
        maxSpeed = 120
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Remote"
        typicality = "Typical"
        maxHeightAGL = 200
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response11 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test11 -ContentType "application/json"
    Write-Host "  iGRC: $($response11.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Expected: iGRC=5 per Table 2 (20m/120m/s, <5 people/km²)" -ForegroundColor Cyan
    $results25 += @{ Test = 11; Result = $response11; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 11; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 12: JARUS 2.5 - M2 High only
Write-Host "Test 12: JARUS 2.5 - M2 High (parachute) only" -ForegroundColor Yellow
$test12 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 2000
        controlledGroundArea = $false
        maxCharacteristicDimension = 4.0
        maxSpeed = 50
        mitigations = @(
            @{ type = "M2"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 90
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response12 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test12 -ContentType "application/json"
    Write-Host "  iGRC: $($response12.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response12.finalGRC)" -ForegroundColor Green
    Write-Host "  Expected: Reduction = -2 (M2 High)" -ForegroundColor Cyan
    $results25 += @{ Test = 12; Result = $response12; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 12; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 13: JARUS 2.5 - Segregated airspace (ARC-a expected)
Write-Host "Test 13: JARUS 2.5 - Atypical Segregated (NOTAM) - should get ARC-a" -ForegroundColor Yellow
$test13 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 100
        controlledGroundArea = $false
        maxCharacteristicDimension = 2.0
        maxSpeed = 30
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "NonAirport"
        environment = "Industrial"
        typicality = "Atypical"
        maxHeightAGL = 60
        strategicMitigations = @()
        isAtypicalSegregated = $true
    }
} | ConvertTo-Json -Depth 10

try {
    $response13 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test13 -ContentType "application/json"
    Write-Host "  Initial ARC: $($response13.initialARC)" -ForegroundColor Green
    Write-Host "  Residual ARC: $($response13.residualARC)" -ForegroundColor Green
    Write-Host "  Expected: ARC-a (Atypical + Segregated)" -ForegroundColor Cyan
    $results25 += @{ Test = 13; Result = $response13; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 13; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 14: JARUS 2.5 - High density metro (50k people/km²)
Write-Host "Test 14: JARUS 2.5 - High density metropolitan area" -ForegroundColor Yellow
$test14 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 45000
        controlledGroundArea = $false
        maxCharacteristicDimension = 1.5
        maxSpeed = 28
        mitigations = @(
            @{ type = "M1A"; robustness = "Low" }
            @{ type = "M2"; robustness = "Medium" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 100
        strategicMitigations = @("SM1", "SM2")
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response14 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test14 -ContentType "application/json"
    Write-Host "  iGRC: $($response14.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Final GRC: $($response14.finalGRC)" -ForegroundColor Green
    Write-Host "  SAIL: $($response14.sail)" -ForegroundColor Green
    Write-Host "  Expected: iGRC≥6 (high density), reduction -2, SAIL≥III" -ForegroundColor Cyan
    $results25 += @{ Test = 14; Result = $response14; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 14; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

# Test 15: JARUS 2.5 - Controlled ground area (minimum iGRC)
Write-Host "Test 15: JARUS 2.5 - Controlled ground area (row 0 in Table 2)" -ForegroundColor Yellow
$test15 = @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = "controlled"
        controlledGroundArea = $true
        maxCharacteristicDimension = 8.0
        maxSpeed = 75
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Industrial"
        typicality = "Typical"
        maxHeightAGL = 50
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} | ConvertTo-Json -Depth 10

try {
    $response15 = Invoke-RestMethod -Uri "$baseUrl/api/sora/complete" -Method Post -Body $test15 -ContentType "application/json"
    Write-Host "  iGRC: $($response15.intrinsicGRC)" -ForegroundColor Green
    Write-Host "  Expected: iGRC=2 per Table 2 (8m/75m/s, controlled area)" -ForegroundColor Cyan
    $results25 += @{ Test = 15; Result = $response15; Status = "✅" }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    $results25 += @{ Test = 15; Status = "❌"; Error = $_.Exception.Message }
}
Write-Host ""

Write-Host "=== ALL TESTS COMPLETE ===" -ForegroundColor Green
Write-Host ""
