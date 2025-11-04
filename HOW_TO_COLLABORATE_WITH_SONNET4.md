# HOW TO COLLABORATE WITH CLAUDE SONNET 4 - SKYWORKS SORA TABLE 2 FIX

## OVERVIEW

This document explains how GitHub Copilot and Claude Sonnet 4 can collaborate to fix the SORA Table 2 discrepancy in SKYWORKS AI Suite.

## THE PROBLEM

**Two different "Table 2" implementations exist:**

1. **Python Backend** (`Backend_Python/main.py`): Uses SORA 2.0 MTOM-based 5×4 matrix
2. **.NET Backend** (`Backend/`): Uses JARUS SORA 2.5 Dimension+Speed-based 7×5 matrix

**This causes calculation discrepancies and test failures.**

## COLLABORATION WORKFLOW

### Phase 1: Information Gathering (Copilot)

**Copilot's role:**
1. ✅ Collect all relevant files
2. ✅ Create comprehensive problem statement
3. ✅ Package context for Sonnet 4

**Files to send to Sonnet 4:**

#### Core Problem Document
- `SONNET_TABLE2_ANALYSIS_REQUEST.md` (this analysis request)

#### Python Backend Files
- `Backend_Python/main.py` (lines 30-92 - JARUSSORATable2 class)
- `Backend_Python/calculations/grc_calculator.py` (full file)
- `Backend_Python/models/sora_models.py` (SORA data models)
- `Backend_Python/main.py` (lines 250-400 - GRC 2.0 and 2.5 endpoints)

#### .NET Backend Files  
- `Backend/JARUS_25_TABLE2_REFERENCE.ps1` (official SORA 2.5 Table 2)
- `Backend/ACCURACY_TEST_REPORT.md` (test results showing 100% pass on .NET)
- `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs` (if exists)

#### Documentation Files
- `ACCURACY_TEST_REPORT.md`
- `COMPREHENSIVE_ACCURACY_REPORT.md`
- `Docs/Compliance/SKYWORKS_SORA_COMPLIANCE_AUDIT_REPORT.md` (lines 325-347)

#### Test Files
- `test_sora_direct.py` (our failed test showing the problem)
- `COMPREHENSIVE_SORA_VALIDATION_TEST.ps1` (PowerShell tests)

### Phase 2: Sonnet 4 Analysis

**Sonnet 4's role:**
1. Read all official JARUS documentation
2. Identify which Table 2 belongs to which SORA version
3. Analyze code discrepancies
4. Provide specific fix recommendations

**Expected Sonnet 4 deliverables:**

```markdown
1. TABLE 2 SPECIFICATION DOCUMENT
   - SORA 2.0 Table 2 structure (from official docs)
   - JARUS SORA 2.5 Table 2 structure (from official docs)
   - Key differences documented

2. CODE ANALYSIS REPORT
   - Which backend implements which version correctly
   - Line-by-line code review with corrections
   - Specific file paths and line numbers to change

3. RECOMMENDED SOLUTION
   One of:
   - Option A: Separate implementations (keep both)
   - Option B: Migrate to SORA 2.5 only
   - Option C: Fix tests (code is correct)

4. IMPLEMENTATION PLAN
   - Step-by-step migration guide
   - Code changes with before/after
   - Updated test expectations
```

### Phase 3: Implementation (Copilot)

**Copilot receives Sonnet 4's response and:**
1. Reads the analysis report
2. Implements recommended code changes
3. Updates test expectations
4. Re-runs validation tests
5. Verifies 100% pass rate

## HOW TO SEND TO SONNET 4

### Method 1: VS Code Integrated (Recommended)

```
1. Open new Claude Sonnet 4 chat in VS Code
2. Attach these files:
   - SONNET_TABLE2_ANALYSIS_REQUEST.md
   - Backend_Python/main.py
   - Backend/JARUS_25_TABLE2_REFERENCE.ps1
   - test_sora_direct.py
   
3. Send message:
   "@SONNET_TABLE2_ANALYSIS_REQUEST.md Please analyze this 
   SORA Table 2 discrepancy and provide detailed fix recommendations 
   per the analysis methodology in the document."
```

### Method 2: Claude.ai Web Interface

```
1. Copy content of SONNET_TABLE2_ANALYSIS_REQUEST.md
2. Attach relevant files (main.py, reference files, tests)
3. Paste analysis request
4. Wait for comprehensive response
5. Copy response back to VS Code
```

### Method 3: API Integration (Advanced)

```python
# send_to_sonnet4.py
import anthropic

client = anthropic.Anthropic(api_key="YOUR_KEY")

with open("SONNET_TABLE2_ANALYSIS_REQUEST.md") as f:
    analysis_request = f.read()

response = client.messages.create(
    model="claude-sonnet-4-20250514",
    max_tokens=8192,
    messages=[{
        "role": "user",
        "content": analysis_request
    }]
)

with open("SONNET4_RESPONSE_TABLE2.md", "w") as f:
    f.write(response.content[0].text)
```

## EXPECTED SONNET 4 RESPONSE FORMAT

```markdown
# SONNET 4 ANALYSIS: SKYWORKS SORA TABLE 2 FIX

## 1. OFFICIAL SPECIFICATION VERIFICATION

### SORA 2.0 Table 2 (EASA AMC)
**Source:** EASA AMC1 UAS.OPEN.040/050/060 Annex
**Structure:** 5 rows (MTOM categories) × 4 columns (population density)
**Matrix:**
[Exact table from official docs]

### JARUS SORA 2.5 Table 2 (JAR-DEL-SRM-SORA-MB-2.5)
**Source:** JARUS SORA 2.5 Main Body Section 2.3, Table 2
**Structure:** 7 rows (population density) × 5 columns (dimension+speed)
**Matrix:**
[Exact table from official docs]

## 2. CODE ANALYSIS

### Python Backend (Backend_Python/main.py)
**Current Implementation:** SORA 2.0 MTOM-based
**Correctness:** ✅ CORRECT for SORA 2.0 / ❌ INCORRECT for SORA 2.5

**Issues Found:**
- Line 12: Comment says "JARUS SORA 2.5" but implements SORA 2.0
- Line 38-45: Population boundaries are SORA 2.0 (≤1, ≤500, ≤10k, >10k)
- Missing: SORA 2.5 dimension+speed logic

### .NET Backend (Backend/...)
**Current Implementation:** JARUS SORA 2.5 Dimension+Speed-based
**Correctness:** ✅ CORRECT for SORA 2.5

## 3. RECOMMENDED SOLUTION

**OPTION A: Dual Implementation (RECOMMENDED)**

Reason: Both SORA versions are valid and in use. Support both.

### Implementation Steps:

1. **Rename existing Python class:**
   ```python
   # main.py line 32
   class SORAv2_0_Table2:  # Rename from JARUSSORATable2
       """SORA 2.0 MTOM-based Table 2 (EASA AMC)"""
   ```

2. **Create new SORA 2.5 class:**
   ```python
   class JARUSSORAv2_5_Table2:
       """JARUS SORA 2.5 Dimension+Speed-based Table 2"""
       
       IGRC_MATRIX = [
           [1, 1, 2, 3, 3],  # Row 0: Controlled
           [2, 3, 4, 5, 6],  # Row 1: <5 ppl/km²
           [3, 4, 5, 6, 7],  # Row 2: <50
           [4, 5, 6, 7, 8],  # Row 3: <500
           [5, 6, 7, 8, 9],  # Row 4: <5k
           [6, 7, 8, 9, 10], # Row 5: <50k
           [7, 8, -1, -1, -1] # Row 6: ≥50k (grey cells)
       ]
       
       @staticmethod
       def get_dimension_speed_category(dimension_m, speed_ms):
           if dimension_m <= 1 and speed_ms <= 25:
               return 0
           elif dimension_m <= 3 and speed_ms <= 35:
               return 1
           elif dimension_m <= 8 and speed_ms <= 75:
               return 2
           elif dimension_m <= 20 and speed_ms <= 120:
               return 3
           else:
               return 4
       
       @staticmethod
       def get_population_category_2_5(population_density):
           if population_density < 5:
               return 1
           elif population_density < 50:
               return 2
           elif population_density < 500:
               return 3
           elif population_density < 5000:
               return 4
           elif population_density < 50000:
               return 5
           else:
               return 6
   ```

3. **Update endpoints to use correct class:**
   ```python
   # Line 250 - SORA 2.0 endpoint
   @app.post("/api/v1/calculate/grc/2.0")
   def calculate_grc_2_0(...):
       result = SORAv2_0_Table2.calculate_initial_grc(mtom_kg, pop_density)
   
   # Line 330 - SORA 2.5 endpoint  
   @app.post("/api/v1/calculate/grc/2.5")
   def calculate_grc_2_5(...):
       result = JARUSSORAv2_5_Table2.calculate_initial_grc(
           dimension_m, speed_ms, pop_density
       )
   ```

4. **Update tests:**
   - SORA 2.0 tests: Use MTOM + population density
   - SORA 2.5 tests: Use dimension + speed + population density

## 4. DETAILED CODE CHANGES

[Specific file edits with exact line numbers and code]

## 5. VALIDATION PLAN

[How to verify the fix is correct]

---
**Analysis Date:** [Date]
**Reviewed By:** Claude Sonnet 4
**Confidence:** 95%
```

## PHASE 4: VERIFICATION (Both Copilot + Sonnet 4)

After implementation:

1. **Copilot runs tests:**
   ```bash
   python test_sora_direct.py
   ```

2. **If tests pass:** ✅ Done!

3. **If tests fail:**
   - Copilot sends test results back to Sonnet 4
   - Sonnet 4 analyzes failures
   - Provides additional corrections
   - Iterate until 100% pass

## SUCCESS CRITERIA

- ✅ All 20 tests pass (10 SORA 2.0 + 10 SORA 2.5)
- ✅ Python backend matches official JARUS specs
- ✅ .NET backend matches official JARUS specs  
- ✅ Clear separation between SORA 2.0 and 2.5 logic
- ✅ Documentation updated with correct Table 2 references

## CURRENT STATUS

- ⏳ Phase 1 (Information Gathering): **COMPLETE**
- ⏳ Phase 2 (Sonnet 4 Analysis): **READY TO START**
- ⏹️ Phase 3 (Implementation): Pending Sonnet 4 response
- ⏹️ Phase 4 (Verification): Pending implementation

## NEXT STEP

**Send this package to Claude Sonnet 4 for detailed analysis.**

---

**Prepared by:** GitHub Copilot  
**Date:** October 29, 2025  
**For:** SKYWORKS AI Suite Development Team
