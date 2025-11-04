param(
  [string]$BaseUrl = "http://localhost:5210"
)

$ErrorActionPreference = 'Stop'

function Assert-Ok($resp, $message) {
  if (-not $resp) { throw "SmokeTest failed: $message" }
}

Write-Host "[web-smoke] Checking mission.html" -ForegroundColor Cyan
$mission = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/app/Pages/mission.html" -Method GET
Assert-Ok ($mission.StatusCode -eq 200) "mission.html not reachable"

Write-Host "[web-smoke] Checking Swagger" -ForegroundColor Cyan
$sw = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/swagger/v1/swagger.json" -Method GET
Assert-Ok ($sw.StatusCode -eq 200) "swagger not reachable"

Write-Host "[web-smoke] Checking Proxora health" -ForegroundColor Cyan
$ph = Invoke-RestMethod -UseBasicParsing -Uri "$BaseUrl/api/proxora/health" -Method GET
Assert-Ok ($ph.python -ne $null) "proxora health failed"

Write-Host "[web-smoke] Composite SORA proxy" -ForegroundColor Cyan
$body = @{ 
  SoraVersion = '2.5';
  Grc25 = @{ max_dimension_m = 0.7; max_speed_ms = 15; population_density = 1600; is_controlled_ground = $false; m1_strategic = 'Low'; m2_ground_impact = 'Medium' };
  Arc25 = @{ max_height_agl_m = 120; max_speed_ms = 0; airspace_class = 'G'; is_controlled = $false; is_tmz = $false; environment = 'Rural'; is_airport_heliport = $false; is_atypical_segregated = $false; tactical_mitigation_level = 'None' };
  Proximities = @(@{ type='airport'; distanceMeters=420 }, @{ type='school'; distanceMeters=1800 })
} | ConvertTo-Json -Depth 6

$resp = Invoke-RestMethod -UseBasicParsing -Uri "$BaseUrl/api/proxora/sora" -Method POST -ContentType 'application/json' -Body $body
Assert-Ok ($resp -and $resp.grc -and $resp.arc -and $resp.sail) "composite calculation failed"

Write-Host "[web-smoke] OK" -ForegroundColor Green
