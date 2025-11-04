# 🧪 SKYWORKS MISSION PLANNER - 20 TEST SCENARIOS

**Objective:** Validate SORA 2.0 and SORA 2.5 calculations with real drone data  
**Date:** October 29, 2025  
**Mission Page URL:** http://localhost:5210/app/Pages/mission.html

---

## 📋 TEST STRUCTURE

- **10 Tests for SORA 2.0** (MTOM-based Table 2)
- **10 Tests for SORA 2.5** (Dimension+Speed-based Table 2)
- Each test includes: Drone selection, Mission parameters, Expected iGRC

---

## ✅ SORA 2.0 TESTS (MTOM-Based)

### Test #1: Ultra-Light Drone, Controlled Area
- **Category:** SORA 2.0
- **Drone:** DJI Mini 2 (0.249 kg, C0)
- **Population Density:** ≤1 ppl/km² (Controlled Ground Area)
- **Expected iGRC:** 1
- **Rationale:** MTOM <0.25kg, Category 0, Row 0, Col 0 → iGRC=1

### Test #2: Light Drone, Sparsely Populated
- **Category:** SORA 2.0
- **Drone:** DJI Mavic Air 2S (0.595 kg, C1)
- **Population Density:** 100 ppl/km² (Sparsely populated)
- **Expected iGRC:** 2
- **Rationale:** MTOM 0.25-1kg, Category 1, Row 1, Col 1 → iGRC=2

### Test #3: Light Drone, Suburban Area
- **Category:** SORA 2.0
- **Drone:** DJI Air 2S (0.595 kg, C1)
- **Population Density:** 8,000 ppl/km² (Moderately populated)
- **Expected iGRC:** 4
- **Rationale:** MTOM 0.25-1kg, Category 1, Row 1, Col 2 → iGRC=4

### Test #4: Medium Drone, High Density
- **Category:** SORA 2.0
- **Drone:** DJI Phantom 4 Pro (1.375 kg, C2)
- **Population Density:** 12,000 ppl/km² (High density)
- **Expected iGRC:** 6
- **Rationale:** MTOM 1-25kg, Category 2, Row 2, Col 3 → iGRC=6

### Test #5: Medium Drone, Controlled Area
- **Category:** SORA 2.0
- **Drone:** DJI Mavic 3 (0.9 kg, C2)
- **Population Density:** 1 ppl/km² (Controlled)
- **Expected iGRC:** 1
- **Rationale:** MTOM 0.25-1kg, Category 1, Row 1, Col 0 → iGRC=1

### Test #6: Heavy Drone, Sparsely Populated
- **Category:** SORA 2.0
- **Drone:** DJI Matrice 300 RTK (6.3 kg, C3)
- **Population Density:** 250 ppl/km² (Sparsely populated)
- **Expected iGRC:** 4
- **Rationale:** MTOM 1-25kg, Category 2, Row 2, Col 1 → iGRC=3

### Test #7: Heavy Drone, Suburban
- **Category:** SORA 2.0
- **Drone:** DJI Matrice 300 RTK (6.3 kg, C3)
- **Population Density:** 5,000 ppl/km² (Moderately populated)
- **Expected iGRC:** 5
- **Rationale:** MTOM 1-25kg, Category 2, Row 2, Col 2 → iGRC=5

### Test #8: Very Heavy Drone, Controlled Area
- **Category:** SORA 2.0
- **Drone:** Custom Heavy Lift (50 kg, C4)
- **MTOM:** 50 kg (manual input)
- **Population Density:** 1 ppl/km² (Controlled)
- **Expected iGRC:** 3
- **Rationale:** MTOM 25-150kg, Category 3, Row 3, Col 0 → iGRC=3

### Test #9: Very Heavy Drone, High Density
- **Category:** SORA 2.0
- **Drone:** Custom Heavy Lift (50 kg, C4)
- **MTOM:** 50 kg (manual input)
- **Population Density:** 15,000 ppl/km² (High density)
- **Expected iGRC:** 7
- **Rationale:** MTOM 25-150kg, Category 3, Row 3, Col 3 → iGRC=7

### Test #10: Ultra-Heavy Cargo Drone, Suburban
- **Category:** SORA 2.0
- **Drone:** Custom Cargo (200 kg, C5)
- **MTOM:** 200 kg (manual input)
- **Population Density:** 7,000 ppl/km² (Moderately populated)
- **Expected iGRC:** 7
- **Rationale:** MTOM >150kg, Category 4, Row 4, Col 2 → iGRC=7

---

## ✅ SORA 2.5 TESTS (Dimension+Speed-Based)

### Test #11: Small Drone, Remote Area
- **Category:** SORA 2.5
- **Drone:** DJI Mini 2 (Dimension: 0.245m, Speed: 16m/s)
- **Population Density:** <5 ppl/km² (Remote)
- **Expected iGRC:** 2
- **Rationale:** Dimension ≤1m, Speed ≤25m/s, Category 0, Row 1, Col 0 → iGRC=2

### Test #12: Small Drone, Special Rule
- **Category:** SORA 2.5
- **Drone:** DJI Mini 2 (MTOM: 0.249kg, Speed: 16m/s)
- **Population Density:** 1,000 ppl/km² (Suburban)
- **Expected iGRC:** 1
- **Rationale:** ≤250g MTOM + ≤25m/s → **Special Rule Override** → iGRC=1

### Test #13: Medium Drone, Lightly Populated
- **Category:** SORA 2.5
- **Drone:** DJI Air 2S (Dimension: 0.30m, Speed: 19m/s)
- **Population Density:** 25 ppl/km² (Lightly populated)
- **Expected iGRC:** 3
- **Rationale:** Dimension ≤3m, Speed ≤35m/s, Category 1, Row 2, Col 1 → iGRC=3

### Test #14: Medium Drone, Suburban
- **Category:** SORA 2.5
- **Drone:** DJI Mavic 3 (Dimension: 0.35m, Speed: 19m/s)
- **Population Density:** 2,000 ppl/km² (Suburban)
- **Expected iGRC:** 6
- **Rationale:** Dimension ≤3m, Speed ≤35m/s, Category 1, Row 4, Col 1 → iGRC=6

### Test #15: Large Drone, Sparsely Populated
- **Category:** SORA 2.5
- **Drone:** DJI Matrice 300 RTK (Dimension: 0.66m, Speed: 23m/s)
- **Population Density:** 200 ppl/km² (Sparsely populated)
- **Expected iGRC:** 5
- **Rationale:** Dimension ≤8m, Speed ≤75m/s, Category 2, Row 3, Col 2 → iGRC=6

### Test #16: Large Drone, High Density Metro
- **Category:** SORA 2.5
- **Drone:** DJI Inspire 2 (Dimension: 0.60m, Speed: 26m/s)
- **Population Density:** 10,000 ppl/km² (High density metro)
- **Expected iGRC:** 8
- **Rationale:** Dimension ≤8m, Speed ≤75m/s, Category 2, Row 5, Col 2 → iGRC=8

### Test #17: Very Large Drone, Remote Area
- **Category:** SORA 2.5
- **Drone:** Custom Fixed-Wing (Dimension: 15m, Speed: 100m/s)
- **Population Density:** 3 ppl/km² (Remote)
- **Expected iGRC:** 5
- **Rationale:** Dimension ≤20m, Speed ≤120m/s, Category 3, Row 1, Col 3 → iGRC=5

### Test #18: Very Large Drone, Suburban
- **Category:** SORA 2.5
- **Drone:** Custom Fixed-Wing (Dimension: 15m, Speed: 100m/s)
- **Population Density:** 3,000 ppl/km² (Suburban)
- **Expected iGRC:** 8
- **Rationale:** Dimension ≤20m, Speed ≤120m/s, Category 3, Row 4, Col 3 → iGRC=8

### Test #19: Extra Large Drone, Lightly Populated
- **Category:** SORA 2.5
- **Drone:** Custom Heavy Cargo (Dimension: 25m, Speed: 130m/s)
- **Population Density:** 40 ppl/km² (Lightly populated)
- **Expected iGRC:** 6
- **Rationale:** Dimension >20m, Speed >120m/s, Category 4, Row 2, Col 4 → iGRC=6

### Test #20: Grey Cell Detection (Out of Scope)
- **Category:** SORA 2.5
- **Drone:** Custom Large Fixed-Wing (Dimension: 10m, Speed: 80m/s)
- **Population Density:** 60,000 ppl/km² (Assemblies of people)
- **Expected iGRC:** -1 (OUT_OF_SCOPE)
- **Rationale:** Row 6, Col 2 → Grey cell → Out of SORA 2.5 scope

---

## 📊 EXPECTED RESULTS SUMMARY

| Version | Tests | Expected Pass Rate | Key Validations |
|---------|-------|-------------------|-----------------|
| SORA 2.0 | 10 | 100% (10/10) | MTOM-based Table 2 (5×4 matrix) |
| SORA 2.5 | 10 | 100% (10/10) | Dimension+Speed Table 2 (7×5 matrix), Special rule, Grey cells |
| **TOTAL** | **20** | **100% (20/20)** | **Dual version compliance** |

---

## 🎯 VALIDATION CRITERIA

✅ **Pass:** iGRC matches expected value within ±0 tolerance  
⚠️ **Warning:** iGRC within ±1 (review needed)  
❌ **Fail:** iGRC differs by >1 or calculation error

---

## 🚀 EXECUTION INSTRUCTIONS

1. **Start Backend + Python Services:**
   ```powershell
   # Backend API (Port 5210)
   cd Backend
   dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210
   
   # Python FastAPI (Port 8001)
   cd Backend_Python
   python -m uvicorn main:app --host 0.0.0.0 --port 8001
   ```

2. **Open Mission Planner:**
   ```
   http://localhost:5210/app/Pages/mission.html
   ```

3. **Load Drone Database:**
   - Verify dropdown shows real drones (not fallback samples)
   - Check console for "Loaded X drones successfully"

4. **Execute Tests:**
   - For each test scenario, fill in the form fields
   - Click "🚀 Run Full SORA Pipeline"
   - Record actual iGRC from results
   - Compare with expected iGRC

5. **Document Results:**
   - Screenshot each test result
   - Note any discrepancies
   - Log browser console errors

---

## 📝 TEST EXECUTION LOG

```
Test #1: SORA 2.0, Mini 2, Controlled Area
Expected: iGRC=1
Actual: ___________
Status: ⬜ PASS / ⬜ FAIL
Notes: _____________________

Test #2: SORA 2.0, Air 2S, Sparsely Populated
Expected: iGRC=2
Actual: ___________
Status: ⬜ PASS / ⬜ FAIL
Notes: _____________________

... (continue for all 20 tests)
```

---

## 🔍 KEY DIFFERENCES TO VERIFY

| Feature | SORA 2.0 | SORA 2.5 |
|---------|----------|----------|
| Table 2 Input | **MTOM (kg)** | **Dimension (m) + Speed (m/s)** |
| Matrix Size | 5×4 | 7×5 |
| Max iGRC | 8 | 10 |
| Special Rules | None | ≤250g + ≤25m/s → iGRC=1 |
| Out of Scope | None | Grey cells (Row 6, Col 2-4) |
| Population Categories | 4 (≤1, ≤500, ≤10k, >10k) | 6 (<5, <50, <500, <5k, <50k, ≥50k) |

---

## ✅ SUCCESS CRITERIA

- **20/20 tests pass** (100% accuracy)
- **SORA 2.0 vs 2.5 calculations differ correctly** (not same results)
- **Drone database loads** (no fallback samples)
- **No backend errors** in browser console
- **Special rule works** (Test #12: ≤250g override)
- **Grey cell detection** (Test #20: Out of scope)

---

**Ready to execute! Θα κάνουμε live testing στο Mission Planner! 🚀**
