param(
  [string]$PromptPath = "$PSScriptRoot/sonnet45_super_prompt.md",
  [Parameter(Mandatory=$true)][string]$ApiKey,
  [switch]$CopyToClipboard = $true,
  [switch]$OpenEdge
)

if (!(Test-Path -LiteralPath $PromptPath)) {
  Write-Error "Prompt file not found: $PromptPath"
  exit 1
}

# Set in current process environment only (not persisted to disk)
$Env:SONNET45_API_KEY = $ApiKey

$content = Get-Content -Raw -LiteralPath $PromptPath
$lines = ($content -split "`n").Count
$chars = $content.Length
Write-Host "[sonnet45] Loaded prompt: $PromptPath" -ForegroundColor Cyan
Write-Host "[sonnet45] Lines: $lines  Chars: $chars" -ForegroundColor Cyan

if ($CopyToClipboard) {
  try {
    Set-Clipboard -Value $content
    Write-Host "[sonnet45] Prompt copied to clipboard. Paste into Sonnet 4.5 chat." -ForegroundColor Green
  }
  catch {
    Write-Warning "[sonnet45] Failed to copy to clipboard."
  }
}

if ($OpenEdge) {
  try {
    Start-Process 'msedge' -ArgumentList 'https://sonnet.ai/chat' | Out-Null
    Write-Host "[sonnet45] Opened Sonnet 4.5 chat in Edge." -ForegroundColor Yellow
  }
  catch {
    Write-Warning "[sonnet45] Could not open Edge. Please open Sonnet 4.5 manually."
  }
}

Write-Host "[sonnet45] SONNET45_API_KEY is set for this session only. It is NOT saved to disk." -ForegroundColor Yellow
