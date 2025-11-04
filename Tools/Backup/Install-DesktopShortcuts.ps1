[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\\..')).Path
$tools = Join-Path $root 'Tools\\Backup'
# Determine all possible Desktop locations (handles OneDrive redirection)
function Get-DesktopPaths {
    $paths = New-Object System.Collections.Generic.List[string]
    try { $paths.Add([Environment]::GetFolderPath('Desktop')) } catch {}
    if ($env:USERPROFILE) { $paths.Add((Join-Path $env:USERPROFILE 'Desktop')) }
    if ($env:OneDrive) { $paths.Add((Join-Path $env:OneDrive 'Desktop')) }
    # Deduplicate and only keep existing folders
    ($paths | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique)
}
$desktops = Get-DesktopPaths

$launcherBat = Join-Path $tools 'SKYWORKS_LAUNCHER.bat'
$backupBat   = Join-Path $tools 'SKYWORKS_BACKUP.bat'
$chatBat     = Join-Path $tools 'OPEN_CHAT.bat'

$WshShell = New-Object -ComObject WScript.Shell

function New-Shortcut($desktopPath, $name, $target, $workingDir) {
    $lnk = Join-Path $desktopPath $name
    if (Test-Path $lnk) { Remove-Item $lnk -Force }
    $sc = $WshShell.CreateShortcut($lnk)
    $sc.TargetPath = $target
    $sc.WorkingDirectory = $workingDir
    $sc.IconLocation = "$env:SystemRoot\System32\shell32.dll, 167"
    $sc.Save()
    return $lnk
}

Write-Host "Installing Desktop shortcuts..."
$created = @()
foreach ($d in $desktops) {
    $created += (New-Shortcut -desktopPath $d -name 'SKYWORKS V5 - Quick Launcher.lnk' -target $launcherBat -workingDir $tools)
    $created += (New-Shortcut -desktopPath $d -name 'SKYWORKS V5 - Backup.lnk'         -target $backupBat   -workingDir $tools)
    $created += (New-Shortcut -desktopPath $d -name 'SKYWORKS V5 - Chat.lnk'           -target $chatBat     -workingDir $tools)
}
Write-Host "Created:" -ForegroundColor Green
$created | ForEach-Object { Write-Host " - $_" }
Write-Host "Done. If βλέπεις μόνο ένα shortcut, πιθανόν ο Desktop σου είναι σε OneDrive — το script έβαλε συντομεύσεις σε όλα τα πιθανά Desktop μονοπάτια." -ForegroundColor Green
