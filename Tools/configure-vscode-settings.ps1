param()
$ErrorActionPreference = 'Stop'

# Sentinel to avoid repeated writes
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$sentinel = Join-Path $workspaceRoot '.vscode/.mcp_configured'

# Resolve server path and normalize to forward slashes (VS Code requirement on Windows)
$serverPath = Join-Path $workspaceRoot 'skyworks-sora-mcp-server/build/index.js'
$serverPathFs = (Resolve-Path $serverPath).Path
$serverPathVscode = $serverPathFs -replace '\\','/'

# VS Code user settings.json
$settingsPath = Join-Path $env:APPDATA 'Code/User/settings.json'
if (-not (Test-Path $settingsPath)) {
  New-Item -ItemType File -Path $settingsPath -Force | Out-Null
  Set-Content -Path $settingsPath -Value '{}' -Encoding UTF8
}

# Load current settings
try {
  $jsonText = Get-Content -Raw -Path $settingsPath -Encoding UTF8
  if ([string]::IsNullOrWhiteSpace($jsonText)) { $jsonText = '{}' }
  $settings = $jsonText | ConvertFrom-Json -AsHashtable
} catch {
  Write-Host "⚠️ settings.json is not valid JSON. Backing up and recreating." -ForegroundColor Yellow
  Copy-Item $settingsPath "$settingsPath.bak" -Force
  $settings = @{}
}

# Ensure mcp.servers block
if (-not $settings.ContainsKey('mcp.servers')) { $settings['mcp.servers'] = @{} }
$servers = $settings['mcp.servers']
$servers['skyworks-sora'] = @{ command = 'node'; args = @($serverPathVscode) }

# Helpful defaults
$settings['workbench.startupEditor'] = 'readme'
$settings['files.autoSave'] = 'onFocusChange'

# Save
($settings | ConvertTo-Json -Depth 20) | Set-Content -Path $settingsPath -Encoding UTF8

# Mark configured
New-Item -ItemType File -Path $sentinel -Force | Out-Null

Write-Host "✅ VS Code settings patched for MCP server (skyworks-sora)." -ForegroundColor Green
