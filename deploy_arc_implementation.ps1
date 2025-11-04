#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Extract and deploy ARC implementation from Sonnet 4 output
.DESCRIPTION
    Parses Sonnet 4 markdown, extracts all code files, and deploys to Backend_Python/arc/
#>

param(
    [string]$MarkdownFile = "SONNET4_ARC_OUTPUT\sonnet4_arc_implementation_20251030_152353.md",
    [string]$OutputDir = "Backend_Python\arc",
    [switch]$DryRun
)

if (-not (Test-Path $MarkdownFile)) {
    Write-Error "❌ Markdown file not found: $MarkdownFile"
    exit 1
}

Write-Host "🚀 Extracting ARC Implementation from Sonnet 4 Output" -ForegroundColor Cyan
Write-Host "📖 Source: $MarkdownFile" -ForegroundColor Gray
Write-Host "📁 Target: $OutputDir" -ForegroundColor Gray
Write-Host ""

# Read content
$content = Get-Content $MarkdownFile -Raw

# Define file patterns to extract
$filePatterns = @(
    @{
        Name = "arc_rules_sora_2_0.yaml"
        Pattern = '### arc_rules_sora_2_0\.yaml\s+```yaml\s+([\s\S]+?)```'
        TargetDir = "rules"
    },
    @{
        Name = "arc_rules_sora_2_5.yaml"
        Pattern = '### arc_rules_sora_2_5\.yaml\s+```yaml\s+([\s\S]+?)```'
        TargetDir = "rules"
    },
    @{
        Name = "arc_models.py"
        Pattern = '### arc_models\.py\s+```python\s+([\s\S]+?)```'
        TargetDir = "models"
    },
    @{
        Name = "arc_calculator.py"
        Pattern = '### arc_calculator\.py\s+```python\s+([\s\S]+?)```'
        TargetDir = "calculators"
    },
    @{
        Name = "arc_validator.py"
        Pattern = '### arc_validator\.py\s+```python\s+([\s\S]+?)```'
        TargetDir = "validators"
    },
    @{
        Name = "arc_api.py"
        Pattern = '### arc_api\.py\s+```python\s+([\s\S]+?)```'
        TargetDir = "."
    },
    @{
        Name = "test_arc_calculator.py"
        Pattern = '### test_arc_calculator\.py\s+```python\s+([\s\S]+?)```'
        TargetDir = "tests"
    }
)

# Create directory structure
$dirs = @(
    $OutputDir,
    "$OutputDir\models",
    "$OutputDir\calculators",
    "$OutputDir\validators",
    "$OutputDir\rules",
    "$OutputDir\tests"
)

Write-Host "📁 Creating directory structure..." -ForegroundColor Yellow
foreach ($dir in $dirs) {
    if (-not (Test-Path $dir)) {
        if ($DryRun) {
            Write-Host "   [DRY RUN] Would create: $dir" -ForegroundColor Gray
        } else {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "   ✅ Created: $dir" -ForegroundColor Green
        }
    } else {
        Write-Host "   ℹ️  Exists: $dir" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "📄 Extracting files..." -ForegroundColor Yellow

$filesCreated = 0
$filesFailed = 0

foreach ($fileInfo in $filePatterns) {
    $fileName = $fileInfo.Name
    $pattern = $fileInfo.Pattern
    $targetSubDir = $fileInfo.TargetDir
    
    if ($targetSubDir -eq ".") {
        $targetPath = Join-Path $OutputDir $fileName
    } else {
        $targetPath = Join-Path $OutputDir $targetSubDir $fileName
    }
    
    # Try to match pattern
    if ($content -match $pattern) {
        $code = $matches[1].Trim()
        $lineCount = ($code -split "`n").Count
        
        if ($DryRun) {
            Write-Host "   [DRY RUN] Would create: $targetPath ($lineCount lines)" -ForegroundColor Gray
            $filesCreated++
        } else {
            try {
                $code | Set-Content $targetPath -Encoding UTF8
                Write-Host "   ✅ Created: $targetPath ($lineCount lines)" -ForegroundColor Green
                $filesCreated++
            } catch {
                Write-Host "   ❌ Failed: $targetPath - $($_.Exception.Message)" -ForegroundColor Red
                $filesFailed++
            }
        }
    } else {
        Write-Host "   ⚠️  Not found: $fileName" -ForegroundColor Yellow
        $filesFailed++
    }
}

# Create __init__.py files
if (-not $DryRun) {
    Write-Host ""
    Write-Host "📝 Creating __init__.py files..." -ForegroundColor Yellow
    
    $initFiles = @(
        "$OutputDir\__init__.py",
        "$OutputDir\models\__init__.py",
        "$OutputDir\calculators\__init__.py",
        "$OutputDir\validators\__init__.py",
        "$OutputDir\tests\__init__.py"
    )
    
    foreach ($initFile in $initFiles) {
        if (-not (Test-Path $initFile)) {
            '"""ARC (Air Risk Class) calculation module for SORA."""' | Set-Content $initFile -Encoding UTF8
            Write-Host "   ✅ Created: $initFile" -ForegroundColor Green
        }
    }
}

# Summary
Write-Host ""
Write-Host "━" * 80 -ForegroundColor Gray
Write-Host "📊 Extraction Summary:" -ForegroundColor Cyan
Write-Host "   ✅ Files created: $filesCreated" -ForegroundColor Green
if ($filesFailed -gt 0) {
    Write-Host "   ❌ Files failed: $filesFailed" -ForegroundColor Red
}
Write-Host "   📁 Output directory: $OutputDir" -ForegroundColor Yellow
Write-Host "━" * 80 -ForegroundColor Gray

if ($DryRun) {
    Write-Host ""
    Write-Host "ℹ️  This was a DRY RUN. Remove -DryRun to actually create files." -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "🎉 Extraction complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Next Steps:" -ForegroundColor Yellow
    Write-Host "   1. Review extracted files in: $OutputDir"
    Write-Host "   2. Install dependencies:"
    Write-Host "      cd Backend_Python"
    Write-Host "      pip install pydantic pyyaml fastapi hypothesis pytest"
    Write-Host "   3. Run tests:"
    Write-Host "      pytest arc/tests/test_arc_calculator.py -v"
    Write-Host "   4. Add to FastAPI:"
    Write-Host "      # In main.py:"
    Write-Host "      from arc.arc_api import router as arc_router"
    Write-Host "      app.include_router(arc_router, prefix='/sora', tags=['ARC'])"
    Write-Host ""
    Write-Host "🧪 Quick Test:" -ForegroundColor Cyan
    Write-Host "   python -m pytest arc/tests/test_arc_calculator.py -v --tb=short"
}
