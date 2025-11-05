"""Unit tests for the standalone SORA 2.5 SAIL function (sail_calculator_v25.py).

We deliberately choose cells that are stable across sources:
- (1,1) → I (low risk)
- (9,7) → VI (any ARC at high GRC)
- (10,1) → VI (any ARC at high GRC)
"""

from sail.sail_calculator_v25 import calculate_sail_v25


def test_v25_low_cell_1_1():
    sail, expl = calculate_sail_v25(1, 1)
    assert sail == "I"
    assert "Final GRC: 1" in expl


def test_v25_high_rows_any_arc_vi():
    s1, _ = calculate_sail_v25(9, 7)
    s2, _ = calculate_sail_v25(10, 1)
    assert s1 == "VI"
    assert s2 == "VI"
