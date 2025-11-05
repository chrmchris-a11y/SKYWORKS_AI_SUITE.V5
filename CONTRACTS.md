# SKYWORKS Contracts: SORA 2.0/2.5 (Annex D/F)

Last updated: 2025-11-05
Status: In effect (aliases in migration window)

## Scope
- Canonical request/response contracts between:
  - Client → Python FastAPI (authoritative SORA logic)
  - .NET API (5210) → Python service (8001)
- Versions covered: SORA 2.0 (Annex D, qualitative ARC → SAIL) and SORA 2.5 (Annex F quantitative GRC + numeric residual ARC → SAIL)
- Objective: strict compliance with EASA/JARUS, reproducible CI parity, zero heuristics in CI/Prod.

## Endpoints
- Python FastAPI
  - GET /health
  - POST /api/v1/calculate/grc/2.5
  - POST /api/v1/calculate/sail
- .NET API (proxy/composite)
  - GET /api/sora/info
  - POST /api/proxora/grc/2.5
  - POST /api/proxora/sora (composite)

## Contracts by Version

### 2.5 — Annex F (Quantitative)

Inputs (units are strict):
- mtom_kg: number — Maximum take-off mass [kilograms]
- max_speed_ms: number — Maximum ground speed [meters/second]
- max_characteristic_dimension_m: number — Max characteristic dimension (span/diameter) [meters]
- population_density: number — Persons per square meter (or as specified by Annex F); domain validated
- mitigations: object — granular flags/parameters per Annex F (e.g., M1/M2 subcomponents)
- residual_arc_level: integer — Required for SAIL routing (2→very low … 9→very high); used by /calculate/sail

Notes:
- Legacy aliases are accepted only via the .NET binder layer during migration (see “Aliases and Migration”). Python 2.5 endpoints expect the exact names above.

Responses:
- /calculate/grc/2.5 → { final_grc: integer, rationale: string, …vendor fields… }
- /calculate/sail → requires {
  sora_version: "2.5",
  final_grc: integer,
  residual_arc_level: integer
} and returns { sail: "I"|…|"VI", rationale: string, … }

Validation & errors:
- 400/422 when required fields missing/invalid; error “detail” may be string, array of strings, or object with field errors. Clients must handle all three.
- Extra/legacy fields are rejected by Python for 2.5.

### 2.0 — Annex D (Qualitative)

Inputs:
- final_grc: integer per legacy GRC scale
- final_arc: string enum — one of "a"|"b"|"c"|"d" (case-insensitive)

Responses:
- /calculate/sail → requires {
  sora_version: "2.0",
  final_grc: integer,
  final_arc: "a"|"b"|"c"|"d"
} → { sail: "I"|…|"VI", rationale: string, … }

Behavioral differences vs 2.5:
- OSO counting applies only in 2.0 paths.
- 2.5 requires numeric residual_arc_level; letters are not valid in 2.5 to Python.

## Aliases and Migration (temporary)
To ease migration in .NET models and composite payloads, the API layer currently binds the following legacy keys to Annex F names:
- weight_kg → mtom_kg
- max_speed_mps → max_speed_ms
- max_dimension_m → max_characteristic_dimension_m

Additionally, if only an ARC letter is supplied on a 2.5 composite proxy, the .NET proxy maps it to a representative residual_arc_level for SAIL call:
- a → 2, b → 4, c → 6, d → 9

Important:
- These mappings exist only in the .NET proxy/binder. The Python 2.5 endpoints remain strict and do not accept legacy names.
- Aliases are slated for removal after the deprecation window (see below).

## Deprecation and Telemetry
- Telemetry: Count alias usage in .NET binder (weight_kg, max_speed_mps, max_dimension_m) and ARC-letter mapping in 2.5 proxy.
- Policy: Remove aliases after 1 sprint of zero usage. Hard fail in CI before removal.
- Tagging: Any dev-only heuristic toggles must tag responses with metadata: compliance_tag: "NON-COMPLIANT".

## Compliance Flags and Heuristics
- ANNEXC_STRICT_ENV: must be 0 for CI/Prod; no heuristics applied.
- Any experimental relaxations only permitted in local/dev and must not ship.

## Acceptance Matrix (excerpt)
- 2.0: (GRC=5, ARC=c) → SAIL IV; (GRC=8, any ARC) → Category C (sail=null)
- 2.5: residual_arc_level numeric; (GRC=9 or 10, any level) → SAIL VI

Artifacts and checks:
- Tools/web-smoke-tests.ps1 validates composite and Python endpoints with Annex F keys.
- Tools/ci_golden_checks.py enforces parity and golden scenarios.

## Examples

2.5 SAIL request:
```json
{
  "sora_version": "2.5",
  "final_grc": 9,
  "residual_arc_level": 5
}
```

2.5 GRC request:
```json
{
  "mtom_kg": 6.8,
  "max_speed_ms": 18.2,
  "max_characteristic_dimension_m": 0.55,
  "population_density": 0.002,
  "mitigations": { "m1_geofencing": true, "m2_latency_ms": 120 }
}
```

2.0 SAIL request:
```json
{
  "sora_version": "2.0",
  "final_grc": 5,
  "final_arc": "c"
}
```

## Error Model (client handling)
- detail: string → display/log as-is.
- detail: string[] → join with newlines for UI; keep array for logs.
- detail: { [field]: string|string[] } → flatten to field: messages.

## Contract Invariants
- Units and names are immutable in 2.5 Python endpoints.
- CI must run golden checks and smoke tests on every PR.
- Any change to inputs or outputs requires updating this document and bumping API minor version with a deprecation plan.
