"""SORA 2.5 SAIL lookup table (numeric residual ARC 1..10).

Centralized source used by calculators to avoid duplication.
"""

from typing import Dict, Tuple

# Adopt the mapping used in sail/sail_calculator_v25.py to keep parity.
SAIL_TABLE_25: Dict[Tuple[int, int], str] = {
    # GRC = 1
    (1, 1): "I",   (1, 2): "I",   (1, 3): "II",  (1, 4): "II",
    (1, 5): "III", (1, 6): "III", (1, 7): "IV",  (1, 8): "IV",
    (1, 9): "V",   (1, 10): "V",

    # GRC = 2
    (2, 1): "I",   (2, 2): "II",  (2, 3): "II",  (2, 4): "II",
    (2, 5): "III", (2, 6): "IV",  (2, 7): "IV",  (2, 8): "V",
    (2, 9): "V",   (2, 10): "VI",

    # GRC = 3
    (3, 1): "II",  (3, 2): "II",  (3, 3): "III", (3, 4): "IV",
    (3, 5): "IV",  (3, 6): "V",   (3, 7): "V",   (3, 8): "VI",
    (3, 9): "VI",  (3, 10): "VI",

    # GRC = 4
    (4, 1): "II",  (4, 2): "III", (4, 3): "III", (4, 4): "V",
    (4, 5): "V",   (4, 6): "VI",  (4, 7): "VI",  (4, 8): "VI",
    (4, 9): "VI",  (4, 10): "VI",

    # GRC = 5
    (5, 1): "III", (5, 2): "IV",  (5, 3): "V",   (5, 4): "VI",
    (5, 5): "VI",  (5, 6): "VI",  (5, 7): "VI",  (5, 8): "VI",
    (5, 9): "VI",  (5, 10): "VI",

    # GRC = 6
    (6, 1): "IV",  (6, 2): "V",   (6, 3): "VI",  (6, 4): "V",
    (6, 5): "VI",  (6, 6): "VI",  (6, 7): "VI",  (6, 8): "VI",
    (6, 9): "VI",  (6, 10): "VI",

    # GRC = 7
    (7, 1): "V",   (7, 2): "VI",  (7, 3): "VI",  (7, 4): "VI",
    (7, 5): "VI",  (7, 6): "VI",  (7, 7): "VI",  (7, 8): "VI",
    (7, 9): "VI",  (7, 10): "VI",

    # GRC = 8
    (8, 1): "VI",  (8, 2): "VI",  (8, 3): "VI",  (8, 4): "VI",
    (8, 5): "VI",  (8, 6): "VI",  (8, 7): "VI",  (8, 8): "VI",
    (8, 9): "VI",  (8, 10): "VI",
}


def get_sail_25(grc: int, residual_arc: int) -> str:
    """Return SAIL for numeric (grc, residual_arc). For grc≥9, always "VI"."""
    if not (1 <= grc <= 10):
        raise ValueError(f"GRC must be 1..10, got {grc}")
    if not (1 <= residual_arc <= 10):
        raise ValueError(f"residual_arc must be 1..10, got {residual_arc}")
    if grc >= 9:
        return "VI"
    sail = SAIL_TABLE_25.get((grc, residual_arc))
    if sail is None:
        raise ValueError(f"No SAIL mapping found for (GRC={grc}, ARC={residual_arc})")
    return sail