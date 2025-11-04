#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Evidence Sync Bot

Scans evidence folders (Docs/Compliance, Logs/*) and updates OSO_to_Evidence_Matrix.md
by appending found artifacts to the Evidence column for the matching Scenario Pack.

Heuristics only (no external APIs). Keeps changes minimal and idempotent.
"""
from __future__ import annotations
import argparse
import os
import re
from datetime import date

MATRIX_PATH = os.path.join('Docs', 'Compliance', 'OSO_to_Evidence_Matrix.md')

KEY_MAP = [
    (re.compile(r'FT\s*test|flight\s*termination', re.I), 'Emergency / Flight Termination'),
    (re.compile(r'geofence|virtual\s*walls?|geo-?ceiling', re.I), 'Geofencing / Containment'),
    (re.compile(r'c2|rssi|latency|comms|emi', re.I), 'C2 reliability & EMI'),
    (re.compile(r'pump|valv|leak|tooling', re.I), 'Tooling leak/fail-safe'),
    (re.compile(r'permit|liaison|authorization', re.I), 'Venue liaison & authorization'),
    (re.compile(r'els|muster|egress', re.I), 'ELS mapping & egress'),
]

SCENARIO_HEADERS = [
    '## Scenario Pack: Facade Cleaning',
    '## Scenario Pack: Stadium Cleaning',
    '## Scenario Pack: Roof Cleaning',
    '## Scenario Pack: PV Park Cleaning',
    '## Scenario Pack: Wind Turbine Cleaning',
    '## Scenario Pack: Windows Cleaning',
]

def find_files(root_paths):
    found = []
    for root in root_paths:
        if not os.path.exists(root):
            continue
        for dirpath, _, filenames in os.walk(root):
            for f in filenames:
                if f.lower().endswith(('.md', '.pdf', '.png', '.jpg', '.jpeg', '.txt', '.log')):
                    rel = os.path.join(dirpath, f)
                    # store relative (POSIX style) for matrix
                    found.append(rel.replace('\\', '/'))
    return found


def load_matrix(matrix_path):
    with open(matrix_path, 'r', encoding='utf-8') as f:
        return f.read()


def save_matrix(matrix_path, content):
    with open(matrix_path, 'w', encoding='utf-8') as f:
        f.write(content)


def append_evidence_to_rows(content: str, artifacts: list[str], scenario_filter: str | None = None) -> str:
    lines = content.split('\n')
    today = date.today().isoformat()
    # Limit changes to Scenario Packs only
    # Find all scenario pack sections
    indices = [i for i,l in enumerate(lines) if any(l.startswith(h) for h in SCENARIO_HEADERS)]
    if not indices:
        return content

    # Build quick map per artifact -> label
    art_map = {}
    for art in artifacts:
        base = os.path.basename(art)
        label = None
        for rx, lbl in KEY_MAP:
            if rx.search(base):
                label = lbl
                break
        art_map[art] = label

    def should_touch_section(header_line: str) -> bool:
        if not scenario_filter:
            return True
        return scenario_filter.lower() in header_line.lower()

    i = 0
    while i < len(lines):
        line = lines[i]
        if any(line.startswith(h) for h in SCENARIO_HEADERS) and should_touch_section(line):
            # Seek table header
            j = i + 1
            while j < len(lines) and not re.match(r'^\|\s*OSO', lines[j]):
                # stop if new section
                if lines[j].startswith('## ') and j != i+1:
                    break
                j += 1
            if j >= len(lines) or not re.match(r'^\|\s*OSO', lines[j]):
                i += 1
                continue
            # Skip separator row
            j += 2
            while j < len(lines):
                row = lines[j]
                if not row.strip() or row.startswith('---') or row.startswith('## '):
                    break
                if not row.startswith('|') or '|---' in row:
                    j += 1
                    continue
                parts = row.split('|')
                if len(parts) < 11:
                    j += 1
                    continue
                evidence = parts[6].strip()
                # Try appending relevant artifacts
                changed = False
                for art, label in art_map.items():
                    if os.path.basename(art) in evidence:
                        continue
                    # give mild relevance if label matches OSO title (parts[1])
                    title = parts[1].lower()
                    if label and label.lower().split()[0] in title:
                        ev_note = f"{evidence}; {os.path.basename(art)}" if evidence else os.path.basename(art)
                        parts[6] = f" {ev_note} "
                        parts[8] = ' chrmc '
                        parts[9] = f" {today} "
                        lines[j] = '|'.join(parts)
                        changed = True
                        break
                if not changed:
                    # generic attach one mission briefing md if row lacks any 'Mission briefing:'
                    if 'Mission briefing:' not in evidence:
                        mds = [a for a in artifacts if a.lower().endswith('.md') and '/Missions/' in a]
                        if mds:
                            note = f"Mission briefing: `{mds[0]}`"
                            parts[6] = f" {evidence + '; ' if evidence else ''}{note} "
                            parts[8] = ' chrmc '
                            parts[9] = f" {today} "
                            lines[j] = '|'.join(parts)
                j += 1
            i = j
        else:
            i += 1
    return '\n'.join(lines)


def main():
    ap = argparse.ArgumentParser(description='Evidence Sync')
    ap.add_argument('--dir', action='append', default=['Docs/Compliance', 'Logs'], help='Directories to scan')
    ap.add_argument('--scenario', default=None, help='Optional scenario title filter (e.g., "Facade Cleaning" or "Stadium")')
    args = ap.parse_args()

    artifacts = find_files(args.dir)
    if not artifacts:
        print('No artifacts found')
        return

    if not os.path.exists(MATRIX_PATH):
        print('Matrix file not found')
        return
    content = load_matrix(MATRIX_PATH)
    updated = append_evidence_to_rows(content, artifacts, args.scenario)
    if updated != content:
        save_matrix(MATRIX_PATH, updated)
        print(f'Updated matrix with {len(artifacts)} artifact(s).')
    else:
        print('No changes applied.')

if __name__ == '__main__':
    main()
