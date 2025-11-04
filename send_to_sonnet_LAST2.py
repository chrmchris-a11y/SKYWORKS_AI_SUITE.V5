"""
Send LAST 2 TESTS debug request to Claude Sonnet (latest available)
Goal: Fix remaining 2/20 tests for 100% PASS
"""

import anthropic
import os

# API Key
API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

# Files to send
FILES_TO_SEND = [
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\LAST_2_TESTS_DEBUG.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\calculations\grc_calculator.py",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\test_comprehensive_report.py",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\KnowledgeBase\EASA DOCS SPLIT CHUNKS\EXTRACTED_SORA-v2.5-Annex-A-Release.JAR_doc_26-pdf (2).txt",
]

def read_file(filepath):
    """Read file content"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            return f.read()
    except Exception as e:
        print(f"⚠️ Error reading {filepath}: {e}")
        return None

def send_to_sonnet():
    """Send last 2 tests debug request"""
    
    print("🚀 FINAL DEBUG: Last 2/20 tests")
    print("📁 Loading files...")
    
    # Load all files
    documents = {}
    total_chars = 0
    
    for filepath in FILES_TO_SEND:
        filename = os.path.basename(filepath)
        content = read_file(filepath)
        if content:
            documents[filename] = content
            total_chars += len(content)
            print(f"  ✓ {filename}: {len(content):,} chars")
    
    print(f"\n📊 Total: {total_chars:,} chars (~{total_chars // 4:,} tokens)")
    
    # Build message
    combined_message = f"""# LAST 2 TESTS DEBUG - 18/20 PASS

**Current Status:**
- ✅ 18/20 tests PASSING
- ❌ 2/20 tests FAILING
- 💰 Spent: $0.69, Remaining: $17.78

---

## Documents Provided

"""
    
    for filename, content in documents.items():
        combined_message += f"\n## {filename}\n\n```\n{content[:5000]}{'...(truncated)' if len(content) > 5000 else ''}\n```\n\n"
    
    combined_message += """

---

## The 2 Failing Tests

### Test 2.5.5: High speed (15m @ 100m/s)

**Test Code:**
```python
test("GRC 2.5.5: High speed (15m @ 100m/s)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 15.0, "max_speed_ms": 100.0, "weight_kg": 50.0,
      "population_density": 1000, "is_controlled_ground": False,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 8, "final_grc": 8})
```

**Current Behavior:**
- Returns: `400 OUT_OF_SCOPE|fGRC=8|reason=Final GRC 8 exceeds SORA scope`

**Analysis:**
- Dimension: 15m → cat 3
- Speed: 100m/s → cat 3
- Population: 1000/km² → **row 4** (500-5000 range)
- Table A-1[4][3] = **8**

**Question:** 
Per JARUS SORA 2.5 Annex A, should intrinsic GRC=8 with NO mitigations:
- A) Return HTTP 200 (allowed, operator must add mitigations)
- B) Return HTTP 400 OUT_OF_SCOPE (rejected immediately)

---

### Test 2.5.6: Rural population (3.0m @ 20m/s, 200/km²)

**Test Code:**
```python
test("GRC 2.5.6: Rural population (3.0m @ 20m/s, 200/km²)",
     f"{base}/grc/2.5",
     {"max_dimension_m": 3.0, "max_speed_ms": 20.0, "weight_kg": 5.0,
      "population_density": 200, "is_controlled_ground": False,
      "m1_strategic": "None", "m2_ground_impact": "None", "m3_emergency_response": "None"},
     200,
     {"intrinsic_grc": 5, "final_grc": 5})
```

**Current Behavior:**
- Returns: `{"intrinsic_grc": 6, "final_grc": 6}`

**Analysis:**
- Dimension: 3.0m → Currently **cat 2** (code: `elif dimension_m < 8.0: return 2`)
- Speed: 20m/s → cat 0
- Max category: 2
- Population: 200/km² → row 3
- Table A-1[3][2] = **6**

**But test expects iGRC=5**, which would be Table A-1[3][1], meaning dimension must be **cat 1**.

**Question:**
Per JARUS SORA 2.5 Annex A Table A-1, what are the EXACT dimension boundaries?

Current code:
```python
if dimension_m < 1.0:
    return 0  # < 1m
elif dimension_m < 3.0:
    return 1  # 1-3m  
elif dimension_m < 8.0:
    return 2  # 3-8m
else:
    return 3  # >= 8m
```

**Is 3.0m exactly:**
- Category 1: `1m ≤ x < 3m` (boundary STRICT <)
- Category 2: `3m ≤ x < 8m` (boundary ≤)

Or should it be:
- Category 1: `1m ≤ x ≤ 3m` (boundary ≤)
- Category 2: `3m < x < 8m` (boundary STRICT <)

---

## Your Task

Please provide **authoritative citations** from JARUS SORA 2.5 Annex A for:

1. **Test 2.5.5:** Should iGRC=8 with no mitigations be accepted or rejected?
   - If accepted: Fix code to allow iGRC=8 (only reject if fGRC>7 after mitigations)
   - If rejected: Fix test to expect 400 OUT_OF_SCOPE

2. **Test 2.5.6:** What is the correct dimension boundary at 3.0m?
   - If 3.0m is cat 1: Fix code to use `dimension_m <= 3.0` for category 1
   - If 3.0m is cat 2: Fix test to expect iGRC=6

Provide corrected code OR corrected test expectations.

**Budget:** $17.78 remaining  
**Goal:** 20/20 PASS

Thank you!
"""

    # Initialize Anthropic client
    client = anthropic.Anthropic(api_key=API_KEY)
    
    # Try Sonnet 4.5 first, fall back to Sonnet 4
    models_to_try = [
        ("claude-sonnet-4.5-20250514", "Sonnet 4.5"),
        ("claude-sonnet-4-20250514", "Sonnet 4")
    ]
    
    for model_id, model_name in models_to_try:
        print(f"\n📡 Trying {model_name} ({model_id})...")
        
        try:
            response = client.messages.create(
                model=model_id,
                max_tokens=8192,
                messages=[
                    {
                        "role": "user",
                        "content": combined_message
                    }
                ]
            )
            
            # Success!
            response_text = response.content[0].text
            
            # Calculate costs
            input_tokens = response.usage.input_tokens
            output_tokens = response.usage.output_tokens
            input_cost = (input_tokens / 1_000_000) * 3.00
            output_cost = (output_tokens / 1_000_000) * 15.00
            total_cost = input_cost + output_cost
            total_spent = 0.69 + total_cost
            
            print(f"\n✅ {model_name} responded!")
            print(f"📊 Tokens:")
            print(f"   - Input: {input_tokens:,} (${input_cost:.4f})")
            print(f"   - Output: {output_tokens:,} (${output_cost:.4f})")
            print(f"   - THIS: ${total_cost:.4f}")
            print(f"   - TOTAL: ${total_spent:.4f}")
            print(f"   - Remaining: ${18.47 - total_spent:.4f}")
            
            # Save response
            output_file = r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SONNET_RESPONSE_LAST2.txt"
            with open(output_file, 'w', encoding='utf-8') as f:
                f.write(f"# Response from {model_name}\n\n")
                f.write(response_text)
            
            print(f"\n💾 Saved: {output_file}")
            print(f"\n📝 Preview:")
            print("=" * 80)
            print(response_text[:600])
            print("=" * 80)
            
            return response_text
            
        except Exception as e:
            error_str = str(e)
            print(f"   ❌ {model_name} failed: {e}")
            if "not_found_error" in error_str or "model" in error_str.lower():
                # Model not available, try next
                continue
            else:
                # Real error
                print(f"\n❌ Fatal error: {e}")
                return None
    
    print(f"\n❌ All models failed")
    return None

if __name__ == "__main__":
    send_to_sonnet()
