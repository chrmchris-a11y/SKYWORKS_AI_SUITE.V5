@echo off
setlocal EnableDelayedExpansion

rem ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
rem Skyworks V5 — Training Center Runner (with logging & retention)
rem ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

set SCRIPT_DIR=%~dp0
set LOG_DIR=%SCRIPT_DIR%logs
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

rem Label for session (morning/afternoon/evening/now)
set SESSION_LABEL=%~1
if "%SESSION_LABEL%"=="" set SESSION_LABEL=now

for /f %%i in ('powershell -NoProfile -Command "(Get-Date).ToString(\"yyyyMMdd_HHmmss\")"') do set TS=%%i
set LOG_FILE=%LOG_DIR%\training_%%SESSION_LABEL%%_!TS!.log

rem Choose python launcher (prefer py -3, fallback to python)
where py >nul 2>&1
if %ERRORLEVEL% EQU 0 (
  set "PY=py -3"
) else (
  set "PY=python"
)

echo [Runner] Starting training (session=%SESSION_LABEL%) at !TS!
echo [Runner] Using: %PY%
echo [Runner] Logs: %LOG_FILE%

rem Run the trainer with absolute defaults (agent_trainer resolves paths itself)
%PY% "%SCRIPT_DIR%agent_trainer.py" 1>>"%LOG_FILE%" 2>&1
set RUN_ERR=%ERRORLEVEL%

if %RUN_ERR% NEQ 0 (
  echo [Runner] Training failed with exit code %RUN_ERR% >>"%LOG_FILE%"
  echo [Runner] Training failed (exit %RUN_ERR%). See log: %LOG_FILE%
  exit /b %RUN_ERR%
) else (
  echo [Runner] Training completed successfully >>"%LOG_FILE%"
  echo [Runner] Training completed successfully.
)

rem Retain last 14 days of logs
forfiles /p "%LOG_DIR%" /m *.log /d -14 /c "cmd /c del @file" >nul 2>&1

endlocal
exit /b 0
