#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Real-time Risk Guard — quick assessment of live risks.

Inputs (optional): --rssi (dBm), --wind (kt), --gust (kt), --flow (none|low|medium|high)
Outputs a Markdown status with GO/CAUTION/NO-GO hints and OSO references.

Usage:
  python risk_guard.py --rssi -55 --wind 8 --gust 14 --flow low
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


def assess(rssi: float | None, wind: float | None, gust: float | None, flow: str | None) -> str:
    now = datetime.now().strftime('%Y-%m-%d %H:%M')
    lines = [f"# Real-time Risk Guard\n\nTimestamp: {now}\n"]

    # C2
    if rssi is None:
        lines.append("- C2: N/A (δώσε --rssi)")
    else:
        if rssi >= -60:
            lines.append(f"- C2: GO (RSSI={rssi} dBm)")
        elif rssi >= -70:
            lines.append(f"- C2: CAUTION (RSSI={rssi} dBm) — βελτίωσε προσανατολισμό κεραιών/απόσταση")
        else:
            lines.append(f"- C2: NO-GO (RSSI={rssi} dBm) — επανεκτίμηση θέσης/κεραιών/παρεμβολών")

    # Wind
    if wind is None:
        lines.append("- Wind: N/A (δώσε --wind)")
    else:
        g = gust or wind
        if wind <= 10 and g <= 16:
            lines.append(f"- Wind: GO ({wind} kt, gust {g} kt)")
        elif wind <= 14 and g <= 22:
            lines.append(f"- Wind: CAUTION ({wind} kt, gust {g} kt) — περιόρισε περίμετρο/ύψος, αύξησε ELS ετοιμότητα")
        else:
            lines.append(f"- Wind: NO-GO ({wind} kt, gust {g} kt) — αναμονή/μετατόπιση σε άλλο παράθυρο")

    # Pedestrian flow
    flow_s = (flow or 'unknown').lower()
    if flow_s in ['none','low']:
        lines.append(f"- Flow: GO ({flow_s})")
    elif flow_s in ['medium']:
        lines.append(f"- Flow: CAUTION ({flow_s}) — ενίσχυσε marshals/CGA και ενημέρωσε signage")
    elif flow_s in ['high']:
        lines.append(f"- Flow: NO-GO ({flow_s}) — μετέφερε την ώρα σε off-peak και δυνάμωσε CGA")
    else:
        lines.append("- Flow: N/A — αξιολόγησε τοπικά και κατέγραψε δείγματα (3 timepoints)")

    lines.append("\n## OSO αναφορές\n- C2 reliability (M)\n- Operational planning (off-peak) (M)\n- CGA containment (M)\n- ELS mapping (M)")
    return "\n".join(lines)


def main():
    ap = argparse.ArgumentParser(description='Real-time Risk Guard')
    ap.add_argument('--rssi', type=float, default=None)
    ap.add_argument('--wind', type=float, default=None)
    ap.add_argument('--gust', type=float, default=None)
    ap.add_argument('--flow', default=None)
    args = ap.parse_args()

    print(assess(args.rssi, args.wind, args.gust, args.flow))


if __name__ == '__main__':
    main()
