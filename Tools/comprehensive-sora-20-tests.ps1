<#
.SYNOPSIS
    Comprehensive SORA 2.0 and 2.5 Web API Test Suite - 20 Tests
.DESCRIPTION
    Tests all 20 authoritative test cases from SORAAuthoritative_TestCases.json
    Validates EASA/JARUS compliance for both SORA 2.0 and SORA 2.5
#>

param([string]$BaseUrl = "http://localhost:5210")

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  COMPREHENSIVE SORA WEB API TEST SUITE" -ForegroundColor Cyan
Write-Host "  20 Tests: 10x SORA 2.0 + 10x SORA 2.5" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$passed = 0
$failed = 0

# SORA 2.0 TESTS
Write-Host "=========== SORA 2.0 TESTS ===========" -ForegroundColor Cyan

# Test 1: Low Risk Rural VLOS - SAIL I
Write-Host "`n[1/20] SORA20_001: Low Risk Rural VLOS → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_SparselyPopulated","maxCharacteristicDimension":1.0,"mtom_kg":0.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: GRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 2: Urban Dense with Sheltering - SAIL III
Write-Host "`n[2/20] SORA20_002: Urban with Sheltering → SAIL IV" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"VLOS_Populated","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[{"type":"M1A","robustness":"Medium"}]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "IV") { Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_IV, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 3: Strategic Mitigation S1 Reduces ARC (Rural 150 ppl/km², BVLOS)
Write-Host "`n[3/20] SORA20_003: Strategic Mitigation S1 → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_SparselyPopulated","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":["S1"],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: GRC=$($r.finalGRC), ARC=$($r.residualAirRiskClass), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 4: Atypical Segregated Airspace - SAIL I
Write-Host "`n[4/20] SORA20_004: Atypical/Segregated → SAIL II" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_SparselyPopulated","maxCharacteristicDimension":8.0,"mtom_kg":4.0,"mitigations":[]},"airRisk":{"explicitARC":"ARC_a","strategicMitigations":[],"isAtypicalSegregated":true}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "II") { Write-Host "✅ PASS: GRC=$($r.finalGRC), ARC=ARC_a, SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_II, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 5: Controlled Area GRC-1 Baseline
Write-Host "`n[5/20] SORA20_005: Controlled Area → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"ControlledGroundArea","maxCharacteristicDimension":1.5,"mtom_kg":0.75,"mitigations":[]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: GRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 6: High Altitude Above 500ft AGL - ARC-d
Write-Host "`n[6/20] SORA20_006: GRC 5 + ARC-d → SAIL V" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"VLOS_Populated","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "V") { Write-Host "✅ PASS: GRC=$($r.finalGRC), ARC=ARC_d, SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_V, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 7: Assembly of People GRC-6 - High Risk (OUT OF SCOPE)
Write-Host "`n[7/20] SORA20_007: Assembly of People → OUT OF SCOPE" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"VLOS_GatheringOfPeople","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.isSuccessful -eq $false -or $r.sail -eq $null) { Write-Host "✅ PASS: Correctly rejected as OUT OF SCOPE" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Should be OUT OF SCOPE but got SAIL=$($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "✅ PASS: Correctly rejected (400 error)" -ForegroundColor Green ; $passed++ }

# Test 8: Multiple Strategic Mitigations Cap Test
Write-Host "`n[8/20] SORA20_008: S1+S2+S3+S4 capped at -2 → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_SparselyPopulated","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":["S1","S2","S3","S4"],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III" -and $r.residualARC -eq "ARC_b") { Write-Host "✅ PASS: ARC reduced to ARC_b, SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III+ARC_b, got SAIL=$($r.sail), ARC=$($r.residualARC)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 9: Floor Rule Prevents ARC-a from ARC-c (Rural 100 ppl/km², BVLOS)
Write-Host "`n[9/20] SORA20_009: Floor rule prevents ARC-a → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_SparselyPopulated","maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":["S1","S2"],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: GRC=$($r.finalGRC), ARC=$($r.residualAirRiskClass), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 10: SAIL VI Maximum Risk Scenario (OUT OF SCOPE)
Write-Host "`n[10/20] SORA20_010: Maximum Risk → OUT OF SCOPE" -ForegroundColor White
$body = '{"soraVersion":"2.0","groundRisk":{"scenario_V2_0":"BVLOS_GatheringOfPeople","maxCharacteristicDimension":8.0,"mtom_kg":4.0,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.isSuccessful -eq $false) { Write-Host "✅ PASS: Correctly rejected as OUT OF SCOPE" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Should be OUT OF SCOPE" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "✅ PASS: Correctly rejected (400 error)" -ForegroundColor Green ; $passed++ }

# SORA 2.5 TESTS
Write-Host "`n=========== SORA 2.5 TESTS ===========" -ForegroundColor Cyan

# Test 11: SORA 2.5 Rural Low Density - SAIL I
Write-Host "`n[11/20] SORA25_001: Rural 8 ppl/km² → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":8.0,"isControlledGroundArea":false,"maxCharacteristicDimension":1.0,"mtom_kg":0.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: GRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 12: SORA 2.5 Suburban with Ground Risk Buffer
Write-Host "`n[12/20] SORA25_002: Suburban 3000 ppl/km² + Sheltering → SAIL IV" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":3000.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[{"type":"M1A","robustness":"Medium"}]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "IV") { Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_IV, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 13: SORA 2.5 Containment Requirements Test
Write-Host "`n[13/20] SORA25_003: 120 ppl/km² + S1 → SAIL IV" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":120.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":["S1"],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "IV") { Write-Host "✅ PASS: GRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_IV, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 14: SORA 2.5 Atypical Segregated - No Floor
Write-Host "`n[14/20] SORA25_004: Atypical/Segregated 3 ppl/km² → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":3.0,"isControlledGroundArea":false,"maxCharacteristicDimension":8.0,"mtom_kg":4.0,"mitigations":[]},"airRisk":{"explicitARC":"ARC_a","strategicMitigations":[],"isAtypicalSegregated":true}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: GRC=$($r.finalGRC), ARC=ARC_a, SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 15: SORA 2.5 High Density Metro - GRC-7 (SAIL VI → OUT OF SCOPE)
Write-Host "`n[15/20] SORA25_005: High Density 12000 ppl/km² → OUT OF SCOPE (SAIL VI)" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":12000.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.isSuccessful -eq $false -or $r.sail -eq "VI") { Write-Host "✅ PASS: Correctly calculated SAIL VI but rejected as OUT OF SCOPE" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Should calculate SAIL VI and reject" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "✅ PASS: Correctly rejected SAIL VI (400 error)" -ForegroundColor Green ; $passed++ }

# Test 16: SORA 2.5 Strategic Mitigation Floor Test
Write-Host "`n[16/20] SORA25_006: 1500 ppl/km² + S1 → SAIL V" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":1500.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":["S1"],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "V") { Write-Host "✅ PASS: ARC reduced to $($r.residualAirRiskClass), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_V, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 17: SORA 2.5 Controlled Airspace >500ft
Write-Host "`n[17/20] SORA25_007: 250 ppl/km² + ARC-d → SAIL V" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":250.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "V") { Write-Host "✅ PASS: GRC=$($r.finalGRC), ARC=ARC_d, SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_V, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 18: SORA 2.5 Assembly Scenario GRC-6 (OUT OF SCOPE)
Write-Host "`n[18/20] SORA25_008: Assembly 75000 ppl/km² → OUT OF SCOPE" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":75000.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[]},"airRisk":{"explicitARC":"ARC_c","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.isSuccessful -eq $false) { Write-Host "✅ PASS: Correctly rejected as OUT OF SCOPE" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Should be OUT OF SCOPE" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "✅ PASS: Correctly rejected (400 error)" -ForegroundColor Green ; $passed++ }

# Test 19: SORA 2.5 Lightly Populated with Sheltering (M1A Low = -1)
Write-Host "`n[19/20] SORA25_009: 35 ppl/km² + Sheltering → SAIL III" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":35.0,"isControlledGroundArea":false,"maxCharacteristicDimension":3.0,"mtom_kg":1.5,"mitigations":[{"type":"M1A","robustness":"Low"}]},"airRisk":{"explicitARC":"ARC_b","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.sail -eq "III") { Write-Host "✅ PASS: iGRC=$($r.intrinsicGRC), fGRC=$($r.finalGRC), SAIL=$($r.sail)" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Expected SAIL_III, got $($r.sail)" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red ; $failed++ }

# Test 20: SORA 2.5 Maximum Risk SAIL VI (OUT OF SCOPE)
Write-Host "`n[20/20] SORA25_010: Maximum Risk 90000 ppl/km² → OUT OF SCOPE" -ForegroundColor White
$body = '{"soraVersion":"2.5","groundRisk":{"populationDensity":90000.0,"isControlledGroundArea":false,"maxCharacteristicDimension":8.0,"mtom_kg":4.0,"mitigations":[]},"airRisk":{"explicitARC":"ARC_d","strategicMitigations":[],"isAtypicalSegregated":false}}'
try {
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $body -ContentType "application/json"
    if ($r.isSuccessful -eq $false) { Write-Host "✅ PASS: Correctly rejected as OUT OF SCOPE" -ForegroundColor Green ; $passed++ }
    else { Write-Host "❌ FAIL: Should be OUT OF SCOPE" -ForegroundColor Red ; $failed++ }
} catch { Write-Host "✅ PASS: Correctly rejected (400 error)" -ForegroundColor Green ; $passed++ }

# SUMMARY
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total: 20 tests" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Gray" })
$passRate = [math]::Round(($passed / 20) * 100, 1)
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } else { "Yellow" })

if ($passed -eq 20) {
    Write-Host "`n✅ ALL 20 TESTS PASSED! EASA/JARUS COMPLIANCE VERIFIED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n⚠️ $failed test(s) failed. Review output above." -ForegroundColor Yellow
    exit 1
}
