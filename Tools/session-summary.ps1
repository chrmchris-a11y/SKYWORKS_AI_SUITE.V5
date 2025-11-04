$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$statusPath = Join-Path $root 'Docs/Knowledge/PROJECT_STATUS.json'
if (-not (Test-Path $statusPath)) { Write-Host 'No PROJECT_STATUS.json found.'; exit 0 }
$data = Get-Content -Raw -Path $statusPath | ConvertFrom-Json
Write-Host "==== SKYWORKS SESSION SUMMARY ====" -ForegroundColor Cyan
Write-Host ("Φάση: {0} — {1}" -f $data.currentPhase, $data.phaseTitle)
Write-Host ("Βήμα: {0}" -f $data.currentStep)
Write-Host ("Επόμενο: {0}" -f $data.nextStep)
Write-Host ("Τελευταία ενημέρωση: {0}" -f $data.updatedAt)
if ($data.notes) { Write-Host "Σημειώσεις: $($data.notes)" }
Write-Host "=================================="
