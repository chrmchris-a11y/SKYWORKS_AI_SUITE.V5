# 💬 SKYWORKS Chat Commands — Training & Agent Monitoring

Χρησιμοποίησε το **@skyworks** chat participant μέσα στο VS Code για να παρακολουθείς training, να μιλάς με agents, και να βλέπεις project status.

---

## 🚀 Βασικές Εντολές

### Training Monitoring

**`/training-status`**  
Δείχνει την κατάσταση του τελευταίου training session:
- Πότε έγινε
- Πόσα documents φορτώθηκαν
- Πόση ώρα πήρε
- Status agents (memory entries)

**Παράδειγμα:**
```
@skyworks /training-status
```

**Απάντηση:**
```
✓ Last Training: 21/10/2025 20:31:30
✓ Duration: ~45 seconds
✓ Documents: 1,623 loaded + 11 context packs
✓ SORA_Compliance_Agent: 1,520 memory entries
✓ Mission_Planning_Agent: 2 memory entries
```

---

**`/training-logs [count]`**  
Εμφανίζει τα πιο πρόσφατα training logs (default: 5).

**Παράδειγμα:**
```
@skyworks /training-logs 3
```

**Απάντηση:**
```
📋 Recent Training Logs:
1. training_morning_20251021_203130.log (21/10 20:31) — 3.5 KB
2. training_now_20251021_202950.log (21/10 20:29) — 1.3 KB
3. training_evening_20251020_200005.log (20/10 20:00) — 7.2 KB
```

---

**`/training-report`**  
Δείχνει αναλυτική αναφορά από το τελευταίο training session:
- Timestamp
- Knowledge sources (σύνολο, ανά κατηγορία)
- Agent metrics (documents processed, memory entries)

**Παράδειγμα:**
```
@skyworks /training-report
```

**Απάντηση:**
```json
{
  "training_session": {
    "timestamp": "2025-10-21T17:31:35+00:00",
    "knowledge_sources": {
      "total_documents": 1623,
      "total_context_packs": 11,
      "sora_documents": 1510,
      "pdra_documents": 0,
      "sts_documents": 0
    }
  },
  "agents": {
    "SORA_Compliance_Agent": {
      "documents_processed": 1520,
      "memory_entries": 1520,
      "expertise_areas": 9
    },
    "Mission_Planning_Agent": {
      "documents_processed": 2,
      "memory_entries": 2,
      "expertise_areas": 8
    }
  }
}
```

---

### Agent Interaction

**`/agents`**  
Λίστα όλων των agents με expertise areas και memory status.

**Παράδειγμα:**
```
@skyworks /agents
```

**Απάντηση:**
```
🤖 Active Agents:

1. SORA_Compliance_Agent
   • Expertise: SORA 2.0 AMC, JARUS SORA 2.5, PDRA-01/02, GRC, ARC, SAIL, OSO
   • Memory: 1,520 entries
   • Last Updated: 21/10/2025 20:31:35

2. Mission_Planning_Agent
   • Expertise: STS-01, STS-02, Operation Manual, Mission Planning
   • Memory: 2 entries
   • Last Updated: 21/10/2025 20:31:35
```

---

**`/ask-sora <question>`**  
Ρωτά τον SORA_Compliance_Agent (operational authorization, GRC/ARC/SAIL, PDRA).

**Παράδειγμα:**
```
@skyworks /ask-sora What is the SAIL level for GRC=3 and ARC=b?
```

**Απάντηση:**
```
🤖 SORA_Compliance_Agent:

Based on JARUS SORA 2.5 methodology:
- GRC = 3 (Ground Risk Class)
- ARC = b (Air Risk Class)
→ SAIL = II (Specific Assurance & Integrity Level)

This requires:
- OSO#01-24 (Operational Safety Objectives)
- TMPR (Technical Mitigation of Population Risk) ≥ Medium
- Robustness level: Medium

📎 Sources:
- SORA-v2.5-Main-Body (Section 3.2)
- GRC Context Pack
```

---

**`/ask-mission <question>`**  
Ρωτά τον Mission_Planning_Agent (STS-01/02, operation manual, flight planning).

**Παράδειγμα:**
```
@skyworks /ask-mission What are the requirements for STS-01 operations?
```

**Απάντηση:**
```
🤖 Mission_Planning_Agent:

STS-01 (Visual Line of Sight operations) requires:
- Maximum take-off mass (MTOM) < 25 kg
- VLOS maintained at all times
- Controlled ground area
- No flight over assemblies of people
- Operational documentation (operations manual)

📎 Sources:
- STS Context Pack
- SORA-v2.5-Annex-A
```

---

### Project Navigation

**`/project-status`**  
Δείχνει που βρίσκεται το project (Phase/Step από MASTER_PROMPT).

**Παράδειγμα:**
```
@skyworks /project-status
```

**Απάντηση:**
```
📊 SKYWORKS AI SUITE — Project Status

✅ Phase 1: Foundation & Core Structure
   ✅ Step 1: Solution structure
   ✅ Step 2: REST API
   ✅ Step 3: Agent Communication System
   ✅ Step 4: Basic endpoints
   ✅ Step 5.1: Training Center (COMPLETE)
      • Two AI agents with persistent memory
      • 3x daily automated training
      • Logs and reports
   ⏳ Step 6: Compliance Framework (NEXT)
   ⏸️ Step 7: Security & Authentication
   ⏸️ Step 8: Web Interface
   ⏸️ Step 9: Documentation System
   ⏸️ Step 10: Integration Testing

Current Focus: Phase 1 Step 5.1 → Training Center validated and operational
Next Step: Phase 1 Step 6 → Configure Compliance Framework
```

---

**`/next-steps`**  
Προτείνει τα επόμενα βήματα (από MASTER_PROMPT workflow).

**Παράδειγμα:**
```
@skyworks /next-steps
```

**Απάντηση:**
```
🎯 Next Steps (from MASTER_PROMPT):

Immediate:
1. Phase 1 Step 6: Compliance Framework
   - Embed SORA/DCA rules into validation engine
   - Create risk assessment pipelines
   - Integrate with agents' knowledge base

2. Phase 1 Step 7: Security & Authentication
   - JWT/RBAC setup
   - User roles: Admin, Operator, Auditor
   - API key management

3. Phase 1 Step 8: Web Interface foundation
   - Blazor/React chat UI
   - Training dashboard
   - Agent Q&A interface

Type "@skyworks /help" for more commands.
```

---

## 🛠️ Utility Commands

**`/help`**  
Δείχνει όλες τις διαθέσιμες εντολές με σύντομη περιγραφή.

**`/run-training [session]`**  
Τρέχει on-demand training (session: morning/afternoon/evening/now).

**Παράδειγμα:**
```
@skyworks /run-training now
```

**Απάντηση:**
```
▶️ Starting training session "now"...
✓ Training completed successfully (45s)
📋 Log: Tools/TrainingCenter/logs/training_now_20251021_204500.log
📊 Report: Tools/TrainingCenter/agent_memory/training_report_20251021_204545.json
```

---

**`/view-log <filename>`**  
Εμφανίζει περιεχόμενο log file (tail 50 lines).

**Παράδειγμα:**
```
@skyworks /view-log training_morning_20251021_203130.log
```

---

**`/scheduled-tasks`**  
Δείχνει τις προγραμματισμένες εργασίες (08:00, 14:00, 20:00).

**Παράδειγμα:**
```
@skyworks /scheduled-tasks
```

**Απάντηση:**
```
📅 Scheduled Training Tasks:

1. Skyworks_AgentTraining_Morning
   • Time: 08:00 daily
   • Status: Ready
   • Next Run: 22/10/2025 08:00:00

2. Skyworks_AgentTraining_Afternoon
   • Time: 14:00 daily
   • Status: Ready
   • Next Run: 22/10/2025 14:00:00

3. Skyworks_AgentTraining_Evening
   • Time: 20:00 daily
   • Status: Ready
   • Next Run: 22/10/2025 20:00:00
```

---

## 💡 Tips

- Όλες οι εντολές ξεκινούν με `/` μέσα στο `@skyworks` chat.
- Μπορείς να γράψεις ελεύθερο κείμενο (π.χ. "Πες μου για το training") και το chat θα προτείνει σχετική εντολή.
- Για γρήγορη πρόσβαση, πάτα `Ctrl+Shift+I` (VS Code Chat) και γράψε `@skyworks`.

---

**Όλες οι εντολές με μια ματιά:**

| Εντολή | Σκοπός |
|--------|--------|
| `/training-status` | Κατάσταση τελευταίου training |
| `/training-logs [N]` | Πιο πρόσφατα N logs |
| `/training-report` | Αναλυτική αναφορά training |
| `/agents` | Λίστα agents με expertise |
| `/ask-sora <Q>` | Ερώτηση σε SORA agent |
| `/ask-mission <Q>` | Ερώτηση σε Mission agent |
| `/project-status` | Που βρίσκεται το project |
| `/next-steps` | Επόμενα βήματα workflow |
| `/run-training [S]` | Τρέξε training τώρα |
| `/view-log <file>` | Δείξε log file |
| `/scheduled-tasks` | Προγραμματισμένες εργασίες |
| `/help` | Εμφάνιση όλων των εντολών |

---

## 🧩 Mission & Compliance Utilities (in chat help as well)

| Εντολή | Σκοπός |
|--------|--------|
| `/plan-mission key=value ...` | Δημιουργεί Mission Briefing (ERP/Crew/Approvals) και αρχεία στο Docs/Compliance/Missions (προαιρετικά: updateMatrix=true) |
| `/evidence-sync [scenario=Facade|Stadium|...]` | Σάρωση Evidence και αυτόματη ενημέρωση OSO matrix |
| `/robustness-check [sail=II|III] [scenario=...]` | Έλεγχος robustness OSO (αναφορά μόνο) |
| `/make-binder operation=facade time=06:00 [mission=Docs/Compliance/Missions/Facade_0600.md]` | Συνθέτει Compliance Binder (briefing + αποσπάσματα OSO + approvals) σε Docs/Compliance/Binder |
| `/cga-generate name=Facade_0600 coords="lon,lat;lon,lat;..."` | Παράγει GeoJSON/KML για CGA και τα αποθηκεύει στο Docs/Compliance/CGA |
| `/what-if current=III target=II scenario=Facade` | Προτάσεις για μείωση SAIL (OSO/mitigations report) |
| `/permit-list [filter=active|expired|all] [entity=...] [type=...]` | Λίστα αδειών/γνωστοποιήσεων από permits.json |
| `/permit-add key=value ...` | Καταχώρηση νέας άδειας (type/entity/location/valid_from/valid_to/status/reference/notes) |
| `/off-peak scenario=Facade zone=urban [day=weekday|weekend]` | Προτεινόμενα off-peak χρονικά παράθυρα για μείωση GRC |
| `/risk-guard rssi=-55 wind=8 gust=14 flow=low` | Γρήγορη αξιολόγηση live ρίσκων (C2/καιρός/ροή πεζών) |
| `/email-bundle scenario=Facade time=06:00 to="ops@example.com,qa@example.com"` | Δημιουργεί ZIP (Binder+Reports) και το στέλνει email ή αποθηκεύει .eml draft |

---

🚀 **Καλή δουλειά με τους agents!**
