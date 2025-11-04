#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Off-peak Optimizer — suggests time windows to minimize third-party exposure (GRC reduction).

Heuristic only (offline). Outputs Markdown recommendation by scenario and zone.

Usage:
  python off_peak_optimizer.py --scenario Facade --zone urban --day weekday
"""
from __future__ import annotations
import argparse
from datetime import datetime
import sys

# Ensure UTF-8 stdout on Windows terminals
try:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
except Exception:
    pass


def recommend_windows(scenario: str | None, zone: str | None, day: str | None) -> str:
    sc = (scenario or 'Generic').title()
    zn = (zone or 'urban').lower()
    dy = (day or 'weekday').lower()

    base = [
        ("05:30–07:00", "Κρίσιμη μείωση ροής πεζών/οχημάτων, ιδανικό για CGA setup και kiểmωση"),
        ("12:30–13:30", "Υψηλή διακύμανση (lunch break) — αξιολόγησε τοπικά πριν επιλέξεις"),
        ("16:00–17:00", "Συνήθως αυξημένη κίνηση — απόφυγε αν γίνεται"),
    ]
    if zn in ['urban','stadium']:
        base.insert(0, ("05:00–06:00", "Ελάχιστη παρουσία κοινού, κατάλληλο για προκαταρκτικούς ελέγχους"))
    if 'stadium' in sc.lower():
        base = [
            ("05:00–07:00", "Πριν οποιοδήποτε event, με έλεγχο crowd-free από venue security"),
            ("22:30–00:00", "Μετά την εκκένωση χώρου, μόνο εφόσον το venue είναι πλήρως κενό"),
        ]
    if 'pv' in sc.lower() or 'solar' in sc.lower():
        base = [
            ("06:00–08:00", "Χαμηλές θερμοκρασίες/άνεμος, μηδενική παρουσία κοινού εντός CGA"),
            ("18:00–19:00", "Εναλλακτικό παράθυρο με ήπιο άνεμο και χαμηλή δραστηριότητα"),
        ]

    lines = [f"# Off-peak Windows — {sc}\n\nGenerated: {datetime.now().strftime('%Y-%m-%d %H:%M')}\n"]
    for (win, why) in base:
        lines.append(f"- {win}: {why}")
    lines.append("\n> Tip: Τεκμηρίωσε off-peak επιλογή με δειγματοληψία ροής πεζών (3 σημεία χρόνου) και ενημέρωσε OSO evidence.")
    return "\n".join(lines)


def main():
    ap = argparse.ArgumentParser(description='Off-peak optimizer')
    ap.add_argument('--scenario', default='Generic')
    ap.add_argument('--zone', default='urban')
    ap.add_argument('--day', choices=['weekday','weekend'], default='weekday')
    args = ap.parse_args()

    print(recommend_windows(args.scenario, args.zone, args.day))


if __name__ == '__main__':
    main()
