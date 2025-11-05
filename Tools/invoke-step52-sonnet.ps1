param(
  [Parameter(Mandatory=$true)][string]$ApiKey
)

# Set env var only for this process
$env:ANTHROPIC_API_KEY = $ApiKey

$python = Join-Path $PSScriptRoot "..\Backend_Python\venv\Scripts\python.exe"
if (Test-Path $python) {
  & $python (Join-Path $PSScriptRoot 'send_step52_to_sonnet.py')
} else {
  python (Join-Path $PSScriptRoot 'send_step52_to_sonnet.py')
}