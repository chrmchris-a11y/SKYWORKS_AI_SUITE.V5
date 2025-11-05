# SAIL API – Version-aware Contract and Examples

This document provides version-specific request/response examples for the Python SAIL API.

## Endpoints
- POST /api/v1/calculate/sail

## Request Models
- SORA 2.0 (letter ARC a–d; GRC>7 ⇒ Category C)
  {
    "sora_version": "SORA_2.0",
    "grc_level": 5,
    "arc_level": "c"
  }

- SORA 2.5 (numeric residual ARC 1..10 only)
  {
    "sora_version": "SORA_2.5",
    "grc_level": 6,
    "residual_arc_level": 4
  }

## Response Models
- SORA 2.0 (GRC 5, ARC c → IV)
  {
    "sail_level": "IV",
    "sail": "IV",
    "sora_version": "SORA_2.0",
    "grc_level": 5,
    "arc_level": "c",
    "residual_arc_level": null,
    "category": null,
    "oso_count": null,
    "reference": "EASA AMC/GM SORA 2.0 Annex D (Table D.1)"
  }

- SORA 2.0 Category C (GRC 8)
  {
    "category": "C",
    "sail_level": null,
    "sail": null,
    "sora_version": "SORA_2.0",
    "grc_level": 8,
    "arc_level": "a",
    "residual_arc_level": null,
    "oso_count": null,
    "reference": "EASA AMC/GM SORA 2.0 Annex D (Table D.1) – Category C trigger (GRC>7)",
    "notes": "Category C (Certified) operation required per SORA 2.0 when final GRC > 7"
  }

- SORA 2.5 (GRC 9, residual_arc_level 5 → VI for high rows)
  {
    "sail_level": "VI",
    "sail": "VI",
    "sora_version": "SORA_2.5",
    "grc_level": 9,
    "arc_level": null,
    "residual_arc_level": 5,
    "category": null,
    "oso_count": null,
    "reference": "JARUS SORA 2.5 Annex D (Table 7)",
    "oso_count_source": "derived-2.5"
  }

## Error Taxonomy
- 400: Missing/invalid inputs at the transport layer
- 422: Pre-model validation failures (schema/conditional)
- 501: Missing module/feature (unimplemented)
- 500: Unexpected server error

## Notes
- SORA 2.5 always echoes numeric residual_arc_level; there is no letter mapping for 2.5.
- Single source of truth: SORA 2.0 and 2.5 SAIL tables live under `sail/data/` and are consumed by calculators to avoid drift.
