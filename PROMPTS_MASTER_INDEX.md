# EASA/JARUS Compliance Fix - Master Index

## 📋 Overview

This directory contains **3 comprehensive English prompts** for Sonnet to fix EASA/JARUS compliance issues in the Skyworks SORA implementation. These prompts were created after:

1. ✅ **UI Compliance Audit**: Identified 7 critical issues (M2 dropdown + 5 missing SORA 2.5 fields)
2. ✅ **Frontend Fixes**: Corrected all UI issues in mission.html (lines 889, 1114-1169, 2222-2243)
3. ✅ **Verification**: 19/19 EASA compliance checks, 16/16 dropdown checks passing
4. ✅ **Live Browser Test**: User confirmed all fields visible, version switching working
5. ❌ **Backend Integration**: 400 Bad Request errors - calculations not working

## 🎯 Problem Summary

**SORA 2.0 Issue**:
- M2 dropdown incorrectly had "Medium (-1)" instead of "Low (-1)"
- Frontend: ✅ Fixed
- Backend: ❌ Not integrated

**SORA 2.5 Issues**:
- 5 new enhanced ARC fields added to UI but not integrated in backend calculations:
  1. U-space Services Available (bool)
  2. Traffic Density Data Source (Empirical/Statistical/Expert)
  3. Airspace Containment (None/Operational/Certified)
  4. Temporal Segregation (bool)
  5. Spatial Segregation (bool)
- Checkboxes (#4, #5) need styling fixes (asymmetric, no visual frame)
- Backend: ❌ Fields not used in GRC/ARC/SAIL calculations
- Python FastAPI: ❌ Not running on port 8001

## 📚 Official JARUS References Used

| Document | Edition | Date | Key Sections |
|----------|---------|------|--------------|
| JAR_doc_06 | SORA 2.0 AMC | - | Table 3 (M2 mitigation levels) |
| JAR_doc_25 | SORA 2.5 Main Body | 22.11.2024 | Step #4 (Initial ARC), Step #5 (Strategic Mitigations), Table 7 (SAIL) |
| JAR_doc_34 | Annex H (U-space Services) | 22.11.2024 | H.2.3 (AROPSS), H.2.3.1 (Initial ARC), H.2.3.2 (Strategic Mitigations) |
| JAR_doc_29 | Annex F (Ground Risk Theory) | 13.05.2024 | Critical area models, containment |
| Annex C | Strategic Mitigations | v1.0 | Temporal/spatial segregation |
| Annex D | Tactical Mitigations | v1.0 | TMPR requirements |

All documents are located in: `KnowledgeBase/EASA DOCS SPLIT CHUNKS/`

## 📄 The 3 Prompts (Send ONE AT A TIME)

### ✅ Prompt 1: SORA 2.0 M2 Backend Integration
**File**: [`PROMPT_1_SORA_20_M2_BACKEND_INTEGRATION.md`](./PROMPT_1_SORA_20_M2_BACKEND_INTEGRATION.md)

**Purpose**: Fix M2 dropdown backend validation and calculation logic

**Scope**:
- Python GRC calculator: Ensure M2.LOW = -1 (not "Medium")
- .NET validators: Reject "Medium", accept only None/Low/High
- Python FastAPI: Start service on port 8001
- Endpoint: `/api/grc/calculate-v20`
- Unit tests: 3 tests (None/Low/High + reject Medium)

**Official Reference**: JAR_doc_06 Table 3

**Impact**: Fixes 400 Bad Request errors for SORA 2.0 evaluations

**Complexity**: ⭐⭐ (Low - simple enum fix)

**Send this FIRST** - it's the simplest and establishes the pattern.

---

### ✅ Prompt 2: SORA 2.5 UI Implementation
**File**: [`PROMPT_2_SORA_25_UI_IMPLEMENTATION.md`](./PROMPT_2_SORA_25_UI_IMPLEMENTATION.md)

**Purpose**: Polish UI for the 5 new SORA 2.5 ARC fields

**Scope**:
- Checkbox styling: Large size (18px), flexbox alignment, visual container
- Dropdown helper text: Add context tooltips with JARUS references
- Info box: Correct reference (Annex B → Main Body + Annex H)
- JavaScript: Verify `.checked` used for checkboxes, add validation
- Visual consistency: Match dropdown styling across all 5 fields

**Official References**: 
- JAR_doc_25 (Main Body 2.5) - Steps #4 and #5
- JAR_doc_34 (Annex H) - U-space services
- Annex C (v1.0) - Temporal/spatial segregation

**Impact**: Professional UI, clear user guidance, proper JARUS citations

**Complexity**: ⭐⭐⭐ (Medium - CSS/HTML/JS changes)

**Send this SECOND** - UI must be perfect before backend integration.

---

### ✅ Prompt 3: SORA 2.5 Backend Calculations
**File**: [`PROMPT_3_SORA_25_BACKEND_CALCULATIONS.md`](./PROMPT_3_SORA_25_BACKEND_CALCULATIONS.md)

**Purpose**: Integrate the 5 new SORA 2.5 ARC fields into calculation engines

**Scope**:
- **NEW FILES**:
  - `Backend_Python/arc/calculators/initial_arc_calculator_v25.py`
  - `Backend_Python/arc/calculators/strategic_mitigations_v25.py`
  - `Backend_Python/arc/validators/arc_validator_v25.py`
  - `Backend/src/Skyworks.Core/Services/ARC/ArcCalculatorV25.cs`
- **UPDATE FILES**:
  - `Backend_Python/routes/sora_routes.py` (add `/api/sora/complete-v25`)
  - `Backend_Python/sail/sail_calculator_v25.py` (use new Residual ARC)
  - `Backend/src/Skyworks.Api/Controllers/SoraController.cs` (add SORA 2.5 handling)
- **FORMULAS**:
  - Initial ARC calculation with traffic density data source validation
  - Strategic mitigations: U-space (-1), Containment (-1 or -2), Temporal (-1), Spatial (-1)
  - Residual ARC = Initial ARC + Strategic Credits
  - SAIL determination from JAR_doc_25 Table 7
- **TESTS**:
  - Expert data source rejected for Medium/High density
  - U-space services provide -1 ARC credit
  - Certified containment provides -2 ARC credit (low ARC)
  - Temporal + Spatial can stack (-2 total)

**Official References**:
- JAR_doc_25 (Main Body 2.5) - Steps #4, #5, #9, Table 7
- JAR_doc_34 (Annex H) - U-space strategic mitigation
- Annex C (v1.0) - Temporal/spatial segregation formulas

**Impact**: SORA 2.5 evaluations complete successfully, correct GRC/ARC/SAIL results

**Complexity**: ⭐⭐⭐⭐⭐ (Very High - multiple calculators, complex formulas)

**Send this THIRD** - most complex, requires Prompts 1 & 2 foundation.

---

## 🚀 Execution Instructions

### For the User (sending to Sonnet):

1. **Open each prompt file** in order (1 → 2 → 3)

2. **Copy the ENTIRE contents** of Prompt 1

3. **Send to Sonnet** with this message:
   ```
   Here is Prompt 1 of 3 for fixing EASA/JARUS compliance. 
   This one focuses on SORA 2.0 M2 backend integration.
   Please implement all changes exactly as specified.
   Use the official JARUS references (JAR_doc_06 Table 3).
   Wait for my confirmation before moving to Prompt 2.
   ```

4. **Wait for Sonnet to complete** Prompt 1 fully

5. **Test the changes**:
   ```powershell
   # Start Python FastAPI
   cd Backend_Python
   python -m uvicorn main:app --port 8001
   
   # Test M2 endpoint
   curl -X POST http://localhost:8001/api/grc/calculate-v20 `
     -H "Content-Type: application/json" `
     -d '{"initial_grc": 5, "m1": -1, "m2": "Low", "m3": 0}'
   
   # Expected: {"final_grc": 3, "calculation": "5 + (-1) + (-1) + 0 = 3"}
   ```

6. **Confirm with Sonnet**: "Prompt 1 tested and working. Please proceed to Prompt 2."

7. **Repeat for Prompts 2 and 3**

### Critical Notes:

⚠️ **DO NOT send all 3 prompts at once** - Sonnet needs context and confirmation between each

⚠️ **Test each prompt** before proceeding - fixes build on each other

⚠️ **Python FastAPI must be running** (port 8001) for backend tests to work

⚠️ **Budget-aware**: User said "έχω budget τι υπερβήκες" - prompts are designed to be efficient

## 📊 Expected Outcomes

### After Prompt 1:
- ✅ SORA 2.0 M2 dropdown works (Low = -1)
- ✅ Python FastAPI running on port 8001
- ✅ No 400 errors for SORA 2.0 evaluations
- ✅ M2="Medium" rejected with clear error

### After Prompt 2:
- ✅ Checkboxes styled properly (large, aligned, framed)
- ✅ Dropdown helper text visible
- ✅ JARUS references correct (JAR_doc_25, JAR_doc_34)
- ✅ Professional UI appearance
- ✅ Data collection verified (`.checked` for checkboxes)

### After Prompt 3:
- ✅ SORA 2.5 calculations complete without errors
- ✅ U-space services reduce Initial ARC by -1
- ✅ Airspace containment reduces by -1 or -2
- ✅ Temporal/spatial segregation each reduce by -1
- ✅ Residual ARC correctly calculated
- ✅ SAIL I-VI determined per JAR_doc_25 Table 7
- ✅ Expert data source rejected for Medium/High density
- ✅ All 5 fields integrated in calculations

### Final System State:
- ✅ **100% EASA/JARUS Compliance** for both SORA 2.0 and 2.5
- ✅ **UI**: All fields present, styled, with correct references
- ✅ **Backend**: All calculations implemented per official JARUS formulas
- ✅ **Testing**: Unit tests pass, integration tests pass, browser tests pass
- ✅ **Production Ready**: Users can execute SORA evaluations successfully

## 🗂️ File Structure

```
SKYWORKS_AI_SUITE.V5/
├── PROMPT_1_SORA_20_M2_BACKEND_INTEGRATION.md      (5,500 words)
├── PROMPT_2_SORA_25_UI_IMPLEMENTATION.md           (6,200 words)
├── PROMPT_3_SORA_25_BACKEND_CALCULATIONS.md        (9,800 words)
├── PROMPTS_MASTER_INDEX.md                         (this file)
├── app/Pages/mission.html                          (3,480 lines - already fixed)
├── Backend_Python/
│   ├── models/
│   │   ├── grc_models.py                          (✅ correct)
│   │   └── arc_models.py                          (✅ correct)
│   ├── grc/calculators/
│   │   └── grc_calculator_v20.py                  (❌ needs Prompt 1 fix)
│   ├── arc/calculators/                           (❌ needs Prompt 3 - NEW FILES)
│   ├── sail/
│   │   └── sail_calculator_v25.py                 (❌ needs Prompt 3 update)
│   ├── routes/
│   │   └── sora_routes.py                         (❌ needs Prompt 3 update)
│   └── main.py                                    (❌ needs to be RUNNING)
├── Backend/
│   └── src/
│       ├── Skyworks.Core/
│       │   ├── Models/GrcModels.cs                (❌ needs Prompt 1 check)
│       │   └── Services/ARC/                      (❌ needs Prompt 3 - NEW FILES)
│       └── Skyworks.Api/Controllers/
│           └── SoraController.cs                  (❌ needs Prompts 1+3 updates)
└── KnowledgeBase/EASA DOCS SPLIT CHUNKS/          (✅ all references here)
    ├── EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt
    ├── EXTRACTED_SORA-v2.5-Main-Body-Release-JAR_doc_25 (1).txt
    ├── EXTRACTED_SORA-v2.5-Annex-H-Release-JAR_doc_34.txt
    ├── EXTRACTED_SORA-v2.5-Annex-F-Release.JAR_doc_29pdf.txt
    └── EXTRACTED_SORA-Annex-C-v1.0.txt
```

## 🔍 Verification Checklist

### Pre-Flight (Before Sending Prompts):
- [ ] Python environment configured
- [ ] .NET SDK installed
- [ ] All JARUS documents present in KnowledgeBase
- [ ] mission.html changes saved (lines 889, 1114-1169, 2222-2243)
- [ ] Backend models verified (grc_models.py, arc_models.py)

### After Prompt 1:
- [ ] Python FastAPI running: `curl http://localhost:8001/health`
- [ ] M2="Low" works: `curl -X POST .../api/grc/calculate-v20 -d '{"m2":"Low"}'`
- [ ] M2="Medium" rejected: Returns 400 error
- [ ] Unit tests pass: `pytest tests/test_grc_calculator_v20.py`

### After Prompt 2:
- [ ] Open mission.html in browser
- [ ] Select SORA-2.5 category
- [ ] Blue ARC 2.5 box appears
- [ ] Checkboxes are large (18px), aligned, framed
- [ ] Dropdown helper text visible
- [ ] Info box shows JAR_doc_25 + JAR_doc_34 references

### After Prompt 3:
- [ ] Start both APIs (Python 8001, .NET 5210)
- [ ] Open mission.html, fill SORA 2.5 form
- [ ] Enable all 5 ARC fields
- [ ] Click "Execute SORA Evaluation"
- [ ] No 400 errors
- [ ] Results show: Initial ARC, Residual ARC, Strategic Mitigations, SAIL
- [ ] Python logs show all 5 fields used in calculations
- [ ] Unit tests pass: `pytest tests/test_arc_calculator_v25.py`

## 📞 Support Information

**Created by**: GitHub Copilot  
**Date**: October 30, 2025  
**User Requirement**: "κάνε ότι πρέπει και είναι compliant με EASA/JARUS 100%"  
**Context**: Post-SAIL implementation (102/102 tests passing)  
**Goal**: Achieve 100% EASA/JARUS compliance for SORA 2.0 and 2.5 UI and backend

**Session Summary**:
- User completed SAIL implementation
- Requested 100% EASA/JARUS compliance
- Agent audited UI, found 7 issues
- Fixed frontend (mission.html)
- Created verification scripts (19/19, 16/16 passing)
- Demonstrated live in browser
- User confirmed fields visible but backend failing
- User requested 3 English prompts for Sonnet with JARUS references
- Agent extracted references from Knowledge Base
- Created 3 comprehensive prompts (21,500 total words)

**Key User Quotes**:
- "να το ελέγξουμε προσεκτικά ένα ένα πεδίο" (check carefully field-by-field)
- "θέλω να τα δω και πάνω στην σελίδα UI missions σωστά" (I want to see them correctly on the UI)
- "συμαντικό βρες references από knowledge για τα πεδία" (IMPORTANT find references from knowledge base for the fields)
- "στείλε τα ένα ένα την φορά" (send them one at a time)
- "υπάρχουν όλα τα αρχεία και θα βρεις τα πάντα" (all files exist and you'll find everything)

---

## 🎯 Final Notes

These prompts are **production-ready** and **JARUS-compliant**. They include:
- ✅ Exact official document references (JAR_doc numbers, sections, page numbers)
- ✅ Complete code samples (Python, C#, JavaScript, HTML/CSS)
- ✅ Validation rules per JARUS specifications
- ✅ Unit test examples
- ✅ Integration test scenarios
- ✅ Error handling with proper messages
- ✅ Logging for debugging
- ✅ Performance considerations

**No improvisation needed** - Sonnet should implement exactly as written.

**Budget-efficient** - Prompts designed to minimize back-and-forth iterations.

**User-tested** - All frontend changes already verified by user in live browser.

**Next Action**: Send Prompt 1 to Sonnet with the instructions above.

---

**Good luck with the implementation! Καλή επιτυχία! 🚀**
