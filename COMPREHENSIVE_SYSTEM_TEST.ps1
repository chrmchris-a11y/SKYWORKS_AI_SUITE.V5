# ═══════════════════════════════════════════════════════════════════════════
# SKYWORKS AI SUITE - COMPREHENSIVE SYSTEM TEST
# Full validation of GRC, ARC, SAIL calculations for SORA 2.0 and 2.5
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n╔═══════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  SKYWORKS AI SUITE - COMPREHENSIVE SYSTEM TEST                        ║" -ForegroundColor Cyan
Write-Host "║  Testing SORA 2.0 AMC & JARUS SORA 2.5 Compliance                     ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:8001"
$apiBase = "http://localhost:5210"
$testResults = @()
$passCount = 0
$failCount = 0

function Test-Endpoint {
    param(
        [string]$TestName,
        [string]$Url,
        [hashtable]$Body,
        [hashtable]$ExpectedResults
    )
    
    Write-Host "`n─────────────────────────────────────────────────────────" -ForegroundColor Gray
    Write-Host "TEST: $TestName" -ForegroundColor Yellow
    
    try {
        # Auto-inject SORA version for SAIL endpoint if missing (default 2.0)
        if ($Url -like "*/sail") {
            if (-not $Body.ContainsKey('sora_version')) {
                $Body['sora_version'] = '2.0'
            }
            # Normalize ARC token format (ARC-a -> ARC_a)
            if ($Body.ContainsKey('residual_arc')) {
                $Body['residual_arc'] = ($Body['residual_arc'] -replace '-', '_')
            }
        }

        $jsonBody = $Body | ConvertTo-Json -Depth 10
        $response = Invoke-RestMethod -Uri $Url -Method POST -Body $jsonBody -ContentType "application/json" -ErrorAction Stop
        
        $passed = $true
        $details = @()
        
        foreach ($key in $ExpectedResults.Keys) {
            $expected = $ExpectedResults[$key]
            # Backward-compat mapping: initial_grc -> intrinsic_grc
            if ($key -eq 'initial_grc') {
                $actual = $response.intrinsic_grc
            } elseif ($key -eq 'mitigation_total') {
                # Compute mitigation_total from m*_effect fields
                $m1 = [int]($response.m1_effect | ForEach-Object { $_ })
                $m2 = [int]($response.m2_effect | ForEach-Object { $_ })
                $m3 = [int]($response.m3_effect | ForEach-Object { $_ })
                if ([string]::IsNullOrEmpty($m1)) { $m1 = 0 }
                if ([string]::IsNullOrEmpty($m2)) { $m2 = 0 }
                if ([string]::IsNullOrEmpty($m3)) { $m3 = 0 }
                $actual = $m1 + $m2 + $m3
            } elseif (($key -eq 'initial_arc') -or ($key -eq 'residual_arc')) {
                $actual = $response.$key
                # Normalize ARC actuals: single-letter (b) -> ARC-b, snake -> dash
                if ($null -ne $actual) {
                    if ($actual -match '^[a-d]$') {
                        $actual = "ARC-" + $actual.ToUpper()
                    }
                    $actual = ($actual -replace '_', '-')
                }
            } else {
                $actual = $response.$key
            }
            
            if ($actual -eq $expected) {
                $details += "  ✓ $key = $actual (PASS)"
                Write-Host "  ✓ $key = $actual" -ForegroundColor Green
            } else {
                $details += "  ✗ $key = $actual (Expected: $expected) (FAIL)"
                Write-Host "  ✗ $key = $actual (Expected: $expected)" -ForegroundColor Red
                $passed = $false
            }
        }
        
        if ($passed) {
            Write-Host "RESULT: PASS ✓" -ForegroundColor Green
            $script:passCount++
            return @{
                TestName = $TestName
                Status = "PASS"
                Details = $details
                Response = $response
            }
        } else {
            Write-Host "RESULT: FAIL ✗" -ForegroundColor Red
            $script:failCount++
            return @{
                TestName = $TestName
                Status = "FAIL"
                Details = $details
                Response = $response
            }
        }
    }
    catch {
        Write-Host "RESULT: ERROR ✗" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        $script:failCount++
        return @{
            TestName = $TestName
            Status = "ERROR"
            Details = @("Error: $($_.Exception.Message)")
            Response = $null
        }
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# SECTION 1: GRC CALCULATIONS - SORA 2.0
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "SECTION 1: GRC CALCULATIONS - SORA 2.0 (JAR_DOC_06)" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

# Test 1.1: Small drone, low population
# JARUS Table 2: 0.5kg (Cat 1) × 500 pop/km² (Cat 1) = iGRC 2
$testResults += Test-Endpoint -TestName "GRC 2.0 - Small Drone (0.5kg), Sparse (500/km²)" `
    -Url "$baseUrl/api/v1/calculate/grc/2.0" `
    -Body @{
        mtom_kg = 0.5
        population_density = 500
        m1_strategic = "Medium"
        m2_impact = "Medium"
        m3_erp = "Medium"
    } `
    -ExpectedResults @{
        initial_grc = 2
        final_grc = 1
        mitigation_total = -3
    }

# Test 1.2: Medium drone (32kg), urban (25,000/km²) - OUR MAIN TEST CASE
# JARUS Table 2: 32kg (Cat 3) × 25k pop/km² (Cat 3) = iGRC 7
$testResults += Test-Endpoint -TestName "GRC 2.0 - Medium Drone (32kg), Urban (25k/km²)" `
    -Url "$baseUrl/api/v1/calculate/grc/2.0" `
    -Body @{
        mtom_kg = 32
        population_density = 25000
        m1_strategic = "Medium"
        m2_impact = "High"
        m3_erp = "Medium"
    } `
    -ExpectedResults @{
        initial_grc = 7
        final_grc = 3
        mitigation_total = -4
    }

# Test 1.3: Large drone (100kg), very high density (50,000/km²)
# JARUS Table 2: 100kg (Cat 3) × 50k pop/km² (Cat 3) = iGRC 7
$testResults += Test-Endpoint -TestName "GRC 2.0 - Large Drone (100kg), Very High Density (50k/km²)" `
    -Url "$baseUrl/api/v1/calculate/grc/2.0" `
    -Body @{
        mtom_kg = 100
        population_density = 50000
        m1_strategic = "High"
        m2_impact = "High"
        m3_erp = "Medium"
    } `
    -ExpectedResults @{
        initial_grc = 7
        final_grc = 3
        mitigation_total = -4
    }

# Test 1.4: Minimum mitigations
$testResults += Test-Endpoint -TestName "GRC 2.0 - Medium Drone (32kg), No Mitigations" `
    -Url "$baseUrl/api/v1/calculate/grc/2.0" `
    -Body @{
        mtom_kg = 32
        population_density = 25000
        m1_strategic = "Low"
        m2_impact = "Low"
        m3_erp = "Low"
    } `
    -ExpectedResults @{
        initial_grc = 7
        final_grc = 6
        mitigation_total = -1
    }

# ═══════════════════════════════════════════════════════════════════════════
# SECTION 2: GRC CALCULATIONS - SORA 2.5
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "SECTION 2: GRC CALCULATIONS - SORA 2.5 (JAR_DOC_25)" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

# Test 2.1: Small drone, low population
# JARUS Table 2: 0.5kg (Cat 1) × 500 pop/km² (Cat 1) = iGRC 2
$testResults += Test-Endpoint -TestName "GRC 2.5 - Small Drone (0.5kg), Sparse (500/km²)" `
    -Url "$baseUrl/api/v1/calculate/grc/2.5" `
    -Body @{
        mtom_kg = 0.5
        population_density = 500
        m1a_sheltering = "Low"
        m1b_operational = "Medium"
        m1c_ground_observation = "Low"
        m2_impact = "Medium"
    } `
    -ExpectedResults @{
        initial_grc = 2
        final_grc = 1
        mitigation_total = -4
    }

# Test 2.2: Medium drone (32kg), urban (25,000/km²) - CONSISTENCY CHECK WITH SORA 2.0
$testResults += Test-Endpoint -TestName "GRC 2.5 - Medium Drone (32kg), Urban (25k/km²) - CONSISTENCY TEST" `
    -Url "$baseUrl/api/v1/calculate/grc/2.5" `
    -Body @{
        mtom_kg = 32
        population_density = 25000
        m1a_sheltering = "Low"
        m1b_operational = "Medium"
        m1c_ground_observation = "Low"
        m2_impact = "Medium"
    } `
    -ExpectedResults @{
        initial_grc = 7
        final_grc = 3
        mitigation_total = -4
    }

# Test 2.3: Large drone (100kg), very high density (50k/km²)
# JARUS Table 2: 100kg (Cat 3) × 50k pop/km² (Cat 3) = iGRC 7
# Mitigations: M1A Medium=-2, M1B Medium=-1, M1C Medium=-1, M2 High=-2 = -6
# Final GRC: 7 - 6 = 1
$testResults += Test-Endpoint -TestName "GRC 2.5 - Large Drone (100kg), Very High Density (50k/km²)" `
    -Url "$baseUrl/api/v1/calculate/grc/2.5" `
    -Body @{
        mtom_kg = 100
        population_density = 50000
        m1a_sheltering = "Medium"
        m1b_operational = "Medium"
        m1c_ground_observation = "Medium"
        m2_impact = "High"
    } `
    -ExpectedResults @{
        initial_grc = 7
        final_grc = 1
        mitigation_total = -6
    }

# ═══════════════════════════════════════════════════════════════════════════
# SECTION 3: ARC CALCULATIONS - SORA 2.0
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "SECTION 3: ARC CALCULATIONS - SORA 2.0" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

# Test 3.1: Low altitude, controlled airspace
# Class G, Urban, 60ft → AEC 9 → ARC-c
$testResults += Test-Endpoint -TestName "ARC 2.0 - Low Altitude (60ft), Class G, Urban" `
    -Url "$apiBase/api/Proxora/arc/2.0" `
    -Body @{
        airspace_class = "G"
        altitude_agl_ft = 60
        environment = "Urban"
        is_atypical_segregated = $false
        strategic_mitigations = @()
    } `
    -ExpectedResults @{
        initial_arc = "ARC-c"
        residual_arc = "ARC-c"
    }

# Test 3.2: High altitude, atypical segregated
# Class G, Urban, Atypical, 400ft → Should be lower ARC with SM mitigations
$testResults += Test-Endpoint -TestName "ARC 2.0 - High Altitude (400ft), Class G, Atypical + SM" `
    -Url "$apiBase/api/Proxora/arc/2.0" `
    -Body @{
        airspace_class = "G"
        altitude_agl_ft = 400
        environment = "Urban"
        is_atypical_segregated = $true
        strategic_mitigations = @("SM1", "SM2")
    } `
    -ExpectedResults @{
        initial_arc = "ARC-c"
        residual_arc = "ARC-a"
    }

# ═══════════════════════════════════════════════════════════════════════════
# SECTION 4: ARC CALCULATIONS - SORA 2.5
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "SECTION 4: ARC CALCULATIONS - SORA 2.5" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

# Test 4.1: Low altitude, controlled airspace (using meters for SORA 2.5)
# Class G, Urban, 20m → AEC 9 → ARC-c
$testResults += Test-Endpoint -TestName "ARC 2.5 - Low Altitude (20m), Class G, Urban" `
    -Url "$apiBase/api/Proxora/arc/2.5" `
    -Body @{
        airspace_class = "G"
        altitude_agl_m = 20
        environment = "Urban"
        is_atypical_segregated = $false
        strategic_mitigations = @()
    } `
    -ExpectedResults @{
        initial_arc = "ARC-c"
        residual_arc = "ARC-c"
    }

# Test 4.2: High altitude with strategic mitigations
$testResults += Test-Endpoint -TestName "ARC 2.5 - High Altitude (120m), Class G, Atypical + SM" `
    -Url "$apiBase/api/Proxora/arc/2.5" `
    -Body @{
        airspace_class = "G"
        altitude_agl_m = 120
        environment = "Urban"
        is_atypical_segregated = $true
        strategic_mitigations = @("SM1", "SM2")
    } `
    -ExpectedResults @{
        initial_arc = "ARC-b"
        residual_arc = "ARC-a"
    }

# ═══════════════════════════════════════════════════════════════════════════
# SECTION 5: SAIL CALCULATIONS (Table 1)
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "SECTION 5: SAIL CALCULATIONS (JARUS SORA Table 1)" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

# Test 5.1: SAIL I (GRC 3, ARC a)
$testResults += Test-Endpoint -TestName "SAIL - GRC 3, ARC-a → SAIL I" `
    -Url "$baseUrl/api/v1/calculate/sail" `
    -Body @{
        final_grc = 3
        residual_arc = "ARC-a"
    } `
    -ExpectedResults @{
        sail = "I"
    }

# Test 5.2: SAIL II (GRC 4, ARC a)
$testResults += Test-Endpoint -TestName "SAIL - GRC 4, ARC-a → SAIL II" `
    -Url "$baseUrl/api/v1/calculate/sail" `
    -Body @{
        final_grc = 4
        residual_arc = "ARC-a"
    } `
    -ExpectedResults @{
        sail = "II"
    }

# Test 5.3: SAIL III (GRC 5, ARC b)
$testResults += Test-Endpoint -TestName "SAIL - GRC 5, ARC-b → SAIL IV" `
    -Url "$baseUrl/api/v1/calculate/sail" `
    -Body @{
        final_grc = 5
        residual_arc = "ARC-b"
    } `
    -ExpectedResults @{
        sail = "IV"
    }

# Test 5.4: SAIL IV (GRC 6, ARC b)
$testResults += Test-Endpoint -TestName "SAIL - GRC 6, ARC-b → SAIL IV" `
    -Url "$baseUrl/api/v1/calculate/sail" `
    -Body @{
        final_grc = 6
        residual_arc = "ARC-b"
    } `
    -ExpectedResults @{
        sail = "IV"
    }

# Test 5.5: SAIL V (GRC 7, ARC c)
$testResults += Test-Endpoint -TestName "SAIL - GRC 7, ARC-c → SAIL VI" `
    -Url "$baseUrl/api/v1/calculate/sail" `
    -Body @{
        final_grc = 7
        residual_arc = "ARC-c"
    } `
    -ExpectedResults @{
        sail = "VI"
    }

# Test 5.6: SAIL VI (GRC 7, ARC d)
$testResults += Test-Endpoint -TestName "SAIL - GRC 7, ARC-d → SAIL VI" `
    -Url "$baseUrl/api/v1/calculate/sail" `
    -Body @{
        final_grc = 7
        residual_arc = "ARC-d"
    } `
    -ExpectedResults @{
        sail = "VI"
    }

# ═══════════════════════════════════════════════════════════════════════════
# FINAL REPORT
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n`n╔═══════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                      COMPREHENSIVE TEST REPORT                        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Host "`nTotal Tests: $($passCount + $failCount)" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })

if ($failCount -eq 0) {
    Write-Host "`n✓✓✓ ALL TESTS PASSED! SYSTEM IS FULLY COMPLIANT ✓✓✓" -ForegroundColor Green
    Write-Host "SORA 2.0 AMC and JARUS SORA 2.5 calculations are ACCURATE!" -ForegroundColor Green
} else {
    Write-Host "`n✗✗✗ SOME TESTS FAILED - REVIEW REQUIRED ✗✗✗" -ForegroundColor Red
}

Write-Host "`n─────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host "Detailed Results:" -ForegroundColor Yellow
foreach ($result in $testResults) {
    $color = if ($result.Status -eq "PASS") { "Green" } else { "Red" }
    Write-Host "  [$($result.Status)] $($result.TestName)" -ForegroundColor $color
}

Write-Host "`n═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
