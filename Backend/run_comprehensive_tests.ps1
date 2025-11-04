# COMPREHENSIVE SORA ACCURACY TESTS
# 10 SORA 2.0 + 10 JARUS 2.5 scenarios with in-depth analysis

param(
    [string]$BaseUrl = "http://localhost:5210"
)

$ErrorActionPreference = "Continue"

# Test if API is running (TCP port check)
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect("localhost", 5210)
    $tcpClient.Close()
    Write-Host "✅ Backend API is running at $BaseUrl" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR: Backend API not running at $BaseUrl" -ForegroundColor Red
    Write-Host "Please start the backend first:" -ForegroundColor Yellow
    Write-Host "  cd Backend\src\Skyworks.Api" -ForegroundColor Yellow
    Write-Host "  dotnet run" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  COMPREHENSIVE SORA ACCURACY TEST SUITE" -ForegroundColor Cyan
Write-Host "  20 In-Depth Scenarios (10 SORA 2.0 + 10 JARUS 2.5)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$allResults = @()
$testNumber = 0

function Test-SORAScenario {
    param(
        [string]$Version,
        [string]$Description,
        [hashtable]$Payload,
        [hashtable]$Expected
    )
    
    $script:testNumber++
    
    Write-Host "─────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "TEST #$testNumber [$Version]: $Description" -ForegroundColor Yellow
    Write-Host "─────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    
    $json = $Payload | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/sora/complete" -Method Post -Body $json -ContentType "application/json"
        
        Write-Host "  📊 RESULTS:" -ForegroundColor Cyan
        Write-Host "    iGRC/Initial GRC: $($response.intrinsicGRC)" -ForegroundColor White
        Write-Host "    Final GRC: $($response.finalGRC)" -ForegroundColor White
        Write-Host "    Initial ARC: $($response.initialARC)" -ForegroundColor White
        Write-Host "    Residual ARC: $($response.residualARC)" -ForegroundColor White
        Write-Host "    SAIL: $($response.sail)" -ForegroundColor White
        Write-Host "    TMPR: $($response.tmpr?.level ?? 'N/A')" -ForegroundColor White
        Write-Host "    Risk Score: $($response.riskScore)" -ForegroundColor White
        
        # Validation
        $passed = $true
        $validationMessages = @()
        
        if ($Expected.iGRC -and $response.intrinsicGRC -ne $Expected.iGRC) {
            $passed = $false
            $validationMessages += "❌ iGRC mismatch: Expected $($Expected.iGRC), Got $($response.intrinsicGRC)"
        } else {
            $validationMessages += "✅ iGRC correct: $($response.intrinsicGRC)"
        }
        
        if ($Expected.finalGRC -and $response.finalGRC -ne $Expected.finalGRC) {
            $passed = $false
            $validationMessages += "❌ Final GRC mismatch: Expected $($Expected.finalGRC), Got $($response.finalGRC)"
        } else {
            $validationMessages += "✅ Final GRC correct: $($response.finalGRC)"
        }
        
        if ($Expected.ARC -and $response.residualARC -notmatch $Expected.ARC) {
            $passed = $false
            $validationMessages += "❌ ARC mismatch: Expected $($Expected.ARC), Got $($response.residualARC)"
        } else {
            $validationMessages += "✅ ARC correct: $($response.residualARC)"
        }
        
        if ($Expected.SAIL -and $response.sail -ne $Expected.SAIL) {
            $passed = $false
            $validationMessages += "❌ SAIL mismatch: Expected $($Expected.SAIL), Got $($response.sail)"
        } else {
            $validationMessages += "✅ SAIL correct: $($response.sail)"
        }
        
        Write-Host ""
        Write-Host "  🔍 VALIDATION:" -ForegroundColor Cyan
        foreach ($msg in $validationMessages) {
            if ($msg -like "❌*") {
                Write-Host "    $msg" -ForegroundColor Red
            } else {
                Write-Host "    $msg" -ForegroundColor Green
            }
        }
        
        if ($response.warnings) {
            Write-Host ""
            Write-Host "  ⚠️ WARNINGS:" -ForegroundColor Yellow
            foreach ($w in $response.warnings) {
                Write-Host "    $w" -ForegroundColor Yellow
            }
        }
        
        Write-Host ""
        if ($passed) {
            Write-Host "  ✅ TEST PASSED" -ForegroundColor Green
        } else {
            Write-Host "  ❌ TEST FAILED" -ForegroundColor Red
        }
        
        $script:allResults += @{
            Test = $testNumber
            Version = $Version
            Description = $Description
            Status = if ($passed) { "PASS" } else { "FAIL" }
            Result = $response
        }
        
    } catch {
        Write-Host "  ❌ API ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $script:allResults += @{
            Test = $testNumber
            Version = $Version
            Description = $Description
            Status = "ERROR"
            Error = $_.Exception.Message
        }
    }
    
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════
# SORA 2.0 TESTS (10 scenarios)
# ═══════════════════════════════════════════════════════════

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          SORA 2.0 TESTS (JAR_doc_06)                     ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Test 1: Minimal scenario (small drone, sparse, no mitigations)
Test-SORAScenario -Version "SORA 2.0" -Description "Small drone (<1m), VLOS sparse, no mitigations" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_SparselyPopulated"
        maxCharacteristicDimension = 0.5
        kineticEnergy = 300
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Typical"
        maxHeightAGL = 50
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 2
    finalGRC = 2
    ARC = "ARC-b"
    SAIL = "I"
}

# Test 2: BVLOS populated with full mitigations
Test-SORAScenario -Version "SORA 2.0" -Description "Medium drone, BVLOS populated, M1 Med + M2 High + M3 High" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "BVLOS_Populated"
        maxCharacteristicDimension = 3.0
        kineticEnergy = 25000
        mitigations = @(
            @{ type = "M1"; robustness = "Medium" }
            @{ type = "M2"; robustness = "High" }
            @{ type = "M3"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 120
        strategicMitigations = @("SM1", "SM2")
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 6
    finalGRC = 1  # 6 - 2 (M1 Med) - 2 (M2 High) - 1 (M3 High) = 1
    ARC = "ARC-d"
    SAIL = "II"
}

# Test 3: Assembly of people (high risk)
Test-SORAScenario -Version "SORA 2.0" -Description "Large drone (8m), Assembly of People, M1 High only" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_AssemblyOfPeople"
        maxCharacteristicDimension = 7.5
        kineticEnergy = 800000
        mitigations = @(
            @{ type = "M1"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 30
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 7
    finalGRC = 3  # 7 - 4 (M1 High) = 3
    ARC = "ARC-c"
    SAIL = "III"
}

# Test 4: M3 penalty test (no ERP = +1)
Test-SORAScenario -Version "SORA 2.0" -Description "Testing M3 penalty: M1 Low only (no M3 = +1 penalty)" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_Populated"
        maxCharacteristicDimension = 2.0
        kineticEnergy = 15000
        mitigations = @(
            @{ type = "M1"; robustness = "Low" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 80
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 5
    finalGRC = 5  # 5 - 1 (M1 Low) + 1 (M3 penalty) = 5 (penalty cancels M1)
    ARC = "ARC-b"
    SAIL = "II"
}

# Test 5: Atypical operation (ARC-a)
Test-SORAScenario -Version "SORA 2.0" -Description "Atypical operation → ARC-a (no TMPR)" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_SparselyPopulated"
        maxCharacteristicDimension = 1.5
        kineticEnergy = 5000
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Atypical"
        maxHeightAGL = 60
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 3
    finalGRC = 3
    ARC = "ARC-a"
    SAIL = "I"
}

# Test 6: BVLOS sparsely populated
Test-SORAScenario -Version "SORA 2.0" -Description "BVLOS sparse, medium drone, M2 only" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "BVLOS_SparselyPopulated"
        maxCharacteristicDimension = 2.5
        kineticEnergy = 18000
        mitigations = @(
            @{ type = "M2"; robustness = "Medium" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Typical"
        maxHeightAGL = 100
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 4
    finalGRC = 3  # 4 - 1 (M2 Med) = 3
    ARC = "ARC-b"
    SAIL = "II"
}

# Test 7: Controlled airspace (higher ARC)
Test-SORAScenario -Version "SORA 2.0" -Description "Controlled airspace + Airport → ARC-d" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_Populated"
        maxCharacteristicDimension = 1.8
        kineticEnergy = 9000
        mitigations = @(
            @{ type = "M1"; robustness = "Medium" }
            @{ type = "M3"; robustness = "Medium" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 80
        strategicMitigations = @("SM1")
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 5
    finalGRC = 3  # 5 - 2 (M1 Med) = 3
    ARC = "ARC-d"
    SAIL = "III"
}

# Test 8: BVLOS Assembly (worst case)
Test-SORAScenario -Version "SORA 2.0" -Description "BVLOS Assembly of People (extreme risk), all mitigations" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "BVLOS_AssemblyOfPeople"
        maxCharacteristicDimension = 5.0
        kineticEnergy = 500000
        mitigations = @(
            @{ type = "M1"; robustness = "High" }
            @{ type = "M2"; robustness = "High" }
            @{ type = "M3"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 150
        strategicMitigations = @("SM1", "SM2")
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 8
    finalGRC = 1  # 8 - 4 (M1 High) - 2 (M2 High) - 1 (M3 High) = 1
    ARC = "ARC-d"
    SAIL = "II"
}

# Test 9: No mitigations (baseline)
Test-SORAScenario -Version "SORA 2.0" -Description "Baseline: VLOS populated, no mitigations at all" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_Populated"
        maxCharacteristicDimension = 3.2
        kineticEnergy = 30000
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 90
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 6
    finalGRC = 7  # 6 + 1 (M3 penalty) = 7
    ARC = "ARC-b"
    SAIL = "III"
}

# Test 10: Edge case - segregated atypical
Test-SORAScenario -Version "SORA 2.0" -Description "Atypical + Segregated → ARC-a guaranteed" -Payload @{
    soraVersion = "2.0"
    groundRisk = @{
        scenario_V2_0 = "VLOS_SparselyPopulated"
        maxCharacteristicDimension = 2.0
        kineticEnergy = 12000
        mitigations = @(
            @{ type = "M1"; robustness = "Low" }
            @{ type = "M3"; robustness = "Medium" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "NonAirport"
        environment = "Industrial"
        typicality = "Atypical"
        maxHeightAGL = 70
        strategicMitigations = @()
        isAtypicalSegregated = $true
    }
} -Expected @{
    iGRC = 4
    finalGRC = 3  # 4 - 1 (M1 Low) = 3
    ARC = "ARC-a"
    SAIL = "I"
}

Write-Host ""
Write-Host "Press Enter to continue to JARUS 2.5 tests..." -ForegroundColor Yellow
Read-Host

# ═══════════════════════════════════════════════════════════
# JARUS 2.5 TESTS (10 scenarios)
# ═══════════════════════════════════════════════════════════

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          JARUS SORA 2.5 TESTS (JAR_doc_25)               ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Test 11: Tiny drone special rule (≤250g & ≤25m/s = iGRC 1)
Test-SORAScenario -Version "JARUS 2.5" -Description "≤250g drone, ≤25m/s → iGRC=1 (special rule)" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 50
        controlledGroundArea = $true
        maxCharacteristicDimension = 0.2
        maxSpeed = 20
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Rural"
        typicality = "Typical"
        maxHeightAGL = 30
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
    mtom = 0.24
} -Expected @{
    iGRC = 1
    finalGRC = 1
    ARC = "ARC-b"
    SAIL = "I"
}

# Test 12: Table 2 edge case (3m/35m/s, 500 people/km²)
Test-SORAScenario -Version "JARUS 2.5" -Description "3m/35m/s, <500 people/km² → iGRC=5 per Table 2" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 500
        controlledGroundArea = $false
        maxCharacteristicDimension = 3.0
        maxSpeed = 35
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 100
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 5
    finalGRC = 5
    ARC = "ARC-b"
    SAIL = "II"
}

# Test 13: All M1 mitigations (M1A + M1B + M1C)
Test-SORAScenario -Version "JARUS 2.5" -Description "M1(A) Med + M1(B) High + M1(C) Low = -5 total" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 5000
        controlledGroundArea = $false
        maxCharacteristicDimension = 5.0
        maxSpeed = 60
        mitigations = @(
            @{ type = "M1A"; robustness = "Medium" }
            @{ type = "M1B"; robustness = "High" }
            @{ type = "M1C"; robustness = "Low" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 150
        strategicMitigations = @("SM1")
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 7
    finalGRC = 2  # 7 - 2 (M1A Med) - 2 (M1B High) - 1 (M1C Low) = 2
    ARC = "ARC-d"
    SAIL = "II"
}

# Test 14: M2 High only (parachute)
Test-SORAScenario -Version "JARUS 2.5" -Description "M2 High (parachute) only = -2" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 2000
        controlledGroundArea = $false
        maxCharacteristicDimension = 4.0
        maxSpeed = 50
        mitigations = @(
            @{ type = "M2"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Suburban"
        typicality = "Typical"
        maxHeightAGL = 90
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 6
    finalGRC = 4  # 6 - 2 (M2 High) = 4
    ARC = "ARC-b"
    SAIL = "II"
}

# Test 15: Assemblies (>50k people/km²)
Test-SORAScenario -Version "JARUS 2.5" -Description "Assemblies (>50k people/km²) → iGRC=7" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 60000
        controlledGroundArea = $false
        maxCharacteristicDimension = 1.0
        maxSpeed = 25
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 40
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 7
    finalGRC = 7
    ARC = "ARC-c"
    SAIL = "III"
}

# Test 16: Large drone (20m/120m/s)
Test-SORAScenario -Version "JARUS 2.5" -Description "20m/120m/s, remote area → iGRC=5" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 5
        controlledGroundArea = $false
        maxCharacteristicDimension = 20.0
        maxSpeed = 120
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Remote"
        typicality = "Typical"
        maxHeightAGL = 200
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 5
    finalGRC = 5
    ARC = "ARC-b"
    SAIL = "II"
}

# Test 17: Atypical + Segregated = ARC-a
Test-SORAScenario -Version "JARUS 2.5" -Description "Atypical + Segregated (NOTAM) → ARC-a" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 100
        controlledGroundArea = $false
        maxCharacteristicDimension = 2.0
        maxSpeed = 30
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "NonAirport"
        environment = "Industrial"
        typicality = "Atypical"
        maxHeightAGL = 60
        strategicMitigations = @()
        isAtypicalSegregated = $true
    }
} -Expected @{
    iGRC = 3
    finalGRC = 3
    ARC = "ARC-a"
    SAIL = "I"
}

# Test 18: High density metropolitan
Test-SORAScenario -Version "JARUS 2.5" -Description "High density metro (45k people/km²), M1A Low + M2 Med" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 45000
        controlledGroundArea = $false
        maxCharacteristicDimension = 1.5
        maxSpeed = 28
        mitigations = @(
            @{ type = "M1A"; robustness = "Low" }
            @{ type = "M2"; robustness = "Medium" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 100
        strategicMitigations = @("SM1", "SM2")
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 6
    finalGRC = 4  # 6 - 1 (M1A Low) - 1 (M2 Med) = 4
    ARC = "ARC-d"
    SAIL = "IV"
}

# Test 19: Controlled ground area (row 0 Table 2)
Test-SORAScenario -Version "JARUS 2.5" -Description "Controlled ground area, 8m/75m/s → iGRC=2" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = "controlled"
        controlledGroundArea = $true
        maxCharacteristicDimension = 8.0
        maxSpeed = 75
        mitigations = @()
    }
    airRisk = @{
        airspaceControl = "Uncontrolled"
        locationType = "NonAirport"
        environment = "Industrial"
        typicality = "Typical"
        maxHeightAGL = 50
        strategicMitigations = @()
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 2
    finalGRC = 2
    ARC = "ARC-b"
    SAIL = "I"
}

# Test 20: Complex scenario (all factors)
Test-SORAScenario -Version "JARUS 2.5" -Description "Complex: Urban, M1A+M1B+M2, controlled airspace" -Payload @{
    soraVersion = "2.5"
    groundRisk = @{
        populationDensity = 8000
        controlledGroundArea = $false
        maxCharacteristicDimension = 6.5
        maxSpeed = 70
        mitigations = @(
            @{ type = "M1A"; robustness = "Low" }
            @{ type = "M1B"; robustness = "Medium" }
            @{ type = "M2"; robustness = "High" }
        )
    }
    airRisk = @{
        airspaceControl = "Controlled"
        locationType = "Airport"
        environment = "Urban"
        typicality = "Typical"
        maxHeightAGL = 120
        strategicMitigations = @("SM1", "SM2")
        isAtypicalSegregated = $false
    }
} -Expected @{
    iGRC = 7
    finalGRC = 3  # 7 - 1 (M1A Low) - 1 (M1B Med) - 2 (M2 High) = 3
    ARC = "ARC-d"
    SAIL = "III"
}

# ═══════════════════════════════════════════════════════════
# FINAL SUMMARY
# ═══════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                    FINAL SUMMARY                          ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

$passed = ($allResults | Where-Object { $_.Status -eq "PASS" }).Count
$failed = ($allResults | Where-Object { $_.Status -eq "FAIL" }).Count
$errors = ($allResults | Where-Object { $_.Status -eq "ERROR" }).Count

Write-Host "Total Tests: $($allResults.Count)" -ForegroundColor White
Write-Host "✅ Passed: $passed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor Red
Write-Host "⚠️ Errors: $errors" -ForegroundColor Yellow
Write-Host ""

if ($failed -gt 0 -or $errors -gt 0) {
    Write-Host "FAILED/ERROR TESTS:" -ForegroundColor Red
    $allResults | Where-Object { $_.Status -ne "PASS" } | ForEach-Object {
        Write-Host "  Test $($_.Test): $($_.Description) - $($_.Status)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Test suite complete. Results saved to memory." -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
