param(
    [string]$BackendPath = "$PSScriptRoot/../Backend",
    [string]$PythonPath = "$PSScriptRoot/../Backend_Python",
    [int]$Port = 8001,
    [int]$HealthTimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'

function Wait-Health($url, $timeoutSeconds) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    do {
        try {
            $r = Invoke-WebRequest -UseBasicParsing -Uri $url -TimeoutSec 3
            if ($r.StatusCode -eq 200) { return $true }
        } catch {
            Start-Sleep -Seconds 1
        }
    } while ((Get-Date) -lt $deadline)
    return $false
}

$pythonProc = $null
try {
    Push-Location $PythonPath

    # Prefer venv python
    $pythonExe = Join-Path $PythonPath 'venv/Scripts/python.exe'
    if (-not (Test-Path $pythonExe)) {
        $pythonExe = 'python'
    }

    # Start uvicorn server
    Write-Host "Starting Python FastAPI on port $Port..."
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $pythonExe
    $psi.Arguments = "-m uvicorn main:app --host 127.0.0.1 --port $Port"
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $pythonProc = New-Object System.Diagnostics.Process
    $pythonProc.StartInfo = $psi
    $null = $pythonProc.Start()

    if (-not (Wait-Health "http://localhost:$Port/health" $HealthTimeoutSeconds)) {
        throw "Python FastAPI health check failed on http://localhost:$Port/health"
    }

    Pop-Location
    Push-Location $BackendPath

    Write-Host "Running dotnet build..."
    dotnet build Skyworks.sln | Write-Host

    Write-Host "Running dotnet test..."
    dotnet test Skyworks.sln --verbosity minimal | Write-Host
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    if ($pythonProc -and -not $pythonProc.HasExited) {
        Write-Host "Stopping Python FastAPI (PID: $($pythonProc.Id))..."
        try { $pythonProc.Kill() } catch {}
        try { $pythonProc.Dispose() } catch {}
    }
}
