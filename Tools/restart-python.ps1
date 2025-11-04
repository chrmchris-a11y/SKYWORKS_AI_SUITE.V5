param(
    [int]$Port = 8001
)

$ErrorActionPreference = 'SilentlyContinue'

Write-Host "Restarting Python FastAPI on port $Port..." -ForegroundColor Cyan

# Kill existing process on port
try {
    $conns = Get-NetTCPConnection -LocalPort $Port -ErrorAction Stop
    $pids = $conns | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($pid in $pids) {
        Write-Host "Killing PID $pid on port $Port" -ForegroundColor Yellow
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 1
} catch {
    Write-Host "No existing process on port $Port" -ForegroundColor DarkGray
}

# Determine Python executable
$workspace = Split-Path -Parent $PSScriptRoot
$backendPy = Join-Path $workspace 'Backend_Python'
$venvPy = Join-Path $backendPy 'venv\Scripts\python.exe'

if (Test-Path $venvPy) {
    $py = $venvPy
} else {
    $py = 'python'
}

Push-Location $backendPy
try {
    Write-Host "Starting uvicorn with --reload..." -ForegroundColor Cyan
    Start-Process -FilePath $py -ArgumentList '-m uvicorn main:app --host 0.0.0.0 --port', $Port, '--reload' -WindowStyle Hidden
    Write-Host "Started." -ForegroundColor Green
} finally {
    Pop-Location
}
