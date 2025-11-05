param(
  [string]$PromptPath = "$PSScriptRoot/sonnet45_super_prompt.md",
  [switch]$CopyToClipboard = $true,
  [string]$OutFile = ""
)

if (!(Test-Path $PromptPath)) {
  Write-Error "Prompt file not found: $PromptPath"
  exit 1
}

$content = Get-Content -Raw -LiteralPath $PromptPath
$lines = ($content -split "`n").Count
$chars = $content.Length
Write-Host "[sonnet45] Loaded prompt: $PromptPath" -ForegroundColor Cyan
Write-Host "[sonnet45] Lines: $lines  Chars: $chars" -ForegroundColor Cyan

if ($CopyToClipboard) {
  try {
    Set-Clipboard -Value $content
    Write-Host "[sonnet45] Prompt copied to clipboard. Paste it into Sonnet 4.5 chat." -ForegroundColor Green
  }
  catch {
    Write-Warning "[sonnet45] Failed to copy to clipboard."
  }
}

if ($OutFile) {
  $content | Set-Content -LiteralPath $OutFile -Encoding UTF8
  Write-Host "[sonnet45] Prompt written to $OutFile" -ForegroundColor Yellow
}

Write-Host "[sonnet45] NOTE: For security, do NOT hardcode API keys in repo. Use an env var (e.g., $Env:SONNET45_API_KEY) if needed by your client tool." -ForegroundColor Yellow
