# ====================================================================
# Claude Sonnet 4.5 - Simple Prompt Runner
# Reads a prompt file and extracts returned code blocks.
# Usage: .\Simple-Sonnet45.ps1 -PromptPath .\Tools\sonnet45_super_prompt.md -AutoOpen
# ====================================================================
param(
  [string]$ApiKey = $env:ANTHROPIC_API_KEY,
  [string]$PromptPath = ".\Tools\sonnet45_super_prompt.md",
  [string]$OutputDir = ".\sonnet_output",
  [switch]$AutoOpen
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
  Write-Host "❌ ANTHROPIC_API_KEY missing. Set env var or pass -ApiKey" -ForegroundColor Red
  exit 1
}

if (-not (Test-Path $PromptPath)) {
  Write-Host "❌ Prompt file not found: $PromptPath" -ForegroundColor Red
  exit 1
}

if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

$prompt = Get-Content -Path $PromptPath -Raw -Encoding UTF8
$headers = @{ 'x-api-key' = $ApiKey; 'anthropic-version' = '2023-06-01'; 'content-type' = 'application/json; charset=utf-8' }

# Escape prompt for JSON
$promptEscaped = $prompt `
  -replace '\\', '\\' `
  -replace '"', '\"' `
  -replace "`n", '\n' `
  -replace "`r", '' `
  -replace "`t", '\t'

$body = @"
{
  "model": "claude-sonnet-4-5-20250929",
  "max_tokens": 16000,
  "temperature": 0,
  "messages": [ { "role": "user", "content": [ { "type": "text", "text": "$promptEscaped" } ] } ]
}
"@

Write-Host "🚀 Calling Sonnet 4.5 with prompt: $PromptPath" -ForegroundColor Cyan
$response = Invoke-WebRequest -Uri "https://api.anthropic.com/v1/messages" -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) -UseBasicParsing -TimeoutSec 300
$json = $response.Content | ConvertFrom-Json
$text = ($json.content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n"

$outText = Join-Path $OutputDir "response.txt"
Set-Content -Path $outText -Value $text -Encoding UTF8
Write-Host "   ✓ Saved raw response: $outText" -ForegroundColor Green

# Extract code blocks
$pattern = '(?:^|\n)(?:#\s*(.+?)\.cs\s*\n)?```(?:csharp|cs)?\s*\n(.*?)\n```'
$codeMatches = [regex]::Matches($text, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
if ($codeMatches.Count -eq 0) {
  Write-Host "⚠️  No code blocks found." -ForegroundColor Yellow
  exit 0
}

$index = 1
foreach ($m in $codeMatches) {
  $name = $m.Groups[1].Value.Trim()
  if (-not $name) { $name = "CodeBlock_$index.cs" }
  $code = $m.Groups[2].Value.Trim()
  $out = Join-Path $OutputDir $name
  Set-Content -Path $out -Value $code -Encoding UTF8
  Write-Host "   ✓ Extracted: $name" -ForegroundColor Green
  $index++
}

if ($AutoOpen) { Start-Process $OutputDir }

Write-Host "✅ Done." -ForegroundColor Green
