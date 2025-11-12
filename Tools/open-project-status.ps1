# Open PROJECT_STATUS.md on workspace launch
# This script is called automatically by VS Code task on folder open

$projectStatusPath = Join-Path $PSScriptRoot ".." "PROJECT_STATUS.md"

if (Test-Path $projectStatusPath) {
    Write-Host "📊 Opening PROJECT_STATUS.md..." -ForegroundColor Cyan
    code $projectStatusPath
} else {
    Write-Warning "⚠️ PROJECT_STATUS.md not found at: $projectStatusPath"
}
