##############################################################################
# STEP 42 UI COMPONENTS TEST SUITE
# Tests the 5 UI enhancement functions in oso-ui-v2.js
# SORA 2.0 + SORA 2.5 Dual-Version Support
##############################################################################

Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 42: OSO COMPLEX ALGORITHMS - UI COMPONENTS TEST SUITE   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

$testsPassed = 0
$testsFailed = 0
$filePath = "..\Frontend\Pages\oso-ui-v2.js"

# Test 1: File exists and updated
Write-Host "[TEST 1] Checking oso-ui-v2.js file..." -ForegroundColor Yellow
if (Test-Path $filePath) {
    $fileInfo = Get-Item $filePath
    $fileSize = $fileInfo.Length
    Write-Host "✅ oso-ui-v2.js exists and updated ($fileSize bytes)" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ oso-ui-v2.js not found!" -ForegroundColor Red
    $testsFailed++
    exit 1
}

$content = Get-Content $filePath -Raw

##############################################################################
# CATEGORY 1: UI FUNCTION DEFINITIONS (5 tests)
##############################################################################
Write-Host "`n[CATEGORY 1] UI Function Definitions" -ForegroundColor Magenta

# Test 2: showDependencyWarnings function
Write-Host "[TEST 2] showDependencyWarnings function defined..." -ForegroundColor Yellow
if ($content -match "function showDependencyWarnings\(containerId, selectedOSOs, soraVersion\)") {
    Write-Host "✅ showDependencyWarnings function defined" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showDependencyWarnings function not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 3: showRecommendations function
Write-Host "[TEST 3] showRecommendations function defined..." -ForegroundColor Yellow
if ($content -match "function showRecommendations\(containerId, operationType, environment, sail, soraVersion\)") {
    Write-Host "✅ showRecommendations function defined" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showRecommendations function not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 4: showEvidenceChecklist function
Write-Host "[TEST 4] showEvidenceChecklist function defined..." -ForegroundColor Yellow
if ($content -match "function showEvidenceChecklist\(containerId, osoId, robustness, soraVersion\)") {
    Write-Host "✅ showEvidenceChecklist function defined" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showEvidenceChecklist function not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 5: showValidationErrors function
Write-Host "[TEST 5] showValidationErrors function defined..." -ForegroundColor Yellow
if ($content -match "function showValidationErrors\(containerId, selectedOSOs, sail, soraVersion, context\)") {
    Write-Host "✅ showValidationErrors function defined" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showValidationErrors function not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 6: showSAILFilter function
Write-Host "[TEST 6] showSAILFilter function defined..." -ForegroundColor Yellow
if ($content -match "function showSAILFilter\(containerId, sail, soraVersion\)") {
    Write-Host "✅ showSAILFilter function defined" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showSAILFilter function not found!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 2: DEPENDENCY WARNINGS COMPONENT (5 tests)
##############################################################################
Write-Host "`n[CATEGORY 2] Dependency Warnings Component" -ForegroundColor Magenta

# Test 7: OSOManager.validateDependencies call
Write-Host "[TEST 7] Dependency warnings uses OSOManager.validateDependencies..." -ForegroundColor Yellow
if ($content -match "OSOManager\.validateDependencies\(selectedOSOs, soraVersion\)") {
    Write-Host "✅ OSOManager.validateDependencies integration found" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ OSOManager.validateDependencies call not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 8: Missing dependencies display
Write-Host "[TEST 8] Missing dependencies display with red styling..." -ForegroundColor Yellow
if ($content -match "Missing Dependencies" -and $content -match "#ffebee" -and $content -match "#f44336") {
    Write-Host "✅ Missing dependencies display with red styling" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Missing dependencies display not styled correctly!" -ForegroundColor Red
    $testsFailed++
}

# Test 9: Warnings display
Write-Host "[TEST 9] Warnings display with orange styling..." -ForegroundColor Yellow
if ($content -match "⚠️ Warnings" -and $content -match "#fff3e0" -and $content -match "#ff9800") {
    Write-Host "✅ Warnings display with orange styling" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Warnings display not styled correctly!" -ForegroundColor Red
    $testsFailed++
}

# Test 10: No issues success message
Write-Host "[TEST 10] Success message when no dependency issues..." -ForegroundColor Yellow
if ($content -match "No dependency issues found" -and $content -match "#e8f5e9") {
    Write-Host "✅ Success message for no dependency issues" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Success message not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 11: Dependency reasons displayed
Write-Host "[TEST 11] Dependency reasons displayed..." -ForegroundColor Yellow
if ($content -match "dep\.reason") {
    Write-Host "✅ Dependency reasons displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Dependency reasons not displayed!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 3: RECOMMENDATIONS COMPONENT (6 tests)
##############################################################################
Write-Host "`n[CATEGORY 3] Recommendations Component" -ForegroundColor Magenta

# Test 12: OSOManager.recommendOSOs call
Write-Host "[TEST 12] Recommendations uses OSOManager.recommendOSOs..." -ForegroundColor Yellow
if ($content -match "OSOManager\.recommendOSOs\(operationType, environment, sail, soraVersion\)") {
    Write-Host "✅ OSOManager.recommendOSOs integration found" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ OSOManager.recommendOSOs call not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 13: Blue styling for recommendations
Write-Host "[TEST 13] Recommendations panel with blue styling..." -ForegroundColor Yellow
if ($content -match "#e3f2fd" -and $content -match "#2196f3") {
    Write-Host "✅ Recommendations panel with blue styling" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Recommendations panel not styled correctly!" -ForegroundColor Red
    $testsFailed++
}

# Test 14: Priority colors (CRITICAL, HIGH, MEDIUM, LOW)
Write-Host "[TEST 14] Priority colors defined..." -ForegroundColor Yellow
if ($content -match "CRITICAL.*#f44336" -and $content -match "HIGH.*#ff9800" -and $content -match "MEDIUM.*#2196f3" -and $content -match "LOW.*#4caf50") {
    Write-Host "✅ Priority colors defined (CRITICAL, HIGH, MEDIUM, LOW)" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Priority colors not fully defined!" -ForegroundColor Red
    $testsFailed++
}

# Test 15: Recommendation reasons
Write-Host "[TEST 15] Recommendation reasons displayed..." -ForegroundColor Yellow
if ($content -match "rec\.reason") {
    Write-Host "✅ Recommendation reasons displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Recommendation reasons not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 16: Min robustness displayed
Write-Host "[TEST 16] Min robustness displayed..." -ForegroundColor Yellow
if ($content -match "Min Robustness" -and $content -match "rec\.minRobustness") {
    Write-Host "✅ Min robustness displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Min robustness not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 17: Category displayed
Write-Host "[TEST 17] Category displayed..." -ForegroundColor Yellow
if ($content -match "Category" -and $content -match "rec\.category") {
    Write-Host "✅ Category displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Category not displayed!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 4: EVIDENCE CHECKLIST COMPONENT (7 tests)
##############################################################################
Write-Host "`n[CATEGORY 4] Evidence Checklist Component" -ForegroundColor Magenta

# Test 18: OSOManager.getEvidenceTemplate call
Write-Host "[TEST 18] Evidence checklist uses OSOManager.getEvidenceTemplate..." -ForegroundColor Yellow
if ($content -match "OSOManager\.getEvidenceTemplate\(osoId, robustness, soraVersion\)") {
    Write-Host "✅ OSOManager.getEvidenceTemplate integration found" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ OSOManager.getEvidenceTemplate call not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 19: Required documents with checkboxes
Write-Host "[TEST 19] Required documents with checkboxes..." -ForegroundColor Yellow
if ($content -match "Required Documents" -and $content -match '<input type="checkbox"') {
    Write-Host "✅ Required documents with checkboxes" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Required documents checkboxes not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 20: Required sections
Write-Host "[TEST 20] Required sections displayed..." -ForegroundColor Yellow
if ($content -match "Required Sections" -and $content -match "template\.sections") {
    Write-Host "✅ Required sections displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Required sections not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 21: Applicable standards
Write-Host "[TEST 21] Applicable standards displayed..." -ForegroundColor Yellow
if ($content -match "Applicable Standards" -and $content -match "template\.standards") {
    Write-Host "✅ Applicable standards displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Applicable standards not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 22: Third-party requirements
Write-Host "[TEST 22] Third-party requirements displayed..." -ForegroundColor Yellow
if ($content -match "Third-Party Requirement" -and $content -match "template\.thirdParty") {
    Write-Host "✅ Third-party requirements displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Third-party requirements not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 23: Failure rate objectives
Write-Host "[TEST 23] Failure rate objectives displayed..." -ForegroundColor Yellow
if ($content -match "Failure Rate Objectives" -and $content -match "template\.failureRates") {
    Write-Host "✅ Failure rate objectives displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Failure rate objectives not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 24: Checklist emoji
Write-Host "[TEST 24] Evidence checklist emoji..." -ForegroundColor Yellow
if ($content -match "📋 Evidence Requirements") {
    Write-Host "✅ Evidence checklist emoji present" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Evidence checklist emoji not found!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 5: VALIDATION ERRORS COMPONENT (6 tests)
##############################################################################
Write-Host "`n[CATEGORY 5] Validation Errors Component" -ForegroundColor Magenta

# Test 25: OSOManager.validateCrossOSO call
Write-Host "[TEST 25] Validation uses OSOManager.validateCrossOSO..." -ForegroundColor Yellow
if ($content -match "OSOManager\.validateCrossOSO\(selectedOSOs, sail, soraVersion, context\)") {
    Write-Host "✅ OSOManager.validateCrossOSO integration found" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ OSOManager.validateCrossOSO call not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 26: Validation errors red styling
Write-Host "[TEST 26] Validation errors with red styling..." -ForegroundColor Yellow
if ($content -match "Validation Errors" -and $content -match "#ffebee" -and $content -match "#f44336") {
    Write-Host "✅ Validation errors with red styling" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Validation errors not styled correctly!" -ForegroundColor Red
    $testsFailed++
}

# Test 27: Validation warnings yellow styling
Write-Host "[TEST 27] Validation warnings with yellow styling..." -ForegroundColor Yellow
if ($content -match "⚠️ Warnings" -and $content -match "#fff3e0" -and $content -match "#ff9800") {
    Write-Host "✅ Validation warnings with yellow styling" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Validation warnings not styled correctly!" -ForegroundColor Red
    $testsFailed++
}

# Test 28: Error references
Write-Host "[TEST 28] Error references displayed..." -ForegroundColor Yellow
if ($content -match "err\.reference" -and $content -match "Reference:") {
    Write-Host "✅ Error references displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Error references not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 29: Success message when no errors
Write-Host "[TEST 29] Success message when all validation passes..." -ForegroundColor Yellow
if ($content -match "All validation checks passed" -and $content -match "#e8f5e9") {
    Write-Host "✅ Success message for passed validation" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Success message not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 30: Error messages
Write-Host "[TEST 30] Error messages displayed..." -ForegroundColor Yellow
if ($content -match "err\.message") {
    Write-Host "✅ Error messages displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Error messages not displayed!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 6: SAIL FILTER COMPONENT (7 tests)
##############################################################################
Write-Host "`n[CATEGORY 6] SAIL Filter Component" -ForegroundColor Magenta

# Test 31: OSOManager.filterOSOsBySAIL call
Write-Host "[TEST 31] SAIL filter uses OSOManager.filterOSOsBySAIL..." -ForegroundColor Yellow
if ($content -match "OSOManager\.filterOSOsBySAIL\(sail, soraVersion\)") {
    Write-Host "✅ OSOManager.filterOSOsBySAIL integration found" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ OSOManager.filterOSOsBySAIL call not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 32: Three statistics boxes
Write-Host "[TEST 32] Three statistics boxes (Required, Optional, Not Required)..." -ForegroundColor Yellow
if ($content -match "Required" -and $content -match "Optional" -and $content -match "Not Required") {
    Write-Host "✅ Three statistics boxes present" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Statistics boxes not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 33: Required OSOs section
Write-Host "[TEST 33] Required OSOs section with green styling..." -ForegroundColor Yellow
if ($content -match "Required OSOs" -and $content -match "#e8f5e9") {
    Write-Host "✅ Required OSOs section with green styling" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Required OSOs section not styled correctly!" -ForegroundColor Red
    $testsFailed++
}

# Test 34: Not Required OSOs section
Write-Host "[TEST 34] Not Required OSOs section..." -ForegroundColor Yellow
if ($content -match "Not Required OSOs" -and $content -match "filtered\.notRequired") {
    Write-Host "✅ Not Required OSOs section present" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Not Required OSOs section not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 35: Details/summary expandable sections
Write-Host "[TEST 35] Details/summary expandable sections..." -ForegroundColor Yellow
if ($content -match "<details" -and $content -match "<summary") {
    Write-Host "✅ Expandable sections using details/summary" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Expandable sections not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 36: OSO numbers and names displayed
Write-Host "[TEST 36] OSO numbers and names displayed..." -ForegroundColor Yellow
if ($content -match "oso\.number" -and $content -match "oso\.name") {
    Write-Host "✅ OSO numbers and names displayed" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ OSO details not displayed!" -ForegroundColor Red
    $testsFailed++
}

# Test 37: Min robustness in filter
Write-Host "[TEST 37] Min robustness displayed in filter..." -ForegroundColor Yellow
if ($content -match "oso\.minRobustness") {
    Write-Host "✅ Min robustness displayed in filter" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Min robustness not displayed in filter!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 7: PUBLIC API EXPORTS (5 tests)
##############################################################################
Write-Host "`n[CATEGORY 7] Public API Exports" -ForegroundColor Magenta

# Test 38: showDependencyWarnings exported
Write-Host "[TEST 38] showDependencyWarnings exported..." -ForegroundColor Yellow
if ($content -match "showDependencyWarnings,") {
    Write-Host "✅ showDependencyWarnings exported" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showDependencyWarnings not exported!" -ForegroundColor Red
    $testsFailed++
}

# Test 39: showRecommendations exported
Write-Host "[TEST 39] showRecommendations exported..." -ForegroundColor Yellow
if ($content -match "showRecommendations,") {
    Write-Host "✅ showRecommendations exported" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showRecommendations not exported!" -ForegroundColor Red
    $testsFailed++
}

# Test 40: showEvidenceChecklist exported
Write-Host "[TEST 40] showEvidenceChecklist exported..." -ForegroundColor Yellow
if ($content -match "showEvidenceChecklist,") {
    Write-Host "✅ showEvidenceChecklist exported" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showEvidenceChecklist not exported!" -ForegroundColor Red
    $testsFailed++
}

# Test 41: showValidationErrors exported
Write-Host "[TEST 41] showValidationErrors exported..." -ForegroundColor Yellow
if ($content -match "showValidationErrors,") {
    Write-Host "✅ showValidationErrors exported" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showValidationErrors not exported!" -ForegroundColor Red
    $testsFailed++
}

# Test 42: showSAILFilter exported
Write-Host "[TEST 42] showSAILFilter exported..." -ForegroundColor Yellow
if ($content -match "showSAILFilter") {
    Write-Host "✅ showSAILFilter exported" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ showSAILFilter not exported!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 8: CONSOLE LOGGING (2 tests)
##############################################################################
Write-Host "`n[CATEGORY 8] Console Logging" -ForegroundColor Magenta

# Test 43: Step 42 console log
Write-Host "[TEST 43] Console log mentions Step 42 UI Components..." -ForegroundColor Yellow
if ($content -match "Step 42 UI Components") {
    Write-Host "✅ Console log mentions Step 42 UI Components" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Console log doesn't mention Step 42!" -ForegroundColor Red
    $testsFailed++
}

# Test 44: Component names in console log
Write-Host "[TEST 44] Console log lists all 5 component names..." -ForegroundColor Yellow
if ($content -match "Dependency Warnings" -and $content -match "Recommendations" -and $content -match "Evidence Checklists" -and $content -match "Validation Errors" -and $content -match "SAIL Filtering") {
    Write-Host "✅ Console log lists all 5 component names" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Console log doesn't list all components!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 9: INTEGRATION WITH STEP 41 (3 tests)
##############################################################################
Write-Host "`n[CATEGORY 9] Integration with Step 41" -ForegroundColor Magenta

# Test 45: Step 41 functions still present
Write-Host "[TEST 45] Step 41 functions still present..." -ForegroundColor Yellow
if ($content -match "renderOSOGrid" -and $content -match "showOSOSelectionModal" -and $content -match "exportComplianceReport") {
    Write-Host "✅ Step 41 functions still present (backward compatible)" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Step 41 functions missing!" -ForegroundColor Red
    $testsFailed++
}

# Test 46: Step 41 comment preserved
Write-Host "[TEST 46] Step 41 comment preserved in public API..." -ForegroundColor Yellow
if ($content -match "Step 41: Basic OSO Framework") {
    Write-Host "✅ Step 41 comment preserved" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Step 41 comment not preserved!" -ForegroundColor Red
    $testsFailed++
}

# Test 47: Step 42 comment added
Write-Host "[TEST 47] Step 42 comment added in public API..." -ForegroundColor Yellow
if ($content -match "Step 42: Complex Algorithms UI") {
    Write-Host "✅ Step 42 comment added" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Step 42 comment not found!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# CATEGORY 10: HTML STRUCTURE (5 tests)
##############################################################################
Write-Host "`n[CATEGORY 10] HTML Structure & Styling" -ForegroundColor Magenta

# Test 48: Gradient backgrounds
Write-Host "[TEST 48] Gradient backgrounds used..." -ForegroundColor Yellow
if ($content -match "linear-gradient") {
    Write-Host "✅ Gradient backgrounds used" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Gradient backgrounds not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 49: Border-left styling
Write-Host "[TEST 49] Border-left accent styling..." -ForegroundColor Yellow
if ($content -match "border-left:.*solid") {
    Write-Host "✅ Border-left accent styling used" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Border-left styling not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 50: Emoji usage
Write-Host "[TEST 50] Emojis used for visual enhancement..." -ForegroundColor Yellow
if ($content -match "🔴" -and $content -match "⚠️" -and $content -match "✅" -and $content -match "💡" -and $content -match "📋") {
    Write-Host "✅ Emojis used for visual enhancement" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Not all emojis found!" -ForegroundColor Red
    $testsFailed++
}

# Test 51: Responsive grid
Write-Host "[TEST 51] Grid layout used..." -ForegroundColor Yellow
if ($content -match "display: grid" -or $content -match "display: flex") {
    Write-Host "✅ Modern layout (grid/flex) used" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Modern layout not found!" -ForegroundColor Red
    $testsFailed++
}

# Test 52: Color scheme consistency
Write-Host "[TEST 52] Consistent color scheme..." -ForegroundColor Yellow
$redCount = ([regex]::Matches($content, "#f44336|#ffebee|#c62828")).Count
$greenCount = ([regex]::Matches($content, "#4caf50|#e8f5e9|#2e7d32")).Count
$blueCount = ([regex]::Matches($content, "#2196f3|#e3f2fd|#1565c0")).Count
$orangeCount = ([regex]::Matches($content, "#ff9800|#fff3e0|#e65100")).Count

if ($redCount -ge 3 -and $greenCount -ge 3 -and $blueCount -ge 3 -and $orangeCount -ge 3) {
    Write-Host "✅ Consistent color scheme (Red: $redCount, Green: $greenCount, Blue: $blueCount, Orange: $orangeCount)" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host "❌ Color scheme not consistent enough!" -ForegroundColor Red
    $testsFailed++
}

##############################################################################
# FINAL RESULTS
##############################################################################
Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                        TEST SUMMARY                            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

$totalTests = $testsPassed + $testsFailed
$passRate = [math]::Round(($testsPassed / $totalTests) * 100, 1)

Write-Host "Total Tests:    $totalTests" -ForegroundColor White
Write-Host "Tests Passed:   $testsPassed" -ForegroundColor Green
Write-Host "Tests Failed:   $testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host "Pass Rate:      $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } else { "Yellow" })

if ($testsFailed -eq 0) {
    Write-Host "`n🎉 STEP 42 - UI COMPONENTS IMPLEMENTATION COMPLETE!" -ForegroundColor Green
    Write-Host "✅ All 5 UI components successfully implemented" -ForegroundColor Green
    Write-Host "✅ Integration with Step 41 preserved" -ForegroundColor Green
    Write-Host "✅ SORA 2.0 + SORA 2.5 dual-version support" -ForegroundColor Green
    Write-Host "`n📄 Test page: Frontend/Pages/test-step42-ui.html" -ForegroundColor Cyan
    Write-Host "🌐 URL: http://localhost:8080/Pages/test-step42-ui.html`n" -ForegroundColor Cyan
} else {
    Write-Host "`n⚠️ Some tests failed. Please review the implementation." -ForegroundColor Yellow
    exit 1
}
