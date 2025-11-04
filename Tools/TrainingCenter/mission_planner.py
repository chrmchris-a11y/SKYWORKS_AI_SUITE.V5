#!/usr/bin/env python3
# -*- coding: utf-8 -*-
r"""
Mission Planner Generator

Outputs a structured Mission Briefing (ERP, crew plan, operation mode, approvals)
for cleaning missions (facades, roofs, PV parks, wind turbines, stadiums, custom).

- Deterministic rules (no external API) so it works offline.
- Prints Markdown to stdout by default and can also emit JSON.

Usage (examples):
    python mission_planner.py --operation facade --time 06:00 --zone urban --airspace G --sail-target II --entries 3 --length-m 120 --format both --out ./Docs/Compliance/Missions/Facade_0600.md

"""
from __future__ import annotations
import argparse
import json
from dataclasses import dataclass, asdict
from typing import List, Dict, Any


@dataclass
class CrewRole:
    role: str
    count: int
    purpose: str


def recommend_operation_mode(operation: str, zone: str, airspace: str, sail_target: str | None) -> str:
    op = operation.lower()
    z = (zone or "").lower()
    a = (airspace or "").upper()
    # Simple heuristics
    if op in ["facade", "windows"]:
        # Urban can create LOS blind corners; recommend EVLOS if facade length/complexity is high
        return "EVLOS" if z in ["urban", "stadium"] else "VLOS"
    if op in ["roof", "pv", "pvpark", "pv_park", "solar", "solarpark"]:
        return "VLOS"
    if op in ["wind", "wind_turbine", "turbine"]:
        # Rural usually VLOS/EVLOS; BVLOS only if explicitly authorised
        return "VLOS" if a == "G" else "EVLOS"
    if op in ["stadium"]:
        return "VLOS"
    # Fallback
    return "VLOS"


def estimate_crew(operation: str, entries: int, length_m: int, mode: str, zone: str) -> List[CrewRole]:
    crew: List[CrewRole] = []
    # Always one PIC
    crew.append(CrewRole("Remote Pilot in Command (PIC)", 1, "Υπεύθυνος ασφαλούς εκτέλεσης πτήσης και αποφάσεων"))

    # Observers (EVLOS needs more; long facades need corner coverage)
    observers = 0
    if mode == "EVLOS":
        observers = 2 if length_m >= 80 else 1
    else:
        observers = 1 if length_m >= 120 else 0
    if operation.lower() in ["wind", "wind_turbine", "turbine"] and observers == 0:
        observers = 1
    if observers:
        crew.append(CrewRole("Observer(s)", observers, "Κάλυψη γωνιών/τυφλών σημείων και διατήρηση LOS"))

    # Marshals (entries or perimeter gates)
    min_marshals = 2 if zone.lower() in ["urban", "stadium"] else 1
    marshals = max(min_marshals, max(0, entries))
    crew.append(CrewRole("Marshal(s)", marshals, "Έλεγχος εισόδων/πεζών και ασφαλών διαδρόμων"))

    # Safety Manager (shared across ops block)
    crew.append(CrewRole("Safety Manager", 1, "Εποπτεία κινδύνων, Go/No-Go, και ERP"))

    # Tooling/Payload Technician
    crew.append(CrewRole("Tooling/Payload Technician", 1, "Έλεγχος αντλιών/βαλβίδων, διαρροών και διαμόρφωσης"))

    # Venue Liaison for stadium/venues
    if zone.lower() == "stadium" or operation.lower() == "stadium":
        crew.append(CrewRole("Venue Liaison", 1, "Συντονισμός με φορέα/ασφάλεια εγκατάστασης"))

    return crew


def approvals_by_zone(zone: str, airspace: str) -> List[str]:
    z = (zone or "").lower()
    a = (airspace or "").upper()
    items: List[str] = []
    # Local permissions
    items.append("Άδεια ιδιοκτήτη/διαχειριστή χώρου (site owner permission)")
    if z in ["urban", "stadium"]:
        items.append("Άδεια/γνωστοποίηση Δήμου ή αρμόδιας αρχής για προσωρινά μέτρα CGA")
        items.append("Σήμανση/περίφραξη προσωρινού CGA και σχέδιο εκτροπής πεζών")
    # Airspace
    if a in ["C", "D", "E", "B"]:
        items.append("Συντονισμός με ANSP/ATC (ενδεχομένως άδεια/slot)")
    if a == "G":
        items.append("Έλεγχος NOTAM/UTM και τοπικών περιορισμών (FRZ/ΝΟΤΑΜ)")
    # Special areas
    if z in ["stadium"]:
        items.append("Συμφωνία με φορέα εγκατάστασης, σχέδιο πλήθους (crowd-free) και ωράριο εκτός εκδηλώσεων")
    return items


def build_erp(operation: str, zone: str) -> Dict[str, Any]:
    # Base ERP components
    triggers = [
        "Απώλεια C2 (RSSI/latency εκτός ορίων)",
        "Είσοδος τρίτων στο CGA",
        "Υπέρβαση ορίων ανέμου/καιρού",
        "Διαρροή ή αστοχία εξοπλισμού (αντλία/βαλβίδες)",
    ]
    actions = [
        "Άμεσο abort callout και εκτέλεση διαδικασίας επιστροφής/τερματισμού",
        "Κατεύθυνση σε προκαθορισμένη ELS εντός CGA",
        "Ενημέρωση marshals για εκκένωση διαδρόμων",
        "Καταγραφή συμβάντος και φωτογραφική τεκμηρίωση μετά το ασφαλές τέλος",
    ]
    muster = [
        "Muster point A (πλησίον εισόδου συνεργείου)",
        "Muster point B (εναλλακτικό, υπήνεμο σημείο)",
    ]
    if zone.lower() in ["urban", "stadium"]:
        actions.append("Επικοινωνία με φορέα/ασφάλεια εγκατάστασης όπου απαιτείται")
    return {
        "triggers": triggers,
        "actions": actions,
        "muster_points": muster,
        "els": "2+ προκαθορισμένα ELS εντός CGA, με αποτύπωση στο διάγραμμα χώρου",
    }


def render_markdown(payload: Dict[str, Any]) -> str:
    crew_lines = []
    for c in payload["crew_plan"]:
        crew_lines.append(f"- {c['role']}: {c['count']} — {c['purpose']}")
    approvals_lines = "\n".join([f"- {x}" for x in payload["approvals"]])
    erp = payload["erp"]
    erp_triggers = "\n".join([f"- {x}" for x in erp["triggers"]])
    erp_actions = "\n".join([f"- {x}" for x in erp["actions"]])
    erp_muster = "\n".join([f"- {x}" for x in erp["muster_points"]])

    md = f"""
# Mission Briefing — {payload['operation'].title()}

- Time window: {payload['time']}
- Zone: {payload['zone']} | Airspace: {payload['airspace']}
- Target SAIL: {payload.get('sail_target') or '-'}
- Recommended Operation Mode: {payload['operation_mode']}

## Crew plan
{chr(10).join(crew_lines)}

## Approvals / Notifications (by zone/airspace)
{approvals_lines}

## ERP (Emergency Response Plan)
### Triggers
{erp_triggers}

### Actions
{erp_actions}

### Muster points
{erp_muster}

### ELS (Emergency Landing Sites)
- {erp['els']}

## Notes
- Εισαγωγή στο `Docs/Compliance/OSO_to_Evidence_Matrix.md` για τις αντίστοιχες OSO και τεκμήρια.
- Ενημέρωσε τους ιδιοκτήτες/διαχειριστές για ώρες, πρόσβαση και προσωρινά μέτρα CGA.
""".strip()
    return md


def main():
    p = argparse.ArgumentParser(description="Mission planner generator (ERP, crew, approvals, mode)")
    p.add_argument("--operation", required=True, help="facade|roof|pv|wind_turbine|stadium|custom")
    p.add_argument("--time", default="06:00", help="Local time start (e.g., 06:00)")
    p.add_argument("--zone", default="urban", help="urban|rural|industrial|stadium|other")
    p.add_argument("--airspace", default="G", help="Airspace class (A-G)")
    p.add_argument("--sail-target", dest="sail_target", default=None, help="Target SAIL (e.g., II)")
    p.add_argument("--entries", type=int, default=2, help="Number of site entries to guard")
    p.add_argument("--length-m", type=int, default=100, help="Approx. facade/perimeter length in meters")
    p.add_argument("--format", choices=["markdown", "json", "both"], default="markdown")
    p.add_argument("--out", default=None, help="Optional output file path for Markdown")
    p.add_argument("--json-out", dest="json_out", default=None, help="Optional output file path for JSON")

    args = p.parse_args()

    mode = recommend_operation_mode(args.operation, args.zone, args.airspace, args.sail_target)
    crew_objs = estimate_crew(args.operation, args.entries, args.length_m, mode, args.zone)
    crew_plan = [asdict(c) for c in crew_objs]

    approvals = approvals_by_zone(args.zone, args.airspace)
    erp = build_erp(args.operation, args.zone)

    payload: Dict[str, Any] = {
        "operation": args.operation,
        "time": args.time,
        "zone": args.zone,
        "airspace": args.airspace,
        "sail_target": args.sail_target,
        "operation_mode": mode,
        "crew_plan": crew_plan,
        "approvals": approvals,
        "erp": erp,
    }

    # Render outputs
    if args.format in ("markdown", "both"):
        md = render_markdown(payload)
        if args.out:
            with open(args.out, "w", encoding="utf-8") as f:
                f.write(md + "\n")
        print(md)
    if args.format in ("json", "both"):
        js = json.dumps(payload, ensure_ascii=False, indent=2)
        if args.json_out:
            with open(args.json_out, "w", encoding="utf-8") as f:
                f.write(js + "\n")
        else:
            print(js)


if __name__ == "__main__":
    main()
