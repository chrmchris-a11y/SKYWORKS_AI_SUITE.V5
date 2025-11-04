# 📋 Εξήγηση Πεδίων Mission Planner UI

## ✅ Διορθώσεις που έγιναν

### 1. **Special Airspace Zones - Προστέθηκαν τα πεδία που έλειπαν**

**Πριν:** Μόνο 2 checkboxes (Mode-S, TMZ)  
**Τώρα:** 6 checkboxes σύμφωνα με EASA SORA requirements:

- ✅ **Mode-S Veil / Transponder Mandatory** - Ζώνη υποχρεωτικού transponder Mode-S
- ✅ **TMZ (Transponder Mandatory Zone)** - Ζώνη υποχρεωτικού transponder
- ✅ **RMZ (Radio Mandatory Zone)** - Ζώνη υποχρεωτικής ραδιοεπικοινωνίας
- ✅ **Danger Area** - Επικίνδυνη περιοχή
- ✅ **Prohibited Area** - Απαγορευμένη περιοχή
- ✅ **Restricted Area** - Περιορισμένη περιοχή

### 2. **ACE Categories Label - Διορθώθηκε το label**

**Πριν:** "Airspace Class (SORA 2.5 AEC Categories)"  
**Τώρα:** "Airspace Class (ACE Categories)"

**Γιατί:** Το ACE framework εφαρμόζεται **και στο SORA 2.0 και στο SORA 2.5** σύμφωνα με EASA AMC1 Article 11(1)(c).

---

## 📏 Εξήγηση Πεδίων Ύψους

### **Max Height AGL (Above Ground Level)**
- **Τι είναι:** Το μέγιστο ύψος πτήσης **πάνω από το έδαφος**
- **Χρήση:** Χρησιμοποιείται για τον υπολογισμό του **ARC (Air Risk Class)**
- **Παράδειγμα:** 
  - Πετάς 60m πάνω από το έδαφος → AGL = 60m
  - Αν το έδαφος είναι σε υψόμετρο 500m → AMSL = 500 + 60 = 560m

**Κανόνες ARC:**
```
AGL ≤ 150ft (45m)   → AEC 7 (χαμηλότερο ARC)
AGL > 150ft (45m)   → AEC 2 (υψηλότερο ARC)
```

### **Max Height AMSL (Above Mean Sea Level)**
- **Τι είναι:** Το μέγιστο ύψος πτήσης **πάνω από τη στάθμη της θάλασσας**
- **Χρήση:** Έλεγχος αν **ξεπερνάς FL600 (18,000m)**
- **Παράδειγμα:**
  - Περιοχή με υψόμετρο 2,000m
  - Πετάς 100m AGL
  - AMSL = 2,000 + 100 = 2,100m

**Κανόνας SORA 2.5:**
```
AMSL > 18,000m (FL600) → Αυτόματα υψηλότερο ARC
```

### **Γιατί χρειάζονται και τα δύο;**

| Πεδίο | Σκοπός | Παράδειγμα |
|-------|--------|-----------|
| **AGL** | Υπολογισμός ARC category | 60m AGL → AEC 2 |
| **AMSL** | Έλεγχος FL600 limit | 2,100m AMSL < 18,000m ✅ |

**Πρακτικό παράδειγμα:**
```
Περιοχή: Βουνό 1,500m υψόμετρο
Drone πετά: 50m AGL
→ AGL = 50m (για ARC)
→ AMSL = 1,550m (για FL600 check)
```

---

## 🌍 Special Airspace Zones - Πότε να τα επιλέξεις

### **Mode-S Veil / Transponder Mandatory Zone**
- **Πότε:** Γύρω από μεγάλα αεροδρόμια (π.χ. 30nm radius)
- **Απαίτηση:** Drone με transponder Mode-S
- **Επίδραση ARC:**
  ```
  AGL > 150ft + Mode-S → ARC = AEC 2 (υψηλό)
  AGL ≤ 150ft + Mode-S → ARC = AEC 7 (μέτριο)
  ```

### **TMZ (Transponder Mandatory Zone)**
- **Πότε:** Ζώνη υποχρεωτικού transponder
- **Απαίτηση:** Ενεργός SSR transponder
- **Επίδραση:** Ίδια με Mode-S

### **RMZ (Radio Mandatory Zone)**
- **Πότε:** Ζώνη υποχρεωτικής ραδιοεπικοινωνίας
- **Απαίτηση:** VHF radio + επικοινωνία με ATC
- **Επίδραση:** Απαιτεί OSO#2 (C2 link performance)

### **Danger Area**
- **Πότε:** Στρατιωτικές ασκήσεις, βολές, test zones
- **Απαίτηση:** NOTAM coordination + ATC clearance
- **Επίδραση:** Απαιτεί επιπλέον OSO mitigations

### **Prohibited Area**
- **Πότε:** Απόλυτα απαγορευμένη περιοχή (π.χ. presidential palace)
- **Απαίτηση:** **ΔΕΝ ΕΠΙΤΡΕΠΕΤΑΙ ΠΤΗΣΗ**
- **Επίδραση:** SORA assessment rejected

### **Restricted Area**
- **Πότε:** Περιορισμένη περιοχή με ειδική άδεια
- **Απαίτηση:** Προηγούμενη έγκριση αρμόδιας αρχής
- **Επίδραση:** Απαιτεί OSO#24 (Environmental impact)

---

## 🔄 ACE Categories - Γιατί εφαρμόζονται παντού

### **Τι είναι το ACE (Airspace Controlled Environment);**
- **Airspace Controlled Environment** = Ελεγχόμενο αεροδιαστημικό περιβάλλον
- Βασίζεται στην **ICAO Airspace Classification (A-G)**

### **Πίνακας ACE Categories**

| Class | ATC Control | IFR/VFR | Παράδειγμα | ARC Impact |
|-------|-------------|---------|-----------|-----------|
| **A** | Full control | IFR only | Υψηλά flight levels | High ARC |
| **B** | Full control | IFR + VFR | Major airports (CTR) | High ARC |
| **C** | Controlled | IFR + VFR | Regional airports (TMA) | Medium ARC |
| **D** | Controlled | IFR + VFR | Small airports | Medium ARC |
| **E** | IFR control | IFR + VFR | En-route airspace | Low-Medium ARC |
| **F** | Advisory | IFR + VFR | Advisory airspace | Low ARC |
| **G** | No control | VFR only | Uncontrolled airspace | Lowest ARC |

### **Γιατί λέει "SORA 2.5" αν εφαρμόζεται και στο 2.0;**

**Ιστορικό:**
1. **SORA 1.0 (2017):** Δεν είχε ACE framework
2. **SORA 2.0 (2019):** Βασίζονταν μόνο σε "Controlled/Uncontrolled"
3. **SORA 2.5 (2023):** Εισήγαγε το **ACE framework** με ICAO classes
4. **EASA AMC1 (2024):** Ενημέρωσε το SORA 2.0 να υποστηρίζει ACE

**Αποτέλεσμα:**  
Τώρα **ΚΑΙ ΤΑ ΔΥΟ SORA 2.0 ΚΑΙ 2.5** χρησιμοποιούν ACE categories!

---

## ✅ Backend Fixes Applied

### **Διορθώσεις στο .NET Backend:**

1. **ARC Field Mapping:**
   ```csharp
   // SORA 2.0: Converts meters to feet
   AltitudeAglFt = env.MaxHeightAGL * 3.28084
   
   // SORA 2.5: Already in meters
   AltitudeAglM = env.MaxHeightAGL
   ```

2. **SAIL Field Mapping:**
   ```csharp
   // BEFORE (WRONG)
   FinalARC = ToARCLabel(input.ResidualARC)
   
   // AFTER (CORRECT)
   ResidualARC = ToARCLabel(input.ResidualARC)
   ```

3. **Files Modified:**
   - ✅ `PythonCalculationClient.cs` - Updated DTOs
   - ✅ `SORAOrchestrationService.cs` - Fixed request creation
   - ✅ `ProxoraController.cs` - Fixed SAIL endpoints (2 locations)

4. **Build Status:**
   ```
   Build succeeded.
     0 Warning(s)
     0 Error(s)
   Time Elapsed 00:00:12.61
   ```

---

## 🎯 Επόμενα Βήματα

1. ✅ **Rebuild completed** - Backend με όλες τις διορθώσεις
2. 🔄 **Restart .NET backend** - Για να φορτώσει τις αλλαγές
3. 🧪 **Test Mission Planner** - End-to-end SORA workflow
4. 📝 **Validate Special Zones** - Έλεγχος νέων checkboxes
5. 🚀 **Create unified startup script** - One-click launch

---

## 📞 Σύνοψη Απαντήσεων

| Ερώτηση | Απάντηση |
|---------|----------|
| **Τι είναι τα 2 πεδία ύψους;** | AGL (για ARC) + AMSL (για FL600) - **Χρειάζονται και τα δύο!** |
| **Γιατί λέει SORA 2.5 ACE;** | Ιστορικός λόγος - τώρα διορθώθηκε σε "ACE Categories" (applies to all) |
| **Πού είναι τα Special Zones;** | Προστέθηκαν! Τώρα υπάρχουν όλα τα 6 zones (Mode-S, TMZ, RMZ, Danger, Prohibited, Restricted) |
| **Χρειάζονται ACE στο SORA 2.0;** | **ΝΑΙ!** Σύμφωνα με EASA AMC1, το ACE framework εφαρμόζεται σε όλες τις εκδόσεις SORA |

---

**Build Status:** ✅ Successful  
**Backend Ready:** ✅ Yes (needs restart)  
**UI Updated:** ✅ Special zones + ACE label fixed  
**Next:** Restart backend and test! 🚀
