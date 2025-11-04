# IMPORTANT: DUAL SORA VERSION REQUIREMENT

**Date:** October 29, 2025  
**To:** Claude Sonnet 4 and all AI development assistants  
**From:** SKYWORKS Development Team  
**Subject:** Critical Design Requirement - Both SORA 2.0 and 2.5 Must Be Supported

---

## EXECUTIVE SUMMARY

**SKYWORKS AI Suite MUST support BOTH SORA 2.0 and SORA 2.5 simultaneously.**

This is NOT a migration from 2.0 to 2.5. This is a **dual-version architecture requirement**.

---

## WHY BOTH VERSIONS ARE REQUIRED

### 1. Regulatory Reality

**Different regulatory bodies require different versions:**

- **EASA (European Union Aviation Safety Agency):**
  - Uses SORA 2.0 (MTOM-based) for UAS.OPEN categories
  - Existing AMC (Acceptable Means of Compliance) reference SORA 2.0
  - Thousands of approved operations use SORA 2.0

- **JARUS (Joint Authorities for Rulemaking on Unmanned Systems):**
  - Released SORA 2.5 (JAR-DEL-SRM-SORA-MB-2.5) in May 2024
  - Uses Dimension+Speed-based Table 2
  - More comprehensive risk assessment framework

### 2. Customer Requirements

**Different operators need different versions:**

- **Existing operators:** Have missions approved under SORA 2.0
  - Cannot suddenly invalidate their risk assessments
  - Need backward compatibility
  - Regulatory continuity required

- **New operators:** May need SORA 2.5 compliance
  - More accurate risk assessment
  - Future-proof regulatory alignment
  - Enhanced safety framework

### 3. Geographic Requirements

- **EU member states:** May mandate SORA 2.0 (EASA alignment)
- **International operations:** May require SORA 2.5 (JARUS global standard)
- **Cross-border operations:** Need BOTH versions for different jurisdictions

---

## WHAT THIS MEANS FOR IMPLEMENTATION

### ❌ WRONG Approach: Migration

```
"Let's deprecate SORA 2.0 and migrate everything to SORA 2.5"
```

**Why this is wrong:**
- Breaks existing approved operations
- Invalidates historical risk assessments
- Regulatory non-compliance in EASA jurisdictions
- Loss of backward compatibility

### ✅ CORRECT Approach: Dual Implementation

```
"Support BOTH versions with clear separation"
```

**Why this is correct:**
- Regulatory compliance in ALL jurisdictions
- Backward compatibility maintained
- Future-proof architecture
- Customer choice based on their requirements

---

## TECHNICAL REQUIREMENTS

### Both Table 2 Implementations Must Exist

**SORA 2.0 Table 2:**
- Class: `SORAv2_0_Table2`
- Structure: 5×4 matrix
- Basis: MTOM categories × Population density
- Endpoint: `/api/v1/calculate/grc/2.0`
- Status: **MANDATORY** - Cannot be removed

**JARUS SORA 2.5 Table 2:**
- Class: `JARUSSORAv2_5_Table2`
- Structure: 7×5 matrix
- Basis: Population density × UA Dimension+Speed
- Endpoint: `/api/v1/calculate/grc/2.5`
- Status: **MANDATORY** - Must be added

### Both Mitigation Frameworks Must Exist

**SORA 2.0 Mitigations:**
- M1, M2, M3 structure
- SORA_20_MITIGATIONS dictionary
- Function: `apply_mitigations_2_0()`
- Status: **MANDATORY** - Cannot be removed

**JARUS SORA 2.5 Mitigations:**
- M1A, M1B, M1C, M2 structure
- SORA_25_MITIGATIONS dictionary
- Function: `apply_mitigations_2_5()`
- Status: **MANDATORY** - Must be added

### Both Calculation Paths Must Work

**Every calculation endpoint MUST support version parameter:**

```python
# Bad - Single version
@app.post("/api/v1/calculate/grc")
def calculate_grc(request):
    # Uses only one version

# Good - Version-specific endpoints
@app.post("/api/v1/calculate/grc/2.0")
def calculate_grc_2_0(request):
    # Uses SORA 2.0

@app.post("/api/v1/calculate/grc/2.5")
def calculate_grc_2_5(request):
    # Uses JARUS SORA 2.5
```

---

## TEST REQUIREMENTS

### All Tests Must Specify Version

**Bad - Version ambiguous:**
```python
test_grc("Small drone suburban", 0.8, 8000, 5)  # Which SORA version?
```

**Good - Version explicit:**
```python
# SORA 2.0 Test
test_grc_2_0("Small drone suburban", 0.8, 8000, 4)  # MTOM-based

# JARUS SORA 2.5 Test
test_grc_2_5("Small drone suburban", 0.8, 25, 8000, 6)  # Dimension+Speed-based
```

### Test Coverage Requirements

- ✅ Minimum 10 tests for SORA 2.0
- ✅ Minimum 10 tests for JARUS SORA 2.5
- ✅ Edge cases for both versions
- ✅ Cross-validation between Python and .NET backends
- ✅ Version-specific boundary conditions

---

## DOCUMENTATION REQUIREMENTS

### Code Comments Must Specify Version

**Bad:**
```python
class JARUSSORATable2:
    """JARUS SORA Table 2"""
    IGRC_MATRIX = [...]
```

**Good:**
```python
class SORAv2_0_Table2:
    """
    SORA 2.0 MTOM-based Table 2
    Source: EASA AMC1 UAS.OPEN.040/050/060
    Status: MANDATORY - Required for EASA compliance
    """
    IGRC_MATRIX = [...]

class JARUSSORAv2_5_Table2:
    """
    JARUS SORA 2.5 Dimension+Speed-based Table 2
    Source: JAR-DEL-SRM-SORA-MB-2.5
    Status: MANDATORY - Required for JARUS compliance
    """
    IGRC_MATRIX = [...]
```

### API Documentation Must Show Both Versions

```yaml
/api/v1/calculate/grc/2.0:
  post:
    summary: Calculate GRC using SORA 2.0 (MTOM-based)
    description: For EASA compliance and existing operations
    
/api/v1/calculate/grc/2.5:
  post:
    summary: Calculate GRC using JARUS SORA 2.5 (Dimension+Speed)
    description: For JARUS compliance and new operations
```

---

## DATABASE REQUIREMENTS

### Version Tracking in Mission Data

```sql
CREATE TABLE missions (
    id UUID PRIMARY KEY,
    sora_version VARCHAR(10) NOT NULL,  -- '2.0' or '2.5'
    calculation_date TIMESTAMP,
    -- ... other fields
);

CREATE INDEX idx_missions_sora_version ON missions(sora_version);
```

### Migration Path

**Existing missions:**
- Tagged as `sora_version = '2.0'`
- Continue to use SORA 2.0 calculations
- **NEVER automatically migrate to 2.5**

**New missions:**
- User chooses version at creation
- System validates using correct version
- Version locked for mission lifetime

---

## FRONTEND REQUIREMENTS

### Version Selection UI

```typescript
interface MissionForm {
    soraVersion: '2.0' | '2.5';  // Mandatory field
    
    // SORA 2.0 fields
    mtomKg?: number;
    
    // SORA 2.5 fields
    dimensionM?: number;
    speedMs?: number;
}
```

### Clear User Guidance

```
"Which SORA version does your operation require?"

○ SORA 2.0 (EASA/EU Operations)
  - Required for EASA UAS.OPEN compliance
  - Use if your operation was approved under SORA 2.0
  - MTOM-based risk assessment

○ JARUS SORA 2.5 (International/New Operations)
  - Latest JARUS standard (May 2024)
  - More comprehensive risk framework
  - Dimension+Speed-based assessment
  
[?] Not sure which to choose? → Help Guide
```

---

## VALIDATION RULES

### Cross-Version Consistency Checks

**DO NOT compare results between versions:**
```python
# Bad - These are DIFFERENT standards
assert grc_2_0 == grc_2_5  # Will fail!
```

**DO validate each version independently:**
```python
# Good - Separate validation
assert grc_2_0 == expected_sora_2_0_value
assert grc_2_5 == expected_jarus_2_5_value
```

### Version-Specific Validation

```python
def validate_mission(mission):
    if mission.sora_version == '2.0':
        validate_sora_2_0_requirements(mission)
    elif mission.sora_version == '2.5':
        validate_jarus_2_5_requirements(mission)
    else:
        raise ValueError(f"Unsupported SORA version: {mission.sora_version}")
```

---

## COMMON MISTAKES TO AVOID

### ❌ Mistake 1: Assuming Migration

```
"Let's update from SORA 2.0 to 2.5"
```

**Correction:** It's not an upgrade, it's parallel support.

### ❌ Mistake 2: Mixing Standards

```python
# Using SORA 2.0 GRC with SORA 2.5 mitigations
grc = calculate_grc_2_0(...)
mitigations = apply_mitigations_2_5(...)  # Wrong!
```

**Correction:** Keep versions completely separate.

### ❌ Mistake 3: Expecting Same Results

```
"Test failed: SORA 2.0 gave 4 but SORA 2.5 gave 6"
```

**Correction:** This is expected! They're different standards.

### ❌ Mistake 4: Single Test Suite

```python
def test_grc():
    # Tests mix 2.0 and 2.5 expectations
```

**Correction:** Separate test suites for each version.

---

## SUCCESS CRITERIA

### Technical Success

- ✅ Both `SORAv2_0_Table2` and `JARUSSORAv2_5_Table2` implemented
- ✅ Separate endpoints: `/grc/2.0` and `/grc/2.5`
- ✅ Version-specific test suites (10+ tests each)
- ✅ 100% pass rate for BOTH versions
- ✅ Clear documentation of version differences

### Business Success

- ✅ Existing SORA 2.0 operations continue unaffected
- ✅ New JARUS SORA 2.5 operations supported
- ✅ Users can choose appropriate version
- ✅ Regulatory compliance in all jurisdictions
- ✅ Backward compatibility guaranteed

### Compliance Success

- ✅ EASA compliance maintained (SORA 2.0)
- ✅ JARUS compliance achieved (SORA 2.5)
- ✅ Audit trail shows correct version used
- ✅ Historical data integrity preserved
- ✅ No regulatory violations introduced

---

## COMMUNICATION TO AI ASSISTANTS

### When Claude Sonnet 4 or GitHub Copilot suggests changes:

**Always ask:**
1. "Does this affect SORA 2.0, SORA 2.5, or both?"
2. "Are both versions still fully functional after this change?"
3. "Do tests cover both versions?"
4. "Is version separation maintained?"

**Always verify:**
- No removal of SORA 2.0 code
- No breaking changes to existing endpoints
- No mixing of version logic
- Clear version indicators in all code

**Always implement:**
- Parallel support, never replacement
- Version-specific tests
- Clear documentation of which version
- Backward compatibility checks

---

## FINAL REMINDER

```
╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║  SORA 2.0 and JARUS SORA 2.5 are BOTH mandatory.               ║
║                                                                  ║
║  This is NOT "old vs new" - it's "standard A vs standard B"     ║
║                                                                  ║
║  Both must coexist indefinitely.                                ║
║                                                                  ║
║  Never suggest removing SORA 2.0 support.                       ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

**Approved by:** SKYWORKS Development Team  
**Effective:** October 29, 2025  
**Applies to:** All AI assistants (Claude Sonnet 4, GitHub Copilot, etc.)  
**Status:** MANDATORY REQUIREMENT
