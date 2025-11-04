param(
    [Parameter(Mandatory = $true, HelpMessage = "Your OpenAI API key (sk-...)")]
    [string]$ApiKey,
    [Parameter(Mandatory = $false, HelpMessage = "OpenAI model name (default: gpt-4o-mini)")]
    [string]$Model = "gpt-4o-mini",
    [switch]$Persist
)

# Set for current PowerShell session
$env:OPENAI_API_KEY = $ApiKey
$env:OPENAI_MODEL = $Model

Write-Host "✅ OPENAI_API_KEY and OPENAI_MODEL set for this PowerShell session" -ForegroundColor Green
Write-Host "   OPENAI_MODEL = $Model"

if ($Persist) {
    [System.Environment]::SetEnvironmentVariable('OPENAI_API_KEY', $ApiKey, 'User')
    [System.Environment]::SetEnvironmentVariable('OPENAI_MODEL', $Model, 'User')
    Write-Host "✅ Persisted to User environment. Restart VS Code to apply in new terminals." -ForegroundColor Green
}

Write-Host "\nTry a quick test:" -ForegroundColor Cyan
Write-Host "python .\\Tools\\TrainingCenter\\agent_llm.py SORA_Compliance_Agent \"What is SAIL level for GRC=3 and ARC=b?\""

Write-Host "\nThen in VS Code Chat:" -ForegroundColor Cyan
Write-Host "@skyworks /ask-sora What are the OSO requirements for SAIL III?"
