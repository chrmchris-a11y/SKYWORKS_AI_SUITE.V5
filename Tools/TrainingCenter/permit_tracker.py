#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Permit Tracker — simple JSON-backed registry for permits/notifications.

Usage:
  python permit_tracker.py --list [--filter active|expired|all] [--entity "City of X"] [--type venue]
  python permit_tracker.py --add --type venue --entity "City of X" --location "Main Square" --valid_from 2025-10-22 --valid_to 2025-10-23 --status approved --reference REF123 --notes "Dawn ops"

Data file: Tools/TrainingCenter/permits.json
"""
from __future__ import annotations
import argparse
import json
import os
from datetime import datetime
from typing import Any, Dict, List

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DATA_PATH = os.path.join(ROOT, 'Tools', 'TrainingCenter', 'permits.json')


def load_data() -> List[Dict[str, Any]]:
    if not os.path.exists(DATA_PATH):
        return []
    with open(DATA_PATH, 'r', encoding='utf-8') as f:
        try:
            return json.load(f)
        except Exception:
            return []


def save_data(items: List[Dict[str, Any]]):
    with open(DATA_PATH, 'w', encoding='utf-8') as f:
        json.dump(items, f, ensure_ascii=False, indent=2)


def is_active(item: Dict[str, Any], today: datetime) -> bool:
    try:
        start = datetime.fromisoformat(item.get('valid_from', '1970-01-01'))
        end = datetime.fromisoformat(item.get('valid_to', '2100-01-01'))
        return start <= today <= end
    except Exception:
        return False


def cmd_list(filter_: str | None, entity: str | None, type_: str | None):
    items = load_data()
    today = datetime.now()
    out: List[str] = []

    def ok(i: Dict[str, Any]) -> bool:
        if entity and entity.lower() not in (i.get('entity','').lower()):
            return False
        if type_ and type_.lower() != (i.get('type','').lower()):
            return False
        if (filter_ or '').lower() == 'active':
            return is_active(i, today)
        if (filter_ or '').lower() == 'expired':
            return not is_active(i, today)
        return True

    filtered = [i for i in items if ok(i)]
    if not filtered:
        print("_No permits found for the given filters._")
        return

    out.append("# Permits\n")
    for i in filtered:
        out.append(f"- [{i.get('type','?')}] {i.get('entity','?')} — {i.get('location','?')} ")
        out.append(f"  • Valid: {i.get('valid_from','?')} → {i.get('valid_to','?')}  • Status: {i.get('status','?')}")
        if i.get('reference'):
            out.append(f"  • Ref: {i.get('reference')}")
        if i.get('notes'):
            out.append(f"  • Notes: {i.get('notes')}")
        out.append("")
    print("\n".join(out))


def cmd_add(fields: Dict[str, str]):
    required = ['type','entity','location','valid_from','valid_to','status']
    missing = [k for k in required if k not in fields]
    if missing:
        raise SystemExit(f"Missing required fields: {', '.join(missing)}")

    # validate dates
    try:
        datetime.fromisoformat(fields['valid_from'])
        datetime.fromisoformat(fields['valid_to'])
    except Exception as e:
        raise SystemExit("valid_from/valid_to must be ISO dates, e.g., 2025-10-22") from e

    items = load_data()
    # build entry
    entry = {
        'id': f"P{int(datetime.now().timestamp())}",
        'type': fields['type'],
        'entity': fields['entity'],
        'location': fields['location'],
        'valid_from': fields['valid_from'],
        'valid_to': fields['valid_to'],
        'status': fields['status'],
        'reference': fields.get('reference',''),
        'notes': fields.get('notes',''),
    }
    items.append(entry)
    save_data(items)
    rel = os.path.relpath(DATA_PATH, ROOT).replace('\\','/')
    print(f"✅ Permit saved to `{rel}`\n")
    print("- Added:")
    print(f"  • [{entry['type']}] {entry['entity']} — {entry['location']}  • {entry['valid_from']} → {entry['valid_to']}  • {entry['status']}")
    if entry['reference']:
        print(f"  • Ref: {entry['reference']}")
    if entry['notes']:
        print(f"  • Notes: {entry['notes']}")


def main():
    ap = argparse.ArgumentParser(description='Permit tracker')
    ap.add_argument('--list', action='store_true')
    ap.add_argument('--add', action='store_true')
    # filters for list
    ap.add_argument('--filter', choices=['active','expired','all'], default=None)
    ap.add_argument('--entity', default=None)
    ap.add_argument('--type', dest='type_', default=None)
    # fields for add
    ap.add_argument('--location', default=None)
    ap.add_argument('--valid_from', default=None)
    ap.add_argument('--valid_to', default=None)
    ap.add_argument('--status', default=None)
    ap.add_argument('--reference', default=None)
    ap.add_argument('--notes', default=None)
    args = ap.parse_args()

    if args.list:
        cmd_list(args.filter, args.entity, args.type_)
        return
    if args.add:
        fields = {}
        if args.type_:
            fields['type'] = args.type_
        if args.entity:
            fields['entity'] = args.entity
        if args.location:
            fields['location'] = args.location
        if args.valid_from:
            fields['valid_from'] = args.valid_from
        if args.valid_to:
            fields['valid_to'] = args.valid_to
        if args.status:
            fields['status'] = args.status
        if args.reference:
            fields['reference'] = args.reference
        if args.notes:
            fields['notes'] = args.notes
        cmd_add(fields)
        return

    raise SystemExit('Use --list or --add')


if __name__ == '__main__':
    main()
