"""SORA 2.5 SAIL lookup tables and OSO requirements."""

from ..models.sail_models import SAILValue

# SORA 2.5 SAIL mapping table (JARUS SORA v2.5 Annex D)
SAIL_TABLE_25 = {
    # GRC 1-2: Same as SORA 2.0
    (1, 2): {
        "a": SAILValue.I,
        "b": SAILValue.I,
        "c": SAILValue.II,
        "d": SAILValue.III
    },
    # GRC 3-4: Same as SORA 2.0
    (3, 4): {
        "a": SAILValue.II,
        "b": SAILValue.II,
        "c": SAILValue.III,
        "d": SAILValue.IV
    },
    # GRC 5: Same as SORA 2.0
    5: {
        "a": SAILValue.III,
        "b": SAILValue.III,
        "c": SAILValue.IV,
        "d": SAILValue.V
    },
    # GRC 6: Different from SORA 2.0 (increased by 1 SAIL level)
    6: {
        "a": SAILValue.IV,   # Was III in SORA 2.0
        "b": SAILValue.IV,   # Was III in SORA 2.0
        "c": SAILValue.V,    # Was IV in SORA 2.0
        "d": SAILValue.VI    # Was V in SORA 2.0
    },
    # GRC 7-8: Different from SORA 2.0 (increased by 1 SAIL level)
    (7, 8): {
        "a": SAILValue.V,    # Was IV in SORA 2.0
        "b": SAILValue.V,    # Was IV in SORA 2.0
        "c": SAILValue.VI,   # Was V in SORA 2.0
        "d": SAILValue.VI    # Was VI in SORA 2.0 (same)
    }
}

# OSO requirements by SAIL (same as SORA 2.0 - cumulative)
OSO_BY_SAIL_25 = {
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

# Flatten SAIL_TABLE_25 for direct (GRC, ARC) → SAIL lookup
_flat_table = {}
for grc_range, mappings in SAIL_TABLE_25.items():
    if isinstance(grc_range, tuple):
        for grc in grc_range:
            for arc, sail in mappings.items():
                _flat_table[(grc, arc)] = sail.value
    else:
        for arc, sail in mappings.items():
            _flat_table[(grc_range, arc)] = sail.value

# Replace grouped table with flat table
SAIL_TABLE_25 = _flat_table

# OSO count by SAIL level (string keys for compatibility)
OSO_BY_SAIL_25 = {
    "I": 6, "II": 10, "III": 15, "IV": 18, "V": 21, "VI": 24
}

# Verification data for testing
SORA_25_TEST_CASES = [
    # (GRC, ARC, Expected SAIL)
    (1, "a", "I"), (1, "b", "I"), (1, "c", "II"), (1, "d", "III"),
    (2, "a", "I"), (2, "b", "I"), (2, "c", "II"), (2, "d", "III"),
    (3, "a", "II"), (3, "b", "II"), (3, "c", "III"), (3, "d", "IV"),
    (4, "a", "II"), (4, "b", "II"), (4, "c", "III"), (4, "d", "IV"),
    (5, "a", "III"), (5, "b", "III"), (5, "c", "IV"), (5, "d", "V"),
    (6, "a", "IV"), (6, "b", "IV"), (6, "c", "V"), (6, "d", "VI"),   # Different from SORA 2.0
    (7, "a", "V"), (7, "b", "V"), (7, "c", "VI"), (7, "d", "VI"),     # Different from SORA 2.0
    (8, "a", "V"), (8, "b", "V"), (8, "c", "VI"), (8, "d", "VI"),     # Different from SORA 2.0
]

# Expected OSO counts by SAIL (same as SORA 2.0)
EXPECTED_OSO_COUNTS_25 = {
    "I": 6, "II": 10, "III": 15, "IV": 18, "V": 21, "VI": 24
}

# Key differences from SORA 2.0
SORA_25_DIFFERENCES = {
    (6, "a"): {"sora_20": "III", "sora_25": "IV", "change": "+1"},
    (6, "b"): {"sora_20": "III", "sora_25": "IV", "change": "+1"},
    (6, "c"): {"sora_20": "IV", "sora_25": "V", "change": "+1"},
    (6, "d"): {"sora_20": "V", "sora_25": "VI", "change": "+1"},
    (7, "a"): {"sora_20": "IV", "sora_25": "V", "change": "+1"},
    (7, "b"): {"sora_20": "IV", "sora_25": "V", "change": "+1"},
    (7, "c"): {"sora_20": "V", "sora_25": "VI", "change": "+1"},
    (8, "a"): {"sora_20": "IV", "sora_25": "V", "change": "+1"},
    (8, "b"): {"sora_20": "IV", "sora_25": "V", "change": "+1"},
    (8, "c"): {"sora_20": "V", "sora_25": "VI", "change": "+1"},
    # (7, "d") and (8, "d") remain VI in both versions
}

# SAIL matrix for easy reference
SAIL_MATRIX_25 = """
SORA 2.5 SAIL Matrix (JARUS SORA v2.5 Annex D)

GRC ↓ / ARC → │  a  │  b  │  c  │  d  │
──────────────┼─────┼─────┼─────┼─────┤
      1       │  I  │  I  │  II │ III │
      2       │  I  │  I  │  II │ III │
      3       │  II │  II │ III │  IV │
      4       │  II │  II │ III │  IV │
      5       │ III │ III │  IV │  V  │
      6       │ *IV │ *IV │ *V  │ *VI │  * Different from SORA 2.0
      7       │ *V  │ *V  │ *VI │  VI │  * Different from SORA 2.0
      8       │ *V  │ *V  │ *VI │  VI │  * Different from SORA 2.0

* = Differences from SORA 2.0 (more stringent requirements)
"""