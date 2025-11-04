# Configure Continue Extension to show Chat in PANEL (above terminal)
# Instead of sidebar (left)

$settingsPath = "$env:APPDATA\Code\User\settings.json"

Write-Host "🔧 Configuring Continue Extension..." -ForegroundColor Cyan

if (-not (Test-Path $settingsPath)) {
    Write-Host "❌ VS Code settings.json not found at: $settingsPath" -ForegroundColor Red
    Write-Host "   Please open VS Code at least once before running this script." -ForegroundColor Yellow
    exit 1
}

# Read settings
$settingsContent = Get-Content $settingsPath -Raw
$settings = $settingsContent | ConvertFrom-Json

# Add/Update Continue settings
$settings | Add-Member -NotePropertyName 'continue.showChatInSidebar' -NotePropertyValue $false -Force
$settings | Add-Member -NotePropertyName 'continue.showChatInPanel' -NotePropertyValue $true -Force

# Save settings
$settings | ConvertTo-Json -Depth 100 | Set-Content $settingsPath

Write-Host "✅ Continue configuration updated!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Settings applied:" -ForegroundColor White
Write-Host "   continue.showChatInSidebar: FALSE (chat WON'T show in left sidebar)" -ForegroundColor Gray
Write-Host "   continue.showChatInPanel: TRUE (chat WILL show in panel above terminal)" -ForegroundColor Gray
Write-Host ""
Write-Host "🔄 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Reload VS Code (F1 → 'Developer: Reload Window')" -ForegroundColor White
Write-Host "   2. Click '🧠 Start Claude Sonnet 4' button in Welcome panel" -ForegroundColor White
Write-Host "   3. Continue chat will open ABOVE the terminal!" -ForegroundColor White
Write-Host ""
Write-Host "✅ Done!" -ForegroundColor Green
