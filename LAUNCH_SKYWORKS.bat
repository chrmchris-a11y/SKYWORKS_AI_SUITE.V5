@echo off
REM ========================================
REM SKYWORKS AI SUITE - DESKTOP LAUNCHER
REM ========================================
REM Quick launcher for VS Code + Copilot Chat
REM ========================================

title SKYWORKS Mission Planner - Launcher

REM Run PowerShell script with execution policy bypass
powershell.exe -ExecutionPolicy Bypass -File "%~dp0LAUNCH_SKYWORKS.ps1"
