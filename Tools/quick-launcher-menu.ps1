#Requires -Version 5.1
<#
.SYNOPSIS
    SKYWORKS V5 Quick Launcher - Interactive Menu
.DESCRIPTION
    Main launcher with options: Open Solution, Backup, Start Services, Run Tests, etc.
#>

param()

$WorkspaceRoot = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"
$BackupRoot = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Tools\Backup"

function Show-Menu {
    Clear-Host
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "    SKYWORKS AI SUITE V5 - QUICK LAUNCHER MENU" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  [1] Open Solution in Visual Studio (Backend\Skyworks.sln)" -ForegroundColor Green
    Write-Host "  [2] Backup Project → Desktop (FOLDER with % progress)" -ForegroundColor Magenta
    Write-Host "  [3] Start Backend API + Python FastAPI (5210 + 8001)" -ForegroundColor Yellow
    Write-Host "  [4] Run All Tests (Backend + E2E Playwright)" -ForegroundColor Cyan
    Write-Host "  [5] Open Project Status (PROJECT_STATUS.md)" -ForegroundColor White
    Write-Host "  [6] Launch Full Stack (MCP + API + Python + Train + UI)" -ForegroundColor DarkYellow
    Write-Host "  [7] Open Workspace in VS Code" -ForegroundColor Blue
    Write-Host ""
    Write-Host "  [0] Exit" -ForegroundColor Red
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
}

function Open-Solution {
    Write-Host "`n🚀 Opening Skyworks.sln in Visual Studio..." -ForegroundColor Green
    $sln = Join-Path $WorkspaceRoot "Backend\Skyworks.sln"
    if (Test-Path $sln) {
        Start-Process $sln
        Write-Host "✅ Visual Studio launched!" -ForegroundColor Green
    } else {
        Write-Host "❌ Solution file not found: $sln" -ForegroundColor Red
    }
}

function Backup-Project {
    Write-Host "`n💾 Creating project backup (FOLDER - incremental)..." -ForegroundColor Magenta
    
    $BackupFolder = "C:\Users\chrmc\Desktop\SKYWORKS_V5_BACKUP"
    
    Write-Host "  � Backup location: $BackupFolder" -ForegroundColor Cyan
    Write-Host "  ⏳ Scanning files..." -ForegroundColor Yellow
    
    # Exclude node_modules, bin, obj, .git, test-results
    $excludeDirs = @('node_modules', 'bin', 'obj', '.git', 'test-results', 'playwright-report', '.vs', 'build', '__pycache__', 'venv')
    
    try {
        # Get all files except excluded directories
        $allFiles = Get-ChildItem -Path $WorkspaceRoot -Recurse -File -ErrorAction SilentlyContinue | 
            Where-Object { 
                $exclude = $false
                foreach ($dir in $excludeDirs) {
                    if ($_.FullName -like "*\$dir\*") {
                        $exclude = $true
                        break
                    }
                }
                -not $exclude
            }
        
        $totalFiles = $allFiles.Count
        $current = 0
        
        Write-Host "  📊 Total files to backup: $totalFiles" -ForegroundColor Cyan
        Write-Host ""
        
        # Create backup folder if not exists
        if (-not (Test-Path $BackupFolder)) {
            New-Item -ItemType Directory -Path $BackupFolder -Force | Out-Null
        }
        
        # Copy files with progress
        foreach ($file in $allFiles) {
            $current++
            $percent = [math]::Round(($current / $totalFiles) * 100)
            
            # Show progress every 50 files or at 100%
            if ($current % 50 -eq 0 -or $current -eq $totalFiles) {
                Write-Host "`r  ⏳ Progress: $percent% ($current/$totalFiles files)   " -NoNewline -ForegroundColor Yellow
            }
            
            # Calculate relative path
            $relativePath = $file.FullName.Substring($WorkspaceRoot.Length + 1)
            $destPath = Join-Path $BackupFolder $relativePath
            $destDir = Split-Path $destPath -Parent
            
            # Create destination directory if needed
            if (-not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
            }
            
            # Copy file (overwrite if exists - incremental backup)
            Copy-Item -Path $file.FullName -Destination $destPath -Force -ErrorAction SilentlyContinue
        }
        
        Write-Host "`n"
        Write-Host "  ✅ Backup complete! $totalFiles files copied." -ForegroundColor Green
        Write-Host "  📂 Location: $BackupFolder" -ForegroundColor Cyan
        Write-Host "  📝 Note: Backup folder will be updated each time you run this." -ForegroundColor Gray
        
        # Open backup folder in Explorer
        Start-Process explorer.exe -ArgumentList $BackupFolder
        
    } catch {
        Write-Host "`n  ❌ Backup failed: $_" -ForegroundColor Red
    }
}

function Start-Services {
    Write-Host "`n🌐 Starting Backend API (5210) + Python FastAPI (8001)..." -ForegroundColor Yellow
    
    # Start Backend API
    Write-Host "  🔹 Starting ASP.NET Core API on port 5210..." -ForegroundColor Cyan
    Start-Process pwsh -ArgumentList "-NoProfile", "-Command", "cd '$WorkspaceRoot\Backend' ; dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210"
    
    Start-Sleep -Seconds 2
    
    # Start Python FastAPI
    Write-Host "  🔹 Starting Python FastAPI on port 8001..." -ForegroundColor Cyan
    $pythonExe = "$WorkspaceRoot\Backend_Python\venv\Scripts\python.exe"
    if (Test-Path $pythonExe) {
        Start-Process pwsh -ArgumentList "-NoProfile", "-Command", "cd '$WorkspaceRoot\Backend_Python' ; .\venv\Scripts\python.exe -m uvicorn main:app --host 0.0.0.0 --port 8001"
    } else {
        Start-Process pwsh -ArgumentList "-NoProfile", "-Command", "cd '$WorkspaceRoot\Backend_Python' ; python -m uvicorn main:app --host 0.0.0.0 --port 8001"
    }
    
    Write-Host "  ✅ Services starting in background terminals..." -ForegroundColor Green
    Write-Host "  🌍 Backend API: http://localhost:5210" -ForegroundColor White
    Write-Host "  🐍 Python API: http://localhost:8001" -ForegroundColor White
}

function Run-Tests {
    Write-Host "`n🧪 Running all tests..." -ForegroundColor Cyan
    
    Write-Host "  🔹 Backend Tests (.NET)..." -ForegroundColor Yellow
    cd "$WorkspaceRoot\Backend"
    dotnet test --verbosity minimal
    
    Write-Host "`n  🔹 E2E Tests (Playwright)..." -ForegroundColor Yellow
    cd "$WorkspaceRoot\e2e"
    npx playwright test --workers=1 --reporter=list
    
    cd $WorkspaceRoot
    Write-Host "`n  ✅ All tests completed!" -ForegroundColor Green
}

function Open-ProjectStatus {
    Write-Host "`n📄 Opening PROJECT_STATUS.md..." -ForegroundColor White
    $statusFile = Join-Path $WorkspaceRoot "PROJECT_STATUS.md"
    if (Test-Path $statusFile) {
        Start-Process code $statusFile
        Write-Host "✅ Opened in VS Code!" -ForegroundColor Green
    } else {
        Write-Host "❌ PROJECT_STATUS.md not found" -ForegroundColor Red
    }
}

function Launch-FullStack {
    Write-Host "`n🚀 Launching Full Stack (MCP + API + Python + Train + UI)..." -ForegroundColor DarkYellow
    Write-Host "  ⚠️  This will start multiple background tasks" -ForegroundColor Yellow
    Write-Host "  📌 Use VS Code Tasks to monitor services" -ForegroundColor Cyan
    
    cd $WorkspaceRoot
    code .
    
    Write-Host "`n  ✅ VS Code opened - Run task 'Launch: MCP + API + Python + Train + UI'" -ForegroundColor Green
}

function Open-VSCode {
    Write-Host "`n💻 Opening workspace in VS Code..." -ForegroundColor Blue
    cd $WorkspaceRoot
    code .
    Write-Host "✅ VS Code launched!" -ForegroundColor Green
}

# Main loop
do {
    Show-Menu
    $choice = Read-Host "Enter your choice (0-7)"
    
    switch ($choice) {
        "1" { Open-Solution }
        "2" { Backup-Project }
        "3" { Start-Services }
        "4" { Run-Tests }
        "5" { Open-ProjectStatus }
        "6" { Launch-FullStack }
        "7" { Open-VSCode }
        "0" { 
            Write-Host "`n👋 Exiting SKYWORKS V5 Launcher..." -ForegroundColor Cyan
            Start-Sleep -Seconds 1
            exit 
        }
        default { 
            Write-Host "`n❌ Invalid choice! Please select 0-7." -ForegroundColor Red 
        }
    }
    
    if ($choice -ne "0") {
        Write-Host "`n"
        Read-Host "Press ENTER to return to menu"
    }
    
} while ($choice -ne "0")
