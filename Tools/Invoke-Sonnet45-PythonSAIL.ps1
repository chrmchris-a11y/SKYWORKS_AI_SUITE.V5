#!/usr/bin/env pwsh
# ====================================================================
# Claude Sonnet 4.5 - Python SAIL Calculator Automation
# Sends a strict prompt, parses file blocks, and auto-applies changes.
# ====================================================================

param(
  [string]$ApiKey = $env:ANTHROPIC_API_KEY,
  [string]$PromptPath = ".\Tools\sonnet45_prompt_sail_calculator.md",
  [string]$OutputDir = ".\sonnet_output\sail",
  [switch]$AutoApply
)

$ErrorActionPreference = 'Stop'

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║    Claude Sonnet 4.5 - Python SAIL Code Automation         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
  Write-Host "❌ ERROR: ANTHROPIC_API_KEY not found!" -ForegroundColor Red
  Write-Host "Set it with:  $env:ANTHROPIC_API_KEY = 'sk-ant-...'" -ForegroundColor Yellow
  exit 1
}

if (-not (Test-Path $PromptPath)) {
  Write-Host "❌ Prompt file not found: $PromptPath" -ForegroundColor Red
  exit 1
}

if (-not (Test-Path $OutputDir)) {
  New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "⚙️  Config:" -ForegroundColor Yellow
Write-Host "   Model: claude-sonnet-4-5-20250929" -ForegroundColor Gray
Write-Host "   Prompt: $PromptPath" -ForegroundColor Gray
Write-Host "   Output: $OutputDir" -ForegroundColor Gray
Write-Host ""

# Include current code context to help produce minimal diffs
function Get-FileIfExists($path) {
  if (Test-Path $path) { return Get-Content $path -Raw -Encoding UTF8 } else { return $null }
}

$calcPath = ".\Backend_Python\sail\calculators\sail_calculator.py"
$modelsPath = ".\Backend_Python\sail\models\sail_models.py"
$table20Path = ".\Backend_Python\sail\data\sail_tables_20.py"
$testsDir = ".\Backend_Python\sail\tests"

$calcCode = Get-FileIfExists $calcPath
$modelsCode = Get-FileIfExists $modelsPath
$table20Code = Get-FileIfExists $table20Path

$promptBody = Get-Content $PromptPath -Raw -Encoding UTF8

$context = @()
if ($modelsCode) { $context += "\n=== CONTEXT: sail/models/sail_models.py ===\n$modelsCode\n=== END CONTEXT ===\n" }
if ($calcCode)   { $context += "\n=== CONTEXT: sail/calculators/sail_calculator.py ===\n$calcCode\n=== END CONTEXT ===\n" }
if ($table20Code){ $context += "\n=== CONTEXT: sail/data/sail_tables_20.py ===\n$table20Code\n=== END CONTEXT ===\n" }

$fullPrompt = @"
$promptBody

---
Current repository context (read-only; keep public API stable):
$($context -join "\n")
"@

Write-Host "Calling Claude Sonnet 4.5..." -ForegroundColor Cyan

$headers = @{
  'x-api-key' = $ApiKey
  'anthropic-version' = '2023-06-01'
}

$bodyObj = [ordered]@{
  model = 'claude-sonnet-4-5-20250929'
  max_tokens = 16000
  temperature = 0
  messages = @(@{ role = 'user'; content = @(@{ type = 'text'; text = $fullPrompt }) })
}

try {
  $jsonResp = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" -Method Post -Headers $headers -Body ($bodyObj | ConvertTo-Json -Depth 10 -Compress) -ContentType 'application/json; charset=utf-8' -TimeoutSec 300
  $text = ($jsonResp.content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n"

  $respPath = Join-Path $OutputDir "response.txt"
  Set-Content -Path $respPath -Value $text -Encoding UTF8
  Write-Host ("   API call succeeded. Tokens: In={0} Out={1}" -f $jsonResp.usage.input_tokens, $jsonResp.usage.output_tokens) -ForegroundColor Green

  # Parse === FILE blocks first
  $filePattern = '=== FILE:\s*(.+?)\s*===\s*(.*?)\s*===\s*END FILE\s*==='
  $matches = [regex]::Matches($text, $filePattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

  if ($matches.Count -eq 0) {
    # Fallback: parse python code fences, with optional header comment containing path
  $codePattern = '(?:^|\n)(?:#\s*([^\n]+\.py)\s*\n)?```(?:python)?\s*\n(.*?)\n```'
  $matches = [regex]::Matches($text, $codePattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
  }

  if ($matches.Count -eq 0) {
    Write-Host "WARNING: No file blocks found. Saved raw response to: $respPath" -ForegroundColor Yellow
    exit 0
  }

  $written = @()
  for ($i=0; $i -lt $matches.Count; $i++) {
    $m = $matches[$i]
    $rel = $m.Groups[1].Value.Trim()
    $code = $m.Groups[2].Value
    if (-not $rel) { $rel = "CodeBlock_$($i+1).py" }

    # Normalize path to repo target
    if ($rel -notmatch '^sail/') { $rel = "sail/" + $rel.TrimStart('/') }
    $targetRel = Join-Path "Backend_Python" $rel
    $targetDir = Split-Path $targetRel -Parent
    if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }

    $outPath = Join-Path $OutputDir ($rel -replace '/', '_')
    Set-Content -Path $outPath -Value $code -Encoding UTF8

    $written += @{ Name = $rel; Temp = $outPath; Target = $targetRel }
    Write-Host "   • Extracted: $rel → $outPath" -ForegroundColor White
  }

  if ($AutoApply) {
    Write-Host "🔄 Auto-applying to repo..." -ForegroundColor Cyan
    foreach ($w in $written) {
      Copy-Item -Path $w.Temp -Destination $w.Target -Force
      Write-Host "   ✓ Applied: $($w.Name) → $($w.Target)" -ForegroundColor Green
    }
  } else {
    Write-Host "💡 Tip: Re-run with -AutoApply to copy files into Backend_Python" -ForegroundColor Gray
  }

} catch {
  Write-Host "❌ API call failed: $($_.Exception.Message)" -ForegroundColor Red
  if ($_.Exception.Response) {
    try { $r = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream()); $d = $r.ReadToEnd(); $r.Close(); Write-Host $d -ForegroundColor DarkGray } catch {}
  }
  exit 1
}
