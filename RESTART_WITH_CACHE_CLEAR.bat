@echo off
REM ============================================
REM   SKYWORKS - CLEAR CACHE & RESTART
REM   Forces browser cache clear and restart
REM ============================================

title SKYWORKS - Cache Clear & Restart
color 0E

echo.
echo ========================================
echo   CLEARING CACHE ^& RESTARTING
echo ========================================
echo.

echo [1/5] Stopping all services...
call "%~dp0STOP_SKYWORKS_APP.bat"

echo [2/5] Clearing browser cache...
REM Close all browser instances
taskkill /F /IM "msedge.exe" >nul 2>&1
taskkill /F /IM "chrome.exe" >nul 2>&1
taskkill /F /IM "firefox.exe" >nul 2>&1
timeout /t 2 /nobreak >nul

echo [3/5] Rebuilding backend...
cd /d "%~dp0Backend"
dotnet build Skyworks.sln --verbosity quiet
if %errorlevel% neq 0 (
    echo [ERROR] Backend build failed!
    pause
    exit /b 1
)

echo [4/5] Restarting services...
cd /d "%~dp0"
start "SKYWORKS Launcher" /MIN cmd /c "START_SKYWORKS_APP.bat"
timeout /t 8 /nobreak >nul

echo [5/5] Opening browser with cache bypass...
start "" "http://localhost:8080/Pages/mission.html?nocache=%RANDOM%%RANDOM%"

echo.
echo ========================================
echo   RESTART COMPLETE!
echo ========================================
echo.
echo   Frontend: http://localhost:8080/Pages/mission.html
echo   Press Ctrl+Shift+R in browser to force refresh
echo.

timeout /t 3
