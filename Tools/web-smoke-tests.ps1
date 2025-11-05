param(
  [string]$BaseUrl = "http://localhost:5210",
  [string]$PythonUrl = "http://localhost:8001"
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
# Εναρμόνιση με Annex F (GRC 2.5): απαιτούνται mtom_kg, max_characteristic_dimension_m, max_speed_ms, population_density.
$body = @{ 
  SoraVersion = '2.5';
  Grc25 = @{ 
    mtom_kg = 0.8; 
    max_dimension_m = 0.7; # back-compat για binder
    max_characteristic_dimension_m = 0.7; 
    max_speed_ms = 15; 
    population_density = 1600; 
    weight_kg = 0.8; # back-compat για binder
    m1a_sheltering = 'None'; 
    m1b_operational = 'None'; 
    m1c_ground_observation = 'None'; 
    m2_impact = 'None' 
  };
  Arc25 = @{ 
    max_height_agl_m = 120; 
    max_speed_ms = 15; 
    airspace_class = 'G'; 
    is_controlled = $false; 
    is_tmz = $false; 
    environment = 'Rural'; 
    is_airport_heliport = $false; 
    is_atypical_segregated = $false; 
    tactical_mitigation_level = 'None' 
  };
  Proximities = @(@{ type='airport'; distanceMeters=420 }, @{ type='school'; distanceMeters=1800 })
} | ConvertTo-Json -Depth 6

$resp = Invoke-RestMethod -UseBasicParsing -Uri "$BaseUrl/api/proxora/sora" -Method POST -ContentType 'application/json' -Body $body
Assert-Ok ($resp -and $resp.grc -and $resp.arc -and $resp.sail) "composite calculation failed"

# Extra: direct Python SAIL checks
Write-Host "[web-smoke] Python SAIL 2.0 check" -ForegroundColor Cyan
$py20 = @{ sora_version = '2.0'; final_grc = 5; residual_arc = 'ARC-c' } | ConvertTo-Json
$r20 = Invoke-RestMethod -UseBasicParsing -Uri "$PythonUrl/api/v1/calculate/sail" -Method POST -ContentType 'application/json' -Body $py20
Assert-Ok ($r20.sail_level -eq 'IV') "Python 2.0 SAIL expected IV"

Write-Host "[web-smoke] Python SAIL 2.5 check" -ForegroundColor Cyan
$py25 = @{ sora_version = '2.5'; final_grc = 9; residual_arc_level = 5 } | ConvertTo-Json
$r25 = Invoke-RestMethod -UseBasicParsing -Uri "$PythonUrl/api/v1/calculate/sail" -Method POST -ContentType 'application/json' -Body $py25
Assert-Ok ($r25.sail_level -eq 'VI') "Python 2.5 SAIL expected VI for GRC=9"

Write-Host "[web-smoke] OK" -ForegroundColor Green
