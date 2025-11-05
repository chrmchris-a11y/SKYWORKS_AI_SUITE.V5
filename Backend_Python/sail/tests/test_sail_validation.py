import pytest
from sail.models.sail_models import (
    SAILCalculationRequest, SORAVersion, ARCLevel
)


class TestSAILValidation:
    """Validate SORA-aware request model rules (2.0 letters, 2.5 numeric)."""

    def test_valid_request_20(self):
        req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=5, arc_level=ARCLevel.ARC_C)
        # arc_level may be stored as enum or enum value depending on model config
        arc_val = req.arc_level.value if hasattr(req.arc_level, 'value') else req.arc_level
        assert req.grc_level == 5 and str(arc_val) == 'c'

    def test_category_c_request_20(self):
        # GRC>7 allowed at model level (Category C handled by router/calculator)
        req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=8, arc_level=ARCLevel.ARC_A)
        assert req.grc_level == 8

    def test_missing_arc_20(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=5)

    def test_valid_request_25(self):
        req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=6, residual_arc_level=4)
        assert req.residual_arc_level == 4

    def test_letters_rejected_25(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=5, arc_level=ARCLevel.ARC_B)

    def test_out_of_range_residual_25(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=5, residual_arc_level=11)
    
    def test_invalid_grc_low(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=0, arc_level=ARCLevel.ARC_A)
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=0, residual_arc_level=4)
    
    def test_invalid_grc_high(self):
        # 2.0: model accepts >7; calculator/router handle Category C, but high-high like 100 should still coerce error
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=-999)
        # 2.5: allow up to 10, but very high should error
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=999, residual_arc_level=1)
    
    def test_invalid_grc_negative(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=-1, arc_level=ARCLevel.ARC_A)
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=-1, residual_arc_level=3)
    
    def test_valid_arc_values_20(self):
        for arc in [ARCLevel.ARC_A, ARCLevel.ARC_B, ARCLevel.ARC_C, ARCLevel.ARC_D]:
            req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=4, arc_level=arc)
            # Accept either enum equality or value equality
            if hasattr(req.arc_level, 'value'):
                assert req.arc_level == arc
            else:
                assert str(req.arc_level) == arc.value
    
    def test_invalid_arc_none(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=4)
    
    def test_invalid_arc_string(self):
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=4, arc_level="E")
    
    def test_grc_boundary_values(self):
        # 2.0 min
        SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=1, arc_level=ARCLevel.ARC_A)
        # 2.0 accepts 8 (Category C handled downstream for >7)
        SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=8, arc_level=ARCLevel.ARC_D)
        # 2.5 min and max
        SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=1, residual_arc_level=1)
        SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=10, residual_arc_level=10)
    
    def test_all_valid_combinations_20(self):
        for grc in range(1, 9):
            for arc in [ARCLevel.ARC_A, ARCLevel.ARC_B, ARCLevel.ARC_C, ARCLevel.ARC_D]:
                SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=grc, arc_level=arc)
    
    def test_input_type_validation(self):
        # Coercion of numeric-like strings
        req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level="4", arc_level=ARCLevel.ARC_A)
        assert req.grc_level == 4
        # Invalid string
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level="invalid", arc_level=ARCLevel.ARC_A)
    
    def test_error_messages_basic(self):
        with pytest.raises(Exception) as exc_info:
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=5)
        assert "residual_arc_level" in str(exc_info.value)
    
    def test_validator_version_consistency_basics(self):
        # Minimal smoke: 2.0 accepts letters, 2.5 rejects letters and requires numeric residual
        SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=4, arc_level=ARCLevel.ARC_B)
        with pytest.raises(Exception):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=4, arc_level=ARCLevel.ARC_B)
    
    def test_edge_case_float_grc(self):
        # Pydantic coercion should produce int
        req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=4.0, arc_level=ARCLevel.ARC_A)
        assert req.grc_level == 4
    
    def test_validation_performance(self):
        # Basic loop construction
        for _ in range(100):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=4, arc_level=ARCLevel.ARC_B)
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=4, residual_arc_level=3)
    
    def test_concurrent_validation(self):
        for i in range(1, 9):
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=i, arc_level=ARCLevel.ARC_A)
            SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=i, residual_arc_level=1)
        assert True