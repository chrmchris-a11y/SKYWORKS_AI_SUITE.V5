@echo off
REM ============================================
REM   SKYWORKS MISSION PLANNER - STOP SCRIPT
REM   Terminates all running servers
REM ============================================

title SKYWORKS Mission Planner - Stopping...
color 0C

echo.
echo ========================================
echo   STOPPING SKYWORKS MISSION PLANNER
echo ========================================
echo.

echo [1/3] Stopping Backend API (port 5210)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5210') do (
    taskkill /F /PID %%a >nul 2>&1
)

echo [2/3] Stopping Frontend Server (port 8080)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :8080') do (
    taskkill /F /PID %%a >nul 2>&1
)

echo [3/3] Cleaning up processes...
taskkill /F /IM "dotnet.exe" /FI "WINDOWTITLE eq Skyworks*" >nul 2>&1
taskkill /F /IM "python.exe" /FI "WINDOWTITLE eq Skyworks*" >nul 2>&1

echo.
echo ========================================
echo   ALL SERVERS STOPPED SUCCESSFULLY
echo ========================================
echo.

timeout /t 3
