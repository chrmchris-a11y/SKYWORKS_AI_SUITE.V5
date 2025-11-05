"""
SAIL Validator for SORA compliance.

Centralizes strict version-aware validation and normalization for GRC, ARC, and SORA versions.

References:
- SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1
- SORA 2.5: JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7
"""

import re
from typing import Literal, Optional, Union
from dataclasses import dataclass


@dataclass
class ValidationResult:
    """
    Result of SAIL parameter validation.
    
    Fields:
    - ok: Whether validation passed
    - normalized_version: Normalized SORA version
    - normalized_grc: Validated GRC value
    - arc_letter: Normalized ARC letter for SORA 2.0
    - arc_level: ARC level for SORA 2.5
    - category: Category C flag for SORA 2.0 when GRC > 7
    - error: Error message if validation failed
    - reference: Reference to the authoritative table
    """
    ok: bool
    normalized_version: Optional[Literal["2.0", "2.5"]] = None
    normalized_grc: Optional[int] = None
    arc_letter: Optional[Literal["a", "b", "c", "d"]] = None
    arc_level: Optional[int] = None
    category: Optional[Literal["C"]] = None
    error: Optional[str] = None
    reference: Optional[str] = None


class SAILValidator:
    """
    Validator for SAIL calculation parameters across SORA versions.
    
    Provides version-aware validation for GRC and ARC parameters with proper
    normalization and error handling per SORA 2.0 and 2.5 requirements.
    """
    
    def validate(self, grc: Union[int, str], arc_or_level: Union[str, int], sora_version: Union[str, int, float]) -> ValidationResult:
        """
        Validate and normalize SAIL calculation parameters.
        
        Args:
            grc: Ground Risk Class (1-7 for SORA 2.0, 1-10 for SORA 2.5)
            arc_or_level: ARC letter for SORA 2.0 (a-d) or numeric residual ARC for SORA 2.5 (1-10)
            sora_version: SORA version (accepts various formats)
            
        Returns:
            ValidationResult with normalized parameters or error information
            
        References:
            - SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1
            - SORA 2.5: JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7
        """
        # Normalize SORA version first
        version_result = self._normalize_sora_version(sora_version)
        if not version_result.ok:
            return version_result
        
        normalized_version = version_result.normalized_version
        reference = self._get_reference(normalized_version)
        
        # Validate GRC
        grc_result = self._validate_grc(grc, normalized_version)
        if not grc_result.ok:
            grc_result.reference = reference
            return grc_result
        
        # Validate ARC first
        arc_result = self._validate_arc(arc_or_level, normalized_version)
        if not arc_result.ok:
            arc_result.reference = reference
            return arc_result
        
        # Check for Category C in SORA 2.0 (preserve normalized ARC letter)
        if normalized_version == "2.0" and grc_result.normalized_grc > 7:
            return ValidationResult(
                ok=True,
                normalized_version=normalized_version,
                normalized_grc=grc_result.normalized_grc,
                arc_letter=arc_result.arc_letter,
                category="C",
                reference=reference
            )

        # Combine results
        return ValidationResult(
            ok=True,
            normalized_version=normalized_version,
            normalized_grc=grc_result.normalized_grc,
            arc_letter=arc_result.arc_letter,
            arc_level=arc_result.arc_level,
            reference=reference
        )
    
    def _normalize_sora_version(self, version: Union[str, int, float]) -> ValidationResult:
        """Normalize SORA version input to standard format."""
        version_str = str(version).upper().strip()
        
        # Remove common prefixes and clean up
        version_str = re.sub(r'^SORA[\s_-]*', '', version_str)
        version_str = version_str.replace('_', '.').replace('-', '.')
        
        if version_str in ["2.0", "2", "20"]:
            return ValidationResult(ok=True, normalized_version="2.0")
        elif version_str in ["2.5", "25"]:
            return ValidationResult(ok=True, normalized_version="2.5")
        else:
            return ValidationResult(
                ok=False,
                error=f"Invalid SORA version: {version}. Must be '2.0' or '2.5'."
            )
    
    def _validate_grc(self, grc: Union[int, str], version: str) -> ValidationResult:
        """Validate Ground Risk Class parameter."""
        try:
            grc_int = int(grc)
        except (ValueError, TypeError):
            return ValidationResult(
                ok=False,
                error=f"GRC must be an integer, got: {grc}"
            )
        
        if version == "2.0":
            if grc_int < 1:
                return ValidationResult(
                    ok=False,
                    error="SORA 2.0 requires GRC >= 1"
                )
            # Allow GRC > 7 for Category C handling
            return ValidationResult(ok=True, normalized_grc=grc_int)
            
        elif version == "2.5":
            if not (1 <= grc_int <= 10):
                return ValidationResult(
                    ok=False,
                    error="SORA 2.5 requires GRC in [1..10]"
                )
            return ValidationResult(ok=True, normalized_grc=grc_int)
        
        return ValidationResult(ok=False, error="Unknown version for GRC validation")
    
    def _validate_arc(self, arc_or_level: Union[str, int], version: str) -> ValidationResult:
        """Validate ARC parameter based on SORA version."""
        if version == "2.0":
            return self._validate_arc_letter(arc_or_level)
        elif version == "2.5":
            return self._validate_arc_level(arc_or_level)
        
        return ValidationResult(ok=False, error="Unknown version for ARC validation")
    
    def _validate_arc_letter(self, arc: Union[str, int]) -> ValidationResult:
        """Validate ARC letter for SORA 2.0."""
        if isinstance(arc, (int, float)):
            return ValidationResult(
                ok=False,
                error="SORA 2.0 requires letter ARC (a, b, c, d), not numeric"
            )
        
        # Clean and normalize arc input
        arc_str = str(arc).strip().upper()
        arc_str = re.sub(r'^ARC[\s_-]*', '', arc_str)  # Remove ARC prefix
        
        arc_lower = arc_str.lower()
        if arc_lower in ['a', 'b', 'c', 'd']:
            return ValidationResult(ok=True, arc_letter=arc_lower)
        else:
            return ValidationResult(
                ok=False,
                error=f"SORA 2.0 requires letter ARC in {{a, b, c, d}}, got: {arc}"
            )
    
    def _validate_arc_level(self, level: Union[str, int]) -> ValidationResult:
        """Validate numeric residual ARC level for SORA 2.5."""
        # Check if input looks like a letter
        if isinstance(level, str) and level.strip().lower() in ['a', 'b', 'c', 'd']:
            return ValidationResult(
                ok=False,
                error="SORA 2.5 uses numeric residual ARC (1..10); letter ARC is not valid"
            )
        
        try:
            level_int = int(level)
        except (ValueError, TypeError):
            return ValidationResult(
                ok=False,
                error="SORA 2.5 requires numeric residual ARC (1..10)"
            )
        
        if not (1 <= level_int <= 10):
            return ValidationResult(
                ok=False,
                error="SORA 2.5 uses numeric residual ARC (1..10)"
            )
        
        return ValidationResult(ok=True, arc_level=level_int)
    
    def _get_reference(self, version: str) -> str:
        """Get the authoritative reference for the SORA version."""
        if version == "2.0":
            return "EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1"
        elif version == "2.5":
            return "JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7"
        return ""

