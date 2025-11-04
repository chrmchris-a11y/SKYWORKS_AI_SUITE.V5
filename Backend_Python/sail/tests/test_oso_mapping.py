import pytest
from sail.calculators.oso_mapper import OSOMapper
from sail.models.sail_models import SAILValue, RobustnessLevel


class TestOSOMapping:
    """Test OSO requirements mapping functionality."""
    
    def setup_method(self):
        """Setup test fixtures."""
        self.mapper_20 = OSOMapper(sora_version="2.0")
        self.mapper_25 = OSOMapper(sora_version="2.5")
    
    def test_sail_i_mapping_20(self):
        """Test SAIL I OSO mapping for SORA 2.0."""
        oso_reqs = self.mapper_20.get_oso_requirements(SAILValue.I)
        
        assert len(oso_reqs) == 6
        # SAIL I has OSO #1-#6
        assert "OSO #1" in oso_reqs
        assert "OSO #2" in oso_reqs
        assert "OSO #3" in oso_reqs
        assert "OSO #4" in oso_reqs
        assert "OSO #5" in oso_reqs
        assert "OSO #6" in oso_reqs
        
        # Check robustness levels (all LOW for SAIL I)
        assert oso_reqs["OSO #1"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #2"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #3"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #4"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #5"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #6"] == RobustnessLevel.LOW
    
    def test_sail_ii_mapping_20(self):
        """Test SAIL II OSO mapping for SORA 2.0."""
        oso_reqs = self.mapper_20.get_oso_requirements(SAILValue.II)
        
        assert len(oso_reqs) == 10
        # SAIL II has OSO #1-#10 (cumulative)
        for i in range(1, 11):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels (all LOW for SAIL II)
        assert oso_reqs["OSO #1"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #5"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #10"] == RobustnessLevel.LOW
    
    def test_sail_iii_mapping_20(self):
        """Test SAIL III OSO mapping for SORA 2.0."""
        oso_reqs = self.mapper_20.get_oso_requirements(SAILValue.III)
        
        assert len(oso_reqs) == 15
        # SAIL III has OSO #1-#15
        for i in range(1, 16):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels (first 6 MEDIUM, rest LOW)
        assert oso_reqs["OSO #1"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #6"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #7"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #15"] == RobustnessLevel.LOW
    
    def test_sail_iv_mapping_20(self):
        """Test SAIL IV OSO mapping for SORA 2.0."""
        oso_reqs = self.mapper_20.get_oso_requirements(SAILValue.IV)
        
        assert len(oso_reqs) == 18
        # SAIL IV has OSO #1-#18
        for i in range(1, 19):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels
        assert oso_reqs["OSO #1"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #10"] == RobustnessLevel.MEDIUM
    
    def test_sail_v_mapping_20(self):
        """Test SAIL V OSO mapping for SORA 2.0."""
        oso_reqs = self.mapper_20.get_oso_requirements(SAILValue.V)
        
        assert len(oso_reqs) == 21
        # SAIL V has OSO #1-#21
        for i in range(1, 22):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels (1-6: HIGH, 7-18: MEDIUM, 19-21: LOW)
        assert oso_reqs["OSO #1"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #6"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #7"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #18"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #19"] == RobustnessLevel.LOW
        assert oso_reqs["OSO #21"] == RobustnessLevel.LOW
    
    def test_sail_vi_mapping_20(self):
        """Test SAIL VI OSO mapping for SORA 2.0."""
        oso_reqs = self.mapper_20.get_oso_requirements(SAILValue.VI)
        
        assert len(oso_reqs) == 24
        # SAIL VI has all OSO #1-#24
        for i in range(1, 25):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels
        assert oso_reqs["OSO #1"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #6"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #10"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #19"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #22"] == RobustnessLevel.LOW
    
    def test_sail_i_mapping_25(self):
        """Test SAIL I OSO mapping for SORA 2.5."""
        oso_reqs = self.mapper_25.get_oso_requirements(SAILValue.I)
        
        assert len(oso_reqs) == 6
        # SAIL I has OSO #1-#6 (same for 2.0 and 2.5)
        for i in range(1, 7):
            assert f"OSO #{i}" in oso_reqs
        
        # All should be low robustness for SAIL I
        for oso, robustness in oso_reqs.items():
            assert robustness == RobustnessLevel.LOW
    
    def test_sail_ii_mapping_25(self):
        """Test SAIL II OSO mapping for SORA 2.5."""
        oso_reqs = self.mapper_25.get_oso_requirements(SAILValue.II)
        
        assert len(oso_reqs) == 10
        # SAIL II has OSO #1-#10 (same for 2.0 and 2.5)
        for i in range(1, 11):
            assert f"OSO #{i}" in oso_reqs
    
    def test_sail_iii_mapping_25(self):
        """Test SAIL III OSO mapping for SORA 2.5."""
        oso_reqs = self.mapper_25.get_oso_requirements(SAILValue.III)
        
        assert len(oso_reqs) == 15
        # SAIL III has OSO #1-#15 (same for 2.0 and 2.5)
        for i in range(1, 16):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels
        assert oso_reqs["OSO #1"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #7"] == RobustnessLevel.LOW
    
    def test_sail_iv_mapping_25(self):
        """Test SAIL IV OSO mapping for SORA 2.5."""
        oso_reqs = self.mapper_25.get_oso_requirements(SAILValue.IV)
        
        assert len(oso_reqs) == 18
        # SAIL IV has OSO #1-#18 (same for 2.0 and 2.5)
        for i in range(1, 19):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels
        assert oso_reqs["OSO #1"] == RobustnessLevel.MEDIUM
    
    def test_sail_v_mapping_25(self):
        """Test SAIL V OSO mapping for SORA 2.5."""
        oso_reqs = self.mapper_25.get_oso_requirements(SAILValue.V)
        
        assert len(oso_reqs) == 21
        # SAIL V has OSO #1-#21 (same for 2.0 and 2.5)
        for i in range(1, 22):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels
        assert oso_reqs["OSO #1"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #7"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #19"] == RobustnessLevel.LOW
    
    def test_sail_vi_mapping_25(self):
        """Test SAIL VI OSO mapping for SORA 2.5."""
        oso_reqs = self.mapper_25.get_oso_requirements(SAILValue.VI)
        
        assert len(oso_reqs) == 24
        # SAIL VI has all OSO #1-#24 (same for 2.0 and 2.5)
        for i in range(1, 25):
            assert f"OSO #{i}" in oso_reqs
        
        # Check robustness levels (same as SORA 2.0)
        assert oso_reqs["OSO #1"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #10"] == RobustnessLevel.HIGH
        assert oso_reqs["OSO #19"] == RobustnessLevel.MEDIUM
        assert oso_reqs["OSO #22"] == RobustnessLevel.LOW
    
    def test_robustness_level_progression(self):
        """Test that robustness levels generally increase with SAIL level."""
        # Compare SAIL I vs SAIL VI for key OSOs
        sail_i_reqs = self.mapper_20.get_oso_requirements(SAILValue.I)
        sail_vi_reqs = self.mapper_20.get_oso_requirements(SAILValue.VI)
        
        # OSO #1 should progress from LOW to HIGH
        assert sail_i_reqs["OSO #1"] == RobustnessLevel.LOW
        assert sail_vi_reqs["OSO #1"] == RobustnessLevel.HIGH
        
        # OSO #2 should progress from LOW to HIGH
        assert sail_i_reqs["OSO #2"] == RobustnessLevel.LOW
        assert sail_vi_reqs["OSO #2"] == RobustnessLevel.HIGH
    
    def test_oso_count_progression(self):
        """Test that OSO count increases with SAIL level."""
        sail_levels = [SAILValue.I, SAILValue.II, SAILValue.III, 
                      SAILValue.IV, SAILValue.V, SAILValue.VI]
        expected_counts = [6, 10, 15, 18, 21, 24]
        
        for i, sail_level in enumerate(sail_levels):
            oso_reqs_20 = self.mapper_20.get_oso_requirements(sail_level)
            oso_reqs_25 = self.mapper_25.get_oso_requirements(sail_level)
            
            assert len(oso_reqs_20) == expected_counts[i]
            assert len(oso_reqs_25) == expected_counts[i]
    
    def test_version_differences(self):
        """Test differences between SORA 2.0 and 2.5 mappings."""
        # Both versions should have same OSO counts
        for sail_level in [SAILValue.I, SAILValue.II, SAILValue.III, 
                          SAILValue.IV, SAILValue.V, SAILValue.VI]:
            oso_reqs_20 = self.mapper_20.get_oso_requirements(sail_level)
            oso_reqs_25 = self.mapper_25.get_oso_requirements(sail_level)
            
            assert len(oso_reqs_20) == len(oso_reqs_25)
            assert set(oso_reqs_20.keys()) == set(oso_reqs_25.keys())
    
    def test_invalid_sail_value(self):
        """Test handling of invalid SAIL values."""
        with pytest.raises(ValueError, match="Invalid SAIL value"):
            self.mapper_20.get_oso_requirements("INVALID")
        
        with pytest.raises(ValueError, match="Invalid SAIL value"):
            self.mapper_25.get_oso_requirements(None)
    
    def test_all_robustness_levels_present(self):
        """Test that all robustness levels are used across SAIL levels."""
        all_robustness_levels = set()
        
        for sail_level in [SAILValue.I, SAILValue.II, SAILValue.III, 
                          SAILValue.IV, SAILValue.V, SAILValue.VI]:
            oso_reqs = self.mapper_20.get_oso_requirements(sail_level)
            all_robustness_levels.update(oso_reqs.values())
        
        assert RobustnessLevel.LOW in all_robustness_levels
        assert RobustnessLevel.MEDIUM in all_robustness_levels
        assert RobustnessLevel.HIGH in all_robustness_levels