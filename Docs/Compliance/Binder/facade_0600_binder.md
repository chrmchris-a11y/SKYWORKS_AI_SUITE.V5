# Compliance Binder — Facade Cleaning (06:00)

Generated: 2025-10-22 00:29

---

## Mission Briefing
# Mission Briefing — Facade

- Time window: 06:00
- Zone: urban | Airspace: G
- Target SAIL: II
- Recommended Operation Mode: EVLOS

## Crew plan
- Remote Pilot in Command (PIC): 1 — Υπεύθυνος ασφαλούς εκτέλεσης πτήσης και αποφάσεων
- Observer(s): 2 — Κάλυψη γωνιών/τυφλών σημείων και διατήρηση LOS
- Marshal(s): 3 — Έλεγχος εισόδων/πεζών και ασφαλών διαδρόμων
- Safety Manager: 1 — Εποπτεία κινδύνων, Go/No-Go, και ERP
- Tooling/Payload Technician: 1 — Έλεγχος αντλιών/βαλβίδων, διαρροών και διαμόρφωσης

## Approvals / Notifications (by zone/airspace)
- Άδεια ιδιοκτήτη/διαχειριστή χώρου (site owner permission)
- Άδεια/γνωστοποίηση Δήμου ή αρμόδιας αρχής για προσωρινά μέτρα CGA
- Σήμανση/περίφραξη προσωρινού CGA και σχέδιο εκτροπής πεζών
- Έλεγχος NOTAM/UTM και τοπικών περιορισμών (FRZ/ΝΟΤΑΜ)

## ERP (Emergency Response Plan)
### Triggers
- Απώλεια C2 (RSSI/latency εκτός ορίων)
- Είσοδος τρίτων στο CGA
- Υπέρβαση ορίων ανέμου/καιρού
- Διαρροή ή αστοχία εξοπλισμού (αντλία/βαλβίδες)

### Actions
- Άμεσο abort callout και εκτέλεση διαδικασίας επιστροφής/τερματισμού
- Κατεύθυνση σε προκαθορισμένη ELS εντός CGA
- Ενημέρωση marshals για εκκένωση διαδρόμων
- Καταγραφή συμβάντος και φωτογραφική τεκμηρίωση μετά το ασφαλές τέλος
- Επικοινωνία με φορέα/ασφάλεια εγκατάστασης όπου απαιτείται

### Muster points
- Muster point A (πλησίον εισόδου συνεργείου)
- Muster point B (εναλλακτικό, υπήνεμο σημείο)

### ELS (Emergency Landing Sites)
- 2+ προκαθορισμένα ELS εντός CGA, με αποτύπωση στο διάγραμμα χώρου

## Notes
- Εισαγωγή στο `Docs/Compliance/OSO_to_Evidence_Matrix.md` για τις αντίστοιχες OSO και τεκμήρια.
- Ενημέρωσε τους ιδιοκτήτες/διαχειριστές για ώρες, πρόσβαση και προσωρινά μέτρα CGA.

---

## Approvals (from mission)
- Άδεια ιδιοκτήτη/διαχειριστή χώρου (site owner permission)
- Άδεια/γνωστοποίηση Δήμου ή αρμόδιας αρχής για προσωρινά μέτρα CGA
- Σήμανση/περίφραξη προσωρινού CGA και σχέδιο εκτροπής πεζών
- Έλεγχος NOTAM/UTM και τοπικών περιορισμών (FRZ/ΝΟΤΑΜ)

---

## OSO Matrix — Scenario Excerpt
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
