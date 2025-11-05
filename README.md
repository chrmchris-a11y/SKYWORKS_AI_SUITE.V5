# SORA Compliance Stack - Components 6-8

This section covers OSO requirements, validation, and .NET service integration for the SORA compliance stack.

## Version Differences

**SORA 2.0 vs 2.5 Key Differences:**
- **GRC Range**: 2.0 supports 1-7 (>7 = Category C), 2.5 supports 1-10
- **ARC Format**: 2.0 uses letters (a-d), 2.5 uses numeric residual ARC (1-10)
- **SAIL Tables**: 2.0 uses Table D.1, 2.5 uses Table 7 with GRC 9-10 → SAIL VI rule
- **OSO Counts**: 2.0 returns fixed counts (I→6, II→10, III→15, IV→18, V→21, VI→24), 2.5 returns None
- **OSO Structure**: 2.5 has different OSO sets and robustness models per Annex E

## Components

### OSO Requirements (`oso_requirements.py`)
- `get_oso_count_for_sail()`: Returns counts for 2.0, None for 2.5
- `get_oso_requirements_for_sail()`: Returns OSO list with robustness per SAIL
- Data sources: `/sail/data/oso_map_2_0.yaml` and `/sail/data/oso_map_2_5.yaml`

### SAIL Validator (`sail_validator.py`)
- Version-aware validation for GRC, ARC, and SORA version inputs
- Handles Category C for 2.0 (GRC > 7), strict ranges for 2.5
- Normalizes inputs and provides detailed error messages

### .NET Service (`SAILService.cs`)
- Calls Python API endpoints to maintain single source of truth
- Provides typed interfaces for C# clients
- Includes telemetry and proper error handling
- Returns OSO counts for 2.0, null counts for 2.5

## References
- **SORA 2.0**: EASA AMC/GM to Reg. (EU) 2019/947, Annexes D & E
- **SORA 2.5**: JARUS SORA v2.5 Main Body & Annexes, Step 9/Annex D Table 7, Annex E
