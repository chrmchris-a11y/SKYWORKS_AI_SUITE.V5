"""
Test suite for SAIL calculator using SORA 2.5 (JARUS) standard.

Tests all 32 combinations of GRC (1-8) × ARC (a-d) to verify correct SAIL levels
and OSO counts according to SORA 2.5 specifications, with emphasis on differences
from SORA 2.0 at GRC levels 6-8.

Doc References:
- JARUS SORA 2.5 Guidelines
- SAIL_FORMULAS_AUTHORITATIVE.md: SORA 2.5 mapping tables and differences
"""

import pytest
from sail.models.sail_models import (
    GRCLevel, ARCLevel, SAILLevel, SORAVersion,
    SAILCalculationRequest, SAILCalculationResponse
)
from sail.calculators.sail_calculator import SAILCalculator
from sail.calculators.oso_mapper import OSOMapper


class TestSAILCalculatorSORA25:
    """Test SAIL calculations for SORA 2.5 standard."""
    
    def setup_method(self):
        """Setup test fixtures."""
        self.calculator = SAILCalculator()
        self.oso_mapper = OSOMapper()
        self.sora_version = SORAVersion.SORA_2_5
    
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
    
    # GRC 1 tests (identical to SORA 2.0: I, I, II, III)
    def test_grc1_arca_sora25(self):
        """Test GRC 1 × ARC a = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_A, SAILLevel.I, 6)
    
    def test_grc1_arcb_sora25(self):
        """Test GRC 1 × ARC b = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_B, SAILLevel.I, 6)
    
    def test_grc1_arcc_sora25(self):
        """Test GRC 1 × ARC c = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_C, SAILLevel.II, 10)
    
    def test_grc1_arcd_sora25(self):
        """Test GRC 1 × ARC d = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_1, ARCLevel.ARC_D, SAILLevel.III, 15)
    
    # GRC 2 tests (identical to SORA 2.0: I, I, II, III)
    def test_grc2_arca_sora25(self):
        """Test GRC 2 × ARC a = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_A, SAILLevel.I, 6)
    
    def test_grc2_arcb_sora25(self):
        """Test GRC 2 × ARC b = SAIL I (6 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_B, SAILLevel.I, 6)
    
    def test_grc2_arcc_sora25(self):
        """Test GRC 2 × ARC c = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_C, SAILLevel.II, 10)
    
    def test_grc2_arcd_sora25(self):
        """Test GRC 2 × ARC d = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_2, ARCLevel.ARC_D, SAILLevel.III, 15)
    
    # GRC 3 tests (identical to SORA 2.0: II, II, III, IV)
    def test_grc3_arca_sora25(self):
        """Test GRC 3 × ARC a = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_A, SAILLevel.II, 10)
    
    def test_grc3_arcb_sora25(self):
        """Test GRC 3 × ARC b = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_B, SAILLevel.II, 10)
    
    def test_grc3_arcc_sora25(self):
        """Test GRC 3 × ARC c = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_C, SAILLevel.III, 15)
    
    def test_grc3_arcd_sora25(self):
        """Test GRC 3 × ARC d = SAIL IV (18 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_3, ARCLevel.ARC_D, SAILLevel.IV, 18)
    
    # GRC 4 tests (identical to SORA 2.0: II, II, III, IV)
    def test_grc4_arca_sora25(self):
        """Test GRC 4 × ARC a = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_A, SAILLevel.II, 10)
    
    def test_grc4_arcb_sora25(self):
        """Test GRC 4 × ARC b = SAIL II (10 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_B, SAILLevel.II, 10)
    
    def test_grc4_arcc_sora25(self):
        """Test GRC 4 × ARC c = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_C, SAILLevel.III, 15)
    
    def test_grc4_arcd_sora25(self):
        """Test GRC 4 × ARC d = SAIL IV (18 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_4, ARCLevel.ARC_D, SAILLevel.IV, 18)
    
    # GRC 5 tests (identical to SORA 2.0: III, III, IV, V)
    def test_grc5_arca_sora25(self):
        """Test GRC 5 × ARC a = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_A, SAILLevel.III, 15)
    
    def test_grc5_arcb_sora25(self):
        """Test GRC 5 × ARC b = SAIL III (15 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_B, SAILLevel.III, 15)
    
    def test_grc5_arcc_sora25(self):
        """Test GRC 5 × ARC c = SAIL IV (18 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_C, SAILLevel.IV, 18)
    
    def test_grc5_arcd_sora25(self):
        """Test GRC 5 × ARC d = SAIL V (21 OSOs)."""
        self._calculate_and_verify(GRCLevel.GRC_5, ARCLevel.ARC_D, SAILLevel.V, 21)
    
    # GRC 6 tests (DIFFERENT from SORA 2.0: IV, IV, V, VI vs III, III, IV, V)
    def test_grc6_arca_sora25(self):
        """Test GRC 6 × ARC a = SAIL IV (18 OSOs) [was SAIL III in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_A, SAILLevel.IV, 18)
    
    def test_grc6_arcb_sora25(self):
        """Test GRC 6 × ARC b = SAIL IV (18 OSOs) [was SAIL III in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_B, SAILLevel.IV, 18)
    
    def test_grc6_arcc_sora25(self):
        """Test GRC 6 × ARC c = SAIL V (21 OSOs) [was SAIL IV in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_C, SAILLevel.V, 21)
    
    def test_grc6_arcd_sora25(self):
        """Test GRC 6 × ARC d = SAIL VI (24 OSOs) [was SAIL V in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_6, ARCLevel.ARC_D, SAILLevel.VI, 24)
    
    # GRC 7 tests (DIFFERENT from SORA 2.0: V, V, VI, VI vs IV, IV, V, VI)
    def test_grc7_arca_sora25(self):
        """Test GRC 7 × ARC a = SAIL V (21 OSOs) [was SAIL IV in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_A, SAILLevel.V, 21)
    
    def test_grc7_arcb_sora25(self):
        """Test GRC 7 × ARC b = SAIL V (21 OSOs) [was SAIL IV in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_B, SAILLevel.V, 21)
    
    def test_grc7_arcc_sora25(self):
        """Test GRC 7 × ARC c = SAIL VI (24 OSOs) [same as 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_C, SAILLevel.VI, 24)
    
    def test_grc7_arcd_sora25(self):
        """Test GRC 7 × ARC d = SAIL VI (24 OSOs) [same as 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_7, ARCLevel.ARC_D, SAILLevel.VI, 24)
    
    # GRC 8 tests (DIFFERENT from SORA 2.0: V, V, VI, VI vs IV, IV, V, VI)
    def test_grc8_arca_sora25(self):
        """Test GRC 8 × ARC a = SAIL V (21 OSOs) [was SAIL IV in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_A, SAILLevel.V, 21)
    
    def test_grc8_arcb_sora25(self):
        """Test GRC 8 × ARC b = SAIL V (21 OSOs) [was SAIL IV in 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_B, SAILLevel.V, 21)
    
    def test_grc8_arcc_sora25(self):
        """Test GRC 8 × ARC c = SAIL VI (24 OSOs) [same as 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_C, SAILLevel.VI, 24)
    
    def test_grc8_arcd_sora25(self):
        """Test GRC 8 × ARC d = SAIL VI (24 OSOs) [same as 2.0]."""
        self._calculate_and_verify(GRCLevel.GRC_8, ARCLevel.ARC_D, SAILLevel.VI, 24)