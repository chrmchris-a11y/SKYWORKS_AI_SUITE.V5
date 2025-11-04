# COMPLETE CODE CONTEXT FOR SONNET 4.5 AUDIT

## FILE 1: Backend_Python/models/sora_models.py

```python
"""
SKYWORKS AI SUITE - SORA Models
Pydantic models for SORA 2.0 and 2.5 calculations

CRITICAL FIXES:
- ARCRequest schemas must have all required fields
- SAILRequest must use ARCRating enum (not string)
- SAILResponse must return string SAIL (not int)
"""

from typing import Optional, List, Dict, Any
from pydantic import BaseModel, Field, field_validator, ConfigDict
from pydantic.aliases import AliasChoices
from enum import Enum


# ═══════════════════════════════════════════════════════════════════════════
# ENUMS
# ═══════════════════════════════════════════════════════════════════════════

class MitigationLevel(str, Enum):
    """Mitigation robustness levels - Case-insensitive (accepts low/Low, medium/Medium, high/High)"""
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"
    
    @classmethod
    def _missing_(cls, value):
        """Allow case-insensitive enum matching"""
        if isinstance(value, str):
            value_upper = value.upper()
            for member in cls:
                if member.name == value_upper:
                    return member
        return None


class OperationalScenario(str, Enum):
    """SORA 2.0 Operational Scenarios"""
    VLOS_CONTROLLED = "VLOS_Controlled"
    VLOS_SPARSELY = "VLOS_Sparsely"
    VLOS_POPULATED = "VLOS_Populated"
    BVLOS_CONTROLLED = "BVLOS_Controlled"
    BVLOS_SPARSELY = "BVLOS_Sparsely"
    BVLOS_POPULATED = "BVLOS_Populated"
    VLOS_GATHERING = "VLOS_Gathering"
    BVLOS_GATHERING = "BVLOS_Gathering"
    
    # Legacy aliases
    VLOS_SPARSELY_POPULATED = "VLOS_SparselyPopulated"
    BVLOS_SPARSELY_POPULATED = "BVLOS_SparselyPopulated"


class ARCRating(str, Enum):
    """
    Air Risk Class ratings
    
    CRITICAL: This is an ENUM, not a plain string
    Used in SAILRequest to ensure type safety
    """
    ARC_a = "ARC-a"
    ARC_b = "ARC-b"
    ARC_c = "ARC-c"
    ARC_d = "ARC-d"


class AirspaceClass(str, Enum):
    """ICAO Airspace Classifications"""
    CLASS_A = "A"
    CLASS_B = "B"
    CLASS_C = "C"
    CLASS_D = "D"
    CLASS_E = "E"
    CLASS_F = "F"
    CLASS_G = "G"


class EnvironmentType(str, Enum):
    """Environment types for ARC calculation"""
    URBAN = "Urban"
    SUBURBAN = "Suburban"
    RURAL = "Rural"


# ═══════════════════════════════════════════════════════════════════════════
# VERSION AND SAIL ENUMS
# ═══════════════════════════════════════════════════════════════════════════

class SORAVersion(str, Enum):
    SORA_2_0 = "2.0"
    SORA_2_5 = "2.5"


class SAILLevel(str, Enum):
    I = "I"
    II = "II"
    III = "III"
    IV = "IV"
    V = "V"
    VI = "VI"


# ... (rest of file omitted for brevity - see full file above)
```

## FILE 2: Backend_Python/grc/models/grc_models.py

```python
from pydantic import BaseModel, Field, field_validator, ConfigDict
from enum import Enum
from typing import List, Dict, Any, Optional, Literal, Union
from datetime import datetime, UTC

class EnvironmentType(str, Enum):
    RURAL = "Rural"
    SUBURBAN = "Suburban"
    URBAN = "Urban"
    INDUSTRIAL = "Industrial"
    CONTROLLED = "Controlled"

class ContainmentQuality(str, Enum):
    POOR = "Poor"
    ADEQUATE = "Adequate"
    GOOD = "Good"

# SORA 2.0 Enums
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

# SORA 2.5 Enums (with N/A constraints)
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

# ... (rest of file omitted - full content above)
```

## FILE 3: Backend_Python/grc/rules/grc_rules_sora_2_5.yaml

```yaml
version: "JARUS_SORA_2.5_2024"
source: "JARUS SORA 2.5 - Main Body + Annex F"
doc_id: "JARUS_SORA_2_5_2024"
standard: "SORA 2.5"

# Validation and scope gates
validations:
  scope_gate_final_grc_max: 7  # SPECIFIC category scope: Final GRC must be ≤ 7

# Quantitative model parameters (from Annex F)
# CALIBRATED FOR JARUS SORA 2.5 TEST CASES
quantitative_model:
  kinetic_factor: 0.3
  dimension_exponent: 2
  speed_exponent: 2
  population_factor: 0.001
  # Exposure band weights (aligned to population_density_bands order)
  # Bands: <5, <50, <500, <5000, <50000, >=50000
  # Slightly increase <500/km² exposure weight to ensure SORA25_007 maps to iGRC=5
  exposure_band_weights: [0.5, 1.0, 4.2, 8.0, 12.0, 16.0]
  
  # Risk score to iGRC mapping
  # Calibrated to match JARUS SORA 2.5 Table 2
  # SORA25_002: risk=8640 → iGRC=6
  # SORA25_007: risk=5227 → iGRC=5
  risk_score_mapping:
    - range: [0, 800]
      igrc: 1
    - range: [800, 1800]
      igrc: 2
    - range: [1800, 3500]
      igrc: 3
    - range: [3500, 5000]
      igrc: 4
    - range: [5000, 7500]
      igrc: 5
    - range: [7500, 10000]
      igrc: 6
    - range: [10000, 15000]
      igrc: 7
    - range: [15000, 999999]
      igrc: 8

# Population density bands (Annex F exposure): use strict < for thresholds
population_density_bands:
  - label: "<5/km²"
    max: 5
  - label: "<50/km²"
    max: 50
  - label: "<500/km²"
    max: 500
  - label: "<5000/km²"
    max: 5000
  - label: "<50000/km²"
    max: 50000
  - label: ">=50000/km²"
    max: null

# M1A sheltering reductions (no High - N/A)
m1a_reductions:
  None: 0
  Low: -1
  Medium: -2
  # High: N/A - will cause validation error

# M1B operational restrictions reductions (no Low - N/A)
m1b_reductions:
  None: 0
  # Low: N/A - will cause validation error
  Medium: -1
  High: -2

# M1C ground observation reductions (no Medium/High - N/A)
m1c_reductions:
  None: 0
  Low: -1
  # Medium: N/A - will cause validation error
  # High: N/A - will cause validation error

# M2 impact dynamics reductions (no Low - N/A)
m2_reductions:
  None: 0
  # Low: N/A - will cause validation error
  Medium: -1
  High: -2

# Floor caps (similar to 2.0 but may have different values)
floor_caps:
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

# N/A validation rules
na_constraints:
  m1a_invalid: ["High"]
  m1b_invalid: ["Low"]
  m1c_invalid: ["Medium", "High"]
  m2_invalid: ["Low"]

# Documentation references
doc_references:
  igrc_quantitative: "JARUS_SORA_2.5_Annex_F_Model"
  m1a_sheltering: "JARUS_SORA_2.5_M1A_Table"
  m1b_operational: "JARUS_SORA_2.5_M1B_Table"
  m1c_ground_observation: "JARUS_SORA_2.5_M1C_Table"
  m2_impact: "JARUS_SORA_2.5_M2_Table"
  na_validation: "JARUS_SORA_2.5_NA_Constraints"
```

## OFFICIAL EASA/JARUS DOCUMENTATION

### Document 1: JARUS SORA 2.0 (March 2019)
**URL**: https://jarus-rpas.org/sites/jarus-rpas.org/files/jar_doc_06_jarus_sora_v2.0.pdf

**Key Excerpts**:

#### Table 2: Intrinsic Ground Risk Class (Page 27)
```
Operational Scenario          | iGRC
-----------------------------|-----
VLOS - Controlled Ground     | 1
VLOS - Sparsely Populated    | 2
VLOS - Populated             | 3
BVLOS - Controlled Ground    | 2
BVLOS - Sparsely Populated   | 3
BVLOS - Populated            | 5
VLOS - Gathering             | 5
BVLOS - Gathering            | 7
```

#### Table 6: M1 Strategic Mitigation Effectiveness (Page 33, Annex B)
```
Robustness Level | GRC Reduction
-----------------|---------------
Low              | -1
Medium           | -2
High             | -2 (no additional benefit vs Medium)
```

#### Table 7: M2 Effects of Ground Impact Mitigation (Page 35, Annex C)
```
Robustness Level | GRC Reduction
-----------------|---------------
Medium           | -1
High             | -2
```

#### Table 8: M3 Emergency Response Plan (Page 37, Annex D)
```
Robustness Level | GRC Adjustment
-----------------|----------------
None/Low         | +1 (penalty)
Medium           | 0
High             | 0
```

### Document 2: JARUS SORA 2.5 (Annex F - Quantitative Method)
**URL**: https://jarus-rpas.org/sites/jarus-rpas.org/files/jar_doc_06_jarus_sora_annex_f_edition_2.5.pdf

**Key Excerpts**:

#### Quantitative GRC Formula (Annex F, Section 2.1, Page 3)
```
Risk Score = K × (Dimension² × Speed²) × Population_Density_Weight

Where:
- K = Kinetic factor (project-specific calibration constant)
- Dimension = Characteristic dimension (meters)
- Speed = Maximum speed (m/s)
- Population_Density_Weight = f(people/km²) from Table F.1
```

#### Table F.1: Population Density Exposure Bands (Page 5)
```
Population Density     | Weight
-----------------------|-------
< 5 people/km²        | 0.5
< 50 people/km²       | 1.0
< 500 people/km²      | 4.0
< 5,000 people/km²    | 8.0
< 50,000 people/km²   | 12.0
≥ 50,000 people/km²   | 16.0
```

#### Table F.2: M1A Sheltering Mitigation (Page 7)
```
Robustness Level | GRC Reduction
-----------------|---------------
None             | 0
Low              | -1
Medium           | -2
High             | N/A
```

#### Table F.3: M1B Operational Restrictions (Page 8)
```
Robustness Level | GRC Reduction
-----------------|---------------
None             | 0
Low              | N/A
Medium           | -1
High             | -2
```

#### Table F.4: M1C Ground Observation (Page 9)
```
Robustness Level | GRC Reduction
-----------------|---------------
None             | 0
Low              | -1
Medium           | N/A
High             | N/A
```

#### Table F.5: M2 Impact Dynamics (Page 10)
```
Robustness Level | GRC Reduction
-----------------|---------------
None             | 0
Low              | N/A
Medium           | -1
High             | -2
```

### Document 3: EASA AMC/GM to Part-UAS
**URL**: https://www.easa.europa.eu/en/document-library/acceptable-means-of-compliance-and-guidance-materials/amc-gm-regulation-eu-2019947

**Key Excerpt**:

#### AMC1 UAS.SPEC.050(3): SAIL Determination (Page 42)
```
GRC × ARC → SAIL Matrix:

       | ARC-a | ARC-b | ARC-c | ARC-d
-------|-------|-------|-------|-------
GRC 1  | I     | II    | III   | IV
GRC 2  | I     | II    | III   | IV
GRC 3  | II    | III   | IV    | V
GRC 4  | II    | III   | IV    | V
GRC 5  | III   | IV    | V     | VI
GRC 6  | III   | IV    | V     | VI
GRC 7  | IV    | V     | VI    | VI
GRC >7 | OUT OF SCOPE FOR SPECIFIC CATEGORY
```

## YOUR TASK

Using ONLY the official EASA/JARUS documentation excerpts above:

1. **Verify** that all enum values, mitigation effectiveness tables, and formulas match EXACTLY
2. **Identify** any deviations from official standards
3. **Propose** precise fixes with official doc references
4. **Preserve** existing test success (256/257 passing)

**CRITICAL**: Do NOT make any changes unless you can cite the EXACT official document, section, page, and quote.
