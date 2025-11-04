@echo off
setlocal
set SCRIPT_DIR=%~dp0
for /f "delims=" %%I in ('where pwsh 2^>nul') do set PS=%%I
if not defined PS set PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe
"%PS%" -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Launcher.ps1"
endlocal
