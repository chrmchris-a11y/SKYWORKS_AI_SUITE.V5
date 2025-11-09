// ================================================================
// GIS Integration Tests (Phase 6 - Step 51.4)
// ================================================================
// Lightweight tests for SORA parameter extraction logic
// Tests the mapping: Map Geometry → SORA Inputs → Calculations
// ================================================================

const assert = require('assert');
const fs = require('fs');

// Load SORA calculator only (pure functions, no DOM)
const soraCalcCode = fs.readFileSync('c:\\Users\\chrmc\\Desktop\\SKYWORKS_AI_SUITE.V5\\WebPlatform\\wwwroot\\app\\Pages\\ui\\assets\\sora-calculator.js', 'utf8');
eval(soraCalcCode);

console.log('\n=== GIS Integration Tests (Phase 6) ===\n');

let testsRun = 0;
let testsPassed = 0;

function test(name, fn) {
  testsRun++;
  try {
    fn();
    testsPassed++;
    console.log(`✅ ${name}`);
  } catch (err) {
    console.error(`❌ ${name}`);
    console.error(`   ${err.message}`);
  }
}

// ================================================================
// PARAMETER EXTRACTION LOGIC (Mirroring airspace.js extractSORAParams)
// ================================================================
function extractSORAParamsTest(mission, layers) {
  const params = {
    populationDensity: 2,
    controlledGroundArea: false,
    altitude_ft: 0,
    controlledAirspace: false,
    airportEnvironment: 'none',
    populatedArea: true,
    atypicalAirspace: false,
    M1: 'none', M2: 'none', M3: 'none',
    tmpr_low: false, tmpr_medium: false, tmpr_high: false,
    tactical_low: false, tactical_medium: false, tactical_high: false,
    strategic_low: false, strategic_medium: false, strategic_high: false
  };

  // 1. Altitude
  if (mission.waypoints.length > 0) {
    const maxAlt_m = Math.max(...mission.waypoints.map(wp => wp.alt_m || 0));
    params.altitude_ft = Math.round(maxAlt_m * 3.28084);
  }

  // 2. Population (BEFORE CGA, so CGA can override)
  if (layers.population) {
    params.populatedArea = true;
    params.populationDensity = 3;
  } else {
    params.populatedArea = false;
    params.populationDensity = 1;
  }

  // 3. Controlled Airspace
  if (layers.rmz || layers.tmz) {
    params.controlledAirspace = 'RMZ_TMZ';
  } else if (layers.ctr) {
    params.controlledAirspace = 'CTR';
  }

  // 4. Airport
  if (layers.atz) {
    params.airportEnvironment = 'Class_E_F_G';
  }

  // 5. Atypical
  if (layers.tsa || layers.tra) {
    params.atypicalAirspace = true;
  }

  // 6. CGA (HIGHEST PRIORITY - overrides population)
  if (mission.cga) {
    params.controlledGroundArea = true;
    params.populationDensity = 1;
  }

  return params;
}

// ================================================================
// TESTS
// ================================================================

// Test 1: Altitude Conversion
test('GIS-1: Altitude conversion (100m = 328ft)', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.altitude_ft, 328);
});

// Test 2: CGA Detection
test('GIS-2: CGA polygon sets controlledGroundArea=true', () => {
  const mission = {
    waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 50 }],
    cga: { type: 'Feature', geometry: { type: 'Polygon', coordinates: [] } }
  };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.controlledGroundArea, true);
  assert.strictEqual(params.populationDensity, 1);
});

// Test 3: RMZ/TMZ
test('GIS-3: RMZ layer sets controlledAirspace="RMZ_TMZ"', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 200 }], cga: null };
  const layers = { rmz: true, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.controlledAirspace, 'RMZ_TMZ');
});

// Test 4: Population Layer
test('GIS-4: Population layer sets populationDensity=3 (High)', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 50 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.populationDensity, 3);
  assert.strictEqual(params.populatedArea, true);
});

// Test 5: ATZ Layer
test('GIS-5: ATZ layer sets airportEnvironment="Class_E_F_G"', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 300 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: true, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.airportEnvironment, 'Class_E_F_G');
});

// Test 6: TSA/TRA
test('GIS-6: TSA layer sets atypicalAirspace=true', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 150 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: true, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.atypicalAirspace, true);
});

// Test 7: Berlin Urban < 500ft → AEC 9
test('GIS-7: Berlin 100m AGL + Population layer → AEC 9', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  console.log('  DEBUG: params =', JSON.stringify(params, null, 2));
  
  const aec = calculateAEC(
    params.altitude_ft,
    params.controlledAirspace,
    params.airportEnvironment,
    params.populatedArea,
    params.atypicalAirspace
  );
  
  console.log(`  DEBUG: AEC calculated = ${aec}`);
  console.log(`  DEBUG: altitude_ft=${params.altitude_ft}, controlled=${params.controlledAirspace}, airport=${params.airportEnvironment}, populated=${params.populatedArea}`);
  
  // NOTE: Custom 0-11 numbering (not official 1-12)
  // AEC 9 = "< 500ft AGL Uncontrolled, Urban"
  assert.strictEqual(aec, 9, 'Expected AEC 9: <500ft AGL, Uncontrolled, Urban');
});

// Test 8: Rural > 500ft → AEC 5
test('GIS-8: Rural 200m AGL (no population layer) → AEC 5', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 200 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  const aec = calculateAEC(
    params.altitude_ft,
    params.controlledAirspace,
    params.airportEnvironment,
    params.populatedArea,
    params.atypicalAirspace
  );
  
  assert.strictEqual(aec, 5, 'Expected AEC 5: >500ft AGL, Uncontrolled, Rural');
});

// Test 9: Full SORA 2.5 Calculation (Berlin Urban 100m)
test('GIS-9: Full SORA 2.5 calculation (Berlin 100m urban)', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  const results = calculateSORA25(params);
  
  assert.strictEqual(results.initialARC, 'ARC-c', 'AEC 9 → ARC-c');
  assert.strictEqual(results.initialGRC, 3, 'Urban density 3 → iGRC 3');
  assert.strictEqual(results.finalGRC, 3, 'No mitigations → Final GRC = iGRC');
  assert.strictEqual(results.SAIL, 'III', 'GRC 3 + ARC-c → SAIL III');
});

// Test 10: CGA Override (Reduces Density)
test('GIS-10: CGA overrides population layer (density 3 → 1)', () => {
  const mission = {
    waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }],
    cga: { type: 'Feature', geometry: { type: 'Polygon', coordinates: [] } }
  };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  // CGA should override population layer density
  assert.strictEqual(params.controlledGroundArea, true);
  assert.strictEqual(params.populationDensity, 1, 'CGA overrides population layer');
  
  const results = calculateSORA25(params);
  assert.strictEqual(results.initialGRC, 1, 'Controlled area → iGRC 1');
});

console.log(`\n=== GIS Integration Test Results ===`);
console.log(`Total: ${testsRun} | Passed: ${testsPassed} | Failed: ${testsRun - testsPassed}`);

if (testsPassed === testsRun) {
  console.log('✅ All GIS integration tests PASSED');
  process.exit(0);
} else {
  console.error(`❌ ${testsRun - testsPassed} test(s) FAILED`);
  process.exit(1);
}
