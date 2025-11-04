# Requires: PowerShell 5+
param(
    [string]$BaseUrl = "http://localhost:5210",
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'

function Invoke-Train {
    param([string]$Url)
    try {
        $resp = Invoke-RestMethod -UseBasicParsing -Uri "$Url/api/knowledge/train" -Method POST -TimeoutSec 10
        return $resp
    } catch {
        return $null
    }
}

$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
Write-Host "[train-knowledge] Waiting for API at $BaseUrl (timeout: $TimeoutSeconds s)" -ForegroundColor Cyan
while ((Get-Date) -lt $deadline) {
    $result = Invoke-Train -Url $BaseUrl
    if ($null -ne $result) {
        Write-Host ("[train-knowledge] Indexed: {0} | Status: {1}" -f $result.indexed, $result.status) -ForegroundColor Green
        exit 0
    }
    Start-Sleep -Seconds 2
}

Write-Host "[train-knowledge] API not ready or training failed within timeout." -ForegroundColor Yellow
exit 1
