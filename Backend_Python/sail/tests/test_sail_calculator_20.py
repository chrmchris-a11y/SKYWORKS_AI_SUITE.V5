"""
Test suite for SAIL calculator using SORA 2.0 (EASA AMC/GM) standard.

Tests all 32 combinations of GRC (1-8) × ARC (a-d) to verify correct SAIL levels
and OSO counts according to SORA 2.0 specifications.

Doc References:
- EASA AMC/GM to Commission Implementing Regulation (EU) 2019/947
- SAIL_FORMULAS_AUTHORITATIVE.md: SORA 2.0 mapping tables
"""

import pytest
from sail.models.sail_models import (
    GRCLevel, ARCLevel, SAILLevel, SORAVersion,
    SAILCalculationRequest, SAILCalculationResponse
)
from sail.calculators.sail_calculator import SAILCalculator
from sail.calculators.oso_mapper import OSOMapper


class TestSAILCalculatorSORA20:
    """Test SAIL calculations for SORA 2.0 standard."""
    
    def setup_method(self):
        """Setup test fixtures."""
        self.calculator = SAILCalculator()
        self.oso_mapper = OSOMapper()
        self.sora_version = SORAVersion.SORA_2_0
    
    def _calculate_and_verify(self, grc: GRCLevel, arc: ARCLevel, expected_sail: SAILLevel, expected_oso: int):
        """Helper method to calculate and verify SAIL and OSO values."""
        request = SAILCalculationRequest(
            grc_level=grc,
            arc_level=arc,
            sora_version=self.sora_version
        )
        
        response = self.calculator.calculate_sail(request)
        oso_count = self.oso_mapper.get_oso_count(response.sail_level)
        
        assert response.sail_level == expected_sail, f"GRC{grc.value}×ARC{arc.value}: expected SAIL {expected_sail.value}, got {response.sail_level.value}"
        assert oso_count == expected_oso, f"GRC{grc.value}×ARC{arc.value}: expected {expected_oso} OSOs, got {oso_count}"
        assert response.sora_version == self.sora_version
        
        return response
    
    # GRC 1 tests (all combinations should yield SAIL I, I, II, III)
    def test_grc1_arca_sora20(self):
        """Test GRC 1 × ARC a = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_A, SAILLevel.I, 6)
    
    def test_grc1_arcb_sora20(self):
        """Test GRC 1 × ARC b = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_B, SAILLevel.I, 6)
    
    def test_grc1_arcc_sora20(self):
        """Test GRC 1 × ARC c = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_C, SAILLevel.II, 10)
    
    def test_grc1_arcd_sora20(self):
        """Test GRC 1 × ARC d = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_D, SAILLevel.III, 15)
    
    # GRC 2 tests (same as GRC 1 in SORA 2.0)
    def test_grc2_arca_sora20(self):
        """Test GRC 2 × ARC a = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_A, SAILLevel.I, 6)
    
    def test_grc2_arcb_sora20(self):
        """Test GRC 2 × ARC b = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_B, SAILLevel.I, 6)
    
    def test_grc2_arcc_sora20(self):
        """Test GRC 2 × ARC c = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_C, SAILLevel.II, 10)
    
    def test_grc2_arcd_sora20(self):
        """Test GRC 2 × ARC d = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_D, SAILLevel.III, 15)
    
    # GRC 3 tests (all combinations should yield SAIL II, II, III, IV)
    def test_grc3_arca_sora20(self):
        """Test GRC 3 × ARC a = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_A, SAILLevel.II, 10)
    
    def test_grc3_arcb_sora20(self):
        """Test GRC 3 × ARC b = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_B, SAILLevel.II, 10)
    
    def test_grc3_arcc_sora20(self):
        """Test GRC 3 × ARC c = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_C, SAILLevel.III, 15)
    
    def test_grc3_arcd_sora20(self):
        """Test GRC 3 × ARC d = SAIL IV (18 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_D, SAILLevel.IV, 18)
    
    # GRC 4 tests (same as GRC 3 in SORA 2.0)
    def test_grc4_arca_sora20(self):
        """Test GRC 4 × ARC a = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_A, SAILLevel.II, 10)
    
    def test_grc4_arcb_sora20(self):
        """Test GRC 4 × ARC b = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_B, SAILLevel.II, 10)
    
    def test_grc4_arcc_sora20(self):
        """Test GRC 4 × ARC c = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_C, SAILLevel.III, 15)
    
    def test_grc4_arcd_sora20(self):
        """Test GRC 4 × ARC d = SAIL IV (18 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_D, SAILLevel.IV, 18)
    
    # GRC 5 tests (all combinations should yield SAIL III, III, IV, V)
    def test_grc5_arca_sora20(self):
        """Test GRC 5 × ARC a = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_A, SAILLevel.III, 15)
    
    def test_grc5_arcb_sora20(self):
        """Test GRC 5 × ARC b = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_B, SAILLevel.III, 15)
    
    def test_grc5_arcc_sora20(self):
        """Test GRC 5 × ARC c = SAIL IV (18 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_C, SAILLevel.IV, 18)
    
    def test_grc5_arcd_sora20(self):
        """Test GRC 5 × ARC d = SAIL V (21 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_D, SAILLevel.V, 21)
    
    # GRC 6 tests (SORA 2.0: III, III, IV, V)
    def test_grc6_arca_sora20(self):
        """Test GRC 6 × ARC a = SAIL III (15 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_A, SAILLevel.III, 15)
    
    def test_grc6_arcb_sora20(self):
        """Test GRC 6 × ARC b = SAIL III (15 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_B, SAILLevel.III, 15)
    
    def test_grc6_arcc_sora20(self):
        """Test GRC 6 × ARC c = SAIL IV (18 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_C, SAILLevel.IV, 18)
    
    def test_grc6_arcd_sora20(self):
        """Test GRC 6 × ARC d = SAIL V (21 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_D, SAILLevel.V, 21)
    
    # GRC 7 tests (SORA 2.0: IV, IV, V, VI)
    def test_grc7_arca_sora20(self):
        """Test GRC 7 × ARC a = SAIL IV (18 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_A, SAILLevel.IV, 18)
    
    def test_grc7_arcb_sora20(self):
        """Test GRC 7 × ARC b = SAIL IV (18 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_B, SAILLevel.IV, 18)
    
    def test_grc7_arcc_sora20(self):
        """Test GRC 7 × ARC c = SAIL V (21 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_C, SAILLevel.V, 21)
    
    def test_grc7_arcd_sora20(self):
        """Test GRC 7 × ARC d = SAIL VI (24 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_D, SAILLevel.VI, 24)
    
    # GRC 8 tests (SORA 2.0: IV, IV, V, VI - same as GRC 7)
    def test_grc8_arca_sora20(self):
        """Test GRC 8 × ARC a = SAIL IV (18 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_A, SAILLevel.IV, 18)
    
    def test_grc8_arcb_sora20(self):
        """Test GRC 8 × ARC b = SAIL IV (18 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_B, SAILLevel.IV, 18)
    
    def test_grc8_arcc_sora20(self):
        """Test GRC 8 × ARC c = SAIL V (21 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_C, SAILLevel.V, 21)
    
    def test_grc8_arcd_sora20(self):
        """Test GRC 8 × ARC d = SAIL VI (24 OSOs) in SORA 2.0."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_D, SAILLevel.VI, 24)
    
    def test_sail_oso_count_consistency_sora20(self):
        """Test that all SAIL levels have correct OSO counts in SORA 2.0."""
        expected_oso_counts = {
            SAILLevel.I: 6,
            SAILLevel.II: 10,
            SAILLevel.III: 15,
            SAILLevel.IV: 18,
            SAILLevel.V: 21,
            SAILLevel.VI: 24
        }
        
        for sail_level, expected_count in expected_oso_counts.items():
            actual_count = self.oso_mapper.get_oso_count(sail_level)
            assert actual_count == expected_count, f"SAIL {sail_level.value}: expected {expected_count} OSOs, got {actual_count}"
    
    def test_sora20_version_consistency(self):
        """Test that all calculations consistently return SORA 2.0 version."""
        test_cases = [
            (GRCLevel.GRC_1, ARCLevel.ARC_A),
            (GRCLevel.GRC_4, ARCLevel.ARC_C),
            (GRCLevel.GRC_6, ARCLevel.ARC_A),  # Key difference case
            (GRCLevel.GRC_8, ARCLevel.ARC_D)
        ]
        
        for grc, arc in test_cases:
            request = SAILCalculationRequest(
                grc_level=grc,
                arc_level=arc,
                sora_version=self.sora_version
            )
            response = self.calculator.calculate_sail(request)
            assert response.sora_version == SORAVersion.SORA_2_0


class TestSAILCalculatorSORA20EdgeCases:
    """Test edge cases and boundary conditions for SORA 2.0."""
    
    def setup_method(self):
        """Setup test fixtures."""
        self.calculator = SAILCalculator()
        self.oso_mapper = OSOMapper()
    
    def test_lowest_risk_combination_sora20(self):
        """Test lowest risk combination (GRC 1 × ARC a) = SAIL I."""
        request = SAILCalculationRequest(
            grc_level=GRCLevel.GRC_1,
            arc_level=ARCLevel.ARC_A,
            sora_version=SORAVersion.SORA_2_0
        )
        response = self.calculator.calculate_sail(request)
        
        assert response.sail_level == SAILLevel.I
        assert self.oso_mapper.get_oso_count(response.sail_level) == 6
    
    def test_highest_risk_combination_sora20(self):
        """Test highest risk combination (GRC 8 × ARC d) = SAIL VI."""
        request = SAILCalculationRequest(
            grc_level=GRCLevel.GRC_8,
            arc_level=ARCLevel.ARC_D,
            sora_version=SORAVersion.SORA_2_0
        )
        response = self.calculator.calculate_sail(request)
        
        assert response.sail_level == SAILLevel.VI
        assert self.oso_mapper.get_oso_count(response.sail_level) == 24
    
    def test_sail_progression_consistency_sora20(self):
        """Test that SAIL levels progress logically with increased risk."""
        # Test GRC progression with fixed ARC
        arc = ARCLevel.ARC_C
        previous_sail_value = 0
        
        for grc in [GRCLevel.GRC_1, GRCLevel.GRC_3, GRCLevel.GRC_5, GRCLevel.GRC_7]:
            request = SAILCalculationRequest(
                grc_level=grc,
                arc_level=arc,
                sora_version=SORAVersion.SORA_2_0
            )
            response = self.calculator.calculate_sail(request)
            
            # Convert SAIL to numeric for comparison
            sail_numeric = {"I": 1, "II": 2, "III": 3, "IV": 4, "V": 5, "VI": 6}[response.sail_level.value]
            assert sail_numeric >= previous_sail_value, f"SAIL should not decrease with higher GRC"
            previous_sail_value = sail_numeric