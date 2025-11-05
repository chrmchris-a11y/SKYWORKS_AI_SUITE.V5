"""Unit tests for SORA 2.5 SAIL calculator (JARUS Annex D Table 7)."""

import pytest
from sail.calculators.sail_calculator import SAILCalculator
from sail.models.sail_models import (
    SAILCalculationRequest,
    SAILLevel,
    SORAVersion,
)


@pytest.fixture
def calculator():
    """Fixture for SAILCalculator instance."""
    return SAILCalculator()


class TestSAIL25Acceptance:
    """Acceptance tests for SORA 2.5 (JARUS SORA 2.5 Annex D Table 7)."""

    def test_grc9_residual_arc_1_returns_sail_vi(self, calculator):
        """GRC 9, residual ARC 1 → SAIL VI."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_5,
            grc_level=9,
            residual_arc_level=1,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.VI
        assert response.sail == SAILLevel.VI
        assert response.grc_level == 9
        assert response.residual_arc_level == 1
        assert "JARUS SORA 2.5 Annex D" in response.reference

    def test_grc9_residual_arc_10_returns_sail_vi(self, calculator):
        """GRC 9, residual ARC 10 → SAIL VI."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_5,
            grc_level=9,
            residual_arc_level=10,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.VI

    def test_grc10_residual_arc_1_returns_sail_vi(self, calculator):
        """GRC 10, residual ARC 1 → SAIL VI."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_5,
            grc_level=10,
            residual_arc_level=1,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.VI

    def test_grc10_residual_arc_10_returns_sail_vi(self, calculator):
        """GRC 10, residual ARC 10 → SAIL VI."""
        request = SAILCalculationRequest(
            sora_version=SORAVersion.SORA_2_5,
            grc_level=10,
            residual_arc_level=10,
        )
        response = calculator.calculate_sail(request)
        assert response.sail_level == SAILLevel.VI

    def test_grc4_residual_arc_samples(self, calculator):
        """GRC 4 × sample residual ARCs (spot checks from Table 7)."""
        # Expected from centralized JARUS Table 7 (GRC 4 row)
        expected = {
            1: SAILLevel.II,
            4: SAILLevel.V,
            7: SAILLevel.VI,
            10: SAILLevel.VI,
        }
        for arc, sail in expected.items():
            request = SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=4,
                residual_arc_level=arc,
            )
            response = calculator.calculate_sail(request)
            assert response.sail_level == sail, f"GRC 4, ARC {arc} failed"

    def test_grc1_full_row(self, calculator):
        """GRC 1 × all residual ARCs 1..10 (full row coverage)."""
        expected = [
            SAILLevel.I, SAILLevel.I, SAILLevel.II, SAILLevel.II, SAILLevel.III,
            SAILLevel.III, SAILLevel.IV, SAILLevel.IV, SAILLevel.V, SAILLevel.V,
        ]
        for arc in range(1, 11):
            request = SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=1,
                residual_arc_level=arc,
            )
            response = calculator.calculate_sail(request)
            assert response.sail_level == expected[arc - 1], f"GRC 1, ARC {arc} failed"

    def test_missing_residual_arc_raises_error(self, calculator):
        """SORA 2.5 without residual_arc_level raises ValueError."""
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=5,
                residual_arc_level=None,
            )

    def test_residual_arc_out_of_range_raises_error(self, calculator):
        """residual_arc_level < 1 or > 10 raises ValueError."""
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=5,
                residual_arc_level=0,
            )
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=5,
                residual_arc_level=11,
            )

    def test_grc_out_of_range_raises_error(self, calculator):
        """GRC < 1 or > 10 raises ValueError for SORA 2.5."""
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=0,
                residual_arc_level=5,
            )
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=11,
                residual_arc_level=5,
            )
