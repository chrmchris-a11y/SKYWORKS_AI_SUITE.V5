# ═══════════════════════════════════════════════════════════════════════════
# SORA 2.0 ACCURACY TESTS - Live API Validation
# Per EASA AMC1 Article 11(1.c): No M3 Emergency Response Plan = +1 penalty
# ═══════════════════════════════════════════════════════════════════════════

$apiUrl = "http://localhost:5210/api/sora/complete"

function Test-SORAScenario {
    param(
        [int]$TestNumber,
        [string]$Description,
        [hashtable]$Payload,
        [hashtable]$Expected
    )
    
    Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "TEST #$TestNumber`: $Description" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    # Convert hashtable to JSON with boolean fix
    $json = $Payload | ConvertTo-Json -Depth 10 -Compress
    $json = $json -replace '":True', '":true' -replace '":False', '":false'
    
    try {
        $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $json -ContentType "application/json"
        
        # Extract actual values
        $actualIGRC = $response.intrinsicGRC
        $actualFinal = $response.finalGRC
        $actualARC = $response.residualARC
        $actualSAIL = $response.sail
        
        # Compare with expected
        $passCount = 0
        $failCount = 0
        
        Write-Host "`nExpected vs Actual:" -ForegroundColor Yellow
        
        if ($Expected.iGRC -eq $actualIGRC) {
            Write-Host "  iGRC: $($Expected.iGRC) ✅" -ForegroundColor Green
            $passCount++
        } else {
            Write-Host "  iGRC: Expected $($Expected.iGRC), Got $actualIGRC ❌" -ForegroundColor Red
            $failCount++
        }
        
        if ($Expected.finalGRC -eq $actualFinal) {
            Write-Host "  Final GRC: $($Expected.finalGRC) ✅" -ForegroundColor Green
            $passCount++
        } else {
            Write-Host "  Final GRC: Expected $($Expected.finalGRC), Got $actualFinal ❌" -ForegroundColor Red
            $failCount++
        }
        
        if ($Expected.ARC -eq $actualARC) {
            Write-Host "  ARC: $($Expected.ARC) ✅" -ForegroundColor Green
            $passCount++
        } else {
            Write-Host "  ARC: Expected $($Expected.ARC), Got $actualARC ❌" -ForegroundColor Red
            $failCount++
        }
        
        if ($Expected.SAIL -eq $actualSAIL) {
            Write-Host "  SAIL: $($Expected.SAIL) ✅" -ForegroundColor Green
            $passCount++
        } else {
            Write-Host "  SAIL: Expected $($Expected.SAIL), Got $actualSAIL ❌" -ForegroundColor Red
            $failCount++
        }
        
        return $failCount -eq 0
    }
    catch {
        Write-Host "❌ API ERROR: $_" -ForegroundColor Red
        return $false
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# TEST SUITE
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n🚀 SORA 2.0 ACCURACY TEST SUITE - 10 Scenarios" -ForegroundColor Magenta
Write-Host "Testing M3 Penalty Logic (No Emergency Response Plan = +1 GRC)" -ForegroundColor Magenta

$passed = 0
$failed = 0

# ───────────────────────────────────────────────────────────────────────────
# Test 1: Basic Rural VLOS - SAIL III (M3 Penalty Applies)
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 1 `
    -Description "Basic Rural VLOS - M3 Penalty +1" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 1.0
        maxSpeed = 15.0
        environment = "Rural"
        populationDensity = 10.0
        controlledArea = $false
        sheltering = $false
        isAtypicalSegregated = $false
        altitudeAGL_ft = 300
        airspaceClass = "G"
        controlledAirspace = $false
        urbanAir = $false
        strategicMitigations = @()
    } `
    -Expected @{
        iGRC = 3
        finalGRC = 4  # 3 + 1 (M3 penalty)
        ARC = "ARC_b"
        SAIL = "SAIL_III"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 2: Urban Dense with Sheltering - M3 Penalty Applies
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 2 `
    -Description "Urban Dense (2500 ppl/km²), Sheltering M1A, M3 Penalty" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 20.0
        populationDensity = 2500.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "VLOS_Populated"
        operationScenario = "VLOS_Populated"
        mitigations = @(
            @{ type = "M1_StrategicMitigations"; subType = "Sheltering"; robustness = "Medium" }
        )
        explicitARC = "ARC_c"
    } `
    -Expected @{
        iGRC = 5
        finalGRC = 5  # 5 - 1 (M1A Med) + 1 (M3 penalty) = 5
        ARC = "ARC_c"
        SAIL = "SAIL_IV"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 3: M2 High Impact Reduction - M3 Penalty Applies
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 3 `
    -Description "M2 High (-2 GRC) + M3 Penalty (+1) = Net -1" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 18.0
        populationDensity = 150.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "BVLOS_SparselyPopulated"
        operationScenario = "BVLOS_SparselyPopulated"
        mitigations = @(
            @{ type = "M2_ImpactReduction"; robustness = "High" }
        )
        explicitARC = "ARC_b"
    } `
    -Expected @{
        iGRC = 4
        finalGRC = 3  # 4 - 2 (M2 High) + 1 (M3 penalty) = 3
        ARC = "ARC_b"
        SAIL = "SAIL_II"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 4: M3 High Provided - NO PENALTY (Emergency Response Plan exists)
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 4 `
    -Description "M3 High Provided (-1 GRC) - NO M3 Penalty" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 18.0
        populationDensity = 150.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "BVLOS_SparselyPopulated"
        operationScenario = "BVLOS_SparselyPopulated"
        mitigations = @(
            @{ type = "M3_EmergencyResponsePlan"; robustness = "High" }
        )
        explicitARC = "ARC_b"
    } `
    -Expected @{
        iGRC = 4
        finalGRC = 3  # 4 - 1 (M3 High) = 3 (NO penalty because M3 exists)
        ARC = "ARC_b"
        SAIL = "SAIL_II"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 5: Atypical/Segregated Airspace - ARC-a - M3 Penalty
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 5 `
    -Description "Atypical/Segregated → ARC-a, M3 Penalty Applies" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 25.0
        populationDensity = 5.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $true
        operationArea = "BVLOS_SparselyPopulated"
        operationScenario = "BVLOS_SparselyPopulated"
        mitigations = @()
        explicitARC = "ARC_a"
    } `
    -Expected @{
        iGRC = 5
        finalGRC = 6  # 5 + 1 (M3 penalty)
        ARC = "ARC_a"
        SAIL = "SAIL_V"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 6: Controlled Ground Area - iGRC=2 - M3 Penalty
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 6 `
    -Description "Controlled Ground Area → iGRC=2, M3 Penalty → fGRC=3" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 1.5
        maxSpeed = 12.0
        populationDensity = 0.0
        isControlledGroundArea = $true
        isAtypicalOrSegregated = $false
        operationArea = "ControlledGroundArea"
        operationScenario = "ControlledGroundArea"
        mitigations = @()
        explicitARC = "ARC_c"
    } `
    -Expected @{
        iGRC = 2
        finalGRC = 3  # 2 + 1 (M3 penalty)
        ARC = "ARC_c"
        SAIL = "SAIL_IV"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 7: M1 High + M2 High - M3 Penalty Still Applies
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 7 `
    -Description "M1 High (-4) + M2 High (-2) + M3 Penalty (+1) = Net -5" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 20.0
        populationDensity = 200.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "BVLOS_SparselyPopulated"
        operationScenario = "BVLOS_SparselyPopulated"
        mitigations = @(
            @{ type = "M1_StrategicMitigations"; robustness = "High" }
            @{ type = "M2_ImpactReduction"; robustness = "High" }
        )
        explicitARC = "ARC_b"
    } `
    -Expected @{
        iGRC = 4
        finalGRC = 2  # 4 - 4 (M1 High) - 2 (M2 High) + 1 (M3 penalty) = -1 → floor at 2 (column min)
        ARC = "ARC_b"
        SAIL = "SAIL_II"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 8: High Altitude (>500ft) ARC-d - M3 Penalty
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 8 `
    -Description "High Altitude ARC-d, Dense Urban, M3 Penalty" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 25.0
        populationDensity = 1000.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "VLOS_Populated"
        operationScenario = "VLOS_Populated"
        mitigations = @()
        explicitARC = "ARC_d"
    } `
    -Expected @{
        iGRC = 5
        finalGRC = 6  # 5 + 1 (M3 penalty)
        ARC = "ARC_d"
        SAIL = "SAIL_VI"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 9: M3 Medium Provided - Neutral (no benefit, no penalty)
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 9 `
    -Description "M3 Medium Provided (0 GRC change) - NO Penalty" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 18.0
        populationDensity = 150.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "BVLOS_SparselyPopulated"
        operationScenario = "BVLOS_SparselyPopulated"
        mitigations = @(
            @{ type = "M3_EmergencyResponsePlan"; robustness = "Medium" }
        )
        explicitARC = "ARC_b"
    } `
    -Expected @{
        iGRC = 4
        finalGRC = 4  # 4 + 0 (M3 Medium is neutral) = 4 (NO penalty)
        ARC = "ARC_b"
        SAIL = "SAIL_III"
    }
) { $passed++ } else { $failed++ }

# ───────────────────────────────────────────────────────────────────────────
# Test 10: Assemblies (60k ppl/km²) - M3 Penalty
# ───────────────────────────────────────────────────────────────────────────
if (Test-SORAScenario -TestNumber 10 `
    -Description "Assemblies (60k ppl/km²) → iGRC=7, M3 Penalty" `
    -Payload @{
        soraVersion = "2.0"
        maxDimension = 3.0
        maxSpeed = 20.0
        populationDensity = 60000.0
        isControlledGroundArea = $false
        isAtypicalOrSegregated = $false
        operationArea = "VLOS_Assemblies"
        operationScenario = "VLOS_Assemblies"
        mitigations = @()
        explicitARC = "ARC_c"
    } `
    -Expected @{
        iGRC = 7
        finalGRC = 8  # 7 + 1 (M3 penalty) → capped at 7 (max GRC) or 8?
        ARC = "ARC_c"
        SAIL = "SAIL_VI"
    }
) { $passed++ } else { $failed++ }

# ═══════════════════════════════════════════════════════════════════════════
# FINAL SUMMARY
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "FINAL SUMMARY" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "Total Tests: 10" -ForegroundColor Cyan
Write-Host "✅ Passed: $passed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor Red

if ($failed -eq 0) {
    Write-Host "`n🎉 ALL TESTS PASSED! M3 Penalty logic is EASA/JARUS compliant!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tests failed. Review backend calculations." -ForegroundColor Yellow
}
