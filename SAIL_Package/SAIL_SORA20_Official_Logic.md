# SAIL — SORA 2.0 (EASA AMC/GM) — Official Logic Summary
Generated: 2025-10-30T09:51:50.020945Z

## Purpose
Map **final ARC (a–d)** and **final GRC (1–8)** to **SAIL I–VI** per EASA AMC/GM (Step 9, Annex D).

## Inputs
- **final_grc** ∈ {1..8}
- **final_arc** ∈ {a,b,c,d}

## Mapping Table (conceptual pattern consistent with AMC Table D.1)
| GRC ↓ / ARC → | a | b | c | d |
|---|---|---|---|---|
| 1–2 | I | I | II | III |
| 3–4 | II | II | III | IV |
| 5–6 | III | III | IV | V |
| 7–8 | IV | IV | V | VI |

> Implementation note: Use grouped GRC rows (1–2, 3–4, 5–6, 7–8) for efficient lookup.

## Outputs
- **sail** ∈ {I, II, III, IV, V, VI}

## OSO/Robustness Link (Annex D)
Each SAIL determines required **Operational Safety Objectives (OSOs)** and **robustness/assurance** levels (Low/Medium/High). Store an index to your OSO matrix to retrieve requirements per SAIL.
