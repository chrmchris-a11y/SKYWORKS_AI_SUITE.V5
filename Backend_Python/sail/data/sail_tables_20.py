"""SORA 2.0 SAIL lookup tables and OSO requirements.

Source of truth for SAIL mapping used across the codebase to avoid drift.
This table matches the current production calculator mapping.
"""

from ..models.sail_models import SAILValue

# SORA 2.0 SAIL mapping table (EASA AMC/GM Annex D Table D.1 style).
# Final GRC > 7 ⇒ Category C (outside this table). Values below reflect the
# current calculator's deterministic mapping.

SAIL_TABLE_20 = {
    # GRC 1
    (1, "a"): "I", (1, "b"): "II", (1, "c"): "IV", (1, "d"): "VI",
    # GRC 2
    (2, "a"): "I", (2, "b"): "II", (2, "c"): "IV", (2, "d"): "VI",
    # GRC 3
    (3, "a"): "I", (3, "b"): "IV", (3, "c"): "IV", (3, "d"): "VI",
    # GRC 4
    (4, "a"): "II", (4, "b"): "IV", (4, "c"): "IV", (4, "d"): "VI",
    # GRC 5
    (5, "a"): "IV", (5, "b"): "IV", (5, "c"): "IV", (5, "d"): "VI",
    # GRC 6
    (6, "a"): "IV", (6, "b"): "IV", (6, "c"): "V",  (6, "d"): "VI",
    # GRC 7
    (7, "a"): "VI", (7, "b"): "VI", (7, "c"): "VI", (7, "d"): "VI",
}

# OSO requirements by SAIL (cumulative)
OSO_REQUIREMENTS_BY_SAIL_20 = {
    SAILValue.I: [
        "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06"
    ],
    SAILValue.II: [
        "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06",
        "OSO#07", "OSO#08", "OSO#09", "OSO#10"
    ],
    SAILValue.III: [
        "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06",
        "OSO#07", "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12",
        "OSO#13", "OSO#14", "OSO#15"
    ],
    SAILValue.IV: [
        "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06",
        "OSO#07", "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12",
        "OSO#13", "OSO#14", "OSO#15", "OSO#16", "OSO#17", "OSO#18"
    ],
    SAILValue.V: [
        "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06",
        "OSO#07", "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12",
        "OSO#13", "OSO#14", "OSO#15", "OSO#16", "OSO#17", "OSO#18",
        "OSO#19", "OSO#20", "OSO#21"
    ],
    SAILValue.VI: [
        "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06",
        "OSO#07", "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12",
        "OSO#13", "OSO#14", "OSO#15", "OSO#16", "OSO#17", "OSO#18",
        "OSO#19", "OSO#20", "OSO#21", "OSO#22", "OSO#23", "OSO#24"
    ]
}

# OSO count by SAIL level (string keys for compatibility)
OSO_COUNT_BY_SAIL_20 = {"I": 6, "II": 10, "III": 15, "IV": 18, "V": 21, "VI": 24}

# Verification data for testing (authoritative cases)
SORA_20_TEST_CASES = [
    # (GRC, ARC, Expected SAIL)
    (1, "a", "I"), (1, "b", "II"), (1, "c", "IV"), (1, "d", "VI"),
    (2, "a", "I"), (2, "b", "II"), (2, "c", "IV"), (2, "d", "VI"),
    (3, "a", "II"), (3, "b", "II"), (3, "c", "IV"), (3, "d", "VI"),
    (4, "a", "III"), (4, "b", "III"), (4, "c", "IV"), (4, "d", "VI"),
    (5, "a", "IV"), (5, "b", "IV"), (5, "c", "IV"), (5, "d", "VI"),
    (6, "a", "V"),  (6, "b", "V"),  (6, "c", "V"),  (6, "d", "VI"),
    (7, "a", "VI"), (7, "b", "VI"), (7, "c", "VI"), (7, "d", "VI"),
]

# Expected OSO counts by SAIL
EXPECTED_OSO_COUNTS_20 = OSO_COUNT_BY_SAIL_20.copy()

# SAIL matrix for easy reference
SAIL_MATRIX_20 = """
SORA 2.0 SAIL Matrix (Authoritative – UK SORA AMC Table 6)

GRC ↓ / ARC → │  a  │  b  │  c  │  d  │
──────────────┼─────┼─────┼─────┼─────┤
    1       │  I  │  II │  IV │  VI │
    2       │  I  │  II │  IV │  VI │
    3       │ II  │ II  │  IV │  VI │
    4       │ III │ III │  IV │  VI │
    5       │ IV  │ IV  │  IV │  VI │
    6       │  V  │  V  │  V  │  VI │
    7       │ VI  │ VI  │  VI │  VI │
"""

def get_sail_20(grc: int, arc: str) -> str:
    """
    Get SAIL value for SORA 2.0.
    
    Args:
        grc: Ground Risk Class (1-8)
        arc: Air Risk Class (a-d)
        
    Returns:
        SAIL value (I-VI)
        
    Raises:
        ValueError: If GRC or ARC is invalid
    """
    if not 1 <= grc <= 7:
        raise ValueError(f"GRC must be 1-7 for SORA 2.0 table, got {grc}")
    
    arc_lower = arc.lower()
    if arc_lower not in ["a", "b", "c", "d"]:
        raise ValueError(f"ARC must be a/b/c/d, got {arc}")
    
    sail = SAIL_TABLE_20.get((grc, arc_lower))
    if not sail:
        raise ValueError(f"No SAIL mapping found for GRC {grc}, ARC {arc}")
    return sail

def get_oso_requirements_20(sail: str) -> list:
    """
    Get OSO requirements for SORA 2.0 SAIL.
    
    Args:
        sail: SAIL value (I-VI)
        
    Returns:
        List of required OSO identifiers
        
    Raises:
        ValueError: If SAIL is invalid
    """
    try:
        sail_enum = SAILValue(sail)
        return OSO_REQUIREMENTS_BY_SAIL_20[sail_enum]
    except ValueError:
        raise ValueError(f"Invalid SAIL value: {sail}")

def validate_sail_mapping_20(grc: int, arc: str, expected_sail: str) -> bool:
    """
    Validate SORA 2.0 SAIL mapping.
    
    Args:
        grc: Ground Risk Class
        arc: Air Risk Class
        expected_sail: Expected SAIL value
        
    Returns:
        True if mapping is correct, False otherwise
    """
    try:
        actual_sail = get_sail_20(grc, arc)
        return actual_sail == expected_sail
    except ValueError:
        return False