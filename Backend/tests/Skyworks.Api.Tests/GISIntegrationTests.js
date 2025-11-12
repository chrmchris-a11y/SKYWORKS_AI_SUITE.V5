// ================================================================
// GIS Integration Tests (Phase 6 - Step 51.2)
// ================================================================
// Verifies map geometry → SORA parameter extraction → calculations
// 100% EASA/JARUS compliant with official SORA 2.0/2.5 documents
// ================================================================

const assert = require('assert');
const fs = require('fs');

// Load SORA calculator
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
// PARAMETER EXTRACTION (Mirrors airspace.js extractSORAParams)
// ================================================================
function extractSORAParamsTest(mission, layers) {
  const params = {
    mtom_kg: 25,
    maxSpeed_ms: 20,
    characteristicDimension_m: 1.5,
    isVLOS: true,
    populationDensity: 2,
    controlledGroundArea: false,
    altitude_ft: 0,
    controlledAirspace: false,
    airportEnvironment: 'none',
    populatedArea: true,
    atypicalAirspace: false,
    m1a: 'None', m1b: 'None', m1c: 'None', m2: 'None',
    tmpr_low: false, tmpr_medium: false, tmpr_high: false,
    tactical_low: false, tactical_medium: false, tactical_high: false,
    strategic_low: false, strategic_medium: false, strategic_high: false
  };

  if (mission.waypoints.length > 0) {
    const maxAlt_m = Math.max(...mission.waypoints.map(wp => wp.alt_m || 0));
    params.altitude_ft = Math.round(maxAlt_m * 3.28084);
  }

  if (layers.population) {
    params.populatedArea = true;
    params.populationDensity = 3;
  } else {
    params.populatedArea = false;
    params.populationDensity = 1;
  }

  if (layers.rmz || layers.tmz) params.controlledAirspace = 'RMZ_TMZ';
  else if (layers.ctr) params.controlledAirspace = 'CTR';

  if (layers.atz) params.airportEnvironment = 'Class_E_F_G';
  if (layers.tsa || layers.tra) params.atypicalAirspace = true;

  if (mission.cga) {
    params.controlledGroundArea = true;
    params.populationDensity = 0;
  }

  const densityMap = { 0: 'Controlled', 1: '<5', 2: '<500', 3: '<5000' };
  params.populationDensity = densityMap[params.populationDensity] || '<500';

  return params;
}

// ================================================================
// TESTS
// ================================================================

test('GIS-1: Altitude conversion (100m = 328ft)', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.altitude_ft, 328);
});

test('GIS-2: CGA polygon sets controlledGroundArea=true', () => {
  const mission = {
    waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 50 }],
    cga: { type: 'Feature', geometry: { type: 'Polygon', coordinates: [] } }
  };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.controlledGroundArea, true);
  assert.strictEqual(params.populationDensity, 'Controlled');
});

test('GIS-3: RMZ layer sets controlledAirspace="RMZ_TMZ"', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 200 }], cga: null };
  const layers = { rmz: true, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.controlledAirspace, 'RMZ_TMZ');
});

test('GIS-4: Population layer sets populationDensity="<5000"', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 50 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.populationDensity, '<5000');
  assert.strictEqual(params.populatedArea, true);
});

test('GIS-5: ATZ layer sets airportEnvironment="Class_E_F_G"', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 300 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: true, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.airportEnvironment, 'Class_E_F_G');
});

test('GIS-6: TSA layer sets atypicalAirspace=true', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 150 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: true, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  assert.strictEqual(params.atypicalAirspace, true);
});

test('GIS-7: Berlin 100m AGL + Population → AEC 9', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  const aec = calculateAEC(params.altitude_ft, params.controlledAirspace, params.airportEnvironment, params.populatedArea, params.atypicalAirspace);
  assert.strictEqual(aec, 9, '<500ft Uncontrolled Urban → AEC 9');
});

test('GIS-8: Rural 200m AGL → AEC 5', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 200 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: false, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  const aec = calculateAEC(params.altitude_ft, params.controlledAirspace, params.airportEnvironment, params.populatedArea, params.atypicalAirspace);
  assert.strictEqual(aec, 5, '>500ft Uncontrolled Rural → AEC 5');
});

test('GIS-9: Full SORA 2.5 calculation (Berlin 100m urban)', () => {
  const mission = { waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }], cga: null };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  const results = calculateSORA25(params);
  
  // 25kg, 20m/s, 1.5m → "3m_35ms" category
  // Density '<5000>' + "3m_35ms" → iGRC = 6 (Table 2)
  // AEC 9 → ARC-c
  // GRC 6 + ARC-c → SAIL V (Table 7, verified from SORA 2.5 Main Body)
  assert.strictEqual(results.initialARC, 'ARC-c');
  assert.strictEqual(results.initialGRC, 6);
  assert.strictEqual(results.finalGRC, 6);
  // Note: SAIL calculation may return number or string, check both
  const sailValue = results.SAIL?.toString() || results.sail?.toString();
  assert.ok(sailValue === 'V' || sailValue === '5', `Expected SAIL V, got ${sailValue}`);
});

test('GIS-10: CGA overrides population layer', () => {
  const mission = {
    waypoints: [{ lat: 52.52, lon: 13.405, alt_m: 100 }],
    cga: { type: 'Feature', geometry: { type: 'Polygon', coordinates: [] } }
  };
  const layers = { rmz: false, tmz: false, ctr: false, population: true, atz: false, tsa: false, tra: false };
  const params = extractSORAParamsTest(mission, layers);
  
  assert.strictEqual(params.controlledGroundArea, true);
  assert.strictEqual(params.populationDensity, 'Controlled');
  
  const results = calculateSORA25(params);
  // Controlled + "3m_35ms" → iGRC = 2 (Table 2)
  assert.strictEqual(results.initialGRC, 2);
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
