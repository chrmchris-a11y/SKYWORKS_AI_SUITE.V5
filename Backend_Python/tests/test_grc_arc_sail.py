import pytest
from calculations.grc_calculator import GRCCalculator
from calculations.arc_calculator import ARCCalculator
from calculations.sail_calculator import SAILCalculator
from models.sora_models import (
    GRCRequest_2_5,
    ARCRequest_2_0,
    SAILRequest,
    SORAVersion,
    AirspaceClass,
    EnvironmentType,
    ARCRating,
)


def test_grc_25_special_case_small_uas():
    calc = GRCCalculator()
    req = GRCRequest_2_5(
        max_dimension_m=0.2,
        max_speed_ms=20.0,
        weight_kg=0.2,
        population_density=10.0,
        is_controlled_ground=False,
    )
    res = calc.calculate_grc_2_5(req)
    assert res.intrinsic_grc == 1
    assert 1 <= res.final_grc <= 7


def test_arc_20_airport_controlled_over_500ft():
    calc = ARCCalculator()
    req = ARCRequest_2_0(
        max_height_agl_m=200.0,
        max_height_amsl_m=600.0,
        airspace_class=AirspaceClass.CLASS_C,
        is_controlled=True,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.URBAN,
        is_airport_heliport=True,
        is_atypical_segregated=False,
        tactical_mitigation_level="None",
    )
    res = calc.calculate_arc_2_0(req)
    assert res.initial_arc.value.upper() == "ARC-D"
    assert res.final_arc.value.upper() in {"ARC-D", "ARC-C", "ARC-B"}


def test_sail_mapping_direct():
    calc = SAILCalculator()
    req = SAILRequest(
        sora_version=SORAVersion.SORA_2_0,
        final_grc=5,
        final_arc=ARCRating.ARC_c,
    )
    res = calc.calculate_sail(req)
    # Per SORA 2.0 mapping, GRC=5 & ARC_c -> SAIL IV
    assert res.sail.value.upper() == "IV"
