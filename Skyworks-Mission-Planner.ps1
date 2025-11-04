#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Skyworks Mission Planner - Desktop Launcher
.DESCRIPTION
    Opens the Skyworks Mission Planner in your default browser with local server
#>

# Set window title
$host.UI.RawUI.WindowTitle = "Skyworks Mission Planner Server"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "      🚀 SKYWORKS MISSION PLANNER - DESKTOP APP       " -ForegroundColor White -BackgroundColor DarkBlue
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if Python is available
$pythonCmd = $null
if (Get-Command python -ErrorAction SilentlyContinue) {
    $pythonCmd = "python"
} elseif (Get-Command python3 -ErrorAction SilentlyContinue) {
    $pythonCmd = "python3"
} else {
    Write-Host "❌ ΣΦΑΛΜΑ: Δεν βρέθηκε η Python" -ForegroundColor Red
    Write-Host ""
    Write-Host "Παρακαλώ εγκαταστήστε Python από:" -ForegroundColor Yellow
    Write-Host "https://www.python.org/downloads/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Πατήστε οποιοδήποτε πλήκτρο για να κλείσετε..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Navigate to Frontend folder
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$frontendPath = Join-Path $scriptPath "Frontend"

if (-not (Test-Path $frontendPath)) {
    Write-Host "❌ ΣΦΑΛΜΑ: Ο φάκελος Frontend δεν βρέθηκε" -ForegroundColor Red
    Write-Host "Αναμενόμενη θέση: $frontendPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Πατήστε οποιοδήποτε πλήκτρο για να κλείσετε..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Set-Location $frontendPath

Write-Host "✅ Frontend Path: $frontendPath" -ForegroundColor Green
Write-Host "✅ Python: $pythonCmd" -ForegroundColor Green
Write-Host ""
Write-Host "───────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host "📡 Ξεκινάει ο τοπικός server..." -ForegroundColor Yellow
Write-Host "🌐 URL: http://localhost:8080" -ForegroundColor Cyan
Write-Host "───────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# Wait a moment for server to start, then open browser
$openBrowser = {
    Start-Sleep -Seconds 2
    $url = "http://localhost:8080/Pages/mission.html"
    Write-Host "🌍 Άνοιγμα browser: $url" -ForegroundColor Cyan
    Start-Process $url
}

# Start browser opener in background
Start-Job -ScriptBlock $openBrowser | Out-Null

Write-Host "💡 ΟΔΗΓΙΕΣ:" -ForegroundColor Yellow
Write-Host "   • Το browser θα ανοίξει αυτόματα σε 2 δευτερόλεπτα" -ForegroundColor White
Write-Host "   • Αν δεν ανοίξει, πήγαινε χειροκίνητα στο: http://localhost:8080/Pages/mission.html" -ForegroundColor White
Write-Host "   • Για να ΣΤΑΜΑΤΗΣΕΙΣ τον server: πάτα Ctrl+C ή κλείσε αυτό το παράθυρο" -ForegroundColor White
Write-Host ""
Write-Host "───────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host "📊 Αρχείο καταγραφής server:" -ForegroundColor Gray
Write-Host ""

# Start Python HTTP server
try {
    & $pythonCmd -m http.server 8080
} catch {
    Write-Host ""
    Write-Host "❌ ΣΦΑΛΜΑ: Αποτυχία εκκίνησης server: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Πατήστε οποιοδήποτε πλήκτρο για να κλείσετε..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
