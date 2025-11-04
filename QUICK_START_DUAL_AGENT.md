# 🚀 QUICK START — Dual Agent Workflow (Claude + Copilot)

## ✅ ΤΙ ΟΛΟΚΛΗΡΩΘΗΚΕ

1. **Continue Extension Configuration**
   - ✅ Chat θα ανοίγει στο **PANEL** (πάνω από το terminal)
   - ✅ Όχι πια στην αριστερή sidebar
   - ✅ Θα βλέπεις το **project root** αριστερά (Explorer)

2. **Skyworks Extension Updated**
   - ✅ Button "🧠 Start Claude Sonnet 4" τώρα ανοίγει Continue PANEL
   - ✅ Greek prompt φορτωμένο
   - ✅ Automatic clipboard copy

3. **Layout Optimization**
   ```
   ┌──────────────────────────────────────────────────────────┐
   │ ΑΡΙΣΤΕΡΑ: Explorer (project files, folders)             │
   ├──────────────────────────────────────────────────────────┤
   │ ΚΕΝΤΡΟ: Editor (code files)                             │
   ├──────────────────────────────────────────────────────────┤
   │ ΠΑΝΩ PANEL: Continue Chat (Claude Sonnet 4) 🧠          │
   ├──────────────────────────────────────────────────────────┤
   │ ΚΑΤΩ: Terminal (PowerShell)                             │
   ├──────────────────────────────────────────────────────────┤
   │ ΔΕΞΙΑ SIDEBAR: Copilot Chat (GitHub Copilot) 🤖         │
   └──────────────────────────────────────────────────────────┘
   ```

---

## 🔄 ΠΩΣ ΝΑ ΞΕΚΙΝΗΣΕΙΣ (ΜΕΤΑ ΑΠΟ RESTART)

### Βήμα 1: Κλείσε το VS Code

Press **`Alt+F4`** ή click το **X** πάνω δεξιά.

---

### Βήμα 2: Άνοιξε το VS Code

**Επιλογή A — Από Desktop shortcut:**
```
Double-click: SKYWORKS_AI_SUITE.V5 (shortcut στο Desktop)
```

**Επιλογή B — Από terminal:**
```powershell
cd C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5
code .
```

**Επιλογή C — Από VS Code:**
```
File → Open Folder → επίλεξε: SKYWORKS_AI_SUITE.V5
```

---

### Βήμα 3: Περίμενε να Φορτώσει

Μόλις ανοίξει το VS Code:

1. **MCP Server θα ξεκινήσει αυτόματα** (background process)
2. **Welcome Panel θα ανοίξει αυτόματα** (Skyworks Assistant)
3. **Project Status θα ενημερωθεί** (αν υπάρχουν changes)

**ΣΗΜΑΝΤΙΚΟ:** Μην κλείσεις το Welcome panel! Το χρειάζεσαι για τα buttons.

---

### Βήμα 4: Άνοιξε το Copilot Chat (Δεξιά Sidebar)

Press **`Ctrl+Shift+I`** ή click το Copilot icon δεξιά.

Αυτό θα ανοίξει το **GitHub Copilot chat** στη δεξιά sidebar.

**Κράτα το ανοιχτό!** Είναι το "safety net" σου για instant help.

---

### Βήμα 5: Επίλεξε Workflow

Στο **Welcome Panel**, θα δεις 4 κουμπιά:

#### 🧠 **Start Claude Sonnet 4** (για Complex Tasks)

Click αυτό όταν θέλεις:
- Complex algorithms (OSO framework, ARC calculations, SAIL formulas)
- PDF generation (30+ σελίδες: SORA I–VI, PDRA, STS reports)
- Full feature implementation (multi-file changes)
- Architecture design (Docker, CI/CD, subscription system)

**Τι θα γίνει:**
1. Greek prompt θα γραφτεί στο clipboard
2. Continue chat θα ανοίξει **πάνω από το terminal** (PANEL)
3. Θα δεις μήνυμα: "🧠 Claude Sonnet 4 Ready! Paste (Ctrl+V)..."
4. Press `Ctrl+V` στο Continue chat
5. Ο Claude θα σε ρωτήσει στα Ελληνικά (3 ερωτήσεις)

---

#### 🚀 **Full Context Start** (για Integration/Tests)

Click αυτό όταν θέλεις:
- Integration του κώδικα που έγραψε ο Claude
- Unit tests
- Small fixes & debugging
- Quick updates σε existing files

**Τι θα γίνει:**
1. Full context prompt θα γραφτεί στο clipboard
2. Copilot chat θα ανοίξει **δεξιά** (sidebar)
3. Press `Ctrl+V` στο Copilot chat
4. Ο Copilot θα ξεκινήσει αμέσως

---

#### 🛡️ **Start with Guardrails** (για EASA Compliance)

Click αυτό όταν χρειάζεσαι:
- Mandatory citations από EASA/JARUS docs
- Evidence logging (SESSION_EVIDENCE.md)
- Full compliance validation

**Τι θα γίνει:**
- Όπως το Full Context Start
- PLUS: Υποχρεωτικά citations και knowledge_check calls

---

#### ⚡ **Quick Continue** (για Γρήγορες Ερωτήσεις)

Click αυτό όταν θέλεις:
- Να ελέγξεις το current status
- Γρήγορη ερώτηση χωρίς full context load

**Τι θα γίνει:**
- Μόνο status info + MCP tools
- Copilot chat θα ανοίξει δεξιά

---

## 🔄 Workflow Example — Step 44

### Σενάριο: Θέλεις να υλοποιήσεις το Step 44 (Complex OSO Algorithm)

#### 1️⃣ Click "🧠 Start Claude Sonnet 4"

Continue chat ανοίγει **πάνω από το terminal**.

Press **`Ctrl+V`** → Ο Claude θα δει:

```
🇬🇷 ΣΗΜΑΝΤΙΚΟ: Μίλα ΠΑΝΤΑ στα ΕΛΛΗΝΙΚΑ!

ΒΗΜΑ 1: ΚΑΤΑΛΑΒΕ ΤΟ PROJECT
- Διάβασε PROJECT_PHASES_12.md (117 βήματα)
- Διάβασε PROJECT_ONBOARDING.md
- Διάβασε CLAUDE_WORKFLOW.md

ΒΗΜΑ 2: ΕΛΕΓΞΕ STATUS (Phase 5, Βήμα 43)
ΒΗΜΑ 3: ΚΑΤΑΛΑΒΕ ΤΟΝ ΡΟΛΟ ΣΟΥ
ΒΗΜΑ 4: Η ΔΟΥΛΕΙΑ ΣΟΥ (Βήμα 44)
```

#### 2️⃣ Ο Claude θα Διαβάσει τα Αρχεία

Θα δεις:
```
[Claude διαβάζει: PROJECT_PHASES_12.md]
[Claude διαβάζει: PROJECT_ONBOARDING.md]
[Claude διαβάζει: CLAUDE_WORKFLOW.md]

✅ Κατάλαβα! Είμαστε στο Step 43 (OSO Framework — UI Integration).
   Επόμενο βήμα: Step 44 (OSO Complex Algorithms — M3 Penalty Calculation).
```

#### 3️⃣ Ο Claude θα σε Ρωτήσει (στα Ελληνικά)

```
Ερώτηση 1: Να ξεκινήσω από το μηδέν ή να συνεχίσω υπάρχοντα κώδικα;
Ερώτηση 2: Ποια αρχεία πρέπει να διαβάσω για το Βήμα 44;
Ερώτηση 3: Υπάρχουν συγκεκριμένες απαιτήσεις ή περιορισμοί;
```

#### 4️⃣ Απάντα στα Ελληνικά

Π.χ.:
```
1. Υπάρχει κώδικας στο Backend/Services/OSOService.cs — συνέχισε από εκεί.
2. Διάβασε:
   - Backend/Services/OSOService.cs
   - Backend/Models/OSORequirement.cs
   - ContextPacks/OSO/OSO_COMPLEX_ALGORITHMS.md
3. Θέλω να υλοποιήσεις το M3 penalty calculation για JARUS Table 2.5.
```

#### 5️⃣ Ο Claude Γράφει τον Κώδικα

Θα δεις:
```
[Claude διαβάζει τα 3 αρχεία που του έδωσες]
[Claude γράφει κώδικα...]

✅ ΤΕΛΕΙΩΣΕ!

📋 Τι έγινε:
- Τροποποίησα OSOService.cs με M3 penalty logic
- Πρόσθεσα CalculateM3Penalty() method
- Υλοποίησα JARUS Table 2.5 compliance check

📤 ΕΠΟΜΕΝΟ ΒΗΜΑ — Μεταφορά στον Copilot:
1. Άνοιξε το Copilot chat (δεξιά sidebar)
2. Πες του: 'Claude τελείωσε το Βήμα 44. Κάνε integration και tests.'
3. Κάνε paste τον κώδικα που σου έδωσα παραπάνω

[Ο κώδικας θα είναι εδώ...]
```

#### 6️⃣ Copy τον Κώδικα και Μετάφερέ τον στον Copilot

**ΣΗΜΑΝΤΙΚΟ:**
- Το Continue chat είναι **πάνω** (PANEL)
- Το Copilot chat είναι **ΔΕΞΙΑ** (SIDEBAR)

Press **`Ctrl+Shift+I`** για να ανοίξεις το Copilot chat δεξιά.

Γράψε:
```
Claude τελείωσε το Βήμα 44 (M3 Penalty Calculation). 
Κάνε integration και tests.

[PASTE ΤΟΝ ΚΩΔΙΚΑ ΕΔΩ]
```

#### 7️⃣ Ο Copilot Κάνει Integration

Ο Copilot θα:
```
✅ Δημιουργήσει/τροποποιήσει Backend/Services/OSOService.cs
✅ Προσθέσει unit tests στο tests/OSOServiceTests.cs
✅ Κάνει validation (dotnet build, dotnet test)
✅ Update στο PROJECT_STATUS.json (Step 44 → Step 45)
```

---

## 🎯 Layout Tips

### 📍 Πώς να Οργανώσεις το Παράθυρο

**Βέλτιστο Layout:**
```
1. ΑΡΙΣΤΕΡΑ: Explorer (πάντα ανοιχτό)
   - Βλέπεις το project structure
   - Drag & drop files
   - Right-click για context menu

2. ΚΕΝΤΡΟ: Editor
   - Κώδικας που δουλεύεις

3. ΠΑΝΩ PANEL: Continue Chat (Claude)
   - Resize με drag το κάτω border
   - Recommended height: 30% της οθόνης

4. ΚΑΤΩ: Terminal
   - dotnet build/test commands
   - git commands
   - PowerShell scripts

5. ΔΕΞΙΑ SIDEBAR: Copilot Chat
   - Toggle με Ctrl+Shift+I
   - Πάντα διαθέσιμο για instant help
```

**Keyboard Shortcuts:**
- `Ctrl+B` — Toggle Explorer (αριστερά)
- `Ctrl+J` — Toggle Terminal (κάτω)
- `Ctrl+Shift+I` — Toggle Copilot Chat (δεξιά)
- `Ctrl+K Ctrl+I` — Inline Copilot suggestions
- `F1` → "Continue: Focus" — Focus στο Continue panel

---

## 🆘 Troubleshooting

### ❌ Το Continue chat δεν ανοίγει στο panel

**Λύση:**
```powershell
# Βεβαιώσου ότι το setting είναι σωστό:
$settingsPath = "$env:APPDATA\Code\User\settings.json"
$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
Write-Host $settings.'continue.showChatInPanel'
# Πρέπει να δείχνει: True
```

Αν είναι `False` ή `null`:
```powershell
pwsh Tools/configure-continue-panel.ps1  # (θα το δημιουργήσω σε λίγο)
```

---

### ❌ Το Copilot chat δεν ανοίγει δεξιά

**Λύση:**
- Press `Ctrl+Shift+I`
- Ή: `F1` → type "GitHub Copilot: Open Chat" → Enter
- Ή: Click το Copilot icon στη δεξιά sidebar (chat bubble)

---

### ❌ Το Welcome panel έκλεισε και δεν ξέρω πώς να το ανοίξω

**Λύση:**
- Press `F1`
- Type: "Skyworks: Show Welcome Panel"
- Enter

---

### ❌ Το MCP server δεν ξεκινάει

**Λύση:**
```powershell
# Check αν τρέχει:
Get-Process | Where-Object { $_.Path -like "*node*" -and $_.CommandLine -like "*mcp*" }

# Αν δεν τρέχει:
cd skyworks-sora-mcp-server
npm run build
# Μετά reload VS Code: F1 → "Developer: Reload Window"
```

---

### ❌ Ο Claude δεν μιλάει Ελληνικά

**Λύση:**
Πες του:
```
Μίλα μου στα Ελληνικά σε παρακαλώ.
```

Αν εξακολουθεί να μιλάει Αγγλικά, κάνε:
```
F1 → "Continue: Restart Extension"
```

---

### ❌ Extension errors μετά το rebuild

**Λύση:**
```powershell
# Rebuild από την αρχή:
cd C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5
pwsh Tools/install-skyworks-extension.ps1

# Μετά:
F1 → "Developer: Reload Window"
```

---

## 📋 Quick Reference — Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+I` | Open/Close Copilot Chat (δεξιά) |
| `Ctrl+B` | Toggle Explorer (αριστερά) |
| `Ctrl+J` | Toggle Terminal (κάτω) |
| `F1` | Command Palette |
| `Ctrl+P` | Quick File Open |
| `Ctrl+Shift+P` | Same as F1 |
| `Ctrl+K Ctrl+I` | Inline Copilot suggestions |
| `Alt+F4` | Close VS Code |
| `Ctrl+W` | Close current tab |
| `Ctrl+Shift+T` | Reopen closed tab |

---

## 🎉 ΣΕ ΠΕΡΙΜΕΝΩ ΣΤΟ CHAT!

Μετά το restart:

1. **Claude Sonnet 4** = PANEL (πάνω από terminal) για complex tasks
2. **GitHub Copilot** = SIDEBAR (δεξιά) για integration/tests/instant help

**Αν έχεις οποιοδήποτε θέμα:**
- Άνοιξε το **Copilot chat** (δεξιά)
- Πες μου το πρόβλημα
- Θα το λύσω αμέσως! 🚀

---

**Ready? Κλείσε το VS Code τώρα και άνοιξέ το ξανά!**

Press `Alt+F4` → Μετά: Double-click το SKYWORKS_AI_SUITE.V5 shortcut → Περίμενε να φορτώσει → Click "🧠 Start Claude Sonnet 4" → Απόλαυσε! 🎯
