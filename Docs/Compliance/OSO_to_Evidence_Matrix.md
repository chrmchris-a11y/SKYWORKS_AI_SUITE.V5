# OSO → Evidence Matrix (Template)

Use this matrix to map required OSOs (from SORA Annex tables or PDRA) to your concrete implementation and evidence.
Adjust/expand rows as needed.

Legend:
- Required Robustness: L = Low, M = Medium, H = High
- Status: Ready / Gap / In progress

---

## Context
- Operation: VLOS solar panel cleaning inside fenced photovoltaic park (CGA)
- UAS: MTOM 24.5 kg
- Path: Prefer PDRA-S01 if fully applicable; else SORA (often SAIL III)

---

| OSO (Code/Title) | Required Robustness | Implementation — Technical | Implementation — Operational | Implementation — Organizational | Evidence (Docs/Tests/Records) | Status | Owner | Last Update |
|---|:--:|---|---|---|---|:--:|---|---|
| Geofencing / Containment aligned to CGA | M | Geo-fence limits set to park boundary; termination function validated; stand-off margins | Pre-flight CGA inspection; marshals; no-entry signage; contingency plan | OM sections for CGA control; roles/responsibilities defined | Geofence validation report; FT demo log; site photos; OM references | Gap | Ops Lead | YYYY-MM-DD |
| Reliable C2 Link (range/latency/interference) | M | Link quality check; antenna orientation; EMI scan pattern; RTH configured | Pre-flight C2 test and periodic in-flight checks; alternative route if RSSI low | Training on C2 anomalies; comms plan; reporting process | C2 test log (RSSI/latency); briefing records; incident log template | In progress | PIC | YYYY-MM-DD |
| Emergency / Flight Termination procedures | M | Termination tested at low altitude; safe landing area defined | Briefed triggers (loss of C2, leak); abort callout; recovery steps | OM emergency procedures; recurrent drills scheduled | FT test report; drill checklist; debrief notes | Ready | Safety Mgr | YYYY-MM-DD |
| Tooling fail-safe (pump/valves/boom) | M | Auto shut-off on pressure drop; non-conductive boom; shielding near junction boxes | Dry-run before each shift; stand-off distance maintained | Maintenance schedule; configuration control for tooling | Pump test sheet; maintenance logs; dry-run checklist | Gap | Maintenance | YYYY-MM-DD |
| Crew competence & recurrent training | M | — | SOPs briefed; role-specific checklists; Go/No-Go | Training records; currency tracking; change management | Training matrix; checkride records; OM references | In progress | Training | YYYY-MM-DD |
| Pre/Post-flight procedures & logs | M | — | Standardized PF/PL checklists; NOTAM/weather check; GNSS/C2 check | Document control; audit trail | Signed checklists; weather/NOTAM evidence; PF/PL logs | Ready | Ops Lead | YYYY-MM-DD |
| Strategic/Tactical mitigations (buffers, segregation) | M | Buffers along panel rows; safe taxi corridors; obstacles database | Area cleared before ops; marshal positions; emergency egress paths | Risk assessment template; periodic review | Site diagram; marshal roster; risk register | In progress | Ops Lead | YYYY-MM-DD |

---

## Notes
- Replace OSO names/codes with the official wording from the current SORA annex table or PDRA-S01 list.
- Set robustness exactly as required by your competent authority.
- Attach hyperlinks to evidence where possible (e.g., `Docs/…`, `Logs/…`).
- Keep Owner and dates up to date to support audits.
- Reference: Για GRC=3 και ARC=b, ο πίνακας SORA (Table 7 – SAIL determination) δίνει SAIL III (μετά τον υπολογισμό Final GRC και Residual ARC).

---

## Quick Checklist (copy/paste)
- [ ] CGA boundaries verified and fenced/signed
- [ ] Geofence/termination configured and validated
- [ ] C2 link tested (RSSI/latency); RTH verified
- [ ] Dry-run tooling; no leaks; stand-off margins defined
- [ ] Briefing done; roles/Go–No-Go criteria documented
- [ ] Weather/NOTAM/EMI risks checked
- [ ] Emergency/contingency triggers briefed and exercised
- [ ] Post-flight inspection and logs completed

---

## Additional Example Rows (Facades / Roofs / Windows / Wind Turbines / Stadium)

| OSO (Code/Title) | Required Robustness | Implementation — Technical | Implementation — Operational | Implementation — Organizational | Evidence (Docs/Tests/Records) | Status | Owner | Last Update |
|---|:--:|---|---|---|---|:--:|---|---|
| Facade ops: Virtual walls & geo-ceiling | M | Virtual walls near facade; ceiling to prevent climb-out; termination to inner safe lane | Early-morning (off-peak) schedule; marshals on sidewalks; abort triggers on ingress | OM: Urban CGA SOP; staff briefing templates | Geofence/FT validation; photos of CGA; marshal roster | In progress | PIC | YYYY-MM-DD |
| Roof ops: Edge buffer & rooftop termination | M | Geo-ceiling; buffer from parapets; termination pad on roof | Roof access control; spotter on roof; weather/gust limits | OM: Roof operations addendum; toolbox talk | Rooftop FT test log; roof plan with buffers; checklists | Gap | Ops Lead | YYYY-MM-DD |
| Windows cleaning: Tooling fail-safe & leak control | M | Auto shutoff on pressure drop; non-conductive boom; IP-rated valves | Dry-run each shift; defined stand-off distances; spill response | Maintenance & configuration control; training records | Pump test sheets; dry-run logs; incident template | In progress | Maintenance | YYYY-MM-DD |
| Wind turbine: Lockout/Tagout & geo-cylinder | M | Turbine lockout (where possible); geo-cylinder around tower; termination to base-pad | Operator coordination; radio comms; lightning risk procedure | Site-specific procedure; authorization letters | Lockout documentation; comms test log; FT/geofence report | Gap | Safety Mgr | YYYY-MM-DD |
| Stadium: Sector geofencing & multi-marshal plan | M | Sector-based geofence; termination to service area | Off-event scheduling; multi-exit egress plan; marshal network | Venue liaison SOP; crowd management policy | Venue map; marshal matrix; comms test; FT demos | In progress | Ops Lead | YYYY-MM-DD |

---

## Scenario Pack: Facade Cleaning — Urban 06:00 (Target SAIL II via GRC reduction)

Use these rows to document concrete mitigations for early‑morning facade cleaning in urban area with VLOS and CGA.

| OSO (Code/Title) | Required Robustness | Implementation — Technical | Implementation — Operational | Implementation — Organizational | Evidence (Docs/Tests/Records) | Status | Owner | Last Update |
|---|:--:|---|---|---|---|:--:|---|---|
| CGA enforcement on sidewalks/forecourt | M | Temporary barriers, cone lines; geo-fence matching CGA | Marshals at entries; pedestrian diversion plan; signage | OM Urban-CGA SOP; marshal training brief | Photos of CGA; marshal roster; incident log template; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | In progress | chrmc | 2025-10-21 |
| Early-morning off-peak scheduling | M | — | Ops at 06:00; pre-op pedestrian-flow sampling (3 timepoints) | Policy for off-peak planning; records retention | Video samples; count sheets; decision log; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | In progress | chrmc | 2025-10-21 |
| Virtual walls & ceiling near facade | M | Vertical/horizontal geofence; termination to inner safe lane | Pre-flight geofence/FT validation; strict stand-off | Change/config control; test scheduling | Geofence screenshots; FT test report with photos; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | Gap | chrmc | 2025-10-21 |
| C2 reliability & EMI handling | M | RSSI/latency baseline; antenna orientation; alternate route | Abort triggers on low RSSI; comms protocol with marshals | Training on C2 anomalies; comms plan | C2 test sheets; comms test log; abort drill record; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | In progress | chrmc | 2025-10-21 |
| Tooling leak/fail-safe & spill response | M | Auto shutoff on pressure drop; non-conductive boom; IP valves | Dry-run per shift; spill kit on-site; stand-off enforcement | Maintenance & tooling SOP; recurrent checks | Pump test sheet; dry-run checklist; spill report form; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | In progress | chrmc | 2025-10-21 |
| Ground risk buffers & approach corridors | M | Geo-buffers along approach; termination into inner safe lane (no public) | Defined approach/taxi paths; marshal positions at entries | Site plan with corridors; marshal SOP | Marked site map; marshal matrix; FT path validation video; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | In progress | chrmc | 2025-10-21 |
| LOS assurance incl. corner coverage | M | Camera + helper spotter coverage at facade corners | PIC + observer placement; handover protocol on blind corners | Training on corner coverage; role cards | Observer checklist; corner handover log; briefing record; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | Ready | chrmc | 2025-10-21 |
| Weather/gust & downdraft thresholds | M | Wind limits per facade canyoning; rain/light threshold for optics | Ops halt if gust > X kt or variable > Y kt; micro‑met checks pre‑start | Weather policy addendum; escalation chain | Weather log; anemometer snapshot; Go/No‑Go record; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | In progress | chrmc | 2025-10-21 |
| Public notification & permit display | L | — | Temporary signage; permit displayed at site; venue contact posted | Public info policy; permit control | Permit copy; signage photos; liaison email; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | Ready | chrmc | 2025-10-21 |
| Emergency landing site (ELS) mapping | M | Pre‑marked ELS pads within CGA; RTH path sanity check | ELS brief at start; clear access route; equipment staged | Emergency response plan; periodic drills | ELS map; drill checklist; photos of pads; Mission briefing: `Docs/Compliance/Missions/Facade_0600.md` | Gap | chrmc | 2025-10-21 |

---

## Scenario Pack: Stadium Cleaning — 05:30 (Target SAIL II via GRC reduction)

Pre‑event, dawn operations to ensure no spectators are present. Coordinated with venue and security; strong CGA, geofencing by sectors, and multi‑marshal communications.

| OSO (Code/Title) | Required Robustness | Implementation — Technical | Implementation — Operational | Implementation — Organizational | Evidence (Docs/Tests/Records) | Status | Owner | Last Update |
|---|:--:|---|---|---|---|:--:|---|---|
| Venue liaison & authorization | M | — | Daily pre‑op check‑in with venue security; access control confirmed | Venue liaison SOP; approval matrix | Signed permit; liaison email; security contact list; Mission briefing: `Docs/Compliance/Missions/Stadium_0530.md` | Ready | chrmc | 2025-10-21 |
| Sector geofencing & no‑go stands | M | Sector polygons aligned to stands/concourses; hard no‑go zones | Marshal posts at sector edges; radio callouts on sector transitions | Stadium ops addendum; sector map control | Geofence screenshots; marshal roster; radio script; Mission briefing: `Docs/Compliance/Missions/Stadium_0530.md` | In progress | chrmc | 2025-10-21 |
| Crowd sweep & barriers (zero public) | M | — | Sweep conducted at 05:15; barriers at all public entries; locks verified | Crowd‑free policy; checklist retention | Sweep checklist; entry photos; lock verification log; Mission briefing: `Docs/Compliance/Missions/Stadium_0530.md` | Ready | chrmc | 2025-10-21 |
| Lighting & obstacle marking at dawn | L | Strobes/aux lights; obstacle markers on cables/booms | Headlamps for crew; light check before taxi; low‑sun glare mitigation | Lighting checklist; equipment maintenance | Pre‑op light check log; photos of markers; maintenance records; Mission briefing: `Docs/Compliance/Missions/Stadium_0530.md` | In progress | chrmc | 2025-10-21 |
| RF/EMI coordination & comms check | M | Antenna orientation; venue RF scan if available | Radio check with all marshals; backup channel agreed | Comms plan; jamming awareness brief | Comms test log; EMI scan report (if any); fallback plan; Mission briefing: `Docs/Compliance/Missions/Stadium_0530.md` | In progress | chrmc | 2025-10-21 |
| Emergency egress & muster points | M | Termination to service bay; taxi corridors clear | Egress routes briefed; muster points assigned; roles defined | Emergency response plan; drill cadence | Egress map; drill debrief; attendance sheet; Mission briefing: `Docs/Compliance/Missions/Stadium_0530.md` | Gap | chrmc | 2025-10-21 |
