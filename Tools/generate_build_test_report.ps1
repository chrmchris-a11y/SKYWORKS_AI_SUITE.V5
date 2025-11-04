param(
    [string]$SolutionPath = "C:\\Users\\chrmc\\Desktop\\SKYWORKS_AI_SUITE.V5\\Backend\\Skyworks.sln",
    [string]$WorkingDir = "C:\\Users\\chrmc\\Desktop\\SKYWORKS_AI_SUITE.V5\\Backend",
    [string]$OutDir = "C:\\Users\\chrmc\\Desktop\\SKYWORKS_AI_SUITE.V5\\Backend"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-Section($title) {
    Write-Host "`n===== $title =====" -ForegroundColor Cyan
}

# Prep paths
$buildOut = Join-Path $OutDir 'build_output.txt'
$testOut  = Join-Path $OutDir 'test_output.txt'
$trxPath  = Join-Path $OutDir 'TestResults.trx'
$reportTxt = Join-Path $OutDir 'build_test_report.txt'
$reportJson = Join-Path $OutDir 'build_test_report.json'

New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

Push-Location $WorkingDir
try {
    # Clean previous artifacts
    Remove-Item $buildOut,$testOut,$trxPath,$reportTxt,$reportJson -ErrorAction SilentlyContinue

    Write-Section 'Build'
    $buildStart = Get-Date
    dotnet build $SolutionPath --nologo -v minimal *> $buildOut
    $buildExit = $LASTEXITCODE
    $buildEnd = Get-Date
    $buildDuration = [math]::Round(($buildEnd - $buildStart).TotalSeconds,2)
    $buildStatus = if ($buildExit -eq 0) { 'PASS' } else { 'FAIL' }
    Write-Host "Build: $buildStatus in ${buildDuration}s (exit=$buildExit)" -ForegroundColor ($buildExit -eq 0 ? 'Green' : 'Red')

    Write-Section 'Tests'
    $testStart = Get-Date
    dotnet test $SolutionPath --nologo --verbosity minimal --logger "trx;LogFileName=$(Split-Path -Leaf $trxPath)" *> $testOut
    $testExit = $LASTEXITCODE
    $testEnd = Get-Date
    $testDuration = [math]::Round(($testEnd - $testStart).TotalSeconds,2)

    # Parse TRX if present
    $total = 0; $passed = 0; $failed = 0; $skipped = 0
    $failedTests = @()
    if (Test-Path $trxPath) {
        [xml]$trx = Get-Content $trxPath
        $counters = $trx.TestRun.ResultSummary.Counters
        if ($counters) {
            $total = [int]$counters.total
            $passed = [int]$counters.passed
            $failed = [int]$counters.failed
            $skipped = [int]$counters.notExecuted
        }
        $failedNodes = $trx.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq 'Failed' }
        foreach ($n in $failedNodes) {
            $name = $n.testName
            $msg = ($n.Output.ErrorInfo.Message | Out-String).Trim()
            if (-not $msg) { $msg = ($n.Output.StdOut | Out-String).Trim() }
            $failedTests += [pscustomobject]@{ name = $name; message = $msg }
        }
    }
    $testsStatus = if ($testExit -eq 0 -and $failed -eq 0) { 'PASS' } else { 'FAIL' }
    Write-Host "Tests: $testsStatus in ${testDuration}s (exit=$testExit) | Total=$total Passed=$passed Failed=$failed Skipped=$skipped" -ForegroundColor ($testsStatus -eq 'PASS' ? 'Green' : 'Red')

    # Compose report objects
    $report = [pscustomobject]@{
        timestamp = (Get-Date).ToString('s')
        workingDir = $WorkingDir
        solution = $SolutionPath
        build = [pscustomobject]@{
            status = $buildStatus
            exitCode = $buildExit
            durationSec = $buildDuration
            log = (Get-Content $buildOut -Raw)
        }
        tests = [pscustomobject]@{
            status = $testsStatus
            exitCode = $testExit
            durationSec = $testDuration
            total = $total
            passed = $passed
            failed = $failed
            skipped = $skipped
            failedTests = $failedTests
            log = (Get-Content $testOut -Raw)
            trxPath = $trxPath
        }
        qualityGates = [pscustomobject]@{
            build = $buildStatus
            tests = $testsStatus
        }
    }

    # Write TXT report (compact)
    $lines = @()
    $lines += "Skyworks Build/Test Report - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $lines += "Workspace: $WorkingDir"
    $lines += "Solution: $SolutionPath"
    $lines += ""
    $lines += "[Build] Status=$($report.build.status) Exit=$($report.build.exitCode) Duration=${buildDuration}s"
    $lines += "[Tests] Status=$($report.tests.status) Exit=$($report.tests.exitCode) Duration=${testDuration}s Total=$total Passed=$passed Failed=$failed Skipped=$skipped"
    if ($failed -gt 0) {
        $lines += "Failed tests:"
        foreach ($f in $failedTests) {
            $lines += " - $($f.name): $($f.message)"
        }
    }
    $lines += ""
    $lines += "Build log: $buildOut"
    $lines += "Test log:  $testOut"
    $lines += "TRX:       $trxPath"
    $lines -join "`n" | Set-Content -Path $reportTxt -Encoding UTF8

    # Write JSON report (detailed)
    $report | ConvertTo-Json -Depth 6 | Set-Content -Path $reportJson -Encoding UTF8

    Write-Section 'Summary'
    Write-Host (Get-Content $reportTxt -Raw)

    Exit 0
}
catch {
    Write-Error $_
    Exit 1
}
finally {
    Pop-Location
}
