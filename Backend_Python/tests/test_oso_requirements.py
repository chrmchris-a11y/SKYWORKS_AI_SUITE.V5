import pytest
from sail.oso_requirements import get_oso_requirements_for_sail, get_oso_count_for_sail, SAILLevel


class TestOSORequirements:
    """Test suite for OSO requirements functionality"""

    def test_get_oso_requirements_for_sail_2_0(self):
        """Test that OSO requirements are returned for SORA 2.0"""
        requirements = get_oso_requirements_for_sail(SAILLevel.III, "2.0")
        
        assert requirements is not None
        assert len(requirements) > 0
        
        # Check structure of requirements
        for req in requirements:
            assert "id" in req
            assert "title" in req
            assert "robustness" in req
            assert req["id"].startswith("OSO#")
            assert req["robustness"] in ["Low", "Medium", "High"]

    def test_get_oso_requirements_for_sail_2_5(self):
        """Test that OSO requirements are returned for SORA 2.5"""
        requirements = get_oso_requirements_for_sail(SAILLevel.III, "2.5")
        
        assert requirements is not None
        assert len(requirements) > 0
        
        # Check structure of requirements
        for req in requirements:
            assert "id" in req
            assert "title" in req
            assert "robustness" in req
            assert req["id"].startswith("OSO#")
            assert req["robustness"] in ["Low", "Medium", "High"]

    def test_get_oso_requirements_for_sail_iv_2_5(self):
        """Test that OSO requirements are returned for SAIL IV in SORA 2.5"""
        requirements = get_oso_requirements_for_sail(SAILLevel.IV, "2.5")
        
        assert requirements is not None
        assert len(requirements) > 0
        
        # Check structure of requirements
        for req in requirements:
            assert "id" in req
            assert "title" in req
            assert "robustness" in req
            assert req["id"].startswith("OSO#")
            assert req["robustness"] in ["Low", "Medium", "High"]

    def test_get_oso_count_for_sail_2_0(self):
        """Test that OSO counts are returned correctly for SORA 2.0"""
        # Test expected counts for SORA 2.0
        assert get_oso_count_for_sail(SAILLevel.I, "2.0") == 6
        assert get_oso_count_for_sail(SAILLevel.II, "2.0") == 10
        assert get_oso_count_for_sail(SAILLevel.III, "2.0") == 15
        assert get_oso_count_for_sail(SAILLevel.IV, "2.0") == 18
        assert get_oso_count_for_sail(SAILLevel.V, "2.0") == 21
        assert get_oso_count_for_sail(SAILLevel.VI, "2.0") == 24

    def test_get_oso_count_for_sail_2_5(self):
        """Test that OSO counts return None for SORA 2.5"""
        # Test that all SAIL levels return None for SORA 2.5
        assert get_oso_count_for_sail(SAILLevel.I, "2.5") is None
        assert get_oso_count_for_sail(SAILLevel.II, "2.5") is None
        assert get_oso_count_for_sail(SAILLevel.III, "2.5") is None
        assert get_oso_count_for_sail(SAILLevel.IV, "2.5") is None
        assert get_oso_count_for_sail(SAILLevel.V, "2.5") is None
        assert get_oso_count_for_sail(SAILLevel.VI, "2.5") is None

    def test_invalid_version(self):
        """Test that invalid version raises appropriate error"""
        with pytest.raises(NotImplementedError):
            get_oso_requirements_for_sail(SAILLevel.III, "3.0")

    def test_all_sail_levels_2_0(self):
        """Test that all SAIL levels work for SORA 2.0"""
        for sail_level in SAILLevel:
            requirements = get_oso_requirements_for_sail(sail_level, "2.0")
            assert requirements is not None
            assert len(requirements) > 0

    def test_all_sail_levels_2_5(self):
        """Test that all SAIL levels work for SORA 2.5"""
        for sail_level in SAILLevel:
            requirements = get_oso_requirements_for_sail(sail_level, "2.5")
            assert requirements is not None
            assert len(requirements) > 0
