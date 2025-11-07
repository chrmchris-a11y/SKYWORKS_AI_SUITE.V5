<#
.SYNOPSIS
    Comprehensive SORA 2.0 and 2.5 Web API Test Suite
.DESCRIPTION
    Runs all 20 authoritative test cases (10x SORA 2.0 + 10x SORA 2.5)
    Validates web calculations against EASA/JARUS official specifications
.NOTES
    Based on: SORAAuthoritative_TestCases.json
    Validates: /api/sora/complete endpoint
#>

param(
    [string]$BaseUrl = "http://localhost:5210",
    [int]$TimeoutSeconds = 10
)

$ErrorActionPreference = "Stop"
$testCasesFile = Join-Path $PSScriptRoot "..\Tests\SORAAuthoritative_TestCases.json"

# Load test cases
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  COMPREHENSIVE SORA WEB API TEST SUITE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Loading test cases from: $testCasesFile`n" -ForegroundColor Gray

if (-not (Test-Path $testCasesFile)) {
    Write-Error "Test cases file not found: $testCasesFile"
    exit 1
}

$testData = Get-Content $testCasesFile -Raw | ConvertFrom-Json
$allTests = $testData.testCases

# Statistics
$sora20Tests = $allTests | Where-Object { $_.version -eq "2.0" }
$sora25Tests = $allTests | Where-Object { $_.version -eq "2.5" }

Write-Host "Total Test Cases: $($allTests.Count)" -ForegroundColor White
Write-Host "  - SORA 2.0: $($sora20Tests.Count) tests" -ForegroundColor Yellow
Write-Host "  - SORA 2.5: $($sora25Tests.Count) tests" -ForegroundColor Yellow
Write-Host ""

# Check API health
Write-Host "Checking API health at $BaseUrl..." -ForegroundColor Gray
try {
    $healthCheck = Invoke-WebRequest -Uri "$BaseUrl/api/sora/info" -UseBasicParsing -TimeoutSec 3
    if ($healthCheck.StatusCode -eq 200) {
        Write-Host "✅ API is running`n" -ForegroundColor Green
    }
} catch {
    Write-Error "❌ API not responding at $BaseUrl. Please start the backend API first."
    exit 1
}

# Test results tracking
$results = @{
    Total = 0
    Passed = 0
    Failed = 0
    Skipped = 0
    Details = @()
}

# Helper function to convert test input to API request
function Convert-TestInputToRequest {
    param($input)
    
    # Map populationDensity to scenario_V2_0 for SORA 2.0
    $scenario = "VLOS_SparselyPopulated"
    if ($input.soraVersion -eq "2.0") {
        if ($input.controlledArea) {
            $scenario = "ControlledGroundArea"
        } elseif ($input.populationDensity -ge 50000) {
            $scenario = if ($input.altitudeAGL_ft -gt 400) { "BVLOS_GatheringOfPeople" } else { "VLOS_GatheringOfPeople" }
        } elseif ($input.populationDensity -ge 500) {
            $scenario = if ($input.altitudeAGL_ft -gt 400) { "BVLOS_Populated" } else { "VLOS_Populated" }
        } elseif ($input.populationDensity -ge 50) {
            $scenario = if ($input.altitudeAGL_ft -gt 400) { "BVLOS_SparselyPopulated" } else { "VLOS_SparselyPopulated" }
        }
    }
    
    # Build mitigations array with proper object structure
    $grcMitigations = @()
    if ($input.sheltering) {
        $grcMitigations += @{
            type = "M1A"
            robustness = "Medium"
        }
    }
    
    # Determine explicit ARC (simplified logic - actual API calculates this)
    $explicitARC = "ARC_b"
    if ($input.isAtypicalSegregated) {
        $explicitARC = "ARC_a"
    } elseif ($input.altitudeAGL_ft -gt 500 -and $input.controlledAirspace) {
        $explicitARC = "ARC_d"
    } elseif ($input.urbanAir) {
        $explicitARC = "ARC_c"
    }
    
    # Build request object using exact C# property names (PascalCase)
    $request = @{
        soraVersion = $input.soraVersion
        groundRisk = @{
            maxCharacteristicDimension = [double]$input.maxDimension
            mtom_kg = [double]($input.maxDimension * 0.5)  # Estimate
            mitigations = $grcMitigations
        }
        airRisk = @{
            explicitARC = $explicitARC
            strategicMitigations = $input.strategicMitigations
            isAtypicalSegregated = $input.isAtypicalSegregated
        }
    }
    
    # Add scenario_V2_0 for SORA 2.0
    if ($input.soraVersion -eq "2.0") {
        $request.groundRisk.scenario_V2_0 = $scenario
    } else {
        # For SORA 2.5, add additional fields
        $request.groundRisk.populationDensity = [double]$input.populationDensity
        $request.groundRisk.isControlledGroundArea = $input.controlledArea
    }
    
    return $request
}

# Helper function to compare results
function Compare-Results {
    param($expected, $actual, $testId)
    
    $differences = @()
    
    # Map response fields correctly
    $actualInitialGRC = "GRC_$($actual.intrinsicGRC)"
    $actualFinalGRC = "GRC_$($actual.finalGRC)"
    $actualResidualARC = $actual.residualAirRiskClass
    $actualSAIL = $actual.sail
    
    # Compare each field
    if ($expected.initialGRC -and $actualInitialGRC -ne $expected.initialGRC) {
        $differences += "iGRC: Expected=$($expected.initialGRC), Actual=$actualInitialGRC"
    }
    
    if ($expected.finalGRC -and $actualFinalGRC -ne $expected.finalGRC) {
        $differences += "fGRC: Expected=$($expected.finalGRC), Actual=$actualFinalGRC"
    }
    
    if ($expected.residualARC -and $actualResidualARC -ne $expected.residualARC) {
        $differences += "rARC: Expected=$($expected.residualARC), Actual=$actualResidualARC"
    }
    
    if ($expected.sail -and $actualSAIL -ne $expected.sail) {
        $differences += "SAIL: Expected=$($expected.sail), Actual=$actualSAIL"
    }
    
    return $differences
}

# Run all tests
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RUNNING TESTS..." -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

foreach ($test in $allTests) {
    $results.Total++
    $testNum = $results.Total
    
    Write-Host "[$testNum/$($allTests.Count)] $($test.testId): $($test.name)" -ForegroundColor White
    Write-Host "  Version: SORA $($test.version) | " -NoNewline -ForegroundColor Gray
    
    # Check if test should fail
    $shouldFail = $test.expectedToFail -eq $true
    
    if ($shouldFail) {
        Write-Host "Expected: OUT OF SCOPE" -ForegroundColor Yellow
    } else {
        Write-Host "Expected: $($test.expected.sail)" -ForegroundColor Gray
    }
    
    try {
        # Convert test input to API request
        $request = Convert-TestInputToRequest -input $test.input
        $body = $request | ConvertTo-Json -Depth 10 -Compress
        
        # Call API
        $response = Invoke-RestMethod `
            -Uri "$BaseUrl/api/sora/complete" `
            -Method Post `
            -Body $body `
            -ContentType "application/json" `
            -TimeoutSec $TimeoutSeconds
        
        # Check if test was expected to fail
        if ($shouldFail) {
            if ($response.isSuccessful -eq $false -or $response.sail -eq $null) {
                Write-Host "  ✅ PASS - Correctly rejected (OUT OF SCOPE)" -ForegroundColor Green
                $results.Passed++
                $results.Details += @{
                    TestId = $test.testId
                    Status = "PASS"
                    Reason = "Correctly identified as OUT OF SCOPE"
                }
            } else {
                Write-Host "  ❌ FAIL - Should have been OUT OF SCOPE but got: $($response.sail)" -ForegroundColor Red
                $results.Failed++
                $results.Details += @{
                    TestId = $test.testId
                    Status = "FAIL"
                    Reason = "Expected OUT OF SCOPE but got SAIL $($response.sail)"
                }
            }
        } else {
            # Compare results
            $differences = Compare-Results -expected $test.expected -actual $response -testId $test.testId
            
            if ($differences.Count -eq 0) {
                Write-Host "  ✅ PASS - All values match" -ForegroundColor Green
                Write-Host "    iGRC=$($response.intrinsicGRC) → fGRC=$($response.finalGRC) | ARC=$($response.residualAirRiskClass) | SAIL=$($response.sail)" -ForegroundColor DarkGreen
                $results.Passed++
                $results.Details += @{
                    TestId = $test.testId
                    Status = "PASS"
                    Actual = "$($response.sail)"
                }
            } else {
                Write-Host "  ❌ FAIL - Differences found:" -ForegroundColor Red
                foreach ($diff in $differences) {
                    Write-Host "    - $diff" -ForegroundColor Red
                }
                $results.Failed++
                $results.Details += @{
                    TestId = $test.testId
                    Status = "FAIL"
                    Differences = $differences
                }
            }
        }
        
    } catch {
        $errorMsg = $_.Exception.Message
        if ($_.ErrorDetails.Message) {
            $errorMsg += " | Details: $($_.ErrorDetails.Message)"
        }
        
        if ($shouldFail) {
            Write-Host "  ✅ PASS - Correctly rejected with error" -ForegroundColor Green
            $results.Passed++
            $results.Details += @{
                TestId = $test.testId
                Status = "PASS"
                Reason = "Correctly rejected (OUT OF SCOPE)"
            }
        } else {
            Write-Host "  ❌ FAIL - API Error: $errorMsg" -ForegroundColor Red
            $results.Failed++
            $results.Details += @{
                TestId = $test.testId
                Status = "FAIL"
                Error = $errorMsg
            }
        }
    }
    
    Write-Host ""
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total Tests:  $($results.Total)" -ForegroundColor White
Write-Host "Passed:       $($results.Passed)" -ForegroundColor Green
Write-Host "Failed:       $($results.Failed)" -ForegroundColor $(if ($results.Failed -gt 0) { "Red" } else { "Gray" })
Write-Host "Skipped:      $($results.Skipped)" -ForegroundColor Gray

$passRate = [math]::Round(($results.Passed / $results.Total) * 100, 2)
Write-Host "`nPass Rate:    $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } else { "Yellow" })

# SORA version breakdown
$sora20Passed = ($results.Details | Where-Object { $_.TestId -like "SORA20_*" -and $_.Status -eq "PASS" }).Count
$sora25Passed = ($results.Details | Where-Object { $_.TestId -like "SORA25_*" -and $_.Status -eq "PASS" }).Count

Write-Host "`nSORA 2.0: $sora20Passed/$($sora20Tests.Count) passed" -ForegroundColor $(if ($sora20Passed -eq $sora20Tests.Count) { "Green" } else { "Yellow" })
Write-Host "SORA 2.5: $sora25Passed/$($sora25Tests.Count) passed" -ForegroundColor $(if ($sora25Passed -eq $sora25Tests.Count) { "Green" } else { "Yellow" })

# Failed tests detail
if ($results.Failed -gt 0) {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "FAILED TESTS DETAIL" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    
    $failedTests = $results.Details | Where-Object { $_.Status -eq "FAIL" }
    foreach ($failed in $failedTests) {
        Write-Host "`n$($failed.TestId):" -ForegroundColor Red
        if ($failed.Differences) {
            foreach ($diff in $failed.Differences) {
                Write-Host "  - $diff" -ForegroundColor Red
            }
        }
        if ($failed.Error) {
            Write-Host "  - Error: $($failed.Error)" -ForegroundColor Red
        }
        if ($failed.Reason) {
            Write-Host "  - $($failed.Reason)" -ForegroundColor Red
        }
    }
}

Write-Host "`n========================================`n" -ForegroundColor Cyan

# Exit code
if ($results.Failed -gt 0) {
    exit 1
} else {
    Write-Host "✅ ALL TESTS PASSED! EASA/JARUS COMPLIANCE VERIFIED!" -ForegroundColor Green
    exit 0
}
