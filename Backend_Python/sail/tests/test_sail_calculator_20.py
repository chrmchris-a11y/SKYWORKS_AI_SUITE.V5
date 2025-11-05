"""Unit tests for SORA 2.0 SAIL calculator (authoritative acceptance cases)."""

import pytest
from sail.calculators.sail_calculator import SAILCalculator
from sail.models.sail_models import (
    SAILCalculationRequest,
    SAILLevel,
    ARCLevel,
    SORAVersion,
)


@pytest.fixture
def calculator():
    """Fixture for SAILCalculator instance."""
    return SAILCalculator()


class TestSAIL20Acceptance:
    """Acceptance tests for SORA 2.0 (EASA AMC/GM Annex D Table D.1)."""

    def test_grc3_arc_a_returns_sail_i(self, calculator):
        """GRC 3, ARC-a → SAIL I (per Table D.1)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=3,
            arc_level=ARCLevel.ARC_A,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.I
        assert response.sail == SAILLevel.I
        assert response.grc_level == 3
        assert response.arc_level == ARCLevel.ARC_A
        assert "EASA AMC/GM SORA 2.0 Annex D" in response.reference

    def test_grc4_arc_a_returns_sail_ii(self, calculator):
        """GRC 4, ARC-a → SAIL II (per Table D.1)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=4,
            arc_level=ARCLevel.ARC_A,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.II
        assert response.sail == SAILLevel.II

    def test_grc5_arc_b_returns_sail_iv(self, calculator):
        """GRC 5, ARC-b → SAIL IV (per Table D.1)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=5,
            arc_level=ARCLevel.ARC_B,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.IV
        assert response.sail == SAILLevel.IV

    def test_grc6_arc_b_returns_sail_iv(self, calculator):
        """GRC 6, ARC-b → SAIL IV (per Table D.1)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=6,
            arc_level=ARCLevel.ARC_B,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.IV
        assert response.sail == SAILLevel.IV

    def test_grc7_arc_c_returns_sail_vi(self, calculator):
        """GRC 7, ARC-c → SAIL VI (per Table D.1)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=7,
            arc_level=ARCLevel.ARC_C,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.VI
        assert response.sail == SAILLevel.VI

    def test_grc7_arc_d_returns_sail_vi(self, calculator):
        """GRC 7, ARC-d → SAIL VI (per Table D.1)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=7,
            arc_level=ARCLevel.ARC_D,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.VI
        assert response.sail == SAILLevel.VI

    def test_grc8_arc_a_returns_category_c(self, calculator):
        """GRC 8, ARC-a → Category C (no SAIL)."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_0,
            grc_level=8,
            arc_level=ARCLevel.ARC_A,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level is None
        assert response.sail is None
        assert response.category == "C"
        assert "Category C" in response.reference

    def test_grc1_all_arcs(self, calculator):
        """GRC 1 × all ARCs (full row coverage)."""
        expected = {
            ARCLevel.ARC_A: SAILLevel.I,
            ARCLevel.ARC_B: SAILLevel.II,
            ARCLevel.ARC_C: SAILLevel.IV,
            ARCLevel.ARC_D: SAILLevel.VI,
        }
        for arc, sail in expected.items():
            request = SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_0, grc_level=1, arc_level=arc
            )
            response = calculator.calculate_sail(request)
            assert response.sail_level == sail, f"GRC 1, ARC {arc.value} failed"

    def test_missing_arc_level_raises_error(self, calculator):
        """SORA 2.0 without arc_level raises ValueError."""
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_0,
                grc_level=4,
                arc_level=None,
            )

    def test_invalid_grc_zero_raises_error(self, calculator):
        """GRC < 1 raises ValueError."""
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_0,
                grc_level=0,
                arc_level=ARCLevel.ARC_A,
            )
