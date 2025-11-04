# 🚀 SKYWORKS MISSION PLANNER - Desktop App

## ΤΙ ΔΗΜΙΟΥΡΓΗΘΗΚΕ

✅ **Desktop Shortcut**: "Skyworks Mission Planner.lnk" στο Desktop σου
- **Διπλό κλικ** → Ανοίγει αυτόματα ο server + browser
- **Κλείσιμο παραθύρου** → Σταματάει ο server

## ΠΩΣ ΝΑ ΤΟ ΧΡΗΣΙΜΟΠΟΙΗΣΕΙΣ

### Βήμα 1: Άνοιξε την εφαρμογή
1. Πήγαινε στο **Desktop**
2. **Διπλό κλικ** στο "Skyworks Mission Planner"
3. Θα ανοίξει ένα παράθυρο PowerShell (μην το κλείσεις!)
4. Μετά από 2 δευτερόλεπτα θα ανοίξει **αυτόματα** ο browser

### Βήμα 2: Χρησιμοποίησε την εφαρμογή
- **URL**: `http://localhost:8080/Pages/mission.html`
- Αν δεν ανοίξει αυτόματα, αντίγραψε το URL και πήγαινε χειροκίνητα

### Βήμα 3: Όταν τελειώσεις
- **Κλείσε** το παράθυρο PowerShell (ή πάτα `Ctrl+C`)
- Αυτό σταματάει τον server

## ΤΙ ΕΧΕΙ ΦΤΙΑΧΤΕΙ ΚΑΙ ΕΙΝΑΙ ΕΤΟΙΜΟ

### ✅ Οργάνωση UI
- Population Density τώρα είναι **κάτω από το Mission Type**
- Drone Selection **κάτω από το Population Density**
- Όλα τα πεδία είναι σωστά τοποθετημένα

### ✅ Μεταφράσεις (EN/EL)
- Language switcher **πάνω δεξιά** (σημαίες)
- Όλες οι ετικέτες μεταφράζονται
- OSO ονόματα και περιγραφές στα Ελληνικά (απλοποιημένες)
- Dropdown επιλογές μεταφράζονται
- Tooltips μεταφράζονται

### ✅ Drone Selection
- **64 drones** στο dropdown (από το Backend API)
- Όταν επιλέγεις drone:
  - Auto-fill: MTOM, Max Dimension, Max Speed
  - Υπολογίζεται αυτόματα το **Kinetic Energy (KE)**
  - Τα manual input πεδία **κρύβονται** (γιατί δεν χρειάζονται)
- Αν διαλέξεις "Manual Input (Other)":
  - Τα πεδία **εμφανίζονται**
  - Μπορείς να τα γεμίσεις χειροκίνητα
  - Έχουν toggle "Edit manually" για να τα ξεκλειδώσεις

### ✅ Live Calculations
- **iGRC (intrinsic GRC)**:
  - Υπολογίζεται από: Drone specs + Population Density
  - Σύμφωνα με SORA 2.5 Table 2
  - Ενημερώνεται **live** καθώς αλλάζεις inputs
- **ARC (Air Risk Class)**:
  - Υπολογίζεται από: Airspace Control + Location Type + Environment + Typicality
  - Simplified logic (το Backend έχει την πλήρη λογική)
  - Ενημερώνεται **live**

### ✅ OSO Display
- Clickable badges για κάθε OSO
- Κάθε badge δείχνει:
  - Όνομα OSO (μεταφρασμένο)
  - Επίπεδο απαίτησης: NR/Low/Medium/High (μεταφρασμένο)
- **Πάτα πάνω** σε ένα OSO:
  - Εμφανίζεται **tooltip** με πλήρη περιγραφή στα Ελληνικά
  - Απλοποιημένο κείμενο (όχι τεχνική ορολογία)

### ✅ Population Density Helper
- Κάτω από το dropdown εμφανίζεται **contextual help**
- Εξηγεί τι σημαίνει κάθε επιλογή (π.χ. "< 5 people/km²")
- Μεταφράζεται στα Ελληνικά/Αγγλικά

### ✅ Professional UI
- Raw JSON **κρυμμένο** σε collapsible "Developer details"
- Live clock **πάνω** με brand colors
- Σωστή ροή: Mission Type → Population → Drone → SORA inputs → Results

## ΤΙ ΔΕΝ ΕΧΕΙ ΟΛΟΚΛΗΡΩΘΕΙ ΑΚΟΜΑ

### ⚠️ Backend API
Το Frontend είναι **έτοιμο**, αλλά για τα πλήρη calculations χρειάζεται το Backend:

**Για να το τρέξεις:**
```powershell
cd Backend
dotnet run --project src/Skyworks.Api
```

**Τι κάνει:**
- Endpoint για drone catalog: `http://localhost:5210/api/drones/dropdown`
- Endpoint για SORA evaluation: `http://localhost:5210/api/sora/evaluate`
- Endpoints για PDRA/STS categories

**Αν δεν τρέχει:**
- Το drone dropdown θα είναι άδειο
- Το "Run Evaluation" button δεν θα δουλεύει
- Τα live calculations (iGRC/ARC) **θα δουλεύουν** (client-side)

### ⚠️ Logo
Το logo (`skyworks-logo.png`) δεν υπάρχει στο `Frontend/assets/`
- Βάλε ένα PNG logo εκεί
- Ή αφαίρεσε τη γραμμή από το `mission.html`

## TROUBLESHOOTING

### "Δεν ανοίγει ο browser"
➡️ Άνοιξέ τον χειροκίνητα και πήγαινε: `http://localhost:8080/Pages/mission.html`

### "Το drone dropdown είναι άδειο"
➡️ Το Backend API δεν τρέχει. Ξεκίνησέ το:
```powershell
cd Backend
dotnet run --project src/Skyworks.Api
```

### "Βλέπω raw keys (π.χ. dropdown.environment.urban)"
➡️ Hard refresh: `Ctrl+Shift+R` ή `Ctrl+F5`

### "Δεν φορτώνουν οι μεταφράσεις"
➡️ Σίγουρα ανοίγεις μέσω `http://localhost:8080` και όχι `file://`;

### "Port 8080 already in use"
➡️ Κάποιος άλλος server τρέχει:
- Κλείσε όλα τα παράθυρα PowerShell
- Ή άλλαξε port στο script (π.χ. 8081)

## ΕΠΟΜΕΝΑ ΒΗΜΑΤΑ (προαιρετικά)

1. **Ολοκλήρωση Backend**:
   - Έλεγχος calculations για SORA 2.0 & 2.5
   - Validation των PDRA/STS rules
   - Database για αποθήκευση missions

2. **Πλήρεις OSO Descriptions**:
   - Προσθήκη των official EASA/JARUS κειμένων
   - Structured content per OSO ID

3. **Professional Styling**:
   - CSS polish για όλα τα sections
   - Responsive design για tablets/mobile
   - Dark mode theme

4. **Testing**:
   - Unit tests για calculations
   - Integration tests για API
   - End-to-end tests για UI flows

## ΣΥΝΟΨΗ

✅ **Έτοιμο τώρα:**
- Desktop launcher (διπλό κλικ)
- Frontend με live calculations (iGRC, ARC)
- Πλήρεις μεταφράσεις EN/EL
- Drone selection με auto-fill
- OSO tooltips με Ελληνικές επεξηγήσεις
- Professional UI flow

⚠️ **Χρειάζεται (για production):**
- Backend API running (για drone data & full evaluation)
- Logo file στο assets/
- Final testing & polish
