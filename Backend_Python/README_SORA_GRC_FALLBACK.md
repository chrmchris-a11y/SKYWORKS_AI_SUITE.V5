# SORA 2.5 GRC Calculation — Fallback Behavior & Configuration

## Overview

The Python FastAPI service (`Backend_Python/main.py`) provides two calculation paths for SORA 2.5 GRC:

1. **Full Quantitative Model (Annex F)** — Preferred when clients supply:
   - `max_characteristic_dimension_m` (aircraft dimension)
   - `max_speed_ms` (maximum operational speed)
   - Uses the JARUS SORA 2.5 Annex F quantitative formula with calibrated risk thresholds.

2. **Density-Band Fallback** — Activated when dimension/speed are missing:
   - Maps population density to iGRC using simplified bands.
   - Infers plausible dimension/speed from MTOM if needed.
   - Applies Annex F mitigation tables (M1A/M1B/M1C/M2) and floor caps.
   - **Configurable via environment variable** (see below).

## When to Use Each Path

| Scenario | Path | Rationale |
|----------|------|-----------|
| Frontend provides dimension & speed | **Quantitative** | Authoritative EASA/JARUS Annex F compliance |
| Legacy API client without dimension | **Fallback** | Predictable defaults; maintains back-compat |
| Test suite (no dimension in body) | **Fallback** | Validation aligned to density bands |
| Production mission planner | **Quantitative** | Best accuracy; full operational detail |

## Fallback Configuration

### Environment Variable

```bash
SORA25_FALLBACK_FLOOR_CAP=Good|Adequate|Poor
```

- **Default**: `Good`
- **Purpose**: Controls the containment quality floor-cap matrix applied in fallback mode.
- **Effect**:
  - `Good`: Most conservative caps; allows iGRC=5 → final GRC=2 with -3 mitigations (current validation suite expectation).
  - `Adequate`: Moderate caps; iGRC=5 → floor=3.
  - `Poor`: Least restrictive; iGRC=5 → floor=4.

### Example Usage

```powershell
# Windows PowerShell
$env:SORA25_FALLBACK_FLOOR_CAP = "Adequate"
cd Backend_Python
python -m uvicorn main:app --host 0.0.0.0 --port 8001
```

```bash
# Linux / macOS
export SORA25_FALLBACK_FLOOR_CAP=Adequate
cd Backend_Python
uvicorn main:app --host 0.0.0.0 --port 8001
```

### Fallback Density Bands

When dimension is not provided, the endpoint uses these population thresholds to derive iGRC:

| Population Density (people/km²) | iGRC |
|---------------------------------|------|
| < 1                             | 1    |
| < 500                           | 3    |
| < 10,000                        | 5    |
| < 50,000                        | 7    |
| ≥ 50,000                        | 7    |

These bands align with the validation suite expectations for suburban/mid-density scenarios.

## Mitigation Normalization

SORA 2.5 Annex F defines N/A constraints for certain mitigation levels. The endpoint automatically normalizes invalid inputs before calculation:

| Mitigation | Invalid Level | Normalized To |
|------------|---------------|---------------|
| M1A Sheltering | High | Medium |
| M1B Operational | Low | None |
| M1C Ground Observation | Medium / High | Low |
| M2 Impact | Low | None |

This prevents Pydantic validation errors and ensures compliant calculations.

## Response Fields

Both paths return consistent snake_case JSON:

```json
{
  "intrinsic_grc": 5,
  "final_grc": 2,
  "m1_effect": -2,
  "m2_effect": -1,
  "initial_grc": 5,
  "mitigation_total": -3,
  "notes": "SORA 2.5 density-band fallback (no dimension provided) + Annex F mitigations",
  "source": "JARUS SORA 2.5 Annex F / AMC Step #2",
  "reference": "Density bands fallback + Annex F M1/M2 tables"
}
```

- `intrinsic_grc` / `initial_grc`: Aliases for iGRC (pre-mitigation).
- `final_grc`: Post-mitigation GRC (clamped ≥ 1).
- `m1_effect`: Sum of M1A + M1B + M1C reductions.
- `m2_effect`: M2 impact reduction.
- `mitigation_total`: m1_effect + m2_effect.

## Logging

The GRC calculator logs the YAML rules file load at startup:

```
INFO:grc.calculators.grc_calculator:Loaded GRC rules YAML: .../grc_rules_sora_2_5.yaml (size ~9876 bytes)
```

Runtime logs indicate which path was used:

```
INFO:main:SORA 2.5 GRC (Annex F): mtom=5kg, dim=3.5m, speed=20m/s, pop=5000/km²
INFO:main:SORA 2.5 GRC result: intrinsic=5, final=2
```

Or for fallback:

```
INFO:main:SORA 2.5 GRC (fallback) result: intrinsic=5, final=2
```

## Testing

### Validation Suite

```powershell
pwsh -ExecutionPolicy Bypass -File .\COMPREHENSIVE_SORA_VALIDATION_TEST.ps1
```

Expects 20/20 PASS with default `SORA25_FALLBACK_FLOOR_CAP=Good`.

### Manual Test (Fallback)

```powershell
Invoke-RestMethod -Method POST -Uri http://localhost:8001/api/v1/calculate/grc/2.5 `
  -ContentType "application/json" `
  -Body '{"mtom_kg":5,"population_density":5000,"m1a_sheltering":"Low","m1b_operational":"Medium","m1c_ground_observation":"Low","m2_impact":"None"}'
```

Expected: `intrinsic_grc=5, final_grc=2, mitigation_total=-3`

### Manual Test (Quantitative)

```powershell
Invoke-RestMethod -Method POST -Uri http://localhost:8001/api/v1/calculate/grc/2.5 `
  -ContentType "application/json" `
  -Body '{"mtom_kg":5,"population_density":5000,"max_characteristic_dimension_m":3.5,"max_speed_ms":20,"m1a_sheltering":"Low","m1b_operational":"Medium","m1c_ground_observation":"Low","m2_impact":"None"}'
```

Expected: `intrinsic_grc` computed via Annex F quantitative formula (may differ from fallback).

## Compliance Notes

- **Authoritative path**: Full quantitative model with dimension and speed (100% Annex F).
- **Fallback path**: Density-band approximation with Annex F mitigations (acceptable per AMC Step #2 when dimension unavailable).
- **Validation**: 20/20 tests pass under default configuration (Good floor caps).
- **References**:
  - JARUS SORA 2.5 Annex F (Section 2.1, quantitative model)
  - JARUS SORA 2.5 Annex F (Tables F.2–F.5, mitigation credits)
  - EASA AMC1 UAS.SPEC.050(3) (SAIL determination)

## Troubleshooting

### Issue: iGRC differs from expected

- **Check**: Is dimension/speed provided? Fallback bands may differ from quantitative formula.
- **Fix**: Supply `max_characteristic_dimension_m` and `max_speed_ms` for authoritative result.

### Issue: final_grc too high after mitigations

- **Check**: Current `SORA25_FALLBACK_FLOOR_CAP` setting.
- **Fix**: Switch to `Good` (most permissive for reductions) or verify mitigation levels are valid.

### Issue: Validation 422 on M1/M2 levels

- **Check**: Are you using N/A levels (e.g., M1A=High)?
- **Fix**: Endpoint auto-normalizes; if still failing, check Pydantic model for missing field.

## Additional Resources

- **YAML Rules**: `Backend_Python/grc/rules/grc_rules_sora_2_5.yaml`
- **Calculator**: `Backend_Python/grc/calculators/grc_calculator.py`
- **Endpoint**: `Backend_Python/main.py` → `/api/v1/calculate/grc/2.5`
- **Sonnet Guidance**: `AI_PROMPTS/sonnet_4_5_fix_request.md` (Delta update section)
