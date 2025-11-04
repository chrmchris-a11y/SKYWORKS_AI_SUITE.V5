# GRC — SORA 2.5 (JARUS) — Official Logic Summary
Generated: 2025-10-30T09:42:25.045761Z

## Step #2 — Intrinsic/Initial GRC (iGRC) — Quantitative model
- **iGRC** is determined using the **quantitative ground-risk model** (Main Body + Annex F).  
- Required inputs: **characteristic dimension (m)**, **maximum speed (m/s)**, **population density (people/km²)**, **environment/context**, and **containment**.  
- Output: **iGRC ∈ {1..8}**.

## Step #3 — Final GRC via Mitigations (split M1 into A/B/C)
Apply mitigations in order and clamp minimum 1:

1) **M1A — Sheltering / population exposure reduction**  
   Allowed levels & effects: **None=0**, **Low=−1**, **Medium=−2**, **High = N/A**.

2) **M1B — Operational restrictions (impact angle/trajectory/temporal boundary)**  
   Allowed levels & effects: **None=0**, **Low=N/A**, **Medium=−1**, **High=−2**.

3) **M1C — Ground observation / tactical measures (population warning/clearing)**  
   Allowed levels & effects: **None=0**, **Low=−1**, **Medium=N/A**, **High=N/A**.

> **M1 cap rule:** As in 2.0, apply any **floor/cap** required by the model/containment context after M1 (per Main Body/Annex B–F references).

4) **M2 — Impact dynamics reduction (energy absorption/frangibility/terminal fall)**  
   Allowed levels & effects: **None=0**, **Low=N/A**, **Medium=−1**, **High=−2**.

**Finalization:**  
`final_grc = max(1, iGRC + ΔM1A + ΔM1B + ΔM1C + ΔM2)` with M1-related caps enforced where applicable.

## Evidence & sequencing
- Keep the **intermediate states** and the evidence for each sub‑mitigation (design data, test reports, procedures).  
- Ensure the **applicability** of each sub‑mitigation per Annex B/C/F criteria before applying its reduction.
