@echo off
echo Opening Skyworks SORA Calculator...
start http://localhost:5210/app/Pages/mission.html
echo.
echo If the page doesn't load, make sure the backend is running:
echo   cd Backend
echo   dotnet run --project src/Skyworks.Api
pause
