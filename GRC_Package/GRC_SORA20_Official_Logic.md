# GRC — SORA 2.0 (EASA AMC/GM) — Official Logic Summary
Generated: 2025-10-30T09:42:25.045761Z

## Step #2 — Intrinsic/Initial GRC (iGRC)
- The **iGRC** is determined using the **SORA 2.0 Step 2 tables** (EASA AMC/GM; iGRC mapping).
- Core drivers (per official tables): **population density of the operational area**, **operational scenario footprint/containment**, and reference assumptions on kinetic severity per scenario.
- Output: **iGRC ∈ {1,2,3,4,5,6,7,8}** (integer class).

> Implementation note: treat `iGRC` as the **initial_grc** value in your API.

## Step #3 — Final GRC via Strategic & Technical Mitigations
Apply mitigations **in order** and then clamp to minimum 1:

1) **M1 — Strategic mitigations** (population exposure management / boundary & chronology / segregation / geo-awareness)  
   **Reductions (official 2.0 Table)**:  
   - None → **0**  
   - Low → **−1**  
   - Medium → **−2**  
   - High → **−4**  
   **M1 floor rule (cap):** M1 **cannot** reduce below the column/footprint floor defined for the iGRC table/containment context. Keep a floor *after* M1.

2) **M2 — Effects of impact** (energy absorption, frangibility, terminal fall dynamics, etc.)  
   **Reductions (official 2.0 Table)**:  
   - None → **0**  
   - Low → **−1**  
   - High → **−2**

3) **M3 — Emergency Response Plan (validated & effective)**  
   **Adjustments (official 2.0 rule):**  
   - **None/Low → +1** (penalty)  
   - **Medium → 0**  
   - **High → −1**

**Finalization:**  
`final_grc = max(1, iGRC + ΔM1 + ΔM2 + ΔM3)` with **M1 floor cap applied right after M1** before proceeding to M2/M3.

## Logging (for audits)
- Record evidence for each mitigation (design data, procedures, ERP validation).  
- Keep the intermediate states: **iGRC → after M1 (cap applied) → after M2 → after M3 → final clamp**.

