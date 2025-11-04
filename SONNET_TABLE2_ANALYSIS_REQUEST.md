# CRITICAL ANALYSIS REQUEST FOR CLAUDE SONNET 4

## PROBLEM SUMMARY

The Python backend (`Backend_Python/main.py`) implements **SORA 2.0 Table 2** (MTOM-based matrix), but the .NET backend reference documentation (`Backend/JARUS_25_TABLE2_REFERENCE.ps1`) shows **JARUS SORA 2.5 Table 2** (Dimension+Speed-based matrix).

**These are TWO COMPLETELY DIFFERENT tables with different structures and values.**

## CURRENT PYTHON IMPLEMENTATION (Backend_Python/main.py lines 32-50)

```python
# Table 2: Initial GRC Matrix (AUTHORITATIVE - JARUS SORA 2.5 Annex A)
# Population Density →  ≤1/km²  ≤500   ≤10k   >10k
# MTOM Category ↓      (Cat 0) (Cat 1)(Cat 2)(Cat 3)
IGRC_MATRIX = [
    [1, 2, 3, 4],  # Category 0 (<0.25kg)
    [1, 2, 4, 5],  # Category 1 (0.25-1kg)  
    [2, 3, 5, 6],  # Category 2 (1-25kg)
    [3, 4, 6, 7],  # Category 3 (25-150kg)
    [4, 5, 7, 8]   # Category 4 (>150kg)
]

@staticmethod
def get_population_category(population_density: int) -> int:
    """Get population density category per JARUS SORA Table 2"""
    if population_density <= 1:
        return 0  # Controlled ground area
    elif population_density <= 500:
        return 1  # Sparsely populated
    elif population_density <= 10000:
        return 2  # Moderately populated  
    else:
        return 3  # High density populated (>10k/km²)
```

**This is SORA 2.0 structure: 5×4 matrix based on MTOM weight categories**

## OFFICIAL JARUS SORA 2.5 TABLE 2 (Backend/JARUS_25_TABLE2_REFERENCE.ps1)

```
## POPULATION DENSITY CATEGORIES (Rows):
# Row 0: Controlled ground area (special management)
# Row 1: < 5 people/km² (Remote)
# Row 2: ≥5 and <50 (Lightly populated)
# Row 3: ≥50 and <500 (Sparsely populated)  
# Row 4: ≥500 and <5,000 (Suburban)
# Row 5: ≥5,000 and <50,000 (High density metro)
# Row 6: ≥50,000 (Assemblies of people)

## UA DIMENSION + SPEED CATEGORIES (Columns):
# Column 0: ≤1m AND ≤25 m/s (Small)
# Column 1: ≤3m AND ≤35 m/s (Medium)
# Column 2: ≤8m AND ≤75 m/s (Large)
# Column 3: ≤20m AND ≤120 m/s (Very Large)
# Column 4: >20m OR >120 m/s (Extra Large)

## JARUS 2.5 TABLE 2 MATRIX:
#             Col0  Col1  Col2  Col3  Col4
#           (1m/  (3m/  (8m/  (20m/ (40m+/
#            25)   35)   75)   120)  200+)
# Row 0 (Controlled)  1     1     2     3     3
# Row 1 (<5)          2     3     4     5     6
# Row 2 (<50)         3     4     5     6     7
# Row 3 (<500)        4     5     6     7     8
# Row 4 (<5k)         5     6     7     8     9
# Row 5 (<50k)        6     7     8     9    10
# Row 6 (≥50k)        7     8    GREY  GREY  GREY

## SPECIAL RULES:
# 1. ≤250g MTOM AND ≤25m/s → ALWAYS iGRC=1 (overrides table)
# 2. Grey cells (Row 6, Col 2-4) = Out of SORA 2.5 scope
```

**This is SORA 2.5 structure: 7×5 matrix based on Dimension + Speed**

## KEY DIFFERENCES

| Aspect | SORA 2.0 (Current Python) | JARUS SORA 2.5 (Reference) |
|--------|---------------------------|----------------------------|
| **Matrix Size** | 5 rows × 4 columns | 7 rows × 5 columns |
| **Row Basis** | MTOM weight categories | Population density (7 levels) |
| **Column Basis** | Population density (4 levels) | UA Dimension + Speed (5 levels) |
| **Population Boundaries** | ≤1, ≤500, ≤10k, >10k | <5, <50, <500, <5k, <50k, ≥50k |
| **Max iGRC** | 8 | 10 |
| **Special Rules** | None | ≤250g + ≤25m/s → iGRC=1 |

## EVIDENCE OF CONFUSION

**Test #4 Failure:**
```
Test 4: Small Drone (0.8kg), Suburban (8000 ppl/km²)
Expected iGRC=5, Got=4 FAIL
```

**Why it failed:**
- In **SORA 2.5**: 8000 ppl/km² is Row 5 (≥5k and <50k), not Row 4
- In **SORA 2.0**: 8000 ppl/km² is Category 2 (≤10k), MTOM 0.8kg is Category 1
- Result: IGRC_MATRIX[1][2] = 4 ✓ (SORA 2.0 correct)
- But expected 5 because test was written for SORA 2.5 logic

## FILES TO ANALYZE

### Python Backend Files (SORA 2.0 implementation)
1. `Backend_Python/main.py` - Lines 30-92 (JARUSSORATable2 class)
2. `Backend_Python/calculations/grc_calculator.py` - GRC calculation logic
3. `Backend_Python/models/sora_models.py` - Data models

### .NET Backend Files (SORA 2.5 reference)
1. `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs`
2. `Backend/JARUS_25_TABLE2_REFERENCE.ps1`
3. `Backend/ACCURACY_TEST_REPORT.md`

### Official Documentation (Need to verify)
- JARUS SORA 2.0 Annex A (if available)
- JARUS SORA 2.5 Annex A Table 2 (JAR-DEL-SRM-SORA-MB-2.5)

## QUESTIONS FOR CLAUDE SONNET 4

### 1. Which Table 2 is correct for each SORA version?
- Does SORA 2.0 use MTOM-based Table 2?
- Does SORA 2.5 use Dimension+Speed-based Table 2?
- Or are they both valid but for different use cases?

### 2. Should Python backend support BOTH versions?
- Should there be two separate calculation paths?
- `/api/v1/calculate/grc/2.0` → MTOM-based Table 2
- `/api/v1/calculate/grc/2.5` → Dimension+Speed-based Table 2

### 3. Are the .NET and Python backends supposed to be identical?
- Currently they implement different Table 2 structures
- Is this intentional (different versions) or a bug?

### 4. What is the migration path?
If SORA 2.5 replaces 2.0:
- Should we deprecate MTOM-based calculations?
- Do we need backward compatibility?
- What about existing missions calculated with SORA 2.0?

## ANALYSIS METHODOLOGY REQUEST

Please analyze this systematically:

### Step 1: Document Review
1. Read official JARUS SORA 2.0 documentation
2. Read official JARUS SORA 2.5 documentation (JAR-DEL-SRM-SORA-MB-2.5)
3. Compare Table 2 structures in both versions
4. Document which structure belongs to which version

### Step 2: Code Analysis
1. Read `Backend_Python/main.py` (full file)
2. Read `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs`
3. Identify which SORA version each backend implements
4. Document discrepancies

### Step 3: Test Case Validation
1. For each test case, determine if it's testing SORA 2.0 or 2.5
2. Verify expected values against correct Table 2
3. Identify which tests are using wrong expected values

### Step 4: Recommendation
Provide ONE of these solutions:

**Option A: Separate Implementations**
- Keep SORA 2.0 with MTOM-based Table 2
- Add SORA 2.5 with Dimension+Speed-based Table 2
- Support both via different endpoints

**Option B: Migrate to SORA 2.5 Only**
- Replace MTOM-based Table 2 with Dimension+Speed-based
- Update all calculations to JARUS 2.5 standards
- Deprecate SORA 2.0 support

**Option C: Fix Current Implementation**
- Current Python code is correct for SORA 2.0
- Tests are wrong (expecting SORA 2.5 values)
- Just fix test expectations

## EXPECTED DELIVERABLES

1. **Table 2 Specification Document** - Which table belongs to which SORA version
2. **Code Fix Recommendations** - Specific file changes with line numbers
3. **Updated Test Suite** - Correct expected values for each SORA version
4. **Migration Guide** - If changes needed, how to implement without breaking existing functionality

## URGENCY

**HIGH** - This is blocking accurate GRC calculations. Current system may be calculating wrong values if wrong Table 2 is used.

---

**Analysis Date:** October 29, 2025  
**Requester:** SKYWORKS AI Suite Development Team  
**Target Reviewer:** Claude Sonnet 4 (Visual Studio Integrated)
