# OSO Framework - Simple Test Script
# Step 41 Validation

Write-Host ""
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host "  OSO FRAMEWORK TEST - Step 41" -ForegroundColor Green
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host ""

$passCount = 0
$failCount = 0

# Test 1: Files exist
Write-Host "TEST 1: File Existence" -ForegroundColor Yellow
if (Test-Path "Frontend\Pages\oso-manager-v2.js") {
    Write-Host "  [PASS] oso-manager-v2.js exists" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] oso-manager-v2.js NOT FOUND" -ForegroundColor Red
    $failCount++
}

if (Test-Path "Frontend\Pages\oso-ui-v2.js") {
    Write-Host "  [PASS] oso-ui-v2.js exists" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] oso-ui-v2.js NOT FOUND" -ForegroundColor Red
    $failCount++
}

if (Test-Path "Frontend\Pages\test-oso-framework.html") {
    Write-Host "  [PASS] test-oso-framework.html exists" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] test-oso-framework.html NOT FOUND" -ForegroundColor Red
    $failCount++
}

Write-Host ""

# Test 2: File sizes
Write-Host "TEST 2: File Sizes" -ForegroundColor Yellow
$managerSize = (Get-Item "Frontend\Pages\oso-manager-v2.js").Length
$uiSize = (Get-Item "Frontend\Pages\oso-ui-v2.js").Length

if ($managerSize -gt 30000) {
    Write-Host "  [PASS] oso-manager-v2.js size: $managerSize bytes (>30KB)" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] oso-manager-v2.js too small: $managerSize bytes" -ForegroundColor Red
    $failCount++
}

if ($uiSize -gt 20000) {
    Write-Host "  [PASS] oso-ui-v2.js size: $uiSize bytes (>20KB)" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] oso-ui-v2.js too small: $uiSize bytes" -ForegroundColor Red
    $failCount++
}

Write-Host ""

# Test 3: Content checks
Write-Host "TEST 3: Code Content" -ForegroundColor Yellow
$managerContent = Get-Content "Frontend\Pages\oso-manager-v2.js" -Raw

if ($managerContent -match "OSO_DEFINITIONS_V25") {
    Write-Host "  [PASS] OSO_DEFINITIONS_V25 found" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] OSO_DEFINITIONS_V25 NOT FOUND" -ForegroundColor Red
    $failCount++
}

if ($managerContent -match "class OSOComplianceTracker") {
    Write-Host "  [PASS] OSOComplianceTracker class found" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] OSOComplianceTracker class NOT FOUND" -ForegroundColor Red
    $failCount++
}

# Count OSOs
$osoCount = ([regex]::Matches($managerContent, "OSO#\d{2}")).Count
if ($osoCount -ge 17) {
    Write-Host "  [PASS] Found $osoCount OSO definitions (>=17)" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] Found only $osoCount OSO definitions (<17)" -ForegroundColor Red
    $failCount++
}

Write-Host ""

# Test 4: Mission.html integration
Write-Host "TEST 4: Mission.html Integration" -ForegroundColor Yellow
$missionContent = Get-Content "Frontend\Pages\mission.html" -Raw

if ($missionContent -match "oso-manager-v2\.js") {
    Write-Host "  [PASS] oso-manager-v2.js referenced in mission.html" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] oso-manager-v2.js NOT referenced" -ForegroundColor Red
    $failCount++
}

if ($missionContent -match "oso-ui-v2\.js") {
    Write-Host "  [PASS] oso-ui-v2.js referenced in mission.html" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] oso-ui-v2.js NOT referenced" -ForegroundColor Red
    $failCount++
}

if ($missionContent -match "OSOUI\.renderOSOGrid") {
    Write-Host "  [PASS] OSOUI.renderOSOGrid() integration found" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] OSOUI.renderOSOGrid() NOT integrated" -ForegroundColor Red
    $failCount++
}

Write-Host ""

# Test 5: Documentation
Write-Host "TEST 5: Documentation" -ForegroundColor Yellow

if (Test-Path "OSO_FRAMEWORK_STEP41_REPORT.txt") {
    Write-Host "  [PASS] Step 41 report exists" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] Step 41 report NOT FOUND" -ForegroundColor Red
    $failCount++
}

if (Test-Path "OSO_INTEGRATION_POINTS.md") {
    Write-Host "  [PASS] Integration points document exists" -ForegroundColor Green
    $passCount++
} else {
    Write-Host "  [FAIL] Integration points document NOT FOUND" -ForegroundColor Red
    $failCount++
}

Write-Host ""

# Final results
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host "  FINAL RESULTS" -ForegroundColor Green
Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host ""

$totalTests = $passCount + $failCount
Write-Host "Total Tests:  $totalTests" -ForegroundColor White
Write-Host "Passed:       $passCount" -ForegroundColor Green
Write-Host "Failed:       $failCount" -ForegroundColor Red

if ($failCount -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS! ALL TESTS PASSED!" -ForegroundColor Green -BackgroundColor DarkGreen
    Write-Host ""
    Write-Host "Next: Open http://localhost:8080/Pages/test-oso-framework.html" -ForegroundColor Cyan
    Write-Host "      Click 'Run All Tests' for interactive UI tests" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "WARNING: Some tests failed" -ForegroundColor Yellow -BackgroundColor DarkYellow
    Write-Host ""
}

Write-Host "=======================================================================" -ForegroundColor Cyan
Write-Host ""
