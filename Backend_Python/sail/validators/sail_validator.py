"""SAIL calculation input and output validators."""

from typing import List, Dict, Any
from ..models.sail_models import (
    SAILValue, ARCValue, SAILInputs20, SAILInputs25, SAILResult,
    InvalidGRCError, InvalidARCError, SAILCalculationError
)


class SAILValidator:
    """Validator for SAIL calculation inputs and outputs."""
    
    def __init__(self, sora_version: str = "2.0"):
        """Initialize validator with SORA version."""
        self.sora_version = sora_version
    
    def validate_inputs(self, inputs) -> None:
        """Validate SAILInputs20 or SAILInputs25."""
        self.validate_sail_inputs(inputs.grc, inputs.arc.value if hasattr(inputs.arc, 'value') else inputs.arc)
    
    def is_valid_grc(self, grc) -> bool:
        """Check if GRC is valid (1-8 and integer)."""
        # Check if it's an integer (or float that equals an integer)
        if isinstance(grc, float):
            if grc != int(grc):
                return False
            grc = int(grc)
        elif not isinstance(grc, int):
            return False
        
        return 1 <= grc <= 8
    
    def is_valid_arc(self, arc) -> bool:
        """Check if ARC is valid (a-d)."""
        arc_str = arc.value if hasattr(arc, 'value') else arc
        return arc_str in ["a", "b", "c", "d"]
    
    @staticmethod
    def validate_sail_inputs(grc: int, arc: str) -> None:
        """
        Validate SAIL calculation inputs.
        
        Args:
            grc: Ground Risk Class value
            arc: Air Risk Class value
            
        Raises:
            InvalidGRCError: If GRC is out of range
            InvalidARCError: If ARC is invalid
        """
        # GRC range check
        if not isinstance(grc, int):
            raise InvalidGRCError(f"GRC must be an integer, got {type(grc).__name__}")
        
        if not 1 <= grc <= 8:
            raise InvalidGRCError(f"GRC must be 1-8, got {grc}")
        
        # ARC value check
        if not isinstance(arc, str):
            raise InvalidARCError(f"ARC must be a string, got {type(arc).__name__}")
        
        valid_arcs = ["a", "b", "c", "d"]
        if arc.lower() not in valid_arcs:
            raise InvalidARCError(f"ARC must be a/b/c/d, got '{arc}'")
    
    @staticmethod
    def validate_sail_inputs_20(inputs: SAILInputs20) -> None:
        """
        Validate SORA 2.0 SAIL inputs.
        
        Args:
            inputs: SORA 2.0 SAIL inputs
            
        Raises:
            InvalidGRCError: If GRC is invalid
            InvalidARCError: If ARC is invalid
        """
        # Για SORA 2.0: GRC επιτρεπτό 1–7 (GRC>7 ⇒ Κατηγορία C / εκτός πίνακα SAIL)
        if not isinstance(inputs.final_grc, int):
            raise InvalidGRCError(f"GRC must be an integer, got {type(inputs.final_grc).__name__}")
        if inputs.final_grc < 1 or inputs.final_grc > 7:
            raise InvalidGRCError(
                f"SORA 2.0: GRC πρέπει να είναι 1–7, got {inputs.final_grc}. Για GRC>7 απαιτείται Κατηγορία C (Certified)"
            )
        # ARC a–d
        if inputs.final_arc.value.lower() not in ["a", "b", "c", "d"]:
            raise InvalidARCError(f"ARC must be a/b/c/d, got '{inputs.final_arc.value}'")
        
        if inputs.version != "SORA_2.0":
            raise SAILCalculationError(f"Expected SORA_2.0 version, got {inputs.version}")
    
    @staticmethod
    def validate_sail_inputs_25(inputs: SAILInputs25) -> None:
        """
        Validate SORA 2.5 SAIL inputs.
        
        Args:
            inputs: SORA 2.5 SAIL inputs
            
        Raises:
            InvalidGRCError: If GRC is invalid
            InvalidARCError: If ARC is invalid
        """
        # Για SORA 2.5: GRC επιτρεπτό 1–8 σύμφωνα με Annex D
        if not isinstance(inputs.final_grc, int):
            raise InvalidGRCError(f"GRC must be an integer, got {type(inputs.final_grc).__name__}")
        if inputs.final_grc < 1 or inputs.final_grc > 8:
            raise InvalidGRCError(f"SORA 2.5: GRC πρέπει να είναι 1–8, got {inputs.final_grc}")
        # ARC a–d
        if inputs.final_arc.value.lower() not in ["a", "b", "c", "d"]:
            raise InvalidARCError(f"ARC must be a/b/c/d, got '{inputs.final_arc.value}'")
        
        if inputs.version != "SORA_2.5":
            raise SAILCalculationError(f"Expected SORA_2.5 version, got {inputs.version}")
    
    @staticmethod
    def validate_sail_result(result: SAILResult) -> None:
        """
        Validate SAIL calculation result.
        
        Args:
            result: SAIL calculation result
            
        Raises:
            SAILCalculationError: If result is invalid
        """
        # SAIL value check
        valid_sails = [e.value for e in SAILValue]
        if result.sail.value not in valid_sails:
            raise SAILCalculationError(f"Invalid SAIL: {result.sail}")
        
        # OSO count check
        expected_counts = {
            "I": 6, "II": 10, "III": 15,
            "IV": 18, "V": 21, "VI": 24
        }
        expected = expected_counts[result.sail.value]
        if result.oso_count != expected:
            raise SAILCalculationError(
                f"SAIL {result.sail.value} should have {expected} OSOs, got {result.oso_count}"
            )
        
        # OSO list length check
        if len(result.oso_requirements) != result.oso_count:
            raise SAILCalculationError(
                f"OSO list length {len(result.oso_requirements)} doesn't match "
                f"oso_count {result.oso_count}"
            )
        
        # Robustness levels check
        if len(result.robustness_levels) != result.oso_count:
            raise SAILCalculationError(
                f"Robustness levels count {len(result.robustness_levels)} doesn't match "
                f"oso_count {result.oso_count}"
            )
        
        # Trace validation
        if not result.calculation_trace:
            raise SAILCalculationError("Calculation trace cannot be empty")
        
        # Check trace has required steps
        trace_steps = [entry.step for entry in result.calculation_trace]
        required_steps = ["input_validation", "sail_determination"]
        missing_steps = [step for step in required_steps if step not in trace_steps]
        if missing_steps:
            raise SAILCalculationError(f"Missing required trace steps: {missing_steps}")
        
        # Version check
        valid_versions = ["SORA_2.0", "SORA_2.5"]
        if result.version not in valid_versions:
            raise SAILCalculationError(f"Invalid version: {result.version}")
        
        # Input consistency check
        if not 1 <= result.final_grc <= 8:
            raise SAILCalculationError(f"Invalid final_grc in result: {result.final_grc}")
        
        valid_arcs = [e.value for e in ARCValue]
        if result.final_arc.value not in valid_arcs:
            raise SAILCalculationError(f"Invalid final_arc in result: {result.final_arc}")
    
    @staticmethod
    def validate_oso_requirements(sail: SAILValue, oso_list: List[str]) -> None:
        """
        Validate OSO requirements for a SAIL level.
        
        Args:
            sail: SAIL level
            oso_list: List of OSO identifiers
            
        Raises:
            SAILCalculationError: If OSO list is invalid
        """
        expected_counts = {
            SAILValue.I: 6, SAILValue.II: 10, SAILValue.III: 15,
            SAILValue.IV: 18, SAILValue.V: 21, SAILValue.VI: 24
        }
        
        expected_count = expected_counts[sail]
        if len(oso_list) != expected_count:
            raise SAILCalculationError(
                f"SAIL {sail.value} should have {expected_count} OSOs, got {len(oso_list)}"
            )
        
        # Check OSO format
        valid_oso_pattern = r"^OSO#\d{2}$"
        import re
        for oso in oso_list:
            if not re.match(valid_oso_pattern, oso):
                raise SAILCalculationError(f"Invalid OSO format: {oso}")
        
        # Check for duplicates
        if len(set(oso_list)) != len(oso_list):
            duplicates = [oso for oso in set(oso_list) if oso_list.count(oso) > 1]
            raise SAILCalculationError(f"Duplicate OSOs found: {duplicates}")
        
        # Check OSO numbers are in valid range (01-24)
        for oso in oso_list:
            oso_num = int(oso.split('#')[1])
            if not 1 <= oso_num <= 24:
                raise SAILCalculationError(f"OSO number out of range: {oso}")
    
    @staticmethod
    def validate_robustness_levels(robustness_dict: Dict[str, str]) -> None:
        """
        Validate robustness levels dictionary.
        
        Args:
            robustness_dict: OSO -> robustness level mapping
            
        Raises:
            SAILCalculationError: If robustness levels are invalid
        """
        valid_levels = ["None", "Low", "Medium", "High"]
        
        for oso, level in robustness_dict.items():
            # Check OSO format
            if not re.match(r"^OSO#\d{2}$", oso):
                raise SAILCalculationError(f"Invalid OSO format in robustness: {oso}")
            
            # Check robustness level
            if level not in valid_levels:
                raise SAILCalculationError(
                    f"Invalid robustness level '{level}' for {oso}. "
                    f"Valid levels: {valid_levels}"
                )
    
    @staticmethod
    def validate_grc_arc_combination(grc: int, arc: str, version: str) -> None:
        """
        Validate GRC-ARC combination for specific SORA version.
        
        Args:
            grc: Ground Risk Class
            arc: Air Risk Class
            version: SORA version
            
        Raises:
            SAILCalculationError: If combination is invalid
        """
        SAILValidator.validate_sail_inputs(grc, arc)
        
        valid_versions = ["SORA_2.0", "SORA_2.5"]
        if version not in valid_versions:
            raise SAILCalculationError(f"Invalid SORA version: {version}")
        
        # All GRC-ARC combinations are valid for both versions
        # No additional validation needed beyond basic input validation


def validate_calculation_trace(trace: List[Dict[str, Any]]) -> None:
    """
    Validate calculation trace entries.
    
    Args:
        trace: List of trace entries
        
    Raises:
        SAILCalculationError: If trace is invalid
    """
    if not trace:
        raise SAILCalculationError("Calculation trace cannot be empty")
    
    required_fields = ["step", "inputs", "result", "rule_ref", "doc_ref", "timestamp"]
    
    for i, entry in enumerate(trace):
        # Check required fields
        missing_fields = [field for field in required_fields if field not in entry]
        if missing_fields:
            raise SAILCalculationError(
                f"Trace entry {i} missing required fields: {missing_fields}"
            )
        
        # Check doc_ref structure
        doc_ref = entry["doc_ref"]
        if not isinstance(doc_ref, dict):
            raise SAILCalculationError(f"Trace entry {i} doc_ref must be a dict")
        
        required_doc_fields = ["doc_id", "standard", "section"]
        missing_doc_fields = [field for field in required_doc_fields if field not in doc_ref]
        if missing_doc_fields:
            raise SAILCalculationError(
                f"Trace entry {i} doc_ref missing fields: {missing_doc_fields}"
            )


# Import re at the top level for all functions
import re