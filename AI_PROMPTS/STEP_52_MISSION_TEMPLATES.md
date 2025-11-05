# Step 52 — Mission Templates (Prompt for Sonnet 4.5)

## Objective
Author a reusable, versioned **Mission Template Schema** and seed catalog that pre‑configures missions while **enforcing SORA input contracts**.

## Regulatory Anchors
- **SORA 2.5 (Annex F)**: required numeric inputs & NA rules (M1A High→Medium; M1B Low→None; M1C only None/Low; M2 Low→None).
- **SAIL 2.5**: Step #9 via Annex D/Table 7 with **numeric** residual ARC.
- **SORA 2.0**: Step #9 via Annex D Table D.1 with **letter** ARC (a–d); **Category C** when final GRC>7.
- **OSO counts** exposed **only** for 2.0 (I=6, II=10, III=15, IV=18, V=21, VI=24).

## Deliverables
1. **Schema v1.0**: identity, sora_version, scenario_tag, constraints (altitude/geofence/speed), SORA input block (2.0 vs 2.5), mitigations profile, crew/competency, ERP, docs pack, regulatory anchors, UI bindings, telemetry tags.
2. **Seed Catalog (≥6)**: Urban BVLOS facade cleaning; Suburban VLOS solar; STS‑01 roof; Linear BVLOS utility; Night VLOS urban; Harbor VLOS near RMZ/TMZ.
3. **Validation Rules**: hard validation (block), normalization (apply NA), policy warnings.
4. **Acceptance Criteria**: template → API validate → SAIL preview; parity test (.NET⇄Python); smoke pass.
5. **Risks**: template/backend drift → round‑trip check and version pinning.

## References
- JARUS SORA 2.5 Main Body §Annex F, §Annex D (Table 7).  
- EASA SORA 2.0 AMC/GM §Annex D (Table D.1).  
- EU 2019/947; STS‑01/02 AMC/GM; PDRA‑S01 (constraints mapping).
