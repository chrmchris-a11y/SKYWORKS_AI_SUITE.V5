# MASTER PROMPT — SKYWORKS SORA Engine (ARC/GRC/SAIL)

## Objective
Build a deterministic, table-driven ARC/GRC/SAIL computation engine aligned with **EASA SORA 2.0 (AMC/GM)** and **JARUS SORA 2.5 (Main Body + Annexes C/D/F)**, with full traceability, strict validation, and zero tolerance for rule violations.

## Deliverables
1) Python package `sora_calc` with modules: `arc.py`, `grc.py`, `sail.py`, `rules.py`, `models.py`, `engine.py`.
2) All rules defined in `rules/` as YAML (separate sets for 2.0 and 2.5). Include caps/floors and N/A constraints.
3) Pydantic models for requests/responses; strict enums with TitleCase normalization.
4) Deterministic engine with steps:
   - **ARC**: iARC → strategic mitigation (cap: ≤1 unless certified segregation) → residual ARC.
   - **GRC (2.0)**: iGRC → M1 (apply floor immediately) → M2 → M3 (+1/0/−1 rule) → clamp ≥1.
   - **GRC (2.5)**: iGRC → M1A/M1B/M1C (respect N/A) → M2 → clamp ≥1; enforce floors/caps per model.
   - **SAIL**: map (final GRC × final ARC) → SAIL (I–VI) per official tables.
5) Logging: return `calculation_trace[]` with (step, input, rule_ref, delta, result).
6) FastAPI API endpoints: `/sora/2.0/grc`, `/sora/2.0/arc`, `/sora/2.0/sail`, `/sora/2.5/*` returning output + `calculation_trace`.
7) Test suite: unit tests for each mitigation combo (valid/invalid), golden tests, and property-based tests.

## Constraints & Policies
- **No randomness**. All computations integer/deterministic.
- **Strict enums** + **TitleCase** normalization on inputs.
- **N/A guardrails** (422 on invalid combos, e.g., 2.5 M1C Medium/High).
- **Version pinning** and **immutable audit log** of inputs & intermediate states.
- **Rule refs**: include document, annex, section/page in every trace entry.

## Inputs Provided
- ARC/GRC/SAIL UI schemas and implementation notes (previous ZIPs).
- This prompt pack (acceptance criteria, algorithm spec, references, checklist).

## Output Format (example)
```json
{
  "version": "JARUS_SORA_2.5",
  "initial_grc": 6,
  "after_m1": 4,
  "after_m2": 2,
  "final_grc": 2,
  "initial_arc": "c",
  "residual_arc": "b",
  "sail": "IV",
  "calculation_trace": [
     {"step":"grc_m1","input":"High","delta":-4,"result":2,"rule_ref":"EASA_EAR_UAS_2024 Table3 note(d)"},
     {"step":"grc_m2","input":"High","delta":-2,"result":0,"rule_ref":"..."},
     {"step":"clamp","result":1,"rule_ref":"min_floor>=1"}
  ]
}
```
