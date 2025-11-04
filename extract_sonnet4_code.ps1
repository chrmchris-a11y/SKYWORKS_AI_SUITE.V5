#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Extract Python/YAML files from Sonnet 4 implementation markdown
.DESCRIPTION
    Parses the Sonnet 4 output markdown and extracts all code blocks
    into separate files with proper directory structure
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$MarkdownFile,
    
    [string]$OutputDir = "Backend_Python/arc",
    
    [switch]$DryRun
)

if (-not (Test-Path $MarkdownFile)) {
    Write-Error "Markdown file not found: $MarkdownFile"
    exit 1
}

Write-Host "📖 Reading implementation: $MarkdownFile" -ForegroundColor Cyan

$content = Get-Content $MarkdownFile -Raw

# Regex to match code blocks with filenames
$pattern = '(?s)```(?:python|yaml|yml)\s+#\s*(?:File:|Filename:)?\s*([^\r\n]+)\s*\r?\n(.*?)```'

$codeMatches = [regex]::Matches($content, $pattern)

Write-Host "🔍 Found $($codeMatches.Count) code blocks" -ForegroundColor Yellow

if ($codeMatches.Count -eq 0) {
    Write-Warning "No code blocks found with filenames!"
    Write-Host "Looking for alternative patterns..." -ForegroundColor Yellow
    
    # Try alternative pattern: filename in comment before code block
    $pattern2 = '(?s)#\s*([a-z_]+\.(?:py|yaml|yml))\s*\r?\n```(?:python|yaml|yml)\s*\r?\n(.*?)```'
    $codeMatches = [regex]::Matches($content, $pattern2)
    
    Write-Host "🔍 Alternative pattern found: $($codeMatches.Count) blocks" -ForegroundColor Yellow
}

if ($codeMatches.Count -eq 0) {
    Write-Error "Still no code blocks found. Please check the markdown format."
    exit 1
}

# Create directory structure
$dirs = @(
    $OutputDir,
    "$OutputDir/models",
    "$OutputDir/calculators", 
    "$OutputDir/validators",
    "$OutputDir/rules",
    "$OutputDir/tests"
)

foreach ($dir in $dirs) {
    if (-not (Test-Path $dir)) {
        if ($DryRun) {
            Write-Host "[DRY RUN] Would create: $dir" -ForegroundColor Gray
        } else {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "📁 Created: $dir" -ForegroundColor Green
        }
    }
}

# Extract and save files
$filesCreated = 0

foreach ($match in $codeMatches) {
    $filename = $match.Groups[1].Value.Trim()
    $code = $match.Groups[2].Value.Trim()
    
    # Determine target directory
    $targetPath = "$OutputDir/$filename"
    
    if ($filename -match "^test_") {
        $targetPath = "$OutputDir/tests/$filename"
    }
    elseif ($filename -match "\.yaml$|\.yml$") {
        $targetPath = "$OutputDir/rules/$filename"
    }
    elseif ($filename -match "_models\.py$") {
        $targetPath = "$OutputDir/models/$filename"
    }
    elseif ($filename -match "_calculator\.py$") {
        $targetPath = "$OutputDir/calculators/$filename"
    }
    elseif ($filename -match "_validator\.py$") {
        $targetPath = "$OutputDir/validators/$filename"
    }
    
    if ($DryRun) {
        Write-Host "[DRY RUN] Would create: $targetPath ($($code.Length) chars)" -ForegroundColor Gray
    } else {
        $code | Set-Content $targetPath -Encoding UTF8
        $filesCreated++
        
        $lineCount = ($code -split "`n").Count
        Write-Host "✅ Created: $targetPath ($lineCount lines)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "🎉 Extraction complete!" -ForegroundColor Green
Write-Host "   Files created: $filesCreated" -ForegroundColor Yellow
Write-Host "   Output directory: $OutputDir" -ForegroundColor Yellow

if ($DryRun) {
    Write-Host ""
    Write-Host "ℹ️  This was a DRY RUN. Remove -DryRun to actually create files." -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "📋 Next Steps:" -ForegroundColor Yellow
    Write-Host "   1. Review extracted files in: $OutputDir"
    Write-Host "   2. Install dependencies: pip install pydantic pyyaml fastapi hypothesis pytest"
    Write-Host "   3. Run tests: pytest $OutputDir/tests/ -v"
    Write-Host "   4. Add to main FastAPI app"
}

# Create __init__.py files
if (-not $DryRun) {
    $initFiles = @(
        "$OutputDir/__init__.py",
        "$OutputDir/models/__init__.py",
        "$OutputDir/calculators/__init__.py",
        "$OutputDir/validators/__init__.py",
        "$OutputDir/tests/__init__.py"
    )
    
    foreach ($initFile in $initFiles) {
        if (-not (Test-Path $initFile)) {
            "" | Set-Content $initFile
            Write-Host "✅ Created: $initFile" -ForegroundColor Green
        }
    }
}
