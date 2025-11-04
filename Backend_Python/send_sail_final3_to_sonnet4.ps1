#!/usr/bin/env pwsh
# SAIL Implementation - Final 3 Files Request
# Requests only the last 3 missing files

$ErrorActionPreference = "Stop"

# Check API key
if (-not $env:ANTHROPIC_API_KEY) {
    Write-Host "❌ ERROR: ANTHROPIC_API_KEY not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:ANTHROPIC_API_KEY = 'sk-ant-...'" -ForegroundColor Yellow
    exit 1
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SAIL IMPLEMENTATION - FINAL 3 FILES" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Create final prompt
$prompt = @"
I have successfully received 10 out of 13 SAIL implementation files. I need only the FINAL 3 FILES:

✅ ALREADY RECEIVED (10 files):
- sail/models/sail_models.py
- sail/calculators/sail_calculator.py
- sail/calculators/oso_mapper.py
- sail/validators/sail_validator.py
- sail/data/sail_tables_20.py
- sail/data/sail_tables_25.py
- sail/data/oso_requirements.py
- sail/api/sail_api.py
- sail/tests/test_sail_calculator_20.py
- sail/tests/test_sail_calculator_25.py

🔴 NEED ONLY THESE 3 FILES:

1. **sail/tests/test_oso_mapping.py**
   - Test OSO requirements mapping for each SAIL level (I-VI)
   - Test robustness level assignments
   - Verify SAIL I=6 OSOs, II=10, III=15, IV=18, V=21, VI=24
   - Test both SORA 2.0 and SORA 2.5

2. **sail/tests/test_sail_validation.py**
   - Test input validation (GRC must be 1-8, ARC must be a-d)
   - Test error cases (invalid GRC, invalid ARC)
   - Test edge cases
   - Test both SORA 2.0 and SORA 2.5

3. **sail/__init__.py**
   - Package initialization
   - Export main classes: SAILCalculator20, SAILCalculator25, SAILInputs20, SAILInputs25, SAILResult
   - Export enums: SAILValue, ARCValue, RobustnessLevel
   - Simple, clean exports for easy imports

CRITICAL REQUIREMENTS:
- Use Pydantic V2 (model_config, ConfigDict, not class Config)
- pytest-compatible tests
- Import from existing modules (sail.models, sail.calculators, etc.)
- NO placeholders, NO "...existing code...", complete implementations
- Follow same patterns as test_sail_calculator_20.py and test_sail_calculator_25.py

Start each file with:
=== FILE: <path> ===

Generate ONLY these 3 files with complete implementations.
"@

Write-Host "📝 Final 3 files prompt created" -ForegroundColor Green
Write-Host "🔢 Estimated tokens: ~800" -ForegroundColor Cyan
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
    max_tokens = 8000
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
    $outputFile = "SAIL_FINAL3_SONNET4_RESPONSE_$timestamp.txt"
    $response.content[0].text | Out-File -FilePath $outputFile -Encoding utf8
    
    Write-Host "💾 Response saved to: $outputFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  NEXT STEPS" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Extract 3 files from: $outputFile" -ForegroundColor White
    Write-Host "2. All 13 SAIL files complete!" -ForegroundColor White
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
