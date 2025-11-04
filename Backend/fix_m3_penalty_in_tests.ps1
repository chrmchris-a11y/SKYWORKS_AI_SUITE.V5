# Fix M3 Penalty in SORA 2.0 Test Expected Values
# Per SORA 2.0 AMC1 Article 11(1.c): No M3 = +1 penalty to final GRC

$testFile = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\tests\Skyworks.Api.Tests\TestData\SORAAuthoritative_TestCases.json"

Write-Host "Reading test file..." -ForegroundColor Cyan
$json = Get-Content $testFile -Raw | ConvertFrom-Json

$updatedCount = 0
foreach ($test in $json.testCases) {
    # Only update SORA 2.0 tests (not JARUS 2.5)
    if ($test.testId -like "SORA20_*") {
        # Extract current final GRC number (e.g., "GRC_3" → 3)
        if ($test.expected.finalGRC -match 'GRC_(\d+)') {
            $currentGRC = [int]$matches[1]
            $newGRC = $currentGRC + 1
            $oldValue = $test.expected.finalGRC
            $newValue = "GRC_$newGRC"
            
            Write-Host "  $($test.testId): $oldValue → $newValue (+1 M3 penalty)" -ForegroundColor Yellow
            $test.expected.finalGRC = $newValue
            $updatedCount++
            
            # Update SAIL if needed (based on JARUS Table 7)
            # GRC_3→GRC_4 or GRC_4→GRC_5 may change SAIL
            $arc = $test.expected.residualARC
            
            # SAIL Matrix (simplified - full matrix in backend code)
            $sailMap = @{
                "GRC_2_ARC_a" = "SAIL_I"
                "GRC_2_ARC_b" = "SAIL_II"
                "GRC_2_ARC_c" = "SAIL_III"
                "GRC_2_ARC_d" = "SAIL_IV"
                "GRC_3_ARC_a" = "SAIL_I"
                "GRC_3_ARC_b" = "SAIL_II"
                "GRC_3_ARC_c" = "SAIL_III"
                "GRC_3_ARC_d" = "SAIL_IV"
                "GRC_4_ARC_a" = "SAIL_II"
                "GRC_4_ARC_b" = "SAIL_III"
                "GRC_4_ARC_c" = "SAIL_IV"
                "GRC_4_ARC_d" = "SAIL_V"
                "GRC_5_ARC_a" = "SAIL_III"
                "GRC_5_ARC_b" = "SAIL_IV"
                "GRC_5_ARC_c" = "SAIL_IV"
                "GRC_5_ARC_d" = "SAIL_V"
                "GRC_6_ARC_a" = "SAIL_V"
                "GRC_6_ARC_b" = "SAIL_V"
                "GRC_6_ARC_c" = "SAIL_V"
                "GRC_6_ARC_d" = "SAIL_VI"
                "GRC_7_ARC_a" = "SAIL_VI"
            }
            
            $sailKey = "${newValue}_${arc}"
            if ($sailMap.ContainsKey($sailKey)) {
                $oldSail = $test.expected.sail
                $newSail = $sailMap[$sailKey]
                if ($oldSail -ne $newSail) {
                    Write-Host "    SAIL updated: $oldSail → $newSail" -ForegroundColor Magenta
                    $test.expected.sail = $newSail
                }
            }
        }
    }
}

Write-Host "`nUpdating JSON file..." -ForegroundColor Cyan
$json | ConvertTo-Json -Depth 100 | Set-Content $testFile -Encoding UTF8

Write-Host "`n✅ Updated $updatedCount SORA 2.0 tests with M3 penalty (+1 final GRC)" -ForegroundColor Green
Write-Host "File: $testFile" -ForegroundColor Gray
