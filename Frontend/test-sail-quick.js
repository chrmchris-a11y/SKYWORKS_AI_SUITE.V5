// Quick Verification Test - SAIL Calculator
// Run with: node test-sail-quick.js

const SAILCalculator = require('./assets/js/sail-calculator.js');

console.log('\n========================================');
console.log('🧪 SAIL CALCULATOR - QUICK TEST');
console.log('========================================\n');

// Test 1: SORA 2.0 - GRC 5 + ARC-c Special Case
console.log('📋 Test 1: GRC 5 + ARC-c Special Case (SORA 2.0)');
const test1 = SAILCalculator.calculateSAIL_v2_0(
    { maxDimension: 3, maxSpeed: 25, mass: 8 },
    {
        soraVersion: '2.0',
        maxHeightAGL: 100,
        maxHeightAMSL: 200,
        airspaceClass: 'G',
        locationType: 'Non-Airport',
        environment: 'Suburban',
        isModeS: false,
        isTMZ: false,
        typicality: 'Typical',
        vlos: false,
        populationDensity: 80,
        isControlledGround: false,
        M1_level: 'Low',      // -1
        M2_level: 'Medium',   // 0
        M3_level: 'High'      // -1 (CORRECTED to achieve GRC 5)
    }
);

console.log('  iGRC:', test1.iGRC);
console.log('  Final GRC:', test1.finalGRC);
console.log('  Residual ARC:', test1.residualARC);
console.log('  SAIL:', test1.SAIL);
console.log('  Expected: SAIL IV (special case!)');
console.log('  Status:', test1.SAIL === 'IV' ? '✅ PASSED' : '❌ FAILED');

// Test 2: SORA 2.5 - 250g Special Rule
console.log('\n📋 Test 2: 250g Special Rule (SORA 2.5)');
const test2 = SAILCalculator.calculateSAIL_v2_5(
    { maxDimension: 0.3, maxSpeed: 20, mass: 0.249 },
    {
        soraVersion: '2.5',
        maxHeightAGL: 100,
        maxHeightAMSL: 200,
        airspaceClass: 'G',
        locationType: 'Non-Airport',
        environment: 'Urban',
        isModeS: false,
        isTMZ: false,
        typicality: 'Typical',
        vlos: true,
        populationDensity: 8000,
        isControlledGround: false,
        M1A_level: 'None',
        M1B_level: 'None',
        M2_level: 'Medium',
        sheltering: false
    }
);

console.log('  iGRC:', test2.iGRC);
console.log('  Final GRC:', test2.finalGRC);
console.log('  Residual ARC:', test2.residualARC);
console.log('  SAIL:', test2.SAIL);
console.log('  Expected: iGRC 1 (special rule), SAIL II');
console.log('  Status:', test2.iGRC === 1 && test2.SAIL === 'II' ? '✅ PASSED' : '❌ FAILED');

// Test 3: SORA 2.0 vs 2.5 Comparison
console.log('\n📋 Test 3: SORA 2.0 vs 2.5 Comparison');
const drone = { maxDimension: 0.9, maxSpeed: 15, mass: 0.6 };
const operation = {
    maxHeightAGL: 70,
    maxHeightAMSL: 150,
    airspaceClass: 'G',
    locationType: 'Non-Airport',
    environment: 'Rural',
    isModeS: false,
    isTMZ: false,
    typicality: 'Typical',
    vlos: true,
    populationDensity: 15,
    isControlledGround: false
};

const test3_v2_0 = SAILCalculator.calculateSAIL_v2_0(
    drone,
    { ...operation, soraVersion: '2.0', M1_level: 'None', M2_level: 'Medium', M3_level: 'None' }
);

const test3_v2_5 = SAILCalculator.calculateSAIL_v2_5(
    drone,
    { ...operation, soraVersion: '2.5', M1A_level: 'None', M1B_level: 'None', M2_level: 'Medium' }
);

console.log('  SORA 2.0: iGRC', test3_v2_0.iGRC, '→ SAIL', test3_v2_0.SAIL);
console.log('  SORA 2.5: iGRC', test3_v2_5.iGRC, '→ SAIL', test3_v2_5.SAIL);
console.log('  Expected: Different SAIL values (2.5 more conservative)');
console.log('  Status:', test3_v2_0.SAIL !== test3_v2_5.SAIL ? '✅ PASSED' : '❌ FAILED');

// Test 4: ARC Determination
console.log('\n📋 Test 4: ARC Determination (12 AEC Categories)');
const arcTests = [
    { name: 'FL600', params: { maxHeightAMSL: 19000, maxHeightAGL: 100, airspaceClass: 'G', locationType: 'Non-Airport', environment: 'Rural', isModeS: false, isTMZ: false, typicality: 'Typical' }, expected: 'b', aec: 11 },
    { name: 'Atypical', params: { maxHeightAMSL: 200, maxHeightAGL: 100, airspaceClass: 'G', locationType: 'Non-Airport', environment: 'Rural', isModeS: false, isTMZ: false, typicality: 'Atypical' }, expected: 'a', aec: 12 },
    { name: 'Airport B/C/D', params: { maxHeightAMSL: 200, maxHeightAGL: 100, airspaceClass: 'C', locationType: 'Airport', environment: 'Urban', isModeS: false, isTMZ: false, typicality: 'Typical' }, expected: 'd', aec: 1 },
    { name: 'Rural <500ft', params: { maxHeightAMSL: 200, maxHeightAGL: 100, airspaceClass: 'G', locationType: 'Non-Airport', environment: 'Rural', isModeS: false, isTMZ: false, typicality: 'Typical' }, expected: 'b', aec: 10 }
];

let arcPassed = 0;
arcTests.forEach(test => {
    const result = SAILCalculator.determineInitialARC(test.params);
    const passed = result.arc === test.expected && result.aec === test.aec;
    console.log(`  ${test.name}: ARC-${result.arc} (AEC ${result.aec}) - ${passed ? '✅' : '❌'}`);
    if (passed) arcPassed++;
});

console.log(`  Status: ${arcPassed}/${arcTests.length} passed`);

// Summary
console.log('\n========================================');
console.log('📊 SUMMARY');
console.log('========================================');
console.log('Test 1 (GRC 5 Special Case):', test1.SAIL === 'IV' ? '✅ PASSED' : '❌ FAILED');
console.log('Test 2 (250g Rule):', test2.iGRC === 1 && test2.SAIL === 'II' ? '✅ PASSED' : '❌ FAILED');
console.log('Test 3 (2.0 vs 2.5):', test3_v2_0.SAIL !== test3_v2_5.SAIL ? '✅ PASSED' : '❌ FAILED');
console.log('Test 4 (ARC):', arcPassed === arcTests.length ? '✅ PASSED' : '❌ FAILED');

const totalPassed = 
    (test1.SAIL === 'IV' ? 1 : 0) +
    (test2.iGRC === 1 && test2.SAIL === 'II' ? 1 : 0) +
    (test3_v2_0.SAIL !== test3_v2_5.SAIL ? 1 : 0) +
    (arcPassed === arcTests.length ? 1 : 0);

console.log('\nTotal:', totalPassed, '/ 4 tests passed');
console.log('Success Rate:', Math.round((totalPassed / 4) * 100) + '%');

if (totalPassed === 4) {
    console.log('\n✅ ALL TESTS PASSED - READY FOR PRODUCTION!');
} else {
    console.log('\n❌ SOME TESTS FAILED - REVIEW REQUIRED');
}

console.log('');
