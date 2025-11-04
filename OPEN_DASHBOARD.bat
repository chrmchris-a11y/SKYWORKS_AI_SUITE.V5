@echo off
REM =======================================================
REM SKYWORKS AI SUITE - DASHBOARD LAUNCHER
REM Opens the main dashboard with navigation
REM =======================================================

echo.
echo ===================================
echo  SKYWORKS DASHBOARD
echo ===================================
echo.
echo Opening Dashboard in browser...
echo.

start http://localhost:5210/app/

echo.
echo Dashboard opened!
echo Close this window to continue.
echo.
timeout /t 3
