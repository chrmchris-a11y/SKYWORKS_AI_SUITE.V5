# ====================================================================
# Claude Sonnet 4.5 - Complete Automation Script
# Usage: .\Invoke-Sonnet45.ps1
# ====================================================================

param(
    [string]$ApiKey = $env:ANTHROPIC_API_KEY,
    # Default to actual backend path
    [string]$SourceFile = ".\Backend\src\Skyworks.Core\Services\Orchestration\SORAOrchestrationService.cs",
    [string]$OutputDir = ".\sonnet_output",
    [switch]$AutoApply
)

$ErrorActionPreference = 'Stop'

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Claude Sonnet 4.5 - Code Fix Automation           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Validate API key
if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    Write-Host "❌ ERROR: ANTHROPIC_API_KEY not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Set it with:" -ForegroundColor Yellow
    Write-Host '  $env:ANTHROPIC_API_KEY = "sk-ant-api03-YOUR-KEY-HERE"' -ForegroundColor White
    Write-Host ""
    exit 1
}

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "⚙️  Configuration:" -ForegroundColor Yellow
Write-Host "   API Key: $($ApiKey.Substring(0,12))..." -ForegroundColor Gray
Write-Host "   Model: claude-sonnet-4-5-20250929" -ForegroundColor Gray
Write-Host "   Source: $SourceFile" -ForegroundColor Gray
Write-Host "   Output: $OutputDir" -ForegroundColor Gray
Write-Host ""

# ====================================================================
# Step 1: Create YAML Calibration
# ====================================================================
Write-Host "📋 Step 1: Creating YAML calibration..." -ForegroundColor Cyan

$yamlContent = @'
# SORA Orchestration Service - Sonnet 4.5 Calibration

critical_fixes:
  grc_2_0:
    - Remove: "Population density inference from Scenario"
    - Action: "Pass Scenario enum directly to Python engine"
    - Validation: "input.Scenario_V2_0 required, no PD mapping"
    
  grc_2_5:
    - Remove: "MTOM_kg fallback to MaxCharacteristicDimension"
    - Action: "Require explicit MTOM_kg and MaxSpeed - reject if missing"
    - Sub250g: "Apply iGRC=1 only if MTOM ≤ 0.25kg AND speed within bin"
    
  arc_inputs:
    - Add: "IsNearAerodrome, DistanceToAerodrome_km, IsInCTR"
    - Units: "2.0=feet (convert), 2.5=meters (direct)"
    - Normalization: "Suburban/Industrial → Urban"
    - Remove: "Default MaxHeightAGL=120m"
    
  explicit_arc:
    - Restrict: "Only in test mode (_allowExplicitARC flag)"
    - Production: "Always derive from AEC decision tree"
    
  scope_validation:
    - Add: "ReasonCode: OOS.SAIL_VI, OOS.GRC_GE_6, OOS.GRC5_PLUS_ARCd"
    - Field: "result.OutOfScopeReason for HTTP 400"
    
  async_pattern:
    - Remove: "All .Result blocking calls"
    - Pattern: "async/await with sync wrapper"

required_model_changes:
  PythonGRCRequest_2_0:
    add: ["string Scenario"]
    remove: ["int PopulationDensity", "double MTOM_kg", "string EnvironmentType"]
  
  PythonGRCRequest_2_5:
    add: ["double MaxSpeed_mps"]
  
  PythonARCRequest_2_0:
    add: ["double? DistanceToAerodrome_nm", "bool IsInCTR"]
  
  PythonARCRequest_2_5:
    add: ["double? DistanceToAerodrome_km", "bool IsInCTR", "bool IsNearAerodrome"]
  
  ARCEnvironmentInput:
    add: ["bool IsNearAerodrome", "double? DistanceToAerodrome_km", "bool IsInCTR"]
  
  AirRiskInput:
    add: ["bool? IsNearAerodrome", "double? DistanceToAerodrome_km", "bool? IsInCTR", "double? MaxSpeed"]
  
  SORACompleteResult:
    add: ["string? OutOfScopeReason"]
'@

$yamlPath = Join-Path $OutputDir "calibration.yaml"
Set-Content -Path $yamlPath -Value $yamlContent -Encoding UTF8
Write-Host "   ✓ Saved to: $yamlPath" -ForegroundColor Green

# ====================================================================
# Step 2: Read Source Code
# ====================================================================
Write-Host "📖 Step 2: Reading source code..." -ForegroundColor Cyan

if (-not (Test-Path $SourceFile)) {
    Write-Host "   ❌ Source file not found: $SourceFile" -ForegroundColor Red
    exit 1
}

$sourceCode = Get-Content -Path $SourceFile -Raw -Encoding UTF8
Write-Host "   ✓ Read $($sourceCode.Length) characters" -ForegroundColor Green

# ====================================================================
# Step 3: Create Prompt
# ====================================================================
Write-Host "✍️  Step 3: Creating prompt..." -ForegroundColor Cyan

$prompt = @"
You are an expert C# developer fixing critical bugs in a SORA (aviation risk assessment) service.

YAML CALIBRATION (apply ALL fixes):
$yamlContent

CURRENT CODE:
``````csharp
$sourceCode
``````

TASK:
1. Apply EVERY fix from the YAML calibration
2. Return COMPLETE updated SORAOrchestrationService.cs
3. Return ALL model classes that need updates (as separate code blocks)
4. Ensure all async patterns are correct (no .Result calls)
5. Add XML comments for new methods/properties

RESPONSE FORMAT:
Return each file in a separate code block with clear headers:

# SORAOrchestrationService.cs
``````csharp
[complete code here]
``````

# PythonGRCRequest_2_0.cs
``````csharp
[complete code here]
``````

[etc for each model that needs updates]

Return ONLY code blocks. No explanations before or after.
"@

$promptPath = Join-Path $OutputDir "prompt.txt"
Set-Content -Path $promptPath -Value $prompt -Encoding UTF8
Write-Host "   ✓ Saved to: $promptPath" -ForegroundColor Green

# ====================================================================
# Step 4: Call Sonnet 4.5 API
# ====================================================================
Write-Host "🚀 Step 4: Calling Claude Sonnet 4.5..." -ForegroundColor Cyan
Write-Host "   (This may take 30-60 seconds...)" -ForegroundColor Gray
Write-Host ""

$headers = @{
    'x-api-key' = $ApiKey
    'anthropic-version' = '2023-06-01'
    'content-type' = 'application/json; charset=utf-8'
}

# Escape prompt for JSON
$promptEscaped = $prompt `
    -replace '\\', '\\' `
    -replace '"', '\"' `
    -replace "`n", '\n' `
    -replace "`r", '' `
    -replace "`t", '\t'

$requestBody = @"
{
    "model": "claude-sonnet-4-5-20250929",
    "max_tokens": 16000,
    "temperature": 0,
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

try {
    $response = Invoke-WebRequest `
        -Uri "https://api.anthropic.com/v1/messages" `
        -Method Post `
        -Headers $headers `
        -Body ([System.Text.Encoding]::UTF8.GetBytes($requestBody)) `
        -TimeoutSec 300 `
        -UseBasicParsing
    
    $responseJson = $response.Content | ConvertFrom-Json
    
    # Extract text
    $responseText = ($responseJson.content | Where-Object { $_.type -eq "text" } | ForEach-Object { $_.text }) -join "`n"
    
    # Save raw response
    $responsePath = Join-Path $OutputDir "response.txt"
    Set-Content -Path $responsePath -Value $responseText -Encoding UTF8
    
    Write-Host "   ✓ API call successful!" -ForegroundColor Green
    Write-Host "   📊 Tokens: Input=$($responseJson.usage.input_tokens), Output=$($responseJson.usage.output_tokens)" -ForegroundColor Gray
    Write-Host ""
    
    # ====================================================================
    # Step 5: Parse Code Blocks
    # ====================================================================
    Write-Host "📦 Step 5: Extracting code blocks..." -ForegroundColor Cyan
    
    # Regex to find code blocks with optional filenames
    $codeBlockPattern = '(?:^|\n)(?:#\s*(.+?)\.cs\s*\n)?```(?:csharp|cs)?\s*\n(.*?)\n```'
    $codeMatches = [regex]::Matches($responseText, $codeBlockPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    if ($codeMatches.Count -eq 0) {
        Write-Host "   ⚠️  No code blocks found in response" -ForegroundColor Yellow
        Write-Host "   💾 Raw response saved to: $responsePath" -ForegroundColor Gray
    } else {
        $extractedFiles = @()
        
        for ($i = 0; $i -lt $codeMatches.Count; $i++) {
            $match = $codeMatches[$i]
            $filename = $match.Groups[1].Value.Trim()
            $code = $match.Groups[2].Value.Trim()
            
            if ([string]::IsNullOrWhiteSpace($filename)) {
                $filename = "CodeBlock_$($i+1).cs"
            }
            
            $outputPath = Join-Path $OutputDir $filename
            Set-Content -Path $outputPath -Value $code -Encoding UTF8
            
            Write-Host "   ✓ Extracted: $filename ($($code.Length) chars)" -ForegroundColor Green
            
            $extractedFiles += @{
                Name = $filename
                Path = $outputPath
                Size = $code.Length
            }
        }
        
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host "✅ SUCCESS! Extracted $($extractedFiles.Count) file(s)" -ForegroundColor Green
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host ""
        
        # Display summary
        Write-Host "📁 Files in: $OutputDir" -ForegroundColor Cyan
        foreach ($file in $extractedFiles) {
            Write-Host "   • $($file.Name)" -ForegroundColor White
        }
        
        # ====================================================================
        # Step 6: Auto-apply (optional)
        # ====================================================================
        if ($AutoApply) {
            Write-Host ""
            Write-Host "🔄 Step 6: Auto-applying changes..." -ForegroundColor Cyan
            
            foreach ($file in $extractedFiles) {
                $targetPath = $null
                
                # Determine target path based on filename
                if ($file.Name -match "SORAOrchestrationService") {
                    $targetPath = $SourceFile
                } elseif ($file.Name -match "PythonCalculationClient|Python.*Request|Python.*Response") {
                    # These classes live in a single file in this repo
                    $targetPath = ".\Backend\src\Skyworks.Core\Services\Python\PythonCalculationClient.cs"
                } elseif ($file.Name -match "ISORAOrchestrationService") {
                    $targetPath = ".\Backend\src\Skyworks.Core\Services\Orchestration\ISORAOrchestrationService.cs"
                } elseif ($file.Name -match "ARCEnvironmentInput|ARCEnvironment") {
                    $targetPath = ".\Backend\src\Skyworks.Core\Models\ARC\ARCEnvironment.cs"
                } elseif ($file.Name -match "AirRiskInput|SORACompleteResult") {
                    # These types are defined within ISORAOrchestrationService.cs
                    $targetPath = ".\Backend\src\Skyworks.Core\Services\Orchestration\ISORAOrchestrationService.cs"
                }
                
                if ($targetPath -and (Test-Path (Split-Path $targetPath -Parent))) {
                    Copy-Item -Path $file.Path -Destination $targetPath -Force
                    Write-Host "   ✓ Applied: $($file.Name) → $targetPath" -ForegroundColor Green
                } else {
                    Write-Host "   ⚠️  Skipped: $($file.Name) (unknown target)" -ForegroundColor Yellow
                }
            }
        }
        
        Write-Host ""
        Write-Host "🎯 Next Steps:" -ForegroundColor Yellow
        Write-Host "   1. Review files in: $OutputDir" -ForegroundColor White
        Write-Host "   2. Copy to project: cp $OutputDir\*.cs .\Skyworks.Core\..." -ForegroundColor White
        Write-Host "   3. Build: dotnet build" -ForegroundColor White
        Write-Host "   4. Test: dotnet test --filter SORA" -ForegroundColor White
        Write-Host ""
        
        if (-not $AutoApply) {
            Write-Host "💡 Tip: Use -AutoApply to automatically copy files" -ForegroundColor Gray
            Write-Host ""
        }
    }
    
} catch {
    Write-Host "   ❌ API call failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    # Try to extract error details
    try {
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorDetail = $reader.ReadToEnd()
            $reader.Close()
            
            Write-Host ""
            Write-Host "Details:" -ForegroundColor Yellow
            Write-Host $errorDetail -ForegroundColor Gray
            
            # Save error
            $errorPath = Join-Path $OutputDir "error.txt"
            Set-Content -Path $errorPath -Value $errorDetail -Encoding UTF8
        }
    } catch {}
    
    exit 1
}
