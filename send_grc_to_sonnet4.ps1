# Send GRC Implementation Specification to Claude Sonnet 4
# Similar to send_arc_to_sonnet4.ps1

$ErrorActionPreference = "Stop"

Write-Host "🚀 Sending GRC specification to Claude Sonnet 4..." -ForegroundColor Cyan
Write-Host ""

# Read the specification
$specPath = "GRC_IMPLEMENTATION_SPECIFICATION.md"
$specContent = Get-Content $specPath -Raw -Encoding UTF8

Write-Host "📄 Spec size: $($specContent.Length.ToString('N0')) characters" -ForegroundColor Yellow

# Prepare the prompt
$prompt = @"
You are Claude Sonnet 4, an expert Python developer implementing drone safety calculations.

Below is a comprehensive GRC (Ground Risk Class) implementation specification for SORA 2.0 and 2.5.

YOUR TASK:
1. Read the CRITICAL IMPLEMENTATION RULES at the top
2. Study the complete specification and ALL embedded reference files
3. Implement complete Python code for GRC calculations
4. Include: YAML rules, Pydantic models, calculator classes, validators, API endpoints, comprehensive tests
5. Follow EXACTLY the requirements for M1 floor cap algorithm, N/A validation, and traceability

CRITICAL FOCUS AREAS:
- M1 floor cap algorithm (most complex part)
- SORA 2.5 N/A constraint validation
- Integer values only (no fractional reductions)
- Sequential application: iGRC → M1(+floor) → M2 → M3 → max(1, result)
- Full traceability with doc_ref in every trace entry

OUTPUT FORMAT:
- Provide complete, production-ready Python code
- Use Pydantic V2 (field_validator, model_config = ConfigDict, model_dump())
- Create all files from Section 10 deliverables checklist
- Include golden test cases from Section 7

---

$specContent
"@

Write-Host "📊 Total prompt: $($prompt.Length.ToString('N0')) characters" -ForegroundColor Yellow
Write-Host ""

# API configuration
$apiKey = $env:ANTHROPIC_API_KEY
if (-not $apiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable."
}
$headers = @{
    "x-api-key" = $apiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

# Request body
$body = @{
    model = "claude-sonnet-4-20250514"
    max_tokens = 16000
    temperature = 0
    messages = @(
        @{
            role = "user"
            content = $prompt
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "⏳ Calling Sonnet 4 API..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod `
        -Uri "https://api.anthropic.com/v1/messages" `
        -Method Post `
        -Headers $headers `
        -Body $body `
        -TimeoutSec 300

    # Extract response text
    $responseText = $response.content[0].text
    $inputTokens = $response.usage.input_tokens
    $outputTokens = $response.usage.output_tokens

    # Save response
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $responsePath = "SONNET_GRC_RESPONSE_$timestamp.txt"
    $responseText | Out-File -FilePath $responsePath -Encoding UTF8

    # Calculate cost (Sonnet 4 pricing: $3/MTok input, $15/MTok output)
    $inputCost = ($inputTokens / 1000000.0) * 3.0
    $outputCost = ($outputTokens / 1000000.0) * 15.0
    $totalCost = $inputCost + $outputCost

    Write-Host ""
    Write-Host "✅ SUCCESS!" -ForegroundColor Green
    Write-Host "Response saved to: $responsePath" -ForegroundColor Green
    Write-Host "Response size: $($responseText.Length.ToString('N0')) characters" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📊 Token Usage:" -ForegroundColor Cyan
    Write-Host "  Input:  $($inputTokens.ToString('N0')) tokens (`$$($inputCost.ToString('F3')))" -ForegroundColor White
    Write-Host "  Output: $($outputTokens.ToString('N0')) tokens (`$$($outputCost.ToString('F3')))" -ForegroundColor White
    Write-Host "  TOTAL:  `$$($totalCost.ToString('F3'))" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📝 Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Review the response in $responsePath" -ForegroundColor White
    Write-Host "  2. Extract implementation files to Backend_Python/grc/" -ForegroundColor White
    Write-Host "  3. Run tests: pytest grc/tests/ -v" -ForegroundColor White

} catch {
    Write-Host ""
    Write-Host "❌ ERROR!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.ErrorDetails.Message) {
        Write-Host ""
        Write-Host "API Error Details:" -ForegroundColor Yellow
        Write-Host $_.ErrorDetails.Message -ForegroundColor White
    }
    
    exit 1
}
