#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sends ARC_IMPLEMENTATION_SPECIFICATION.md to Claude Sonnet 4 for implementation
.DESCRIPTION
    Reads the complete ARC specification and sends it to Claude Sonnet 4 via Anthropic API
    with clear instructions to implement all requirements.
#>

param(
    [string]$ApiKey = $env:ANTHROPIC_API_KEY,
    [string]$SpecFile = "ARC_IMPLEMENTATION_SPECIFICATION.md",
    [string]$OutputDir = "SONNET4_ARC_OUTPUT",
    [int]$MaxTokens = 16000
)

# Check prerequisites
if (-not $ApiKey) {
    Write-Error "❌ ANTHROPIC_API_KEY not found! Set it with: `$env:ANTHROPIC_API_KEY = 'your-key'"
    exit 1
}

if (-not (Test-Path $SpecFile)) {
    Write-Error "❌ Specification file not found: $SpecFile"
    exit 1
}

# Read specification
Write-Host "📖 Reading specification: $SpecFile" -ForegroundColor Cyan
$specContent = Get-Content $SpecFile -Raw
$specSize = (Get-Item $SpecFile).Length
$estimatedTokens = [math]::Round($specSize / 4)

Write-Host "📊 Specification Stats:" -ForegroundColor Yellow
Write-Host "   • Size: $([math]::Round($specSize/1KB, 2)) KB"
Write-Host "   • Estimated Input Tokens: ~$estimatedTokens"
Write-Host "   • Estimated Cost: ~$([math]::Round($estimatedTokens * 0.003 / 1000, 2)) USD (input only)"

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
    Write-Host "📁 Created output directory: $OutputDir" -ForegroundColor Green
}

# Prepare prompt for Sonnet 4
$systemPrompt = @"
You are an expert Python developer implementing SORA (Specific Operations Risk Assessment) calculations for drone operations according to EASA and JARUS standards.

Your task is to implement the COMPLETE ARC (Air Risk Class) calculation module based on the comprehensive specification provided by the user.

CRITICAL REQUIREMENTS:
1. Read the ENTIRE specification carefully - it contains ALL reference files embedded
2. Follow the CRITICAL IMPLEMENTATION RULES at the top of the document
3. Implement EXACTLY as specified - no shortcuts, no assumptions
4. Create YAML-driven rules (no hardcoded logic)
5. Use Pydantic models for all data structures
6. Include full traceability (rule_ref fields pointing to EASA/JARUS sources)
7. Implement ALL test cases from Section 7
8. Create ALL deliverables from Section 10 checklist

VERIFICATION BEFORE RESPONDING:
✓ Integer classes only (no 0.5 fractional reductions)
✓ DAA NOT in strategic ARC
✓ Thresholds extracted from official sources (no arbitrary values)
✓ Reduction caps enforced (≤1 or ≤2 with certification)
✓ U-space as supporting evidence (not automatic -1)
✓ Geo-fencing as supporting evidence only (0 direct reduction)
✓ Full trace references (doc_id + annex + section)

OUTPUT FORMAT:
Provide complete, working code organized exactly as specified in Section 10:
1. YAML rule files (arc_rules_sora_2_0.yaml, arc_rules_sora_2_5.yaml)
2. Python modules (arc_models.py, arc_calculator.py, arc_validator.py)
3. FastAPI endpoints (arc_api.py)
4. Test files (test_arc_calculator.py with golden tests + property-based tests)
5. README with usage examples

Start with SORA 2.0 implementation, then SORA 2.5.
"@

$userPrompt = @"
# ARC (Air Risk Class) Implementation Request

Please implement the complete ARC calculation module according to this specification:

---

$specContent

---

## Implementation Instructions

**Step 1: SORA 2.0 Implementation**
1. Create arc_rules_sora_2_0.yaml with Initial ARC determination rules + Strategic mitigation rules
2. Create arc_models.py with Pydantic models (ArcInputs_v2_0, ArcResult, etc.)
3. Create arc_calculator.py with ArcCalculator_v2_0 class
4. Create arc_validator.py with validation logic
5. Create test_arc_calculator_2_0.py with ALL test cases from Section 7

**Step 2: SORA 2.5 Implementation**
1. Create arc_rules_sora_2_5.yaml with enhanced mitigation mechanisms
2. Extend models for SORA 2.5 fields
3. Create ArcCalculator_v2_5 class
4. Create test_arc_calculator_2_5.py

**Step 3: FastAPI Integration**
1. Create arc_api.py with /sora/2.0/arc and /sora/2.5/arc endpoints
2. Include full traceability in responses

**Step 4: Documentation**
1. Create README_ARC.md with usage examples
2. Document all YAML rules
3. Provide example requests/responses

**CRITICAL REMINDERS:**
- ⚠️ Read the "CRITICAL IMPLEMENTATION RULES" section at the top of the specification FIRST
- ⚠️ All 6 reference files are embedded in Section 12 - use them for exact values
- ⚠️ Section 14 lists the redline corrections - make sure you implement them correctly
- ⚠️ Zero tolerance for deviations from EASA/JARUS standards

Please provide the complete implementation now.
"@

# Prepare API request
$headers = @{
    "x-api-key" = $ApiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = "claude-sonnet-4-20250514"
    max_tokens = $MaxTokens
    system = $systemPrompt
    messages = @(
        @{
            role = "user"
            content = $userPrompt
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host ""
Write-Host "🚀 Sending request to Claude Sonnet 4..." -ForegroundColor Cyan
Write-Host "   Model: claude-sonnet-4-20250514" -ForegroundColor Gray
Write-Host "   Max Output Tokens: $MaxTokens" -ForegroundColor Gray
Write-Host ""

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

try {
    # Send request
    $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" `
        -Method Post `
        -Headers $headers `
        -Body $body `
        -TimeoutSec 300

    # Save response
    $responseFile = Join-Path $OutputDir "sonnet4_arc_response_$timestamp.json"
    $response | ConvertTo-Json -Depth 10 | Set-Content $responseFile

    Write-Host "✅ Response received!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Usage Stats:" -ForegroundColor Yellow
    Write-Host "   • Input Tokens: $($response.usage.input_tokens)"
    Write-Host "   • Output Tokens: $($response.usage.output_tokens)"
    Write-Host "   • Total Tokens: $($response.usage.input_tokens + $response.usage.output_tokens)"
    
    $inputCost = $response.usage.input_tokens * 0.003 / 1000
    $outputCost = $response.usage.output_tokens * 0.015 / 1000
    $totalCost = $inputCost + $outputCost
    
    Write-Host "   • Input Cost: `$$([math]::Round($inputCost, 3))" -ForegroundColor Cyan
    Write-Host "   • Output Cost: `$$([math]::Round($outputCost, 3))" -ForegroundColor Cyan
    Write-Host "   • Total Cost: `$$([math]::Round($totalCost, 3))" -ForegroundColor Green
    Write-Host ""

    # Extract and save text content
    if ($response.content -and $response.content.Count -gt 0) {
        $textContent = ($response.content | Where-Object { $_.type -eq "text" } | ForEach-Object { $_.text }) -join "`n`n"
        
        $textFile = Join-Path $OutputDir "sonnet4_arc_implementation_$timestamp.md"
        $textContent | Set-Content $textFile -Encoding UTF8
        
        Write-Host "📄 Implementation saved to:" -ForegroundColor Green
        Write-Host "   $textFile" -ForegroundColor White
        Write-Host ""
        Write-Host "📝 Preview (first 1000 chars):" -ForegroundColor Yellow
        Write-Host "─" * 80 -ForegroundColor Gray
        Write-Host $textContent.Substring(0, [Math]::Min(1000, $textContent.Length))
        Write-Host "─" * 80 -ForegroundColor Gray
        Write-Host ""
        Write-Host "💡 Full implementation is in: $textFile" -ForegroundColor Cyan
    }

    # Save full response for debugging
    Write-Host ""
    Write-Host "📦 Full JSON response saved to:" -ForegroundColor Gray
    Write-Host "   $responseFile" -ForegroundColor White

} catch {
    Write-Error "❌ API request failed: $($_.Exception.Message)"
    
    # Save error details
    $errorFile = Join-Path $OutputDir "sonnet4_arc_error_$timestamp.txt"
    $_.Exception | Out-File $errorFile
    Write-Host "🔍 Error details saved to: $errorFile" -ForegroundColor Yellow
    
    exit 1
}

Write-Host ""
Write-Host "✅ COMPLETE! Review the implementation in $OutputDir" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Review the implementation in $textFile"
Write-Host "   2. Extract Python files from the response"
Write-Host "   3. Create Backend_Python/arc/ directory structure"
Write-Host "   4. Run tests to verify correctness"
Write-Host "   5. Deploy to FastAPI"
