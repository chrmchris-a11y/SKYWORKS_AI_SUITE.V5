<!-- Phase1 Step5 — Skyworks V5 -->
# Context Packs — Training Center

## Τι είναι τα Context Packs;

Τα **Context Packs** είναι markdown αρχεία που περιέχουν επιλεγμένα αποσπάσματα από το EASA/SORA corpus, οργανωμένα ανά θέμα (π.χ. GRC, ARC, SAIL, OSO).

Χρησιμοποιούνται για:
- **GitHub Copilot Chat**: Attach via **Add context (#)** για contextual Q&A
- **Spark Prompts**: Paste σχετικά αποσπάσματα για targeted responses

---

## Δομή Corpus

**Relative Path (in-repo):**
```
KnowledgeBase/EASA DOCS SPLIT CHUNKS
```

**Υποστηριζόμενα formats:**
- `.md`, `.txt`, `.jsonl` (keys: text, source), `.csv` (cols: text, source)

---

## Πώς να δημιουργήσεις ένα Context Pack

### 1. Ρύθμιση (config.yaml)

Επεξεργάσου το `Tools/TrainingCenter/config.yaml`:

```yaml
topics:
  - name: "GRC"
    keywords: ["ground risk", "population density", "annex a"]
    max_chars: 50000
```

### 2. Εκτέλεση Generator

```bash
# Single topic
py -3 Tools/TrainingCenter/make_context_pack.py --topic GRC

# All topics
py -3 Tools/TrainingCenter/make_context_pack.py --all
```

### 3. Output

Το pack δημιουργείται εδώ:
```
ContextPacks/<topic>/pack.md
```

---

## Πώς να χρησιμοποιήσεις τα Packs

### GitHub Copilot Chat

1. Άνοιξε το Copilot Chat panel
2. Κάνε κλικ στο **Add context (#)**
3. Επίλεξε **#file** και βρες το pack: `ContextPacks/GRC/pack.md`
4. Ρώτα ερωτήσεις σχετικά με το θέμα

**Παράδειγμα:**
```
#file:ContextPacks/GRC/pack.md
Explain the GRC calculation methodology for urban areas.
```

---

### Spark Prompts

1. Άνοιξε το pack: `ContextPacks/GRC/pack.md`
2. Copy σχετικό απόσπασμα (π.χ. 500-1000 chars)
3. Paste στο Spark prompt με context

**Παράδειγμα:**
```
Context:
[Paste excerpt from pack.md]

Question:
Calculate GRC for a 25kg drone over a medium-density area.
```

---

## Αυτοματοποίηση (Scheduled Tasks)

Για να δημιουργήσεις 3 daily tasks (08:00, 14:00, 20:00):

```cmd
cd Tools\TrainingCenter
create_schedules.cmd
```

Αυτό δημιουργεί Windows Task Scheduler jobs που ανανεώνουν τα packs 3x ημερησίως.

---

## Versioning & Traceability

- Κάθε pack περιέχει **Source** paths για κάθε απόσπασμα
- Generated timestamp στην κορυφή κάθε pack
- Config version tracked στο `config.yaml`

---

## Troubleshooting

**Problem:** Generator fails with "No chunks found"  
**Solution:** Έλεγξε ότι το corpus path είναι σωστό: `KnowledgeBase/EASA DOCS SPLIT CHUNKS`

**Problem:** Pack is empty  
**Solution:** Διεύρυνε τα keywords ή αύξησε το `max_chars` στο config

**Problem:** PyYAML not found  
**Solution:** Generator χρησιμοποιεί fallback parser. Optional: `pip install pyyaml`

---

**End of README**
