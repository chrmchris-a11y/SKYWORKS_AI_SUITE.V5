import pytest
from calculations.arc_calculator import ARCCalculator
from models.sora_models import ARCRequest_2_0, AirspaceClass, EnvironmentType, MitigationLevel, ARCRating

@pytest.mark.parametrize(
    "amsl,expected",
    [
        (20000.0, ARCRating.ARC_b),  # Above FL600 -> ARC-b
        (100.0, None),
    ],
)
def test_arc20_fl600_and_normal(amsl, expected):
    calc = ARCCalculator()
    req = ARCRequest_2_0(
        max_height_agl_m=200.0,
        max_height_amsl_m=amsl,
        airspace_class=AirspaceClass.CLASS_G,
        is_controlled=False,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.RURAL,
        is_airport_heliport=False,
        is_atypical_segregated=False,
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res = calc.calculate_arc_2_0(req)
    if expected:
        assert res.initial_arc == expected


def test_arc20_atypical_segregated_goes_arc_a():
    calc = ARCCalculator()
    req = ARCRequest_2_0(
        max_height_agl_m=50.0,
        max_height_amsl_m=500.0,
        airspace_class=AirspaceClass.CLASS_G,
        is_controlled=False,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.RURAL,
        is_airport_heliport=False,
        is_atypical_segregated=True,
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res = calc.calculate_arc_2_0(req)
    assert res.initial_arc == ARCRating.ARC_a


def test_arc20_airport_class_c_arc_d():
    calc = ARCCalculator()
    req = ARCRequest_2_0(
        max_height_agl_m=100.0,
        max_height_amsl_m=500.0,
        airspace_class=AirspaceClass.CLASS_C,
        is_controlled=True,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.URBAN,
        is_airport_heliport=True,
        is_atypical_segregated=False,
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res = calc.calculate_arc_2_0(req)
    assert res.initial_arc == ARCRating.ARC_d


def test_arc20_above_500ft_uncontrolled_rural_arc_c():
    """
    Per corrections pack Section 6 & Table C.1:
    - Threshold: 150m (~500ft)
    - Uncontrolled Rural >150m → AEC 5 → ARC-c (Density 2)
    
    Test with 160m (above threshold)
    Note: Using alias 'max_height_agl_m' which accepts meters and converts internally
    """
    calc = ARCCalculator()
    req = ARCRequest_2_0(
        max_height_agl_m=160.0,  # 160m (above 150m threshold) - uses alias field
        airspace_class=AirspaceClass.CLASS_G,
        is_controlled=False,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.RURAL,
        is_airport_heliport=False,
        is_atypical_segregated=False,
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res = calc.calculate_arc_2_0(req)
    assert res.initial_arc == ARCRating.ARC_c
    assert res.aec_number == 5  # AEC 5: Uncontrolled Rural >150m
