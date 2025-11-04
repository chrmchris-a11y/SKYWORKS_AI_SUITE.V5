# 🤖 DUAL-AGENT WORKFLOW — Claude + Copilot

## 🇬🇷 Οδηγός Χρήσης

### ✅ Τι Ολοκληρώθηκε

1. **MCP Server** (`skyworks-sora-mcp-server`)
   - ✅ Compiled και verified
   - ✅ Περιέχει 23 EASA/JARUS documents
   - ✅ Tools: get_grc_table, calculate_sail, apply_mitigation, get_oso_requirements, validate_floor_rule, knowledge_check

2. **VS Code Extension** (`vscode-skyworks-assistant`)
   - ✅ Installed και rebuilt
   - ✅ Welcome panel με 4 buttons
   - ✅ Greek prompt για Claude Sonnet 4
   - ✅ Auto-start disabled (δεν ανοίγουν docs αυτόματα)
   - ✅ Status tracking μέσω PROJECT_STATUS.json

3. **Documentation**
   - ✅ PROJECT_PHASES_12.md - Πλήρες 12-φασικό πλάνο (117 βήματα)
   - ✅ CLAUDE_WORKFLOW.md - Task allocation (ποιος κάνει τι)
   - ✅ CLAUDE_PROMPT_TEMPLATE.txt - Greek template

---

## 🚀 Πώς να Ξεκινήσεις

### Βήμα 1: Reload VS Code

Press `F1` → type "Developer: Reload Window" → Enter

Αυτό θα φορτώσει το ανανεωμένο extension με το Greek Claude prompt.

---

### Βήμα 2: Άνοιξε το Welcome Panel

Θα ανοίξει αυτόματα μετά το reload. Αν όχι:

- Click στο Skyworks icon στη sidebar
- Ή press `Ctrl+Shift+P` → "Skyworks: Show Welcome Panel"

---

### Βήμα 3: Επίλεξε Workflow

Θα δεις 4 κουμπιά:

#### 🎯 **Full Context Start (GitHub Copilot)**
- Φορτώνει ολόκληρο το 12-φασικό πλάνο
- Ανοίγει το Copilot chat (δεξιά sidebar)
- **Χρήση:** Για integration, tests, small fixes

#### 🛡️ **Start with Guardrails (GitHub Copilot)**
- Όπως το Full Context αλλά με citation policy
- **Χρήση:** Όταν θέλεις references σε EASA/JARUS docs

#### 🧠 **Start Claude Sonnet 4 (Continue Extension)**
- **ΝΕΟΣ!** Greek prompt για Claude
- Ανοίγει το Continue chat (αριστερά sidebar)
- **Χρήση:** Για complex algorithms, PDF generation (30+ pages), full features

#### ⚡ **Quick Continue (GitHub Copilot)**
- Δείχνει μόνο το current status
- **Χρήση:** Γρήγορες ερωτήσεις

---

## 🧠 Πότε να Χρησιμοποιείς τον Claude (Continue)

### ✅ Χρήση Claude για:

- **Phase 7** — Operational Manual Generation (30-40 σελίδες PDF)
  - SORA I–VI Reports
  - PDRA 01/02 Templates
  - STS 01/02 Templates

- **Phase 9** — Final Report Generation με GREEN/YELLOW/RED approval
  - Multi-page reports
  - Complex formatting
  - SAIL decrease suggestions

- **Phase 11** — Complex Architecture
  - Docker setup
  - CI/CD pipelines
  - Monitoring & alerting

- **Phase 12** — Subscription System
  - Payment integration
  - License key generation
  - Birthday/nameday auto-greetings

- **Οποιοδήποτε Complex Algorithm**
  - Step 42 (OSO requirements)
  - ARC calculations
  - SAIL formulas με mitigations

---

## 🤖 Πότε να Χρησιμοποιείς τον Copilot

### ✅ Χρήση Copilot για:

- MCP server management
- Integration του κώδικα που έγραψε ο Claude
- Unit tests
- Small fixes & debugging
- Quick updates σε existing files
- Validation & error checking

---

## 🔄 Workflow — Claude → Copilot

### Παράδειγμα: Step 61 (Generate SORA I Report)

#### 1️⃣ **Ξεκίνα με Claude**

Click το button **"🧠 Start Claude Sonnet 4"** στο Welcome panel.

Ο Claude θα:
- Διαβάσει το PROJECT_PHASES_12.md
- Θα βρει το Step 61
- Θα σου ρωτήσει στα Ελληνικά:
  - "Να ξεκινήσω από το μηδέν ή να συνεχίσω υπάρχοντα κώδικα;"
  - "Ποια αρχεία πρέπει να διαβάσω για το Βήμα 61;"
  - "Υπάρχουν συγκεκριμένες απαιτήσεις ή περιορισμοί;"

#### 2️⃣ **Απάντησέ του στα Ελληνικά**

Π.χ.:
```
"Ξεκίνα από το μηδέν. Διάβασε αυτά τα αρχεία:
1. Backend/Services/SORAPDFService.cs
2. ContextPacks/SORA_25_MainBody/SORA_2.5_COMPLETE.md
3. Backend/Models/OperationManualData.cs

Θέλω να δημιουργήσεις 30-σέλιδο PDF report για SORA I
με πλήρη EASA compliance."
```

#### 3️⃣ **Claude γράφει τον κώδικα**

Ο Claude θα γράψει:
- `SoraReportGenerator.cs` (Core logic)
- `SoraIPDFTemplate.cshtml` (Template)
- Unit tests
- Documentation σε comments

Όταν τελειώσει, θα σου πει:

```
✅ ΟΛΟΚΛΗΡΩΘΗΚΕ το Βήμα 61!

📋 Τι έγινε:
- Δημιούργησα SoraReportGenerator.cs με PDF rendering engine
- Προσθέτω SORA I template με 8 sections
- Υλοποίησα automatic table generation για GRC/ARC

📤 ΕΠΟΜΕΝΟ ΒΗΜΑ — Μεταφορά στον Copilot:
1. Άνοιξε το Copilot chat (δεξιά sidebar)
2. Πες του: 'Claude τελείωσε το Βήμα 61. Κάνε integration και tests.'
3. Κάνε paste τον κώδικα που σου έδωσα παραπάνω
```

#### 4️⃣ **Μετάφερε στον Copilot**

Άνοιξε το **Copilot chat** (δεξιά sidebar).

Πες του:
```
Claude τελείωσε το Βήμα 61 (SORA I PDF Report). 
Κάνε integration και tests.

[Κάνε paste τον κώδικα που σου έδωσε ο Claude]
```

Ο Copilot θα:
- Δημιουργήσει τα files στα σωστά directories
- Θα κάνει integration με existing services
- Θα γράψει unit tests
- Θα update το PROJECT_STATUS.json (Step 61 → 62)

---

## 📋 Task Allocation Matrix

| Phase | Task | Agent | Sidebar |
|-------|------|-------|---------|
| 5 | Step 43 (OSO Framework UI) | Claude | Αριστερά |
| 5 | Integration + Tests | Copilot | Δεξιά |
| 6 | Step 51-60 (All phases) | Claude + Copilot | Dual |
| 7 | Step 61-70 (PDF Reports) | **Claude** | Αριστερά |
| 8 | Step 71-80 (i18n) | Copilot | Δεξιά |
| 9 | Step 81-90 (Final Reports) | **Claude** | Αριστερά |
| 10 | Step 91-100 (Testing) | Copilot | Δεξιά |
| 11 | Step 101-110 (DevOps) | **Claude** | Αριστερά |
| 12 | Step 111-117 (Subscriptions) | **Claude** | Αριστερά |

---

## 🔴 ΣΗΜΑΝΤΙΚΟ — Κανόνες

### ❌ ΜΗΝ

- ΜΗΝ χρησιμοποιείς τον Claude για small fixes (χρησιμοποίησε Copilot)
- ΜΗΝ χρησιμοποιείς τον Copilot για 30+ σελίδες PDF (χρησιμοποίησε Claude)
- ΜΗΝ τους βάζεις να δουλεύουν ταυτόχρονα στο ίδιο file
- ΜΗΝ ξεχνάς να update το PROJECT_STATUS.json μετά από κάθε βήμα

### ✅ ΝΑΙ

- ΝΑΙ χρησιμοποίησε τον Claude για complex logic και architecture
- ΝΑΙ χρησιμοποίησε τον Copilot για integration και tests
- ΝΑΙ ακολούθησε το workflow: Claude → User → Copilot
- ΝΑΙ ρώτα τον Claude ερωτήσεις στα Ελληνικά

---

## 🎯 Current Status

Τρέχον Status: **Phase 5, Step 43**

Επόμενο Βήμα: **Step 44** (Δες PROJECT_PHASES_12.md)

---

## 🆘 Troubleshooting

### Το Welcome panel δεν ανοίγει αυτόματα

**Λύση:**
- Press `Ctrl+Shift+P`
- Type "Skyworks: Show Welcome Panel"
- Enter

### Ο Claude δεν μιλάει Ελληνικά

**Λύση:**
- Πες του: "Μίλα μου στα Ελληνικά σε παρακαλώ"
- Ή κάνε reload του Continue extension

### Το PROJECT_STATUS.json δεν ενημερώνεται

**Λύση:**
- Πες στον Copilot: "Update PROJECT_STATUS.json to Step X"
- Ή άνοιξε το file manually και άλλαξε το

### Extension errors μετά το rebuild

**Λύση:**
- Press `F1` → "Developer: Reload Window"
- Αν δεν λύσει το πρόβλημα, run: `pwsh Tools/install-skyworks-extension.ps1`

---

## 📖 Related Docs

- `PROJECT_PHASES_12.md` — Full 12-phase plan (117 steps)
- `CLAUDE_WORKFLOW.md` — Task allocation details
- `PROJECT_ONBOARDING.md` — Tech stack & architecture
- `AI_WARMUP.md` — How to use MCP tools
- `SESSION_EVIDENCE.md` — Track your work

---

## 🎉 Ready to Start!

1. **Reload VS Code** (F1 → Developer: Reload Window)
2. **Click "🧠 Start Claude Sonnet 4"** για complex tasks
3. **Click "🎯 Full Context Start"** για integration/tests
4. **Follow the workflow:** Claude writes logic → Copilot integrates

**Καλή επιτυχία με το SKYWORKS project! 🚀**
