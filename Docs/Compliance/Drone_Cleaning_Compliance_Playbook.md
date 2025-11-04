# Drone Cleaning Compliance Playbook

Operational compliance guide for common drone cleaning services. Use with SORA/PDRA/STS and your OSO→Evidence Matrix.

Important:
- Prefer PDRA-S01 when applicable (VLOS, <25 kg, CGA, ≤120 m AGL). If not fully applicable, run full SORA and map SAIL→OSOs.
- Always confirm with your competent authority. Numbers and robustness levels come from official SORA tables (JARUS SORA v2.5 Annexes) and PDRA specs.

---

## 1) Photovoltaic Parks (VLOS, CGA)
- Regulatory path: PDRA-S01 likely applicable (VLOS, CGA, <25 kg). Else SORA (often SAIL II–III).
- Key mitigations:
  - CGA fencing/signage; marshals; controlled entry.
  - Geofencing aligned to park boundary; termination to safe corridor.
  - C2 reliability test (RSSI/latency) and GNSS checks; alternate routing if weak signal.
  - Tooling dry-run; pressure/leak checks; non-conductive booms.
- Evidence examples:
  - Geofence validation report; FT demo log; site photos.
  - C2 test sheets; maintenance and tooling logs.
  - Briefings, checklists, incident/safety reports.
- Checklist (condensed): preflight CGA, geofence/FT test, C2/GNSS check, tooling dry-run, weather/NOTAM/EMI, briefing & Go/No-Go, postflight logs.

## 2) Building Facades (VLOS, urban, 06:00)
- Regulatory path: PDRA-S01 if CGA is strictly enforced; else SORA (target SAIL II by reducing GRC via CGA and off-peak timing).
- Key mitigations:
  - CGA on sidewalks/forecourts; early-morning ops to minimize third-party exposure.
  - Virtual walls & ceiling; termination lane into safe zone.
  - Marshals; abort triggers on crowd ingress/low RSSI.
  - Tooling stand-off; dry-run; shielding near electrical fixtures.
- Evidence: photos of CGA, geofence/FT validation, C2 tests, marshal roster, briefings, checklists.
- Checklist: preflight pedestrian-flow sampling; CGA setup; geofence/FT; T/O&L inside CGA; comms signals; postflight inspection & logs.

## 3) Roof Cleaning (VLOS, near edges)
- Regulatory path: PDRA-S01 if CGA is below/around building; else SORA.
- Hazards: edge vortices, downwash near parapets, GNSS multipath.
- Key mitigations: roof-edge buffer, geo-ceiling, termination to rooftop safe pad, team on roof for segregation, tooling fail-safe.
- Evidence: roof plan with buffers; FT test on rooftop; maintenance/tooling logs.

## 4) Solar Panel Cleaning (rooftop or ground arrays)
- If inside fenced park → like #1.
- Rooftop arrays: combine #3 with electric safety near junction boxes; strict stand-off margins; IP-rated hoses/valves.

## 5) Windows Cleaning (high-rises, VLOS)
- Urban pedestrian management critical. CGA and marshals mandatory.
- Virtual walls close to facade; termination to secure inner courtyard or protected lane.
- Tooling: leak detection, auto shutoff; dry-run each shift.

## 6) Wind Turbine (Anemogenitries) Cleaning/Inspection
- Regulatory path: SORA typically; local NOTAMs; coordination with wind farm operator.
- Hazards: blade movement, tower-induced turbulence, EMI from nacelle equipment.
- Key mitigations: lockout/tagout of turbine (where possible), geo-cylinder around tower, termination to base-pad, line-of-sight positioning for stable C2, lightning risk check.
- Evidence: operator authorization; lockout documentation; site-specific checklists; C2/FT tests.

## 7) Stadium Cleaning (large venues)
- Regulatory path: SORA; events drive high GRC — schedule strictly off-events/off-peak.
- CGA over seating and corridors; multiple marshals; multi-exit egress plan.
- Geofence per sector; termination to inner service area; radio comms plan.
- Evidence: venue layout with CGA; staff roster; geofence/FT logs; comms tests.

---

## OSO Implementation Hints (generic for SAIL II–III)
- Organizational: Operations Manual (normal/emergency), roles & responsibilities, training & recurrent checks, safety reporting, change/configuration control.
- Operational: risk assessment, CGA setup, pre/post checklists, team briefing, Go/No-Go criteria, comms plan, incident logs.
- Technical: geofencing/containment, termination function, C2 reliability tests, maintenance/tooling records, navigation integrity checks.

---

## Documentation Pointers
- Use `Docs/Compliance/OSO_to_Evidence_Matrix.md` to map each required OSO to your implementation and attach evidence links.
- Keep photos, logs, test reports, and briefings versioned. Upload samples under `Docs/` or `Logs/` and reference them in the matrix.
