"""
Unit tests for SAIL validator.

Tests validation and normalization of GRC, ARC, and SORA version parameters.
"""

import pytest
from sail.sail_validator import SAILValidator, ValidationResult


class TestSAILValidator:
    """Test SAIL parameter validation across versions."""
    
    def setup_method(self):
        """Set up validator instance for each test."""
        self.validator = SAILValidator()
    
    def test_sora_20_valid_inputs(self):
        """Test SORA 2.0 valid inputs are accepted."""
        test_cases = [
            (3, 'a', '2.0'),
            (4, 'A', 'SORA_2_0'),
            (5, 'b', 2.0),
            (6, 'c', 'SORA 2.0'),
            (7, 'd', '2')
        ]
        
        for grc, arc, version in test_cases:
            result = self.validator.validate(grc, arc, version)
            assert result.ok, f"Should accept GRC={grc}, ARC={arc}, version={version}, got error: {result.error}"
            assert result.normalized_version == "2.0"
            assert result.normalized_grc == grc
            assert result.arc_letter in ['a', 'b', 'c', 'd']
            assert result.category is None  # No category C for GRC <= 7
            assert "Table D.1" in result.reference
    
    def test_sora_20_category_c(self):
        """Test SORA 2.0 GRC > 7 returns Category C."""
        result = self.validator.validate(8, 'a', '2.0')
        
        assert result.ok
        assert result.normalized_version == "2.0"
        assert result.normalized_grc == 8
        assert result.arc_letter == 'a'
        assert result.category == "C"
        assert "Table D.1" in result.reference
    
    def test_sora_20_invalid_arc(self):
        """Test SORA 2.0 rejects invalid ARC letters."""
        invalid_arcs = ['e', 'x', '1', 2]
        
        for arc in invalid_arcs:
            result = self.validator.validate(3, arc, '2.0')
            assert not result.ok, f"Should reject ARC={arc}"
            # Error should mention the phrase 'letter arc' regardless of original casing
            assert "letter arc" in result.error.lower()
    
    def test_sora_25_valid_inputs(self):
        """Test SORA 2.5 valid inputs are accepted."""
        test_cases = [
            (6, 4, '2.5'),
            (9, 1, 'SORA_2_5'),
            (10, 10, 2.5),
            (1, 1, 'SORA 2.5')
        ]
        
        for grc, arc_level, version in test_cases:
            result = self.validator.validate(grc, arc_level, version)
            assert result.ok, f"Should accept GRC={grc}, ARC={arc_level}, version={version}, got error: {result.error}"
            assert result.normalized_version == "2.5"
            assert result.normalized_grc == grc
            assert result.arc_level == arc_level
            assert result.arc_letter is None
            assert "Table 7" in result.reference
    
    def test_sora_25_invalid_grc(self):
        """Test SORA 2.5 rejects GRC outside 1-10 range."""
        invalid_grcs = [0, 11, -1, 15]
        
        for grc in invalid_grcs:
            result = self.validator.validate(grc, 5, '2.5')
            assert not result.ok, f"Should reject GRC={grc}"
            assert "GRC in [1..10]" in result.error
    
    def test_sora_25_invalid_arc_level(self):
        """Test SORA 2.5 rejects ARC level outside 1-10 range."""
        invalid_levels = [0, 11, -1, 15]
        
        for level in invalid_levels:
            result = self.validator.validate(5, level, '2.5')
            assert not result.ok, f"Should reject ARC level={level}"
            assert "1..10" in result.error
    
    def test_sora_25_rejects_letter_arc(self):
        """Test SORA 2.5 rejects letter ARC inputs."""
        letter_arcs = ['a', 'b', 'c', 'd', 'A', 'B']
        
        for arc in letter_arcs:
            result = self.validator.validate(5, arc, '2.5')
            assert not result.ok, f"Should reject letter ARC={arc}"
            assert "numeric residual ARC" in result.error
            assert "letter ARC is not valid" in result.error
    
    def test_version_normalization(self):
        """Test various version format inputs are normalized correctly."""
        version_variants = {
            '2.0': ['2.0', '2', 'SORA_2_0', 'SORA-2-0', 'sora 2.0'],
            '2.5': ['2.5', 'SORA_2_5', 'SORA-2-5', 'sora 2.5', '25']
        }
        
        for expected, variants in version_variants.items():
            for variant in variants:
                if expected == '2.0':
                    result = self.validator.validate(3, 'a', variant)
                else:
                    result = self.validator.validate(3, 1, variant)
                
                assert result.ok, f"Should accept version variant: {variant}"
                assert result.normalized_version == expected
    
    def test_invalid_version(self):
        """Test invalid version inputs are rejected."""
        invalid_versions = ['3.0', '1.0', 'invalid', '', None]
        
        for version in invalid_versions:
            result = self.validator.validate(3, 'a', version)
            assert not result.ok, f"Should reject invalid version: {version}"
            assert "Invalid SORA version" in result.error
    
    def test_golden_cases_sora_20(self):
        """Test SORA 2.0 golden cases from requirements."""
        golden_cases = [
            (3, 'a'),  # → SAIL I
            (4, 'a'),  # → SAIL II  
            (5, 'b'),  # → SAIL IV
            (6, 'b'),  # → SAIL IV
            (7, 'c'),  # → SAIL VI
            (7, 'd')   # → SAIL VI
        ]
        
        for grc, arc in golden_cases:
            result = self.validator.validate(grc, arc, '2.0')
            assert result.ok, f"Golden case GRC={grc}, ARC={arc} should be valid"
            assert result.normalized_grc == grc
            assert result.arc_letter == arc
            assert result.category is None
    
    def test_golden_cases_sora_25(self):
        """Test SORA 2.5 golden cases from requirements.""" 
        golden_cases = [
            (6, 4),   # Table 7 lookup
            (9, 1),   # → SAIL VI (GRC 9-10 rule)
            (10, 5)   # → SAIL VI (GRC 9-10 rule)
        ]
        
        for grc, arc_level in golden_cases:
            result = self.validator.validate(grc, arc_level, '2.5')
            assert result.ok, f"Golden case GRC={grc}, ARC={arc_level} should be valid"
            assert result.normalized_grc == grc
            assert result.arc_level == arc_level

