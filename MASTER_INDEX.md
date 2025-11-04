# 📚 SKYWORKS AI SUITE - MASTER INDEX

**Τι να διαβάσεις και πότε - Πλήρης οδηγός για AI Agents & Humans**

---

## 🚀 ΓΙΑ ΝΕΟΥΣ AI AGENTS: ΔΙΑΒΑΣΕ ΑΥΤΑ ΠΡΩΤΑ

### **Ελάχιστη Απαίτηση (5 λεπτά):**

1. **`README_NEW_CHAT.md`** ← Ξεκίνα εδώ - Template για κάθε νέα συνομιλία
2. **`PROJECT_ONBOARDING.md`** ← Τι είναι το SKYWORKS, τι κάνει, πώς δουλεύει

### **Βασική Εκπαίδευση (30 λεπτά):**

3. **`MCP_SERVER_GUIDE.md`** ← Πώς να χρησιμοποιείς τα 7 MCP tools
4. **`Docs/Knowledge/SORA_2_5_TABLES_REFERENCE.md`** ← Πυρήνας SORA 2.5 μεθοδολογίας

### **Εμβάθυνση (2 ώρες):**

5. **`Docs/Knowledge/SORA_2_0_TABLES_REFERENCE.md`** ← Ιστορικό πλαίσιο
6. **`Docs/Knowledge/OPERATIONS_MANUAL_STRUCTURE.md`** ← Part A-T framework
7. **`Docs/Knowledge/AIR_RISK_ARC_TMPR_REFERENCE.md`** ← Air risk calculations

### **Expert Level (Όλα τα 23 έγγραφα):**

8. **`KnowledgeBase/EASA DOCS SPLIT CHUNKS/`** ← Ολόκληρη η JARUS/EASA βιβλιοθήκη

---

## 🎯 ΓΙΑ ΑΝΘΡΩΠΟΥΣ: ΠΩΣ ΝΑ ΞΕΚΙΝΗΣΕΙΣ

### **Πρώτη Φορά στο Project:**

1. Διάβασε: **`PROJECT_ONBOARDING.md`**
2. Κατέβασε: MCP server με **`skyworks-sora-mcp-server/install.ps1`**
3. Ρύθμισε: VS Code settings.json (οδηγίες στο `MCP_SERVER_GUIDE.md`)

### **Κάθε Νέα Συνομιλία με AI:**

**Template:**
```
SKYWORKS PROJECT - Step [αριθμός].
Task: [περιγραφή]
Use MCP Server: skyworks-sora
Context: [αρχεία]
Expected output: [τι θέλω]
```

**Παράδειγμα:**
```
SKYWORKS PROJECT - Step 47.
Task: Validate GRC calculations
Use MCP Server: skyworks-sora
Context: Backend/Services/GRCCalculationService.cs
Expected output: Validation report
```

---

## 📁 ΑΡΧΕΙΑ ΑΝΑ ΚΑΤΗΓΟΡΙΑ

### **🚀 Quick Start (Διάβασε αυτά πρώτα)**

| Αρχείο | Σκοπός | Χρόνος |
|--------|--------|--------|
| `README_NEW_CHAT.md` | Template για νέα chat | 3 min |
| `PROJECT_ONBOARDING.md` | Πλήρης επισκόπηση project | 15 min |
| `MCP_SERVER_GUIDE.md` | Οδηγίες MCP tools | 10 min |

### **📖 PERMANENT MEMORY (Core Knowledge)**

| Αρχείο | Περιεχόμενο | Πότε να διαβάσεις |
|--------|-------------|-------------------|
| `SORA_2_5_TABLES_REFERENCE.md` | Table 2 (iGRC 7×5), M1A/B/C, SAIL, OSO | Βασική εκπαίδευση |
| `SORA_2_0_TABLES_REFERENCE.md` | Table 2 (GRC 7×4), M1/M2/M3, SAIL | Ιστορικό πλαίσιο |
| `OPERATIONS_MANUAL_STRUCTURE.md` | Part A-T, Compliance Matrix | Manual generation |
| `AIR_RISK_ARC_TMPR_REFERENCE.md` | Figure 4, 12 AECs, TMPR | Air risk tasks |

### **🛠️ MCP Server (Τεχνικά)**

| Αρχείο | Σκοπός | Για ποιον |
|--------|--------|-----------|
| `skyworks-sora-mcp-server/README.md` | MCP overview | Developers |
| `skyworks-sora-mcp-server/install.ps1` | Installation script | Humans |
| `skyworks-sora-mcp-server/src/index.ts` | Server code | Advanced users |
| `skyworks-sora-mcp-server/package.json` | Dependencies | Developers |

### **💻 Backend Code**

| Service | Σκοπός | SORA Reference |
|---------|--------|----------------|
| `Backend/Services/GRCCalculationService.cs` | Ground Risk | Table 2 |
| `Backend/Services/ARCCalculationService.cs` | Air Risk | Figure 4, 12 AECs |
| `Backend/Services/SAILService.cs` | SAIL Determination | Table 5 (2.0), Table 7 (2.5) |
| `Backend/Services/MitigationService.cs` | M1/M2/M3 + Floor Rule | Table 3, Annex B |
| `Backend/Services/OSOService.cs` | OSO Requirements | Table 6, Annex E |
| `Backend/Services/ContainmentService.cs` | CV/GRB | Annex A Section 5.2 |

### **🧪 Tests**

| Αρχείο | Τι ελέγχει | Πότε να τρέξεις |
|--------|-----------|-----------------|
| `Backend/tests/GRCCalculationTests.cs` | GRC formulas | After GRC changes |
| `Backend/tests/SAILTests.cs` | SAIL matrix | After SAIL changes |
| `Backend/tests/MitigationTests.cs` | Floor rule, M1/M2 | After mitigation changes |
| `Backend/tests/SORA25Tests.cs` | SORA 2.5 specific | SORA 2.5 migration |

### **� Reports & Cheat Sheets (Νέα)**

| Αρχείο | Περιεχόμενο | Χρήση |
|--------|-------------|-------|
| `SORA_2_5_CHEAT_SHEET.md` | Συνοπτική αναφορά Table 2/3/5 και mappings | Γρήγορο cross-check |
| `GRC_MITIGATION_MODELS_SORA_2_5_COMPLIANCE_REPORT.md` | Αναλυτική τεκμηρίωση πιστώσεων M1A/M1B/M1C/M2 | Συμμόρφωση & review |
| `ROBUSTNESS_LEVEL_ENUM_SORA_COMPLIANCE_REPORT.md` | Διευκρινίσεις για RobustnessLevel vs service logic | Τεκμηρίωση enum |
| `FIXES_1_TO_14_TEST_REPORT.md` | Συνολική αναφορά για Fixes #1–#14 (239/239 PASS) | Traceability |

### **�📚 EASA Documents (23 Αρχεία)**

| Document | Content | Lines | Priority |
|----------|---------|-------|----------|
| `EXTRACTED_SORA-v2.5-Main-Body.txt` | Core methodology 2.5 | ~8000 | **HIGH** |
| `EXTRACTED_SORA-v2.5-Annex-A.txt` | Operations Manual | ~1500 | **HIGH** |
| `EXTRACTED_SORA-v2.5-Annex-B.txt` | M1 Mitigations | ~1800 | **HIGH** |
| `EXTRACTED_SORA-v2.5-Annex-E.txt` | OSO Criteria | ~4500 | **HIGH** |
| `EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt` | SORA 2.0 Main | ~1500 | Medium |
| `EXTRACTED_Easy_Access_Rules_UAS.txt` | EU Regulations | 24739 | Medium |
| 17 more documents... | Various topics | Various | Low-Medium |

**Full list:** `KnowledgeBase/EASA DOCS SPLIT CHUNKS/`

---

## 🔧 MCP SERVER TOOLS - ΓΡΗΓΟΡΗ ΑΝΑΦΟΡΑ

### **7 Available Tools:**

```typescript
1. get_grc_table({ version, scenario, ua_size_column })
   → Returns: GRC value from Table 2

2. calculate_sail({ final_grc, residual_arc })
   → Returns: SAIL level (I-VI)

3. apply_mitigation({ version, intrinsic_grc, m1_level, m2_level, ... })
   → Returns: Final GRC with floor rule validation

4. get_oso_requirements({ sail })
   → Returns: Required OSOs for SAIL level

5. validate_floor_rule({ version, final_grc, scenario, ua_size_column })
   → Returns: ✅/❌ floor rule validation

6. search_sora_docs({ query, document? })
   → Returns: Search results from 23 EASA docs

7. get_operations_manual_structure({ part? })
   → Returns: Part A-T structure
```

### **4 Available Resources:**

```
skyworks://knowledge/sora-2.0-tables       → SORA 2.0 reference
skyworks://knowledge/sora-2.5-tables       → SORA 2.5 reference
skyworks://knowledge/operations-manual     → Part A-T structure
skyworks://knowledge/air-risk-arc-tmpr     → ARC/TMPR reference
```

---

## 📊 WORKFLOW ΓΙΑ ΣΥΝΗΘΙΣΜΕΝΕΣ ΕΡΓΑΣΙΕΣ

### **Task 1: Code Validation**

**Steps:**
1. Human: "SKYWORKS - Step X. Validate GRCCalculationService.cs"
2. AI reads: `GRCCalculationService.cs`
3. AI uses MCP: `get_grc_table({ version: "2.5", ... })`
4. AI compares: Code constants vs SORA tables
5. AI reports: ✅/❌ with corrections

**Files needed:**
- `Backend/Services/GRCCalculationService.cs`
- MCP tool: `get_grc_table`
- Reference: `SORA_2_5_TABLES_REFERENCE.md`

### **Task 2: Add Documentation**

**Steps:**
1. Human: "SKYWORKS - Step X. Add JARUS references to MitigationService.cs"
2. AI reads: `MitigationService.cs`
3. AI uses MCP: `search_sora_docs({ query: "M1 mitigation floor rule" })`
4. AI adds comments: `// SORA 2.5 Table 3: M1 High = -4 GRC`
5. AI validates: Code matches references

**Files needed:**
- `Backend/Services/MitigationService.cs`
- MCP tool: `search_sora_docs`
- Reference: `SORA_2_5_TABLES_REFERENCE.md`

### **Task 3: Run Tests**

**Steps:**
1. Human: "SKYWORKS - Step X. Run all tests"
2. AI executes: `dotnet test Skyworks.sln --verbosity minimal`
3. AI analyzes: Test output
4. AI cross-references: Failed tests with SORA tables (via MCP)
5. AI reports: Summary + fixes

**Files needed:**
- `Backend/Skyworks.sln`
- MCP tools: `get_grc_table`, `calculate_sail`, `validate_floor_rule`

### **Task 4: Generate Operations Manual**

**Steps:**
1. Human: "SKYWORKS - Step X. Generate Part B (Procedures)"
2. AI uses MCP: `get_operations_manual_structure({ part: "B" })`
3. AI reads: `OPERATIONS_MANUAL_STRUCTURE.md`
4. AI generates: Part B template
5. AI cross-references: OSO requirements (via MCP)

**Files needed:**
- MCP tool: `get_operations_manual_structure`
- Reference: `OPERATIONS_MANUAL_STRUCTURE.md`
- SORA Annex A (via MCP resource)

---

## 🎓 LEARNING PATHS

### **Path 1: Beginner AI Agent (1 hour)**

1. **Read:** `README_NEW_CHAT.md` (3 min)
2. **Read:** `PROJECT_ONBOARDING.md` (15 min)
3. **Read:** `MCP_SERVER_GUIDE.md` (10 min)
4. **Practice:** MCP tools (15 min)
   - `get_grc_table({ version: "2.5" })`
   - `calculate_sail({ final_grc: 4, residual_arc: "b" })`
5. **Read:** `SORA_2_5_TABLES_REFERENCE.md` (15 min)
6. **Test:** Validate one backend service (10 min)

**You're ready for:** Code validation, basic testing

### **Path 2: Intermediate AI Agent (4 hours)**

7. **Read:** `SORA_2_0_TABLES_REFERENCE.md` (30 min)
8. **Read:** `OPERATIONS_MANUAL_STRUCTURE.md` (30 min)
9. **Study:** Backend services (1 hour)
   - `GRCCalculationService.cs`
   - `SAILService.cs`
   - `MitigationService.cs`
10. **Run:** All tests + analyze results (1 hour)
11. **Compare:** SORA 2.0 vs 2.5 differences (1 hour)

**You're ready for:** Documentation, complex validation, manual generation

### **Path 3: Expert AI Agent (Full Knowledge)**

12. **Read:** All 23 EASA documents (8-12 hours)
13. **Study:** Multi-agent orchestration (2 hours)
14. **Deep dive:** OSO compliance workflows (2 hours)
15. **Master:** Floor rule edge cases (1 hour)
16. **Practice:** Complex scenarios (2 hours)

**You're ready for:** Everything, including new feature development

---

## 🚦 STATUS INDICATORS

**Για AI Agents:**

```
✅ COMPLETED - Fully implemented, tested, documented
🚧 IN PROGRESS - Partially complete, being worked on
📋 PLANNED - Designed but not yet implemented
⚠️ NEEDS ATTENTION - Known issues or gaps
```

**Current Project Status:**

- ✅ Backend services (GRC, ARC, SAIL, OSO, Mitigation, Containment)
- ✅ 100+ unit tests
- ✅ 4× PERMANENT MEMORY files
- ✅ 23× EASA documents ingested
- ✅ MCP Server created
- 🚧 SORA 2.5 migration (backend uses 2.0 baseline)
- 🚧 Code documentation with JARUS references
- 📋 Auto-generated operations manuals
- 📋 U-Space integration

---

## 🎯 SUCCESS CHECKLIST

**Πριν ξεκινήσεις εργασία:**

- [ ] MCP Server installed (`skyworks-sora` in VS Code)
- [ ] Read `PROJECT_ONBOARDING.md`
- [ ] Read `MCP_SERVER_GUIDE.md`
- [ ] Tested MCP tool: `get_grc_table`
- [ ] Read relevant PERMANENT MEMORY file
- [ ] Understand current step number

**Μετά την ολοκλήρωση εργασίας:**

- [ ] Code validated via MCP tools
- [ ] Tests passed (`dotnet test`)
- [ ] JARUS references added
- [ ] Documentation updated
- [ ] No floor rule violations
- [ ] ≤250g special cases handled
- [ ] Completion report provided

---

## 📞 SUPPORT & TROUBLESHOOTING

### **"Where do I start?"**
→ `README_NEW_CHAT.md`

### **"How does the MCP server work?"**
→ `MCP_SERVER_GUIDE.md`

### **"What is SKYWORKS?"**
→ `PROJECT_ONBOARDING.md`

### **"What's the SORA 2.5 floor rule?"**
→ `SORA_2_5_TABLES_REFERENCE.md` → Section "Floor Rule"

### **"How to calculate SAIL?"**
→ MCP tool: `calculate_sail({ final_grc: X, residual_arc: "Y" })`

### **"MCP not working?"**
→ `MCP_SERVER_GUIDE.md` → Troubleshooting section

### **"Tests failing?"**
→ Compare with SORA tables via MCP tools

### **"Need SORA Annex B details?"**
→ MCP tool: `search_sora_docs({ query: "M1 mitigation Annex B" })`

---

## 🌟 KEY CONCEPTS - QUICK REFERENCE

**FLOOR RULE:**
> Final GRC **≥** Column minimum from Table 2

**M1/M2 Sequence (SORA 2.5):**
> M1A + M1B + M1C → M2 (no M3 in 2.5)

**≤250g Rule:**
> If UA ≤250g + <80J → iGRC -2 levels

**SAIL Matrix:**
```
       ARC-a  ARC-b  ARC-c  ARC-d
GRC≤2    I     II     IV     VI
GRC 3    II    II     IV     VI
GRC 4    III   III    IV     VI
GRC 5    IV    IV     IV     VI
GRC 6    V     V      V      VI
GRC 7    VI    VI     VI     VI
```

**OSO Count:**
- SAIL I: 6 OSOs
- SAIL II: 10 OSOs
- SAIL III: 15 OSOs
- SAIL IV: 18 OSOs
- SAIL V: 21 OSOs
- SAIL VI: 22 OSOs

---

## 📚 FINAL NOTES

**Για AI Agents:**
- Μην ξανα-διαβάσεις αρχεία που έχεις ήδη διαβάσει
- Χρησιμοποίησε **πάντα** το MCP server για SORA formulas
- Validate **όλα** με τα official SORA tables
- Προσθέτε JARUS references στα comments

**Για Humans:**
- Πες **πάντα** στο AI agent: "SKYWORKS - Step X. [Task]."
- Verify ότι το MCP server τρέχει (VS Code settings)
- Διάβασε το `PROJECT_ONBOARDING.md` πρώτα
- Use το template από `README_NEW_CHAT.md`

---

**🚁 SKYWORKS AI Suite - Full JARUS SORA 2.0/2.5 Compliance Platform ✨**

**Last Updated:** October 27, 2025  
**Version:** 1.0.0  
**Total Files:** 100+  
**Knowledge Base:** 23 EASA/JARUS documents  
**MCP Tools:** 7  
**MCP Resources:** 4  
**Permanent Memory:** 4 files  

**Θυμήσου:** Το MCP server εξαλείφει το 90% του file reading. Όλη η SORA γνώση είναι instant! 🎯
