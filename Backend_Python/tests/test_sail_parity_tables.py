"""Parity tests: centralized SORA tables vs calculators.

These tests guard against drift by asserting that the SAILCalculator's
outputs match the centralized data tables for representative entries.
"""

import itertools

import pytest

from sail.calculators.sail_calculator import SAILCalculator
from sail.models.sail_models import (
    ARCLevel,
    SAILLevel,
    SAILCalculationRequest,
    SORAVersion,
)
from sail.data.sail_tables_20 import SAIL_TABLE_20
from sail.data.sail_tables_25 import SAIL_TABLE_25


def _arc_letter_to_enum(letter: str) -> ARCLevel:
    mapping = {"a": ARCLevel.ARC_A, "b": ARCLevel.ARC_B, "c": ARCLevel.ARC_C, "d": ARCLevel.ARC_D}
    return mapping[letter.lower()]


@pytest.mark.parametrize(
    "grc,arc_letter,expected",
    sorted([(k[0], k[1], v) for k, v in SAIL_TABLE_20.items()], key=lambda t: (t[0], t[1]))
)
def test_parity_sora20_table_vs_calculator(grc, arc_letter, expected):
    calc = SAILCalculator()
    req = SAILCalculationRequest(
        sora_version=SORAVersion.SORA_2_0,
        grc_level=grc,
        arc_level=_arc_letter_to_enum(arc_letter),
    )
    res = calc.calculate_sail(req)
    assert res.category is None, "No Category C inside 1..7 table range"
    assert res.sail_level == SAILLevel(expected)


def test_sora20_category_c_when_grc_over_7():
    calc = SAILCalculator()
    req = SAILCalculationRequest(
        sora_version=SORAVersion.SORA_2_0,
        grc_level=8,
        arc_level=ARCLevel.ARC_A,
    )
    res = calc.calculate_sail(req)
    assert res.category == "C"
    assert res.sail_level is None


@pytest.mark.parametrize(
    "grc,arc_num,expected",
    sorted([(k[0], k[1], v) for k, v in SAIL_TABLE_25.items()], key=lambda t: (t[0], t[1]))
)
def test_parity_sora25_table_vs_calculator(grc, arc_num, expected):
    calc = SAILCalculator()
    req = SAILCalculationRequest(
        sora_version=SORAVersion.SORA_2_5,
        grc_level=grc,
        residual_arc_level=arc_num,
    )
    res = calc.calculate_sail(req)
    assert res.sail_level == SAILLevel(expected)


@pytest.mark.parametrize("grc,arc_num", list(itertools.product([9, 10], [1, 5, 10])))
def test_sora25_high_grc_rows_are_vi(grc, arc_num):
    calc = SAILCalculator()
    req = SAILCalculationRequest(
        sora_version=SORAVersion.SORA_2_5,
        grc_level=grc,
        residual_arc_level=arc_num,
    )
    res = calc.calculate_sail(req)
    assert res.sail_level == SAILLevel.VI
