# FINAL COMPREHENSIVE FIX REQUEST FOR CLAUDE SONNET 4.5

## CONTEXT
You are reviewing the SKYWORKS AI SUITE SORA implementation after successful test completion (256 passed, 0 failed). However, we want to ensure 100% compliance with official EASA/JARUS standards before production deployment.

## OBJECTIVE
Review and fix ALL Python backend models and calculations to ensure perfect alignment with official EASA/JARUS SORA documentation:
1. **JARUS SORA 2.0** (March 2019)
2. **JARUS SORA 2.5** (Annex F - Quantitative Method)
3. **EASA AMC/GM to Part-UAS** (Official regulatory text)

## CURRENT STATUS
✅ **Tests Passing**: 256/257 (100% success rate)
✅ **Fixed Issues**:
- Micro-UAS 250g rule (mass ≤0.25 AND speed ≤25 AND dimension <1)
- Python validation for dimension requirements
- YAML calibration for quantitative model
- SORA 2.5 M2 mitigation level mapping (Low→None)
- OutOfScopeReason field added
- Async wrapper ExecuteCompleteAsync added

## FILES TO REVIEW AND FIX

### 1. Python Models (`Backend_Python/models/sora_models.py`)
**Current Issues to Check**:
- GRCRequest_2_0, GRCRequest_2_5 input schemas
- GRCResponse output format
- Enum definitions (MitigationLevel, ARCRating, OperationalScenario, etc.)
- Field aliases and validation

**Required Fixes**:
1. Ensure all enum values match EXACTLY with EASA/JARUS official terminology
2. Validate field names match official SORA documentation (e.g., "characteristic_dimension" vs "max_dimension")
3. Check that all mitigation levels (M1A, M1B, M1C, M2) use correct enum values per SORA 2.5 Annex F
4. Verify population density bands match official ranges (<5, <50, <500, <5000, <50000, >=50000)

### 2. GRC Models (`Backend_Python/grc/models/grc_models.py`)
**Current Implementation**:
```python
class M1ALevel25(str, Enum):
    NONE = "None"
    MEDIUM = "Medium"
    # High is N/A

class M1BLevel25(str, Enum):
    NONE = "None"
    MEDIUM = "Medium"
    HIGH = "High"
    # Low is N/A

class M1CLevel25(str, Enum):
    NONE = "None"
    LOW = "Low"
    # Medium/High are N/A

class M2Level25(str, Enum):
    NONE = "None"
    MEDIUM = "Medium"
    HIGH = "High"
    # Low is N/A
```

**Questions for EASA/JARUS Compliance**:
1. Are these mitigation level enums 100% correct per SORA 2.5 Annex F Table?
2. Should M1A really have no "High" option?
3. Should M1C really have no "Medium" or "High" options?
4. Is M2 "Low" truly not applicable in SORA 2.5?

### 3. GRC Rules YAML (`Backend_Python/grc/rules/grc_rules_sora_2_5.yaml`)
**Current Calibration**:
```yaml
quantitative_model:
  version: "SORA_2.5_Annex_F"
  kinetic_factor: 0.3
  
  dimension_exponent: 2
  speed_exponent: 2
  
  population_density_bands:
    - threshold: 5
      weight: 0.5
    - threshold: 50
      weight: 1.0
    - threshold: 500
      weight: 4.2  # Calibrated from 4.0 to fix SORA25_007
    - threshold: 5000
      weight: 8.0
    - threshold: 50000
      weight: 12.0
    - threshold: 999999999
      weight: 16.0
  
  risk_score_mapping:
    - max_score: 7.0
      grc: 1
    - max_score: 15.0
      grc: 2
    - max_score: 25.0
      grc: 3
    - max_score: 40.0
      grc: 4
    - max_score: 60.0
      grc: 5
    - max_score: 85.0
      grc: 6
    - max_score: 115.0
      grc: 7
    - max_score: 999999
      grc: 8
```

**Questions for EASA/JARUS Compliance**:
1. Are these population density band thresholds EXACTLY as specified in SORA 2.5 Annex F?
2. Are the weights scientifically derived or empirically calibrated?
3. Is the kinetic_factor=0.3 officially documented or is it a project-specific constant?
4. Are dimension_exponent=2 and speed_exponent=2 correct per official formula?
5. Is the risk_score_mapping table aligned with official iGRC buckets?

### 4. Mitigation Effectiveness Tables
**SORA 2.0 M1/M2/M3** (from JARUS SORA 2.0 Table 6):
```
M1 (Strategic Mitigation):
- Low: -1 GRC
- Medium: -2 GRC
- High: -2 GRC (no additional benefit)

M2 (Effects of Ground Impact):
- Medium: -1 GRC
- High: -2 GRC

M3 (Emergency Response Plan):
- Low: +1 GRC (negative mitigation)
- Medium: 0 GRC
- High: 0 GRC
```

**SORA 2.5 M1A/M1B/M1C/M2** (from Annex F):
```
M1A (Sheltering):
- Medium: -X GRC (NEED OFFICIAL VALUE)

M1B (Operational Restrictions):
- Medium: -Y GRC (NEED OFFICIAL VALUE)
- High: -Z GRC (NEED OFFICIAL VALUE)

M1C (Ground Observation):
- Low: -W GRC (NEED OFFICIAL VALUE)

M2 (Impact Dynamics):
- Medium: -A GRC (NEED OFFICIAL VALUE)
- High: -B GRC (NEED OFFICIAL VALUE)
```

**CRITICAL REQUEST**: Provide the EXACT mitigation effectiveness values from EASA/JARUS official documentation.

## OFFICIAL EASA/JARUS DOCUMENTATION REQUIRED

Please reference these official documents:

### 1. JARUS SORA 2.0 (March 2019)
**Key Sections**:
- **Table 2**: Intrinsic Ground Risk Class determination (operational scenarios)
- **Table 6**: Mitigation effectiveness (M1, M2, M3)
- **Section 3.2**: Ground Risk assessment methodology
- **Section 3.3**: Air Risk assessment methodology
- **Annex B**: Strategic Mitigations (M1)
- **Annex C**: Effects of Ground Impact (M2)
- **Annex D**: Emergency Response Plan (M3)

### 2. JARUS SORA 2.5 (Annex F - Quantitative Method)
**Key Sections**:
- **Annex F Introduction**: Quantitative GRC calculation formula
- **Annex F Table**: Population density bands and weights
- **Annex F Mitigation Tables**: M1A, M1B, M1C, M2 effectiveness
- **Formula**: `iGRC = f(kinetic_energy, population_density, dimension, speed)`

### 3. EASA AMC/GM to Part-UAS
**Key Sections**:
- **AMC1 UAS.SPEC.050(3)**: SAIL determination table (GRC × ARC → SAIL)
- **GM1 UAS.SPEC.050**: Operational Safety Objectives (OSO) requirements per SAIL
- **AMC2 UAS.SPEC.050**: Out-of-scope operations (SAIL VI, GRC >7)

## SPECIFIC VERIFICATION TASKS

### Task 1: Enum Value Verification
✅ **Verify each enum value against official EASA/JARUS terminology**:
- [ ] OperationalScenario enum matches SORA 2.0 Table 2 exactly
- [ ] M1Level20, M2Level20, M3Level20 match SORA 2.0 Annex B/C/D
- [ ] M1ALevel25, M1BLevel25, M1CLevel25, M2Level25 match SORA 2.5 Annex F
- [ ] ARCRating enum uses official notation (ARC-a, ARC-b, ARC-c, ARC-d)
- [ ] SAILLevel enum uses Roman numerals (I, II, III, IV, V, VI)

### Task 2: Formula Verification
✅ **Verify quantitative model formula against SORA 2.5 Annex F**:
- [ ] Kinetic energy formula: `KE = 0.5 * MTOM * speed²`
- [ ] Dimension factor: `D²` (squared)
- [ ] Speed factor: `v²` (squared)
- [ ] Population density weighting function
- [ ] Final risk score calculation
- [ ] iGRC mapping from risk score

### Task 3: Mitigation Effectiveness Verification
✅ **Cross-reference ALL mitigation effectiveness values**:
- [ ] SORA 2.0: M1 Low=-1, Medium=-2, High=-2
- [ ] SORA 2.0: M2 Medium=-1, High=-2
- [ ] SORA 2.0: M3 Low=+1, Medium=0, High=0
- [ ] SORA 2.5: M1A Medium=? (GET FROM DOCS)
- [ ] SORA 2.5: M1B Medium=?, High=? (GET FROM DOCS)
- [ ] SORA 2.5: M1C Low=? (GET FROM DOCS)
- [ ] SORA 2.5: M2 Medium=?, High=? (GET FROM DOCS)

### Task 4: Population Density Bands Verification
✅ **Verify population density thresholds match official SORA 2.5**:
```
Current Implementation:
<5/km²    → weight 0.5
<50/km²   → weight 1.0
<500/km²  → weight 4.2  ← CALIBRATED (was 4.0)
<5000/km² → weight 8.0
<50000/km² → weight 12.0
>=50000/km² → weight 16.0
```
**Question**: Are these thresholds and weights official or project-calibrated?

### Task 5: Micro-UAS Rule Verification
✅ **Verify 250g micro-UAS gating conditions** (already fixed):
```python
# Current implementation (FIXED):
if (mtom_kg <= 0.25 and 
    max_speed_ms <= 25 and 
    max_characteristic_dimension_m < 1.0):
    return GRC_1_with_appropriate_mitigations
```
**Question**: Is this the EXACT official rule from EASA/JARUS?

## OUTPUT FORMAT REQUIRED

### 1. Compliance Report
```markdown
## EASA/JARUS Compliance Audit

### ✅ COMPLIANT Items
- [Item]: [Explanation with official doc reference]

### ⚠️ ISSUES FOUND
- [Item]: [Current implementation vs Official requirement]
  - **Fix**: [Exact code change needed]
  - **Reference**: [EASA/JARUS doc section]

### 📋 VERIFICATION NEEDED
- [Item]: [Question requiring official doc clarification]
```

### 2. Code Fixes
Provide complete, production-ready Python code for:
- `sora_models.py` (if changes needed)
- `grc_models.py` (if changes needed)
- `grc_rules_sora_2_5.yaml` (if changes needed)

### 3. Documentation References
For EVERY change, cite:
- **Document**: [e.g., "JARUS SORA 2.5 Annex F"]
- **Section**: [e.g., "Table F.2 - Mitigation Effectiveness"]
- **Page**: [e.g., "Page 87"]
- **Exact Quote**: [verbatim text from official doc]

## CRITICAL CONSTRAINTS

1. **DO NOT break existing tests** (256 passing tests must remain passing)
2. **ONLY fix items that deviate from official EASA/JARUS docs**
3. **Provide official documentation reference for EVERY fix**
4. **If uncertain, mark as "VERIFICATION NEEDED" with specific question**
5. **Preserve backward compatibility with existing .NET client calls**

## CURRENT TEST RESULTS (FOR REFERENCE)

```
✅ SORA25 Tests: 8/8 passed
✅ Full Test Suite: 256 passed, 0 failed, 1 skipped
✅ Micro-UAS 250g rule: Working correctly
✅ YAML calibration: Tests pass with exposure_band_weights[2]=4.2
```

## YOUR MISSION

1. **READ** all official EASA/JARUS documentation carefully
2. **COMPARE** current implementation against official standards
3. **IDENTIFY** any deviations or ambiguities
4. **PROVIDE** exact fixes with official doc references
5. **PRESERVE** existing test success (no regressions)

## DELIVERABLES

1. **Compliance Audit Report** (markdown)
2. **Fixed Python Files** (complete code, ready to apply)
3. **Documentation Cross-Reference Table** (every change → official source)
4. **Test Validation Plan** (how to verify fixes don't break tests)

---

**BEGIN YOUR AUDIT NOW. BE EXTREMELY THOROUGH. CITE OFFICIAL SOURCES FOR EVERYTHING.**
