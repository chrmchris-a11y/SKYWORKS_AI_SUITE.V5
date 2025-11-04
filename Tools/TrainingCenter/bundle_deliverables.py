#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Bundle Deliverables — creates a ZIP with Binder + Reports for a scenario/time.

Usage:
  python bundle_deliverables.py --scenario Facade --time 06:00
  python bundle_deliverables.py --scenario Stadium --time 05:30 --out Docs/Compliance/Deliverables/Custom.zip

Emits a path to the ZIP (stdout) and creates it under Docs/Compliance/Deliverables by default.
"""
from __future__ import annotations
import argparse
import os
import sys
import zipfile
from typing import List

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
BINDER_DIR = os.path.join(ROOT, 'Docs', 'Compliance', 'Binder')
REPORTS_DIR = os.path.join(ROOT, 'Docs', 'Compliance', 'Reports')
DELIV_DIR = os.path.join(ROOT, 'Docs', 'Compliance', 'Deliverables')

SCENARIO_KEY = {
    'facade': ('facade', 'Facade'),
    'roof': ('roof', 'Roof'),
    'pv': ('pv', 'PV'),
    'windows': ('windows', 'Windows'),
    'wind': ('wind', 'Wind'),
    'stadium': ('stadium', 'Stadium'),
}


def safe_time(t: str) -> str:
    return t.replace(':', '')


def file_if_exists(p: str) -> List[str]:
    return [p] if os.path.exists(p) else []


def gather_files(scenario: str, time: str) -> List[str]:
    skey = scenario.lower()
    if skey not in SCENARIO_KEY:
        raise SystemExit(f"Unknown scenario: {scenario}")
    lower_key, title_key = SCENARIO_KEY[skey]
    time_s = safe_time(time)

    binder = os.path.join(BINDER_DIR, f"{lower_key}_{time_s}_binder.md")
    what_if = os.path.join(REPORTS_DIR, f"{title_key}_{time_s}_what_if.md")
    off_peak = os.path.join(REPORTS_DIR, f"{title_key}_{time_s}_off_peak.md")

    files: List[str] = []
    files += file_if_exists(binder)
    files += file_if_exists(what_if)
    files += file_if_exists(off_peak)
    return files


def make_zip(files: List[str], out_path: str):
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with zipfile.ZipFile(out_path, 'w', compression=zipfile.ZIP_DEFLATED) as z:
        for f in files:
            arcname = os.path.relpath(f, BINDER_DIR) if f.startswith(BINDER_DIR) else os.path.basename(f)
            z.write(f, arcname=arcname)


def main():
    if hasattr(sys.stdout, 'reconfigure'):
        try:
            sys.stdout.reconfigure(encoding='utf-8')
        except Exception:
            pass

    ap = argparse.ArgumentParser(description='Bundle deliverables (Binder + Reports)')
    ap.add_argument('--scenario', required=True)
    ap.add_argument('--time', required=True)
    ap.add_argument('--out', default=None)
    args = ap.parse_args()

    files = gather_files(args.scenario, args.time)
    if not files:
        raise SystemExit('No files found to bundle. Ensure binder and reports exist first.')

    out_path = args.out or os.path.join(DELIV_DIR, f"{args.scenario.title()}_{safe_time(args.time)}_Deliverables.zip")
    make_zip(files, out_path)

    rel = os.path.relpath(out_path, ROOT).replace('\\','/')
    print(rel)


if __name__ == '__main__':
    main()
