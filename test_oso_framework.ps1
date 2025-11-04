# ═══════════════════════════════════════════════════════════════════════════
# OSO FRAMEWORK - AUTOMATED TEST SUITE (Step 41)
# ═══════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🧪 OSO FRAMEWORK - AUTOMATED TEST SUITE" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$totalTests = 0
$passedTests = 0
$failedTests = 0

function Test-Result {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message
    )
    
    $global:totalTests++
    
    if ($Passed) {
        $global:passedTests++
        Write-Host "✅ PASS: $TestName" -ForegroundColor Green
        Write-Host "   $Message" -ForegroundColor Gray
    } else {
        $global:failedTests++
        Write-Host "❌ FAIL: $TestName" -ForegroundColor Red
        Write-Host "   $Message" -ForegroundColor Yellow
    }
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════════════════
# TEST 1: File Existence
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "📂 TEST 1: File Existence & Structure" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

# Test oso-manager-v2.js exists
if (Test-Path "Frontend\Pages\oso-manager-v2.js") {
    $fileSize = (Get-Item "Frontend\Pages\oso-manager-v2.js").Length
    Test-Result "oso-manager-v2.js exists" $true "File size: $fileSize bytes"
} else {
    Test-Result "oso-manager-v2.js exists" $false "File not found!"
}

# Test oso-ui-v2.js exists
if (Test-Path "Frontend\Pages\oso-ui-v2.js") {
    $fileSize = (Get-Item "Frontend\Pages\oso-ui-v2.js").Length
    Test-Result "oso-ui-v2.js exists" $true "File size: $fileSize bytes"
} else {
    Test-Result "oso-ui-v2.js exists" $false "File not found!"
}

# Test mission.html integration
if (Test-Path "Frontend\Pages\mission.html") {
    $missionContent = Get-Content "Frontend\Pages\mission.html" -Raw
    
    # Check if oso-manager-v2.js is referenced
    if ($missionContent -match 'oso-manager-v2\.js') {
        Test-Result "mission.html references oso-manager-v2.js" $true "Script tag found"
    } else {
        Test-Result "mission.html references oso-manager-v2.js" $false "Script tag not found!"
    }
    
    # Check if oso-ui-v2.js is referenced
    if ($missionContent -match 'oso-ui-v2\.js') {
        Test-Result "mission.html references oso-ui-v2.js" $true "Script tag found"
    } else {
        Test-Result "mission.html references oso-ui-v2.js" $false "Script tag not found!"
    }
    
    # Check if osoGrid div exists
    if ($missionContent -match 'id="osoGrid"') {
        Test-Result "mission.html has osoGrid container" $true "Container div found"
    } else {
        Test-Result "mission.html has osoGrid container" $false "Container div not found!"
    }
    
    # Check if renderOSOs function updated
    if ($missionContent -match 'OSOUI\.renderOSOGrid') {
        Test-Result "renderOSOs() calls OSOUI.renderOSOGrid()" $true "Integration code found"
    } else {
        Test-Result "renderOSOs() calls OSOUI.renderOSOGrid()" $false "Integration code not found!"
    }
} else {
    Test-Result "mission.html exists" $false "File not found!"
}

# ═══════════════════════════════════════════════════════════════════════════
# TEST 2: OSO Manager Content
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "📋 TEST 2: OSO Manager Content & Structure" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

$managerContent = Get-Content "Frontend\Pages\oso-manager-v2.js" -Raw

# Check for OSO_DEFINITIONS_V25
if ($managerContent -match 'const OSO_DEFINITIONS_V25 = ') {
    Test-Result "OSO_DEFINITIONS_V25 defined" $true "Main OSO data structure found"
} else {
    Test-Result "OSO_DEFINITIONS_V25 defined" $false "Main OSO data structure not found!"
}

# Check for all 17 OSOs (SORA 2.5)
$osoNumbers = @(1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 16, 17, 18, 19, 20, 23, 24)
$foundOSOs = 0

foreach ($osoNum in $osoNumbers) {
    $pattern = "OSO#" + $osoNum.ToString('00')
    if ($managerContent -match $pattern) {
        $foundOSOs++
    }
}

if ($foundOSOs -eq 17) {
    Test-Result "All 17 SORA 2.5 OSOs present" $true "Found $foundOSOs/17 OSOs"
} else {
    Test-Result "All 17 SORA 2.5 OSOs present" $false "Found only $foundOSOs/17 OSOs!"
}

# Check for OSOComplianceTracker class
if ($managerContent -match 'class OSOComplianceTracker') {
    Test-Result "OSOComplianceTracker class defined" $true "Main tracker class found"
} else {
    Test-Result "OSOComplianceTracker class defined" $false "Main tracker class not found!"
}

# Check for key methods
$methods = @(
    'getRequiredOSOs',
    'selectOSO',
    'validateCompliance',
    'exportComplianceReport',
    'importComplianceData'
)

$foundMethods = 0
foreach ($method in $methods) {
    if ($managerContent -match $method) {
        $foundMethods++
    }
}

if ($foundMethods -eq $methods.Count) {
    Test-Result "All tracker methods present" $true "Found $foundMethods/$($methods.Count) methods"
} else {
    Test-Result "All tracker methods present" $false "Found only $foundMethods/$($methods.Count) methods!"
}

# Check for robustness levels (NR, L, M, H)
if ($managerContent -match "NR" -and 
    $managerContent -match "'L':" -and 
    $managerContent -match "'M':" -and 
    $managerContent -match "'H':") {
    Test-Result "Robustness levels (NR/L/M/H) defined" $true "All robustness levels found"
} else {
    Test-Result "Robustness levels (NR/L/M/H) defined" $false "Missing robustness level definitions!"
}

# Check for SAIL requirements (I-VI)
$sailLevels = @('I', 'II', 'III', 'IV', 'V', 'VI')
$foundSAILs = 0

foreach ($sailLevel in $sailLevels) {
    if ($managerContent -match "sailRequirements:.*${sailLevel}:") {
        $foundSAILs++
    }
}

if ($foundSAILs -eq 6) {
    Test-Result "SAIL I-VI requirements mapped" $true "Found all 6 SAIL levels"
} else {
    Test-Result "SAIL I-VI requirements mapped" $false "Found only $foundSAILs/6 SAIL levels!"
}

# ═══════════════════════════════════════════════════════════════════════════
# TEST 3: OSO UI Content
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "🎨 TEST 3: OSO UI Content & Functions" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

$uiContent = Get-Content "Frontend\Pages\oso-ui-v2.js" -Raw

# Check for OSOUI object
if ($uiContent -match 'const OSOUI') {
    Test-Result "OSOUI module defined" $true "IIFE pattern found"
} else {
    Test-Result "OSOUI module defined" $false "IIFE pattern not found!"
}

# Check for key UI functions
$uiFunctions = @(
    'renderOSOGrid',
    'showOSOSelectionModal',
    'exportComplianceReport',
    'importComplianceReport',
    'getCurrentTracker'
)

$foundUIFunctions = 0
foreach ($func in $uiFunctions) {
    if ($uiContent -match "function $func") {
        $foundUIFunctions++
    }
}

if ($foundUIFunctions -eq $uiFunctions.Count) {
    Test-Result "All UI functions present" $true "Found $foundUIFunctions/$($uiFunctions.Count) functions"
} else {
    Test-Result "All UI functions present" $false "Found only $foundUIFunctions/$($uiFunctions.Count) functions!"
}

# Check for color-coded badges (green, orange, red)
if ($uiContent -match '#4caf50' -and $uiContent -match '#ff9800' -and $uiContent -match '#f44336') {
    Test-Result "Color-coded compliance badges" $true "Green/Orange/Red colors found"
} else {
    Test-Result "Color-coded compliance badges" $false "Missing color definitions!"
}

# Check for modal implementation
if ($uiContent -match 'modal' -or $uiContent -match 'overlay') {
    Test-Result "Modal dialog implementation" $true "Modal/overlay code found"
} else {
    Test-Result "Modal dialog implementation" $false "Modal/overlay code not found!"
}

# Check for compliance percentage display
if ($uiContent -match 'compliancePercentage' -or $uiContent -match 'compliance.*%') {
    Test-Result "Compliance percentage tracking" $true "Percentage tracking code found"
} else {
    Test-Result "Compliance percentage tracking" $false "Percentage tracking code not found!"
}

# ═══════════════════════════════════════════════════════════════════════════
# TEST 4: JARUS References
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "📚 TEST 4: JARUS Authoritative References" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

# Check for SORA 2.5 references
if ($managerContent -match 'SORA 2\.5') {
    Test-Result "SORA 2.5 references" $true "Found SORA 2.5 mentions"
} else {
    Test-Result "SORA 2.5 references" $false "No SORA 2.5 references found!"
}

# Check for Table 14 reference
if ($managerContent -match 'Table 14') {
    Test-Result "JARUS Table 14 reference" $true "Table 14 mentioned"
} else {
    Test-Result "JARUS Table 14 reference" $false "Table 14 not referenced!"
}

# Check for Annex E reference
if ($managerContent -match 'Annex E') {
    Test-Result "JARUS Annex E reference" $true "Annex E mentioned"
} else {
    Test-Result "JARUS Annex E reference" $false "Annex E not referenced!"
}

# Check for jarusRef field in OSOs
if ($managerContent -match 'jarusRef:') {
    Test-Result "JARUS reference field in OSOs" $true "jarusRef field found"
} else {
    Test-Result "JARUS reference field in OSOs" $false "jarusRef field not found!"
}

# ═══════════════════════════════════════════════════════════════════════════
# TEST 5: Documentation
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "📝 TEST 5: Documentation & Reports" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

# Check for Step 41 report
if (Test-Path "OSO_FRAMEWORK_STEP41_REPORT.txt") {
    Test-Result "Step 41 report exists" $true "Documentation file found"
} else {
    Test-Result "Step 41 report exists" $false "Documentation file not found!"
}

# Check for integration points document
if (Test-Path "OSO_INTEGRATION_POINTS.md") {
    Test-Result "Integration points document exists" $true "Integration guide found"
} else {
    Test-Result "Integration points document exists" $false "Integration guide not found!"
}

# Check for test page
if (Test-Path "Frontend\Pages\test-oso-framework.html") {
    Test-Result "Test suite page exists" $true "Test HTML file found"
} else {
    Test-Result "Test suite page exists" $false "Test HTML file not found!"
}

# ═══════════════════════════════════════════════════════════════════════════
# FINAL RESULTS
# ═══════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 FINAL TEST RESULTS" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "Total Tests:  " -NoNewline
Write-Host "$totalTests" -ForegroundColor White

Write-Host "Passed:       " -NoNewline
Write-Host "$passedTests" -ForegroundColor Green

Write-Host "Failed:       " -NoNewline
$failColor = if ($failedTests -eq 0) { 'Green' } else { 'Red' }
Write-Host "$failedTests" -ForegroundColor $failColor

$successRate = [math]::Round(($passedTests / $totalTests) * 100, 2)
Write-Host "Success Rate: " -NoNewline
$color = if ($successRate -ge 90) { 'Green' } elseif ($successRate -ge 70) { 'Yellow' } else { 'Red' }
Write-Host "$successRate%" -ForegroundColor $color

Write-Host ""

if ($failedTests -eq 0) {
    Write-Host "🎉 ALL TESTS PASSED! OSO FRAMEWORK IS READY! 🎉" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ Step 41 - OSO Basic Framework: COMPLETE" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Open http://localhost:8080/Pages/test-oso-framework.html" -ForegroundColor Cyan
    Write-Host "  2. Click Run All Tests button to see interactive UI tests" -ForegroundColor Cyan
    Write-Host "  3. Test in mission.html by running SORA evaluation" -ForegroundColor Cyan
    Write-Host "  4. Proceed to Step 42: Complex OSO Algorithms" -ForegroundColor Cyan
} else {
    Write-Host "⚠️  SOME TESTS FAILED - PLEASE REVIEW" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Failed tests: $failedTests" -ForegroundColor Red
    Write-Host "Please check the error messages above" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Return exit code based on test results
exit $(if ($failedTests -eq 0) { 0 } else { 1 })
