# SKYWORKS FULL RESET & TEST SCRIPT
# Cleans, rebuilds, and launches everything fresh

param(
    [switch]$SkipBuild,
    [switch]$SkipTests,
    [switch]$OpenSailTests
)

$ErrorActionPreference = "Stop"

# Colors
function Write-Header { param($text) Write-Host "`n$text" -ForegroundColor Cyan }
function Write-Success { param($text) Write-Host "✅ $text" -ForegroundColor Green }
function Write-Info { param($text) Write-Host "ℹ️  $text" -ForegroundColor Yellow }
function Write-Step { param($text) Write-Host "🔄 $text" -ForegroundColor White }

Clear-Host

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🚀 SKYWORKS AI SUITE - FULL RESET & TEST               ║
║                                                           ║
║   Backend:  http://localhost:5210                        ║
║   Version:  5.0                                          ║
║   Date:     $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')                        ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

# Step 1: Kill existing processes
Write-Header "STEP 1: Cleanup"
Write-Step "Stopping any running backend processes..."
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Write-Success "All processes stopped"

# Step 2: Clean & Build Backend
if (-not $SkipBuild) {
    Write-Header "STEP 2: Backend Build"
    
    Push-Location "$PSScriptRoot\Backend"
    
    Write-Step "Cleaning solution..."
    dotnet clean Skyworks.sln --verbosity quiet
    Write-Success "Clean complete"
    
    Write-Step "Rebuilding solution..."
    $buildOutput = dotnet build Skyworks.sln --verbosity minimal 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build succeeded"
        
        # Show warnings if any
        $warnings = $buildOutput | Select-String "warning"
        if ($warnings) {
            Write-Info "Build warnings: $($warnings.Count)"
        }
    } else {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        Write-Host $buildOutput
        Pop-Location
        exit 1
    }
    
    Pop-Location
} else {
    Write-Info "Skipping build (--SkipBuild flag)"
}

# Step 3: Run Tests (Optional)
if (-not $SkipTests) {
    Write-Header "STEP 3: Running Tests"
    
    # .NET Tests
    Push-Location "$PSScriptRoot\Backend"
    
    Write-Step "Executing .NET test suite..."
    $testOutput = dotnet test Skyworks.sln --verbosity minimal --no-build 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        # Parse test results
        $passed = ($testOutput | Select-String "Passed!").ToString()
        Write-Success ".NET tests passed: $passed"
    } else {
        Write-Host "⚠️  Some .NET tests failed" -ForegroundColor Yellow
        $failed = ($testOutput | Select-String "Failed!").ToString()
        Write-Host $failed -ForegroundColor Red
    }
    
    Pop-Location
    
    # Python SORA Comprehensive Tests
    Write-Step "Running Python SORA Calculator Tests..."
    Push-Location "$PSScriptRoot\Backend_Python"
    
    # Start Python service temporarily for tests
    $pythonExe = "$PSScriptRoot\Backend_Python\venv\Scripts\python.exe"
    
    if (Test-Path $pythonExe) {
        # Start service in background
        $serviceJob = Start-Job -ScriptBlock {
            param($pythonPath, $workDir)
            Set-Location $workDir
            & $pythonPath -m uvicorn --app-dir $workDir main:app --port 8001
        } -ArgumentList $pythonExe, "$PSScriptRoot\Backend_Python"
        
        Write-Step "Waiting for Python service to start..."
        Start-Sleep -Seconds 5
        
        # Run comprehensive tests
        $pythonTestOutput = & $pythonExe "$PSScriptRoot\Backend_Python\test_comprehensive_report.py" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            # Parse Python test results
            $pythonPassed = ($pythonTestOutput | Select-String "PASSED:" | Out-String).Trim()
            Write-Success "Python SORA tests: $pythonPassed"
            
            # Generate HTML report
            Write-Step "Generating HTML accuracy report..."
            & $pythonExe "$PSScriptRoot\Backend_Python\generate_html_report.py" | Out-Null
            if (Test-Path "$PSScriptRoot\Backend_Python\COMPREHENSIVE_ACCURACY_REPORT.html") {
                Write-Success "HTML report generated: Backend_Python\COMPREHENSIVE_ACCURACY_REPORT.html"
            }
        } else {
            Write-Host "⚠️  Some Python SORA tests failed" -ForegroundColor Yellow
            $pythonTestOutput | Select-String "❌|⚠️" | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        }
        
        # Stop service
        Stop-Job $serviceJob -ErrorAction SilentlyContinue
        Remove-Job $serviceJob -ErrorAction SilentlyContinue
    } else {
        Write-Info "Python venv not found - skipping Python SORA tests"
    }
    
    Pop-Location
} else {
    Write-Info "Skipping tests (--SkipTests flag)"
}

# Step 4: Start Backend
Write-Header "STEP 4: Starting Backend"

Write-Step "Launching backend server on http://localhost:5210..."

$backendPath = "$PSScriptRoot\Backend\src\Skyworks.Api"
Start-Process pwsh -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd '$backendPath' ; dotnet run --urls 'http://localhost:5210'"
)

Write-Step "Waiting for backend to initialize..."
Start-Sleep -Seconds 6

# Verify backend is running
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5210/index.html" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Success "Backend is running and responding"
} catch {
    Write-Host "⚠️  Backend may not be fully started yet" -ForegroundColor Yellow
    Write-Info "Check the backend terminal window for status"
}

# Step 5: Open Frontend
Write-Header "STEP 5: Opening Frontend"

Write-Step "Opening main application..."
Start-Process "http://localhost:5210/index.html"
Start-Sleep -Seconds 2

Write-Step "Opening mission planner..."
Start-Process "http://localhost:5210/Pages/mission.html"
Start-Sleep -Seconds 1

# Step 6: Open SAIL Tests (Optional)
if ($OpenSailTests) {
    Write-Header "STEP 6: Opening SAIL Test Suite"
    
    Write-Step "Opening SAIL calculator tests..."
    Start-Process "http://localhost:5210/test-sail-calculator.html"
    Write-Success "SAIL tests opened - Click 'Run All Tests' button"
}

# Summary
Write-Header "✅ RESET COMPLETE!"

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                    🎯 QUICK LINKS                        ║
╠═══════════════════════════════════════════════════════════╣
║                                                           ║
║  📱 Main App:                                            ║
║     http://localhost:5210/index.html                     ║
║                                                           ║
║  🗺️  Mission Planner:                                    ║
║     http://localhost:5210/Pages/mission.html             ║
║                                                           ║
║  🧪 SAIL Calculator Test:                                ║
║     http://localhost:5210/test-sail-calculator.html      ║
║                                                           ║
║  📊 API Docs:                                            ║
║     http://localhost:5210/swagger                        ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

Write-Host @"
🧪 NEW FEATURES TO TEST:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1️⃣  SAIL Calculator (100% EASA/JARUS Compliant)
   ✅ SORA 2.0 & 2.5 support
   ✅ 12 AEC categories
   ✅ GRC 5 + ARC-c special case
   ✅ 250g special rule
   ✅ 14 comprehensive tests
   
   📍 Test it: http://localhost:5210/test-sail-calculator.html

2️⃣  OSO Framework (SORA 2.5 Annex E)
   ✅ 17 OSOs from JARUS Table 14
   ✅ Robustness levels (Low/Medium/High)
   ✅ SAIL-based filtering
   ✅ Compliance tracking
   
   📍 Test it: Mission Planner → OSO Section

3️⃣  Drone Model Integration
   ✅ Airspace class selector (A-G)
   ✅ Mode-S veil checkbox
   ✅ TMZ checkbox
   ✅ Manual dimension/speed override
   
   📍 Test it: Mission Planner → Drone Details

4️⃣  Full 12-Category ARC
   ✅ All AEC 1-12 implemented
   ✅ FL600 detection
   ✅ Atypical airspace
   ✅ Airport environments
   ✅ Mode-S/TMZ zones
   
   📍 Test it: Mission Planner → Risk Assessment

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

"@ -ForegroundColor White

Write-Host "💡 TIP: Use " -NoNewline -ForegroundColor Yellow
Write-Host ".\RESTART_WITH_CACHE_CLEAR.bat" -NoNewline -ForegroundColor Cyan
Write-Host " to force browser cache refresh" -ForegroundColor Yellow

Write-Host ""
Write-Host "Press Ctrl+C to stop the backend server" -ForegroundColor Gray
Write-Host ""
