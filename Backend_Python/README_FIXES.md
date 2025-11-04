# SKYWORKS SORA Fix Pack - Applied Changes

This repository enables config-driven governance for SORA 2.0/2.5.

## What changed

- Added rules/grc_rules_sora_2_5.yaml
  - Authoritative SORA 2.5 Annex-A Table A-1 iGRC matrix
  - Optional Annex-F style quantitative parameters (disabled by default)
  - M1A/B/C & M2 mitigation credits in config
- Added rules/grc_rules_sora_2_0.yaml
  - SORA 2.0 Table 2 iGRC by scenario and mitigation factors
- Added tables/sail_table_v20.json
  - Official GRC×ARC → SAIL matrix (GM/AMC)
- Python calculators updated to optionally load config files safely (no new dependencies required)
  - Fallback to built-in matrices if files are missing

## Why

- EASA GM/AMC allows quantitative or qualitative criteria for Step #2 (GRC); this lets us tune without code.
- SAIL mapping is standardized; externalizing ensures traceability and easy audit.
- Governance-by-config reduces code churn, enabling deterministic outcomes tied to documented sources.

## Scope gate (C# Orchestration)

- Out of scope only when Final GRC > 7 (Category C): SCOPE_HIGH_GRC
- SAIL VI is always out of scope for Specific category
- Combined GRC≥5 + ARC-d is NOT an automatic out-of-scope condition

## ARC-d rule (Python ARC v2.5)

- Controlled airspace and altitude_agl_m > 150 → initial ARC-d before mitigations

## How to tune (Annex-F optional)

- Set `quantitative.enabled: true` in `rules/grc_rules_sora_2_5.yaml`
- Adjust exponents/factor and risk_score_mapping bins to align with authoritative tests
- Keep internal cap 8; orchestration enforces Final GRC ≤ 7 for in-scope operations

## Verification

- Python unit tests should pass unchanged.
- .NET end-to-end: GOLD-25-OOS-001 returns 400 only if Final GRC > 7.
- SORA25_002 (Urban + Sheltering): iGRC ≈ 6 → Final 5; ARC_c → SAIL IV.
- SORA25_007 (Controlled >500 ft): ARC-d, with proper SAIL mapping via JSON.
