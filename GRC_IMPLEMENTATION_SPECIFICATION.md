# GRC (Ground Risk Class) Implementation Specification for Claude Sonnet 4

**Document Version**: 1.0  
**Date**: 2025-10-30  
**Target**: Complete Python implementation (SORA 2.0 & 2.5)  
**Standards**: EASA AMC/GM (SORA 2.0), JARUS (SORA 2.5)

---

## ⚠️ CRITICAL IMPLEMENTATION RULES (READ FIRST!)

These are **NON-NEGOTIABLE** constraints derived from official EASA/JARUS standards:

1. **✅ INTEGER VALUES ONLY**  
   All M1/M2/M3 reduction values are integers (0, -1, -2, -4). NO fractional values (e.g., -0.5, -1.5).

2. **✅ M1 FLOOR CAP ALGORITHM**  
   M1 cannot reduce GRC below the column/footprint floor defined by iGRC table/containment.  
   **Apply floor cap IMMEDIATELY after M1, BEFORE M2/M3**.

3. **✅ EXACT MITIGATION VALUES**  
   Use EXACT values from official tables:
   - M1 (SORA 2.0): None=0, Low=-1, Medium=-2, High=-4
   - M2 (SORA 2.0): None=0, Low=-1, High=-2
   - M3 (SORA 2.0): None/Low=+1, Medium=0, High=-1
   - M1A (SORA 2.5): None=0, Low=-1, Medium=-2, High=N/A
   - M1B (SORA 2.5): None=0, Low=N/A, Medium=-1, High=-2
   - M1C (SORA 2.5): None=0, Low=-1, Medium=N/A, High=N/A
   - M2 (SORA 2.5): None=0, Low=N/A, Medium=-1, High=-2

4. **✅ N/A VALIDATION (SORA 2.5)**  
   Reject inputs with N/A combinations:
   - M1A High → 422 error
   - M1B Low → 422 error
   - M1C Medium or High → 422 error
   - M2 Low → 422 error

5. **✅ SEQUENTIAL APPLICATION**  
   Apply mitigations in EXACT order: iGRC → M1(+floor) → M2 → M3 → max(1, result)

6. **✅ FLOOR AT 1**  
   Final GRC cannot be less than 1. Always apply: `final_grc = max(1, calculated_value)`

7. **✅ FULL TRACE REFERENCES**  
   Every calculation step must have `rule_ref` with format: `doc_id + standard + section`  
   Example: "EASA_SORA_2.0_AMC_Step2_Table" or "JARUS_SORA_2.5_Annex_F_Model"

---

## 1. Objective

Implement complete GRC (Ground Risk Class) calculation module for drone SORA assessments supporting:
- **SORA 2.0** (EASA AMC/GM): iGRC table + M1/M2/M3 with floor cap
- **SORA 2.5** (JARUS): Quantitative model + M1A/M1B/M1C/M2 with N/A validation

**Key Requirements:**
- YAML-driven rules (no hardcoded logic)
- Deterministic calculations (no randomness)
- Full traceability (every step logged with official references)
- Pydantic models with validation
- FastAPI endpoints
- Comprehensive tests (golden + property-based)

---

## 2. Official References

### SORA 2.0 (EASA)
- **Document**: EASA Easy Access Rules for UAS (AMC/GM)
- **Version**: AMC1 Article 11, July 2024
- **Step 2**: iGRC determination via population density × scenario mapping
- **Step 3**: Strategic mitigations (M1/M2/M3) with floor cap

### SORA 2.5 (JARUS)
- **Document**: JARUS SORA 2.5
- **Annexes**: Main Body + Annex F (Quantitative Ground Risk Model)
- **Step 2**: iGRC via quantitative calculation (dimension, speed, population)
- **Step 3**: M1A/M1B/M1C/M2 with N/A validation

---

## 3. SORA 2.0 Implementation

### 3.1 Initial GRC Determination (iGRC)

**Inputs:**
- `population_density_p_km2` (integer): People per km²
- `environment_type` (optional): Rural/Suburban/Urban/Industrial/Controlled
- `containment_quality` (optional): Poor/Adequate/Good

**Logic:**
```python
# Simplified iGRC mapping (extract exact table from EASA AMC/GM)
def determine_igrc_20(population_density, environment_type, containment_quality):
    # Example mapping (MUST extract from official Step 2 tables)
    if population_density < 10:
        base = 1
    elif population_density < 100:
        base = 2
    elif population_density < 500:
        base = 3
    elif population_density < 1000:
        base = 4
    elif population_density < 2500:
        base = 5
    elif population_density < 5000:
        base = 6
    elif population_density < 10000:
        base = 7
    else:
        base = 8
    
    # Adjust for containment/scenario (from official tables)
    # [PLACEHOLDER - Extract exact logic from EASA Step 2]
    
    return base  # iGRC ∈ {1,2,3,4,5,6,7,8}
```

**Output:** iGRC (integer 1-8)

**Trace:**
```json
{
  "step": "initial_grc_determination",
  "inputs": {"population_density": 150, "environment": "Suburban"},
  "result": "3",
  "rule_ref": "EASA_SORA_2.0_AMC_Step2_iGRC_Mapping",
  "doc_ref": {
    "doc_id": "EASA_AMC_GM_UAS_2024",
    "standard": "SORA 2.0",
    "section": "Step 2 - iGRC Table"
  },
  "delta": null
}
```

### 3.2 Strategic Mitigations (M1/M2/M3)

#### 3.2.1 M1 Strategic Mitigation

**Levels & Reductions:**
| Level | Reduction | Description |
|-------|-----------|-------------|
| None | 0 | No strategic measures |
| Low | -1 | Basic boundary/chronology |
| Medium | -2 | Enhanced segregation |
| High | -4 | Full certified segregation |

**CRITICAL: M1 Floor Cap Algorithm**

```python
def apply_m1_with_floor_cap(igrc, m1_level, containment_quality):
    # Get reduction from table
    reductions = {"None": 0, "Low": -1, "Medium": -2, "High": -4}
    delta_m1 = reductions[m1_level]
    
    # Apply reduction
    after_m1 = igrc + delta_m1
    
    # Determine floor based on containment/scenario
    # [EXTRACT FROM OFFICIAL TABLES]
    floor_map = {
        "Poor": {1: 1, 2: 2, 3: 3, 4: 4, 5: 4, 6: 5, 7: 5, 8: 6},
        "Adequate": {1: 1, 2: 1, 3: 2, 4: 3, 5: 3, 6: 4, 7: 4, 8: 5},
        "Good": {1: 1, 2: 1, 3: 1, 4: 2, 5: 2, 6: 3, 7: 3, 8: 4}
    }
    
    floor = floor_map.get(containment_quality, {}).get(igrc, 1)
    
    # Apply floor cap
    grc_after_m1_capped = max(floor, after_m1)
    
    # Calculate actual reduction (may be less than delta_m1 due to floor)
    actual_delta = grc_after_m1_capped - igrc
    
    return grc_after_m1_capped, actual_delta, floor
```

**Trace:**
```json
{
  "step": "strategic_mitigation_m1",
  "inputs": {"igrc": 5, "m1_level": "High", "containment": "Good"},
  "result": "2",
  "rule_ref": "EASA_SORA_2.0_M1_High_with_Floor_Cap",
  "doc_ref": {
    "doc_id": "EASA_AMC_GM_UAS_2024",
    "standard": "SORA 2.0",
    "section": "Step 3 - M1 Table with Floor Rules"
  },
  "delta": -3,
  "notes": "Floor cap applied: 5 + (-4) = 1, but floor=2 for Good containment at iGRC=5"
}
```

#### 3.2.2 M2 Impact Effects

**Levels & Reductions:**
| Level | Reduction | Description |
|-------|-----------|-------------|
| None | 0 | No impact mitigation |
| Low | -1 | Basic energy absorption |
| High | -2 | Advanced frangibility/parachute |

**Logic:**
```python
def apply_m2(grc_after_m1, m2_level):
    reductions = {"None": 0, "Low": -1, "High": -2}
    delta_m2 = reductions[m2_level]
    grc_after_m2 = grc_after_m1 + delta_m2
    return grc_after_m2, delta_m2
```

#### 3.2.3 M3 Emergency Response Plan

**Levels & Adjustments:**
| Level | Adjustment | Description |
|-------|------------|-------------|
| None | +1 | No ERP (penalty) |
| Low | +1 | Inadequate ERP (penalty) |
| Medium | 0 | Adequate ERP |
| High | -1 | Validated effective ERP |

**Logic:**
```python
def apply_m3(grc_after_m2, m3_level):
    adjustments = {"None": +1, "Low": +1, "Medium": 0, "High": -1}
    delta_m3 = adjustments[m3_level]
    grc_after_m3 = grc_after_m2 + delta_m3
    return grc_after_m3, delta_m3
```

### 3.3 Final GRC Calculation

```python
def calculate_grc_20(inputs: GRCInputs20) -> GRCResult:
    trace = []
    
    # Step 1: Determine iGRC
    igrc = determine_igrc_20(
        inputs.population_density_p_km2,
        inputs.environment_type,
        inputs.containment_quality
    )
    trace.append(create_trace_entry("initial_grc", igrc, ...))
    
    # Step 2: Apply M1 with floor cap
    grc_after_m1, delta_m1, floor = apply_m1_with_floor_cap(
        igrc, inputs.m1_strategic, inputs.containment_quality
    )
    trace.append(create_trace_entry("mitigation_m1", grc_after_m1, delta_m1, ...))
    
    # Step 3: Apply M2
    grc_after_m2, delta_m2 = apply_m2(grc_after_m1, inputs.m2_impact)
    trace.append(create_trace_entry("mitigation_m2", grc_after_m2, delta_m2, ...))
    
    # Step 4: Apply M3
    grc_after_m3, delta_m3 = apply_m3(grc_after_m2, inputs.m3_erp)
    trace.append(create_trace_entry("mitigation_m3", grc_after_m3, delta_m3, ...))
    
    # Step 5: Apply floor at 1
    final_grc = max(1, grc_after_m3)
    if final_grc != grc_after_m3:
        trace.append(create_trace_entry("floor_clamp", final_grc, ...))
    
    return GRCResult(
        version="SORA_2.0",
        initial_grc=igrc,
        residual_grc=final_grc,
        total_reduction=igrc - final_grc,
        m1_reduction=delta_m1,
        m2_reduction=delta_m2,
        m3_adjustment=delta_m3,
        calculation_trace=trace
    )
```

---

## 4. SORA 2.5 Implementation

### 4.1 Initial GRC (Quantitative Model)

**Inputs:**
- `characteristic_dimension_m`: Aircraft dimension (m)
- `max_speed_mps`: Maximum speed (m/s)
- `population_density_p_km2`: People per km²
- `environment_type`: Context
- `containment_quality`: Footprint control

**Logic:**
```python
def determine_igrc_25(dimension_m, speed_mps, population_density, environment, containment):
    # Quantitative model from JARUS Annex F
    # [EXTRACT EXACT FORMULA FROM ANNEX F]
    
    # Simplified example (MUST use official formula):
    kinetic_energy = 0.5 * (dimension_m ** 2) * (speed_mps ** 2)  # Simplified
    exposure_factor = population_density / 1000.0
    
    risk_score = kinetic_energy * exposure_factor
    
    # Map risk score to iGRC class (from Annex F tables)
    if risk_score < 10:
        igrc = 1
    elif risk_score < 50:
        igrc = 2
    elif risk_score < 150:
        igrc = 3
    # ... [COMPLETE FROM OFFICIAL ANNEX F]
    else:
        igrc = 8
    
    return igrc
```

### 4.2 Mitigations (M1A/M1B/M1C/M2)

#### 4.2.1 M1A Sheltering

**Allowed Levels:** None, Low, Medium (High = N/A → 422 error)

**Reductions:**
| Level | Reduction |
|-------|-----------|
| None | 0 |
| Low | -1 |
| Medium | -2 |
| High | N/A (reject) |

**Validation:**
```python
def validate_m1a(level):
    if level == "High":
        raise ValueError("M1A High is N/A per JARUS SORA 2.5")
    return {"None": 0, "Low": -1, "Medium": -2}[level]
```

#### 4.2.2 M1B Operational Restrictions

**Allowed Levels:** None, Medium, High (Low = N/A → 422 error)

**Reductions:**
| Level | Reduction |
|-------|-----------|
| None | 0 |
| Low | N/A (reject) |
| Medium | -1 |
| High | -2 |

#### 4.2.3 M1C Ground Observation

**Allowed Levels:** None, Low (Medium/High = N/A → 422 error)

**Reductions:**
| Level | Reduction |
|-------|-----------|
| None | 0 |
| Low | -1 |
| Medium | N/A (reject) |
| High | N/A (reject) |

#### 4.2.4 M2 Impact Dynamics

**Allowed Levels:** None, Medium, High (Low = N/A → 422 error)

**Reductions:**
| Level | Reduction |
|-------|-----------|
| None | 0 |
| Low | N/A (reject) |
| Medium | -1 |
| High | -2 |

### 4.3 Final GRC Calculation (SORA 2.5)

```python
def calculate_grc_25(inputs: GRCInputs25) -> GRCResult:
    trace = []
    
    # Step 1: Determine iGRC (quantitative)
    igrc = determine_igrc_25(
        inputs.characteristic_dimension_m,
        inputs.max_speed_mps,
        inputs.population_density_p_km2,
        inputs.environment_type,
        inputs.containment_quality
    )
    trace.append(create_trace_entry("initial_grc_quantitative", igrc, ...))
    
    # Step 2: Apply M1A/M1B/M1C (with N/A validation)
    delta_m1a = validate_m1a(inputs.m1a_sheltering)
    delta_m1b = validate_m1b(inputs.m1b_operational)
    delta_m1c = validate_m1c(inputs.m1c_ground_observation)
    
    grc_after_m1 = igrc + delta_m1a + delta_m1b + delta_m1c
    
    # Apply floor cap if model specifies (similar to 2.0)
    floor = determine_floor_25(igrc, inputs.containment_quality)
    grc_after_m1_capped = max(floor, grc_after_m1)
    
    trace.append(create_trace_entry("mitigation_m1_combined", grc_after_m1_capped, ...))
    
    # Step 3: Apply M2
    delta_m2 = validate_m2(inputs.m2_impact)
    grc_after_m2 = grc_after_m1_capped + delta_m2
    trace.append(create_trace_entry("mitigation_m2", grc_after_m2, delta_m2, ...))
    
    # Step 4: Floor at 1
    final_grc = max(1, grc_after_m2)
    
    return GRCResult(
        version="SORA_2.5",
        initial_grc=igrc,
        residual_grc=final_grc,
        total_reduction=igrc - final_grc,
        m1a_reduction=delta_m1a,
        m1b_reduction=delta_m1b,
        m1c_reduction=delta_m1c,
        m2_reduction=delta_m2,
        calculation_trace=trace
    )
```

---

## 5. UI Fields & Dropdowns

### 5.1 SORA 2.0 Fields

**(Complete GRC_SORA20_DROPDOWNS.json embedded in Section 12)**

Key fields:
- `population_density_p_km2`: number
- `environment_type`: enum [Rural, Suburban, Urban, Industrial, Controlled]
- `containment_quality`: enum [Poor, Adequate, Good]
- `m1_strategic`: enum [None, Low, Medium, High]
- `m2_impact`: enum [None, Low, High]
- `m3_erp`: enum [None, Low, Medium, High]

### 5.2 SORA 2.5 Fields

**(Complete GRC_SORA25_DROPDOWNS.json embedded in Section 12)**

Key fields:
- `characteristic_dimension_m`: number
- `max_speed_mps`: number
- `population_density_p_km2`: number
- `m1a_sheltering`: enum [None, Low, Medium] (no High)
- `m1b_operational`: enum [None, Medium, High] (no Low)
- `m1c_ground_observation`: enum [None, Low] (no Medium/High)
- `m2_impact`: enum [None, Medium, High] (no Low)

---

## 6. Validation Rules

### 6.1 Input Validation

**SORA 2.0:**
```python
def validate_grc_20_inputs(data: dict) -> list[str]:
    errors = []
    
    # Population density must be positive
    if data.get("population_density_p_km2", 0) < 0:
        errors.append("Population density cannot be negative")
    
    # M1/M2/M3 must be valid enum values
    valid_m1 = ["None", "Low", "Medium", "High"]
    if data.get("m1_strategic") not in valid_m1:
        errors.append(f"M1 must be one of: {valid_m1}")
    
    # Similar for M2, M3
    
    return errors
```

**SORA 2.5:**
```python
def validate_grc_25_inputs(data: dict) -> list[str]:
    errors = []
    
    # Dimension and speed must be positive
    if data.get("characteristic_dimension_m", 0) <= 0:
        errors.append("Characteristic dimension must be positive")
    
    if data.get("max_speed_mps", 0) <= 0:
        errors.append("Max speed must be positive")
    
    # N/A validation
    if data.get("m1a_sheltering") == "High":
        errors.append("M1A High is N/A per JARUS SORA 2.5")
    
    if data.get("m1b_operational") == "Low":
        errors.append("M1B Low is N/A per JARUS SORA 2.5")
    
    if data.get("m1c_ground_observation") in ["Medium", "High"]:
        errors.append("M1C Medium/High are N/A per JARUS SORA 2.5")
    
    if data.get("m2_impact") == "Low":
        errors.append("M2 Low is N/A per JARUS SORA 2.5")
    
    return errors
```

### 6.2 Result Validation

```python
def validate_grc_result(result: dict) -> list[str]:
    errors = []
    
    # GRC must be 1-8
    if not (1 <= result.get("initial_grc", 0) <= 8):
        errors.append("Initial GRC must be between 1 and 8")
    
    if not (1 <= result.get("residual_grc", 0) <= 8):
        errors.append("Residual GRC must be between 1 and 8")
    
    # Residual GRC cannot be less than 1
    if result.get("residual_grc", 999) < 1:
        errors.append("Residual GRC cannot be less than 1")
    
    # Total reduction must be non-negative
    if result.get("total_reduction", -1) < 0:
        errors.append("Total reduction cannot be negative")
    
    return errors
```

---

## 7. Test Requirements

### 7.1 Golden Test Cases

#### Test Case 1: SORA 2.0 - Low Population, No Mitigations
```python
def test_grc_20_low_population_no_mitigations():
    inputs = GRCInputs20(
        population_density_p_km2=50,
        environment_type="Rural",
        containment_quality="Good",
        m1_strategic="None",
        m2_impact="None",
        m3_erp="None"
    )
    
    result = GRCCalculator20().calculate(inputs)
    
    assert result.initial_grc == 2  # Low density → iGRC=2
    assert result.residual_grc == 3  # No mitigations + M3 penalty (+1)
    assert result.m1_reduction == 0
    assert result.m2_reduction == 0
    assert result.m3_adjustment == 1  # None → +1 penalty
    assert len(result.calculation_trace) >= 4
```

#### Test Case 2: SORA 2.0 - High M1 with Floor Cap
```python
def test_grc_20_m1_floor_cap():
    inputs = GRCInputs20(
        population_density_p_km2=800,
        environment_type="Urban",
        containment_quality="Good",
        m1_strategic="High",
        m2_impact="None",
        m3_erp="Medium"
    )
    
    result = GRCCalculator20().calculate(inputs)
    
    assert result.initial_grc == 4  # High density → iGRC=4
    # High M1 = -4, but floor for Good/iGRC=4 is 2
    # So: 4 + (-4) = 0, but floor=2 → 2
    assert result.residual_grc == 2
    assert result.m1_reduction == -2  # Actual reduction after floor cap
    assert result.m3_adjustment == 0  # Medium → 0
```

#### Test Case 3: SORA 2.0 - All Mitigations
```python
def test_grc_20_all_mitigations():
    inputs = GRCInputs20(
        population_density_p_km2=1500,
        environment_type="Urban",
        containment_quality="Adequate",
        m1_strategic="Medium",
        m2_impact="High",
        m3_erp="High"
    )
    
    result = GRCCalculator20().calculate(inputs)
    
    assert result.initial_grc == 5
    # M1 Medium = -2 → 3
    # M2 High = -2 → 1
    # M3 High = -1 → 0, but floor at 1
    assert result.residual_grc == 1
    assert result.m1_reduction == -2
    assert result.m2_reduction == -2
    assert result.m3_adjustment == -1
```

#### Test Case 4: SORA 2.5 - N/A Validation
```python
def test_grc_25_na_validation():
    # M1A High should be rejected
    with pytest.raises(ValidationError) as exc:
        inputs = GRCInputs25(
            characteristic_dimension_m=2.5,
            max_speed_mps=25.0,
            population_density_p_km2=500,
            m1a_sheltering="High",  # N/A!
            m1b_operational="None",
            m1c_ground_observation="None",
            m2_impact="None"
        )
    
    assert "M1A High is N/A" in str(exc.value)
```

#### Test Case 5: SORA 2.5 - Quantitative Model
```python
def test_grc_25_quantitative():
    inputs = GRCInputs25(
        characteristic_dimension_m=1.5,
        max_speed_mps=20.0,
        population_density_p_km2=300,
        environment_type="Suburban",
        containment_quality="Good",
        m1a_sheltering="Low",
        m1b_operational="Medium",
        m1c_ground_observation="Low",
        m2_impact="High"
    )
    
    result = GRCCalculator25().calculate(inputs)
    
    # Verify quantitative model used
    assert result.initial_grc in range(1, 9)
    # Verify M1A/M1B/M1C applied
    assert result.m1a_reduction == -1
    assert result.m1b_reduction == -1
    assert result.m1c_reduction == -1
    # Verify M2 applied
    assert result.m2_reduction == -2
    # Verify floor at 1
    assert result.residual_grc >= 1
```

### 7.2 Property-Based Tests

```python
from hypothesis import given, strategies as st

@given(
    population=st.integers(min_value=0, max_value=20000),
    m1=st.sampled_from(["None", "Low", "Medium", "High"]),
    m2=st.sampled_from(["None", "Low", "High"]),
    m3=st.sampled_from(["None", "Low", "Medium", "High"])
)
def test_grc_20_properties(population, m1, m2, m3):
    inputs = GRCInputs20(
        population_density_p_km2=population,
        containment_quality="Good",
        m1_strategic=m1,
        m2_impact=m2,
        m3_erp=m3
    )
    
    result = GRCCalculator20().calculate(inputs)
    
    # Property 1: iGRC must be 1-8
    assert 1 <= result.initial_grc <= 8
    
    # Property 2: final GRC must be 1-8
    assert 1 <= result.residual_grc <= 8
    
    # Property 3: all reductions are integers
    assert isinstance(result.m1_reduction, int)
    assert isinstance(result.m2_reduction, int)
    assert isinstance(result.m3_adjustment, int)
    
    # Property 4: trace has at least initial + final entries
    assert len(result.calculation_trace) >= 2
```

---

## 8. Implementation Requirements

### 8.1 Package Structure

```
Backend_Python/grc/
├── __init__.py
├── models/
│   ├── __init__.py
│   └── grc_models.py          # Pydantic models
├── calculators/
│   ├── __init__.py
│   └── grc_calculator.py      # GRCCalculator20, GRCCalculator25
├── validators/
│   ├── __init__.py
│   └── grc_validator.py       # Validation logic
├── rules/
│   ├── grc_rules_sora_2_0.yaml
│   └── grc_rules_sora_2_5.yaml
├── tests/
│   ├── __init__.py
│   ├── test_grc_calculator_2_0.py
│   └── test_grc_calculator_2_5.py
└── grc_api.py                 # FastAPI endpoints
```

### 8.2 Pydantic Models

```python
from pydantic import BaseModel, Field, field_validator, ConfigDict
from enum import Enum
from typing import List, Dict, Any, Literal
from datetime import datetime, UTC

class M1Level20(str, Enum):
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"

class M2Level20(str, Enum):
    NONE = "None"
    LOW = "Low"
    HIGH = "High"

class M3Level20(str, Enum):
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"

class GRCInputs20(BaseModel):
    """SORA 2.0 GRC inputs"""
    version: Literal["SORA_2.0"] = "SORA_2.0"
    population_density_p_km2: int = Field(ge=0, description="People per km²")
    environment_type: Optional[str] = Field(None, description="Rural/Suburban/Urban/Industrial/Controlled")
    containment_quality: str = Field(description="Poor/Adequate/Good")
    m1_strategic: M1Level20
    m2_impact: M2Level20
    m3_erp: M3Level20
    
    model_config = ConfigDict(use_enum_values=True)

class M1ALevel25(str, Enum):
    NONE = "None"
    LOW = "Low"
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

class GRCInputs25(BaseModel):
    """SORA 2.5 GRC inputs"""
    version: Literal["SORA_2.5"] = "SORA_2.5"
    characteristic_dimension_m: float = Field(gt=0, description="Aircraft dimension (m)")
    max_speed_mps: float = Field(gt=0, description="Maximum speed (m/s)")
    population_density_p_km2: int = Field(ge=0, description="People per km²")
    environment_type: Optional[str] = None
    containment_quality: str = Field(description="Poor/Adequate/Good")
    m1a_sheltering: M1ALevel25
    m1b_operational: M1BLevel25
    m1c_ground_observation: M1CLevel25
    m2_impact: M2Level25
    
    model_config = ConfigDict(use_enum_values=True)

class TraceEntry(BaseModel):
    """Calculation trace entry"""
    step: str
    inputs: Dict[str, Any]
    result: str
    rule_ref: str
    doc_ref: Dict[str, str]
    delta: Optional[int] = None
    timestamp: str = Field(default_factory=lambda: datetime.now(UTC).isoformat())
    notes: Optional[str] = None

class GRCResult(BaseModel):
    """GRC calculation result"""
    version: str
    initial_grc: int = Field(ge=1, le=8)
    residual_grc: int = Field(ge=1, le=8)
    total_reduction: int
    m1_reduction: Optional[int] = None
    m2_reduction: Optional[int] = None
    m3_adjustment: Optional[int] = None
    m1a_reduction: Optional[int] = None
    m1b_reduction: Optional[int] = None
    m1c_reduction: Optional[int] = None
    calculation_trace: List[TraceEntry]
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "version": "SORA_2.0",
                "initial_grc": 5,
                "residual_grc": 2,
                "total_reduction": 3,
                "m1_reduction": -2,
                "m2_reduction": -1,
                "m3_adjustment": 0
            }
        }
    )
```

### 8.3 YAML Rules Structure

**grc_rules_sora_2_0.yaml:**
```yaml
version: "EASA_SORA_2.0_AMC_2024-07"
source: "EASA Easy Access Rules for UAS (AMC/GM) - Step 2 & 3"
doc_id: "EASA_AMC_GM_UAS_2024"

# iGRC determination (extract from official Step 2 tables)
igrc_mapping:
  - population_range: [0, 10]
    base_igrc: 1
  - population_range: [10, 100]
    base_igrc: 2
  - population_range: [100, 500]
    base_igrc: 3
  # [COMPLETE FROM OFFICIAL TABLES]

# M1 reductions
m1_reductions:
  None: 0
  Low: -1
  Medium: -2
  High: -4

# M1 floor caps by containment quality
m1_floor_caps:
  Poor:
    1: 1
    2: 2
    3: 3
    4: 4
    5: 4
    6: 5
    7: 5
    8: 6
  Adequate:
    1: 1
    2: 1
    3: 2
    4: 3
    5: 3
    6: 4
    7: 4
    8: 5
  Good:
    1: 1
    2: 1
    3: 1
    4: 2
    5: 2
    6: 3
    7: 3
    8: 4

# M2 reductions
m2_reductions:
  None: 0
  Low: -1
  High: -2

# M3 adjustments
m3_adjustments:
  None: 1
  Low: 1
  Medium: 0
  High: -1
```

**grc_rules_sora_2_5.yaml:**
```yaml
version: "JARUS_SORA_2.5_2024"
source: "JARUS SORA 2.5 - Main Body + Annex F"
doc_id: "JARUS_SORA_2_5_2024"

# Quantitative model parameters (from Annex F)
quantitative_model:
  formula: "TBD"  # [EXTRACT FROM ANNEX F]
  
# M1A reductions (no High)
m1a_reductions:
  None: 0
  Low: -1
  Medium: -2
  # High: N/A

# M1B reductions (no Low)
m1b_reductions:
  None: 0
  # Low: N/A
  Medium: -1
  High: -2

# M1C reductions (no Medium/High)
m1c_reductions:
  None: 0
  Low: -1
  # Medium: N/A
  # High: N/A

# M2 reductions (no Low)
m2_reductions:
  None: 0
  # Low: N/A
  Medium: -1
  High: -2

# Floor caps (if applicable from model)
floor_caps:
  # [EXTRACT IF SPECIFIED IN ANNEX F]
```

### 8.4 Dependencies

```txt
pydantic>=2.0
pyyaml>=6.0
fastapi>=0.100
pytest>=7.0
hypothesis>=6.0
```

---

## 9. FastAPI Endpoints

### 9.1 SORA 2.0 Endpoint

```python
from fastapi import APIRouter, HTTPException
from arc.models.grc_models import GRCInputs20, GRCResult
from arc.calculators.grc_calculator import GRCCalculator20
from arc.validators.grc_validator import GRCValidator

router = APIRouter(prefix="/sora", tags=["GRC - Ground Risk Class"])

calc_20 = GRCCalculator20()

@router.post("/2.0/grc", response_model=GRCResult)
def calculate_grc_20(request: GRCInputs20):
    """
    Calculate GRC for SORA 2.0
    
    - Determines iGRC from population density
    - Applies M1 strategic mitigation with floor cap
    - Applies M2 impact effects
    - Applies M3 ERP adjustment
    - Returns final GRC with full trace
    """
    try:
        # Validate inputs
        validation_errors = GRCValidator.validate_sora_20_inputs(request.model_dump())
        if validation_errors:
            raise HTTPException(status_code=422, detail=validation_errors)
        
        # Calculate
        result = calc_20.calculate(request)
        
        # Validate result
        result_errors = GRCValidator.validate_grc_result(result.model_dump())
        if result_errors:
            raise HTTPException(status_code=500, detail=result_errors)
        
        return result
        
    except Exception as e:
        logger.error(f"GRC 2.0 calculation failed: {e}")
        raise HTTPException(status_code=500, detail=str(e))
```

### 9.2 SORA 2.5 Endpoint

```python
calc_25 = GRCCalculator25()

@router.post("/2.5/grc", response_model=GRCResult)
def calculate_grc_25(request: GRCInputs25):
    """
    Calculate GRC for SORA 2.5
    
    - Determines iGRC using quantitative model
    - Applies M1A/M1B/M1C (with N/A validation)
    - Applies M2 impact dynamics
    - Returns final GRC with full trace
    """
    try:
        # Validate inputs (including N/A checks)
        validation_errors = GRCValidator.validate_sora_25_inputs(request.model_dump())
        if validation_errors:
            raise HTTPException(status_code=422, detail=validation_errors)
        
        # Calculate
        result = calc_25.calculate(request)
        
        # Validate result
        result_errors = GRCValidator.validate_grc_result(result.model_dump())
        if result_errors:
            raise HTTPException(status_code=500, detail=result_errors)
        
        return result
        
    except Exception as e:
        logger.error(f"GRC 2.5 calculation failed: {e}")
        raise HTTPException(status_code=500, detail=str(e))
```

---

## 10. Deliverables Checklist

- [ ] `grc_rules_sora_2_0.yaml` - Complete rule file with iGRC table, M1/M2/M3 values, floor caps
- [ ] `grc_rules_sora_2_5.yaml` - Complete rule file with quantitative model, M1A/M1B/M1C/M2, N/A rules
- [ ] `grc_models.py` - All Pydantic models (GRCInputs20/25, GRCResult, TraceEntry, Enums)
- [ ] `grc_calculator.py` - GRCCalculator20 and GRCCalculator25 classes with floor cap algorithm
- [ ] `grc_validator.py` - Input/output validation including N/A checks
- [ ] `grc_api.py` - FastAPI router with /sora/2.0/grc and /sora/2.5/grc endpoints
- [ ] `test_grc_calculator_2_0.py` - 5 golden tests for SORA 2.0 including floor cap test
- [ ] `test_grc_calculator_2_5.py` - 3 golden tests for SORA 2.5 including N/A validation
- [ ] `README_GRC.md` - Usage documentation with examples

---

## 11. Acceptance Criteria

1. ✅ **Integer Values Only** - All M1/M2/M3 reductions are integers (no fractional)
2. ✅ **M1 Floor Cap Working** - Floor cap algorithm correctly prevents reduction below column floor
3. ✅ **Exact Mitigation Values** - M1/M2/M3 values match official EASA/JARUS tables
4. ✅ **N/A Validation (2.5)** - Reject M1A High, M1B Low, M1C Medium/High, M2 Low
5. ✅ **Sequential Application** - Mitigations applied in correct order: iGRC → M1(+floor) → M2 → M3
6. ✅ **Floor at 1** - Final GRC never less than 1
7. ✅ **Full Traceability** - Every trace entry has rule_ref with doc_id + standard + section
8. ✅ **YAML-Driven** - All logic in YAML, no hardcoded values in Python
9. ✅ **All Tests Pass** - 8+ golden tests + property-based tests all passing
10. ✅ **Deterministic** - Same inputs always produce same outputs
11. ✅ **422 Errors** - Invalid inputs return 422 with clear error messages
12. ✅ **Version Separation** - SORA 2.0 and 2.5 clearly separated (different classes, endpoints, rules)
13. ✅ **Documentation** - README with usage examples and API documentation
14. ✅ **Type Safety** - Pydantic models with proper validation
15. ✅ **Floor Cap Trace** - Floor cap application visible in calculation trace

---

## 12. COMPLETE OFFICIAL REFERENCE FILES

### 12.1 GRC_SORA20_Official_Logic.md

```markdown
# GRC — SORA 2.0 (EASA AMC/GM) — Official Logic Summary
Generated: 2025-10-30T09:42:25.045761Z

## Step #2 — Intrinsic/Initial GRC (iGRC)
- The **iGRC** is determined using the **SORA 2.0 Step 2 tables** (EASA AMC/GM; iGRC mapping).
- Core drivers (per official tables): **population density of the operational area**, **operational scenario footprint/containment**, and reference assumptions on kinetic severity per scenario.
- Output: **iGRC ∈ {1,2,3,4,5,6,7,8}** (integer class).

> Implementation note: treat `iGRC` as the **initial_grc** value in your API.

## Step #3 — Final GRC via Strategic & Technical Mitigations
Apply mitigations **in order** and then clamp to minimum 1:

1) **M1 — Strategic mitigations** (population exposure management / boundary & chronology / segregation / geo-awareness)  
   **Reductions (official 2.0 Table)**:  
   - None → **0**  
   - Low → **−1**  
   - Medium → **−2**  
   - High → **−4**  
   **M1 floor rule (cap):** M1 **cannot** reduce below the column/footprint floor defined for the iGRC table/containment context. Keep a floor *after* M1.

2) **M2 — Effects of impact** (energy absorption, frangibility, terminal fall dynamics, etc.)  
   **Reductions (official 2.0 Table)**:  
   - None → **0**  
   - Low → **−1**  
   - High → **−2**

3) **M3 — Emergency Response Plan (validated & effective)**  
   **Adjustments (official 2.0 rule):**  
   - **None/Low → +1** (penalty)  
   - **Medium → 0**  
   - **High → −1**

**Finalization:**  
`final_grc = max(1, iGRC + ΔM1 + ΔM2 + ΔM3)` with **M1 floor cap applied right after M1** before proceeding to M2/M3.

## Logging (for audits)
- Record evidence for each mitigation (design data, procedures, ERP validation).  
- Keep the intermediate states: **iGRC → after M1 (cap applied) → after M2 → after M3 → final clamp**.
```

### 12.2 GRC_SORA25_Official_Logic.md

```markdown
# GRC — SORA 2.5 (JARUS) — Official Logic Summary
Generated: 2025-10-30T09:42:25.045761Z

## Step #2 — Intrinsic/Initial GRC (iGRC) — Quantitative model
- **iGRC** is determined using the **quantitative ground-risk model** (Main Body + Annex F).  
- Required inputs: **characteristic dimension (m)**, **maximum speed (m/s)**, **population density (people/km²)**, **environment/context**, and **containment**.  
- Output: **iGRC ∈ {1..8}**.

## Step #3 — Final GRC via Mitigations (split M1 into A/B/C)
Apply mitigations in order and clamp minimum 1:

1) **M1A — Sheltering / population exposure reduction**  
   Allowed levels & effects: **None=0**, **Low=−1**, **Medium=−2**, **High = N/A**.

2) **M1B — Operational restrictions (impact angle/trajectory/temporal boundary)**  
   Allowed levels & effects: **None=0**, **Low=N/A**, **Medium=−1**, **High=−2**.

3) **M1C — Ground observation / tactical measures (population warning/clearing)**  
   Allowed levels & effects: **None=0**, **Low=−1**, **Medium=N/A**, **High=N/A**.

> **M1 cap rule:** As in 2.0, apply any **floor/cap** required by the model/containment context after M1 (per Main Body/Annex B–F references).

4) **M2 — Impact dynamics reduction (energy absorption/frangibility/terminal fall)**  
   Allowed levels & effects: **None=0**, **Low=N/A**, **Medium=−1**, **High=−2**.

**Finalization:**  
`final_grc = max(1, iGRC + ΔM1A + ΔM1B + ΔM1C + ΔM2)` with M1-related caps enforced where applicable.

## Evidence & sequencing
- Keep the **intermediate states** and the evidence for each sub‑mitigation (design data, test reports, procedures).  
- Ensure the **applicability** of each sub‑mitigation per Annex B/C/F criteria before applying its reduction.
```

### 12.3 GRC_SORA20_DROPDOWNS.json

```json
{
  "schema_version": "1.0",
  "standard": "SORA 2.0 (EASA AMC/GM) — GRC UI",
  "generated_utc": "2025-10-30T09:42:25.045761Z",
  "inputs": [
    {
      "name": "population_density_p_km2",
      "label": "Population Density (people/km²)",
      "type": "number",
      "required": true
    },
    {
      "name": "environment_type",
      "label": "Environment Type",
      "type": "enum",
      "enum": [
        "Rural",
        "Suburban",
        "Urban",
        "Industrial",
        "Controlled"
      ],
      "required": false
    },
    {
      "name": "containment_quality",
      "label": "Containment / Footprint Control",
      "type": "enum",
      "enum": [
        "Poor",
        "Adequate",
        "Good"
      ],
      "required": false
    },
    {
      "name": "m1_strategic",
      "label": "M1 Strategic Mitigation Level",
      "type": "enum",
      "enum": [
        "None",
        "Low",
        "Medium",
        "High"
      ],
      "required": true
    },
    {
      "name": "m2_impact",
      "label": "M2 Impact Effects Level",
      "type": "enum",
      "enum": [
        "None",
        "Low",
        "High"
      ],
      "required": true
    },
    {
      "name": "m3_erp",
      "label": "M3 ERP Level (validated & effective)",
      "type": "enum",
      "enum": [
        "None",
        "Low",
        "Medium",
        "High"
      ],
      "required": true
    }
  ],
  "outputs": [
    {
      "name": "initial_grc",
      "type": "integer",
      "enum": [
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8
      ]
    },
    {
      "name": "final_grc",
      "type": "integer",
      "enum": [
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8
      ]
    }
  ],
  "calculation_notes": [
    "Compute iGRC via Step 2 mapping table.",
    "Apply M1 with floor cap; then M2; then M3 (+1/0/−1 rule).",
    "Clamp final GRC to >= 1."
  ]
}
```

### 12.4 GRC_SORA25_DROPDOWNS.json

```json
{
  "schema_version": "1.0",
  "standard": "SORA 2.5 (JARUS) — GRC UI",
  "generated_utc": "2025-10-30T09:42:25.045761Z",
  "inputs": [
    {
      "name": "characteristic_dimension_m",
      "label": "Characteristic Dimension (m)",
      "type": "number",
      "required": true
    },
    {
      "name": "max_speed_mps",
      "label": "Maximum Speed (m/s)",
      "type": "number",
      "required": true
    },
    {
      "name": "population_density_p_km2",
      "label": "Population Density (people/km²)",
      "type": "number",
      "required": true
    },
    {
      "name": "environment_type",
      "label": "Environment Type",
      "type": "enum",
      "enum": [
        "Rural",
        "Suburban",
        "Urban",
        "Industrial",
        "Controlled"
      ],
      "required": false
    },
    {
      "name": "containment_quality",
      "label": "Containment / Footprint Control",
      "type": "enum",
      "enum": [
        "Poor",
        "Adequate",
        "Good"
      ],
      "required": false
    },
    {
      "name": "m1a_sheltering",
      "label": "M1A (Sheltering)",
      "type": "enum",
      "enum": [
        "None",
        "Low",
        "Medium"
      ],
      "required": true
    },
    {
      "name": "m1b_operational",
      "label": "M1B (Operational Restrictions)",
      "type": "enum",
      "enum": [
        "None",
        "Medium",
        "High"
      ],
      "required": true
    },
    {
      "name": "m1c_ground_observation",
      "label": "M1C (Ground Observation/Tactical)",
      "type": "enum",
      "enum": [
        "None",
        "Low"
      ],
      "required": true
    },
    {
      "name": "m2_impact",
      "label": "M2 (Impact Dynamics)",
      "type": "enum",
      "enum": [
        "None",
        "Medium",
        "High"
      ],
      "required": true
    }
  ],
  "outputs": [
    {
      "name": "initial_grc",
      "type": "integer",
      "enum": [
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8
      ]
    },
    {
      "name": "final_grc",
      "type": "integer",
      "enum": [
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8
      ]
    }
  ],
  "calculation_notes": [
    "Compute iGRC via quantitative model (Main Body + Annex F).",
    "Apply M1A/M1B/M1C (with N/A constraints) then M2.",
    "Apply any model-specific floor/cap after M1; clamp final >= 1."
  ]
}
```

---

## 13. Implementation Notes from MASTER_PROMPT_PACK

### 13.1 YAML-Driven Architecture

**From MASTER_PROMPT.md:**
- All calculation rules MUST be in YAML files
- NO hardcoded logic in Python (only YAML loading and execution)
- Version-pinned rule files with hash verification
- Support hot-reloading for regulatory updates

### 13.2 Deterministic Engine

**From ALGO_SPEC.md:**
- Zero randomness in calculations
- Same inputs ALWAYS produce same outputs
- Reproducible for audits and compliance verification
- Use integer arithmetic only (no floating-point errors)

### 13.3 Traceability Requirements

**From ACCEPTANCE_CRITERIA.md:**
- Every calculation step logged with timestamp
- Full rule_ref chain (doc_id + standard + section + page if applicable)
- Intermediate states captured (iGRC, after M1, after M2, after M3, final)
- Evidence linkage for audit compliance

### 13.4 Testing Standards

**From CHECKLIST.md:**
- Golden test cases covering all major scenarios
- Property-based tests with Hypothesis for edge cases
- Regression tests to prevent silent downgrades
- Performance tests (target: <50ms per calculation)

### 13.5 API Design

**From REFERENCES.md:**
- RESTful FastAPI endpoints
- JSON Schema validation on request/response
- 422 errors for validation failures with detailed messages
- 500 errors only for unexpected server issues
- Rate limiting and authentication (future)

---

## 14. CRITICAL REMINDERS FOR SONNET 4

⚠️ **BEFORE YOU START CODING:**

1. **Read ALL 4 reference files** in Section 12 - they contain EXACT values
2. **M1 Floor Cap Algorithm** is the MOST COMPLEX part - implement carefully
3. **N/A validation** for SORA 2.5 - REJECT invalid combinations with 422 error
4. **Integer values ONLY** - no fractional reductions (0.5, 1.5, etc.)
5. **Sequential order** - iGRC → M1(+floor) → M2 → M3 → max(1, result)
6. **Full traceability** - every step needs doc_ref with doc_id + standard + section
7. **Test floor cap** - create specific test case verifying floor prevents over-reduction

⚠️ **WHILE YOU CODE:**

- Extract EXACT values from reference files (don't guess or approximate)
- Use placeholders `[EXTRACT_FROM_EASA]` if official table not in references
- Keep M1 floor cap logic SEPARATE and VISIBLE in code
- Log intermediate states (after M1, after M2, after M3)
- Create enum types that match EXACTLY the allowed levels

⚠️ **WHEN YOU'RE DONE:**

- Run all golden tests - they must pass 100%
- Verify floor cap test shows floor being applied
- Check that N/A combinations raise ValidationError
- Confirm trace has full doc_ref for every entry
- Ensure no hardcoded values (all in YAML)

---

**END OF GRC SPECIFICATION FOR SONNET 4**

**Instructions for Sonnet 4:**
1. ⚠️ **READ THE CRITICAL RULES FIRST** (Section at top of document)
2. Read this entire specification carefully
3. Study the complete reference files in Section 12
4. **PAY SPECIAL ATTENTION** to M1 floor cap algorithm - it's complex!
5. Follow the MASTER_PROMPT requirements (deterministic, YAML-driven, traced)
6. Implement SORA 2.0 first, then SORA 2.5
7. Create all deliverables from Section 10 checklist
8. Run all tests from Section 7 before declaring complete
9. **Test the floor cap** - create specific test showing floor prevents over-reduction
10. Zero tolerance for deviations from EASA/JARUS standards

**Token/Cost Estimate:**
- File size: ~50KB
- Estimated tokens: ~12,500 tokens (at ~4 chars/token)
- Claude Sonnet 4 cost: ~$0.04 input + variable output cost
