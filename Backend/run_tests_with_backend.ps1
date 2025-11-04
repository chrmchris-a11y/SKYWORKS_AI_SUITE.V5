# AUTOMATED TEST RUNNER WITH BACKEND PROCESS MANAGEMENT
# Starts backend in separate process, runs all tests, keeps backend alive

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SKYWORKS COMPREHENSIVE TEST SUITE" -ForegroundColor Cyan
Write-Host "  20 In-Depth SORA Accuracy Tests" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Kill any existing dotnet processes on port 5210
Write-Host "🔧 Cleaning up any existing processes..." -ForegroundColor Yellow
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Start backend in separate process
Write-Host "🚀 Starting backend API server..." -ForegroundColor Green
$backendPath = "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\src\Skyworks.Api"
$backendJob = Start-Process powershell -ArgumentList "-NoProfile", "-Command", "cd '$backendPath' ; dotnet run" -PassThru -WindowStyle Minimized

Write-Host "   Backend process started (PID: $($backendJob.Id))" -ForegroundColor Green
Write-Host "   Waiting 10 seconds for backend to initialize..." -ForegroundColor Yellow

Start-Sleep -Seconds 10

# Verify backend is running
Write-Host "🔍 Verifying backend health..." -ForegroundColor Yellow
$maxRetries = 5
$retryCount = 0
$backendReady = $false

while ($retryCount -lt $maxRetries -and -not $backendReady) {
    try {
        # Test with a simple OPTIONS request or just check if port is listening
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("localhost", 5210)
        $tcpClient.Close()
        $backendReady = $true
        Write-Host "   ✅ Backend is healthy and ready!" -ForegroundColor Green
    } catch {
        $retryCount++
        Write-Host "   Retry $retryCount/$maxRetries..." -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

if (-not $backendReady) {
    Write-Host "   ❌ Backend failed to start!" -ForegroundColor Red
    Stop-Process -Id $backendJob.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Running 20 Comprehensive Tests..." -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Run the comprehensive test script
try {
    & "c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\run_comprehensive_tests.ps1"
} catch {
    Write-Host "❌ Error running tests: $($_.Exception.Message)" -ForegroundColor Red
}

# Keep backend alive for inspection
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  Tests Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Backend is still running (PID: $($backendJob.Id))" -ForegroundColor Yellow
Write-Host "You can:" -ForegroundColor Yellow
Write-Host "  - Test manually: http://localhost:5210" -ForegroundColor Cyan
Write-Host "  - Stop backend: Press Ctrl+C or close this window" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Enter to stop backend and exit..." -ForegroundColor Yellow
Read-Host

# Stop backend
Write-Host "Stopping backend..." -ForegroundColor Yellow
Stop-Process -Id $backendJob.Id -Force -ErrorAction SilentlyContinue
Write-Host "Done!" -ForegroundColor Green
