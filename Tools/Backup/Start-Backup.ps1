param(
    [string]$SourceRoot,
    [string]$BackupRoot,
    [switch]$OpenAfter,
    [int]$MaxFiles = 0
)

# Clean, ASCII-only UI with a real progress bar
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Continue'

function Write-Header {
    Write-Host ""; Write-Host "===============================" -ForegroundColor Green
    Write-Host " SKYWORKS V5 BACKUP" -ForegroundColor Green
    Write-Host "===============================" -ForegroundColor Green
}

function Test-IsExcluded([string]$path) {
    $segments = $path.ToLower().Split([io.path]::DirectorySeparatorChar)
    $excluded = @('bin','obj','.git','.vs','node_modules','packages','testresults','.cache')
    foreach ($seg in $segments) { if ($excluded -contains $seg) { return $true } }
    return $false
}

function Resolve-WorkspaceRoot() {
    # If called without SourceRoot, default to repo root (two levels above this script)
    $root = Resolve-Path (Join-Path $PSScriptRoot '..\\..')
    return $root.Path
}

function Get-DesktopPath() {
    # Prefer known folder Desktop (supports OneDrive redirection)
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

function Resolve-BackupRoot([string]$inputRoot) {
    if ($inputRoot) {
        try { $rp = Resolve-Path -LiteralPath $inputRoot -ErrorAction SilentlyContinue } catch {}
        if ($rp) { return $rp.Path } else { return $inputRoot }
    }
    return (Join-Path (Get-DesktopPath) 'SKYWORKS_BACKUPS')
}

function Start-BackupInternal([string]$src, [string]$backupRoot, [int]$maxFiles) {
    Write-Header
    if (-not (Test-Path $src)) { throw "Source folder not found: $src" }

    # Ensure backup root
    if (-not (Test-Path $backupRoot)) { New-Item -ItemType Directory -Path $backupRoot | Out-Null }

    $timestamp = (Get-Date -Format 'yyyyMMdd_HHmmss')
    $dest = Join-Path $backupRoot "SKYWORKS_V5_BACKUP_$timestamp"
    New-Item -ItemType Directory -Path $dest | Out-Null

    Write-Host "Source : $src"
    Write-Host "Target : $dest"
    Write-Host ""; Write-Host "Building file list..."

    $allFiles = Get-ChildItem -Path $src -Recurse -File -Force | Where-Object { -not (Test-IsExcluded $_.FullName) }
    if ($maxFiles -gt 0) { $files = $allFiles | Select-Object -First $maxFiles } else { $files = $allFiles }

    $total = [math]::Max(1, $files.Count)
    Write-Host ("Files  : {0}" -f $files.Count)
    Write-Host ""; $lastShown = -1

    $i = 0; $errors = 0
    foreach ($f in $files) {
    $rel = $f.FullName.Substring($src.Length)
    $rel = ($rel -replace '^[\\/]+','')
        $target = Join-Path $dest $rel
        $targetDir = Split-Path -Parent $target
        if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }
        try {
            Copy-Item -LiteralPath $f.FullName -Destination $target -Force -ErrorAction Stop
        } catch {
            $errors++
        }
        $i++
        $percent = [int](($i / $total) * 100)
        Write-Progress -Activity "Copying files" -Status "$percent%" -PercentComplete $percent
        # Show coarse milestones 0,5,10,...100
        $milestone = [int]([math]::Floor($percent / 5) * 5)
        if ($milestone -ne $lastShown) { Write-Host ("{0}%" -f $milestone); $lastShown = $milestone }
    }
    Write-Progress -Activity "Copying files" -Completed

    Write-Host ""; Write-Host "Backup completed." -ForegroundColor Green
    Write-Host ("Copied: {0} files, Errors: {1}" -f $i, $errors)
    Write-Host ("Saved : {0}" -f $dest)

    if ($OpenAfter) { Start-Process explorer.exe $dest | Out-Null }
}

try {
    if (-not $SourceRoot) { $SourceRoot = Resolve-WorkspaceRoot } else { $SourceRoot = (Resolve-Path $SourceRoot).Path }
    $BackupRoot = Resolve-BackupRoot -inputRoot $BackupRoot
    Start-BackupInternal -src $SourceRoot -backupRoot $BackupRoot -maxFiles $MaxFiles
    exit 0
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
