# Backend_Python API quick reference

This service implements SORA 2.0 & 2.5 endpoints (FastAPI).

## Contract v1.0 (frozen)

- SORA 2.5 SAIL: numeric residual ARC only via `residual_arc_level` in [1..10]; letter tokens are rejected (400).
- SORA 2.0 SAIL: letter residual ARC a–d; when final GRC > 7 ⇒ Category C (no SAIL value).
- OSO counts: returned only for SORA 2.0; SORA 2.5 returns null/derived.
- EU profile: ignores Mode‑S veil; no SUBURBAN→URBAN coercion unless `ANNEXC_STRICT_ENV=1`.
- Authoritative-only in production; dev fallbacks gated behind `ALLOW_NONCOMPLIANT_FALLBACKS=1`.
- Response shape: `sail_level` (primary) with `sail` alias; optional fields for version-aware payloads.

## Health

- GET http://localhost:8001/health
  - Shows module flags: sora_20, sora_25, arc_alt_available, sail_router_mounted
  - Config echoes: ANNEXC_STRICT_ENV, CORS_ORIGINS

## SAIL

- POST http://localhost:8001/api/v1/calculate/sail (main)
  - SORA 2.0: `residual_arc` letter a–d; Category C when `final_grc > 7`
  - SORA 2.5: `residual_arc_level` 1..10 (numeric only)

- POST http://localhost:8001/sail/calculate (router, authoritative)
  - 2.5 numeric ARC only; oso_count is null by design

## GRC

- POST http://localhost:8001/api/v1/calculate/grc/2.0
  - Authoritative path when `operational_scenario` and `max_dimension_m` present
  - Dev-only fallback behind `ALLOW_NONCOMPLIANT_FALLBACKS=1`

- POST http://localhost:8001/api/v1/calculate/grc/2.5
  - Annex F quantitative model (requires dimension + speed)

## ARC (FE mapping)

- POST http://localhost:8001/arc/v2.0/initial
- POST http://localhost:8001/arc/v2.5/initial
  - Always returns 200 with `{ ok, data, error }` body
  - EU profile: ignores Mode-S veil
  - Environment: preserved by default; optional SUBURBAN→URBAN coercion when `ANNEXC_STRICT_ENV=1`

## Example requests

```bash
# Health
curl -s http://localhost:8001/health | jq

# SAIL 2.5 (numeric ARC, main endpoint)
curl -sX POST http://localhost:8001/api/v1/calculate/sail \
  -H "Content-Type: application/json" \
  -d '{"sora_version":"2.5","final_grc":5,"residual_arc_level":4}' | jq

# SAIL 2.0 (letter ARC, main endpoint)
curl -sX POST http://localhost:8001/api/v1/calculate/sail \
  -H "Content-Type: application/json" \
  -d '{"sora_version":"2.0","final_grc":5,"residual_arc":"c"}' | jq

# Optional: SAIL 2.5 via router (authoritative table)
curl -sX POST http://localhost:8001/sail/calculate \
  -H "Content-Type: application/json" \
  -d '{"sora_version":"2.5","final_grc":5,"residual_arc_level":4}' | jq

# GRC 2.0 authoritative
curl -sX POST http://localhost:8001/api/v1/calculate/grc/2.0 \
  -H "Content-Type: application/json" \
  -d '{"mtom_kg":12,"population_density":300,"operational_scenario":"BVLOS_Populated","max_dimension_m":1.2}' | jq

# GRC 2.5 (Annex F)
curl -sX POST http://localhost:8001/api/v1/calculate/grc/2.5 \
  -H "Content-Type: application/json" \
  -d '{"mtom_kg":12,"population_density":850,"max_characteristic_dimension_m":1.2,"max_speed_ms":20,"m1a_sheltering":"Medium","m2_impact":"Medium"}' | jq

# ARC 2.0 FE mapping
curl -sX POST http://localhost:8001/arc/v2.0/initial \
  -H "Content-Type: application/json" \
  -d '{"max_height_agl_m":120,"environment":"Urban","airspace_class":"C","is_controlled":true}' | jq
```

## Configuration flags

- `ANNEXC_STRICT_ENV=1` → coerces SUBURBAN→URBAN in FE mapping
- `CORS_ORIGINS` → comma-separated origins (default `*`)
- `ALLOW_NONCOMPLIANT_FALLBACKS=1` → enable GRC 2.0 dev fallback
