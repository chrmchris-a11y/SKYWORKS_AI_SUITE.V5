import pytest
from calculations.sail_calculator import SAILCalculator
from models.sora_models import SAILRequest, SORAVersion, ARCRating

@pytest.mark.parametrize("grc,arc,expected", [
    (5, ARCRating.ARC_c, "IV"),  # 2.0 edge case
    (3, ARCRating.ARC_d, "V"),
    (6, ARCRating.ARC_b, "VI"),
])
def test_sail_20_cases(grc, arc, expected):
    calc = SAILCalculator()
    req = SAILRequest(sora_version=SORAVersion.SORA_2_0, final_grc=grc, final_arc=arc)
    res = calc.calculate_sail(req)
    assert res.sail.value == expected

@pytest.mark.parametrize("grc,arc,expected", [
    (2, ARCRating.ARC_b, "II"),   # stricter than 2.0 for low GRC
    (4, ARCRating.ARC_b, "III"),  # less conservative mid GRC
    (5, ARCRating.ARC_b, "IV"),   # simplified row
])
def test_sail_25_cases(grc, arc, expected):
    calc = SAILCalculator()
    req = SAILRequest(sora_version=SORAVersion.SORA_2_5, final_grc=grc, final_arc=arc)
    res = calc.calculate_sail(req)
    assert res.sail.value == expected
