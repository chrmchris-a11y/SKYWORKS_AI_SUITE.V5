"""SORA 2.0 SAIL lookup tables and OSO requirements."""

from ..models.sail_models import SAILValue

# SORA 2.0 SAIL mapping table (EASA AMC/GM Annex D, Table D.1)
SAIL_TABLE_20 = {
    # GRC 1-2: Same SAIL mappings
    (1, 2): {
        "a": SAILValue.I,
        "b": SAILValue.I,
        "c": SAILValue.II,
        "d": SAILValue.III
    },
    # GRC 3-4: Same SAIL mappings
    (3, 4): {
        "a": SAILValue.II,
        "b": SAILValue.II,
        "c": SAILValue.III,
        "d": SAILValue.IV
    },
    # GRC 5-6: Same SAIL mappings
    (5, 6): {
        "a": SAILValue.III,
        "b": SAILValue.III,
        "c": SAILValue.IV,
        "d": SAILValue.V
    },
    # GRC 7-8: Same SAIL mappings
    (7, 8): {
        "a": SAILValue.IV,
        "b": SAILValue.IV,
        "c": SAILValue.V,
        "d": SAILValue.VI
    }
}

# OSO requirements by SAIL (cumulative)
OSO_BY_SAIL_20 = {
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

# Flatten SAIL_TABLE_20 for direct (GRC, ARC) → SAIL lookup
_flat_table = {}
for grc_range, mappings in SAIL_TABLE_20.items():
    if isinstance(grc_range, tuple):
        for grc in grc_range:
            for arc, sail in mappings.items():
                _flat_table[(grc, arc)] = sail.value
    else:
        for arc, sail in mappings.items():
            _flat_table[(grc_range, arc)] = sail.value

# Replace grouped table with flat table
SAIL_TABLE_20 = _flat_table

# OSO count by SAIL level (string keys for compatibility)
OSO_BY_SAIL_20 = {
    "I": 6, "II": 10, "III": 15, "IV": 18, "V": 21, "VI": 24
}

# Verification data for testing
SORA_20_TEST_CASES = [
    # (GRC, ARC, Expected SAIL)
    (1, "a", "I"), (1, "b", "I"), (1, "c", "II"), (1, "d", "III"),
    (2, "a", "I"), (2, "b", "I"), (2, "c", "II"), (2, "d", "III"),
    (3, "a", "II"), (3, "b", "II"), (3, "c", "III"), (3, "d", "IV"),
    (4, "a", "II"), (4, "b", "II"), (4, "c", "III"), (4, "d", "IV"),
    (5, "a", "III"), (5, "b", "III"), (5, "c", "IV"), (5, "d", "V"),
    (6, "a", "III"), (6, "b", "III"), (6, "c", "IV"), (6, "d", "V"),
    (7, "a", "IV"), (7, "b", "IV"), (7, "c", "V"), (7, "d", "VI"),
    (8, "a", "IV"), (8, "b", "IV"), (8, "c", "V"), (8, "d", "VI"),
]

# Expected OSO counts by SAIL
EXPECTED_OSO_COUNTS_20 = {
    "I": 6, "II": 10, "III": 15, "IV": 18, "V": 21, "VI": 24
}

# SAIL matrix for easy reference
SAIL_MATRIX_20 = """
SORA 2.0 SAIL Matrix (EASA AMC/GM Annex D, Table D.1)

GRC ↓ / ARC → │  a  │  b  │  c  │  d  │
──────────────┼─────┼─────┼─────┼─────┤
      1       │  I  │  I  │  II │ III │
      2       │  I  │  I  │  II │ III │
      3       │  II │  II │ III │  IV │
      4       │  II │  II │ III │  IV │
      5       │ III │ III │  IV │  V  │
      6       │ III │ III │  IV │  V  │
      7       │  IV │  IV │  V  │  VI │
      8       │  IV │  IV │  V  │  VI │
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
    if not 1 <= grc <= 8:
        raise ValueError(f"GRC must be 1-8, got {grc}")
    
    arc_lower = arc.lower()
    if arc_lower not in ["a", "b", "c", "d"]:
        raise ValueError(f"ARC must be a/b/c/d, got {arc}")
    
    # Find matching GRC range
    for grc_range, arc_map in SAIL_TABLE_20.items():
        if grc_range[0] <= grc <= grc_range[1]:
            return arc_map[arc_lower].value
    
    raise ValueError(f"No SAIL mapping found for GRC {grc}, ARC {arc}")

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
        return OSO_BY_SAIL_20[sail_enum]
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