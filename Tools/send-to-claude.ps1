param(
    [Parameter(Mandatory = $false)] [string]$ApiKey,
    [Parameter(Mandatory = $false)] [string]$Model = "claude-3-opus-20240229",
    [Parameter(Mandatory = $true)]  [string]$PromptFile,
    [Parameter(Mandatory = $false)] [int]$MaxTokens = 4000,
    [Parameter(Mandatory = $false)] [double]$Temperature = 0.2,
    [Parameter(Mandatory = $false)] [string]$OutFile = "CLAUDE_OPUS_4_1_FIX_RESPONSE.json"
)

# Usage examples:
#   $env:ANTHROPIC_API_KEY = "<YOUR_KEY>"
#   .\Tools\send-to-claude.ps1 -PromptFile .\CLAUDE_OPUS_4_1_FIX_PROMPT.md
# or
#   .\Tools\send-to-claude.ps1 -ApiKey "<YOUR_KEY>" -PromptFile .\CLAUDE_OPUS_4_1_FIX_PROMPT.md

if (-not $ApiKey) {
    $ApiKey = $env:ANTHROPIC_API_KEY
}
if (-not $ApiKey) {
    Write-Error "Anthropic API key not provided. Set -ApiKey or $env:ANTHROPIC_API_KEY."
    exit 1
}

if (-not (Test-Path $PromptFile)) {
    Write-Error "Prompt file not found: $PromptFile"
    exit 1
}

$prompt = Get-Content -LiteralPath $PromptFile -Raw

$headers = @{
    "x-api-key" = $ApiKey
    "anthropic-version" = "2023-06-01"
    "content-type" = "application/json"
}

$body = @{
    model = $Model
    max_tokens = $MaxTokens
    temperature = $Temperature
    messages = @(@{ role = "user"; content = $prompt })
} | ConvertTo-Json -Depth 10

try {
    Write-Host "Sending prompt to Anthropic model '$Model'..." -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" -Method Post -Headers $headers -Body $body -ErrorAction Stop
    $response | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutFile -Encoding UTF8
    Write-Host "Response saved to $OutFile" -ForegroundColor Green
} catch {
    $msg = $null
    try {
        $webEx = $_.Exception
        if ($webEx -and $webEx.Response) {
            $statusCode = $null
            $statusDescription = $null
            try { $statusCode = [int]$webEx.Response.StatusCode } catch {}
            try { $statusDescription = $webEx.Response.StatusDescription } catch {}
            $stream = $webEx.Response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $errBody = $reader.ReadToEnd()
                $reader.Dispose()
                $msg = "HTTP error${statusCode}: ${statusDescription}. Body: ${errBody}"
            }
        }
    } catch {}
    if (-not $msg) { $msg = $_.Exception.Message }
    Write-Error "Failed to call Anthropic API: $msg"
    exit 1
}
