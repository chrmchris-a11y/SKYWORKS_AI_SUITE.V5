param(
    [string]$ApiKey = $env:ANTHROPIC_API_KEY
)

if (-not $ApiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable or pass -ApiKey."
}

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SEND FULL FILES TO CLAUDE OPUS 4 FOR COMPLETE FIXES" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Read current Python main.py
Write-Host "[1/5] Reading Python main.py..." -ForegroundColor Yellow
$pythonMainPath = "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\main.py"
$pythonMainContent = Get-Content $pythonMainPath -Raw

# Read test case JSON
Write-Host "[2/5] Reading test cases..." -ForegroundColor Yellow
$testCasesPath = "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\tests\Skyworks.Api.Tests\TestData\SORAAuthoritative_TestCases.v2.json"
$testCasesContent = Get-Content $testCasesPath -Raw | ConvertFrom-Json
$relevantTests = $testCasesContent.testCases | Where-Object { 
    $_.name -match "Low Risk Rural|High Altitude.*Controlled|SORA 2.5"
} | Select-Object -First 5

# Run tests and get failures
Write-Host "[3/5] Running tests to capture failures..." -ForegroundColor Yellow
Push-Location "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend"
$testOutput = dotnet test Skyworks.sln --verbosity normal 2>&1 | Out-String
Pop-Location

# Extract failed test details
$failedTests = $testOutput -split "`n" | Where-Object { $_ -match "Failed.*SORA|Assert.Equal" }

# Build comprehensive prompt
Write-Host "[4/5] Building comprehensive prompt with full files..." -ForegroundColor Yellow

$prompt = @"
# COMPLETE FILE-LEVEL FIX REQUEST FOR SORA GRC CALCULATION

## Context
I have a C# .NET backend with Python FastAPI for SORA 2.0/2.5 compliance. Currently **248/257 tests pass** but **8 tests still fail**.

You previously provided code snippets, but I need **COMPLETE, FULL FILES** that I can copy-paste directly.

## Current Status
- ✅ Python GRC endpoints exist at `/api/v1/calculate/grc/2.0` and `/api/v1/calculate/grc/2.5`
- ✅ MTOM validation working
- ❌ 8 tests still failing (GRC calculation + out-of-scope validation)

## Test Failures Summary

### Pattern 1: GRC Calculation Mismatches (5 tests)
Tests expect specific GRC values based on JARUS specs but Python returns different values.

**Examples:**
- SORA20_001: population_density=10 → Expected GRC_3, Got GRC_2
- SORA20_006: population_density=5000 → Expected GRC_5, Got different value
- SORA25_002: population_density=3000 + sheltering → Expected GRC_6→GRC_5, Got GRC_4
- SORA25_007: population_density=250 → Expected GRC_5, Got GRC_3

### Pattern 2: Out-of-Scope Validation (3 tests)
High-risk operations (SAIL VI, GRC≥6) should return 400 BadRequest but system returns 200 OK.

**Examples:**
- GOLD-25-OOS-001: GRC=6 + ARC_d → Expected 400, Got 200
- IT-OOS-001: SAIL VI operation → Expected 400, Got 200

## Current Python main.py (FULL FILE)

\`\`\`python
$pythonMainContent
\`\`\`

## Sample Test Cases

\`\`\`json
$($relevantTests | ConvertTo-Json -Depth 10)
\`\`\`

## Recent Test Output (Last 1000 chars)

\`\`\`
$($testOutput.Substring([Math]::Max(0, $testOutput.Length - 1000)))
\`\`\`

## What I Need From You

**CRITICAL: I need COMPLETE, READY-TO-USE FILES, not code snippets!**

Please provide:

1. **FULL `main.py` file** (Backend_Python/main.py)
   - Complete file from line 1 to end
   - With correct JARUS GRC thresholds
   - With proper MTOM adjustments
   - All existing endpoints preserved
   - Ready to copy-paste directly

2. **FULL `SORAOrchestrationService.cs` file** (Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs)
   - Complete file with out-of-scope validation
   - Method `ValidateOperationScope(int finalGRC, string residualARC, string sail)`
   - Modified `ExecuteCompleteAsync` to check scope
   - All existing logic preserved

3. **FULL `SoraProxyController.cs` modifications** (Backend/src/Skyworks.Api/Controllers/SoraProxyController.cs)
   - Complete `[HttpPost("complete")]` method
   - BadRequest handling for out-of-scope
   - All existing validation preserved

4. **Test case corrections** (if needed)
   - Which test expectations are wrong per JARUS specs
   - Exact values that should be corrected

## JARUS SORA Specifications

### SORA 2.0 GRC Thresholds (JAR_doc_06 Annex B):
According to official JARUS documents:
- **GRC 1**: 0-1 people/km² (unpopulated)
- **GRC 2**: 1-10 people/km² (rural)
- **GRC 3**: 10-100 people/km² (suburban)
- **GRC 4**: 100-300 people/km² (urban)
- **GRC 5**: 300-1000 people/km² (dense urban)
- **GRC 6**: 1000+ people/km² (very dense urban)
- **GRC 7**: Reserved for exceptional cases

### MTOM Impact:
- **< 10 kg**: Use base GRC
- **10-25 kg**: +1 to GRC
- **≥ 25 kg**: +2 to GRC
- **Max GRC = 7**

### Out-of-Scope Rules:
1. **SAIL VI** → Reject (400 BadRequest) - requires CERTIFIED category
2. **GRC ≥ 6** → Reject (400 BadRequest) - exceptional risk
3. **GRC ≥ 5 + ARC_d** → Reject (400 BadRequest) - combined high risk

## Response Format Required

Please structure your response as:

### 1. Root Cause Analysis
Brief explanation of what's wrong

### 2. File: Backend_Python/main.py
\`\`\`python
# COMPLETE FILE CONTENTS HERE
# FROM LINE 1 TO END
# READY TO COPY-PASTE
\`\`\`

### 3. File: Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs
\`\`\`csharp
// COMPLETE FILE CONTENTS HERE
// FROM LINE 1 TO END
// READY TO COPY-PASTE
\`\`\`

### 4. File: Backend/src/Skyworks.Api/Controllers/SoraProxyController.cs
\`\`\`csharp
// COMPLETE FILE CONTENTS HERE (OR JUST THE MODIFIED METHOD)
\`\`\`

### 5. Test Corrections (if any)
List of test expectations that need adjustment

**IMPORTANT:** 
- Provide COMPLETE files, not snippets with "// existing code..."
- Include ALL imports, ALL methods, ALL logic
- Make files ready to copy-paste directly
- Preserve all existing functionality

Thank you!
"@

# Call Claude API
Write-Host "[5/5] Sending request to Claude Opus 4..." -ForegroundColor Yellow

$headers = @{
    "x-api-key" = $ApiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = "claude-opus-4-20250514"
    max_tokens = 16000  # Increased for full files
    messages = @(
        @{
            role = "user"
            content = $prompt
        }
    )
} | ConvertTo-Json -Depth 10

try {
    Write-Host "Calling Claude API with full file context..." -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" -Method Post -Headers $headers -Body $body
    
    $claudeResponse = $response.content[0].text
    
    Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "  CLAUDE OPUS 4 COMPLETE FILE FIXES" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════`n" -ForegroundColor Green
    
    Write-Output $claudeResponse
    
    # Save to file
    $outputPath = "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Tools\claude-complete-files.md"
    Set-Content -Path $outputPath -Value $claudeResponse -Encoding UTF8
    
    Write-Host "`n═══════════════════════════════════════════════════════`n" -ForegroundColor Green
    Write-Host "✅ Response saved to: $outputPath" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "1. Review the complete files above" -ForegroundColor White
    Write-Host "2. Copy-paste main.py to Backend_Python/main.py" -ForegroundColor White
    Write-Host "3. Copy-paste C# files to their locations" -ForegroundColor White
    Write-Host "4. Run: dotnet test Skyworks.sln" -ForegroundColor White
    
} catch {
    Write-Host "`n❌ Error calling Claude API:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "`nResponse body:" -ForegroundColor Yellow
        Write-Host $responseBody -ForegroundColor Gray
    }
}
