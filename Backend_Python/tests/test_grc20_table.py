import pytest
from calculations.grc_calculator import GRCCalculator
from models.sora_models import GRCRequest_2_0, MitigationLevel

# Mapping of scenarios per SORA 2.0 nomenclature used in calculator
SCENARIOS = [
    "VLOS_Controlled",
    "VLOS_Sparsely",
    "BVLOS_Sparsely",
    "VLOS_Populated",
    "BVLOS_Populated",
    "VLOS_Gathering",
    "BVLOS_Gathering",
]

# Expected intrinsic GRC table from SORA 2.0 Table 2
EXPECTED = {
    "1m":            [1, 2, 3, 4, 5, 7, 8],
    "3m":            [2, 3, 4, 5, 6, None, None],
    "8m":            [3, 4, 5, 6, 8, None, None],
    ">8m":           [4, 5, 6, 8, 10, None, None],
}

@pytest.mark.parametrize("dim, value", [("1m",0.5),("3m",2.0),("8m",5.0),(">8m",9.0)])
@pytest.mark.parametrize("idx,scenario", list(enumerate(SCENARIOS)))
def test_sora20_igrc_table(dim, value, idx, scenario):
    calc = GRCCalculator()
    req = GRCRequest_2_0(
        max_dimension_m=value,
        operational_scenario=scenario,
        m1_strategic=MitigationLevel.NONE,
        m2_ground_impact=MitigationLevel.MEDIUM,
        m3_emergency_response=MitigationLevel.MEDIUM,
    )

    expected = EXPECTED[dim][idx]

    if expected is None:
        with pytest.raises(ValueError):
            calc.calculate_grc_2_0(req)
    else:
        res = calc.calculate_grc_2_0(req)
        assert res.intrinsic_grc == expected
        # If intrinsic > 7, operation is out of scope → final >= 8 marker
        if expected > 7:
            assert res.final_grc >= 8
        else:
            assert 1 <= res.final_grc <= 7
