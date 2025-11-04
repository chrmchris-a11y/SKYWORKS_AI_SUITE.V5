param(
  [Parameter(Mandatory=$false)][string]$DatasetPath = "./dataset.sora.samples.json",
  [Parameter(Mandatory=$false)][string]$PromptPath = "./prompt-sora-trainer-en.txt",
  [Parameter(Mandatory=$false)][string]$Model = "gpt-4o-mini",
  [Parameter(Mandatory=$false)][string]$Endpoint = $env:AI_ENDPOINT,
  [Parameter(Mandatory=$false)][string]$ApiKey = $env:AI_API_KEY
)

# Simple prompt runner scaffold. It does not call any external service by default.
# Fill in $Endpoint and $ApiKey via environment variables or parameters to enable real runs.

if (-not (Test-Path $DatasetPath)) { Write-Error "Dataset not found: $DatasetPath"; exit 1 }
if (-not (Test-Path $PromptPath))  { Write-Error "Prompt file not found: $PromptPath"; exit 1 }

$dataset = Get-Content $DatasetPath -Raw | ConvertFrom-Json
$prompt  = Get-Content $PromptPath -Raw

Write-Host "Loaded $($dataset.Count) samples and prompt '$PromptPath'"

# Dry-run: print the first 2 assembled messages
$preview = 0
foreach ($item in $dataset) {
  $system = $prompt
  $user = @{
    soraVersion = $item.soraVersion
    drone = $item.drone
    groundRisk = $item.groundRisk
    airRisk = $item.airRisk
    mitigations = $item.mitigations
  } | ConvertTo-Json -Depth 6

  Write-Host "--- Sample: $($item.id) ---"
  Write-Host "SYSTEM>" ($system.Substring(0, [Math]::Min(160, $system.Length)))
  Write-Host "USER>" $user

  $preview++
  if ($preview -ge 2) { break }
}

if (-not $Endpoint -or -not $ApiKey) {
  Write-Warning "No endpoint/API key configured. Set AI_ENDPOINT and AI_API_KEY to run actual calls. Exiting after preview."
  exit 0
}

# Example: call an OpenAI-compatible endpoint (pseudo; adapt to your client)
# This section is intentionally left as guidance to avoid leaking keys.
# You can replace with Invoke-RestMethod for your provider.
<#[
$headers = @{ Authorization = "Bearer $ApiKey"; "Content-Type" = "application/json" }
foreach ($item in $dataset) {
  $messages = @(
    @{ role = "system"; content = $prompt },
    @{ role = "user"; content = (@{ input = $item } | ConvertTo-Json -Depth 6) }
  )
  $body = @{ model = $Model; messages = $messages } | ConvertTo-Json -Depth 6
  $resp = Invoke-RestMethod -Uri "$Endpoint/chat/completions" -Method Post -Headers $headers -Body $body
  # TODO: capture and evaluate $resp against $item.expected
}
]#

Write-Host "Prompt runner finished (dry-run)."