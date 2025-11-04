#!/usr/bin/env python3
"""
Send COMPLETE system fix request to Claude Sonnet
FINAL comprehensive fix for all issues
"""
import anthropic
import os

api_key = os.getenv("ANTHROPIC_API_KEY")
if not api_key:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

client = anthropic.Anthropic(api_key=api_key)

# Read problem description
with open("COMPLETE_SYSTEM_FIX_FOR_SONNET.md", "r", encoding="utf-8") as f:
    problem = f.read()

# Read PythonCalculationClient.cs
with open("Backend/src/Skyworks.Core/Services/Python/PythonCalculationClient.cs", "r", encoding="utf-8") as f:
    dotnet_client = f.read()

# Read sora_models.py
with open("Backend_Python/models/sora_models.py", "r", encoding="utf-8") as f:
    python_models = f.read()

prompt = f"""You are a full-stack expert fixing a critical field mapping issue in a microservices architecture.

# PROBLEM DESCRIPTION
{problem}

# CURRENT CODE

## .NET Backend: PythonCalculationClient.cs
```csharp
{dotnet_client}
```

## Python Backend: sora_models.py  
```python
{python_models}
```

# YOUR TASK

Provide COMPLETE fixed code for:

1. **PythonCalculationClient.cs** - Fix field names from `max_height_agl_m` to correct `altitude_agl_ft` (SORA 2.0) and `altitude_agl_m` (SORA 2.5)

2. **sora_models.py** - Add backwards compatibility field aliases so it accepts BOTH old and new field names

3. **START_ALL_BACKENDS.ps1** - Complete PowerShell script to start all 3 backends in separate windows with health monitoring

4. **Any other critical fixes** for production-ready deployment

# CRITICAL CONSTRAINTS
- Must maintain 100% EASA/JARUS compliance
- Must work with existing 20/20 passing test suite
- Must handle field name variations gracefully
- Zero downtime, zero manual intervention after startup

# DELIVERABLE
Complete, production-ready code files that can be copy-pasted directly.
"""

print("🚀 Sending COMPLETE SYSTEM FIX to Claude Sonnet 4...")
print(f"📝 Prompt length: {len(prompt)} chars")

try:
    message = client.messages.create(
        model="claude-sonnet-4-20250514",
        max_tokens=8000,
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
    with open("SONNET_COMPLETE_FIX.txt", "w", encoding="utf-8") as f:
        f.write(response_text)
    
    print(f"\n💾 Saved to SONNET_COMPLETE_FIX.txt")
    print(f"\n" + "="*80)
    print(response_text[:2000])  # Print first 2000 chars
    print("="*80)
    print(f"\n📄 Full response in SONNET_COMPLETE_FIX.txt")

except Exception as e:
    print(f"\n❌ Error: {e}")
    raise
