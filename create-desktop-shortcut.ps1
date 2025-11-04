$WshShell = New-Object -ComObject WScript.Shell
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$ShortcutPath = Join-Path $DesktopPath "🚀 SKYWORKS Mission Planner.lnk"
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\LAUNCH_SKYWORKS.bat"
$Shortcut.WorkingDirectory = "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"
# Using VS Code icon for IDE integration
$Shortcut.IconLocation = "C:\Program Files\Microsoft VS Code\Code.exe,0"
$Shortcut.Description = "SKYWORKS AI Suite - Launch VS Code + Copilot Chat + Project"
$Shortcut.Save()
Write-Host "✅ Desktop shortcut created: $ShortcutPath" -ForegroundColor Green
Write-Host "🎯 Double-click to launch VS Code with SKYWORKS + Copilot Chat!" -ForegroundColor Cyan
Write-Host ""
pause

