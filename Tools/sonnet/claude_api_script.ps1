param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$true)]
    [string]$PromptPath,
    
    [Parameter(Mandatory=$true)]
    [string]$OutPath,
    
    [string]$Model = 'claude-3-5-sonnet-20241022',
    [int]$MaxTokens = 8000,
    [double]$Temperature = 0,
    
    [switch]$LogVerbose,
    [string]$ApiUrl = 'https://api.anthropic.com/v1/messages'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Helper function for verbose logging
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    if ($LogVerbose -or $Level -eq "ERROR") {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $(
            switch ($Level) {
                "ERROR" { "Red" }
                "WARN" { "Yellow" }
                "SUCCESS" { "Green" }
                default { "White" }
            }
        )
    }
}

try {
    Write-Log "Starting Claude API call..." "INFO"
    Write-Log "Model: $Model, MaxTokens: $MaxTokens, Temperature: $Temperature" "INFO"
    
    # Validate prompt file exists
    if (-not (Test-Path -Path $PromptPath -PathType Leaf)) {
        throw "Prompt file not found: $PromptPath"
    }
    
    $promptFullPath = Resolve-Path -Path $PromptPath
    Write-Log "Reading prompt from: $promptFullPath" "INFO"
    
    # Read prompt content with UTF8 encoding
    $promptContent = Get-Content -Raw -Path $PromptPath -Encoding UTF8
    if ([string]::IsNullOrWhiteSpace($promptContent)) {
        throw "Prompt file is empty: $PromptPath"
    }
    
    Write-Log "Prompt length: $($promptContent.Length) characters" "INFO"
    
    # Build request body properly with escaped JSON
    # Escape the prompt content for JSON
    $promptEscaped = $promptContent `
        -replace '\\', '\\' `
        -replace '"', '\"' `
        -replace "`n", '\n' `
        -replace "`r", '\r' `
        -replace "`t", '\t'
    
    # Build JSON manually to avoid PowerShell hashtable issues
    $requestBody = @"
{
    "model": "$Model",
    "max_tokens": $MaxTokens,
    "temperature": $Temperature,
    "messages": [
        {
            "role": "user",
            "content": [
                {
                    "type": "text",
                    "text": "$promptEscaped"
                }
            ]
        }
    ]
}
"@
    
    Write-Log "Request body size: $($requestBody.Length) bytes" "INFO"
    
    # Set up headers
    $headers = @{
        'x-api-key' = $ApiKey
        'anthropic-version' = '2023-06-01'
        'content-type' = 'application/json; charset=utf-8'
    }
    
    # Make API call
    $uri = $ApiUrl
    Write-Log "Calling Claude API..." "INFO"
    
    # Persist the request body for debugging
    try {
        $reqPath = Join-Path -Path (Split-Path -Path $OutPath -Parent) -ChildPath 'anthropic_request.json'
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($reqPath, $requestBody, $utf8NoBom)
        Write-Log "Saved request body to: $reqPath" "INFO"
    } catch {}

    $webResponse = Invoke-WebRequest `
        -Uri $uri `
        -Method Post `
        -Headers $headers `
        -Body ([System.Text.Encoding]::UTF8.GetBytes($requestBody)) `
        -TimeoutSec 300 `
        -UseBasicParsing
    
    Write-Log "API call successful (HTTP $($webResponse.StatusCode))" "SUCCESS"
    
    # Parse response
    $responseJson = $null
    try {
        $responseJson = $webResponse.Content | ConvertFrom-Json
    }
    catch {
        Write-Log "Failed to parse JSON response: $($_.Exception.Message)" "WARN"
        throw "Invalid JSON response from API"
    }
    
    # Extract text content from response
    $outputText = $null
    
    if ($responseJson.content -and $responseJson.content.Count -gt 0) {
        # Standard response format
        $textBlocks = $responseJson.content | Where-Object { $_.type -eq "text" }
        if ($textBlocks) {
            $outputText = ($textBlocks | ForEach-Object { $_.text }) -join "`n`n"
        }
    }
    
    if ([string]::IsNullOrWhiteSpace($outputText)) {
        # Fallback: try to get any text from response
        if ($responseJson.PSObject.Properties["text"]) {
            $outputText = $responseJson.text
        }
        else {
            Write-Log "No text content found in response" "WARN"
            $outputText = "ERROR: No text content in API response`n`nRaw response:`n$($webResponse.Content)"
        }
    }
    
    Write-Log "Extracted response length: $($outputText.Length) characters" "INFO"
    
    # Ensure output directory exists
    $outDir = Split-Path -Path $OutPath -Parent
    if (-not [string]::IsNullOrWhiteSpace($outDir) -and -not (Test-Path -Path $outDir)) {
        Write-Log "Creating output directory: $outDir" "INFO"
        New-Item -ItemType Directory -Path $outDir -Force | Out-Null
    }
    
    # Write output file with UTF8 encoding (no BOM)
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($OutPath, $outputText, $utf8NoBom)
    
    $outFullPath = Resolve-Path -Path $OutPath
    Write-Log "Response saved to: $outFullPath" "SUCCESS"
    
    # Display usage info if available
    if ($responseJson.usage) {
        Write-Log "Token usage: Input=$($responseJson.usage.input_tokens), Output=$($responseJson.usage.output_tokens)" "INFO"
    }
    
    Write-Host "`nSUCCESS: Response saved to $OutPath" -ForegroundColor Green
    exit 0
}
catch {
    $errorMsg = $_.Exception.Message
    $errorDetail = $null
    
    # Try to extract detailed error from HTTP response
    try {
        if ($_.Exception.Response) {
            $responseStream = $_.Exception.Response.GetResponseStream()
            if ($responseStream) {
                $reader = New-Object System.IO.StreamReader($responseStream)
                $errorDetail = $reader.ReadToEnd()
                $reader.Close()
                
                # Try to parse error JSON
                try {
                    $errorJson = $errorDetail | ConvertFrom-Json
                    if ($errorJson.error -and $errorJson.error.message) {
                        $errorMsg = "API Error: $($errorJson.error.type) - $($errorJson.error.message)"
                    }
                }
                catch {
                    # Could not parse as JSON, use raw detail
                }
            }
        }
    }
    catch {
        # Ignore errors in error handling
    }
    
    $fullErrorMsg = "ERROR: $errorMsg"
    if ($errorDetail) {
        $fullErrorMsg += "`n`nDETAIL:`n$errorDetail"
    }
    
    Write-Log $errorMsg "ERROR"
    
    # Try to write error to output file
    try {
        $outDir = Split-Path -Path $OutPath -Parent
        if (-not [string]::IsNullOrWhiteSpace($outDir) -and -not (Test-Path -Path $outDir)) {
            New-Item -ItemType Directory -Path $outDir -Force | Out-Null
        }
        
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($OutPath, $fullErrorMsg, $utf8NoBom)
        
        Write-Log "Error details saved to: $OutPath" "WARN"
    }
    catch {
        Write-Log "Could not write error to output file: $($_.Exception.Message)" "ERROR"
    }
    
    Write-Host "`nFAILED: $errorMsg" -ForegroundColor Red
    exit 1
}
