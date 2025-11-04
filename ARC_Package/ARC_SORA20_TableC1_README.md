# ARC — SORA 2.0 (EASA AMC/GM) — Table C.1 Overview
Generated: 2025-10-30T09:38:15.591657Z

## Purpose
Determine **initial ARC (iARC: a–d)** at **SORA Step 4** using the EASA AMC/GM to Part‑UAS (ED Decision 2023/012/R).

## Inputs expected
- **Airspace Class / Zone**: Class G/E/D/C, CTR, TMA, TIZ, ATZ, RMZ, TMZ
- **Air Traffic Density rating**: Very Low (1) .. Very High (5)
- **Operational Altitude (AGL, m)**
- **Proximity to aerodromes/heliports**: Inside CTR/TMA / Near / Outside
- **Operation Type**: VLOS / EVLOS / BVLOS
- **Operational Volume defined (geofence)** and **Adjacent Airspace characterised** (true/false)

## Outcome
- **iARC ∈ {a,b,c,d}**

## Notes (implementation)
- Use the EASA AMC/GM **Table C.1** logic: blend airspace context, traffic density and proximity to aerodromes to select iARC.
- Keep a trace: source of density data, dates/times used, altitude band.
