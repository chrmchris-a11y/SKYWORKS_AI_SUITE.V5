#!/usr/bin/env pwsh
param(
  [Parameter(Mandatory=$true)] [string]$ResponsePath,
  [switch]$AutoApply,
  [string]$OutputDir = ".\sonnet_output\sail_parsed"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ResponsePath)) { Write-Error "Response file not found: $ResponsePath"; exit 1 }
if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

$text = Get-Content $ResponsePath -Raw -Encoding UTF8

# Parse === FILE blocks
$filePattern = '=== FILE:\s*(.+?)\s*===\s*(.*?)\s*===\s*END FILE\s*=='
$matches = [regex]::Matches($text, $filePattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

if ($matches.Count -eq 0) {
  Write-Host "No === FILE blocks found in: $ResponsePath" -ForegroundColor Yellow
  exit 0
}

$written = @()
foreach ($m in $matches) {
  $rel = $m.Groups[1].Value.Trim()
  $code = $m.Groups[2].Value
  if (-not $rel) { continue }

  if ($rel -notmatch '^sail/') { $rel = "sail/" + $rel.TrimStart('/') }
  $targetRel = Join-Path "Backend_Python" $rel
  $targetDir = Split-Path $targetRel -Parent
  if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }

  $outPath = Join-Path $OutputDir ($rel -replace '/', '_')
  Set-Content -Path $outPath -Value $code -Encoding UTF8
  $written += @{ Name=$rel; Temp=$outPath; Target=$targetRel }
  Write-Host "Extracted: $rel -> $outPath" -ForegroundColor Green
}

if ($AutoApply) {
  foreach ($w in $written) {
    Copy-Item -Path $w.Temp -Destination $w.Target -Force
    Write-Host "Applied: $($w.Name) -> $($w.Target)" -ForegroundColor Cyan
  }
}
