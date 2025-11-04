"""
Send documents to Claude Sonnet 4 via API - ITERATION 2
Focus on 5 remaining test failures with full EASA/JARUS docs
"""

import anthropic
import os

# API Key
API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

# Files to send - ITERATION 2
FILES_TO_SEND = [
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\REMAINING_5_FAILURES_FOR_SONNET.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\test_results_SONNET_FIX.txt",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\calculations\grc_calculator.py",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\KnowledgeBase\EASA DOCS SPLIT CHUNKS\EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\KnowledgeBase\EASA DOCS SPLIT CHUNKS\EXTRACTED_SORA-v2.5-Annex-A-Release.JAR_doc_26-pdf (2).txt",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\ARC_CALCULATION_AUTHORITATIVE_REFERENCE.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\COMPREHENSIVE_ACCURACY_REPORT.md",
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
    """Send request to Claude Sonnet 4"""
    
    print("🚀 ITERATION 2: Sending 5 remaining test failures to Claude Sonnet 4...")
    print(f"📁 Loading {len(FILES_TO_SEND)} files (including EASA/JARUS docs)...")
    
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
    
    # Build combined message
    combined_message = f"""# GRC Calculator Fix Request - ITERATION 2

## Current Status
- **Previous iteration result:** 15/20 tests PASSING (improved from 14/20)
- **Remaining failures:** 5 GRC tests
- **Cost so far:** $0.1553
- **Budget remaining:** ~$18.31

## Documents Provided

"""
    
    for filename, content in documents.items():
        combined_message += f"\n## {filename}\n\n```\n{content}\n```\n\n---\n\n"
    
    combined_message += """

## Your Task

You are an expert in JARUS SORA 2.0 and 2.5 GRC (Ground Risk Class) calculations.

Analyze the 5 remaining test failures documented in `REMAINING_5_FAILURES_FOR_SONNET.md`:

1. **GRC 2.0.2**: Floor enforcement (3.0m BVLOS_Populated + M1 High) - expects 3, got 5
2. **GRC 2.5.4**: Column floor enforcement (5.0m @ 30m/s + M1 High) - expects 3, got 4  
3. **GRC 2.5.5**: High speed (15m @ 100m/s) - expects HTTP 200, got 400 OUT_OF_SCOPE
4. **GRC 2.5.6**: Rural population (3.0m @ 20m/s, 200/km²) - expects GRC 5, got 6
5. **GRC 2.5.7**: All mitigations High (10m @ 50m/s) - expects 4, got 3

Using the **authoritative EASA/JARUS documents** I've provided:
- `EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt` (SORA 2.0 specification)
- `EXTRACTED_SORA-v2.5-Annex-A-Release.JAR_doc_26-pdf (2).txt` (SORA 2.5 Annex A)
- `ARC_CALCULATION_AUTHORITATIVE_REFERENCE.md` (proven accurate ARC logic)
- `COMPREHENSIVE_ACCURACY_REPORT.md` (shows what was working correctly before)

**Please provide:**

1. **Root cause analysis** for each of the 5 failures
2. **Corrected Python code** for `grc_calculator.py` that fixes ONLY these 5 issues
3. **Explanation** of what was wrong and what you changed
4. **Citations** from the EASA/JARUS docs to support your fixes

**CRITICAL:**
- Do NOT break the 15 tests that are currently passing
- Focus on floor enforcement, population density boundaries, and OUT_OF_SCOPE handling
- Use the EXACT formulas from the official JARUS documents
- Preserve all existing code structure and comments

Return ONLY the corrected `grc_calculator.py` code wrapped in ```python blocks, followed by your explanation.
"""

    # Initialize Anthropic client
    client = anthropic.Anthropic(api_key=API_KEY)
    
    print("\n📡 Sending to Claude Sonnet 4 (claude-sonnet-4-20250514)...")
    
    try:
        # Make API call
        response = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=8192,
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
        input_cost = (input_tokens / 1_000_000) * 3.00  # $3 per 1M input tokens
        output_cost = (output_tokens / 1_000_000) * 15.00  # $15 per 1M output tokens
        total_cost = input_cost + output_cost
        
        print(f"\n✅ Response received!")
        print(f"📊 Tokens used:")
        print(f"   - Input: {input_tokens:,} tokens (${input_cost:.4f})")
        print(f"   - Output: {output_tokens:,} tokens (${output_cost:.4f})")
        print(f"   - TOTAL COST: ${total_cost:.4f}")
        print(f"   - Remaining budget: ${18.31 - 0.1553 - total_cost:.4f}")
        
        # Save response
        output_file = r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SONNET_RESPONSE_ITERATION2.txt"
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(response_text)
        
        print(f"\n💾 Response saved to: {output_file}")
        print(f"\n📝 Response preview (first 500 chars):")
        print("=" * 80)
        print(response_text[:500])
        print("=" * 80)
        
        return response_text
        
    except Exception as e:
        print(f"\n❌ Error: {e}")
        return None

if __name__ == "__main__":
    send_to_sonnet()
