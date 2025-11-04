# SAIL — SORA 2.5 (JARUS) — Official Logic Summary
Generated: 2025-10-30T09:51:50.020945Z

## Purpose
Map **final ARC (a–d)** and **final GRC (1–8)** to **SAIL I–VI** per JARUS SORA v2.5 (Main Body + Annex D v1.0).

## Inputs
- **final_grc** ∈ {1..8}
- **final_arc** ∈ {a,b,c,d}

## Mapping Table (conceptual pattern aligned with JARUS v2.5)
| GRC ↓ / ARC → | a | b | c | d |
|---|---|---|---|---|
| 1–2 | I | I | II | III |
| 3–4 | II | II | III | IV |
| 5 | III | III | IV | V |
| 6 | IV | IV | V | VI |
| 7–8 | V | V | VI | VI |

> Notes:
> - v2.5 clarifies certain VLOS/short‑exposure cases; some authorities allow **“light SAIL II”** classification for very low‑risk VLOS operations meeting specific criteria.
> - Keep the standard mapping above unless a competent authority provides a qualified deviation for special cases.

## Outputs
- **sail** ∈ {I, II, III, IV, V, VI}

## OSO/Robustness Link (Annex D v1.0)
Use SAIL to query the **OSO set** and **robustness/assurance** levels (L/M/H). Maintain traceability to the exact Annex D clauses used.
