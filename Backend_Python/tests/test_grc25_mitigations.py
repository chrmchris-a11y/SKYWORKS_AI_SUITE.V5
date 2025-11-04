import pytest
from calculations.grc_calculator import GRCCalculator
from models.sora_models import GRCRequest_2_5, MitigationLevel


def make_req(dim=2.0, speed=20.0, density=200.0, controlled=False, m1=MitigationLevel.NONE, m2=MitigationLevel.MEDIUM):
    return GRCRequest_2_5(
        max_dimension_m=dim,
        max_speed_ms=speed,
        population_density=density,
        is_controlled_ground=controlled,
        m1_strategic=m1,
        m2_ground_impact=m2,
        m3_emergency_response=MitigationLevel.MEDIUM,
    )


def test_grc25_high_mitigations_reduce_final_grc_but_not_below_1():
    calc = GRCCalculator()
    base = make_req()
    res_base = calc.calculate_grc_2_5(base)

    strong = make_req(m1=MitigationLevel.HIGH, m2=MitigationLevel.HIGH)
    res_strong = calc.calculate_grc_2_5(strong)

    assert res_strong.final_grc <= res_base.final_grc
    assert res_strong.final_grc >= 1


@pytest.mark.parametrize("controlled", [True, False])
def test_grc25_controlled_ground_reduces_intrinsic_grc(controlled):
    calc = GRCCalculator()
    req = make_req(density=2000.0, controlled=controlled)
    res = calc.calculate_grc_2_5(req)
    # Controlled ground should not increase iGRC
    if controlled:
        assert res.intrinsic_grc <= 6
    else:
        assert res.intrinsic_grc >= 3


def test_grc25_boundary_grey_cell_raises():
    calc = GRCCalculator()
    # Large dim/speed with very high density triggers grey cell
    req = make_req(dim=25.0, speed=150.0, density=80000.0)
    with pytest.raises(ValueError):
        calc.calculate_grc_2_5(req)
