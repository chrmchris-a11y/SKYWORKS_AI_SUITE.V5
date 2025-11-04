#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Email Sender — sends email with attachments via SMTP from environment variables,
otherwise writes a .eml draft if SMTP settings are missing.

Env vars (for send):
  SKYMAIL_HOST, SKYMAIL_PORT, SKYMAIL_USER, SKYMAIL_PASS, SKYMAIL_FROM

Usage:
  python email_sender.py --to someone@example.com --subject "Subject" --body "Body" --attach "a.txt;b.pdf"

Output:
  - If SMTP vars present: sends email and prints "SENT".
  - Else: writes Docs/Compliance/Deliverables/<subject_sanitized>.eml and prints path.
"""
from __future__ import annotations
import argparse
import os
import sys
import mimetypes
from email.message import EmailMessage
from email.utils import formatdate, make_msgid
import smtplib

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DELIV_DIR = os.path.join(ROOT, 'Docs', 'Compliance', 'Deliverables')


def have_smtp() -> bool:
    return all(os.getenv(k) for k in ['SKYMAIL_HOST','SKYMAIL_PORT','SKYMAIL_USER','SKYMAIL_PASS','SKYMAIL_FROM'])


def build_message(from_addr: str, to_addrs: list[str], subject: str, body: str, attachments: list[str]) -> EmailMessage:
    msg = EmailMessage()
    msg['From'] = from_addr
    msg['To'] = ', '.join(to_addrs)
    msg['Date'] = formatdate(localtime=True)
    msg['Subject'] = subject
    msg['Message-ID'] = make_msgid()
    msg.set_content(body)

    for path in attachments:
        if not os.path.exists(path):
            continue
        ctype, encoding = mimetypes.guess_type(path)
        if ctype is None or encoding is not None:
            ctype = 'application/octet-stream'
        maintype, subtype = ctype.split('/', 1)
        with open(path, 'rb') as f:
            data = f.read()
        msg.add_attachment(data, maintype=maintype, subtype=subtype, filename=os.path.basename(path))
    return msg


def send_or_save(to: str, subject: str, body: str, attach: str | None):
    to_addrs = [x.strip() for x in to.split(',') if x.strip()]
    attachments = [x.strip() for x in (attach or '').split(';') if x.strip()]
    if have_smtp():
        host = os.getenv('SKYMAIL_HOST')
        port = int(os.getenv('SKYMAIL_PORT'))
        user = os.getenv('SKYMAIL_USER')
        pwd  = os.getenv('SKYMAIL_PASS')
        from_addr = os.getenv('SKYMAIL_FROM')
        msg = build_message(from_addr, to_addrs, subject, body, attachments)
        with smtplib.SMTP(host, port, timeout=30) as s:
            s.starttls()
            s.login(user, pwd)
            s.send_message(msg)
        print('SENT')
    else:
        os.makedirs(DELIV_DIR, exist_ok=True)
        safe = ''.join(c for c in subject if c.isalnum() or c in (' ','_','-')).strip().replace(' ','_')
        out_path = os.path.join(DELIV_DIR, f"{safe}.eml")
        from_addr = 'noreply@example.com'
        msg = build_message(from_addr, to_addrs, subject, body, attachments)
        with open(out_path, 'wb') as f:
            f.write(bytes(msg))
        rel = os.path.relpath(out_path, ROOT).replace('\\','/')
        print(rel)


def main():
    if hasattr(sys.stdout, 'reconfigure'):
        try:
            sys.stdout.reconfigure(encoding='utf-8')
        except Exception:
            pass

    ap = argparse.ArgumentParser(description='Email sender (SMTP or EML draft)')
    ap.add_argument('--to', required=True, help='Comma-separated recipients')
    ap.add_argument('--subject', required=True)
    ap.add_argument('--body', required=True)
    ap.add_argument('--attach', default=None, help='Semicolon-separated attachment paths')
    args = ap.parse_args()

    send_or_save(args.to, args.subject, args.body, args.attach)


if __name__ == '__main__':
    main()
