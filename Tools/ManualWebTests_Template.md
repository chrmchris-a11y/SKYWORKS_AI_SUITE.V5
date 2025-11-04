# SKYWORKS Manual Web API Tests - SORA 2.0 & 2.5

## Test Endpoint
**POST** `http://localhost:5210/api/sora/complete`

## How to Test
1. Start services: Run `Tools\WebSmokeTests.ps1` or manually start Python (port 8001) + .NET API (port 5210)
2. Use Postman, curl, or browser Dev Tools to send POST requests
3. Copy the **Request JSON** from below
4. Paste the **Response** you get in the "Actual Response" section
5. I'll validate if results are EASA/JARUS compliant ✅

---

## SORA 2.0 Tests (10 cases)

### Test 2.0-01: Small Drone VLOS Sparse
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 0.5,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-02: BVLOS Urban Medium Drone
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "BVLOS_DenselyPopulated",
    "maxCharacteristicDimension": 2.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_c",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-03: Boundary Test - Exactly 1.0m
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 1.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-04: Boundary Test - Exactly 3.0m
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 3.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-05: Boundary Test - Exactly 8.0m
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 8.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_d",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-06: Controlled Ground Area
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "isControlledGroundArea": true,
    "maxCharacteristicDimension": 0.8,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-07: M1 High Mitigation
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 2.5,
    "mitigations": [
      { "type": "M1A", "robustness": "High" }
    ]
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-08: M2 High Mitigation
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 2.0,
    "mitigations": [
      { "type": "M2", "robustness": "High" }
    ]
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-09: Strategic Mitigations (S1, S2)
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "BVLOS_SparselyPopulated",
    "maxCharacteristicDimension": 3.5,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_c",
    "strategicMitigations": ["S1", "S2"]
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.0-10: Large Drone BVLOS
**Request:**
```json
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "BVLOS_SparselyPopulated",
    "maxCharacteristicDimension": 6.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_d",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

## SORA 2.5 Tests (10 cases)

### Test 2.5-01: Sub-250g Rule (0.20m, 20 m/s)
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 10.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 0.20,
    "maxSpeed": 20.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Expected:** intrinsicGRC=1, SAIL=I (sub-250g rule applied)

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-02: Out-of-Scope (High Population + Large Dimension)
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 9000.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 9.0,
    "maxSpeed": 90.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_d",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Expected:** HTTP 400, intrinsicGRC ≥ 9 (out-of-scope)

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-03: Worst-Case 3m @ 20 m/s
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 50.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 3.0,
    "maxSpeed": 20.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Expected:** Dimension category "3m" (MAX logic: max(dim_idx=1, speed_idx=0) = 1)

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-04: Controlled Ground + M1 High
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 100.0,
    "isControlledGroundArea": true,
    "maxCharacteristicDimension": 2.0,
    "maxSpeed": 15.0,
    "mitigations": [
      { "type": "M1A", "robustness": "High" }
    ]
  },
  "airRisk": {
    "explicitARC": "ARC_a",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.5-05: M2 High Penalty
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 300.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 2.0,
    "maxSpeed": 12.0,
    "mitigations": [
      { "type": "M2", "robustness": "High" }
    ]
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": ["S1"]
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

### Test 2.5-06: Boundary Test - Exactly 1.0m
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 20.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 1.0,
    "maxSpeed": 5.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a"
  },
  "implementedOSOs": []
}
```
**Expected:** Dimension category "1m"

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-07: Boundary Test - Exactly 3.0m
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 20.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 3.0,
    "maxSpeed": 5.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a"
  },
  "implementedOSOs": []
}
```
**Expected:** Dimension category "3m"

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-08: Boundary Test - Exactly 8.0m
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 20.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 8.0,
    "maxSpeed": 5.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a"
  },
  "implementedOSOs": []
}
```
**Expected:** Dimension category "8m"

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-09: ARC Floor (ARC_b + S1 + S2)
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 80.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 2.5,
    "maxSpeed": 18.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_b",
    "strategicMitigations": ["S1", "S2"]
  },
  "implementedOSOs": []
}
```
**Expected:** residualARC should be "a" or "b" (floor prevents going below ARC-b unless atypical)

**Actual Response:** (paste here)
```json

```

---

### Test 2.5-10: High ARC_d
**Request:**
```json
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 400.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 4.0,
    "maxSpeed": 22.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_d"
  },
  "implementedOSOs": []
}
```
**Actual Response:** (paste here)
```json

```

---

## How to Submit for Validation

1. **Fill in all "Actual Response" sections** with the JSON you receive from the API
2. **Save this file** as `ManualWebTests_Results.md` in the same folder
3. **Tell me:** "έτοιμος για validation" and I'll check all 20 responses against JARUS/EASA tables! ✅

---

## Quick curl Examples (if you prefer terminal)

**SORA 2.0 Test:**
```powershell
$body = @'
{
  "soraVersion": "2.0",
  "groundRisk": {
    "scenario_V2_0": "VLOS_SparselyPopulated",
    "maxCharacteristicDimension": 0.5,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
'@
Invoke-WebRequest -Uri "http://localhost:5210/api/sora/complete" -Method POST -Body $body -ContentType "application/json" | Select-Object -ExpandProperty Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

**SORA 2.5 Test:**
```powershell
$body = @'
{
  "soraVersion": "2.5",
  "groundRisk": {
    "populationDensity": 10.0,
    "isControlledGroundArea": false,
    "maxCharacteristicDimension": 0.20,
    "maxSpeed": 20.0,
    "mitigations": []
  },
  "airRisk": {
    "explicitARC": "ARC_a",
    "strategicMitigations": []
  },
  "implementedOSOs": []
}
'@
Invoke-WebRequest -Uri "http://localhost:5210/api/sora/complete" -Method POST -Body $body -ContentType "application/json" | Select-Object -ExpandProperty Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
```
