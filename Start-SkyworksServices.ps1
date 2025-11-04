# Start-SkyworksServices.ps1
# Launches both Python and .NET backends for Skyworks AI Suite

param(
    [switch]$Debug = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== Skyworks AI Suite Service Startup ===" -ForegroundColor Cyan
Write-Host "Starting backend services..." -ForegroundColor Yellow

# Set paths
$workspacePath = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"
$pythonBackendPath = Join-Path $workspacePath "Backend_Python"
$dotnetBackendPath = Join-Path $workspacePath "Backend\src\Skyworks.Api"

# Function to check if port is in use
function Test-Port {
    param($Port)
    $tcpConnection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
    return $tcpConnection.TcpTestSucceeded
}

# Function to kill process on port
function Stop-ProcessOnPort {
    param($Port)
    $process = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
    if ($process) {
        Stop-Process -Id $process -Force
        Write-Host "Killed process on port $Port" -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

# Step 1: Clean up existing processes
Write-Host "`nStep 1: Cleaning up existing processes..." -ForegroundColor Green

if (Test-Port 8001) {
    Write-Host "Port 8001 is in use, stopping process..." -ForegroundColor Yellow
    Stop-ProcessOnPort 8001
}

if (Test-Port 5210) {
    Write-Host "Port 5210 is in use, stopping process..." -ForegroundColor Yellow
    Stop-ProcessOnPort 5210
}

# Step 2: Start Python Backend
Write-Host "`nStep 2: Starting Python Backend (Port 8001)..." -ForegroundColor Green

Set-Location $pythonBackendPath

# Check if venv exists
if (Test-Path ".\venv\Scripts\python.exe") {
    Write-Host "Using virtual environment..." -ForegroundColor Cyan
    $pythonExe = ".\venv\Scripts\python.exe"
} else {
    Write-Host "Using system Python..." -ForegroundColor Cyan
    $pythonExe = "python"
}

$pythonProcess = Start-Process -FilePath $pythonExe -ArgumentList "main.py" -PassThru -WindowStyle Normal
Write-Host "Python backend starting with PID: $($pythonProcess.Id)" -ForegroundColor Cyan

# Wait for Python backend to be ready
Write-Host "Waiting for Python backend to be ready..." -ForegroundColor Yellow
$attempts = 0
while (-not (Test-Port 8001) -and $attempts -lt 30) {
    Start-Sleep -Seconds 1
    $attempts++
    Write-Host "." -NoNewline
}

if (Test-Port 8001) {
    Write-Host "`nPython backend is ready!" -ForegroundColor Green
    
    # Test Python health
    try {
        $pythonHealth = Invoke-RestMethod -Uri "http://localhost:8001/" -Method Get
        Write-Host "Python backend status: Healthy" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not verify Python backend health" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nERROR: Python backend failed to start!" -ForegroundColor Red
    exit 1
}

# Step 3: Start .NET Backend
Write-Host "`nStep 3: Starting .NET Backend (Port 5210)..." -ForegroundColor Green

Set-Location $dotnetBackendPath

# Run the .NET backend (skip build for speed)
$dotnetProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:5210" -PassThru -WindowStyle Normal
Write-Host ".NET backend starting with PID: $($dotnetProcess.Id)" -ForegroundColor Cyan

# Wait for .NET backend to be ready
Write-Host "Waiting for .NET backend to be ready..." -ForegroundColor Yellow
$attempts = 0
while (-not (Test-Port 5210) -and $attempts -lt 60) {
    Start-Sleep -Seconds 1
    $attempts++
    if ($attempts % 5 -eq 0) {
        Write-Host "." -NoNewline
    }
}

if (Test-Port 5210) {
    Write-Host "`n.NET backend is ready!" -ForegroundColor Green
    
    # Test .NET health
    try {
        $dotnetHealth = Invoke-RestMethod -Uri "http://localhost:5210/health" -Method Get
        Write-Host ".NET backend status: $($dotnetHealth.status)" -ForegroundColor Green
        
        # Test Python connectivity from .NET
        Start-Sleep -Seconds 2
        $pythonCheck = Invoke-RestMethod -Uri "http://localhost:5210/health/python" -Method Get
        Write-Host "Python backend connectivity: $($pythonCheck.status)" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not verify .NET backend health" -ForegroundColor Yellow
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Gray
    }
} else {
    Write-Host "`nERROR: .NET backend failed to start!" -ForegroundColor Red
    Write-Host "Check the terminal window for error details" -ForegroundColor Yellow
    exit 1
}

# Step 4: Summary
Write-Host "`n=== Service Startup Complete ===" -ForegroundColor Cyan
Write-Host "✅ Python Backend: http://localhost:8001" -ForegroundColor Green
Write-Host "✅ .NET Backend: http://localhost:5210" -ForegroundColor Green
Write-Host "✅ Health Check: http://localhost:5210/health" -ForegroundColor Green
Write-Host "✅ Python Health: http://localhost:5210/health/python" -ForegroundColor Green
Write-Host "✅ Mission Page: http://localhost:5210/app/Pages/mission.html" -ForegroundColor Green
Write-Host "`nPress Ctrl+C to stop all services" -ForegroundColor Yellow

# Keep script running
try {
    while ($true) {
        Start-Sleep -Seconds 60
        
        # Periodic health check
        if (-not (Test-Port 8001)) {
            Write-Host "`nWARNING: Python backend is not responding!" -ForegroundColor Red
        }
        if (-not (Test-Port 5210)) {
            Write-Host "`nWARNING: .NET backend is not responding!" -ForegroundColor Red
        }
    }
} finally {
    Write-Host "`nShutting down services..." -ForegroundColor Yellow
    Stop-Process -Id $pythonProcess.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $dotnetProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Services stopped." -ForegroundColor Green
}
