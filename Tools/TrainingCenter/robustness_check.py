#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
OSO Robustness Check (report-only)

Parses OSO_to_Evidence_Matrix.md scenario tables and reports potential
mismatches or missing robustness against a target SAIL.

Note: Without full Annex E mapping, we implement checks:
- Missing robustness cell
- Non-standard values (not in {L,M,H})
- Optional hint mapping (heuristic) per OSO keyword vs SAIL (II/III -> expect M for core OSOs)
"""
from __future__ import annotations
import argparse
import os
import re

MATRIX_PATH = os.path.join('Docs', 'Compliance', 'OSO_to_Evidence_Matrix.md')

CORE_OSO_HINTS = [
    # (keyword regex, expected robustness by SAIL)
    (re.compile(r'geofenc|contain|virtual|ceiling', re.I), {'II': 'M', 'III': 'M'}),
    (re.compile(r'c2|rssi|latency|comms|emi', re.I), {'II': 'M', 'III': 'M'}),
    (re.compile(r'termination|emergency', re.I), {'II': 'M', 'III': 'M'}),
    (re.compile(r'tooling|leak|pump|valv', re.I), {'II': 'M', 'III': 'M'}),
]

def scan_matrix(target_sail: str | None, scenario_filter: str | None):
    if not os.path.exists(MATRIX_PATH):
        return '❌ Matrix not found.'
    with open(MATRIX_PATH, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    report = []
    in_table = False
    header = ''
    for i, line in enumerate(lines):
        if line.startswith('## Scenario Pack:'):
            header = line.strip()
            in_table = False
            continue
        if scenario_filter and header and scenario_filter.lower() not in header.lower():
            continue
        if re.match(r'^\|\s*OSO', line):
            in_table = True
            continue
        if in_table:
            if not line.strip() or line.startswith('---') or line.startswith('## '):
                in_table = False
                continue
            if not line.startswith('|') or '|---' in line:
                continue
            parts = line.split('|')
            if len(parts) < 11:
                continue
            title = parts[1].strip()
            robust = parts[2].strip()
            # Check robustness value
            if robust not in ('L','M','H'):
                report.append(f'⚠️ {header}: "{title}" has non-standard or missing robustness: "{robust}"')
                continue
            # Heuristic hints
            if target_sail:
                t = target_sail.upper().replace('SAIL','').strip()
                for rx, mapping in CORE_OSO_HINTS:
                    if rx.search(title):
                        exp = mapping.get(t)
                        if exp and robust != exp:
                            report.append(f'ℹ️ {header}: "{title}" robustness is {robust}; heuristic expects {exp} for SAIL {t}.')
                        break
    return '\n'.join(report) if report else '✅ No issues detected (heuristic).'


def main():
    ap = argparse.ArgumentParser(description='OSO Robustness Check')
    ap.add_argument('--sail', default=None, help='Target SAIL (e.g., II, III)')
    ap.add_argument('--scenario', default=None, help='Optional scenario filter substring')
    args = ap.parse_args()
    print(scan_matrix(args.sail, args.scenario))

if __name__ == '__main__':
    main()
