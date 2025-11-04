@echo off
REM =======================================================
REM SKYWORKS AI SUITE - ONE-CLICK LAUNCHER
REM Starts .NET backend + Python calculation service + SORA Calculator
REM =======================================================

echo.
echo =====================================================
echo  SKYWORKS AI SUITE - HYBRID ARCHITECTURE
echo  .NET Backend + Python Calculations
echo =====================================================
echo.

cd /d "%~dp0"

REM ===== 1. CHECK .NET BACKEND =====
echo [1/3] Checking .NET Backend (port 5210)...
curl -s http://localhost:5210/health >nul 2>&1
if %errorlevel% neq 0 (
    echo Starting .NET Backend...
    cd Backend
    start "Skyworks .NET Backend" cmd /c "dotnet run --project src/Skyworks.Api"
    cd ..
    echo Waiting for .NET backend to start (10 seconds)...
    timeout /t 10 /nobreak >nul
    echo [SUCCESS] .NET Backend started on port 5210
) else (
    echo [OK] .NET Backend already running
)

REM ===== 2. CHECK PYTHON SERVICE =====
echo.
echo [2/3] Checking Python Calculation Service (port 8001)...
curl -s http://localhost:8001/health >nul 2>&1
if %errorlevel% neq 0 (
    echo Starting Python Calculation Service...
    cd Backend_Python
    
    REM Check if venv exists
    if not exist "venv" (
        echo Creating Python virtual environment...
        python -m venv venv
        call venv\Scripts\activate.bat
        python -m pip install --upgrade pip >nul 2>&1
        pip install -r requirements.txt
    ) else (
        call venv\Scripts\activate.bat
    )
    
    start "Skyworks Python Service" cmd /c "python main.py"
    cd ..
    echo Waiting for Python service to start (8 seconds)...
    timeout /t 8 /nobreak >nul
    echo [SUCCESS] Python service started on port 8001
) else (
    echo [OK] Python service already running
)

REM ===== 3. OPEN SORA CALCULATOR =====
echo.
echo [3/3] Opening SORA Calculator...
timeout /t 2 /nobreak >nul
start http://localhost:5210/app/Pages/mission.html

echo.
echo =====================================================
echo  SKYWORKS IS READY!
echo =====================================================
echo.
echo  [Backend Services]
echo  - .NET Backend:     http://localhost:5210
echo  - Python Service:   http://localhost:8001
echo  - Python API Docs:  http://localhost:8001/api/docs
echo.
echo  [Applications]
echo  - SORA Calculator:  http://localhost:5210/app/Pages/mission.html
echo  - Dashboard:        http://localhost:5210/app/
echo.
echo  [Architecture]
echo  - Frontend calls .NET API (port 5210)
echo  - .NET calls Python for calculations (port 8001)
echo  - 100%% EASA/JARUS accurate calculations!
echo.
echo  To stop, close both "Skyworks .NET Backend" 
echo  and "Skyworks Python Service" windows
echo.
echo =====================================================
timeout /t 10
