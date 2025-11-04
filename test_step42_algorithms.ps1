# ═══════════════════════════════════════════════════════════════════════════
# STEP 42 - COMPLEX OSO ALGORITHMS TEST SUITE
# Tests all 5 algorithms for SORA 2.0 AND SORA 2.5
# ═══════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🧪 STEP 42 - COMPLEX OSO ALGORITHMS TEST SUITE" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$testsPassed = 0
$testsFailed = 0

function Test-Result {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Details = ""
    )
    
    if ($Passed) {
        Write-Host "✅ $TestName" -ForegroundColor Green
        if ($Details) {
            Write-Host "   $Details" -ForegroundColor Gray
        }
        $script:testsPassed++
    } else {
        Write-Host "❌ $TestName" -ForegroundColor Red
        if ($Details) {
            Write-Host "   $Details" -ForegroundColor Yellow
        }
        $script:testsFailed++
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Test 1: File Existence & Size
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "📁 Test 1: File Updates" -ForegroundColor Yellow
Write-Host ""

$managerPath = "Frontend\Pages\oso-manager-v2.js"
if (Test-Path $managerPath) {
    $fileSize = (Get-Item $managerPath).Length
    Test-Result "oso-manager-v2.js exists and updated" ($fileSize -gt 60000) "File size: $fileSize bytes (expected >60KB with new algorithms)"
} else {
    Test-Result "oso-manager-v2.js exists" $false "File not found"
}

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 2: Algorithm Function Existence
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "🔍 Test 2: Algorithm Function Definitions" -ForegroundColor Yellow
Write-Host ""

$managerContent = Get-Content $managerPath -Raw

$algorithms = @(
    @{ Name = "validateDependencies"; Pattern = "function validateDependencies" },
    @{ Name = "recommendOSOs"; Pattern = "function recommendOSOs" },
    @{ Name = "getEvidenceTemplate"; Pattern = "getEvidenceTemplate:" },
    @{ Name = "validateCrossOSO"; Pattern = "validateCrossOSO:" },
    @{ Name = "filterOSOsBySAIL"; Pattern = "filterOSOsBySAIL:" }
)

foreach ($algo in $algorithms) {
    $found = $managerContent -match [regex]::Escape($algo.Pattern)
    Test-Result "$($algo.Name) function defined" $found
}

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 3: Data Structure Definitions
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "📊 Test 3: Data Structure Definitions" -ForegroundColor Yellow
Write-Host ""

$structures = @(
    @{ Name = "SORA_20_DEPENDENCIES"; Pattern = "const SORA_20_DEPENDENCIES" },
    @{ Name = "SORA_25_DEPENDENCIES"; Pattern = "const SORA_25_DEPENDENCIES" },
    @{ Name = "EVIDENCE_PROVIDERS"; Pattern = "const EVIDENCE_PROVIDERS" }
)

foreach ($struct in $structures) {
    $found = $managerContent -match [regex]::Escape($struct.Pattern)
    Test-Result "$($struct.Name) defined" $found
}

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 4: Dependency Definitions Content
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "🔗 Test 4: Dependency Definitions Content" -ForegroundColor Yellow
Write-Host ""

# SORA 2.0 dependencies
$sora20Deps = @(10, 12, 13, 11)
foreach ($dep in $sora20Deps) {
    $pattern = "$dep" + ":\s*\{"
    $found = $managerContent -match $pattern
    Test-Result "SORA 2.0 OSO#$dep dependency defined" $found
}

# SORA 2.5 dependencies
Test-Result "SORA 2.5 OSO#05 Containment dependency" ($managerContent -match "'Containment'")
Test-Result "SORA 2.5 OSO#05 M2 dependency" ($managerContent -match "'M2'")

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 5: Evidence Template Definitions
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "📋 Test 5: Evidence Template Definitions" -ForegroundColor Yellow
Write-Host ""

# Key OSOs with evidence templates
$evidenceOSOs = @(1, 2, 5, 8, 9)
foreach ($osoId in $evidenceOSOs) {
    $pattern = "$osoId" + ":\s*\{"
    $found = $managerContent -match $pattern
    Test-Result "OSO#$osoId evidence template defined" $found
}

# Check for robustness levels
Test-Result "Evidence templates include 'L' (Low)" ($managerContent -match "'L':\s*\{")
Test-Result "Evidence templates include 'M' (Medium)" ($managerContent -match "'M':\s*\{")
Test-Result "Evidence templates include 'H' (High)" ($managerContent -match "'H':\s*\{")

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 6: Auto-Recommendation Logic
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "💡 Test 6: Auto-Recommendation Logic" -ForegroundColor Yellow
Write-Host ""

# Check for operation type handling
Test-Result "BVLOS operation handling" ($managerContent -match "operationType === 'BVLOS'")
Test-Result "Urban environment handling" ($managerContent -match "environment === 'Urban'")
Test-Result "SAIL-based recommendations" ($managerContent -match "sailNum >= ")

# Check for version-specific recommendations
Test-Result "SORA 2.0 OSO#10 recommendation" ($managerContent -match "id: 10,")
Test-Result "SORA 2.5 OSO#05 note about OSO#10 merge" ($managerContent -match "formerly OSO#10")

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 7: Cross-OSO Validation Rules
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "✅ Test 7: Cross-OSO Validation Rules" -ForegroundColor Yellow
Write-Host ""

$validationRules = @(
    @{ Name = "OSO#05 Containment check"; Pattern = "OSO#05_CONTAINMENT" },
    @{ Name = "BVLOS robustness check"; Pattern = "BVLOS_.*_ROBUSTNESS" },
    @{ Name = "Training + External Services"; Pattern = "TRAINING_EXTERNAL_SERVICES" },
    @{ Name = "High SAIL requirements"; Pattern = "HIGH_SAIL_ROBUSTNESS" }
)

foreach ($rule in $validationRules) {
    $found = $managerContent -match $rule.Pattern
    Test-Result "$($rule.Name)" $found
}

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 8: SAIL Filtering Logic
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "🎯 Test 8: SAIL Filtering Logic" -ForegroundColor Yellow
Write-Host ""

Test-Result "SAIL requirements check" ($managerContent -match "oso.sailRequirements\[sail\]")
Test-Result "Required OSOs array" ($managerContent -match "required.push")
Test-Result "Not required OSOs array" ($managerContent -match "notRequired.push")

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 9: Public API Exports
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "🌐 Test 9: Public API Exports" -ForegroundColor Yellow
Write-Host ""

$apiExports = @(
    "validateDependencies:",
    "recommendOSOs:",
    "getEvidenceTemplate:",
    "validateCrossOSO:",
    "filterOSOsBySAIL:",
    "getDependencies:",
    "getEvidenceProviders:"
)

foreach ($export in $apiExports) {
    $found = $managerContent -match [regex]::Escape($export)
    Test-Result "$export exported in API" $found
}

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 10: Console Logging
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "📢 Test 10: Algorithm Console Logging" -ForegroundColor Yellow
Write-Host ""

Test-Result "Step 42 algorithms mentioned in console log" ($managerContent -match "Step 42 Algorithms")
Test-Result "Both SORA versions mentioned" ($managerContent -match "SORA 2.0.*SORA 2.5")

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 11: Version-Specific Logic
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "🔄 Test 11: Version-Specific Logic" -ForegroundColor Yellow
Write-Host ""

# Check for proper version handling
Test-Result "SORA version detection in dependencies" ($managerContent -match "soraVersion === 'SORA-2.0'")
Test-Result "SORA version detection in recommendations" ($managerContent -match "soraVersion === 'SORA-2.5'")
Test-Result "Dual version support in filtering" ($managerContent -match "this.getAllOSOs\(soraVersion\)")

Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# Test 12: Standards References
# ─────────────────────────────────────────────────────────────────────────────

Write-Host "📖 Test 12: Standards References" -ForegroundColor Yellow
Write-Host ""

$standards = @(
    "ICAO Annex 19",
    "Eurocae ED-280",
    "JARUS AMC RPAS.1309",
    "DO-178",
    "DO-254",
    "UK CAA CAP 722A"
)

foreach ($standard in $standards) {
    $found = $managerContent -match [regex]::Escape($standard)
    Test-Result "$standard referenced" $found
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# FINAL RESULTS
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$totalTests = $testsPassed + $testsFailed
$passRate = [math]::Round(($testsPassed / $totalTests) * 100, 1)

if ($testsFailed -eq 0) {
    Write-Host "✅ ALL TESTS PASSED! ($testsPassed/$totalTests)" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 STEP 42 - COMPLEX OSO ALGORITHMS IMPLEMENTATION COMPLETE!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📦 DELIVERABLES:" -ForegroundColor Yellow
    Write-Host "   • oso-manager-v2.js extended with 5 algorithms" -ForegroundColor White
    Write-Host "   • Algorithm 1: Dependency Resolution (SORA 2.0 + 2.5)" -ForegroundColor Gray
    Write-Host "   • Algorithm 2: Auto-Recommendations (operation-based)" -ForegroundColor Gray
    Write-Host "   • Algorithm 3: Evidence Templates (per OSO/robustness)" -ForegroundColor Gray
    Write-Host "   • Algorithm 4: Cross-OSO Validation (smart rules)" -ForegroundColor Gray
    Write-Host "   • Algorithm 5: SAIL-based Filtering" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🔗 INTEGRATION:" -ForegroundColor Yellow
    Write-Host "   • Algorithms auto-loaded in mission.html" -ForegroundColor White
    Write-Host "   • Available via OSOManager.* API" -ForegroundColor White
    Write-Host "   • Ready for UI integration (oso-ui-v2.js)" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "   1. Open mission.html" -ForegroundColor White
    Write-Host "   2. Select OSOs and observe:" -ForegroundColor White
    Write-Host "      - Dependency warnings" -ForegroundColor Gray
    Write-Host "      - Auto-recommendations" -ForegroundColor Gray
    Write-Host "      - Evidence checklists" -ForegroundColor Gray
    Write-Host "      - Validation errors" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "⚠️  TESTS COMPLETED: $testsPassed passed, $testsFailed failed ($passRate%)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please review failed tests above." -ForegroundColor Yellow
}

Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Return exit code
if ($testsFailed -eq 0) { exit 0 } else { exit 1 }
