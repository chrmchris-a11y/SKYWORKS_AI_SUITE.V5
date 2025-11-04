# Send SAIL Implementation Specification to Claude Sonnet 4
# Similar to ARC ($0.297) and GRC ($0.287) - Expected cost ~$0.30

$ErrorActionPreference = "Stop"

# Configuration
$SPEC_FILE = "SAIL_IMPLEMENTATION_SPECIFICATION.md"
$MODEL = "claude-sonnet-4-20250514"
$MAX_TOKENS = 16000

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SAIL IMPLEMENTATION - SONNET 4 CODE GENERATION" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if spec file exists
if (-not (Test-Path $SPEC_FILE)) {
    Write-Host "❌ Error: $SPEC_FILE not found!" -ForegroundColor Red
    exit 1
}

# Get file size
$fileSize = (Get-Item $SPEC_FILE).Length
$fileSizeKB = [math]::Round($fileSize / 1024, 2)
Write-Host "📄 Specification File: $SPEC_FILE" -ForegroundColor Green
Write-Host "📊 File Size: $fileSizeKB KB" -ForegroundColor Green
Write-Host ""

# Read specification
Write-Host "📖 Reading specification..." -ForegroundColor Yellow
$specContent = Get-Content $SPEC_FILE -Raw -Encoding UTF8

# Count approximate tokens (rough estimate: 4 chars = 1 token)
$estimatedTokens = [math]::Ceiling($specContent.Length / 4)
Write-Host "🔢 Estimated Input Tokens: ~$estimatedTokens" -ForegroundColor Yellow
Write-Host ""

# Prepare prompt
$prompt = @"
You are an expert Python developer implementing SAIL (Specific Assurance and Integrity Level) calculation for drone operations per EASA AMC/GM and JARUS SORA standards.

# YOUR TASK

Implement **COMPLETE** SAIL calculation system based on the attached specification.

## IMPLEMENTATION REQUIREMENTS

1. **Generate ALL files** specified in Section 13 (File Structure):
   - sail/models/sail_models.py (Pydantic V2 models)
   - sail/calculators/sail_calculator.py (SAILCalculator20, SAILCalculator25)
   - sail/calculators/oso_mapper.py (OSO requirements)
   - sail/validators/sail_validator.py (Input/output validation)
   - sail/data/sail_tables_20.py (SORA 2.0 lookup tables)
   - sail/data/sail_tables_25.py (SORA 2.5 lookup tables)
   - sail/data/oso_requirements.py (OSO lists & robustness)
   - sail/api/sail_api.py (FastAPI endpoints)
   - sail/tests/test_sail_calculator_20.py (SORA 2.0 tests)
   - sail/tests/test_sail_calculator_25.py (SORA 2.5 tests)
   - sail/tests/test_oso_mapping.py (OSO tests)
   - sail/tests/test_sail_validation.py (Validation tests)

2. **CRITICAL RULES** (Section 1):
   - Pydantic V2 only (BaseModel, Field, field_validator, ConfigDict)
   - Exact field names (final_grc, final_arc, sail, oso_requirements, robustness_levels)
   - String enums (SAILValue.I = "I", ARCValue.A = "a")
   - Full traceability (TraceEntry with DocReference)

3. **SAIL MAPPING TABLES**:
   - SORA 2.0: Section 3 (8 GRC × 4 ARC → SAIL I-VI)
   - SORA 2.5: Section 4 (different mappings for GRC 6-8)
   - Use grouped GRC ranges for efficient lookup

4. **OSO REQUIREMENTS** (Section 5):
   - SAIL I: 6 OSOs
   - SAIL II: 10 OSOs
   - SAIL III: 15 OSOs
   - SAIL IV: 18 OSOs
   - SAIL V: 21 OSOs
   - SAIL VI: 24 OSOs

5. **COMPLETE TEST COVERAGE**:
   - All 32 SORA 2.0 combinations (8 GRC × 4 ARC)
   - All 32 SORA 2.5 combinations
   - OSO count verification for all 6 SAILs
   - Robustness level tests
   - Validation error handling

## OUTPUT FORMAT

For EACH file, output EXACTLY in this format:

```
=== FILE: sail/models/sail_models.py ===
[COMPLETE file content - no placeholders, no "..." shortcuts]
=== END FILE ===
```

## QUALITY REQUIREMENTS

- ✅ Type hints on all functions
- ✅ Docstrings for all classes/methods
- ✅ Full error handling
- ✅ Complete traceability (every calculation traced)
- ✅ No TODOs, no placeholders
- ✅ Production-ready code

## REFERENCE DOCUMENTS

The specification includes 4 embedded reference files (Section 12):
- SAIL_SORA20_Official_Logic.md (EASA AMC/GM)
- SAIL_SORA25_Official_Logic.md (JARUS SORA v2.5)
- SAIL_DROPDOWNS.json (UI/API schema)
- SAIL_FORMULAS_AUTHORITATIVE.md (584 lines - complete formulas)

---

# SPECIFICATION

$specContent
"@

Write-Host "🤖 Calling Claude Sonnet 4..." -ForegroundColor Cyan
Write-Host ""

# Prepare API request
$headers = @{
    "x-api-key" = $env:ANTHROPIC_API_KEY
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = $MODEL
    max_tokens = $MAX_TOKENS
    messages = @(
        @{
            role = "user"
            content = $prompt
        }
    )
} | ConvertTo-Json -Depth 10

# Call API
try {
    $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" `
        -Method Post `
        -Headers $headers `
        -Body $body `
        -ContentType "application/json"
    
    # Extract response
    $responseText = $response.content[0].text
    $inputTokens = $response.usage.input_tokens
    $outputTokens = $response.usage.output_tokens
    
    Write-Host "✅ Response received!" -ForegroundColor Green
    Write-Host "📊 Input tokens: $inputTokens" -ForegroundColor Yellow
    Write-Host "📊 Output tokens: $outputTokens" -ForegroundColor Yellow
    Write-Host ""
    
    # Calculate cost (Sonnet 4: $3/1M input, $15/1M output)
    $inputCost = ($inputTokens / 1000000) * 3
    $outputCost = ($outputTokens / 1000000) * 15
    $totalCost = $inputCost + $outputCost
    
    Write-Host "💰 Estimated cost: `$$($totalCost.ToString('F3'))" -ForegroundColor Cyan
    Write-Host ""
    
    # Save full response
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $responseFile = "SAIL_SONNET4_RESPONSE_$timestamp.txt"
    $responseText | Out-File -FilePath $responseFile -Encoding UTF8
    
    Write-Host "💾 Full response saved to: $responseFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  NEXT STEPS" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Extract files from response" -ForegroundColor Yellow
    Write-Host "2. Create sail/ directory structure" -ForegroundColor Yellow
    Write-Host "3. Save each file to correct location" -ForegroundColor Yellow
    Write-Host "4. Run: pytest sail/tests/ -v" -ForegroundColor Yellow
    Write-Host "5. Create verify_sail_calculations.py" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Response file: $responseFile" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error calling API:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
