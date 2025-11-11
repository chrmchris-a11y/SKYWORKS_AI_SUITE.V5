# SKYWORKS V5 - Interactive GUI Launcher
# Professional Windows Forms menu with progress tracking

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$workspaceRoot = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"

# Create main form
$form = New-Object System.Windows.Forms.Form
$form.Text = "SKYWORKS AI Suite V5 - Quick Launcher"
$form.Size = New-Object System.Drawing.Size(600, 500)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox = $false
$form.BackColor = [System.Drawing.Color]::FromArgb(240, 240, 240)

# Title Label
$titleLabel = New-Object System.Windows.Forms.Label
$titleLabel.Location = New-Object System.Drawing.Point(20, 20)
$titleLabel.Size = New-Object System.Drawing.Size(560, 40)
$titleLabel.Text = "🚀 SKYWORKS AI Suite V5"
$titleLabel.Font = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Bold)
$titleLabel.ForeColor = [System.Drawing.Color]::FromArgb(0, 120, 215)
$form.Controls.Add($titleLabel)

# Subtitle
$subtitleLabel = New-Object System.Windows.Forms.Label
$subtitleLabel.Location = New-Object System.Drawing.Point(20, 60)
$subtitleLabel.Size = New-Object System.Drawing.Size(560, 25)
$subtitleLabel.Text = "EASA/JARUS SORA 2.5 Compliance Platform"
$subtitleLabel.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$subtitleLabel.ForeColor = [System.Drawing.Color]::Gray
$form.Controls.Add($subtitleLabel)

# Progress Bar (hidden initially)
$progressBar = New-Object System.Windows.Forms.ProgressBar
$progressBar.Location = New-Object System.Drawing.Point(20, 400)
$progressBar.Size = New-Object System.Drawing.Size(560, 25)
$progressBar.Visible = $false
$form.Controls.Add($progressBar)

# Status Label
$statusLabel = New-Object System.Windows.Forms.Label
$statusLabel.Location = New-Object System.Drawing.Point(20, 430)
$statusLabel.Size = New-Object System.Drawing.Size(560, 20)
$statusLabel.Text = ""
$statusLabel.Font = New-Object System.Drawing.Font("Segoe UI", 9)
$statusLabel.ForeColor = [System.Drawing.Color]::Green
$form.Controls.Add($statusLabel)

# Function to create styled button
function New-StyledButton {
    param($text, $y, $icon)
    $button = New-Object System.Windows.Forms.Button
    $button.Location = New-Object System.Drawing.Point(20, $y)
    $button.Size = New-Object System.Drawing.Size(560, 45)
    $button.Text = "$icon  $text"
    $button.Font = New-Object System.Drawing.Font("Segoe UI", 11)
    $button.BackColor = [System.Drawing.Color]::White
    $button.FlatStyle = "Flat"
    $button.FlatAppearance.BorderColor = [System.Drawing.Color]::FromArgb(0, 120, 215)
    $button.FlatAppearance.BorderSize = 1
    $button.Cursor = [System.Windows.Forms.Cursors]::Hand
    return $button
}

# Button 1: Open VS Code + Chat with Copilot
$btnChat = New-StyledButton "💬 Open VS Code & Chat with Copilot" 100 "🤖"
$btnChat.Add_Click({
    $statusLabel.Text = "Opening VS Code with Copilot Chat..."
    $statusLabel.ForeColor = [System.Drawing.Color]::Blue
    $form.Refresh()
    
    Start-Process "code" -ArgumentList $workspaceRoot
    Start-Sleep -Seconds 2
    # Open Copilot Chat panel
    Start-Process "code" -ArgumentList "--command", "workbench.action.chat.open"
    
    $statusLabel.Text = "✅ VS Code opened with Copilot Chat!"
    $statusLabel.ForeColor = [System.Drawing.Color]::Green
})
$form.Controls.Add($btnChat)

# Button 2: Backup Project with Progress
$btnBackup = New-StyledButton "💾 Backup Project (with progress %)" 155 "📦"
$btnBackup.Add_Click({
    $statusLabel.Text = "Starting backup..."
    $statusLabel.ForeColor = [System.Drawing.Color]::Blue
    $progressBar.Visible = $true
    $progressBar.Value = 0
    $progressBar.Style = "Continuous"
    $form.Refresh()
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupPath = "C:\Users\chrmc\Desktop\SKYWORKS_V5_BACKUP_$timestamp"
    
    # Run backup in background with progress updates
    $job = Start-Job -ScriptBlock {
        param($source, $dest)
        
        $allFiles = Get-ChildItem -Path $source -Recurse -File -ErrorAction SilentlyContinue | 
            Where-Object { $_.FullName -notmatch '(node_modules|\.git|bin|obj|venv|__pycache__|test-results|playwright-report)' }
        
        $totalFiles = $allFiles.Count
        $current = 0
        
        New-Item -ItemType Directory -Path $dest -Force | Out-Null
        
        foreach ($file in $allFiles) {
            $current++
            $percent = [math]::Round(($current / $totalFiles) * 100)
            
            Write-Progress -Activity "Backing up files" -Status "$percent% Complete" -PercentComplete $percent
            
            $relativePath = $file.FullName.Substring($source.Length + 1)
            $destPath = Join-Path $dest $relativePath
            $destDir = Split-Path $destPath -Parent
            
            if (-not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
            }
            
            Copy-Item -Path $file.FullName -Destination $destPath -Force -ErrorAction SilentlyContinue
            
            # Output progress for parent script
            [PSCustomObject]@{
                Percent = $percent
                Current = $current
                Total = $totalFiles
            }
        }
    } -ArgumentList $workspaceRoot, $backupPath
    
    # Monitor job progress
    while ($job.State -eq 'Running') {
        $progress = Receive-Job -Job $job -ErrorAction SilentlyContinue | Select-Object -Last 1
        if ($progress) {
            $progressBar.Value = $progress.Percent
            $statusLabel.Text = "Backing up... $($progress.Percent)% ($($progress.Current)/$($progress.Total) files)"
            $form.Refresh()
        }
        Start-Sleep -Milliseconds 100
    }
    
    # Cleanup
    Remove-Job -Job $job -Force
    
    $progressBar.Value = 100
    $statusLabel.Text = "✅ Backup complete! Opening folder..."
    $statusLabel.ForeColor = [System.Drawing.Color]::Green
    $form.Refresh()
    
    # Open backup folder
    Start-Process "explorer.exe" -ArgumentList $backupPath
    
    Start-Sleep -Seconds 2
    $progressBar.Visible = $false
    $statusLabel.Text = "✅ Backup saved to: $backupPath"
})
$form.Controls.Add($btnBackup)

# Button 3: Open Visual Studio (Skyworks.sln)
$btnVS = New-StyledButton "🎯 Open Backend Solution (Visual Studio)" 210 "🔧"
$btnVS.Add_Click({
    $statusLabel.Text = "Opening Visual Studio..."
    $statusLabel.ForeColor = [System.Drawing.Color]::Blue
    $form.Refresh()
    
    Start-Process "$workspaceRoot\Backend\Skyworks.sln"
    
    $statusLabel.Text = "✅ Visual Studio opened!"
    $statusLabel.ForeColor = [System.Drawing.Color]::Green
})
$form.Controls.Add($btnVS)

# Button 4: Run All Tests
$btnTests = New-StyledButton "🧪 Run All Tests (Backend + E2E)" 265 "✅"
$btnTests.Add_Click({
    $statusLabel.Text = "Running tests..."
    $statusLabel.ForeColor = [System.Drawing.Color]::Blue
    $progressBar.Visible = $true
    $progressBar.Style = "Marquee"
    $form.Refresh()
    
    Start-Process "pwsh" -ArgumentList "-NoProfile", "-Command", "cd '$workspaceRoot\Backend' ; dotnet test --verbosity minimal ; cd '$workspaceRoot\e2e' ; npx playwright test --reporter=list ; Read-Host 'Press Enter to close'"
    
    $progressBar.Visible = $false
    $statusLabel.Text = "✅ Tests launched in new window!"
    $statusLabel.ForeColor = [System.Drawing.Color]::Green
})
$form.Controls.Add($btnTests)

# Button 5: Start Backend + Python + UI
$btnLaunch = New-StyledButton "🚀 Launch Full Stack (API + Python + UI)" 320 "⚡"
$btnLaunch.Add_Click({
    $statusLabel.Text = "Starting all services..."
    $statusLabel.ForeColor = [System.Drawing.Color]::Blue
    $form.Refresh()
    
    # Start backend
    Start-Process "pwsh" -ArgumentList "-NoProfile", "-Command", "cd '$workspaceRoot\Backend' ; dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210"
    
    Start-Sleep -Seconds 2
    
    # Start Python
    Start-Process "pwsh" -ArgumentList "-NoProfile", "-Command", "cd '$workspaceRoot\Backend_Python' ; if (Test-Path .\venv\Scripts\python.exe) { .\venv\Scripts\python.exe -m uvicorn main:app --host 0.0.0.0 --port 8001 } else { python -m uvicorn main:app --host 0.0.0.0 --port 8001 }"
    
    Start-Sleep -Seconds 5
    
    # Open UI
    Start-Process "msedge" -ArgumentList "http://localhost:5210/app/Pages/index.html"
    
    $statusLabel.Text = "✅ All services started! UI opened in browser."
    $statusLabel.ForeColor = [System.Drawing.Color]::Green
})
$form.Controls.Add($btnLaunch)

# Button 6: Exit
$btnExit = New-StyledButton "❌ Exit" 375 "🚪"
$btnExit.BackColor = [System.Drawing.Color]::FromArgb(255, 240, 240)
$btnExit.Add_Click({
    $form.Close()
})
$form.Controls.Add($btnExit)

# Show form
$form.Add_Shown({$form.Activate()})
[void]$form.ShowDialog()
