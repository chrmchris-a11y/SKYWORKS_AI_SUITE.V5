# Send SORA Consistency Issue to Claude Sonnet 4
# Run this script to automatically send the request and get the response

$apiKey = $env:ANTHROPIC_API_KEY
if (-not $apiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable."
}
$endpoint = "https://api.anthropic.com/v1/messages"

# Read the request document
$requestDoc = Get-Content "SONNET_SORA_CONSISTENCY_REQUEST.md" -Raw

# Prepare API request
$headers = @{
    "x-api-key" = $apiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = "claude-sonnet-4-20250514"
    max_tokens = 8000
    messages = @(
        @{
            role = "user"
            content = @"
You are a JARUS SORA compliance expert. I need you to analyze calculation inconsistencies between SORA 2.0 and SORA 2.5 implementations.

**CRITICAL REQUIREMENTS:**
1. Provide EXACT code fixes (not pseudocode)
2. Explain WHY each fix is needed
3. Show calculation steps for validation
4. Do NOT suggest workarounds - fix the root cause
5. Ensure both versions produce consistent results for same inputs

Here is the complete analysis:

$requestDoc

Please provide:
1. Root cause analysis for each bug
2. Exact Python code fixes for Backend_Python/main.py
3. Exact C# code fixes for SORAOrchestrationService.cs
4. Validation test cases with expected outputs
5. Explanation of expected differences between SORA 2.0 and 2.5
"@
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "🚀 Sending request to Claude Sonnet 4..." -ForegroundColor Cyan
Write-Host "📊 Estimated cost: `$0.15-0.20" -ForegroundColor Yellow
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method Post -Headers $headers -Body $body
    
    # Extract response content
    $content = $response.content[0].text
    
    # Save response to file
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $outputFile = "SONNET_SORA_CONSISTENCY_RESPONSE_$timestamp.txt"
    $content | Out-File $outputFile -Encoding UTF8
    
    Write-Host "✅ Response received!" -ForegroundColor Green
    Write-Host "📝 Saved to: $outputFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "💰 Usage:" -ForegroundColor Cyan
    Write-Host "   Input tokens: $($response.usage.input_tokens)" -ForegroundColor White
    Write-Host "   Output tokens: $($response.usage.output_tokens)" -ForegroundColor White
    
    # Calculate cost
    $inputCost = ($response.usage.input_tokens / 1000000) * 3.00
    $outputCost = ($response.usage.output_tokens / 1000000) * 15.00
    $totalCost = $inputCost + $outputCost
    
    Write-Host "   Total cost: `$$([math]::Round($totalCost, 4))" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📄 Response preview:" -ForegroundColor Cyan
    Write-Host "----------------------------------------" -ForegroundColor Gray
    Write-Host ($content -split "`n" | Select-Object -First 50 | Out-String) -ForegroundColor White
    Write-Host "----------------------------------------" -ForegroundColor Gray
    Write-Host ""
    Write-Host "✅ Full response saved to: $outputFile" -ForegroundColor Green
    Write-Host "Open the file to see complete analysis and code fixes." -ForegroundColor Yellow
    
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Response details:" -ForegroundColor Yellow
    Write-Host $_.Exception.Response -ForegroundColor Gray
}
