# Request: Align SKYWORKS AI SUITE with JARUS/EASA and fix failing comprehensive scripts

You are an expert engineer tasked to bring the SKYWORKS AI SUITE to green for all tests while preserving backwards compatibility. Please read carefully and produce code diffs and rationale. Keep changes minimal, additive, and covered by tests.

## Current status (verified 2025-11-03)
- Primary .NET test suite (API + ProxORA + Python) is green: Total 273 • Passed 272 • Skipped 1.
- Web Smoke Tests: PASS.
- Two legacy comprehensive PowerShell scripts fail end-to-end:
  - COMPREHENSIVE_SYSTEM_TEST.ps1 → FAIL: Total 17, Passed 0, Failed 17.
  - COMPREHENSIVE_SORA_VALIDATION_TEST.ps1 → FAIL: Total 20, Passed 0, Failed 20.

## Failure patterns from the legacy scripts
1) ARC endpoints 404
   - Scripts call: /api/v1/calculate/arc/2.0 and /api/v1/calculate/arc/2.5 on the Python service (http://localhost:8001)
   - Actual endpoints available today:
     - Python: /arc/v2.0/initial and /arc/v2.5/initial
     - API Proxora: /api/Proxora/arc/2.0 and /api/Proxora/arc/2.5 (preferred to ensure non-empty ARC via fallbacks)

2) SAIL 422 (Unprocessable Content)
   - The scripts post to /api/v1/calculate/sail (Python) with bodies like { final_grc: 3/4/5/6/7/8, residual_arc: "ARC-<x>" } and receive 422.
   - The .NET suite succeeds when using { sora_version: "2.0", final_grc: n, residual_arc: "ARC_b" } etc.
   - Hypothesis: The SAIL service currently expects certain combinations or stricter validation; version parameter may be required/assumed; acceptable ARC tokens may be normalized.

3) GRC initial_grc/mitigation_total mismatch
   - Legacy scripts expect: initial_grc and mitigation_total
   - Python responses provide: intrinsic_grc, final_grc, m1_effect, m2_effect, (and m3_effect for 2.0)
   - Scripts show blanks for initial_grc/mitigation_total, causing FAIL on comparisons.

## Goal
Make the comprehensive scripts pass without breaking the primary green suite. Prefer additive compatibility on the Python API (aliases/compat shims) OR minimally patch the scripts to use current endpoints and field names. Choose the smallest, safest path with tests.

## Constraints and preferences
- DO NOT break existing .NET green tests.
- Prefer backwards-compatibility in Python service by adding non-invasive aliases/fields.
- If script edits are simpler and safer, update only the scripts and add comments. Keep expectations accurate to authoritative tables.
- Proxora controller fallbacks (in C#) guarantee non-empty ARC. Reusing them from scripts is allowed.

## Proposed minimal changes (choose A or B)
A) Backwards-compatible Python API shims
   1. GRC endpoints (/api/v1/calculate/grc/2.0 and /2.5):
      - Include initial_grc in the JSON as an alias of intrinsic_grc.
      - Include mitigation_total computed as m1_effect + m2_effect (+ m3_effect for 2.0). Keep existing m*_effect fields.
   2. SAIL endpoint (/api/v1/calculate/sail):
      - Accept payloads without sora_version; default to 2.0; keep 2.5 valid.
      - Tolerate residual_arc casing/tokens (e.g., "ARC-a", "ARC_a", "a"). Normalize internally.
      - Return 200 for valid table combinations per JARUS SORA Table 1; return 422 only on truly invalid combos.
   3. ARC routes:
      - Add compatibility routes /api/v1/calculate/arc/2.0 and /2.5 that forward to /arc/v2.x/initial.

B) Script-only updates (PowerShell)
   1. Point ARC tests to API Proxora endpoints: http://localhost:5210/api/Proxora/arc/{version}
   2. Change expected GRC fields: initial_grc → intrinsic_grc; mitigation_total → compute from m*_effect in the script or compare final_grc only.
   3. For SAIL tests, add sora_version: "2.0" to the request bodies and ensure residual_arc tokens are acceptable (e.g., "ARC-b").

Pick A if you control Python service and want stable, backward-compatible API. Pick B if you want a quick fix limited to tests without touching service behavior.

## Acceptance criteria
- Primary .NET test suite remains green: Total 273, Passed 272, Skipped 1 (or better).
- Web Smoke Tests remain PASS.
- COMPREHENSIVE_SYSTEM_TEST.ps1: All tests PASS.
- COMPREHENSIVE_SORA_VALIDATION_TEST.ps1: All tests PASS.
- No regressions on endpoints documented below.

## Reference endpoints and files
- Python service (current):
  - GRC 2.0: POST /api/v1/calculate/grc/2.0 → { intrinsic_grc, final_grc, m1_effect, m2_effect, m3_effect? }
  - GRC 2.5: POST /api/v1/calculate/grc/2.5 → { intrinsic_grc, final_grc, m1_effect, m2_effect }
  - ARC 2.0: POST /arc/v2.0/initial → { initial_arc, residual_arc, ... }
  - ARC 2.5: POST /arc/v2.5/initial → { initial_arc, residual_arc, ... }
  - SAIL: POST /api/v1/calculate/sail → { sail, ... }
- API Proxora (C#):
  - /api/Proxora/arc/2.0 and /api/Proxora/arc/2.5 (stabilized ARC with fallbacks)
  - /api/Proxora/sora (composite SORA)

## Authoritative documentation to align with
- EASA AMC/GM to Regulation (EU) 2019/947 — SORA Step #2/#3 (GRC), Step #5 (SAIL), Annex guidance.
- JARUS SORA v2.0 (JAR_doc_06): Ground risk tables and mapping; SAIL Table 1.
- JARUS SORA v2.5 (Annex F): Quantitative model for GRC with M1/M2 mitigations.
- JARUS SORA Annex C: Strategic mitigations handling and double counting constraints (for ARC reductions).

## Deliverables
1. Code changes (choose A or B) with brief rationale.
2. Updated tests:
   - If A: add/adjust minimal Python unit tests for alias fields and SAIL normalization.
   - If B: update the two PowerShell scripts and re-run to 100% pass.
3. Short CHANGELOG entry and API response examples.

## Notes on edge cases
- Ensure ARC normalization handles absent residual_arc by falling back to initial_arc or defaulting to lowest safe (e.g., "ARC_b").
- For SAIL, reject only non-sense combos; valid table combinations should return 200 with a clear sail value.
- Avoid double-counting in strategic mitigations per Annex C; if exposed in API, document behavior.

## Runbook after changes
1) Start Python (8001) and API (5210)
2) Run .NET tests: should remain green.
3) Run Web Smoke Tests: PASS.
4) Run COMPREHENSIVE_SYSTEM_TEST.ps1 and COMPREHENSIVE_SORA_VALIDATION_TEST.ps1: PASS.

Thank you. Please provide a unified diff or patch ready to apply, plus a short summary of the changes.
