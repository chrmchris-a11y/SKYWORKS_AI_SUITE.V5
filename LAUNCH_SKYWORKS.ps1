# ========================================
# SKYWORKS AI SUITE - QUICK LAUNCHER
# ========================================
# Opens VS Code with project + GitHub Copilot Chat ready
# Author: AI Assistant
# Date: 2025-10-25
# ========================================

Write-Host "🚀 Starting SKYWORKS Mission Planner Suite..." -ForegroundColor Cyan
Write-Host ""

# Project path
$ProjectPath = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"

# Check if VS Code is installed
$VsCodePath = "C:\Program Files\Microsoft VS Code\Code.exe"
if (-not (Test-Path $VsCodePath)) {
    $VsCodePath = "C:\Users\chrmc\AppData\Local\Programs\Microsoft VS Code\Code.exe"
}

if (-not (Test-Path $VsCodePath)) {
    Write-Host "❌ ERROR: VS Code not found!" -ForegroundColor Red
    Write-Host "Please install Visual Studio Code from https://code.visualstudio.com" -ForegroundColor Yellow
    pause
    exit 1
}

# 1. Open VS Code with project
Write-Host "📂 Opening VS Code with SKYWORKS project..." -ForegroundColor Green
Start-Process $VsCodePath -ArgumentList $ProjectPath

# Wait for VS Code to load
Start-Sleep -Seconds 3

# 2. Open GitHub Copilot Chat (Command Palette shortcut)
Write-Host "💬 Launching GitHub Copilot Chat..." -ForegroundColor Green
Write-Host "   → Press Ctrl+Shift+I in VS Code to open Copilot Chat" -ForegroundColor Yellow

# 3. Open Frontend in browser (if server is running)
Start-Sleep -Seconds 2
Write-Host "🌐 Checking if frontend server is running..." -ForegroundColor Green

try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080" -TimeoutSec 2 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ Frontend detected! Opening browser..." -ForegroundColor Green
        Start-Process "http://localhost:8080"
    }
} catch {
    Write-Host "   ℹ️  Frontend not running. Start manually with:" -ForegroundColor Yellow
    Write-Host "      cd Frontend" -ForegroundColor Cyan
    Write-Host "      npx serve ." -ForegroundColor Cyan
}

# 4. Check if backend is running
Write-Host "🔧 Checking if backend API is running..." -ForegroundColor Green

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5210/api/sora/info" -TimeoutSec 2 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ Backend API is running!" -ForegroundColor Green
    }
} catch {
    Write-Host "   ℹ️  Backend not running. Start manually with:" -ForegroundColor Yellow
    Write-Host "      cd Backend\src\Skyworks.Api" -ForegroundColor Cyan
    Write-Host "      dotnet run" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "🎯 QUICK REFERENCE:" -ForegroundColor Magenta
Write-Host "  • VS Code opened at: $ProjectPath" -ForegroundColor White
Write-Host "  • Frontend URL: http://localhost:8080" -ForegroundColor White
Write-Host "  • Backend API: http://localhost:5210" -ForegroundColor White
Write-Host "  • Copilot Chat: Ctrl+Shift+I (in VS Code)" -ForegroundColor White
Write-Host ""
Write-Host "✨ Ready to work! Happy coding! 🚁" -ForegroundColor Green
Write-Host ""

# Keep window open
Write-Host "Press any key to close this launcher..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
