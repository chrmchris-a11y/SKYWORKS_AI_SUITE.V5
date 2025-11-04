# Send PROMPT 3 to Claude Sonnet 4 via Anthropic API
# This script sends the SORA 2.5 backend calculations prompt and saves artifacts

param(
    [string]$ApiKey = $env:ANTHROPIC_API_KEY,
    [string]$PromptFile = "PROMPT_3_SORA_25_BACKEND_CALCULATIONS.md",
    [string]$OutputDir = "SONNET4"
)

# Configuration
$ApiUrl = "https://api.anthropic.com/v1/messages"
$Model = "claude-sonnet-4-20250514"
$MaxTokens = 8000

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "  SENDING PROMPT 3 TO CLAUDE SONNET 4" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

# Validate API key
if (-not $ApiKey) {
    Write-Error "API key required. Set ANTHROPIC_API_KEY environment variable or pass -ApiKey parameter"
    exit 1
}

# Read prompt content
$PromptPath = Join-Path $PSScriptRoot "..\$PromptFile"
if (-not (Test-Path $PromptPath)) {
    Write-Error "Prompt file not found: $PromptPath"
    exit 1
}

Write-Host "📄 Reading prompt from: $PromptFile" -ForegroundColor Yellow
$PromptContent = Get-Content $PromptPath -Raw -Encoding UTF8
$WordCount = ($PromptContent -split '\s+').Count
Write-Host "   Words: $WordCount" -ForegroundColor Gray
Write-Host ""

# Prepare request body
$RequestBody = @{
    model = $Model
    max_tokens = $MaxTokens
    messages = @(
        @{
            role = "user"
            content = $PromptContent
        }
    )
} | ConvertTo-Json -Depth 10

# Prepare headers
$Headers = @{
    "x-api-key" = $ApiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

# Send request
Write-Host "🚀 Sending request to Anthropic API..." -ForegroundColor Yellow
Write-Host "   Model: $Model" -ForegroundColor Gray
Write-Host "   Max Tokens: $MaxTokens" -ForegroundColor Gray
Write-Host ""

try {
    $Response = Invoke-RestMethod -Uri $ApiUrl -Method Post -Headers $Headers -Body $RequestBody -TimeoutSec 120
    
    Write-Host "✅ Response received successfully" -ForegroundColor Green
    Write-Host ""
    
    # Parse response
    $Content = $Response.content[0].text
    $StopReason = $Response.stop_reason
    $InputTokens = $Response.usage.input_tokens
    $OutputTokens = $Response.usage.output_tokens
    
    Write-Host "📊 Usage Statistics:" -ForegroundColor Cyan
    Write-Host "   Input Tokens:  $InputTokens" -ForegroundColor Gray
    Write-Host "   Output Tokens: $OutputTokens" -ForegroundColor Gray
    Write-Host "   Stop Reason:   $StopReason" -ForegroundColor Gray
    Write-Host ""
    
    # Save complete response
    $OutputPath = Join-Path $PSScriptRoot "..\$OutputDir"
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    
    $Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $ResponseFile = Join-Path $OutputPath "PROMPT3_RESPONSE_$Timestamp.txt"
    $Content | Out-File -FilePath $ResponseFile -Encoding UTF8
    
    Write-Host "💾 Full response saved to:" -ForegroundColor Green
    Write-Host "   $ResponseFile" -ForegroundColor Gray
    Write-Host ""
    
    # Extract and save Python artifacts
    Write-Host "🔍 Extracting Python artifacts..." -ForegroundColor Yellow
    
    # Pattern to match code blocks with filenames
    $CodeBlockPattern = '```python\s*#\s*([^\n]+\.py)\s*(.*?)```'
    $Matches = [regex]::Matches($Content, $CodeBlockPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    if ($Matches.Count -eq 0) {
        Write-Host "⚠️  No Python artifacts found in response" -ForegroundColor Yellow
        Write-Host "   Trying alternative pattern..." -ForegroundColor Gray
        
        # Try alternative pattern: ```python without filename
        $CodeBlockPattern2 = '```python\s*(.*?)```'
        $Matches2 = [regex]::Matches($Content, $CodeBlockPattern2, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        
        if ($Matches2.Count -gt 0) {
            Write-Host "   Found $($Matches2.Count) code blocks" -ForegroundColor Gray
            Write-Host "   Manual extraction required - check response file" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   Found $($Matches.Count) Python files" -ForegroundColor Green
        Write-Host ""
        
        foreach ($Match in $Matches) {
            $FileName = $Match.Groups[1].Value.Trim()
            $Code = $Match.Groups[2].Value.Trim()
            
            $ArtifactPath = Join-Path $OutputPath $FileName
            $Code | Out-File -FilePath $ArtifactPath -Encoding UTF8
            
            $LineCount = ($Code -split "`n").Count
            Write-Host "   ✅ $FileName ($LineCount lines)" -ForegroundColor Green
        }
        
        Write-Host ""
        Write-Host "💾 Artifacts saved to: $OutputDir/" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "=" * 80 -ForegroundColor Cyan
    Write-Host "  ✅ PROMPT 3 COMPLETED SUCCESSFULLY" -ForegroundColor Green
    Write-Host "=" * 80 -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Review artifacts in $OutputDir/" -ForegroundColor Gray
    Write-Host "2. Integrate Python files into Backend_Python/" -ForegroundColor Gray
    Write-Host "3. Test SORA 2.5 calculations" -ForegroundColor Gray
    Write-Host ""
    
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Response Details:" -ForegroundColor Yellow
    Write-Host $_.Exception.Response -ForegroundColor Gray
    exit 1
}
