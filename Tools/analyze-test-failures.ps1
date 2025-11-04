#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Analyzes test failures and sends detailed prompt to Claude Sonnet 4 for fixes
.DESCRIPTION
    Collects all test errors, analyzes patterns, and generates comprehensive fix request
#>

param(
    [string]$BackendPath = "$PSScriptRoot\..\Backend",
    [string]$ApiKey = $env:ANTHROPIC_API_KEY
)

if (-not $ApiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable or pass -ApiKey."
}

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  TEST FAILURE ANALYSIS & CLAUDE SONNET 4 FIX REQUEST" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Step 1: Run tests and capture detailed output
Write-Host "[1/4] Running tests with detailed output..." -ForegroundColor Yellow
Push-Location $BackendPath

$testOutput = dotnet test Skyworks.sln --verbosity normal --no-build 2>&1 | Out-String
$testSummary = $testOutput | Select-String -Pattern "(Passed:|Failed:|Total tests:)" | Out-String

Write-Host "Test Summary:" -ForegroundColor Green
Write-Host $testSummary

# Extract failure details
$failures = @()
$lines = $testOutput -split "`n"
$inFailure = $false
$currentFailure = ""

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    
    if ($line -match "Failed\s+(.+?)\s+\[") {
        $inFailure = $true
        $currentFailure = "TEST: $($matches[1])`n"
    }
    elseif ($inFailure) {
        $currentFailure += "$line`n"
        
        if ($line -match "Stack Trace:" -or $line -match "^\s*$") {
            # End of this failure, capture next 5 lines for context
            for ($j = 1; $j -le 5 -and ($i + $j) -lt $lines.Count; $j++) {
                $currentFailure += "$($lines[$i + $j])`n"
            }
            $failures += $currentFailure
            $inFailure = $false
            $currentFailure = ""
        }
    }
}

Pop-Location

Write-Host "`nFound $($failures.Count) test failures" -ForegroundColor Yellow

# Step 2: Analyze patterns
Write-Host "`n[2/4] Analyzing failure patterns..." -ForegroundColor Yellow

$patterns = @{
    "GRC Mismatch" = @()
    "BadRequest vs OK" = @()
    "Out of Scope" = @()
    "Other" = @()
}

foreach ($failure in $failures) {
    if ($failure -match "Expected.*GRC_\d.*Actual.*GRC_\d") {
        $patterns["GRC Mismatch"] += $failure
    }
    elseif ($failure -match "Expected: OK.*Actual:.*BadRequest") {
        $patterns["BadRequest vs OK"] += $failure
    }
    elseif ($failure -match "Expected: 400.*Actual: 200|Expected: 200.*Actual: 400") {
        $patterns["Out of Scope"] += $failure
    }
    else {
        $patterns["Other"] += $failure
    }
}

Write-Host "Pattern Distribution:" -ForegroundColor Green
foreach ($pattern in $patterns.Keys) {
    $count = $patterns[$pattern].Count
    Write-Host "  - ${pattern}: $count" -ForegroundColor Cyan
}

# Step 3: Build comprehensive prompt for Claude Sonnet 4
Write-Host "`n[3/4] Building detailed prompt for Claude Sonnet 4..." -ForegroundColor Yellow

$prompt = @"
# SORA 2.0/2.5 Test Failure Analysis - Fix Request

## Context
I'm working on a C# .NET 8 backend with Python FastAPI integration for SORA (Specific Operations Risk Assessment) 2.0 and 2.5 compliance testing. The system has **248/257 tests passing (96.5%)** but **8 tests are failing** with specific patterns.

## System Architecture
- **C# Backend**: ASP.NET Core API (port 5210)
  - `SoraProxyController.cs`: Validated and compliant with all 13 EASA/JARUS fixes
  - `SORAOrchestrationService.cs`: Orchestrates GRC/ARC/SAIL calculations
  - Local orchestration for SORA 2.0, Python delegation for SORA 2.5
  
- **Python Backend**: FastAPI (port 8001)
  - `/api/v1/calculate/grc/2.0`: GRC calculation for SORA 2.0
  - `/api/v1/calculate/grc/2.5`: GRC calculation for SORA 2.5
  - Both endpoints working but returning unexpected values

- **Test Framework**: xUnit.net with integration tests
  - Tests validate against JARUS SORA 2.0/2.5 official specifications
  - Test data: `SORAAuthoritative_TestCases.v2.json`

## Test Failures Summary

### Total: 8 Failures

**Pattern 1: GRC Calculation Mismatch ($($patterns["GRC Mismatch"].Count) tests)**
Tests expect specific GRC values but Python endpoints return different values.

**Pattern 2: BadRequest vs OK ($($patterns["BadRequest vs OK"].Count) tests)**
Tests expect 200 OK but get 400 BadRequest (or vice versa).

**Pattern 3: Out-of-Scope Validation ($($patterns["Out of Scope"].Count) tests)**
Tests expect operation rejection (400) but system accepts it (200), or opposite.

**Pattern 4: Other ($($patterns["Other"].Count) tests)**

## Detailed Test Failures

$($failures -join "`n`n═══════════════════════════════════════════════════════`n`n")

## Recent Changes Made
1. ✅ Added `MTOM_kg` field to test request builders (SORAAuthoritative_E2E_Tests.cs)
2. ✅ Created `/api/v1/calculate/grc/2.0` Python endpoint with GRC logic
3. ✅ Created `/api/v1/calculate/grc/2.5` Python endpoint with GRC logic
4. ✅ Both endpoints tested manually and working (returning valid JSON responses)

## Key Files

### C# Test Builder (SORAAuthoritative_E2E_Tests.cs)
```csharp
private object BuildSORARequest(AuthoritativeTestCase testCase)
{
    // Derives MTOM from maxCharacteristicDimension
    double mtom_kg = input.MaxDimension switch
    {
        < 1.0 => 0.5,
        < 2.0 => 1.5,
        < 4.0 => 3.0,
        < 10.0 => 8.0,
        _ => 15.0
    };
    
    // Builds groundRisk object with mtom_kg included
    // Routes to /api/sail/calculate (SORA 2.0) or Python (SORA 2.5)
}
```

### Python GRC 2.0 Endpoint (main.py)
```python
@app.post("/api/v1/calculate/grc/2.0")
async def calculate_grc_v20_new(request: GrcRequest20):
    # Simplified GRC calculation based on population density and MTOM
    if request.population_density < 100:
        initial_grc = 1
    elif request.population_density < 1000:
        initial_grc = 2 if request.mtom_kg < 1 else 3
    elif request.population_density < 5000:
        initial_grc = 3 if request.mtom_kg < 2 else 4
    else:
        initial_grc = 5 if request.mtom_kg < 10 else 6
    
    # Apply mitigations (M1, M2, M3)
    # Returns: intrinsicGRC, finalGRC, notes, m1Effect, m2Effect, m3Effect
```

### Python GRC 2.5 Endpoint (main.py)
```python
@app.post("/api/v1/calculate/grc/2.5")
async def calculate_grc_v25(request: GrcRequest25):
    # Similar logic for SORA 2.5
    # Includes M1A sheltering and M2 impact mitigations
    # Returns: initial_grc, final_grc, reference
```

## Specific Questions

1. **GRC Calculation Logic**: Are the Python GRC thresholds and MTOM weighting correct per JARUS SORA 2.0/2.5 Annex B?
2. **Expected vs Actual Mismatches**: Why do tests expect GRC_6 but get GRC_4? Are test expectations wrong or calculations?
3. **Validation Logic**: Should certain high-risk scenarios (SAIL VI, GRC≥6) be automatically rejected with 400 BadRequest?
4. **SORA 2.0 vs 2.5 Differences**: Are there different GRC calculation rules between versions that I'm missing?

## What I Need

Please provide:
1. **Root cause analysis** for each failure pattern
2. **Specific code fixes** (with file paths and exact code changes)
3. **Explanation** of correct SORA 2.0/2.5 GRC calculation logic per JARUS specs
4. **Test expectation corrections** if test data is wrong

## Standards References
- JARUS SORA 2.0: JAR_doc_06 (Annex B for GRC)
- JARUS SORA 2.5: JAR_doc_25 (Enhanced GRC with buffer zones)
- EASA Easy Access Rules for UAS

Please analyze these failures and provide detailed fixes!
"@

# Step 4: Send to Claude Sonnet 4 via API
Write-Host "`n[4/4] Sending request to Claude Sonnet 4..." -ForegroundColor Yellow

$headers = @{
    "x-api-key" = $ApiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = "claude-opus-4-20250514"
    max_tokens = 8000
    messages = @(
        @{
            role = "user"
            content = $prompt
        }
    )
} | ConvertTo-Json -Depth 10

try {
    Write-Host "Calling Claude API..." -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" -Method Post -Headers $headers -Body $body
    
    $claudeResponse = $response.content[0].text
    
    Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "  CLAUDE SONNET 4 ANALYSIS & FIXES" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    Write-Host $claudeResponse -ForegroundColor White
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
    
    # Save response to file
    $outputPath = "$PSScriptRoot\claude-fix-response.md"
    $claudeResponse | Out-File -FilePath $outputPath -Encoding UTF8
    Write-Host "`nResponse saved to: $outputPath" -ForegroundColor Yellow
    
    return $claudeResponse
}
catch {
    Write-Host "`n❌ Error calling Claude API:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "`nRequest body preview:" -ForegroundColor Yellow
    Write-Host ($body | ConvertFrom-Json | ConvertTo-Json -Depth 3) -ForegroundColor Gray
    exit 1
}
