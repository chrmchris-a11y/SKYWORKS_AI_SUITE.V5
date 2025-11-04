#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Compliance Binder Generator

Composes an audit-ready binder that bundles:
- Mission Briefing (Markdown)
- OSO matrix excerpt for the matching Scenario Pack (if found)
- Approvals extracted from mission JSON (if available)

Outputs: Docs/Compliance/Binder/<operation>_<time>_binder.md (by default)

Usage:
  python compliance_binder.py --operation facade --time 06:00 --mission Docs/Compliance/Missions/Facade_0600.md
  python compliance_binder.py --operation pv --time 08:00 --out Docs/Compliance/Binder/PV_0800_binder.md
"""
from __future__ import annotations
import argparse
import os
import re
import json
from datetime import datetime
from typing import Optional, Tuple

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OSO_PATH = os.path.join(ROOT, 'Docs', 'Compliance', 'OSO_to_Evidence_Matrix.md')
BINDER_DIR = os.path.join(ROOT, 'Docs', 'Compliance', 'Binder')

SCENARIO_TITLES = {
    'facade': 'Facade Cleaning',
    'windows': 'Windows Cleaning',
    'roof': 'Roof Cleaning',
    'pv': 'PV Park Cleaning', 'pvpark': 'PV Park Cleaning', 'pv_park': 'PV Park Cleaning', 'solar': 'PV Park Cleaning', 'solarpark': 'PV Park Cleaning',
    'wind': 'Wind Turbine Cleaning', 'wind_turbine': 'Wind Turbine Cleaning', 'turbine': 'Wind Turbine Cleaning',
    'stadium': 'Stadium Cleaning'
}


def read_text(path: str) -> str:
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return f.read()
    except FileNotFoundError:
        return ''


def find_mission_artifacts(mission_md: Optional[str]) -> Tuple[str, Optional[str]]:
    if not mission_md:
        return '', None
    md_path = mission_md if os.path.isabs(mission_md) else os.path.join(ROOT, mission_md)
    md = read_text(md_path)
    js_path = os.path.splitext(md_path)[0] + '.json'
    js = read_text(js_path)
    return md, js if js else None


def extract_oso_block(operation: str, time_str: str) -> str:
    """Extract the OSO Scenario Pack block for the given operation and time; fallback to empty string."""
    raw = read_text(OSO_PATH)
    if not raw:
        return ''
    title = SCENARIO_TITLES.get(operation.lower())
    if not title:
        return ''
    header_prefix = f"## Scenario Pack: {title}"
    # Try to match header line that also contains the HH:MM time
    lines = raw.split('\n')
    header_idx = -1
    for i, line in enumerate(lines):
        if line.startswith(header_prefix) and time_str in line:
            header_idx = i
            break
    if header_idx == -1:
        # If not found with time, pick first scenario pack for this title
        for i, line in enumerate(lines):
            if line.startswith(header_prefix):
                header_idx = i
                break
    if header_idx == -1:
        return ''
    # Collect until next '---' or next '## '
    block_lines = []
    i = header_idx
    while i < len(lines):
        line = lines[i]
        if i > header_idx and (line.startswith('## ') or line.strip() == '---'):
            break
        block_lines.append(line)
        i += 1
    return '\n'.join(block_lines).strip()


def render_binder(operation: str, time_str: str, mission_md: str, mission_js: Optional[str], oso_excerpt: str) -> str:
    now = datetime.now().strftime('%Y-%m-%d %H:%M')
    base_title = SCENARIO_TITLES.get(operation.lower(), operation.title())
    approvals_md = ''
    if mission_js:
        try:
            data = json.loads(mission_js)
            approvals = data.get('approvals') or []
            if approvals:
                approvals_md = '\n'.join([f"- {a}" for a in approvals])
        except Exception:
            approvals_md = ''

    md_parts = [
        f"# Compliance Binder — {base_title} ({time_str})",
        '',
        f"Generated: {now}",
        '',
        '---',
        '',
        '## Mission Briefing',
        mission_md.strip() if mission_md else '_Mission briefing file not provided/found._',
        '',
        '---',
        '',
        '## Approvals (from mission)',
        approvals_md if approvals_md else '_No approval list found in mission JSON._',
        '',
        '---',
        '',
        '## OSO Matrix — Scenario Excerpt',
        oso_excerpt if oso_excerpt else '_No matching Scenario Pack found in OSO matrix._',
        '',
    ]
    return '\n'.join(md_parts).strip() + '\n'


def main():
    ap = argparse.ArgumentParser(description='Compliance Binder composer')
    ap.add_argument('--operation', required=True)
    ap.add_argument('--time', default='06:00')
    ap.add_argument('--mission', default=None, help='Path to Mission Briefing Markdown (optional)')
    ap.add_argument('--out', default=None, help='Output path for binder (Markdown)')
    args = ap.parse_args()

    mission_md, mission_js = find_mission_artifacts(args.mission)
    oso_excerpt = extract_oso_block(args.operation, args.time)

    binder_md = render_binder(args.operation, args.time, mission_md, mission_js, oso_excerpt)

    # Ensure output dir
    os.makedirs(BINDER_DIR, exist_ok=True)
    base = f"{args.operation.lower()}_{args.time.replace(':','')}"
    out_path = args.out if args.out else os.path.join(BINDER_DIR, f"{base}_binder.md")
    with open(out_path, 'w', encoding='utf-8') as f:
        f.write(binder_md)

    rel = os.path.relpath(out_path, ROOT).replace('\\','/')
    print(f"✅ Binder generated: `{rel}`\n")
    print(binder_md)


if __name__ == '__main__':
    main()
