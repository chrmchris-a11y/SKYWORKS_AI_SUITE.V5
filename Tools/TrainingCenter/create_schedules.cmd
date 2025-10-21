@echo off
REM Phase1 Step5 — Skyworks V5
REM Creates 3 daily Windows Task Scheduler jobs for context pack generation

echo Creating scheduled tasks for Context Pack generation...

set REPO_ROOT=%~dp0..\..
set SCRIPT_PATH=%REPO_ROOT%\Tools\TrainingCenter\make_context_pack.py

REM Task 1: 08:00 - GRC + ARC
schtasks /create /tn "Skyworks_ContextPack_Morning" /tr "py -3 %SCRIPT_PATH% --topic GRC && py -3 %SCRIPT_PATH% --topic ARC" /sc daily /st 08:00 /f

REM Task 2: 14:00 - SAIL + OSO
schtasks /create /tn "Skyworks_ContextPack_Afternoon" /tr "py -3 %SCRIPT_PATH% --topic SAIL && py -3 %SCRIPT_PATH% --topic OSO" /sc daily /st 14:00 /f

REM Task 3: 20:00 - PDRA + STS
schtasks /create /tn "Skyworks_ContextPack_Evening" /tr "py -3 %SCRIPT_PATH% --topic PDRA && py -3 %SCRIPT_PATH% --topic STS" /sc daily /st 20:00 /f

echo.
echo ✓ Created 3 scheduled tasks:
echo   - Skyworks_ContextPack_Morning (08:00)
echo   - Skyworks_ContextPack_Afternoon (14:00)
echo   - Skyworks_ContextPack_Evening (20:00)
echo.
echo To view tasks: schtasks /query /tn "Skyworks_ContextPack_*"
echo To delete tasks: schtasks /delete /tn "Skyworks_ContextPack_*" /f
echo.
pause
