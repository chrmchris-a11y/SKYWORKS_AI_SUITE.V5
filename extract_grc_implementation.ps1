# Extract GRC implementation files from Sonnet 4 response
# Similar to deploy_arc_implementation.ps1

$ErrorActionPreference = "Stop"

Write-Host "📦 Extracting GRC implementation files from Sonnet 4 response..." -ForegroundColor Cyan
Write-Host ""

# Read response
$responseFile = "SONNET_GRC_RESPONSE_20251030_155759.txt"
$responseContent = Get-Content $responseFile -Raw -Encoding UTF8

Write-Host "📄 Response size: $($responseContent.Length.ToString('N0')) characters" -ForegroundColor Yellow

# Create GRC directory structure
$grcDir = "Backend_Python\grc"
$dirs = @(
    "$grcDir",
    "$grcDir\models",
    "$grcDir\calculators",
    "$grcDir\validators",
    "$grcDir\rules",
    "$grcDir\tests"
)

foreach ($dir in $dirs) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "✅ Created directory: $dir" -ForegroundColor Green
    }
}

# Create __init__.py files
$initFiles = @(
    "$grcDir\__init__.py",
    "$grcDir\models\__init__.py",
    "$grcDir\calculators\__init__.py",
    "$grcDir\validators\__init__.py",
    "$grcDir\tests\__init__.py"
)

foreach ($initFile in $initFiles) {
    '"""GRC (Ground Risk Class) calculation module for SORA 2.0 and 2.5"""' | Out-File -FilePath $initFile -Encoding UTF8
    Write-Host "✅ Created: $initFile" -ForegroundColor Green
}

# Function to extract code blocks
function Extract-CodeBlock {
    param(
        [string]$Content,
        [string]$Marker,
        [string]$Language
    )
    
    # Escape special regex characters in marker
    $escapedMarker = [regex]::Escape($Marker)
    
    # Pattern: ### `marker` followed by ```language ... ```
    $pattern = "(?s)### ``$escapedMarker``\s+````$Language\s+(.*?)````"
    
    if ($Content -match $pattern) {
        return $Matches[1].Trim()
    }
    return $null
}

# Extract files
$files = @(
    @{
        Marker = "grc_rules_sora_2_0.yaml"
        Path = "$grcDir\rules\grc_rules_sora_2_0.yaml"
        Language = "yaml"
    },
    @{
        Marker = "grc_rules_sora_2_5.yaml"
        Path = "$grcDir\rules\grc_rules_sora_2_5.yaml"
        Language = "yaml"
    },
    @{
        Marker = "models/grc_models.py"
        Path = "$grcDir\models\grc_models.py"
        Language = "python"
    },
    @{
        Marker = "validators/grc_validator.py"
        Path = "$grcDir\validators\grc_validator.py"
        Language = "python"
    },
    @{
        Marker = "calculators/grc_calculator.py"
        Path = "$grcDir\calculators\grc_calculator.py"
        Language = "python"
    },
    @{
        Marker = "grc_api.py"
        Path = "$grcDir\grc_api.py"
        Language = "python"
    },
    @{
        Marker = "tests/test_grc_calculator_2_0.py"
        Path = "$grcDir\tests\test_grc_calculator_2_0.py"
        Language = "python"
    },
    @{
        Marker = "tests/test_grc_calculator_2_5.py"
        Path = "$grcDir\tests\test_grc_calculator_2_5.py"
        Language = "python"
    }
)

Write-Host ""
Write-Host "📂 Extracting files..." -ForegroundColor Cyan

foreach ($file in $files) {
    Write-Host "  Processing: $($file.Marker)" -ForegroundColor Yellow
    
    $content = Extract-CodeBlock -Content $responseContent -Marker $file.Marker -Language $file.Language
    
    if ($content) {
        $content | Out-File -FilePath $file.Path -Encoding UTF8
        $lines = ($content -split "`n").Count
        Write-Host "    ✅ Extracted $lines lines to $($file.Path)" -ForegroundColor Green
    } else {
        Write-Host "    ⚠️  Could not find code block for $($file.Marker)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "✅ Extraction complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Cyan
Get-ChildItem $grcDir -Recurse -File | ForEach-Object {
    $relPath = $_.FullName.Replace((Get-Location).Path + "\", "")
    $size = "{0:N0}" -f $_.Length
    Write-Host "  $relPath ($size bytes)" -ForegroundColor White
}

Write-Host ""
Write-Host "📝 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review extracted files for any issues" -ForegroundColor White
Write-Host "  2. Fix any Pydantic V2 compatibility issues" -ForegroundColor White
Write-Host "  3. Run tests: pytest grc/tests/ -v" -ForegroundColor White
