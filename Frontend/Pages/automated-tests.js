/**
 * SKYWORKS SORA Automated Test Runner
 * 
 * USAGE:
 * 1. Open Mission Planner: http://localhost:5210/app/Pages/mission.html
 * 2. Open Browser Console (F12)
 * 3. Copy/paste this entire file
 * 4. Run: await runAllTests()
 * 
 * Results will be displayed in console and on-page
 */

const TEST_SCENARIOS = [
  // ============================================
  // SORA 2.0 TESTS (10 scenarios)
  // ============================================
  {
    name: "SORA 2.0 - Test 1: DJI Mini 4 Pro - VLOS Populated - Low Mitigations",
    soraVersion: "2.0",
    droneId: "DJI_MINI4PRO",
    groundRisk: {
      MTOM_kg: 0.249,
      Scenario_V2_0: "VLOS_Populated",
      Mitigations: [
        { Type: "M1", Robustness: "Low" },
        { Type: "M2", Robustness: "Low" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: []
    },
    expectedResults: {
      intrinsicGRC: 3,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.0 - Test 2: DJI Mavic 3 Enterprise - VLOS Populated - Medium Mitigations",
    soraVersion: "2.0",
    droneId: "DJI_MAVIC3_ENTERPRISE",
    groundRisk: {
      MTOM_kg: 0.895,
      Scenario_V2_0: "VLOS_Populated",
      Mitigations: [
        { Type: "M1", Robustness: "Medium" },
        { Type: "M2", Robustness: "Medium" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: ["TM1"]
    },
    expectedResults: {
      intrinsicGRC: 4,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.0 - Test 3: Sky Tech SC05 - BVLOS Sparsely - High Mitigations",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC05",
    groundRisk: {
      MTOM_kg: 3.5,
      Scenario_V2_0: "BVLOS_SparselyPopulated",
      Mitigations: [
        { Type: "M1", Robustness: "High" },
        { Type: "M2", Robustness: "High" },
        { Type: "M3", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2"]
    },
    expectedResults: {
      intrinsicGRC: 4,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.0 - Test 4: Sky Tech SC15 - BVLOS Populated - High Mitigations",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC15",
    groundRisk: {
      MTOM_kg: 32,
      Scenario_V2_0: "BVLOS_Populated",
      Mitigations: [
        { Type: "M1", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3"]
    },
    expectedResults: {
      intrinsicGRC: 6,
      minFinalGRC: 2,
      minSAIL: "II"
    }
  },
  {
    name: "SORA 2.0 - Test 5: Sky Tech SC25 - Controlled Ground - High Mitigations",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC25",
    groundRisk: {
      MTOM_kg: 50,
      Scenario_V2_0: "ControlledGroundArea",
      Mitigations: [
        { Type: "M1", Robustness: "High" },
        { Type: "M2", Robustness: "High" },
        { Type: "M3", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3", "TM4"]
    },
    expectedResults: {
      intrinsicGRC: 2,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.0 - Test 6: Sky Tech SC35 - BVLOS Populated - High Mitigations",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC35",
    groundRisk: {
      MTOM_kg: 120,
      Scenario_V2_0: "BVLOS_Populated",
      Mitigations: [
        { Type: "M1", Robustness: "High" },
        { Type: "M2", Robustness: "High" },
        { Type: "M3", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3", "TM4"]
    },
    expectedResults: {
      intrinsicGRC: 6,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.0 - Test 7: DJI Inspire 3 - VLOS Gathering - Medium Mitigations",
    soraVersion: "2.0",
    droneId: "DJI_INSPIRE3",
    groundRisk: {
      MTOM_kg: 4.0,
      Scenario_V2_0: "VLOS_GatheringOfPeople",
      Mitigations: [
        { Type: "M1", Robustness: "Medium" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: ["TM1"]
    },
    expectedResults: {
      intrinsicGRC: 7,
      minFinalGRC: 3,
      minSAIL: "III"
    }
  },
  {
    name: "SORA 2.0 - Test 8: Autel EVO Max 4T - VLOS Populated - Low Mitigations",
    soraVersion: "2.0",
    droneId: "AUTEL_EVO_MAX_4T",
    groundRisk: {
      MTOM_kg: 1.6,
      Scenario_V2_0: "VLOS_Populated",
      Mitigations: [
        { Type: "M1", Robustness: "Low" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: []
    },
    expectedResults: {
      intrinsicGRC: 4,
      minFinalGRC: 2,
      minSAIL: "II"
    }
  },
  {
    name: "SORA 2.0 - Test 9: Parrot Anafi USA - BVLOS Sparsely - Medium Mitigations",
    soraVersion: "2.0",
    droneId: "PARROT_ANAFI_USA",
    groundRisk: {
      MTOM_kg: 0.5,
      Scenario_V2_0: "BVLOS_SparselyPopulated",
      Mitigations: [
        { Type: "M1", Robustness: "Medium" },
        { Type: "M2", Robustness: "Medium" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: ["TM1"]
    },
    expectedResults: {
      intrinsicGRC: 3,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.0 - Test 10: Freefly Alta X - BVLOS Populated - High Mitigations",
    soraVersion: "2.0",
    droneId: "FREEFLY_ALTA_X",
    groundRisk: {
      MTOM_kg: 10.5,
      Scenario_V2_0: "BVLOS_Populated",
      Mitigations: [
        { Type: "M1", Robustness: "High" },
        { Type: "M2", Robustness: "High" },
        { Type: "M3", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3"]
    },
    expectedResults: {
      intrinsicGRC: 6,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },

  // ============================================
  // SORA 2.5 TESTS (10 scenarios)
  // ============================================
  {
    name: "SORA 2.5 - Test 1: DJI Mini 4 Pro - Low Speed/Dimensions",
    soraVersion: "2.5",
    droneId: "DJI_MINI4PRO",
    groundRisk: {
      CharacteristicDimension_m: 0.25,
      MaxSpeed_ms: 15,
      PopulationDensity: 1000,
      Mitigations: [
        { Type: "M1A", Robustness: "Medium" },
        { Type: "M1B", Robustness: "Medium" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: []
    },
    expectedResults: {
      intrinsicGRC: 1,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 2: DJI Mavic 3 Enterprise - Medium Dimensions",
    soraVersion: "2.5",
    droneId: "DJI_MAVIC3_ENTERPRISE",
    groundRisk: {
      CharacteristicDimension_m: 0.45,
      MaxSpeed_ms: 20,
      PopulationDensity: 5000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M2", Robustness: "Medium" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: ["TM1"]
    },
    expectedResults: {
      intrinsicGRC: 2,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 3: Sky Tech SC05 - Medium Dimensions - High Mitigations",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC05",
    groundRisk: {
      CharacteristicDimension_m: 0.8,
      MaxSpeed_ms: 25,
      PopulationDensity: 10000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M1C", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2"]
    },
    expectedResults: {
      intrinsicGRC: 3,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 4: Sky Tech SC15 - Large Dimensions - High Mitigations",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC15",
    groundRisk: {
      CharacteristicDimension_m: 1.5,
      MaxSpeed_ms: 30,
      PopulationDensity: 15000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M1C", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3"]
    },
    expectedResults: {
      intrinsicGRC: 4,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 5: Sky Tech SC25 - Large Dimensions - High Speed",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC25",
    groundRisk: {
      CharacteristicDimension_m: 2.0,
      MaxSpeed_ms: 35,
      PopulationDensity: 20000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M1C", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3", "TM4"]
    },
    expectedResults: {
      intrinsicGRC: 5,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 6: Sky Tech SC35 - Very Large Dimensions",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC35",
    groundRisk: {
      CharacteristicDimension_m: 3.0,
      MaxSpeed_ms: 40,
      PopulationDensity: 25000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M1C", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3", "TM4", "TM5"]
    },
    expectedResults: {
      intrinsicGRC: 6,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 7: DJI Inspire 3 - Medium Dimensions - Medium Mitigations",
    soraVersion: "2.5",
    droneId: "DJI_INSPIRE3",
    groundRisk: {
      CharacteristicDimension_m: 0.6,
      MaxSpeed_ms: 22,
      PopulationDensity: 8000,
      Mitigations: [
        { Type: "M1A", Robustness: "Medium" },
        { Type: "M1B", Robustness: "Medium" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: ["TM1"]
    },
    expectedResults: {
      intrinsicGRC: 3,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 8: Autel EVO Max 4T - Medium Dimensions - High Mitigations",
    soraVersion: "2.5",
    droneId: "AUTEL_EVO_MAX_4T",
    groundRisk: {
      CharacteristicDimension_m: 0.5,
      MaxSpeed_ms: 18,
      PopulationDensity: 6000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2"]
    },
    expectedResults: {
      intrinsicGRC: 2,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 9: Parrot Anafi USA - Small Dimensions - Medium Mitigations",
    soraVersion: "2.5",
    droneId: "PARROT_ANAFI_USA",
    groundRisk: {
      CharacteristicDimension_m: 0.3,
      MaxSpeed_ms: 16,
      PopulationDensity: 3000,
      Mitigations: [
        { Type: "M1A", Robustness: "Medium" },
        { Type: "M1B", Robustness: "Medium" }
      ]
    },
    airRisk: {
      AirspaceType: "Uncontrolled",
      AdjacentAirspace: false,
      AtcServiceAvailable: false,
      TechnicalMitigations: []
    },
    expectedResults: {
      intrinsicGRC: 1,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  },
  {
    name: "SORA 2.5 - Test 10: Freefly Alta X - Large Dimensions - High Mitigations",
    soraVersion: "2.5",
    droneId: "FREEFLY_ALTA_X",
    groundRisk: {
      CharacteristicDimension_m: 1.2,
      MaxSpeed_ms: 28,
      PopulationDensity: 12000,
      Mitigations: [
        { Type: "M1A", Robustness: "High" },
        { Type: "M1B", Robustness: "High" },
        { Type: "M1C", Robustness: "High" },
        { Type: "M2", Robustness: "High" }
      ]
    },
    airRisk: {
      AirspaceType: "Controlled",
      AdjacentAirspace: true,
      AtcServiceAvailable: true,
      TechnicalMitigations: ["TM1", "TM2", "TM3"]
    },
    expectedResults: {
      intrinsicGRC: 4,
      minFinalGRC: 1,
      minSAIL: "I"
    }
  }
];

/**
 * Execute a single SORA assessment test
 */
async function runSingleTest(testConfig, testIndex) {
  console.group(`🧪 Test ${testIndex + 1}/${TEST_SCENARIOS.length}: ${testConfig.name}`);
  
  try {
    const baseUrl = window.location.origin;
    const apiUrl = `${baseUrl}/api/sora/complete`;
    
    const requestBody = {
      SoraVersion: testConfig.soraVersion,
      DroneId: testConfig.droneId,
      GroundRisk: testConfig.groundRisk,
      AirRisk: testConfig.airRisk
    };
    
    console.log('📤 Request:', JSON.stringify(requestBody, null, 2));
    
    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(requestBody)
    });
    
    if (!response.ok) {
      const errorText = await response.text();
      console.error('❌ HTTP Error:', response.status, errorText);
      console.groupEnd();
      return {
        testName: testConfig.name,
        passed: false,
        error: `HTTP ${response.status}: ${errorText}`,
        details: null
      };
    }
    
    const result = await response.json();
    console.log('📥 Response:', result);
    
    // Validate results
    const validations = {
      isSuccessful: result.isSuccessful === true,
      intrinsicGRC: result.intrinsicGRC === testConfig.expectedResults.intrinsicGRC,
      intrinsicGRC_NotZero: result.intrinsicGRC > 0,
      finalGRC_Valid: result.finalGRC >= testConfig.expectedResults.minFinalGRC,
      SAIL_Valid: result.sail !== null && result.sail !== undefined,
      OSOs_Present: result.requiredOSOCount > 0
    };
    
    const allPassed = Object.values(validations).every(v => v === true);
    
    if (allPassed) {
      console.log('✅ PASSED');
    } else {
      console.warn('⚠️ VALIDATION ISSUES:', validations);
    }
    
    console.log(`
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Results Summary:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✓ Success: ${result.isSuccessful}
  ✓ Intrinsic GRC: ${result.intrinsicGRC} (expected: ${testConfig.expectedResults.intrinsicGRC})
  ✓ Final GRC: ${result.finalGRC}
  ✓ Initial ARC: ${result.initialARC}
  ✓ Residual ARC: ${result.residualARC}
  ✓ SAIL: ${result.sail}
  ✓ TMPR: ${result.tmpr}
  ✓ Required OSOs: ${result.requiredOSOCount}
  ✓ Implemented OSOs: ${result.implementedOSOCount}
  ✓ Compliant: ${result.isCompliant}
  ✓ Risk Score: ${result.riskScore}
  ✓ Risk Band: ${result.riskBand}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    `);
    
    console.groupEnd();
    
    return {
      testName: testConfig.name,
      passed: allPassed,
      validations: validations,
      details: result
    };
    
  } catch (error) {
    console.error('❌ Exception:', error);
    console.groupEnd();
    return {
      testName: testConfig.name,
      passed: false,
      error: error.message,
      details: null
    };
  }
}

/**
 * Run all test scenarios
 */
async function runAllTests() {
  console.clear();
  console.log(`
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║        🚁 SKYWORKS SORA AUTOMATED TEST SUITE 🚁                   ║
║                                                                   ║
║  Total Tests: ${TEST_SCENARIOS.length} (10 SORA 2.0 + 10 SORA 2.5)                    ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
  `);
  
  const startTime = Date.now();
  const results = [];
  
  for (let i = 0; i < TEST_SCENARIOS.length; i++) {
    const testResult = await runSingleTest(TEST_SCENARIOS[i], i);
    results.push(testResult);
    
    // Small delay between tests to avoid overwhelming the server
    await new Promise(resolve => setTimeout(resolve, 500));
  }
  
  const endTime = Date.now();
  const duration = ((endTime - startTime) / 1000).toFixed(2);
  
  // Summary
  const passed = results.filter(r => r.passed).length;
  const failed = results.filter(r => !r.passed).length;
  
  console.log(`
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║                     📊 TEST SUMMARY 📊                            ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝

  Total Tests: ${TEST_SCENARIOS.length}
  ✅ Passed: ${passed}
  ❌ Failed: ${failed}
  ⏱️  Duration: ${duration}s
  
  Success Rate: ${((passed / TEST_SCENARIOS.length) * 100).toFixed(1)}%
  `);
  
  if (failed > 0) {
    console.group('❌ Failed Tests:');
    results.filter(r => !r.passed).forEach(r => {
      console.log(`  • ${r.testName}`);
      if (r.error) {
        console.log(`    Error: ${r.error}`);
      }
      if (r.validations) {
        const failedChecks = Object.entries(r.validations)
          .filter(([_, passed]) => !passed)
          .map(([check]) => check);
        console.log(`    Failed checks: ${failedChecks.join(', ')}`);
      }
    });
    console.groupEnd();
  }
  
  // Display results on page
  displayResultsOnPage(results, duration);
  
  return results;
}

/**
 * Display test results on the page
 */
function displayResultsOnPage(results, duration) {
  // Remove existing results panel if any
  const existingPanel = document.getElementById('testResultsPanel');
  if (existingPanel) {
    existingPanel.remove();
  }
  
  // Create results panel
  const panel = document.createElement('div');
  panel.id = 'testResultsPanel';
  panel.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    width: 400px;
    max-height: 80vh;
    overflow-y: auto;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 12px;
    box-shadow: 0 10px 40px rgba(0,0,0,0.3);
    padding: 20px;
    z-index: 10000;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  `;
  
  const passed = results.filter(r => r.passed).length;
  const failed = results.filter(r => !r.passed).length;
  const successRate = ((passed / results.length) * 100).toFixed(1);
  
  panel.innerHTML = `
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
      <h2 style="margin: 0; font-size: 18px;">🧪 Test Results</h2>
      <button onclick="document.getElementById('testResultsPanel').remove()" 
              style="background: rgba(255,255,255,0.2); border: none; color: white; 
                     padding: 5px 10px; border-radius: 5px; cursor: pointer; font-size: 16px;">
        ✖
      </button>
    </div>
    
    <div style="background: rgba(255,255,255,0.1); padding: 15px; border-radius: 8px; margin-bottom: 15px;">
      <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
        <span>Total Tests:</span>
        <strong>${results.length}</strong>
      </div>
      <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
        <span>✅ Passed:</span>
        <strong style="color: #4ade80;">${passed}</strong>
      </div>
      <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
        <span>❌ Failed:</span>
        <strong style="color: #f87171;">${failed}</strong>
      </div>
      <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
        <span>⏱️ Duration:</span>
        <strong>${duration}s</strong>
      </div>
      <div style="display: flex; justify-content: space-between;">
        <span>Success Rate:</span>
        <strong style="color: ${successRate >= 90 ? '#4ade80' : successRate >= 70 ? '#fbbf24' : '#f87171'};">
          ${successRate}%
        </strong>
      </div>
    </div>
    
    <div style="max-height: 400px; overflow-y: auto;">
      ${results.map((r, i) => `
        <div style="background: rgba(255,255,255,0.1); padding: 10px; border-radius: 6px; 
                    margin-bottom: 8px; border-left: 4px solid ${r.passed ? '#4ade80' : '#f87171'};">
          <div style="font-weight: bold; margin-bottom: 5px; font-size: 12px;">
            ${r.passed ? '✅' : '❌'} Test ${i + 1}
          </div>
          <div style="font-size: 11px; opacity: 0.9; line-height: 1.4;">
            ${r.testName.replace(/SORA \d\.\d - Test \d+: /, '')}
          </div>
          ${r.details ? `
            <div style="font-size: 10px; margin-top: 5px; opacity: 0.8;">
              iGRC: ${r.details.intrinsicGRC} → fGRC: ${r.details.finalGRC} | 
              SAIL: ${r.details.sail} | OSOs: ${r.details.requiredOSOCount}
            </div>
          ` : ''}
        </div>
      `).join('')}
    </div>
  `;
  
  document.body.appendChild(panel);
}

/**
 * Quick test - run first 3 tests only
 */
async function quickTest() {
  console.log('🚀 Running Quick Test (first 3 scenarios)...\n');
  const quickScenarios = TEST_SCENARIOS.slice(0, 3);
  
  for (let i = 0; i < quickScenarios.length; i++) {
    await runSingleTest(quickScenarios[i], i);
    await new Promise(resolve => setTimeout(resolve, 500));
  }
  
  console.log('✅ Quick Test Complete!');
}

// Export functions to window for easy access
window.runAllTests = runAllTests;
window.quickTest = quickTest;
window.TEST_SCENARIOS = TEST_SCENARIOS;

console.log(`
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║  ✅ Test Suite Loaded Successfully!                               ║
║                                                                   ║
║  Commands:                                                        ║
║  • await runAllTests()  - Run all 20 tests                        ║
║  • await quickTest()    - Run first 3 tests (quick validation)    ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
`);
