#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
What-if Engine — Suggest mitigations to reduce SAIL.

Offline heuristics based on typical SORA practices for cleaning operations.
Generates a Markdown report with OSO linkages and practical actions.

Usage:
  python what_if_engine.py --current III --target II --scenario Facade
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


def suggest_mitigations(current: str, target: str, scenario: str | None) -> str:
    cur = (current or '').upper()
    tgt = (target or '').upper()
    scn = (scenario or 'Generic').title()

    header = f"# What-if: Reduce SAIL {cur} → {tgt} ({scn})\n\nGenerated: {datetime.now().strftime('%Y-%m-%d %H:%M')}\n\n"

    if cur == tgt:
        return header + "- Ο στόχος είναι ήδη επιτευγμένος. Εστίασε στην τεκμηρίωση robustness (OSO, evidence).\n"

    lines = []
    # Generic levers: lower GRC via CGA and timing; lower ARC via airspace/strategic mitigations
    lines.append("## Κύριοι Μοχλοί Μείωσης\n")
    lines.append("- Ενίσχυση CGA (περίφραξη, σήμανση, marshals, προ-έλεγχος εισόδων) — OSO: Containment/CGA\n")
    lines.append("- Γεωφράξεις/τερματισμός (FT) με τεστ πριν την έναρξη — OSO: Geofencing/FT\n")
    lines.append("- Off-peak χρονοπαράθυρα (π.χ., 05:30–07:00) — OSO: Operational planning\n")
    lines.append("- Buffer ζωνών/διάδρομοι taxi και προκαθορισμένα ELS — OSO: Ground risk buffers / ELS\n")
    lines.append("- Διασφάλιση LOS (EVLOS όπου χρειάζεται) — OSO: LOS assurance\n")
    lines.append("- Επάρκεια πληρώματος (παρατηρητές, marshals, safety) — OSO: Crew competence\n")
    lines.append("- C2/EMI αξιοπιστία (RSSI/latency baseline, προσανατολισμός κεραιών) — OSO: C2 reliability\n\n")

    # Scenario-specific hints
    lines.append("## Στοχευμένες ενέργειες\n")
    sc = scn.lower()
    if 'facade' in sc or 'windows' in sc:
        lines.extend([
            "- Εφάρμοσε κάθετες/οριζόντιες εικονικές επιφάνειες (virtual walls/ceiling)",
            "- Αύξησε marshals σε εισόδους/πεζοδρόμια, όρισε μονοδρόμηση και ασφαλείς διαδρόμους"
        ])
    elif 'stadium' in sc:
        lines.extend([
            "- Τομεοποίηση γεωφράξεων (sector geofencing) και transitions με radio callouts",
            "- Δέσμευση χώρου πριν τις ώρες κοινού, έλεγχος μηδενικού πλήθους πριν την έναρξη"
        ])
    elif 'pv' in sc or 'solar' in sc:
        lines.extend([
            "- Γεωφράξεις περιμέτρου και termination εντός service lane",
            "- Έλεγχος θυρών/πυλών και MARSHAL σε σημεία πρόσβασης"
        ])
    elif 'roof' in sc:
        lines.extend([
            "- Γεω-οροφή (ceiling) και buffer από ακμές/στηθαία",
            "- Παρατηρητής στην οροφή και ELS πάνω στο δώμα"
        ])
    elif 'wind' in sc:
        lines.extend([
            "- Lockout/Tagout όπου γίνεται και geo-cylinder γύρω από τον πύργο",
            "- Εξειδικευμένη ενημέρωση για κεραυνούς/EMI και δοκιμή C2"
        ])
    else:
        lines.append("- Εφάρμοσε τα γενικά μέτρα CGA/LOS/ELS/crew/EMI και απόδειξε robustness με evidence\n")

    # Quick OSO alignment
    lines.append("\n## OSO ευθυγράμμιση\n")
    lines.append("- Containment/CGA → Required Robustness: M (τεκμηρίωση με φωτογραφίες/σχέδια)\n")
    lines.append("- Geofencing/FT → M (screen captures + FT report)\n")
    lines.append("- LOS assurance → M (σχέδιο κάλυψης γωνιών/handovers)\n")
    lines.append("- C2 reliability → M (RSSI/latency sheets)\n")
    lines.append("- Crew competence → M (training matrix, role cards)\n")
    lines.append("- ELS mapping → M (χάρτης ELS, διαδρομές εκκένωσης)\n\n")

    lines.append("> Tip: Ανέβασε αυτά τα artifacts στο `Docs/Compliance` και τρέξε `/evidence-sync` για αυτόματη ενημέρωση OSO matrix.\n")

    return header + "\n".join(lines)


def main():
    ap = argparse.ArgumentParser(description='What-if engine for SAIL reduction suggestions')
    ap.add_argument('--current', required=True, help='Current SAIL (e.g., III)')
    ap.add_argument('--target', required=True, help='Target SAIL (e.g., II)')
    ap.add_argument('--scenario', default='Generic', help='Facade|Roof|PV|Stadium|Wind|...')
    args = ap.parse_args()

    md = suggest_mitigations(args.current, args.target, args.scenario)
    print(md)


if __name__ == '__main__':
    main()
