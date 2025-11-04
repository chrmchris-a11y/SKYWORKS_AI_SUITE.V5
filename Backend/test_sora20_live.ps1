# ═══════════════════════════════════════════════════════════════════════════
# SORA 2.0 ACCURACY TESTS - Simplified (using corrected test JSON directly)
# Per EASA AMC1 Article 11(1.c): No M3 Emergency Response Plan = +1 penalty
# ═══════════════════════════════════════════════════════════════════════════

$apiUrl = "http://localhost:5210/api/sora/complete"

Write-Host "`n🚀 SORA 2.0 ACCURACY TEST SUITE - 10 Scenarios" -ForegroundColor Magenta
Write-Host "Testing M3 Penalty Logic (No Emergency Response Plan = +1 GRC)" -ForegroundColor Magenta

# Run the exact same tests that unit tests use (already corrected for M3 penalty)
$testData = Get-Content "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\tests\Skyworks.Api.Tests\TestData\SORAAuthoritative_TestCases.json" | ConvertFrom-Json

$passed = 0
$failed = 0
$testNumber = 0

foreach ($testCase in $testData.testCases) {
    # Only run SORA 2.0 tests (skip JARUS 2.5)
    if ($testCase.testId -notlike "SORA20_*") {
        continue
    }
    
    $testNumber++
    
    # Skip out-of-scope tests
    if ($testCase.expectedToFail -eq $true) {
        Write-Host "`n══════════════════════════════════════════════════════" -ForegroundColor Gray
        Write-Host "SKIPPING TEST #$($testNumber): $($testCase.name) (Out of Scope)" -ForegroundColor Gray
        continue
    }
    
    Write-Host "`n══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "TEST #$($testNumber): $($testCase.name)" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "Description: $($testCase.description)" -ForegroundColor Gray
    Write-Host "Rationale: $($testCase.rationale)" -ForegroundColor Gray
    
    try {
        $json = $testCase.input | ConvertTo-Json -Depth 10 -Compress
        $json = $json -replace '":null', '":null' -replace '":true', '":true' -replace '":false', '":false'
        
        $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $json -ContentType "application/json"
        
        $actualIGRC = $response.intrinsicGRC
        $actualFinal = $response.finalGRC
        $actualARC = $response.residualARC
        $actualSAIL = $response.sail
        
        # Extract expected values
        $expectedIGRC = [int]($testCase.expected.initialGRC -replace 'GRC_', '')
        $expectedFinal = [int]($testCase.expected.finalGRC -replace 'GRC_', '')
        $expectedARC = $testCase.expected.residualARC
        $expectedSAIL = $testCase.expected.sail
        
        Write-Host "`nExpected vs Actual:" -ForegroundColor Yellow
        
        $testPassed = $true
        
        if ($expectedIGRC -eq $actualIGRC) {
            Write-Host "  iGRC: $expectedIGRC ✅" -ForegroundColor Green
        } else {
            Write-Host "  iGRC: Expected $expectedIGRC, Got $actualIGRC ❌" -ForegroundColor Red
            $testPassed = $false
        }
        
        if ($expectedFinal -eq $actualFinal) {
            Write-Host "  Final GRC: $expectedFinal ✅" -ForegroundColor Green
        } else {
            Write-Host "  Final GRC: Expected $expectedFinal, Got $actualFinal ❌" -ForegroundColor Red
            $testPassed = $false
        }
        
        if ($expectedARC -eq $actualARC) {
            Write-Host "  ARC: $expectedARC ✅" -ForegroundColor Green
        } else {
            Write-Host "  ARC: Expected $expectedARC, Got $actualARC ❌" -ForegroundColor Red
            $testPassed = $false
        }
        
        if ($expectedSAIL -eq $actualSAIL) {
            Write-Host "  SAIL: $expectedSAIL ✅" -ForegroundColor Green
        } else {
            Write-Host "  SAIL: Expected $expectedSAIL, Got $actualSAIL ❌" -ForegroundColor Red
            $testPassed = $false
        }
        
        if ($testPassed) {
            $passed++
        } else {
            $failed++
        }
    }
    catch {
        Write-Host "❌ API ERROR: $_" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        $failed++
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# FINAL SUMMARY
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "FINAL SUMMARY" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "Total Tests: $testNumber" -ForegroundColor Cyan
Write-Host "✅ Passed: $passed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor Red

if ($failed -eq 0) {
    Write-Host "`n🎉 ALL TESTS PASSED! M3 Penalty logic is EASA/JARUS compliant!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tests failed. Review backend calculations." -ForegroundColor Yellow
}
