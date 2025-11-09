// DEBUG: Matrice 300 SAIL calculation issue
const calc = require('../../../WebPlatform/wwwroot/app/Pages/ui/assets/sora-calculator.js');

console.log('=== DEBUG: Matrice 300 RTK Test ===\n');

const params = {
  mtom_kg: 6.3,
  maxSpeed_ms: 23,
  characteristicDimension_m: 0.7,
  populationDensity: ">50000",
  m1a: "Low",
  m1b: "High",
  m1c: "Low",
  m2: "High",
  altitude_ft: 600,
  controlledAirspace: false,
  airportEnvironment: true,
  populatedArea: true,
  atypicalAirspace: false,
  isVLOS: false
};

console.log('Input params:', params);
console.log('');

const result = calc.calculateSORA25(params);

console.log('Results:');
console.log(`  iGRC: ${result.initialGRC}`);
console.log(`  fGRC: ${result.finalGRC}`);
console.log(`  AEC: ${result.aec}`);
console.log(`  iARC: ${result.initialARC}`);
console.log(`  rARC: ${result.residualARC}`);
console.log(`  SAIL: ${result.sail}`);
console.log('');

console.log('Expected:');
console.log('  iGRC: 7 (0.7m @ 23m/s, >50000)');
console.log('  fGRC: 1 (7 - 1(M1A) - 2(M1B) - 1(M1C) - 2(M2) = 1)');
console.log('  AEC: 6 (>500ft airport)');
console.log('  iARC: ARC-c');
console.log('  rARC: ARC-c (BVLOS, no reduction)');
console.log('  SAIL: II (GRC=1, ARC-c → Table 7)');
console.log('');

if (result.sail === 'II') {
  console.log('✅ TEST PASSED!');
  process.exit(0);
} else {
  console.error(`❌ TEST FAILED: Expected SAIL II, got ${result.sail}`);
  console.error('');
  console.error('Diagnosis:');
  if (result.finalGRC !== 1) {
    console.error(`  - finalGRC is ${result.finalGRC}, should be 1`);
    console.error('  - Mitigations not applied correctly?');
  }
  if (result.residualARC !== 'ARC-c') {
    console.error(`  - residualARC is ${result.residualARC}, should be ARC-c`);
  }
  process.exit(1);
}
