"""Unit tests for SAILCalculator (item 5) covering both versions.

We pick values that are stable and compliant with authoritative rules.
"""

from sail.calculators.sail_calculator import SAILCalculator
from sail.models.sail_models import (
    SAILCalculationRequest,
    SAILLevel,
    SORAVersion,
    ARCLevel,
)


def test_calculate_sail_v20_basic():
    calc = SAILCalculator()
    req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=5, arc_level=ARCLevel.ARC_C)
    res = calc.calculate_sail(req)
    assert res.sail_level == SAILLevel.IV
    assert res.sail == SAILLevel.IV
    assert res.grc_level == 5
    assert res.arc_level == ARCLevel.ARC_C


def test_calculate_sail_v20_category_c():
    calc = SAILCalculator()
    req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_0, grc_level=8, arc_level=ARCLevel.ARC_B)
    res = calc.calculate_sail(req)
    assert res.category == "C"
    assert res.sail_level is None


def test_calculate_sail_v25_high_rows_vi():
    calc = SAILCalculator()
    req = SAILCalculationRequest(sora_version=SORAVersion.SORA_2_5, grc_level=9, residual_arc_level=1)
    res = calc.calculate_sail(req)
    assert res.sail_level == SAILLevel.VI
    assert res.residual_arc_level == 1
