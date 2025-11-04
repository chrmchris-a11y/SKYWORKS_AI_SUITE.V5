@echo off
REM ============================================
REM   SKYWORKS MISSION PLANNER - LOCAL LAUNCHER
REM   Auto-starts Backend + Frontend + Browser
REM ============================================

title SKYWORKS Mission Planner - Starting...
color 0A

echo.
echo ========================================
echo   SKYWORKS MISSION PLANNER v5.0
echo   Starting Local Development Server
echo ========================================
echo.

REM Check if backend is already running
netstat -ano | findstr :5210 >nul
if %errorlevel% equ 0 (
    echo [WARNING] Backend already running on port 5210
    echo [INFO] Skipping backend start...
) else (
    echo [1/4] Starting Backend API on http://localhost:5210...
    start "Skyworks Backend API" /MIN cmd /c "cd /d "%~dp0Backend\src\Skyworks.Api" && dotnet run --no-build 2>&1"
    timeout /t 3 /nobreak >nul
)

REM Check if frontend is already running
netstat -ano | findstr :8080 >nul
if %errorlevel% equ 0 (
    echo [WARNING] Frontend already running on port 8080
    echo [INFO] Skipping frontend start...
) else (
    echo [2/4] Starting Frontend on http://localhost:8080...
    start "Skyworks Frontend" /MIN cmd /c "cd /d "%~dp0Frontend" && python -m http.server 8080 2>&1"
    timeout /t 2 /nobreak >nul
)

echo [3/4] Waiting for services to initialize...
timeout /t 5 /nobreak >nul

echo [4/4] Opening Mission Planner in browser...
start "" "http://localhost:8080/Pages/mission.html"

echo.
echo ========================================
echo   SKYWORKS MISSION PLANNER - RUNNING
echo ========================================
echo.
echo   Backend API:  http://localhost:5210
echo   Frontend UI:  http://localhost:8080
echo   Mission Page: http://localhost:8080/Pages/mission.html
echo.
echo   Press CTRL+C to stop all servers
echo   Or close this window to continue running
echo ========================================
echo.

REM Keep window open
pause
