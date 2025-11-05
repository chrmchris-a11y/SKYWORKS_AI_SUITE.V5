# PowerShell SORA SAIL Validation Test
# "The SAIL defined in Step #9 is the level of robustness required to satisfy the safety objectives specified in Annex E." (EASA AMC/GM – SORA)

param(
    [string]$ApiBaseUrl = "https://localhost:7001",
    [string]$PythonBaseUrl = "http://localhost:8001"
)

Write-Host "=== COMPREHENSIVE SORA VALIDATION TEST ===" -ForegroundColor Cyan
Write-Host "API Base URL: $ApiBaseUrl" -ForegroundColor Gray
Write-Host "Python Base URL: $PythonBaseUrl" -ForegroundColor Gray

# Service readiness check
Write-Host "`n=== SERVICE READINESS CHECK ===" -ForegroundColor Yellow
try {
    $pythonHealth = Invoke-RestMethod -Uri "$PythonBaseUrl/health" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✓ Python service is ready" -ForegroundColor Green
} catch {
    Write-Host "✗ Python service not reachable at $PythonBaseUrl" -ForegroundColor Red
    Write-Host "HINT: Start uvicorn on :8001" -ForegroundColor Yellow
    exit 1
}

try {
    $apiHealth = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✓ API service is ready" -ForegroundColor Green
} catch {
    Write-Host "✗ API service not reachable at $ApiBaseUrl" -ForegroundColor Red
    exit 1
}

# Test tracking
$script:TestResults = @{
    Sora20Passed = 0
    Sora20Failed = 0
    Sora25Passed = 0
    Sora25Failed = 0
    NegativeTestsPassed = 0
    NegativeTestsFailed = 0
}

function Test-SAILEndpoint {
    param(
        [string]$TestName,
        [hashtable]$RequestBody,
        [string]$ExpectedSail = $null,
        [string]$ExpectedCategory = $null,
        [int]$ExpectedOsoCount = -1,
        [int]$ExpectedStatusCode = 200,
        [string]$TestType = "Functional"
    )
    
    try {
        $headers = @{ "Content-Type" = "application/json" }
        $body = $RequestBody | ConvertTo-Json -Depth 3
        
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/sail/calculate" -Method Post -Body $body -Headers $headers -TimeoutSec 10 -ErrorAction Stop
        
        $actualStatusCode = 200 # Invoke-RestMethod only returns on success
        
        if ($ExpectedStatusCode -ne 200) {
            Write-Host "  ✗ $TestName - Expected status $ExpectedStatusCode but got $actualStatusCode" -ForegroundColor Red
            if ($TestType -eq "Negative") { $script:TestResults.NegativeTestsFailed++ }
            elseif ($RequestBody.soraVersion -eq "2.0") { $script:TestResults.Sora20Failed++ }
            else { $script:TestResults.Sora25Failed++ }
            return
        }
        
        $passed = $true
        $issues = @()
        
        if ($ExpectedSail -and $response.sail -ne $ExpectedSail) {
            $issues += "SAIL: expected '$ExpectedSail', got '$($response.sail)'"
            $passed = $false
        }
        
        if ($ExpectedCategory -and $response.category -ne $ExpectedCategory) {
            $issues += "Category: expected '$ExpectedCategory', got '$($response.category)'"
            $passed = $false
        }
        
        if ($ExpectedOsoCount -ge 0 -and $response.osoCount -ne $ExpectedOsoCount) {
            $issues += "OSO Count: expected $ExpectedOsoCount, got $($response.osoCount)"
            $passed = $false
        }
        
        if ($RequestBody.soraVersion -eq "2.5" -and $response.osoCount -ne $null) {
            $issues += "OSO Count should be null for SORA 2.5, got $($response.osoCount)"
            $passed = $false
        }
        
        if ($passed) {
            Write-Host "  ✓ $TestName" -ForegroundColor Green
            if ($TestType -eq "Negative") { $script:TestResults.NegativeTestsPassed++ }
            elseif ($RequestBody.soraVersion -eq "2.0") { $script:TestResults.Sora20Passed++ }
            else { $script:TestResults.Sora25Passed++ }
        } else {
            Write-Host "  ✗ $TestName - $($issues -join ', ')" -ForegroundColor Red
            if ($TestType -eq "Negative") { $script:TestResults.NegativeTestsFailed++ }
            elseif ($RequestBody.soraVersion -eq "2.0") { $script:TestResults.Sora20Failed++ }
            else { $script:TestResults.Sora25Failed++ }
        }
        
    } catch {
        $statusCode = 500
        if ($_.Exception -match "(\d{3})") {
            $statusCode = [int]$matches[1]
        }
        
        if ($statusCode -eq $ExpectedStatusCode) {
            Write-Host "  ✓ $TestName (Expected $ExpectedStatusCode)" -ForegroundColor Green
            if ($TestType -eq "Negative") { $script:TestResults.NegativeTestsPassed++ }
            elseif ($RequestBody.soraVersion -eq "2.0") { $script:TestResults.Sora20Passed++ }
            else { $script:TestResults.Sora25Passed++ }
        } else {
            Write-Host "  ✗ $TestName - Expected status $ExpectedStatusCode, got $statusCode" -ForegroundColor Red
            Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Gray
            Write-Host "    URL: $ApiBaseUrl/api/sail/calculate" -ForegroundColor Gray
            if ($TestType -eq "Negative") { $script:TestResults.NegativeTestsFailed++ }
            elseif ($RequestBody.soraVersion -eq "2.0") { $script:TestResults.Sora20Failed++ }
            else { $script:TestResults.Sora25Failed++ }
        }
    }
}

# SORA 2.0 Golden Cases
Write-Host "`n=== SORA 2.0 GOLDEN CASES ===" -ForegroundColor Yellow

Test-SAILEndpoint -TestName "GRC 3, ARC-a → SAIL I" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 3
    residualArc = "a"
} -ExpectedSail "I" -ExpectedOsoCount 6

Test-SAILEndpoint -TestName "GRC 4, ARC-a → SAIL II" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 4
    residualArc = "a"
} -ExpectedSail "II" -ExpectedOsoCount 10

Test-SAILEndpoint -TestName "GRC 5, ARC-b → SAIL IV" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 5
    residualArc = "b"
} -ExpectedSail "IV" -ExpectedOsoCount 18

Test-SAILEndpoint -TestName "GRC 6, ARC-b → SAIL IV" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 6
    residualArc = "b"
} -ExpectedSail "IV" -ExpectedOsoCount 18

Test-SAILEndpoint -TestName "GRC 7, ARC-c → SAIL VI" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 7
    residualArc = "c"
} -ExpectedSail "VI" -ExpectedOsoCount 24

Test-SAILEndpoint -TestName "GRC 8, ARC-a → Category C" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 8
    residualArc = "a"
} -ExpectedCategory "C"

# SORA 2.5 Golden Cases  
Write-Host "`n=== SORA 2.5 GOLDEN CASES ===" -ForegroundColor Yellow

Test-SAILEndpoint -TestName "GRC 6, ARC 4 → SAIL V" -RequestBody @{
    soraVersion = "2.5"
    finalGrc = 6
    residualArcLevel = 4
} -ExpectedSail "V"

Test-SAILEndpoint -TestName "GRC 10, ARC 10 → SAIL VI (high-GRC rule)" -RequestBody @{
    soraVersion = "2.5"
    finalGrc = 10
    residualArcLevel = 10
} -ExpectedSail "VI"

Test-SAILEndpoint -TestName "GRC 9, ARC 1 → SAIL VI (high-GRC rule)" -RequestBody @{
    soraVersion = "2.5"
    finalGrc = 9
    residualArcLevel = 1
} -ExpectedSail "VI"

# Negative Tests
Write-Host "`n=== NEGATIVE TESTS ===" -ForegroundColor Yellow

Test-SAILEndpoint -TestName "SORA 2.5 without residualArcLevel" -RequestBody @{
    soraVersion = "2.5"
    finalGrc = 5
} -ExpectedStatusCode 400 -TestType "Negative"

Test-SAILEndpoint -TestName "SORA 2.0 with numeric ARC" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 5
    residualArcLevel = 3
} -ExpectedStatusCode 400 -TestType "Negative"

Test-SAILEndpoint -TestName "SORA 2.5 with letter ARC" -RequestBody @{
    soraVersion = "2.5"
    finalGrc = 5
    residualArc = "b"
    residualArcLevel = 3
} -ExpectedStatusCode 400 -TestType "Negative"

Test-SAILEndpoint -TestName "Invalid SORA version" -RequestBody @{
    soraVersion = "3.0"
    finalGrc = 5
    residualArc = "a"
} -ExpectedStatusCode 400 -TestType "Negative"

Test-SAILEndpoint -TestName "SORA 2.0 GRC out of range (0)" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 0
    residualArc = "a"
} -ExpectedStatusCode 400 -TestType "Negative"

Test-SAILEndpoint -TestName "SORA 2.5 GRC out of range (11)" -RequestBody @{
    soraVersion = "2.5"
    finalGrc = 11
    residualArcLevel = 5
} -ExpectedStatusCode 400 -TestType "Negative"

# Additional 2.0 Matrix Tests
Write-Host "`n=== ADDITIONAL SORA 2.0 MATRIX TESTS ===" -ForegroundColor Yellow

Test-SAILEndpoint -TestName "GRC 1, ARC-a → SAIL I" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 1
    residualArc = "a"
} -ExpectedSail "I" -ExpectedOsoCount 6

Test-SAILEndpoint -TestName "GRC 2, ARC-d → SAIL III" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 2
    residualArc = "d"
} -ExpectedSail "III" -ExpectedOsoCount 15

Test-SAILEndpoint -TestName "GRC 7, ARC-d → SAIL VI" -RequestBody @{
    soraVersion = "2.0"
    finalGrc = 7
    residualArc = "d"
} -ExpectedSail "VI" -ExpectedOsoCount 24

# Report Results
Write-Host "`n=== TEST RESULTS SUMMARY ===" -ForegroundColor Cyan

$totalTests = $script:TestResults.Sora20Passed + $script:TestResults.Sora20Failed + 
              $script:TestResults.Sora25Passed + $script:TestResults.Sora25Failed +
              $script:TestResults.NegativeTestsPassed + $script:TestResults.NegativeTestsFailed

$totalPassed = $script:TestResults.Sora20Passed + $script:TestResults.Sora25Passed + $script:TestResults.NegativeTestsPassed
$totalFailed = $script:TestResults.Sora20Failed + $script:TestResults.Sora25Failed + $script:TestResults.NegativeTestsFailed

Write-Host "SORA 2.0 Tests: $($script:TestResults.Sora20Passed) passed, $($script:TestResults.Sora20Failed) failed" -ForegroundColor $(if ($script:TestResults.Sora20Failed -eq 0) { "Green" } else { "Red" })
Write-Host "SORA 2.5 Tests: $($script:TestResults.Sora25Passed) passed, $($script:TestResults.Sora25Failed) failed" -ForegroundColor $(if ($script:TestResults.Sora25Failed -eq 0) { "Green" } else { "Red" })
Write-Host "Negative Tests: $($script:TestResults.NegativeTestsPassed) passed, $($script:TestResults.NegativeTestsFailed) failed" -ForegroundColor $(if ($script:TestResults.NegativeTestsFailed -eq 0) { "Green" } else { "Red" })

Write-Host "`nOVERALL: $totalPassed/$totalTests tests passed" -ForegroundColor $(if ($totalFailed -eq 0) { "Green" } else { "Red" })

if ($totalFailed -gt 0) {
    Write-Host "`nTest failures detected. Check API endpoint: $ApiBaseUrl/api/sail/calculate" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "`n🎉 All tests passed! SORA SAIL calculations are working correctly." -ForegroundColor Green
    exit 0
}

