# Send to Claude Opus 4.1 via API

param(
    [string]$ApiKey = $env:ANTHROPIC_API_KEY,
    [string]$PromptFile = "OPUS_FIX_REQUEST.md",
    [string]$OutputDir = "OPUS_RESPONSE"
)

if (-not $ApiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable or pass -ApiKey."
}

$ApiUrl = "https://api.anthropic.com/v1/messages"
$Model = "claude-opus-4-20250514"  # Latest Opus 4.1
$MaxTokens = 8000

Write-Host "`n" -NoNewline
Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host "  SENDING FIX REQUEST TO CLAUDE OPUS 4.1" -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host ""

# Read prompt
$PromptPath = Join-Path $PSScriptRoot "..\$PromptFile"
if (-not (Test-Path $PromptPath)) {
    Write-Error "Prompt file not found: $PromptPath"
    exit 1
}

Write-Host "📄 Reading comprehensive fix request..." -ForegroundColor Yellow
$PromptContent = Get-Content $PromptPath -Raw -Encoding UTF8
$WordCount = ($PromptContent -split '\s+').Count
Write-Host "   Words: $WordCount" -ForegroundColor Gray
Write-Host "   Model: $Model (Opus 4.1 - Most Intelligent)" -ForegroundColor Gray
Write-Host ""

# Prepare request
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

$Headers = @{
    "x-api-key" = $ApiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

Write-Host "🚀 Sending to Claude Opus 4.1..." -ForegroundColor Yellow
Write-Host ""

try {
    $Response = Invoke-RestMethod -Uri $ApiUrl -Method Post -Headers $Headers -Body $RequestBody -TimeoutSec 180
    
    Write-Host "✅ Response received from Opus 4.1!" -ForegroundColor Green
    Write-Host ""
    
    $Content = $Response.content[0].text
    $StopReason = $Response.stop_reason
    $InputTokens = $Response.usage.input_tokens
    $OutputTokens = $Response.usage.output_tokens
    
    Write-Host "📊 Usage Statistics:" -ForegroundColor Cyan
    Write-Host "   Input Tokens:  $InputTokens" -ForegroundColor Gray
    Write-Host "   Output Tokens: $OutputTokens" -ForegroundColor Gray
    Write-Host "   Stop Reason:   $StopReason" -ForegroundColor Gray
    Write-Host ""
    
    # Save response
    $OutputPath = Join-Path $PSScriptRoot $OutputDir
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    
    $Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $ResponseFile = Join-Path $OutputPath "OPUS_FIX_RESPONSE_$Timestamp.md"
    $Content | Out-File -FilePath $ResponseFile -Encoding UTF8
    
    Write-Host "💾 Complete response saved to:" -ForegroundColor Green
    Write-Host "   $ResponseFile" -ForegroundColor Cyan
    Write-Host ""
    
    # Extract code files
    Write-Host "🔍 Extracting code artifacts..." -ForegroundColor Yellow
    
    # Pattern for code blocks with filenames
    $CodeBlockPattern = '```(?:csharp|javascript|powershell|cs|js|ps1)\s*(?:#\s*)?([^\n]+)\s*(.*?)```'
    $Matches = [regex]::Matches($Content, $CodeBlockPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    if ($Matches.Count -gt 0) {
        Write-Host "   Found $($Matches.Count) code artifacts" -ForegroundColor Green
        Write-Host ""
        
        foreach ($Match in $Matches) {
            $FileName = $Match.Groups[1].Value.Trim()
            $Code = $Match.Groups[2].Value.Trim()
            
            if ($FileName -match '\.(cs|js|ps1)$') {
                $ArtifactPath = Join-Path $OutputPath $FileName
                $Code | Out-File -FilePath $ArtifactPath -Encoding UTF8
                
                $LineCount = ($Code -split "`n").Count
                Write-Host "   ✅ $FileName ($LineCount lines)" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "   ℹ️  No code artifacts found - check response file for instructions" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor Cyan
    Write-Host "  ✅ OPUS 4.1 RESPONSE COMPLETE" -ForegroundColor Green
    Write-Host ("=" * 80) -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📁 All files saved to: $OutputDir/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Review the response file" -ForegroundColor Gray
    Write-Host "2. Apply the fixes to your project" -ForegroundColor Gray
    Write-Host "3. Test SORA 2.0 and 2.5 in UI" -ForegroundColor Gray
    Write-Host ""
    
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    if ($_.Exception.Response) {
        Write-Host "Response Details:" -ForegroundColor Yellow
        Write-Host $_.Exception.Response -ForegroundColor Gray
    }
    exit 1
}
