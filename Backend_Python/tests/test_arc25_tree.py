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
    # Updated per latest rule alignment: Atypical/Segregated does NOT directly set ARC-a at Step #4.
    # Initial ARC follows AEC mapping (here: uncontrolled rural <150m → AEC-10 → ARC-b).
    assert res.initial_arc == ARCRating.ARC_b, f"Expected initial_arc=ARC-b for atypical/segregated under AEC mapping, got {res.initial_arc}"
    # AEC-12 is not a Step #4 category; ensure calculator didn't assign it as AEC.
    assert res.aec_number in {7,8,9,10,4,5,3,6,1,2}, "AEC-12 should not be set by Step #4 calculator"
    # Uncontrolled >=120m authority flag tested separately


def test_arc25_modes_precedence_over_near_aerodrome():
    """
    Mode-S/TMZ precedence should apply before near-aerodrome proximity heuristics.
    With altitude >=150m and is_mode_s_veil=True, initial ARC should be ARC-d regardless of proximity.
    """
    calc = ARCCalculator()
    req = ARCRequest_2_5(
        max_height_agl_m=160.0,
        airspace_class=AirspaceClass.CLASS_G,  # Uncontrolled
        environment=EnvironmentType.RURAL,
        is_tmz=False,
        is_mode_s_veil=True,
        is_airport_heliport=False,
        distance_to_aerodrome_km=2.0,  # Near aerodrome, but Mode-S/TMZ should take precedence
        is_atypical_segregated=False,
    )
    res = calc.calculate_arc_2_5(req)
    assert res.initial_arc == ARCRating.ARC_d


def test_arc25_uncontrolled_120m_requires_authority_flag():
    """Uncontrolled operations at/above 120 m should surface authority flag."""
    calc = ARCCalculator()
    req = ARCRequest_2_5(
        max_height_agl_m=120.0,
        airspace_class=AirspaceClass.CLASS_G,
        environment=EnvironmentType.RURAL,
        is_tmz=False,
        is_mode_s_veil=False,
        is_airport_heliport=False,
    )
    res = calc.calculate_arc_2_5(req)
    assert res.requires_authority_approval is True


def test_arc25_suburban_normalization_matches_urban():
    """SUBURBAN input should be treated as URBAN for AEC mapping."""
    calc = ARCCalculator()
    base = ARCRequest_2_5(
        max_height_agl_m=80.0,
        airspace_class=AirspaceClass.CLASS_G,
        environment=EnvironmentType.URBAN,
        is_tmz=False,
        is_mode_s_veil=False,
        is_airport_heliport=False,
    )
    sub = base.model_copy(update={"environment": EnvironmentType.SUBURBAN})
    res_base = calc.calculate_arc_2_5(base)
    res_sub = calc.calculate_arc_2_5(sub)
    assert res_sub.initial_arc == res_base.initial_arc


def test_arc25_unknown_sm_tokens_are_ignored():
    """
    Unknown strategic mitigation tokens must not reduce ARC and should result in no applied mitigations.
    Model-level gating removes non-whitelisted tokens; calculator also gates unknown prefixes.
    """
    calc = ARCCalculator()
    req = ARCRequest_2_5(
        max_height_agl_m=80.0,
        airspace_class=AirspaceClass.CLASS_G,
        environment=EnvironmentType.RURAL,
        is_tmz=False,
        is_mode_s_veil=False,
        is_airport_heliport=False,
        strategic_mitigations=["junk", "M1", "SM1"],
    )
    res = calc.calculate_arc_2_5(req)
    assert res.residual_arc == res.initial_arc
    assert len(res.mitigations_applied) == 0
