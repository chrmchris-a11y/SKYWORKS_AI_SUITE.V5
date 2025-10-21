@echo off
REM ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
REM Phase1 Step5.1 — Skyworks V5: AI Agent Training Scheduler
REM ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
REM
REM Δημιουργεί 3 προγραμματισμένες εργασίες στα Windows για καθημερινή εκπαίδευση
REM των AI agents στις 08:00, 14:00, 20:00
REM
REM Agents:
REM   1. SORA_Compliance_Agent (SORA 2.0/2.5, PDRA, GRC, ARC, SAIL, OSO)
REM   2. Mission_Planning_Agent (STS-01/02, Operations, Mission Planning)
REM ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

echo.
echo ╔═══════════════════════════════════════════════════════════════════╗
echo ║   SKYWORKS AI AGENT TRAINING SCHEDULER                            ║
echo ║   Creating 3 Daily Training Sessions (08:00, 14:00, 20:00)       ║
echo ╚═══════════════════════════════════════════════════════════════════╝
echo.

SET SCRIPT_PATH=%~dp0agent_trainer.py

REM Delete existing tasks if they exist
echo [1/4] Removing existing scheduled tasks (if any)...
schtasks /delete /tn "Skyworks_AgentTraining_Morning" /f >nul 2>&1
schtasks /delete /tn "Skyworks_AgentTraining_Afternoon" /f >nul 2>&1
schtasks /delete /tn "Skyworks_AgentTraining_Evening" /f >nul 2>&1
echo ✓ Cleanup complete

REM Task 1: 08:00 - Morning Training (Full Knowledge Refresh)
echo.
echo [2/4] Creating Morning Training Task (08:00)...
schtasks /create /tn "Skyworks_AgentTraining_Morning" /tr "py -3 \"%SCRIPT_PATH%\"" /sc daily /st 08:00 /f
IF %ERRORLEVEL% EQU 0 (
    echo ✓ Morning training scheduled successfully
) ELSE (
    echo ✗ Failed to schedule morning training
)

REM Task 2: 14:00 - Afternoon Training (Incremental Update)
echo.
echo [3/4] Creating Afternoon Training Task (14:00)...
schtasks /create /tn "Skyworks_AgentTraining_Afternoon" /tr "py -3 \"%SCRIPT_PATH%\"" /sc daily /st 14:00 /f
IF %ERRORLEVEL% EQU 0 (
    echo ✓ Afternoon training scheduled successfully
) ELSE (
    echo ✗ Failed to schedule afternoon training
)

REM Task 3: 20:00 - Evening Training (Daily Summary & Consolidation)
echo.
echo [4/4] Creating Evening Training Task (20:00)...
schtasks /create /tn "Skyworks_AgentTraining_Evening" /tr "py -3 \"%SCRIPT_PATH%\"" /sc daily /st 20:00 /f
IF %ERRORLEVEL% EQU 0 (
    echo ✓ Evening training scheduled successfully
) ELSE (
    echo ✗ Failed to schedule evening training
)

echo.
echo ╔═══════════════════════════════════════════════════════════════════╗
echo ║   SCHEDULED TASKS CREATED SUCCESSFULLY                            ║
echo ╚═══════════════════════════════════════════════════════════════════╝
echo.
echo Training Schedule:
echo   - 08:00: Morning Training   (Full Knowledge Refresh)
echo   - 14:00: Afternoon Training (Incremental Update)
echo   - 20:00: Evening Training   (Daily Summary)
echo.
echo Agents Trained:
echo   1. SORA_Compliance_Agent
echo      • SORA 2.0 AMC, JARUS SORA 2.5
echo      • PDRA-01, PDRA-02
echo      • GRC, ARC, SAIL, OSO
echo      • Operational Authorization
echo.
echo   2. Mission_Planning_Agent
echo      • STS-01 (VLOS operations)
echo      • STS-02 (BVLOS operations)
echo      • Operation Manual creation
echo      • Mission planning with markers
echo.
echo Memory Location:
echo   Tools/TrainingCenter/agent_memory/
echo.
echo To view scheduled tasks:
echo   schtasks /query /tn "Skyworks_AgentTraining_*"
echo.
echo To run training manually:
echo   py -3 "%SCRIPT_PATH%"
echo.
pause
