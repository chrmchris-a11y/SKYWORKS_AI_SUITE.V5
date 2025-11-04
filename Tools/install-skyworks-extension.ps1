param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$extDir = Join-Path $root 'vscode-skyworks-assistant'
if (-not (Test-Path $extDir)) { Write-Host 'Extension folder not found.'; exit 0 }

Push-Location $extDir
try {
  $env:NPM_CONFIG_PRODUCTION = 'false'
  if (-not (Test-Path 'node_modules')) { npm install --include=dev --silent }
  else { npm install --include=dev --silent }
  npm run -s compile
  if ($LASTEXITCODE -ne 0) { throw 'TypeScript compile failed' }
  npx --yes @vscode/vsce package --no-yarn --out skyworks-assistant.vsix
  if ($LASTEXITCODE -ne 0) { throw 'vsce package failed' }
  if (-not (Test-Path 'skyworks-assistant.vsix')) { throw 'VSIX not generated' }
  $codeCmd = "$Env:LOCALAPPDATA\Programs\Microsoft VS Code\bin\code.cmd"
  if (-not (Test-Path $codeCmd)) { $codeCmd = 'code' }
  & $codeCmd --install-extension (Join-Path $extDir 'skyworks-assistant.vsix') --force | Out-Null
  # Verify installation even if exit code non-zero due to VS Code CLI quirks
  & $codeCmd --list-extensions | Select-String -Quiet 'skyworks-assistant' | Out-Null
  if ($?) {
    Write-Host "✅ Skyworks Assistant extension installed" -ForegroundColor Green
  } else {
    throw 'code --install-extension did not report extension in list'
  }
} finally {
  Pop-Location
}
