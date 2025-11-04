@echo off
REM SKYWORKS FULL RESET & TEST - Quick Launcher
REM Double-click this file to reset and test everything

echo.
echo ╔═══════════════════════════════════════════════════════════╗
echo ║                                                           ║
echo ║   🚀 SKYWORKS AI SUITE - FULL RESET & TEST               ║
echo ║                                                           ║
echo ╚═══════════════════════════════════════════════════════════╝
echo.

REM Check if PowerShell is available
where pwsh >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Using PowerShell Core...
    pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0RESET_AND_TEST.ps1" %*
) else (
    echo Using Windows PowerShell...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0RESET_AND_TEST.ps1" %*
)

pause
