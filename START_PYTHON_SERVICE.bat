@echo off
REM ====================================================================
REM SKYWORKS AI SUITE - Python Calculation Service Setup & Start
REM ====================================================================

cd /d "%~dp0Backend_Python"

echo.
echo ================================================================
echo  SKYWORKS AI SUITE - Python Calculation Service
echo ================================================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Python is not installed or not in PATH!
    echo Please install Python 3.11 or higher from python.org
    echo.
    pause
    exit /b 1
)

echo [1/4] Python version check...
python --version

REM Check if virtual environment exists
if not exist "venv" (
    echo.
    echo [2/4] Creating Python virtual environment...
    python -m venv venv
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to create virtual environment!
        pause
        exit /b 1
    )
    echo [SUCCESS] Virtual environment created!
) else (
    echo [2/4] Virtual environment already exists.
)

REM Activate virtual environment
echo.
echo [3/4] Activating virtual environment...
call venv\Scripts\activate.bat

REM Install/Update dependencies
echo.
echo [4/4] Installing Python packages...
echo (This may take a few minutes on first run)
echo.
python -m pip install --upgrade pip
pip install -r requirements.txt

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Failed to install packages!
    pause
    exit /b 1
)

echo.
echo ================================================================
echo  STARTING SKYWORKS PYTHON CALCULATION SERVICE
echo ================================================================
echo.
echo  Port: 8001
echo  API Docs: http://localhost:8001/api/docs
echo  Health Check: http://localhost:8001/health
echo.
echo  Press Ctrl+C to stop the server
echo ================================================================
echo.

REM Start FastAPI server
python main.py

pause
