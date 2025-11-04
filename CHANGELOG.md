# Changelog

All notable changes to this project will be documented in this file.

## [2.0.0] - 2025-11-04

### Highlights
- SORA 2.5 Step #9 now returns only numeric residual ARC levels (1..10). No letters in 2.5 results.
- SORA 2.0 retains letter ARC (a–d) and enforces Category C when final GRC > 7.
- OSO behavior is versioned: counted only in 2.0; for 2.5, OSO is null/derived as specified.
- EU profile adjustments: ignores Mode‑S veil; no SUBURBAN→URBAN coercion unless strict flag is set.
- Production hardening: only authentic logic in production; dev fallbacks are gated by flag.
- API Contract v1.0 frozen; OpenAPI 3.1 updated and validated.
- Mapping tables finalized including SORA 2.5 coverage for GRC 9–10 → VI.

### API Contract (v1.0)
- Response models unified across SORA 2.0/2.5 while preserving versioned semantics.
- New/clarified fields:
  - 2.5: `residual_arc_level` is an integer 1..10.
  - 2.0: `residual_arc_letter` in {a,b,c,d}; `category` coerced to `C` when `grc_final > 7`.
- Endpoints: clarified GET/POST semantics where applicable and removed non-authentic fallbacks in production paths.

### Environment flags
- `ALLOW_NONCOMPLIANT_FALLBACKS=0|1`
  - Default: `0` (disabled). When `1`, limited dev-only fallbacks are enabled.
- `ANNEXC_STRICT_ENV=0|1`
  - Default: `0`. When `1`, optionally coerce SUBURBAN→URBAN in front-end mapping per strict Annex C interpretation.
- `CORS_ORIGINS="origin1,origin2"`
  - Default: `*`. Set to your real frontend origins in production, e.g. `http://localhost:5210`.
- `LOGLEVEL=DEBUG|INFO|WARNING|ERROR`
  - Default: `INFO`.

### EU Profile specifics
- Mode‑S veil ignored.
- SUBURBAN→URBAN coercion is off by default; enable only with `ANNEXC_STRICT_ENV=1`.

### Security & compliance
- Removed hard-coded Anthropic API keys from scripts/configs; switched to `ANTHROPIC_API_KEY` environment variable.
- `.env` sanitized and env files added to `.gitignore`.
- Published release on a clean orphan branch to avoid tainted history; tag `v2.0.0` points to the clean commit.

### Migration notes
- If your client expected SORA 2.5 letter residual ARC, update to numeric 1..10.
- If you relied on production fallbacks, set `ALLOW_NONCOMPLIANT_FALLBACKS=1` only in development environments.
- Review CORS settings for production: set `CORS_ORIGINS` to your actual origins rather than `*`.

### Testing & validation
- Updated unit tests to match official matrices for SORA 2.0/2.5.
- Health checks stabilized; web smoke tests added.
- OpenAPI renders and matches the frozen contract v1.0.

### Known follow-ups
- Verify CORS and flags in deployment target.
- Publish release notes in the repo/PR and update any external docs.

---

## [1.x] - Previous releases
- Earlier releases combined or allowed permissive fallbacks; this version splits behavior cleanly between SORA 2.0 and 2.5 and removes non-authentic logic from production paths.
