#!/usr/bin/env python3
"""
Send ARC Field Mapping Fix to Claude Sonnet
"""
import anthropic
import os

api_key = os.getenv("ANTHROPIC_API_KEY")
if not api_key:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please export it or set it in your environment.")

client = anthropic.Anthropic(api_key=api_key)

# Read problem description
with open("ARC_FIELD_MAPPING_FIX_FOR_SONNET.md", "r", encoding="utf-8") as f:
    problem = f.read()

# Read current main.py
with open("Backend_Python/main.py", "r", encoding="utf-8") as f:
    current_main = f.read()

prompt = f"""You are a FastAPI expert. The detailed logging you added revealed the exact problem!

{problem}

# CURRENT main.py (with your logging enhancements)
```python
{current_main}
```

# YOUR TASK
Provide ONLY the complete fixed code for the two ARC endpoints that:
1. Maps frontend field names → backend field names
2. Converts units (meters → feet for SORA 2.0)
3. Preserves your detailed logging
4. Returns 200 OK

Focus ONLY on the calculate_arc_2_0() and calculate_arc_2_5() functions.
"""

print("🚀 Sending field mapping fix to Claude Sonnet 4...")

try:
    message = client.messages.create(
        model="claude-sonnet-4-20250514",
        max_tokens=3000,
        messages=[{"role": "user", "content": prompt}]
    )
    
    response_text = message.content[0].text
    
    # Calculate cost
    input_tokens = message.usage.input_tokens
    output_tokens = message.usage.output_tokens
    input_cost = (input_tokens / 1_000_000) * 3.0
    output_cost = (output_tokens / 1_000_000) * 15.0
    total_cost = input_cost + output_cost
    
    print(f"\n✅ Sonnet 4 responded!")
    print(f"📊 Tokens:")
    print(f"   - Input: {input_tokens:,} (${input_cost:.4f})")
    print(f"   - Output: {output_tokens:,} (${output_cost:.4f})")
    print(f"   - THIS: ${total_cost:.4f}")
    print(f"   - TOTAL: ${0.8063 + total_cost:.4f}")
    print(f"   - Remaining: ${18.47 - (0.8063 + total_cost):.4f}")
    
    # Save response
    with open("SONNET_ARC_FIELD_MAPPING_FIX.txt", "w", encoding="utf-8") as f:
        f.write(response_text)
    
    print(f"\n💾 Saved to SONNET_ARC_FIELD_MAPPING_FIX.txt")
    print(f"\n" + "="*80)
    print(response_text)
    print("="*80)

except Exception as e:
    print(f"\n❌ Error: {e}")
    raise
