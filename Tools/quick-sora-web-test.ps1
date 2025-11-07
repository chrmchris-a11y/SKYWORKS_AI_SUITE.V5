<#
.SYNOPSIS
    Quick SORA 2.0 and 2.5 Web API Test (Minimal 6 tests)
.DESCRIPTION
    Tests 3 SORA 2.0 + 3 SORA 2.5 critical scenarios
#>

param([string]$BaseUrl = "http://localhost:5210")

Write-Host "`n========== QUICK SORA WEB API TEST ==========" -ForegroundColor Cyan

# Test 1: SORA 2.0 - Basic VLOS_SparselyPopulated
Write-Host "`n[1/6] SORA 2.0 - VLOS_SparselyPopulated → SAIL II" -ForegroundColor Yellow
$body1 = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"VLOS_SparselyPopulated","maxCharacteristicDimension":1.0,"mtom_kg":0.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body1 -ContentType "application/json"
    if ($r.sail -eq "II") {
        Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green
    } else {
        Write-Host "❌ FAIL: Expected SAIL II, got $($r.sail)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: SORA 2.0 - GRC 5 + ARC-d → SAIL V (our main fix!)
Write-Host "`n[2/6] SORA 2.0 - GRC 5 + ARC-d → SAIL V" -ForegroundColor Yellow
$body2 = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"VLOS_Populated","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body2 -ContentType "application/json"
    if ($r.sail -eq "V") {
        Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green
    } else {
        Write-Host "❌ FAIL: Expected SAIL V, got $($r.sail)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: SORA 2.0 - Atypical/Segregated → ARC-a
Write-Host "`n[3/6] SORA 2.0 - Atypical/Segregated → SAIL II" -ForegroundColor Yellow
$body3 = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_SparselyPopulated","maxCharacteristicDimension":8.0,"mtom_kg":4.0,"mitigations":[]},"airRisk":{"explicitARC":"ARC_a","strategicMitigations":[],"isAtypicalSegregated":true}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body3 -ContentType "application/json"
    if ($r.sail -eq "II") {
        Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), ARC=$($r.residualAirRiskClass), SAIL=$($r.sail)" -ForegroundColor Green
    } else {
        Write-Host "❌ FAIL: Expected SAIL II, got $($r.sail)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: SORA 2.5 - Low density rural
Write-Host "`n[4/6] SORA 2.5 - Rural 35 ppl/km² → SAIL III" -ForegroundColor Yellow
$body4 = '{"soraVersion":"2.5","groundRisk":{"populationDensity":35.0,"isControlledGroundArea":false,"maxCharacteristicDimension":1.0,"mtom_kg":0.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body4 -ContentType "application/json"
    if ($r.sail -eq "III") {
        Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green
    } else {
        Write-Host "❌ FAIL: Expected SAIL III, got $($r.sail)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: SORA 2.5 - Suburban with sheltering
Write-Host "`n[5/6] SORA 2.5 - Suburban 3000 ppl/km² + Sheltering → SAIL IV" -ForegroundColor Yellow
$body5 = '{"soraVersion":"2.5","groundRisk":{"populationDensity":3000.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[{"type":"M1A","robustness":"Medium"}]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body5 -ContentType "application/json"
    if ($r.sail -eq "IV") {
        Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green
    } else {
        Write-Host "❌ FAIL: Expected SAIL IV, got $($r.sail)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: SORA 2.5 - High altitude controlled airspace
Write-Host "`n[6/6] SORA 2.5 - 250 ppl/km² + ARC-d → SAIL V" -ForegroundColor Yellow
$body6 = '{"soraVersion":"2.5","groundRisk":{"populationDensity":250.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body6 -ContentType "application/json"
    if ($r.sail -eq "V") {
        Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green
    } else {
        Write-Host "❌ FAIL: Expected SAIL V, got $($r.sail)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========== TEST COMPLETE ==========" -ForegroundColor Cyan
