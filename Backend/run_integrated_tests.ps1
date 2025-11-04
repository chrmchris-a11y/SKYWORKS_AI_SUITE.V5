# INTEGRATED TEST RUNNER - Start Python service, run .NET tests, cleanup
param(
    [int]$PythonPort = 8001
)

$ErrorActionPreference = "Continue"

Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SKYWORKS INTEGRATED TEST SUITE" -ForegroundColor Cyan
Write-Host "  Python Service + .NET Tests" -ForegroundColor Cyan  
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Cleanup any existing Python processes
Write-Host "🧹 Cleaning up existing Python processes..." -ForegroundColor Yellow
Stop-Process -Name python -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Start Python service in background
Write-Host "🚀 Starting Python calculation service on port $PythonPort..." -ForegroundColor Green
$pythonPath = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"
$venvPython = "$pythonPath\Backend_Python\venv\Scripts\python.exe"
$env:PYTHONPATH = $pythonPath

$pythonProcess = Start-Process -FilePath $venvPython `
    -ArgumentList "-m", "uvicorn", "Backend_Python.main:app", "--host", "127.0.0.1", "--port", $PythonPort `
    -WorkingDirectory $pythonPath `
    -PassThru `
    -WindowStyle Hidden

Write-Host "   Python service started (PID: $($pythonProcess.Id))" -ForegroundColor Green  
Write-Host "   Waiting for service to be ready..." -ForegroundColor Yellow

# Health check with retry
$maxRetries = 10
$retryCount = 0
$serviceReady = $false

while ($retryCount -lt $maxRetries -and -not $serviceReady) {
    Start-Sleep -Seconds 1
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:$PythonPort/health" -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $serviceReady = $true
            Write-Host "   ✅ Python service is healthy and ready!" -ForegroundColor Green
        }
    } catch {
        $retryCount++
        Write-Host "   Retry $retryCount/$maxRetries..." -ForegroundColor Yellow
    }
}

if (-not $serviceReady) {
    Write-Host "   ❌ Python service failed to start!" -ForegroundColor Red
    Stop-Process -Id $pythonProcess.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Running .NET Test Suite..." -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Run .NET tests
try {
    cd "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend"
    dotnet test Skyworks.sln --no-build --logger "console;verbosity=minimal"
    $testExitCode = $LASTEXITCODE
} catch {
    Write-Host "❌ Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    $testExitCode = 1
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Cleanup" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Stop Python service
Write-Host "🛑 Stopping Python service (PID: $($pythonProcess.Id))..." -ForegroundColor Yellow
Stop-Process -Id $pythonProcess.Id -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host "✅ Done!" -ForegroundColor Green
Write-Host ""

exit $testExitCode
