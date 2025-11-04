#!/usr/bin/env python3
"""
Send Frontend ARC 422 Error to Claude Sonnet for fix
"""
import anthropic
import os

api_key = os.getenv("ANTHROPIC_API_KEY")
if not api_key:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

client = anthropic.Anthropic(api_key=api_key)

# Read problem description
with open("FRONTEND_ARC_BUG_FOR_SONNET.md", "r", encoding="utf-8") as f:
    problem = f.read()

# Read sora_models.py
with open("Backend_Python/models/sora_models.py", "r", encoding="utf-8") as f:
    models = f.read()

# Read main.py
with open("Backend_Python/main.py", "r", encoding="utf-8") as f:
    main_code = f.read()

prompt = f"""You are a FastAPI/Pydantic expert debugging a 422 validation error.

# PROBLEM
{problem}

# BACKEND CODE

## sora_models.py (Pydantic models)
```python
{models}
```

## main.py (FastAPI endpoints)
```python
{main_code}
```

# YOUR TASK
1. **Identify root cause** of 422 validation errors for ARC endpoints
2. **Add detailed error logging** to catch and log validation errors with field details
3. **Suggest fixes** to make ARC endpoints work like GRC endpoints (which return 200 OK)
4. **Provide complete code** for fixed main.py ARC endpoints

# CRITICAL CONSTRAINTS
- GRC endpoints work perfectly (200 OK)
- ARC endpoints fail with 422 (same frontend, same .NET backend proxy)
- Tests pass 20/20 in isolation (direct Python calls work)
- Only frontend→backend calls fail

# DELIVERABLE
Complete fixed code for main.py ARC endpoints with proper validation error handling and logging.
"""

print("🚀 Sending to Claude Sonnet 4...")
print(f"📝 Prompt length: {len(prompt)} chars")

try:
    message = client.messages.create(
        model="claude-sonnet-4-20250514",
        max_tokens=4000,
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
    print(f"   - TOTAL: ${0.7263 + total_cost:.4f}")
    print(f"   - Remaining: ${18.47 - (0.7263 + total_cost):.4f}")
    
    # Save response
    with open("SONNET_ARC_FIX_RESPONSE.txt", "w", encoding="utf-8") as f:
        f.write(response_text)
    
    print(f"\n💾 Saved to SONNET_ARC_FIX_RESPONSE.txt")
    print(f"\n" + "="*80)
    print(response_text)
    print("="*80)

except Exception as e:
    print(f"\n❌ Error: {e}")
    raise
