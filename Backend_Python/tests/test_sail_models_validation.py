"""Validation tests for Pydantic models in sail_models.py (item 4)."""

import pytest
from sail.models.sail_models import SAILCalculationRequest, SORAVersion


class TestSailModelsValidation:
    def test_v20_requires_arc_level(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=5)

    def test_v25_requires_numeric_arc_level(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=6,
                residual_arc_level=None,
            )

    def test_v25_disallows_letter_arc(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(
                sora_version=SORAVersion.SORA_2_5,
                grc_level=6,
                residual_arc_level=4,
                arc_level="c",  # type: ignore[arg-type]
            )

    def test_v25_out_of_range_numeric_arc(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=6, residual_arc_level=0)
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=6, residual_arc_level=11)
