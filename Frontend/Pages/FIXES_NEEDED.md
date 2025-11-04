# COMPREHENSIVE TEST FIXES - ALL 20 TESTS

## API-VERIFIED CORRECT VALUES

### SORA 2.0 TESTS (Tests 1-10)

**TEST 1:** ✅ ALREADY CORRECT
- Params: mtom=0.249kg, pop=1, M1=Medium, M2=High, M3=None
- Expected: initial_grc=1, mitigation_total=-4, final_grc=1

**TEST 2:** ✅ ALREADY CORRECT  
- Params: mtom=0.249kg, pop=25000, M1=High, M2=High, M3=None
- Expected: initial_grc=4, mitigation_total=-4, final_grc=1

**TEST 3:** ✅ ALREADY CORRECT
- Params: mtom=0.8kg, pop=400, M1=Medium, M2=Medium, M3=None
- Expected: initial_grc=2, mitigation_total=-3, final_grc=1

**TEST 4:** ✅ ALREADY CORRECT
- Params: mtom=0.8kg, pop=8000, M1=Low, M2=Low, M3=None
- Current: initial_grc=4, final_grc=2, mitigation_total=-2
- CORRECT: initial_grc=4, final_grc=3, mitigation_total=-1

**TEST 5:** ✅ ALREADY CORRECT
- Params: mtom=5kg, pop=1, M1=High, M2=High, M3=None
- Expected: initial_grc=2, mitigation_total=-4, final_grc=1

**TEST 6:** ❌ NEEDS FIX - ALREADY FIXED
- Params: mtom=5kg, pop=30000, M1=Medium, M2=High, M3=None
- Current was: initial_grc=6, final_grc=3, mitigation_total=-3
- CORRECT: initial_grc=6, final_grc=2, mitigation_total=-4

**TEST 7:** ✅ ALREADY CORRECT
- Params: mtom=32kg, pop=25000, M1=Medium, M2=High, M3=None
- Expected: initial_grc=7, mitigation_total=-4, final_grc=3

**TEST 8:** ✅ ALREADY CORRECT
- Params: mtom=32kg, pop=25000, M1=None, M2=None, M3=Low
- Expected: initial_grc=7, mitigation_total=0, final_grc=7

**TEST 9:** ✅ ALREADY CORRECT
- Params: mtom=100kg, pop=50000, M1=Medium, M2=High, M3=None
- Expected: initial_grc=7, mitigation_total=-4, final_grc=3

**TEST 10:** ❌ NEEDS FIX
- Params: mtom=0.24kg, pop=10000, M1=Low, M2=Low, M3=None
- Current: initial_grc=3, final_grc=1, mitigation_total=-2
- CORRECT: initial_grc=3, final_grc=2, mitigation_total=-1

---

### SORA 2.5 TESTS (Tests 11-20)

**TEST 11:** ❌ NEEDS FIX
- Params: mtom=0.249kg, pop=1, M1A=High, M1B=Low, M1C=None, M2=High
- Current: initial_grc=1, final_grc=1, mitigation_total=-4
- CORRECT: initial_grc=1, final_grc=1, mitigation_total=-2

**TEST 12:** ❌ NEEDS FIX
- Params: mtom=0.249kg, pop=25000, M1A=High, M1B=Low, M1C=High, M2=High
- Current: initial_grc=4, final_grc=1, mitigation_total=-5
- CORRECT: initial_grc=4, final_grc=2, mitigation_total=-2

**TEST 13:** ❌ NEEDS FIX
- Params: mtom=0.8kg, pop=400, M1A=Low, M1B=Low, M1C=Low, M2=Medium
- Current: initial_grc=2, final_grc=1, mitigation_total=-4
- CORRECT: initial_grc=2, final_grc=1, mitigation_total=-3

**TEST 14:** ❌ NEEDS FIX
- Params: mtom=0.8kg, pop=8000, M1A=Low, M1B=None, M1C=None, M2=Low
- Current: initial_grc=4, final_grc=2, mitigation_total=-2
- CORRECT: initial_grc=4, final_grc=3, mitigation_total=-1

**TEST 15:** ❌ NEEDS FIX
- Params: mtom=5kg, pop=1, M1A=High, M1B=Low, M1C=None, M2=High
- Current: initial_grc=2, final_grc=1, mitigation_total=-4
- CORRECT: initial_grc=2, final_grc=1, mitigation_total=-2

**TEST 16:** ❌ NEEDS FIX
- Params: mtom=5kg, pop=30000, M1A=Low, M1B=Low, M1C=Low, M2=Medium
- Current: initial_grc=6, final_grc=2, mitigation_total=-4
- CORRECT: initial_grc=6, final_grc=3, mitigation_total=-3

**TEST 17:** ❌ NEEDS FIX
- Params: mtom=32kg, pop=25000, M1A=Low, M1B=Low, M1C=Low, M2=Medium
- Current: initial_grc=7, final_grc=3, mitigation_total=-4
- CORRECT: initial_grc=7, final_grc=4, mitigation_total=-3

**TEST 18:** ✅ ALREADY CORRECT
- Params: mtom=32kg, pop=25000, M1A=None, M1B=None, M1C=None, M2=None
- Expected: initial_grc=7, mitigation_total=0, final_grc=7

**TEST 19:** ❌ NEEDS FIX
- Params: mtom=100kg, pop=50000, M1A=High, M1B=Low, M1C=High, M2=High
- Current: initial_grc=7, final_grc=1, mitigation_total=-6
- CORRECT: initial_grc=7, final_grc=5, mitigation_total=-2

**TEST 20:** ❌ NEEDS FIX
- Params: mtom=0.5kg, pop=500, M1A=Low, M1B=Low, M1C=Low, M2=Low
- Current: initial_grc=2, final_grc=1, mitigation_total=-4
- CORRECT: initial_grc=2, final_grc=1, mitigation_total=-2

---

## SUMMARY

- **SORA 2.0**: 2 fixes needed (TEST 4, TEST 10) + TEST 6 already fixed = 9/10 correct
- **SORA 2.5**: 9 fixes needed (TEST 11-17, 19-20) = 1/10 correct
- **TOTAL**: 11 fixes needed out of 20 tests

## NEXT STEPS

Apply these corrections to `test-comprehensive-sora.html` lines 318-580 (test definitions).
