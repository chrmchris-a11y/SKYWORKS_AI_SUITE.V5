# COMPLETE GRC CALCULATOR IMPLEMENTATION REQUEST

## EXECUTIVE SUMMARY

**Mission**: Create **grc_calculator_complete.py** - A comprehensive, production-ready GRC calculator with 100% EASA/JARUS compliance

**Current Status**: We have `grc_calculator.py` (389 lines, 256/256 tests passing, 95% documented)

**Goal**: Create a COMPLETE alternative implementation with:
- ✅ **Enhanced architecture** with better separation of concerns
- ✅ **100% EASA/JARUS compliance** with full traceability
- ✅ **Comprehensive error handling** and validation
- ✅ **Production-ready code quality** with type hints, docstrings, examples
- ✅ **Complete test coverage** with edge cases
- ✅ **Performance optimizations** where applicable

---

## YOUR TASK

You must provide a **COMPLETE, PRODUCTION-READY** implementation of `grc_calculator_complete.py` that:

1. **Preserves ALL existing functionality** from `grc_calculator.py`
2. **Enhances architecture** with better class design
3. **Adds comprehensive documentation** with EASA/JARUS references
4. **Includes robust error handling** for all edge cases
5. **Provides complete traceability** to official documents
6. **Maintains 100% test compatibility** (256/256 tests must still pass)

---

## OFFICIAL EASA/JARUS DOCUMENTATION

### JARUS SORA 2.0 (March 2019)

#### Table 2: Initial GRC Determination (Page 27)

**AUTHORITATIVE SOURCE** for scenario-based iGRC determination.

```
Operational Scenarios × Dimension Brackets Matrix:

Dimension Brackets:
- ≤ 1m (Small UAS)
- > 1m and ≤ 3m (Medium UAS)
- > 3m and ≤ 8m (Large UAS)
- > 8m (Very Large UAS)

Scenarios and iGRC Values:
1. VLOS over Controlled Ground Area:     [1, 2, 3, 4]
2. VLOS over Sparsely Populated:         [2, 3, 4, 5]
3. VLOS over Populated Environment:      [3, 4, 5, 6]
4. VLOS over Assembly of People:         [4, 5, 6, 7]
5. BVLOS over Controlled Ground Area:    [2, 3, 4, 5]
6. BVLOS over Sparsely Populated:        [3, 4, 5, 6]
7. BVLOS over Populated Environment:     [4, 5, 6, 7]
8. BVLOS over Assembly of People:        [5, 6, 7, 8]
```

**Implementation Requirements**:
- ✅ EXACT values from table (no modifications)
- ✅ Dimension thresholds: ≤1m, ≤3m, ≤8m, >8m
- ✅ Document: "JARUS SORA 2.0 Table 2 (Page 27)"

---

#### M1 Strategic Mitigation with Floor Cap (Table 6, Page 33)

**THE MOST COMPLEX ALGORITHM** in SORA 2.0.

**M1 Reduction Values**:
```
None:   0
Low:   -1
Medium: -2
High:   -3
```

**Floor Cap Matrix** (CRITICAL):
```
Containment Quality: POOR
iGRC → Floor:  1→1, 2→2, 3→3, 4→4, 5→4, 6→5, 7→5, 8→6

Containment Quality: ADEQUATE
iGRC → Floor:  1→1, 2→1, 3→2, 4→3, 5→3, 6→4, 7→4, 8→5

Containment Quality: GOOD
iGRC → Floor:  1→1, 2→1, 3→1, 4→2, 5→2, 6→3, 7→3, 8→4
```

**Algorithm**:
```
Step 1: GRC_uncapped = iGRC + M1_reduction
Step 2: floor = FloorCapMatrix[containment][iGRC]
Step 3: GRC_after_M1 = max(floor, GRC_uncapped)
Step 4: actual_reduction = GRC_after_M1 - iGRC
```

**Implementation Requirements**:
- ✅ EXACT floor values (no approximations)
- ✅ Apply floor cap BEFORE M2/M3
- ✅ Document when floor is applied
- ✅ Return both requested and actual reduction

---

#### M2 Effects Mitigation (Table 7, Page 35)

```
None:   0
Low:   -1
Medium: -2
High:   -3
```

**Requirements**:
- ✅ Apply AFTER M1
- ✅ NO floor cap for M2
- ✅ Document: "JARUS SORA 2.0 Table 7 (Page 35)"

---

#### M3 Emergency Response Plan (Table 8, Page 37)

```
None:   0
Low:   -1
Medium: -2
High:   -3
```

**Requirements**:
- ✅ Apply AFTER M2
- ✅ NO floor cap for M3
- ✅ Terminology: "adjustment" not "reduction"
- ✅ Document: "JARUS SORA 2.0 Table 8 (Page 37)"

---

#### Final Floor Rule (Section 6.4, Page 40)

```
Final_GRC = max(1, GRC_after_M3)
```

**Requirements**:
- ✅ Apply as LAST step
- ✅ Document when floor is applied
- ✅ Document: "JARUS SORA 2.0 Section 6.4 (Page 40)"

---

### JARUS SORA 2.5 Annex F (March 2023)

#### Micro-UAS Exemption Rule (Section 1.2, Page 2)

**Official Rule**:
> "Unmanned aircraft with MTOM ≤ 250g operating at speeds ≤ 25 m/s 
> with maximum characteristic dimension < 1m shall be assigned iGRC = 1"

**CRITICAL**: ALL THREE conditions must be met (AND logic):
1. MTOM ≤ 250g (0.25 kg)
2. Speed ≤ 25 m/s
3. Dimension < 1m (strict <, not ≤)

**Requirements**:
- ✅ Check ALL THREE with AND
- ✅ Use strict < for dimension
- ✅ Apply BEFORE quantitative model
- ✅ Document: "JARUS SORA 2.5 Annex F Section 1.2 (Page 2)"

---

#### Quantitative Risk Model (Section 2.1, Page 3)

**Official Formula**:
```
Risk Score = K × (D² × v²) × ρ

Where:
- K = kinetic factor = 0.3 (CALIBRATED, not in official doc)
- D = dimension [m]
- v = speed [m/s]
- ρ = population density exposure factor
```

**Population Density Bands** (Table F.1, Page 5):
```
Band | Density [people/km²] | Weight (ρ)
-----|----------------------|------------
  1  | < 5                  |  0.5
  2  | < 50                 |  1.0
  3  | < 500                |  4.2 (CALIBRATED from 4.0)
  4  | < 5000               |  8.0
  5  | < 50000              | 12.0
  6  | ≥ 50000              | 16.0
```

**Risk Score → iGRC Mapping** (Section 2.2, Page 4):
```
[0, 800)        → iGRC 1
[800, 1800)     → iGRC 2
[1800, 3500)    → iGRC 3
[3500, 5000)    → iGRC 4
[5000, 7500)    → iGRC 5
[7500, 10000)   → iGRC 6
[10000, 15000)  → iGRC 7
[15000, ∞)      → iGRC 8
```

**Requirements**:
- ✅ Use EXACT formula structure
- ✅ Document K as calibrated
- ✅ Document band 3 calibration (4.0→4.2)
- ✅ Use half-open intervals [min, max)

---

#### M1A/M1B/M1C/M2 Mitigations (Tables F.2-F.5, Pages 7-10)

**M1A Sheltering** (Table F.2, Page 7):
```
None:   0
Low:   -1
Medium: -2
High:   N/A (explicitly not available)
```

**M1B Operational Restrictions** (Table F.3, Page 8):
```
None:   0
Low:    N/A (explicitly not available)
Medium: -1
High:   -2
```

**M1C Ground Observation** (Table F.4, Page 9):
```
None:   0
Low:   -1
Medium: N/A (explicitly not available)
High:   N/A (explicitly not available)
```

**M2 Impact Dynamics** (Table F.5, Page 10):
```
None:   0
Low:    N/A (explicitly not available)
Medium: -1
High:   -2
```

**Requirements**:
- ✅ EXACT reduction values
- ✅ Validate N/A constraints
- ✅ Document N/A in code comments
- ✅ Floor cap applies to combined M1A+M1B+M1C

---

## ENHANCED ARCHITECTURE REQUIREMENTS

### Class Design

```python
# Base Classes
class GRCCalculatorBase:
    """Base class with common functionality"""
    - _load_rules()
    - _create_trace_entry()
    - _validate_inputs()
    - _create_error_result()

class ValidationMixin:
    """Input validation utilities"""
    - validate_dimension()
    - validate_speed()
    - validate_mtom()
    - validate_mitigation_level()
    - validate_containment_quality()

class TracingMixin:
    """Enhanced tracing capabilities"""
    - start_calculation()
    - add_step()
    - add_validation_warning()
    - finalize_trace()

# Main Implementations
class GRCCalculator20Complete(GRCCalculatorBase, ValidationMixin, TracingMixin):
    """Enhanced SORA 2.0 calculator"""
    
class GRCCalculator25Complete(GRCCalculatorBase, ValidationMixin, TracingMixin):
    """Enhanced SORA 2.5 calculator"""

# Helper Classes
class FloorCapCalculator:
    """Dedicated floor cap algorithm implementation"""
    
class QuantitativeModelCalculator:
    """Dedicated quantitative model implementation"""

class MitigationApplicator:
    """Centralized mitigation application logic"""
```

### Error Handling

```python
class GRCCalculationError(Exception):
    """Base exception for GRC calculations"""

class InvalidInputError(GRCCalculationError):
    """Invalid input parameter"""

class OutOfScopeError(GRCCalculationError):
    """Operation outside SORA scope"""

class ConfigurationError(GRCCalculationError):
    """YAML configuration issue"""
```

### Type Hints

```python
from typing import Dict, Any, List, Tuple, Optional, Union
from dataclasses import dataclass
from enum import Enum

@dataclass
class GRCCalculationResult:
    """Strongly typed result object"""
    initial_grc: int
    final_grc: int
    calculation_trace: List[TraceEntry]
    validation_warnings: List[str]
    is_out_of_scope: bool
    error_message: Optional[str] = None
```

---

## IMPLEMENTATION REQUIREMENTS

### Documentation Standards

**Every method must have**:
1. **Comprehensive docstring** with:
   - One-line summary
   - Detailed description
   - Official EASA/JARUS reference with page number
   - Algorithm explanation
   - Example calculation
   - Args with types
   - Returns with types
   - Raises with exception types

2. **Inline comments** for:
   - Complex logic
   - EASA/JARUS compliance notes
   - Edge cases
   - Calibration notes

### Testing Compatibility

**CRITICAL**: Must maintain 100% compatibility with existing tests:
- Same input/output interface as current `grc_calculator.py`
- Same method signatures for `calculate()`
- Same return types (GRCResult)
- Same YAML rules structure

### Performance Considerations

- Use `@lru_cache` for floor cap lookups
- Pre-compute risk score thresholds
- Optimize population band searches
- Minimize YAML re-parsing

---

## OUTPUT REQUIREMENTS

### 1. Complete Python File

Provide the **COMPLETE** `grc_calculator_complete.py` (estimated 800-1000 lines) with:

```python
"""
grc_calculator_complete.py - Complete EASA/JARUS Compliant GRC Calculator

This is a comprehensive, production-ready implementation with:
- 100% EASA/JARUS compliance with full traceability
- Enhanced architecture with separation of concerns
- Robust error handling and validation
- Comprehensive documentation
- Type hints and dataclasses
- Performance optimizations

Official References:
- JARUS SORA 2.0 (March 2019)
- JARUS SORA 2.5 Annex F (March 2023)
- EASA AMC/GM Part-UAS

Compliance Status: 100% EASA/JARUS compliant
Test Status: 256/256 tests passing
Version: 2.0.0
Last Updated: 2025-11-03
"""

# [FULL IMPLEMENTATION HERE - 800-1000 LINES]
```

### 2. Implementation Report

```markdown
## IMPLEMENTATION REPORT

### Architecture Improvements
- [List all architectural enhancements]

### EASA/JARUS Compliance
- SORA 2.0: 100% ✓
- SORA 2.5: 100% ✓
- All tables verified: ✓
- All formulas verified: ✓

### Code Quality Metrics
- Lines of code: [X]
- Docstring coverage: 100%
- Type hint coverage: 100%
- Cyclomatic complexity: [X]

### Test Compatibility
- Existing tests: 256/256 passing ✓
- New edge case tests: [X] added
- Error handling tests: [X] added

### Performance
- Floor cap lookups: O(1) via caching
- Quantitative model: O(log n) band search
- Overall: < 1ms per calculation
```

### 3. Migration Guide

```markdown
## MIGRATION GUIDE

### Switching to Complete Calculator

```python
# Old
from grc.calculators.grc_calculator import GRCCalculator20, GRCCalculator25

# New
from grc.calculators.grc_calculator_complete import (
    GRCCalculator20Complete, 
    GRCCalculator25Complete
)

# Use identical interface
calc = GRCCalculator25Complete()
result = calc.calculate(inputs)  # Same as before
```

### Benefits
- Better error messages
- Enhanced validation
- Full traceability
- Production-ready
```

---

## CRITICAL CONSTRAINTS

### DO NOT BREAK:
- ❌ Do NOT change YAML structure
- ❌ Do NOT change method signatures for `calculate()`
- ❌ Do NOT change return types
- ❌ Do NOT change enum values
- ❌ Do NOT modify test expectations

### MUST DO:
- ✅ Use EXACT same YAML files
- ✅ Return same GRCResult structure
- ✅ Maintain 256/256 test compatibility
- ✅ Use ASCII-only in docstrings
- ✅ Add comprehensive error handling
- ✅ Include type hints everywhere
- ✅ Document every algorithm with EASA/JARUS refs

---

## EXAMPLE CODE STRUCTURE

```python
class GRCCalculator20Complete(GRCCalculatorBase, ValidationMixin):
    """
    Complete JARUS SORA 2.0 GRC Calculator with Enhanced Features
    
    Official Reference: JARUS SORA 2.0 (March 2019)
    
    Features:
    - 100% EASA/JARUS compliant
    - Comprehensive input validation
    - Detailed error messages
    - Full calculation traceability
    - Production-ready error handling
    
    Example:
        >>> calc = GRCCalculator20Complete()
        >>> inputs = GRCInputs20(
        ...     scenario="VLOS_Sparsely",
        ...     dimension_m=0.6,
        ...     m1_strategic="Medium",
        ...     m2_impact="Low",
        ...     m3_erp="None",
        ...     containment_quality="Good"
        ... )
        >>> result = calc.calculate(inputs)
        >>> print(f"Final GRC: {result.residual_grc}")
        Final GRC: 1
    """
    
    def __init__(self):
        """Initialize calculator with SORA 2.0 rules"""
        super().__init__("grc_rules_sora_2_0.yaml")
        self._validate_rules()
    
    def calculate(self, inputs: GRCInputs20) -> GRCResult:
        """
        Calculate GRC using JARUS SORA 2.0 methodology.
        
        [COMPREHENSIVE DOCSTRING WITH FULL ALGORITHM]
        """
        # Validate inputs
        self._validate_inputs(inputs)
        
        # [IMPLEMENTATION WITH DETAILED COMMENTS]
        
        return result
```

---

## EXPECTED OUTCOME

After implementing `grc_calculator_complete.py`:

✅ **256/256 tests still PASSING**
✅ **100% EASA/JARUS compliance**
✅ **Production-ready code quality**
✅ **Complete documentation**
✅ **Robust error handling**
✅ **Enhanced architecture**
✅ **Full traceability**

**Ready to create the COMPLETE implementation?**
