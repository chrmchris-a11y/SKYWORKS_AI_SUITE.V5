import pytest
from calculations.sail_calculator import SAILCalculator
from models.sora_models import SAILRequest, SORAVersion, ARCRating


@pytest.mark.parametrize("grc,arc,expected", [
    (1, ARCRating.ARC_b, "I"),
    (3, ARCRating.ARC_c, "III"),
    (4, ARCRating.ARC_d, "IV"),
    (5, ARCRating.ARC_d, "V"),
    (6, ARCRating.ARC_b, "IV"),
])
def test_sail_25_additional_edges(grc, arc, expected):
    calc = SAILCalculator()
    req = SAILRequest(sora_version=SORAVersion.SORA_2_5, final_grc=grc, final_arc=arc)
    res = calc.calculate_sail(req)
    assert res.sail.value == expected
