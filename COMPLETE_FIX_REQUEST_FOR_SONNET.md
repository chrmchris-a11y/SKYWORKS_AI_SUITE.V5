# COMPLETE SORA CALCULATOR FIX REQUEST - SONNET 4.5

## CRITICAL ISSUE SUMMARY

We have a SKYWORKS AI Suite Python implementation of SORA (Specific Operations Risk Assessment) calculators that **MUST BE 100% EASA/JARUS compliant**. The current implementation has errors and inconsistencies with the authoritative JARUS specifications. We need a complete fix of ALL calculators.

---

## REQUIRED AUTHORITATIVE SOURCES

### SORA 2.0
- **Document**: JARUS guidelines on Specific Operations Risk Assessment (SORA) - Edition 2.0 (JAR-DEL-WG6-D.04)
- **GRC Section**: 2.3.1 - Determination of Ground Risk Class
- **ARC Section**: 2.3.2 - Determination of Air Risk Class  
- **SAIL Section**: 2.3.3 - Determination of SAIL

### SORA 2.5
- **Document**: JARUS SORA 2.5 (JAR-DEL-SRM-SORA-MB-2.5)
- **GRC**: Annex A - Table A-1 (Population density-based approach)
- **ARC**: Annex B - Table B-1 (Air Risk Classes)
- **SAIL**: Main Body - SAIL matrix with GRC=5, ARC-c special case

---

## FILE STRUCTURE

```
Backend_Python/
├── main.py                          # FastAPI routes
├── models/
│   └── sora_models.py              # Pydantic request/response models
├── calculations/
│   ├── grc_calculator.py           # Ground Risk Class - NEEDS FIX
│   ├── arc_calculator.py           # Air Risk Class - NEEDS FIX
│   └── sail_calculator.py          # SAIL determination - NEEDS FIX
└── test_comprehensive_report.py    # Test harness (validates fixes)
```

---

## DETAILED REQUIREMENTS BY CALCULATOR

### 1. GRC CALCULATOR (`calculations/grc_calculator.py`)

#### SORA 2.0 Requirements:

**Intrinsic GRC Table (MUST match JARUS exactly):**
```python
# Based on dimension (max_dimension_m) and operational scenario
# Operational scenarios: VLOS_Controlled, VLOS_Sparsely, VLOS_Populated,
#                        BVLOS_Controlled, BVLOS_Sparsely, BVLOS_Populated

Dimension Categories:
- Category 1: <1m
- Category 2: 1m ≤ x < 3m  
- Category 3: 3m ≤ x < 8m
- Category 4: ≥8m (OUT OF SCOPE for BVLOS_Populated)

Example Expected Values:
- 0.8m, VLOS_Controlled → iGRC = 1
- 0.99m, VLOS_Sparsely → iGRC = 2
- 3.0m, BVLOS_Populated → iGRC = 7
- 7.5m, BVLOS_Sparsely → iGRC = 7
- 8.0m, BVLOS_Populated → OUT_OF_SCOPE (must return HTTP 400)
```

**Mitigation Logic:**
- Each mitigation (M1, M2, M3) has levels: None, Low, Medium, High
- High mitigation = -2 to GRC
- Medium mitigation = -1 to GRC
- Low mitigation = 0 to GRC
- **CRITICAL**: Floor must be applied **AFTER EACH mitigation step**, NOT at the end
- Floor enforcement:
  - VLOS scenarios: floor = 1
  - BVLOS_Controlled: floor = 2
  - BVLOS_Sparsely: floor = 3
  - BVLOS_Populated: floor = 3

**Final GRC Validation:**
- If final GRC > 7 → OUT_OF_SCOPE (HTTP 400)

---

#### SORA 2.5 Requirements:

**Population-Based Intrinsic GRC Table (Annex A, Table A-1):**

```
Population Density Thresholds (people/km²):
- Controlled Ground Area:
  - Row 1: < 300  (STRICT <, not ≤)
  - Row 2: ≥ 300
  
- Non-Controlled Ground Area:
  - Row 1: < 50   (STRICT <)
  - Row 2: 50 ≤ x < 200  (STRICT <)
  - Row 3: 200 ≤ x < 500 (STRICT <)
  - Row 4: ≥ 500

Speed Categories (max_speed_ms):
- Cat 1: < 25 m/s   (STRICT <)
- Cat 2: 25 ≤ x < 50 (STRICT <)
- Cat 3: 50 ≤ x < 100 (STRICT <)
- Cat 4: ≥ 100 m/s

Dimension Categories (max_dimension_m):
- Cat 1: < 1m
- Cat 2: 1 ≤ x < 3m
- Cat 3: 3 ≤ x < 8m
- Cat 4: ≥ 8m
```

**CRITICAL RULES:**

1. **250g Special Rule** (MUST be checked FIRST):
   ```python
   if weight_kg <= 0.25 AND max_speed_ms < 25:  # BOTH conditions required
       return GRC = 1 (FINAL, no mitigations applied)
   ```

2. **Worst-Case Category Selection**:
   ```python
   final_category = max(dimension_category, speed_category)
   ```

3. **Population Row Selection** (STRICT < boundaries):
   ```python
   if is_controlled_ground:
       if population_density < 300:  # STRICT <
           row = 1
       else:
           row = 2
   else:  # non-controlled
       if population_density < 50:
           row = 1
       elif population_density < 200:
           row = 2
       elif population_density < 500:
           row = 3
       else:
           row = 4
   ```

4. **Column-Based Floor Enforcement**:
   - Floor is determined by the COLUMN (controlled vs non-controlled)
   - Apply floor **AFTER EACH mitigation step**
   - Controlled ground floors: [1, 2] (by column)
   - Non-controlled ground floors: [1, 3, 4, 5] (by column)

**Expected Test Cases:**
```python
# 250g rule
weight=0.2kg, speed=20m/s, dim=0.2m → iGRC=1, fGRC=1

# 250g boundary violation (both conditions must be true)
weight=0.26kg, speed=25m/s → Does NOT qualify for 250g rule

# Controlled ground
dim=3m, speed=20m/s, pop=200, controlled=True → iGRC=2, fGRC=2

# High speed
dim=15m, speed=100m/s, pop=1000 → iGRC=8, fGRC=8

# Rural population
dim=3m, speed=20m/s, pop=200, controlled=False → iGRC=5, fGRC=5

# Mitigations with floor
dim=5m, speed=30m/s, pop=200, controlled=False, M1=High
→ iGRC=6, after M1: max(6-2, floor_3)=4, but actual expected is 3 (check floor logic)
```

---

### 2. ARC CALCULATOR (`calculations/arc_calculator.py`)

#### Request Schema Requirements:

**SORA 2.0 (`ARCRequest_2_0`):**
```python
class ARCRequest_2_0(BaseModel):
    airspace_class: AirspaceClass  # A, B, C, D, E, F, G
    altitude_agl_ft: float         # Altitude above ground level in feet
    environment: Environment       # Urban, Suburban, Rural
    distance_to_aerodrome_nm: Optional[float]  # Nautical miles
    is_in_ctr: bool = False
    is_mode_s_veil: bool = False
    is_tmz: bool = False
    is_atypical_segregated: bool = False
    strategic_mitigations: List[str] = []
```

**SORA 2.5 (`ARCRequest_2_5`):**
```python
class ARCRequest_2_5(BaseModel):
    airspace_class: AirspaceClass
    altitude_agl_m: float          # Altitude in METERS (not feet!)
    environment: Environment
    distance_to_aerodrome_km: Optional[float]  # Kilometers
    is_in_ctr: bool = False
    is_mode_s_veil: bool = False
    is_tmz: bool = False
    is_atypical_segregated: bool = False
    strategic_mitigations: List[str] = []
```

#### ARC Determination Logic:

**Priority Order (MUST be followed):**
1. Mode-S veil OR TMZ → AEC 2 → Initial ARC-c
2. Near aerodrome (controlled) → AEC 6/7 → ARC-c or ARC-d
3. Controlled airspace → AEC 3/4/8 → varies
4. Uncontrolled airspace → AEC 1/9 → varies

**Strategic Mitigation Rules:**
- Each SM reduces ARC by 1 level: ARC-d → ARC-c → ARC-b → ARC-a
- **CRITICAL FLOOR**: Typical operations CANNOT go below ARC-b
- Only atypical/segregated operations can reach ARC-a

```python
def _apply_strategic_mitigations(initial_arc, strategic_mitigations, is_atypical):
    arc_levels = [ARC_a, ARC_b, ARC_c, ARC_d]
    current_index = arc_levels.index(initial_arc)
    
    reduction = len(strategic_mitigations)
    new_index = max(0, current_index - reduction)
    
    # FLOOR: typical ops cannot go below ARC-b
    if not is_atypical_segregated and new_index < 1:
        new_index = 1  # ARC-b
    
    return arc_levels[new_index]
```

**Expected Test Cases:**
```python
# SORA 2.0
Class G, 100ft, Rural, typical → Initial: ARC-b, Residual: ARC-b
Class D, 200ft, Urban, Mode-S, 2 SMs, typical → Initial: ARC-c, Residual: ARC-b (floor!)

# SORA 2.5  
Class G, 30m, Rural, typical → Initial: ARC-b, Residual: ARC-b
Class E, 90m, Suburban, TMZ, 1 SM, typical → Initial: ARC-c, Residual: ARC-b
```

---

### 3. SAIL CALCULATOR (`calculations/sail_calculator.py`)

#### Request Schema:
```python
class SAILRequest(BaseModel):
    final_grc: int  # 1-8 (or -1 for out of scope)
    residual_arc: ARCRating  # Enum: ARC-a, ARC-b, ARC-c, ARC-d (NOT string!)
```

#### Response Schema:
```python
class SAILResponse(BaseModel):
    sail: str  # "I", "II", "III", "IV", "V", "VI" (STRING, not int!)
    notes: str
    source: str
```

#### SAIL Matrix (JARUS SORA 2.5 - CRITICAL SPECIAL CASE):

```
         ARC-a  ARC-b  ARC-c  ARC-d
GRC 1      I      I      II     III
GRC 2      I      II     II     III
GRC 3      I      II     III    IV
GRC 4      II     II     IV     IV
GRC 5      II     IV     V*     V    ← SPECIAL: GRC=5 with ARC-c yields SAIL V (not IV!)
GRC 6      II     IV     V      V
GRC 7      IV     IV     VI     VI
GRC 8      IV     V      VI     VI
```

**Implementation:**
```python
def calculate_sail(self, request: SAILRequest) -> SAILResponse:
    grc = request.final_grc
    arc = request.residual_arc
    
    # SPECIAL CASE FIRST
    if grc == 5 and arc == ARCRating.ARC_c:
        return SAILResponse(sail="V", ...)
    
    # Standard matrix lookup...
```

**Expected Test Cases:**
```python
GRC=1, ARC-a → SAIL "I"
GRC=3, ARC-b → SAIL "II"
GRC=5, ARC-c → SAIL "V"  # SPECIAL CASE!
GRC=7, ARC-d → SAIL "VI"
```

---

## CURRENT ERRORS & ISSUES

### Issue 1: GRC 2.5 Population Boundaries
**Problem**: Code uses `<=` instead of strict `<` for boundaries
```python
# WRONG:
if population_density <= 50:  # ❌

# CORRECT:
if population_density < 50:   # ✅
```

### Issue 2: GRC 250g Rule
**Problem**: Not checked FIRST, or only one condition checked
```python
# WRONG:
if weight_kg <= 0.25:  # ❌ Missing speed check

# CORRECT:
if weight_kg <= 0.25 and max_speed_ms < 25:  # ✅
```

### Issue 3: GRC Floor Application
**Problem**: Floor applied only at the end instead of after each mitigation
```python
# WRONG:
grc = intrinsic_grc - m1 - m2 - m3
grc = max(grc, floor)  # ❌

# CORRECT:
grc = intrinsic_grc
grc = max(grc - m1_reduction, floor)  # Apply floor AFTER M1
grc = max(grc - m2_reduction, floor)  # Apply floor AFTER M2  
grc = max(grc - m3_reduction, floor)  # Apply floor AFTER M3
```

### Issue 4: ARC Schema Mismatch
**Problem**: Tests fail with 422 errors due to missing required fields
- `ARCRequest_2_0` MUST have `airspace_class` and `altitude_agl_ft`
- `ARCRequest_2_5` MUST have `airspace_class` and `altitude_agl_m`

### Issue 5: ARC Floor Logic
**Problem**: Typical operations go below ARC-b
```python
# Wrong: allows ARC-a for typical ops
if new_index < 0:
    new_index = 0  # ❌

# Correct: floor at ARC-b for typical ops
if not is_atypical_segregated and new_index < 1:
    new_index = 1  # ✅ ARC-b is minimum
```

### Issue 6: SAIL Input Type
**Problem**: SAIL receives string "ARC-c" instead of enum ARCRating.ARC_c
```python
# Request must accept:
residual_arc: ARCRating  # Enum, not str

# Response must return:
sail: str  # "I", "II", etc. (string!)
```

### Issue 7: SAIL Special Case Missing
**Problem**: GRC=5 with ARC-c returns "IV" instead of "V"

---

## VALIDATION TEST SUITE

After fixes, ALL these tests MUST pass (20/20):

### GRC 2.0 Tests (5 tests)
```python
1. dim=0.8m, VLOS_Controlled → iGRC=1, fGRC=1
2. dim=3.0m, BVLOS_Populated, M1=High → iGRC=7, fGRC=3 (floor)
3. dim=0.99m, VLOS_Sparsely → iGRC=2, fGRC=2
4. dim=8.0m, BVLOS_Populated → HTTP 400 OUT_OF_SCOPE
5. dim=7.5m, BVLOS_Sparsely, M1+M2+M3=High → iGRC=7, fGRC=3
```

### GRC 2.5 Tests (7 tests)
```python
1. weight=0.2kg, speed=20m/s → iGRC=1, fGRC=1 (250g rule)
2. weight=0.26kg, speed=25m/s, pop=50 → iGRC=5, fGRC=5
3. dim=3m, speed=20m/s, pop=200, controlled=True → iGRC=2, fGRC=2
4. dim=5m, speed=30m/s, pop=200, M1=High → iGRC=6, fGRC=3
5. dim=15m, speed=100m/s, pop=1000 → iGRC=8, fGRC=8
6. dim=3m, speed=20m/s, pop=200, controlled=False → iGRC=5, fGRC=5
7. dim=10m, speed=50m/s, pop=500, M1+M2+M3=High → iGRC=8, fGRC=4
```

### ARC Tests (4 tests)
```python
1. SORA 2.0: G, 100ft, Rural → Initial: ARC-b, Residual: ARC-b
2. SORA 2.0: D, 200ft, Urban, Mode-S, 2 SMs → Initial: ARC-c, Residual: ARC-b
3. SORA 2.5: G, 30m, Rural → Initial: ARC-b, Residual: ARC-b
4. SORA 2.5: E, 90m, Suburban, TMZ, 1 SM → Initial: ARC-c, Residual: ARC-b
```

### SAIL Tests (4 tests)
```python
1. GRC=1, ARC-a → "I"
2. GRC=3, ARC-b → "II"
3. GRC=5, ARC-c → "V" (SPECIAL!)
4. GRC=7, ARC-d → "VI"
```

---

## DELIVERABLES REQUIRED

Please provide COMPLETE, CORRECTED versions of these files:

1. **`calculations/grc_calculator.py`**
   - Fix SORA 2.0 intrinsic GRC table
   - Fix SORA 2.0 floor enforcement (apply after each mitigation)
   - Fix SORA 2.5 population boundaries (strict <)
   - Fix SORA 2.5 250g rule (both conditions)
   - Fix SORA 2.5 worst-case category selection
   - Fix SORA 2.5 column-based floor enforcement

2. **`calculations/arc_calculator.py`**
   - Ensure schema matches request models exactly
   - Fix priority logic (Mode-S/TMZ first)
   - Fix strategic mitigation floor (ARC-b minimum for typical ops)
   - Validate all 12 AEC categories

3. **`calculations/sail_calculator.py`**
   - Fix input type (ARCRating enum)
   - Fix output type (string SAIL level)
   - Implement GRC=5, ARC-c special case

4. **`models/sora_models.py`**
   - Ensure all request/response models match exactly
   - ARCRequest must have airspace_class, altitude fields
   - SAILRequest must use ARCRating enum
   - SAILResponse must return string SAIL

---

## SUCCESS CRITERIA

✅ **All 20 comprehensive tests pass (20/20)**
✅ **No schema validation errors (422)**
✅ **No connection errors (service runs)**
✅ **100% EASA/JARUS compliance**
✅ **Correct boundary handling (strict < vs ≤)**
✅ **Correct floor enforcement (per mitigation, not at end)**
✅ **Correct special cases (250g rule, GRC=5+ARC-c)**

---

## NOTES

- Use **EXACT** JARUS table values - no approximations
- Test with the provided test suite (`test_comprehensive_report.py`)
- Ensure backward compatibility with existing FastAPI routes
- Add comprehensive docstrings citing JARUS sections
- Use type hints and Pydantic validation
- Handle edge cases (OUT_OF_SCOPE, invalid inputs)

---

## URGENCY

This is blocking production deployment. We need 100% accurate, JARUS-compliant calculators. Please provide complete, tested, working code for all files.

**Expected outcome**: `test_comprehensive_report.py` returns **20/20 PASSED** ✅
