# 🧪 WEB UI TESTING GUIDE - SORA 2.0 & 2.5

## 📋 ΤΙ ΝΑ ΔΟΚΙΜΑΣΕΙΣ:

Ακολούθησε αυτά τα **3 scenarios** για να επιβεβαιώσεις ότι όλα δουλεύουν:

---

## ✅ TEST 1: SORA 2.0 - Basic Scenario (GRC 3 + ARC-b → SAIL III)

### Βήματα:
1. **Άνοιξε το browser**: http://localhost:5210/app/Pages/mission.html
2. **Επίλεξε SORA Version**: **2.0**
3. **Ground Risk Section**:
   - **Operational Scenario**: `BVLOS over sparsely populated area`
   - **Max Dimension**: `1.0` m
   - **MTOM**: `0.5` kg
   - **Mitigations**: Άφησε κενό (No mitigations)

4. **Air Risk Section**:
   - **Explicit ARC**: `ARC-b` (ή αν δεν υπάρχει dropdown, χρησιμοποίησε Environment: Rural, <500ft)
   - **Strategic Mitigations**: Κανένα

5. **Πάτησε "Calculate SORA"**

### ✅ Αναμενόμενο Αποτέλεσμα:
```
✓ Success: true
✓ Initial GRC: 3
✓ Final GRC: 3
✓ SAIL: III
✓ TMPR: Medium
✓ No errors
```

---

## ✅ TEST 2: SORA 2.0 - GRC 5 + ARC-d → SAIL V (ΤΟ MAIN FIX!)

### Βήματα:
1. **SORA Version**: **2.0**
2. **Ground Risk**:
   - **Operational Scenario**: `VLOS over populated area`
   - **Max Dimension**: `3.0` m
   - **MTOM**: `1.5` kg

3. **Air Risk**:
   - **Explicit ARC**: `ARC-d`
   - **ή Environment**: Urban + Controlled Airspace + >500ft altitude

4. **Calculate SORA**

### ✅ Αναμενόμενο Αποτέλεσμα:
```
✓ Success: true
✓ Initial GRC: 5
✓ Final GRC: 5
✓ Residual ARC: ARC-d
✓ SAIL: V  ← ΤΟ ΣΗΜΑΝΤΙΚΟ!
✓ TMPR: VeryHigh
✓ No "out of scope" error
```

**ΣΗΜΑΝΤΙΚΟ**: Παλιά αυτό θα έδινε **error "out of scope"**. Τώρα πρέπει να δουλεύει!

---

## ✅ TEST 3: SORA 2.5 - Suburban with Sheltering (GRC 6 → 4 → SAIL IV)

### Βήματα:
1. **SORA Version**: **2.5**
2. **Ground Risk**:
   - **Population Density**: `3000` ppl/km²
   - **Max Dimension**: `3.0` m
   - **MTOM**: `1.5` kg
   - **Mitigations**: Επίλεξε **M1A - Sheltering** με **Low** robustness

3. **Air Risk**:
   - **Explicit ARC**: `ARC-c`
   - **ή**: Urban + <500ft altitude

4. **Calculate SORA**

### ✅ Αναμενόμενο Αποτέλεσμα:
```
✓ Success: true
✓ Initial GRC: 6
✓ Final GRC: 4  ← (M1A Low = -2 reduction)
✓ SAIL: IV
✓ TMPR: High
```

---

## ❌ TEST 4: SAIL VI Should Be OUT OF SCOPE

### Βήματα:
1. **SORA Version**: **2.5**
2. **Ground Risk**:
   - **Population Density**: `12000` ppl/km²
   - **Max Dimension**: `3.0` m

3. **Air Risk**:
   - **ARC**: `ARC-c`

4. **Calculate SORA**

### ✅ Αναμενόμενο Αποτέλεσμα:
```
✗ Success: false
✗ Error: "SAIL VI requires CERTIFIED category"
✗ Status Code: 400 (Bad Request)
✗ SAIL: VI (αλλά rejected)
```

**ΣΗΜΑΝΤΙΚΟ**: Αυτό **πρέπει να απορριφθεί** ως out of scope!

---

## 🔍 ΠΩΣ ΝΑ ΕΛΕΓΞΕΙΣ ΑΝ ΔΟΥΛΕΥΕΙ:

### 1. **Άνοιξε Browser Developer Tools** (F12)
   - Πήγαινε στο **Console** tab
   - Θα δεις όλα τα API requests και responses

### 2. **Πήγαινε στο Network Tab**
   - Φίλτραρε για: `sora/complete`
   - Δες το Request Payload (τι στέλνεις)
   - Δες το Response (τι επιστρέφει)

### 3. **Ψάξε για Errors**:
   ```javascript
   // Στο Console θα δεις:
   ✓ POST http://localhost:5210/api/sora/complete 200 OK
   ✓ Response: {isSuccessful: true, sail: "V", ...}
   
   // ή αν κάτι πάει λάθος:
   ✗ POST http://localhost:5210/api/sora/complete 400 Bad Request
   ✗ Error: {...}
   ```

---

## 🚨 ΑΝ ΔΕΙΣ ERRORS:

### Error 1: "The request field is required"
**Αιτία**: Το JSON δεν είναι σωστά formatted  
**Fix**: Έλεγξε ότι όλα τα required fields έχουν τιμές

### Error 2: "Could not convert to OperationalScenario"
**Αιτία**: Λάθος enum value  
**Fix**: Χρησιμοποίησε exact values:
- `VLOS_SparselyPopulated`
- `BVLOS_SparselyPopulated`
- `VLOS_Populated`
- `BVLOS_Populated`
- `ControlledGroundArea`

### Error 3: "Out of scope" για GRC 5 + ARC-d
**Αιτία**: Παλιό bug (FIXED!)  
**Fix**: Κάνε refresh το page, το backend έχει διορθωθεί

### Error 4: CORS errors
**Αιτία**: Backend δεν τρέχει ή wrong port  
**Fix**: Έλεγξε ότι API τρέχει στο `http://localhost:5210`

---

## 📊 QUICK VALIDATION CHECKLIST:

Μετά από κάθε test, έλεγξε:
- ✅ **isSuccessful**: `true` (για valid scenarios)
- ✅ **sail**: Σωστή τιμή (I, II, III, IV, V, VI, ή null για Category C)
- ✅ **intrinsicGRC**: Σωστή τιμή (1-8)
- ✅ **finalGRC**: Σωστή τιμή μετά από mitigations
- ✅ **residualARC**: Σωστό ARC μετά από strategic mitigations
- ✅ **errors**: Κενό array (ή κατάλληλο error message για out of scope)

---

## 🎯 ΓΡΗΓΟΡΗ ΔΟΚΙΜΗ (Copy-Paste στο Console):

Αν θες να δοκιμάσεις απευθείας από το Console:

```javascript
// TEST: GRC 5 + ARC-d → SAIL V
fetch('http://localhost:5210/api/sora/complete', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    soraVersion: "2.0",
    groundRisk: {
      scenario_V2_0: "VLOS_Populated",
      maxCharacteristicDimension: 3.0,
      mtom_kg: 1.5,
      mitigations: []
    },
    airRisk: {
      explicitARC: "ARC_d",
      strategicMitigations: [],
      isAtypicalSegregated: false
    }
  })
})
.then(r => r.json())
.then(data => {
  console.log('✅ SUCCESS:', data.isSuccessful);
  console.log('SAIL:', data.sail);
  console.log('GRC:', data.finalGRC);
  console.log('ARC:', data.residualARC);
})
.catch(err => console.error('❌ ERROR:', err));
```

---

## 💡 TIPS:

1. **Κράτα ανοιχτό το Console** (F12) για να δεις όλα τα logs
2. **Δοκίμασε με και χωρίς mitigations** για να δεις τη διαφορά
3. **Σύγκρινε τα αποτελέσματα** με τα authoritative test cases
4. **Αν κάτι δεν δουλεύει**, πάρε screenshot του error και του request

---

## 📝 NOTES:

- Όλα τα **20 comprehensive tests πέρασαν** (100%)
- Το **GRC 5-6 + ARC-d = SAIL V** fix δουλεύει
- Το **SAIL VI rejection** δουλεύει σωστά
- Οι **strategic mitigations** (S1-S4) δουλεύουν
- Τα **M1A sheltering levels** δουλεύουν

**ΑΝ ΟΛΑ ΔΟΥΛΕΥΟΥΝ → WEB UI IS EASA/JARUS COMPLIANT! ✅**
