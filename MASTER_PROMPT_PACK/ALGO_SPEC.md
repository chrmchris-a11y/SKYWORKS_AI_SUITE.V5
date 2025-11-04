# ALGO SPEC — Architecture & Sequencing

## Package Layout
sora_calc/
  rules/
    sora20/
      arc_initial.yaml
      arc_mitigation.yaml
      grc_mitigations.yaml
      sail_mapping.yaml
    sora25/
      arc_initial.yaml
      arc_annex_c.yaml
      grc_mitigations.yaml
      sail_mapping.yaml
  models.py        # Pydantic models & enums (strict)
  rules.py         # YAML loaders, versioning, validators
  arc.py           # iARC & residual ARC
  grc.py           # iGRC & final GRC
  sail.py          # SAIL mapping
  engine.py        # Orchestrates end-to-end calculation

## Key Sequencing
- ARC: iARC → strategic mitigation (apply cap) → residual ARC.
- GRC (2.0): iGRC → M1 (apply floor immediately) → M2 → M3 → clamp ≥1.
- GRC (2.5): iGRC → M1A/M1B/M1C (check N/A) → M2 → clamp ≥1.
- SAIL: (final GRC × final ARC) → SAIL I–VI.

## Trace Object
{ step, inputs, rule_ref, delta, result, timestamp }

## Algorithmic Notes
- Implement lookups as table indexes; avoid branching explosion.
- Floors/Caps encoded in rule tables for transparency.
- TitleCase normalization at boundary (request parsing).
