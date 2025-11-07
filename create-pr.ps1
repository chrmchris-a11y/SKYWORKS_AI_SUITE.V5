# Create GitHub PR for feat/ui-mission-planner-spec-pack
# Usage: Set $env:GITHUB_TOKEN first, then run this script

param(
    [string]$Token = $env:GITHUB_TOKEN
)

if (-not $Token) {
    Write-Error "GitHub token required. Set `$env:GITHUB_TOKEN or pass -Token parameter"
    exit 1
}

# Load PR body from template
$prBody = Get-Content -Path "PR_TEMPLATE.md" -Raw

# Strip the markdown title (first line) since GitHub API wants title separate
$bodyLines = $prBody -split "`n"
$bodyContent = ($bodyLines[2..($bodyLines.Length-1)] -join "`n").Trim()

# PR metadata
$owner = "chrmchris-a11y"
$repo = "SKYWORKS_AI_SUITE.V5"
$head = "feat/ui-mission-planner-spec-pack"
$base = "main"
$title = "feat(ui): Mission Planner UI + Phase 6 Airspace Maps"

# Build JSON payload
$payload = @{
    title = $title
    head = $head
    base = $base
    body = $bodyContent
    draft = $false
} | ConvertTo-Json -Depth 10

# Create PR via GitHub API
$headers = @{
    "Authorization" = "Bearer $Token"
    "Accept" = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

Write-Host "🚀 Creating PR: $title" -ForegroundColor Cyan
Write-Host "   Branch: $head → $base" -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/pulls" `
        -Method Post `
        -Headers $headers `
        -Body $payload `
        -ContentType "application/json"
    
    Write-Host "✅ PR created successfully!" -ForegroundColor Green
    Write-Host "   PR #$($response.number): $($response.html_url)" -ForegroundColor Green
    
    # Add labels
    Write-Host "`n🏷️  Adding labels..." -ForegroundColor Cyan
    $labels = @("ui", "maps", "sora", "compliance", "ready-for-merge")
    $labelPayload = @{
        labels = $labels
    } | ConvertTo-Json
    
    $labelResponse = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/issues/$($response.number)/labels" `
        -Method Post `
        -Headers $headers `
        -Body $labelPayload `
        -ContentType "application/json"
    
    Write-Host "✅ Labels added: $($labels -join ', ')" -ForegroundColor Green
    
    # Output PR URL for easy access
    Write-Host "`n📋 PR Details:" -ForegroundColor Yellow
    Write-Host "   Number: #$($response.number)" -ForegroundColor White
    Write-Host "   URL: $($response.html_url)" -ForegroundColor White
    Write-Host "   Status: $($response.state)" -ForegroundColor White
    
    # Copy URL to clipboard
    $response.html_url | Set-Clipboard
    Write-Host "`n✅ PR URL copied to clipboard!" -ForegroundColor Green
    
    return $response
    
} catch {
    Write-Error "Failed to create PR: $_"
    Write-Error $_.Exception.Message
    exit 1
}
