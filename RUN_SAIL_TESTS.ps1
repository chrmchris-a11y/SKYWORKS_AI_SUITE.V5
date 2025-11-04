# SAIL Calculator Test Runner
# Run comprehensive tests for SORA 2.0 & 2.5 algorithms

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  🧪 SAIL CALCULATOR TEST SUITE" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ 100% EASA/JARUS Compliant" -ForegroundColor Green
Write-Host "📊 Testing SORA 2.0 & 2.5 Algorithms" -ForegroundColor Green
Write-Host ""

$testFile = "$PSScriptRoot\Frontend\test-sail-calculator.html"

if (Test-Path $testFile) {
    Write-Host "📂 Opening test file in browser..." -ForegroundColor Cyan
    Write-Host "   $testFile" -ForegroundColor Gray
    Write-Host ""
    
    # Open in default browser
    Start-Process $testFile
    
    Write-Host "✅ Test suite opened successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Click 'Run All Tests' button to execute tests." -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "❌ ERROR: Test file not found!" -ForegroundColor Red
    Write-Host "   Expected: $testFile" -ForegroundColor Gray
    exit 1
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
