#!/usr/bin/env pwsh
# Extract all 13 SAIL implementation files from 3 Sonnet 4 responses

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SAIL IMPLEMENTATION - FILE EXTRACTION" -ForegroundColor Cyan
Write-Host "  Extracting 13 files from 3 responses" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Define source files and target files
$extractions = @(
    @{
        Source = "SAIL_SONNET4_RESPONSE_20251030_164407.txt"
        Files = @(
            "sail/models/sail_models.py",
            "sail/calculators/sail_calculator.py",
            "sail/calculators/oso_mapper.py",
            "sail/validators/sail_validator.py",
            "sail/data/sail_tables_20.py",
            "sail/data/sail_tables_25.py"
        )
    },
    @{
        Source = "SAIL_CONTINUATION_SONNET4_RESPONSE_20251030_165738.txt"
        Files = @(
            "sail/data/oso_requirements.py",
            "sail/api/sail_api.py",
            "sail/tests/test_sail_calculator_20.py",
            "sail/tests/test_sail_calculator_25.py"
        )
    },
    @{
        Source = "SAIL_FINAL3_SONNET4_RESPONSE_20251030_170032.txt"
        Files = @(
            "sail/tests/test_oso_mapping.py",
            "sail/tests/test_sail_validation.py",
            "sail/__init__.py"
        )
    }
)

$totalExtracted = 0

foreach ($extraction in $extractions) {
    $sourceFile = $extraction.Source
    
    if (-not (Test-Path $sourceFile)) {
        Write-Host "❌ Source file not found: $sourceFile" -ForegroundColor Red
        continue
    }
    
    Write-Host "📄 Reading: $sourceFile" -ForegroundColor Cyan
    $content = Get-Content $sourceFile -Raw
    
    foreach ($targetFile in $extraction.Files) {
        Write-Host "  🔍 Extracting: $targetFile" -ForegroundColor Yellow
        
        # Find file markers
        $startMarker = "=== FILE: $targetFile ==="
        $startIndex = $content.IndexOf($startMarker)
        
        if ($startIndex -eq -1) {
            Write-Host "    ⚠️  File marker not found!" -ForegroundColor Red
            continue
        }
        
        # Find content start (after marker and newlines)
        $contentStart = $startIndex + $startMarker.Length
        while ($contentStart -lt $content.Length -and ($content[$contentStart] -eq "`n" -or $content[$contentStart] -eq "`r")) {
            $contentStart++
        }
        
        # Find next file marker or end of content
        $nextMarkerIndex = $content.IndexOf("=== FILE:", $contentStart)
        if ($nextMarkerIndex -eq -1) {
            $nextMarkerIndex = $content.Length
        }
        
        # Extract file content
        $fileContent = $content.Substring($contentStart, $nextMarkerIndex - $contentStart).TrimEnd()
        
        # Save file
        $fullPath = Join-Path (Get-Location) $targetFile
        $directory = Split-Path $fullPath -Parent
        
        if (-not (Test-Path $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        
        $fileContent | Out-File -FilePath $fullPath -Encoding utf8 -NoNewline
        
        $lines = ($fileContent -split "`n").Count
        Write-Host "    ✅ Saved: $targetFile ($lines lines)" -ForegroundColor Green
        $totalExtracted++
    }
    
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  EXTRACTION COMPLETE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Total files extracted: $totalExtracted / 13" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Directory structure:" -ForegroundColor Cyan
Write-Host "  sail/" -ForegroundColor White
Write-Host "    ├── __init__.py" -ForegroundColor White
Write-Host "    ├── models/" -ForegroundColor White
Write-Host "    │   └── sail_models.py" -ForegroundColor White
Write-Host "    ├── calculators/" -ForegroundColor White
Write-Host "    │   ├── sail_calculator.py" -ForegroundColor White
Write-Host "    │   └── oso_mapper.py" -ForegroundColor White
Write-Host "    ├── validators/" -ForegroundColor White
Write-Host "    │   └── sail_validator.py" -ForegroundColor White
Write-Host "    ├── data/" -ForegroundColor White
Write-Host "    │   ├── sail_tables_20.py" -ForegroundColor White
Write-Host "    │   ├── sail_tables_25.py" -ForegroundColor White
Write-Host "    │   └── oso_requirements.py" -ForegroundColor White
Write-Host "    ├── api/" -ForegroundColor White
Write-Host "    │   └── sail_api.py" -ForegroundColor White
Write-Host "    └── tests/" -ForegroundColor White
Write-Host "        ├── test_sail_calculator_20.py" -ForegroundColor White
Write-Host "        ├── test_sail_calculator_25.py" -ForegroundColor White
Write-Host "        ├── test_oso_mapping.py" -ForegroundColor White
Write-Host "        └── test_sail_validation.py" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Next: pytest sail/tests/ -v" -ForegroundColor Yellow
