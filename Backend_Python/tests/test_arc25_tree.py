import pytest
from calculations.arc_calculator import ARCCalculator
from models.sora_models import ARCRequest_2_5, AirspaceClass, EnvironmentType, MitigationLevel, ARCRating


@pytest.mark.parametrize(
    "controlled,clazz,env,airport,expected",
    [
        (False, AirspaceClass.CLASS_G, EnvironmentType.RURAL, False, ARCRating.ARC_b),
        (True, AirspaceClass.CLASS_C, EnvironmentType.URBAN, True, ARCRating.ARC_d),
        (True, AirspaceClass.CLASS_D, EnvironmentType.SUBURBAN, False, ARCRating.ARC_c),
    ],
)
def test_arc25_initial_arc_basic(controlled, clazz, env, airport, expected):
    calc = ARCCalculator()
    req = ARCRequest_2_5(
        max_height_agl_m=120.0,
        max_speed_ms=25.0,
        airspace_class=clazz,
        is_controlled=controlled,
        is_modes_veil=False,
        is_tmz=False,
        environment=env,
        is_airport_heliport=airport,
        is_atypical_segregated=False,
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res = calc.calculate_arc_2_5(req)
    assert res.initial_arc == expected


def test_arc25_modes_veil_or_tmz_raise_arc():
    calc = ARCCalculator()
    # Base uncontrolled rural case
    base = ARCRequest_2_5(
        max_height_agl_m=120.0,
        max_speed_ms=15.0,
        airspace_class=AirspaceClass.CLASS_G,
        is_controlled=False,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.RURAL,
        is_airport_heliport=False,
        is_atypical_segregated=False,
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res_base = calc.calculate_arc_2_5(base)

    modes = base.model_copy(update={"is_modes_veil": True})
    res_modes = calc.calculate_arc_2_5(modes)

    tmz = base.model_copy(update={"is_tmz": True})
    res_tmz = calc.calculate_arc_2_5(tmz)

    # Either condition should not reduce ARC below the base case
    assert res_modes.initial_arc >= res_base.initial_arc
    assert res_tmz.initial_arc >= res_base.initial_arc


def test_arc25_atypical_segregated_can_reach_arc_a():
    """
    Per corrections pack Section 3: Atypical/Segregated → AEC-12 → Initial ARC-a globally.
    
    This is NOT based on normal AEC mapping - atypical/segregated operations go DIRECTLY
    to AEC-12 → ARC-a as initial ARC (HIGHEST PRIORITY, before all other checks).
    
    Requires authority approval per corrections pack.
    """
    calc = ARCCalculator()
    req = ARCRequest_2_5(
        max_height_agl_m=60.0,  # <150m
        max_speed_ms=10.0,
        airspace_class=AirspaceClass.CLASS_G,  # Uncontrolled
        is_controlled=False,
        is_modes_veil=False,
        is_tmz=False,
        environment=EnvironmentType.RURAL,
        is_airport_heliport=False,
        is_atypical_segregated=True,  # ← Triggers AEC-12 → ARC-a
        tactical_mitigation_level=MitigationLevel.NONE,
    )
    res = calc.calculate_arc_2_5(req)
    # Per corrections pack Section 3: Atypical → AEC-12 → Initial ARC-a
    assert res.initial_arc == ARCRating.ARC_a, f"Expected initial_arc=ARC-a for atypical/segregated (AEC-12), got {res.initial_arc}"
    assert res.aec_number == 12, f"Expected AEC-12 for atypical/segregated, got {res.aec_number}"
    assert res.requires_authority_approval == True, "Atypical/segregated requires authority approval"
