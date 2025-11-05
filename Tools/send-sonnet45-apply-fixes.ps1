#!/usr/bin/env pwsh
# Send exact prompt to Claude Sonnet 4.5 and auto-apply returned files into the repo
#
# Usage:
#   $env:ANTHROPIC_API_KEY = 'sk-ant-...'
#   pwsh -NoProfile -File Tools/send-sonnet45-apply-fixes.ps1 -PromptFile "sonnet_output\prompt_sonnet45_apply_fixes.txt"
#
# Notes:
# - Reads ANTHROPIC_API_KEY from environment (won't echo it).
# - Expects model name for Sonnet 4.5; override with -Model if needed.
# - Auto-applies any blocks in the form:
#     === FILE: <relative\path> ===\n
#     ...file content...

param(
  [Parameter(Mandatory=$true)][string]$PromptFile,
  [string]$Model = "claude-sonnet-4-20250514",  # override if your tenant exposes Sonnet 4.5 under a different id
  [int]$MaxTokens = 16000,
  [switch]$NoApply,            # if set, only saves response without applying
  [string]$OutDir = "sonnet_output"
)

$ErrorActionPreference = "Stop"

function Assert-File {
  param([string]$Path)
  if (-not (Test-Path -LiteralPath $Path)) {
    throw "File not found: $Path"
  }
}

Assert-File -Path $PromptFile

if (-not $env:ANTHROPIC_API_KEY) {
  Write-Host "❌ ANTHROPIC_API_KEY not set in environment" -ForegroundColor Red
  Write-Host "Set it for this session and rerun (key will not be echoed)." -ForegroundColor Yellow
  exit 1
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$promptText = Get-Content -LiteralPath $PromptFile -Raw -Encoding UTF8

Write-Host "🤖 Sending prompt to Anthropic (model=$Model)…" -ForegroundColor Yellow

$headers = @{
  "x-api-key" = $env:ANTHROPIC_API_KEY
  "anthropic-version" = "2023-06-01"
  "content-type" = "application/json"
}

$body = @{
  model    = $Model
  max_tokens = $MaxTokens
  messages = @(@{ role = "user"; content = $promptText })
} | ConvertTo-Json -Depth 10

$resp = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" -Method Post -Headers $headers -Body $body -TimeoutSec 300

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$rawOut = Join-Path $OutDir "SONNET45_APPLY_FIXES_RESPONSE_$timestamp.txt"
$resp.content[0].text | Out-File -FilePath $rawOut -Encoding utf8
Write-Host "💾 Saved raw response: $rawOut" -ForegroundColor Green

if ($NoApply) { exit 0 }

# Auto-apply: parse === FILE: path === sections
$text = [System.IO.File]::ReadAllText($rawOut)
$pattern = "(?ms)^===\s+FILE:\s+(?<path>.+?)\s*===\s*\n(?<content>.*?)(?=^===\s+FILE:|\z)"
$fileMatches = [System.Text.RegularExpressions.Regex]::Matches($text, $pattern)

if ($fileMatches.Count -eq 0) {
  Write-Host "ℹ️ No '=== FILE: … ===' sections found; nothing to apply." -ForegroundColor Yellow
  exit 0
}

$applied = @()
foreach ($m in $fileMatches) {
  $relPath = $m.Groups['path'].Value.Trim()
  $content = $m.Groups['content'].Value

  # Strip surrounding triple backticks if present
  $content = $content -replace "(?ms)^```[a-zA-Z0-9-]*\s*\n", ""
  $content = $content -replace "(?ms)\n```\s*$", "\n"

  $absPath = Join-Path -Path (Get-Location) -ChildPath $relPath
  $dir = Split-Path -Parent $absPath
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  [System.IO.File]::WriteAllText($absPath, $content, [System.Text.Encoding]::UTF8)
  $applied += $relPath
}

Write-Host "✅ Applied files:" -ForegroundColor Green
$applied | ForEach-Object { Write-Host "  • $_" -ForegroundColor Green }

Write-Host "🏁 Done. Review changes and run tests." -ForegroundColor Cyan
