import pytest
from calculations.grc_calculator import GRCCalculator
from models.sora_models import GRCRequest_2_0, MitigationLevel


@pytest.mark.parametrize("scenario", [
    "VLOS_Controlled",
    "VLOS_Sparsely",
    "BVLOS_Sparsely",
    "VLOS_Populated",
    "BVLOS_Populated",
])
def test_grc20_scenarios_intrinsic_ranges(scenario):
    calc = GRCCalculator()
    req = GRCRequest_2_0(
        max_dimension_m=0.9,
        operational_scenario=scenario,
        m1_strategic=MitigationLevel.LOW,
        m2_ground_impact=MitigationLevel.MEDIUM,
        m3_emergency_response=MitigationLevel.MEDIUM,
    )
    res = calc.calculate_grc_2_0(req)
    # Expect intrinsic within 1..7 for these scenarios at small dimension
    assert 1 <= res.intrinsic_grc <= 7


def test_grc20_large_dimension_populated_can_go_out_of_scope():
    calc = GRCCalculator()
    req = GRCRequest_2_0(
        max_dimension_m=25.0,
        operational_scenario="BVLOS_Populated",
        m1_strategic=MitigationLevel.NONE,
        m2_ground_impact=MitigationLevel.MEDIUM,
        m3_emergency_response=MitigationLevel.MEDIUM,
    )
    res = calc.calculate_grc_2_0(req)
    # Out-of-scope marker behavior: final_grc is clamped to 8 for SORA 2.0 if intrinsic exceeds 7
    assert res.final_grc >= 8
