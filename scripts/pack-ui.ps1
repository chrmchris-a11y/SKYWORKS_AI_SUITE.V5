# ================================================================
# SKYWORKS UI Packaging Script (PowerShell)
# Creates dist/skyworks_static_ui.zip with all 12 pages + assets
# ================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  SKYWORKS UI Packaging (PowerShell)" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Paths
$SRC = "WebPlatform/wwwroot/app/Pages/ui"
$DST = "dist/skyworks_static_ui.zip"

# Verify source exists
if (-not (Test-Path $SRC)) {
    Write-Host "❌ ERROR: Source directory not found: $SRC" -ForegroundColor Red
    exit 1
}

# Create dist directory
if (-not (Test-Path "dist")) {
    New-Item -ItemType Directory -Path "dist" | Out-Null
    Write-Host "✅ Created dist/ directory" -ForegroundColor Green
}

# Remove existing ZIP
if (Test-Path $DST) {
    Remove-Item $DST -Force
    Write-Host "✅ Removed existing ZIP" -ForegroundColor Green
}

# Count files
$fileCount = (Get-ChildItem -Path $SRC -Recurse -File).Count
Write-Host "📦 Packing $fileCount files from $SRC..." -ForegroundColor Yellow

# Create ZIP
try {
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    [System.IO.Compression.ZipFile]::CreateFromDirectory($SRC, $DST)
    Write-Host "✅ ZIP created: $DST" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR: Failed to create ZIP: $_" -ForegroundColor Red
    exit 1
}

# Verify ZIP contents
$zipInfo = Get-Item $DST
$zipSize = [math]::Round($zipInfo.Length / 1KB, 2)
Write-Host "📊 ZIP size: $zipSize KB" -ForegroundColor Cyan

# List ZIP contents
Write-Host ""
Write-Host "📋 ZIP Contents:" -ForegroundColor Cyan
Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
$zip = [System.IO.Compression.ZipFile]::OpenRead($DST)
$zip.Entries | Select-Object -First 20 | ForEach-Object {
    Write-Host "   $($_.FullName)" -ForegroundColor Gray
}
if ($zip.Entries.Count -gt 20) {
    Write-Host "   ... and $($zip.Entries.Count - 20) more files" -ForegroundColor Gray
}
$zip.Dispose()

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  ✅ SUCCESS! Package ready for distribution" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📦 Package: $DST" -ForegroundColor Yellow
Write-Host "📊 Files: $fileCount" -ForegroundColor Yellow
Write-Host "💾 Size: $zipSize KB" -ForegroundColor Yellow
Write-Host ""
