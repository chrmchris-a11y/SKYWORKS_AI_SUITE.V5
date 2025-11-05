"""
Send Step 52 Mission Templates prompt to Claude Sonnet 4.5
- Reads AI_PROMPTS/STEP_52_MISSION_TEMPLATES.md
- Requires ANTHROPIC_API_KEY in environment (not stored in repo)
- Saves response under SONNET4/SONNET_STEP52_RESPONSE_<timestamp>.txt
"""

import os
import sys
import datetime as _dt
from pathlib import Path

try:
    import anthropic
except Exception as e:
    print("anthropic package not found. Please install with your chosen interpreter:")
    print("  pip install anthropic")
    sys.exit(1)

ROOT = Path(__file__).resolve().parents[1]
PROMPT_PATH = ROOT / "AI_PROMPTS" / "STEP_52_MISSION_TEMPLATES.md"
OUT_DIR = ROOT / "SONNET4"
OUT_DIR.mkdir(parents=True, exist_ok=True)

API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    print("ERROR: ANTHROPIC_API_KEY is not set in environment. Aborting to avoid leaking secrets.")
    sys.exit(1)

if not PROMPT_PATH.exists():
    print(f"ERROR: Prompt file not found: {PROMPT_PATH}")
    sys.exit(1)

prompt = PROMPT_PATH.read_text(encoding="utf-8")
print(f"Loaded Step 52 prompt: {PROMPT_PATH} ({len(prompt):,} chars)")

client = anthropic.Anthropic(api_key=API_KEY)
print("Connecting to Anthropic and sending request (Sonnet 4.5)...")

try:
    message = client.messages.create(
        model="claude-sonnet-4-20250514",
        max_tokens=8000,
        temperature=0,
        messages=[{"role": "user", "content": prompt}],
    )
    response_text = message.content[0].text
    ts = _dt.datetime.now().strftime("%Y%m%d_%H%M%S")
    out_file = OUT_DIR / f"SONNET_STEP52_RESPONSE_{ts}.txt"
    out_file.write_text(response_text, encoding="utf-8")

    latest = OUT_DIR / "SONNET_STEP52_RESPONSE_LATEST.txt"
    latest.write_text(response_text, encoding="utf-8")

    print("SUCCESS: Response saved to:")
    print(f"  {out_file}")
    print(f"  {latest}")
    print("Usage:")
    try:
        print(f"  Input tokens: {message.usage.input_tokens}")
        print(f"  Output tokens: {message.usage.output_tokens}")
    except Exception:
        pass
except Exception as e:
    print("ERROR while contacting Anthropic:")
    print(e)
    sys.exit(2)
