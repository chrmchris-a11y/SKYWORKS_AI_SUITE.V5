"""
Send FINAL comprehensive request to Claude Sonnet 4 - ITERATION 3
Goal: 20/20 tests passing with 100% EASA/JARUS compliance
"""

import anthropic
import os

# API Key
API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

# Files to send - FINAL ITERATION
FILES_TO_SEND = [
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\FINAL_REQUEST_FOR_SONNET.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\calculations\grc_calculator.py",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\test_comprehensive_report.py",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\KnowledgeBase\EASA DOCS SPLIT CHUNKS\EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\KnowledgeBase\EASA DOCS SPLIT CHUNKS\EXTRACTED_SORA-v2.5-Annex-A-Release.JAR_doc_26-pdf (2).txt",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SONNET_RESPONSE_ITERATION2.txt",
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
    """Send final request to Claude Sonnet 4"""
    
    print("🚀 FINAL ITERATION 3: 100% EASA/JARUS Compliance Request")
    print(f"📁 Loading {len(FILES_TO_SEND)} files...")
    
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
        else:
            print(f"  ✗ {filename}: FAILED TO LOAD")
    
    print(f"\n📊 Total characters: {total_chars:,}")
    print(f"📊 Estimated tokens: ~{total_chars // 4:,}")
    
    # Build message with all context
    combined_message = f"""# FINAL REQUEST: 100% EASA/JARUS SORA COMPLIANCE

I need your expert analysis to achieve **20/20 tests passing** with **full JARUS compliance**.

## Current Status
- **Iteration 1:** 14/20 → 15/20 tests (cost: $0.1553)
- **Iteration 2:** 15/20 tests (cost: $0.2525)
- **Total cost:** $0.4078
- **Budget remaining:** $17.90

## The Challenge

After 2 iterations, we have **5 failing tests**. Your previous response (iteration 2) suggested that **some test expectations may be incorrect** rather than the code.

I need you to be the **ultimate authority** and determine:
1. Which tests have **wrong expectations** (fix the test)
2. Which code has **wrong implementation** (fix the code)
3. Ensure **100% JARUS SORA 2.0 AMC + JARUS SORA 2.5 compliance**

---

## Documents Provided

"""
    
    for filename, content in documents.items():
        combined_message += f"\n## FILE: {filename}\n\n```\n{content}\n```\n\n---\n\n"
    
    combined_message += """

---

## Your Mission

Please provide:

### 1. **Definitive Analysis for Each of the 5 Failing Tests**

For each test, state:
- ✅ **Authoritative Answer** (cite specific JARUS section/table/page)
- ✅ **Is Test Expectation Correct?** (YES/NO with citation)
- ✅ **Is Code Implementation Correct?** (YES/NO with citation)
- ✅ **Required Fix** (test expectation OR code OR both)

### 2. **Corrected Files**

Provide COMPLETE files (not diffs):

**A) `grc_calculator.py`** (full corrected Python code)  
**B) `test_expectations_corrections.py`** (only the 5 failing test corrections)

### 3. **JARUS Compliance Verification**

For **SORA 2.0**:
- Confirm `INTRINSIC_GRC_2_0` table matches JARUS SORA 2.0 Section 2.3.1
- Confirm `FLOOR_2_0` values are correct
- Clarify floor enforcement: applied per mitigation or final?

For **SORA 2.5**:
- Confirm `INTRINSIC_GRC_2_5` table matches JARUS Annex A Table A-1
- Confirm `MIN_GRC_FLOOR_2_5` values (row 0 of Table A-1)
- Confirm population density boundaries (< 5, < 50, < 500, < 5000, < 50000, ≥ 50000)
- Clarify floor enforcement mechanism
- Confirm 250g rule (weight ≤ 0.25kg AND speed < 25 m/s)

---

## Critical Requirements

1. ✅ **Do NOT break the 15 passing tests**
2. ✅ **Cite specific JARUS sections** for every decision
3. ✅ **Be definitive** - no "maybe" or "appears to be"
4. ✅ **100% compliance** - if JARUS says floor=3, enforce floor=3
5. ✅ **Preserve code structure** and comments

---

## The 5 Failing Tests (Quick Reference)

1. **GRC 2.0.2**: Expects final_grc=3, got 5 (floor enforcement)
2. **GRC 2.5.4**: Expects final_grc=3, got 4 (column floor enforcement)
3. **GRC 2.5.5**: Expects HTTP 200, got 400 OUT_OF_SCOPE (15m @ 100m/s)
4. **GRC 2.5.6**: Expects iGRC=5/fGRC=5, got 6/6 (200/km² population)
5. **GRC 2.5.7**: Expects final_grc=4, got 3 (all mitigations High)

See `FINAL_REQUEST_FOR_SONNET.md` for detailed analysis.

---

## Expected Output Format

```markdown
# FINAL ANALYSIS: 20/20 TESTS

## Test-by-Test Analysis

### Test 1: GRC 2.0.2 (Floor enforcement)
**JARUS Citation:** [Section X.X.X, Page Y]
**Test Expectation:** ✅ CORRECT / ❌ WRONG (should be Z)
**Code Implementation:** ✅ CORRECT / ❌ WRONG
**Fix Required:** [Test/Code/Both]
**Reasoning:** [explanation]

### Test 2: GRC 2.5.4 (Column floor)
...

## Corrected Code

### File: grc_calculator.py
\```python
[FULL corrected code]
\```

### File: test_corrections.py
\```python
# GRC 2.0.2: Floor enforcement
test(..., expected={"intrinsic_grc": X, "final_grc": Y})

# GRC 2.5.4: Column floor
test(..., expected={"intrinsic_grc": X, "final_grc": Y})
...
\```

## JARUS Compliance Verification

### SORA 2.0
- ✅ Intrinsic GRC table: [verified against Section X]
- ✅ Floor values: [verified]
- ✅ Floor enforcement: [mechanism explained]

### SORA 2.5
- ✅ Table A-1: [verified row by row]
- ✅ Floor values: [verified]
- ✅ Population boundaries: [verified]
- ✅ 250g rule: [verified]
```

---

**This is the FINAL iteration. We need 20/20 PASS.**

Thank you for your authoritative expertise!
"""

    # Initialize Anthropic client
    client = anthropic.Anthropic(api_key=API_KEY)
    
    print("\n📡 Sending FINAL request to Claude Sonnet 4 (claude-sonnet-4-20250514)...")
    print("⏳ This may take a moment due to comprehensive analysis...")
    
    try:
        # Make API call
        response = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=16384,  # Increased for comprehensive response
            messages=[
                {
                    "role": "user",
                    "content": combined_message
                }
            ]
        )
        
        # Extract response
        response_text = response.content[0].text
        
        # Calculate costs
        input_tokens = response.usage.input_tokens
        output_tokens = response.usage.output_tokens
        input_cost = (input_tokens / 1_000_000) * 3.00
        output_cost = (output_tokens / 1_000_000) * 15.00
        total_cost = input_cost + output_cost
        total_spent = 0.4078 + total_cost
        
        print(f"\n✅ Response received!")
        print(f"📊 Tokens used:")
        print(f"   - Input: {input_tokens:,} tokens (${input_cost:.4f})")
        print(f"   - Output: {output_tokens:,} tokens (${output_cost:.4f})")
        print(f"   - THIS REQUEST: ${total_cost:.4f}")
        print(f"   - TOTAL SPENT: ${total_spent:.4f}")
        print(f"   - Remaining budget: ${18.47 - total_spent:.4f}")
        
        # Save response
        output_file = r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SONNET_RESPONSE_FINAL.txt"
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(response_text)
        
        print(f"\n💾 Response saved to: {output_file}")
        print(f"\n📝 Response preview (first 800 chars):")
        print("=" * 100)
        print(response_text[:800])
        print("=" * 100)
        
        return response_text
        
    except Exception as e:
        print(f"\n❌ Error: {e}")
        return None

if __name__ == "__main__":
    send_to_sonnet()
