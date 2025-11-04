[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Continue'

# Resolve paths robustly using $PSScriptRoot (works for shortcuts, bat wrappers, direct runs)
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\\..')).Path
$toolsBackup = $PSScriptRoot
$backupScript = Join-Path $toolsBackup 'Start-Backup.ps1'
function Get-DesktopPath() {
    try {
        $desk = [Environment]::GetFolderPath('Desktop')
        if ($desk -and (Test-Path $desk)) { return $desk }
    } catch {}
    if ($env:OneDrive) {
        $od = Join-Path $env:OneDrive 'Desktop'
        if (Test-Path $od) { return $od }
    }
    return (Join-Path $env:USERPROFILE 'Desktop')
}

$backupsFolder = Join-Path (Get-DesktopPath) 'SKYWORKS_BACKUPS'

function Open-VSCode {
    param([string]$path)
    try { code $path } catch { Start-Process $path }
}

function Open-CopilotChat {
    param([string]$path)
    # Try to open VS Code and then trigger Copilot Chat panel. If the command id is unknown, VS Code still opens.
    if (Get-Command code -ErrorAction SilentlyContinue) {
        $commands = @('github.copilot.openPanel','github.copilot-chat.open','workbench.action.openChat')
        foreach ($cmd in $commands) {
            try { Start-Process code -ArgumentList @($path,'--command', $cmd) -ErrorAction Stop; return } catch {}
        }
        try { Start-Process code -ArgumentList @($path) -ErrorAction Stop; return } catch {}
    }
    # Fallback: just open the folder; user can press Ctrl+Shift+I inside VS Code
    Open-VSCode -path $path
}

function Menu {
    Clear-Host
    Write-Host "===============================" -ForegroundColor Green
    Write-Host " SKYWORKS V5 - QUICK LAUNCHER" -ForegroundColor Green
    Write-Host "===============================" -ForegroundColor Green
    Write-Host ""
    Write-Host "1. Open VS Code (Project)"
    Write-Host "2. Backup Project"
    Write-Host "3. Open Backups Folder"
    Write-Host "4. Quick Start Guide"
    Write-Host "5. Open Copilot Chat"
    Write-Host "6. Exit"
    Write-Host ""
}

while ($true) {
    Menu
    $choice = Read-Host "Choose (1-6)"
    switch ($choice) {
        '1' {
            Open-VSCode -path $root
        }
        '2' {
            & $backupScript -SourceRoot $root -BackupRoot $backupsFolder -OpenAfter
            Write-Host "Press Enter to continue..."; Read-Host | Out-Null
        }
        '3' {
            if (-not (Test-Path $backupsFolder)) { New-Item -ItemType Directory -Path $backupsFolder -Force | Out-Null }
            Start-Process explorer.exe $backupsFolder
        }
        '4' {
            $qs = Join-Path $root 'QUICK_START.md'
            try { code $qs } catch { Start-Process $qs }
        }
        '5' {
            Open-CopilotChat -path $root
        }
        '6' { break }
        Default { }
    }
}
