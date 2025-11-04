#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start a local HTTP server for the Skyworks Frontend
.DESCRIPTION
    Starts Python's built-in HTTP server to serve the Frontend folder on http://localhost:8080
    This resolves CORS issues when opening HTML files directly in the browser.
#>

Write-Host "🚀 Starting Skyworks Frontend Server..." -ForegroundColor Cyan
Write-Host ""

# Check if Python is available
$pythonCmd = $null
if (Get-Command python -ErrorAction SilentlyContinue) {
    $pythonCmd = "python"
} elseif (Get-Command python3 -ErrorAction SilentlyContinue) {
    $pythonCmd = "python3"
} else {
    Write-Host "❌ ERROR: Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Python from https://www.python.org/downloads/" -ForegroundColor Yellow
    Write-Host "or use an alternative method (see README)" -ForegroundColor Yellow
    exit 1
}

# Navigate to Frontend folder
$frontendPath = Join-Path $PSScriptRoot "Frontend"
if (-not (Test-Path $frontendPath)) {
    Write-Host "❌ ERROR: Frontend folder not found at $frontendPath" -ForegroundColor Red
    exit 1
}

Set-Location $frontendPath

Write-Host "📂 Serving from: $frontendPath" -ForegroundColor Green
Write-Host "🌐 Server URL: http://localhost:8080" -ForegroundColor Green
Write-Host "📄 Mission Page: http://localhost:8080/Pages/mission.html" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray
Write-Host ""

# Start Python HTTP server
try {
    & $pythonCmd -m http.server 8080
} catch {
    Write-Host "❌ ERROR: Failed to start server: $_" -ForegroundColor Red
    exit 1
}
