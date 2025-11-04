param(
  [string]$ApiUrl = "http://localhost:5210",
  [string]$PythonHost = "127.0.0.1",
  [int]$PythonPort = 8001
)

$ErrorActionPreference = "Stop"

Write-Host "==== SKYWORKS Web Smoke Tests ====" -ForegroundColor Cyan

function Start-PythonService {
  param([string]$PyHost, [int]$Port)
  # Check health
  try {
  $resp = Invoke-WebRequest -Uri "http://$($PyHost):$($Port)/health" -UseBasicParsing -TimeoutSec 2
  if ($resp.StatusCode -eq 200) { Write-Host "Python already running on $($Host):$($Port)" -ForegroundColor Green; return $null }
  } catch {}

  $env:PYTHONPATH = (Resolve-Path "$PSScriptRoot\..\").Path
  $py = Join-Path (Resolve-Path "$PSScriptRoot\..\Backend_Python\venv\Scripts").Path "python.exe"
  $app = "Backend_Python.main:app"
  $cmd = "$py -m uvicorn $app --host $PyHost --port $Port"
  Write-Host "Starting Python: $cmd" -ForegroundColor Yellow
  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = "pwsh"
  $psi.Arguments = "-NoProfile -Command `$env:PYTHONPATH='$($env:PYTHONPATH)'; $cmd"
  $psi.UseShellExecute = $false
  $psi.CreateNoWindow = $true
  $proc = [System.Diagnostics.Process]::Start($psi)
  # Wait for health
  for ($i=1; $i -le 20; $i++) {
    Start-Sleep -Milliseconds 400
  try { $r = Invoke-WebRequest -Uri "http://$($PyHost):$($Port)/health" -UseBasicParsing -TimeoutSec 2; if ($r.StatusCode -eq 200) { Write-Host "Python ready" -ForegroundColor Green; break } } catch {}
  }
  return $proc
}

function Start-ApiService {
  param([string]$Url)
  # ping info
  try { $r = Invoke-WebRequest -Uri "$Url/api/sora/info" -UseBasicParsing -TimeoutSec 2; if ($r.StatusCode -eq 200) { Write-Host "API already running at $Url" -ForegroundColor Green; return $null } } catch {}
  $root = Resolve-Path "$PSScriptRoot\..\Backend"
  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = "pwsh"
  $psi.Arguments = "-NoProfile -Command cd '$root'; dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls $Url"
  $psi.UseShellExecute = $false
  $psi.CreateNoWindow = $true
  $proc = [System.Diagnostics.Process]::Start($psi)
  for ($i=1; $i -le 30; $i++) {
    Start-Sleep -Milliseconds 500
    try { $r = Invoke-WebRequest -Uri "$Url/api/sora/info" -UseBasicParsing -TimeoutSec 2; if ($r.StatusCode -eq 200) { Write-Host "API ready" -ForegroundColor Green; break } } catch {}
  }
  return $proc
}

function Invoke-Json {
  param([string]$Url, [object]$Body)
  $json = ($Body | ConvertTo-Json -Depth 10 -Compress)
  return Invoke-WebRequest -Uri $Url -Method POST -ContentType 'application/json' -Body $json -UseBasicParsing -TimeoutSec 30 -SkipHttpErrorCheck
}

class SmokeCase {
  [string]$Name
  [string]$Version
  [object]$Request
  [int]$ExpectedStatus = 200
  [scriptblock]$Assert
}

$cases = @()
# ---- SORA 2.0 (10 cases) ----
$cases += [SmokeCase]@{ Name='20-VLOS-LowDim-ARC_a'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; maxCharacteristicDimension=0.5; mitigations=@() }; airRisk=@{ explicitARC='ARC_a'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if($json.soraVersion -ne '2.0'){throw 'wrong version'} if(-not $json.sail){throw 'missing sail'} } }
$cases += [SmokeCase]@{ Name='20-BVLOS-Sparse-ARC_c'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='BVLOS_SparselyPopulated'; maxCharacteristicDimension=2.5; mitigations=@() }; airRisk=@{ explicitARC='ARC_c'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.sail){throw 'missing sail'} } }
$cases += [SmokeCase]@{ Name='20-Boundary-1m'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; maxCharacteristicDimension=1.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='20-Boundary-3m'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; maxCharacteristicDimension=3.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='20-Boundary-8m'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; maxCharacteristicDimension=8.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_d'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='20-ARCd-High'; Version='2.0'; ExpectedStatus=400; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='BVLOS_DenselyPopulated'; maxCharacteristicDimension=4.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_d'; strategicMitigations=@() }; implementedOSOs=@() } }
$cases += [SmokeCase]@{ Name='20-Controlled-Area'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; isControlledGroundArea=$true; maxCharacteristicDimension=0.4; mitigations=@() }; airRisk=@{ explicitARC='ARC_a'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if($json.sail -notin @('I','II')){throw 'unexpected sail'} } }
$cases += [SmokeCase]@{ Name='20-M2-High-penalty'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; maxCharacteristicDimension=2.0; mitigations=@(@{ type='M2'; robustness='High' }) }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='20-M1-High-strong'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='VLOS_SparselyPopulated'; maxCharacteristicDimension=2.0; mitigations=@(@{ type='M1A'; robustness='High' }) }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='20-ARC-floor'; Version='2.0'; ExpectedStatus=200; Request = @{ soraVersion='2.0'; groundRisk=@{ scenario_V2_0='BVLOS_SparselyPopulated'; maxCharacteristicDimension=3.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@('S1','S2') }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.residualARC){throw 'missing residualARC'} } }

# ---- SORA 2.5 (10 cases) ----
$cases += [SmokeCase]@{ Name='25-Sub250g'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=10.0; isControlledGroundArea=$false; maxCharacteristicDimension=0.20; maxSpeed=20.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_a'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if($json.intrinsicGRC -ne 1 -or $json.sail -ne 'I'){throw 'sub250g mismatch'} } }
$cases += [SmokeCase]@{ Name='25-OutOfScope'; Version='2.5'; ExpectedStatus=400; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=9000.0; isControlledGroundArea=$false; maxCharacteristicDimension=9.0; maxSpeed=90.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_d'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.intrinsicGRC -or $json.intrinsicGRC -lt 9){throw 'expected igrc >= 9'} } }
$cases += [SmokeCase]@{ Name='25-3m@20ms-WorstCase'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=50.0; isControlledGroundArea=$false; maxCharacteristicDimension=3.0; maxSpeed=20.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='25-ControlledGround'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=100.0; isControlledGroundArea=$true; maxCharacteristicDimension=2.0; maxSpeed=15.0; mitigations=@(@{ type='M1A'; robustness='High'}) }; airRisk=@{ explicitARC='ARC_a'; strategicMitigations=@() }; implementedOSOs=@() }; Assert={ param($json) if($json.sail -notin @('I','II')){throw 'unexpected sail'} } }
$cases += [SmokeCase]@{ Name='25-ARCb-Mitigations'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=300.0; isControlledGroundArea=$false; maxCharacteristicDimension=2.0; maxSpeed=12.0; mitigations=@(@{ type='M2'; robustness='High'}) }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@('S1') }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.initialARC){throw 'missing initialARC'} } }
$cases += [SmokeCase]@{ Name='25-Boundary-1m'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=20.0; isControlledGroundArea=$false; maxCharacteristicDimension=1.0; maxSpeed=5.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_a' }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='25-Boundary-3m'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=20.0; isControlledGroundArea=$false; maxCharacteristicDimension=3.0; maxSpeed=5.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_a' }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='25-Boundary-8m'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=20.0; isControlledGroundArea=$false; maxCharacteristicDimension=8.0; maxSpeed=5.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_a' }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.finalGRC){throw 'missing finalGRC'} } }
$cases += [SmokeCase]@{ Name='25-ARC-floor'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=80.0; isControlledGroundArea=$false; maxCharacteristicDimension=2.5; maxSpeed=18.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_b'; strategicMitigations=@('S1','S2') }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.residualARC){throw 'missing residualARC'} } }
$cases += [SmokeCase]@{ Name='25-ARCd-High'; Version='2.5'; ExpectedStatus=200; Request = @{ soraVersion='2.5'; groundRisk=@{ populationDensity=400.0; isControlledGroundArea=$false; maxCharacteristicDimension=4.0; maxSpeed=22.0; mitigations=@() }; airRisk=@{ explicitARC='ARC_d' }; implementedOSOs=@() }; Assert={ param($json) if(-not $json.initialARC){throw 'missing initialARC'} } }

$pythonProc = $null
$apiProc = $null
try {
  $pythonProc = Start-PythonService -PyHost $PythonHost -Port $PythonPort
  $apiProc = Start-ApiService -Url $ApiUrl

  $ok=0; $fail=0
  foreach ($c in $cases) {
    $name = $c.Name
    $url = "$ApiUrl/api/sora/complete"
    try {
      $resp = Invoke-Json -Url $url -Body $c.Request
      $body = $resp.Content | ConvertFrom-Json
      if ($resp.StatusCode -ne $c.ExpectedStatus) { throw "Expected $($c.ExpectedStatus) got $($resp.StatusCode)" }
      if ($c.ExpectedStatus -eq 200) { if ($body.soraVersion -ne $c.Version) { throw "Version mismatch" } }
      if ($c.Assert) { & $c.Assert $body }
      Write-Host ("PASS  [{0}] {1}" -f $c.Version, $name) -ForegroundColor Green
      $ok++
    }
    catch {
      Write-Host ("FAIL  [{0}] {1} -> {2}" -f $c.Version, $name, $_.Exception.Message) -ForegroundColor Red
      if ($_.Exception.Response -and $_.Exception.Response.Content) {
        try { $errBody = $_.Exception.Response.Content | ConvertFrom-Json; if($errBody.intrinsicGRC){ Write-Host ("   error.iGRC: {0}" -f $errBody.intrinsicGRC) -ForegroundColor DarkYellow } } catch {}
      }
      $fail++
    }
  }
  Write-Host "==== Summary: Passed=$ok Failed=$fail Total=$($ok+$fail)" -ForegroundColor Cyan
}
finally {
  if ($apiProc -and -not $apiProc.HasExited) { try { $apiProc.Kill(); Start-Sleep -Milliseconds 200 } catch {} }
  if ($pythonProc -and -not $pythonProc.HasExited) { try { $pythonProc.Kill(); Start-Sleep -Milliseconds 200 } catch {} }
}
