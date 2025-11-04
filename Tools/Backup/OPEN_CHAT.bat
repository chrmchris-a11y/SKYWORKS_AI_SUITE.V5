@echo off
setlocal
set SCRIPT_DIR=%~dp0
set PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe
%PS% -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Open-Chat.ps1"
endlocal
