# SKYWORKS SORA MCP SERVER - Installation Script
# Run this ONCE to set up the MCP server

Write-Host "🚀 SKYWORKS SORA MCP SERVER - Installation" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$mcpPath = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\skyworks-sora-mcp-server"

# Step 1: Check Node.js
Write-Host "📦 Step 1: Checking Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js installed: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js NOT found. Install from https://nodejs.org" -ForegroundColor Red
    exit 1
}

# Step 2: Install dependencies
Write-Host ""
Write-Host "📦 Step 2: Installing dependencies..." -ForegroundColor Yellow
Set-Location $mcpPath
npm install
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Dependencies installed successfully" -ForegroundColor Green
} else {
    Write-Host "❌ npm install failed. Check errors above." -ForegroundColor Red
    exit 1
}

# Step 3: Compile TypeScript
Write-Host ""
Write-Host "🔨 Step 3: Compiling TypeScript..." -ForegroundColor Yellow
npm run build
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ TypeScript compiled successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed. Check errors above." -ForegroundColor Red
    exit 1
}

# Step 4: Test server
Write-Host ""
Write-Host "🧪 Step 4: Testing MCP server..." -ForegroundColor Yellow
$testProcess = Start-Process -FilePath "node" -ArgumentList "build/index.js" -PassThru -NoNewWindow -RedirectStandardError "test-error.log"
Start-Sleep -Seconds 2
if ($testProcess.HasExited) {
    Write-Host "❌ Server crashed. Check test-error.log" -ForegroundColor Red
    Get-Content "test-error.log"
    exit 1
} else {
    Stop-Process -Id $testProcess.Id -Force
    Write-Host "✅ Server starts successfully" -ForegroundColor Green
}

# Step 5: Generate VS Code config
Write-Host ""
Write-Host "⚙️ Step 5: Generating VS Code configuration..." -ForegroundColor Yellow

$vsCodeConfig = @"
{
  "mcp.servers": {
    "skyworks-sora": {
      "command": "node",
      "args": [
        "C:/Users/chrmc/Desktop/SKYWORKS_AI_SUITE.V5/skyworks-sora-mcp-server/build/index.js"
      ]
    }
  }
}
"@

$configPath = Join-Path $mcpPath "vscode-settings-snippet.json"
$vsCodeConfig | Out-File -FilePath $configPath -Encoding UTF8
Write-Host "✅ Config saved to: $configPath" -ForegroundColor Green

# Step 6: Instructions
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✨ INSTALLATION COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 NEXT STEPS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Open VS Code Settings (Ctrl+,)" -ForegroundColor White
Write-Host "2. Search for 'mcp'" -ForegroundColor White
Write-Host "3. Click 'Edit in settings.json'" -ForegroundColor White
Write-Host "4. Add this to the JSON file:" -ForegroundColor White
Write-Host ""
Write-Host $vsCodeConfig -ForegroundColor Cyan
Write-Host ""
Write-Host "5. Reload VS Code:" -ForegroundColor White
Write-Host "   Ctrl+Shift+P → 'Developer: Reload Window'" -ForegroundColor White
Write-Host ""
Write-Host "6. Verify MCP server is running:" -ForegroundColor White
Write-Host "   New AI chat → Should see 'skyworks-sora' tools" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📚 Documentation:" -ForegroundColor Yellow
Write-Host "   - MCP_SERVER_GUIDE.md" -ForegroundColor White
Write-Host "   - PROJECT_ONBOARDING.md" -ForegroundColor White
Write-Host "   - README_NEW_CHAT.md" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Test with:" -ForegroundColor Yellow
Write-Host "   AI Agent: Use MCP tool 'get_grc_table'" -ForegroundColor White
Write-Host ""
Write-Host "✅ Installation successful! 🚁✨" -ForegroundColor Green
