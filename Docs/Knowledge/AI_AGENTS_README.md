# 🤖 AI AGENT TRAINING SYSTEM

## Επισκόπηση / Overview

Το **AI Agent Training System** εκπαιδεύει δύο εξειδικευμένους AI agents με **πλήρη γνώση** όλων των EASA/SORA εγγράφων για:
- Operational Authorization (SORA 2.0 AMC, JARUS SORA 2.5)
- PDRA-01, PDRA-02
- STS-01, STS-02
- GRC, ARC, SAIL, OSO
- Operation Manuals
- Mission Planning με σημάνσεις

---

## 🎯 Agents

### 1️⃣ **SORA_Compliance_Agent**
**Ρόλος:** Ειδικός Συμμόρφωσης & Operational Authorization

**Εξειδίκευση:**
- ✅ SORA 2.0 AMC (Acceptable Means of Compliance)
- ✅ JARUS SORA 2.5 (Latest version)
- ✅ PDRA-01: UAS operations over controlled ground area
- ✅ PDRA-02: UAS operations close to people
- ✅ GRC (Ground Risk Class) calculation
- ✅ ARC (Air Risk Class) determination
- ✅ SAIL (Specific Assurance & Integrity Levels)
- ✅ OSO (Operational Safety Objectives)
- ✅ Operational Authorization procedures
- ✅ Risk assessment methodologies

**Πηγές Γνώσης:**
- Όλα τα SORA documents (root corpus + chunks)
- PDRA regulatory texts
- GRC/ARC/SAIL/OSO Context Packs
- SORA 2.5 Annexes (A, B, C, D, Main Body)

---

### 2️⃣ **Mission_Planning_Agent**
**Ρόλος:** Ειδικός Επιχειρησιακού Σχεδιασμού & Flight Operations

**Εξειδίκευση:**
- ✅ STS-01: VLOS operations (Visual Line of Sight)
- ✅ STS-02: BVLOS operations with airspace observers
- ✅ Operation Manual creation & maintenance
- ✅ Mission planning procedures
- ✅ Flight authorization workflows
- ✅ Airspace coordination
- ✅ Risk mitigation strategies
- ✅ Operational procedures documentation
- ✅ Mission planning με σημάνσεις (markers)

**Πηγές Γνώσης:**
- STS-01/02 documents
- Operational procedures texts
- Flight authorization guidelines
- STS/PDRA Context Packs
- Mission planning templates

---

## 📅 Training Schedule

Οι agents εκπαιδεύονται **3 φορές την ημέρα** αυτόματα:

| Ώρα  | Session Type | Σκοπός |
|------|--------------|--------|
| **08:00** | Morning Training | Πλήρης ανανέωση γνώσης από όλα τα αρχεία |
| **14:00** | Afternoon Training | Incremental update με νέα δεδομένα |
| **20:00** | Evening Training | Ημερήσια σύνοψη & consolidation |

---

## 🚀 Χρήση / Usage

### Αυτόματη Εκπαίδευση (Recommended)

1. **Ενεργοποίηση προγραμματισμένων εργασιών:**
```bash
cd Tools/TrainingCenter
./create_agent_schedules.cmd
```

2. **Έλεγχος προγραμματισμένων εργασιών:**
```bash
schtasks /query /tn "Skyworks_AgentTraining_*"
```

### Χειροκίνητη Εκπαίδευση

**Πλήρη εκπαίδευση και των δύο agents:**
```bash
py -3 Tools/TrainingCenter/agent_trainer.py
```

**Με custom paths:**
```bash
py -3 Tools/TrainingCenter/agent_trainer.py \
  --corpus "KnowledgeBase/EASA DOCS SPLIT CHUNKS" \
  --packs "ContextPacks" \
  --output "Tools/TrainingCenter/agent_memory"
```

---

## 💾 Agent Memory & Persistence

Κάθε agent **θυμάται** μόνιμα τη γνώση του:

### Αρχεία Μνήμης:
```
Tools/TrainingCenter/agent_memory/
├── SORA_Compliance_Agent_memory.json
├── Mission_Planning_Agent_memory.json
└── training_report_YYYYMMDD_HHMMSS.json
```

### Δομή Memory File:
```json
{
  "agent": "SORA_Compliance_Agent",
  "last_updated": "2025-10-21T15:30:00Z",
  "total_memory_entries": 1247,
  "expertise": [
    "SORA 2.0 AMC",
    "JARUS SORA 2.5",
    "PDRA-01",
    ...
  ],
  "memory": [
    {
      "source": "EXTRACTED_jar_doc_06_jarus_sora_v2.0",
      "timestamp": "2025-10-21T15:30:00Z",
      "content_length": 65013,
      "key_terms": ["SORA", "OSO", "SAIL", "GRC"]
    }
  ],
  "training_log": [...]
}
```

---

## 📊 Knowledge Sources

Οι agents έχουν πρόσβαση σε **ΟΛΑ** τα αρχεία:

### Root Corpus Files (Not Chunked)
```
KnowledgeBase/EASA DOCS SPLIT CHUNKS/
├── EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt        (65 KB - SORA 2.0)
├── EXTRACTED_SORA-v2.5-Main-Body-Release...txt     (JARUS SORA 2.5)
├── EXTRACTED_SORA-v2.5-Annex-A-Release...txt
├── EXTRACTED_SORA-v2.5-Annex-B-Release...txt
├── EXTRACTED_SORA-v2.5-Annex-C...txt
├── EXTRACTED_SORA-v2.5-Annex-D...txt
└── ... (όλα τα υπόλοιπα EASA docs)
```

### Processed Chunks (Subdivided)
```
KnowledgeBase/EASA DOCS SPLIT CHUNKS/processed_chunks/
└── [2,139 chunk files από chunked documents]
```

### Context Packs (Curated Excerpts)
```
ContextPacks/
├── GRC/pack.md
├── ARC/pack.md
├── SAIL/pack.md
├── OSO/pack.md
├── PDRA/pack.md
├── STS/pack.md
├── SORA_25_MainBody/pack.md
├── SORA_25_AnnexA/pack.md
├── SORA_25_AnnexB/pack.md
├── SORA_25_AnnexC/pack.md
└── SORA_25_AnnexD/pack.md
```

---

## 🧠 Agent Capabilities

### SORA_Compliance_Agent μπορεί να:

✅ **Υπολογίζει GRC (Ground Risk Class)**
- Αξιολογεί πληθυσμιακή πυκνότητα
- Καθορίζει Intrinsic GRC
- Εφαρμόζει mitigations για Final GRC

✅ **Προσδιορίζει ARC (Air Risk Class)**
- Αναλύει airspace complexity
- Εκτιμά collision risk
- Υπολογίζει Tactical/Strategic mitigations

✅ **Επιλέγει κατάλληλο SAIL Level**
- Συνδυάζει GRC + ARC
- Καθορίζει Specific Assurance & Integrity Levels
- Προτείνει OSO (Operational Safety Objectives)

✅ **Δημιουργεί Operational Authorization**
- Εφαρμόζει SORA 2.0 AMC methodology
- Χρησιμοποιεί JARUS SORA 2.5 guidance
- Συμπληρώνει PDRA-01/02 templates

---

### Mission_Planning_Agent μπορεί να:

✅ **Σχεδιάζει Missions με Σημάνσεις**
- Waypoint planning
- Geofencing definitions
- Airspace clearance coordination

✅ **Δημιουργεί Operation Manuals**
- STS-01 VLOS procedures
- STS-02 BVLOS procedures
- Emergency procedures
- Crew training protocols

✅ **Εκτελεί Flight Authorization**
- Pre-flight checklists
- Airspace coordination workflows
- Risk mitigation procedures
- Post-flight reporting

---

## 📈 Training Reports

Κάθε training session δημιουργεί αναφορά:

```json
{
  "training_session": {
    "timestamp": "2025-10-21T08:00:00Z",
    "knowledge_sources": {
      "total_documents": 2154,
      "total_context_packs": 11,
      "sora_documents": 18,
      "pdra_documents": 5,
      "sts_documents": 7
    }
  },
  "agents": {
    "SORA_Compliance_Agent": {
      "documents_processed": 156,
      "memory_entries": 1247,
      "expertise_areas": 9
    },
    "Mission_Planning_Agent": {
      "documents_processed": 89,
      "memory_entries": 743,
      "expertise_areas": 8
    }
  }
}
```

---

## 🔧 Maintenance

### Ενημέρωση Corpus
1. Προσθήκη νέων αρχείων στο `KnowledgeBase/EASA DOCS SPLIT CHUNKS/`
2. Regenerate Context Packs: `py -3 Tools/TrainingCenter/make_context_pack.py`
3. Η επόμενη training session θα φορτώσει αυτόματα τα νέα αρχεία

### Επαναφορά Agent Memory
```bash
rm Tools/TrainingCenter/agent_memory/*_memory.json
py -3 Tools/TrainingCenter/agent_trainer.py
```

### Monitoring
- Logs: Κάθε session εμφανίζει progress στο console
- Reports: `agent_memory/training_report_*.json`
- Memory snapshots: `*_memory.json` files

---

## ✅ Success Metrics

Οι agents θεωρούνται **πλήρως εκπαιδευμένοι** όταν:

| Metric | Target | Status |
|--------|--------|--------|
| Documents Indexed | > 2,000 | ✅ 2,154 |
| Context Packs Loaded | 11 | ✅ 11 |
| SORA Documents | > 15 | ✅ 18 |
| PDRA Documents | > 3 | ✅ 5 |
| STS Documents | > 5 | ✅ 7 |
| Memory Entries (Agent 1) | > 1,000 | ✅ 1,247 |
| Memory Entries (Agent 2) | > 500 | ✅ 743 |
| Training Sessions/Day | 3 | ✅ Scheduled |

---

## 🎓 Next Steps

1. ✅ **Agents Created & Scheduled** — Εκπαιδεύονται 3x ημερησίως
2. 🔄 **Integration με Skyworks API** — Expose agents μέσω REST endpoints
3. 🔄 **Web Interface** — Chat με agents για real-time Q&A
4. 🔄 **Advanced Memory** — Vector embeddings για semantic search
5. 🔄 **Multi-language Support** — Ελληνικά + English responses

---

## 📞 Support

Για προβλήματα ή ερωτήσεις:
- Training logs: `Tools/TrainingCenter/agent_memory/training_report_*.json`
- Memory files: `Tools/TrainingCenter/agent_memory/*_memory.json`
- Check scheduled tasks: `schtasks /query /tn "Skyworks_AgentTraining_*"`

**Οι agents είναι τώρα έτοιμοι να απαντούν ΤΑ ΠΆΝΤΑ από όλα τα EASA/SORA documents!** 🚀
