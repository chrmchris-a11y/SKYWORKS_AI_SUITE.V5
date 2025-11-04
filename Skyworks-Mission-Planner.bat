@echo off
REM ===================================================================
REM   SKYWORKS MISSION PLANNER - DESKTOP LAUNCHER
REM   Anoigei ton local server kai to Mission Planner sto browser
REM ===================================================================

title Skyworks Mission Planner Server

echo.
echo ============================================================
echo      SKYWORKS MISSION PLANNER - DESKTOP APP
echo ============================================================
echo.

REM Check for PowerShell
where pwsh >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    where powershell >nul 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] PowerShell not found
        echo Please ensure PowerShell is installed
        pause
        exit /b 1
    )
    set PWSH_CMD=powershell
) else (
    set PWSH_CMD=pwsh
)

REM Run the PowerShell launcher
%PWSH_CMD% -NoProfile -ExecutionPolicy Bypass -File "%~dp0Skyworks-Mission-Planner.ps1"

pause
