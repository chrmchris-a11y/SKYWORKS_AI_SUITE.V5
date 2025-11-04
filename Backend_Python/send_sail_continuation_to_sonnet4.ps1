#!/usr/bin/env pwsh
# SAIL Implementation - Continuation Request for Missing Files
# Requests the remaining 7 files from Sonnet 4

$ErrorActionPreference = "Stop"

# Check API key
if (-not $env:ANTHROPIC_API_KEY) {
    Write-Host "❌ ERROR: ANTHROPIC_API_KEY not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:ANTHROPIC_API_KEY = 'sk-ant-...'" -ForegroundColor Yellow
    exit 1
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SAIL IMPLEMENTATION - CONTINUATION REQUEST" -ForegroundColor Cyan
Write-Host "  Missing 7 Files from Previous Response" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Create continuation prompt
$prompt = @"
Your previous response was cut off at 16,000 tokens. You successfully provided these 6 files:

✅ sail/models/sail_models.py
✅ sail/calculators/sail_calculator.py
✅ sail/calculators/oso_mapper.py
✅ sail/validators/sail_validator.py
✅ sail/data/sail_tables_20.py
✅ sail/data/sail_tables_25.py

Please provide the REMAINING 7 files from the SAIL_IMPLEMENTATION_SPECIFICATION.md:

🔴 MISSING FILES (7 total):
1. sail/data/oso_requirements.py
   - OSO #1-#24 descriptions
   - Robustness levels per SAIL
   - SORA 2.0 vs 2.5 differences

2. sail/api/sail_api.py
   - FastAPI endpoints: /sail/calculate, /sail/oso-requirements
   - Both SORA 2.0 and 2.5 routes

3. sail/tests/test_sail_calculator_20.py
   - 32 tests: All GRC (1-8) × ARC (a-d) combinations for SORA 2.0
   - Verify SAIL levels (I-VI)
   - Verify OSO counts (6,10,15,18,21,24)

4. sail/tests/test_sail_calculator_25.py
   - 32 tests: All GRC (1-8) × ARC (a-d) combinations for SORA 2.5
   - Verify differences at GRC 6-8 (more stringent than 2.0)

5. sail/tests/test_oso_mapping.py
   - Test OSO requirements for each SAIL level
   - Test robustness level assignments

6. sail/tests/test_sail_validation.py
   - Test input validation (GRC 1-8, ARC a-d)
   - Test error cases

7. sail/__init__.py
   - Package initialization and exports

CRITICAL REQUIREMENTS:
- Use Pydantic V2 (model_config, ConfigDict, not class Config)
- Follow same patterns as existing 6 files
- Include full doc_references for traceability
- pytest-compatible tests using exact values from SAIL_FORMULAS_AUTHORITATIVE.md
- NO placeholders, NO "...existing code...", NO comments like "add more tests"

SAIL MAPPING TABLES (from spec):

SORA 2.0 (EASA AMC/GM):
GRC 1-2: a=I, b=I, c=II, d=III
GRC 3-4: a=II, b=II, c=III, d=IV
GRC 5-6: a=III, b=III, c=IV, d=V
GRC 7-8: a=IV, b=IV, c=V, d=VI

SORA 2.5 (JARUS) - DIFFERENCES:
GRC 6: a=IV, b=IV, c=V, d=VI (not III/III/IV/V)
GRC 7-8: a=V, b=V, c=VI, d=VI (not IV/IV/V/VI)

OSO Requirements:
SAIL I = 6 OSOs, II = 10 OSOs, III = 15 OSOs
SAIL IV = 18 OSOs, V = 21 OSOs, VI = 24 OSOs

Generate ONLY these 7 files with complete implementations. Start each file with:
=== FILE: <path> ===
"@

Write-Host "📝 Continuation prompt created" -ForegroundColor Green
Write-Host "🔢 Estimated tokens: ~1500" -ForegroundColor Cyan
Write-Host ""
Write-Host "🤖 Calling Claude Sonnet 4..." -ForegroundColor Yellow

# Prepare API request
$headers = @{
    "x-api-key" = $env:ANTHROPIC_API_KEY
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = "claude-sonnet-4-20250514"
    max_tokens = 16000
    messages = @(
        @{
            role = "user"
            content = $prompt
        }
    )
} | ConvertTo-Json -Depth 10

try {
    # Call API
    $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" `
        -Method Post `
        -Headers $headers `
        -Body $body `
        -TimeoutSec 300

    Write-Host ""
    Write-Host "✅ Response received!" -ForegroundColor Green
    
    # Extract metrics
    $inputTokens = $response.usage.input_tokens
    $outputTokens = $response.usage.output_tokens
    
    # Calculate cost (Sonnet 4: $3/1M input, $15/1M output)
    $inputCost = ($inputTokens / 1000000) * 3
    $outputCost = ($outputTokens / 1000000) * 15
    $totalCost = $inputCost + $outputCost
    
    Write-Host "📊 Input tokens: $inputTokens" -ForegroundColor Cyan
    Write-Host "📊 Output tokens: $outputTokens" -ForegroundColor Cyan
    Write-Host ""
    Write-Host ("💰 Estimated cost: `${0:F3}" -f $totalCost) -ForegroundColor Yellow
    Write-Host ""
    
    # Save response
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $outputFile = "SAIL_CONTINUATION_SONNET4_RESPONSE_$timestamp.txt"
    $response.content[0].text | Out-File -FilePath $outputFile -Encoding utf8
    
    Write-Host "💾 Response saved to: $outputFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  NEXT STEPS" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Extract 7 files from: $outputFile" -ForegroundColor White
    Write-Host "2. Combine with previous 6 files" -ForegroundColor White
    Write-Host "3. Run: pytest sail/tests/ -v" -ForegroundColor White
    Write-Host "4. Create verify_sail_calculations.py" -ForegroundColor White
    Write-Host ""
    Write-Host "Response file: $outputFile" -ForegroundColor Cyan
    
} catch {
    Write-Host ""
    Write-Host "❌ API call failed!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.ErrorDetails.Message) {
        $errorJson = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host ""
        Write-Host "Error details:" -ForegroundColor Yellow
        Write-Host ($errorJson | ConvertTo-Json -Depth 10) -ForegroundColor Yellow
    }
    
    exit 1
}
