param(
    [Parameter(Mandatory=$true)] [string]$ApiKey,
    [Parameter(Mandatory=$true)] [string]$PromptPath,
    [Parameter(Mandatory=$true)] [string]$OutPath,
    [string]$Model = 'claude-3-5-sonnet-latest',
    [int]$MaxTokens = 2000,
    [double]$Temperature = 0
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -Path $PromptPath)) {
    throw "Prompt file not found: $PromptPath"
}

try {
    $prompt = Get-Content -Raw -Path $PromptPath

    # Build request body with explicit JSON to avoid hashtable parsing issues
    $promptJson = ConvertTo-Json -InputObject $prompt -Depth 2
    $contentBlocks = "[{`"type`":`"text`",`"text`": " + $promptJson + "}]"
    $body = "{`"model`":`"$Model`",`"max_tokens`":$MaxTokens,`"temperature`":$Temperature,`"messages`": [{`"role`":`"user`",`"content`": $contentBlocks}]}"

    $headers = @{
        'x-api-key' = $ApiKey
        'anthropic-version' = '2023-06-01'
        'content-type' = 'application/json'
    }

    $uri = 'https://api.anthropic.com/v1/messages'
    # Write request body for diagnostics
    $reqPath = Join-Path -Path (Split-Path -Path $OutPath -Parent) -ChildPath 'anthropic_request.json'
    Set-Content -Path $reqPath -Value $body -Encoding UTF8

    $webResp = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers -Body $body
    $resp = $null
    try { $resp = $webResp.Content | ConvertFrom-Json } catch { $resp = $null }

    # Extract text content from response
    $text = $null
    if ($resp -and $resp.content) {
        $text = ($resp.content | ForEach-Object { $_.text }) -join "`n"
    }
    if (-not $text) {
        $text = if ($webResp.Content) { $webResp.Content } else { ($resp | ConvertTo-Json -Depth 8) }
    }

    $outDir = Split-Path -Path $OutPath -Parent
    if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }
    Set-Content -Path $OutPath -Value $text -Encoding UTF8
    Write-Output "Saved: $OutPath"
    exit 0
}
catch {
    $msg = "ERROR: $($_.Exception.Message)"
    $reqInfo = "Request saved to: $reqPath"
    $detail = $null
    try {
        if ($_.Exception.Response) {
            $respStream = $_.Exception.Response.GetResponseStream()
            if ($respStream) {
                $reader = New-Object System.IO.StreamReader($respStream)
                $detail = $reader.ReadToEnd()
                $reader.Close()
            }
        }
    } catch {}
    if ($detail) { $msg = "$msg`nDETAIL: $detail" }
    $msg = "$msg`n$reqInfo"
    Write-Error $msg
    try { Set-Content -Path $OutPath -Value $msg -Encoding UTF8 } catch {}
    exit 1
}
