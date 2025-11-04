#!/usr/bin/env python3
"""
Send ARC_IMPLEMENTATION_SPECIFICATION.md to Claude Sonnet 4 for implementation.

This script reads the complete ARC specification and sends it to Claude Sonnet 4
via the Anthropic API with clear instructions to implement all requirements.
"""

import os
import sys
import json
from pathlib import Path
from datetime import datetime
import anthropic

# Configuration
SPEC_FILE = "ARC_IMPLEMENTATION_SPECIFICATION.md"
OUTPUT_DIR = "SONNET4_ARC_OUTPUT"
MAX_TOKENS = 16000
MODEL = "claude-sonnet-4-20250514"

def main():
    # Check API key
    api_key = os.environ.get("ANTHROPIC_API_KEY")
    if not api_key:
        print("❌ ANTHROPIC_API_KEY not found!")
        print("   Set it with: export ANTHROPIC_API_KEY='your-key'")
        sys.exit(1)
    
    # Check specification file
    spec_path = Path(SPEC_FILE)
    if not spec_path.exists():
        print(f"❌ Specification file not found: {SPEC_FILE}")
        sys.exit(1)
    
    # Read specification
    print(f"📖 Reading specification: {SPEC_FILE}")
    spec_content = spec_path.read_text(encoding='utf-8')
    spec_size = spec_path.stat().st_size
    estimated_tokens = spec_size // 4
    
    print(f"\n📊 Specification Stats:")
    print(f"   • Size: {spec_size / 1024:.2f} KB")
    print(f"   • Estimated Input Tokens: ~{estimated_tokens:,}")
    print(f"   • Estimated Input Cost: ~${estimated_tokens * 0.003 / 1000:.3f}")
    
    # Create output directory
    output_path = Path(OUTPUT_DIR)
    output_path.mkdir(exist_ok=True)
    print(f"📁 Output directory: {OUTPUT_DIR}")
    
    # Prepare system prompt
    system_prompt = """You are an expert Python developer implementing SORA (Specific Operations Risk Assessment) calculations for drone operations according to EASA and JARUS standards.

Your task is to implement the COMPLETE ARC (Air Risk Class) calculation module based on the comprehensive specification provided by the user.

CRITICAL REQUIREMENTS:
1. Read the ENTIRE specification carefully - it contains ALL reference files embedded
2. Follow the CRITICAL IMPLEMENTATION RULES at the top of the document
3. Implement EXACTLY as specified - no shortcuts, no assumptions
4. Create YAML-driven rules (no hardcoded logic)
5. Use Pydantic models for all data structures
6. Include full traceability (rule_ref fields pointing to EASA/JARUS sources)
7. Implement ALL test cases from Section 7
8. Create ALL deliverables from Section 10 checklist

VERIFICATION BEFORE RESPONDING:
✓ Integer classes only (no 0.5 fractional reductions)
✓ DAA NOT in strategic ARC
✓ Thresholds extracted from official sources (no arbitrary values)
✓ Reduction caps enforced (≤1 or ≤2 with certification)
✓ U-space as supporting evidence (not automatic -1)
✓ Geo-fencing as supporting evidence only (0 direct reduction)
✓ Full trace references (doc_id + annex + section)

OUTPUT FORMAT:
Provide complete, working code organized exactly as specified in Section 10:
1. YAML rule files (arc_rules_sora_2_0.yaml, arc_rules_sora_2_5.yaml)
2. Python modules (arc_models.py, arc_calculator.py, arc_validator.py)
3. FastAPI endpoints (arc_api.py)
4. Test files (test_arc_calculator.py with golden tests + property-based tests)
5. README with usage examples

Start with SORA 2.0 implementation, then SORA 2.5."""
    
    # Prepare user prompt
    user_prompt = f"""# ARC (Air Risk Class) Implementation Request

Please implement the complete ARC calculation module according to this specification:

---

{spec_content}

---

## Implementation Instructions

**Step 1: SORA 2.0 Implementation**
1. Create arc_rules_sora_2_0.yaml with Initial ARC determination rules + Strategic mitigation rules
2. Create arc_models.py with Pydantic models (ArcInputs_v2_0, ArcResult, etc.)
3. Create arc_calculator.py with ArcCalculator_v2_0 class
4. Create arc_validator.py with validation logic
5. Create test_arc_calculator_2_0.py with ALL test cases from Section 7

**Step 2: SORA 2.5 Implementation**
1. Create arc_rules_sora_2_5.yaml with enhanced mitigation mechanisms
2. Extend models for SORA 2.5 fields
3. Create ArcCalculator_v2_5 class
4. Create test_arc_calculator_2_5.py

**Step 3: FastAPI Integration**
1. Create arc_api.py with /sora/2.0/arc and /sora/2.5/arc endpoints
2. Include full traceability in responses

**Step 4: Documentation**
1. Create README_ARC.md with usage examples
2. Document all YAML rules
3. Provide example requests/responses

**CRITICAL REMINDERS:**
- ⚠️ Read the "CRITICAL IMPLEMENTATION RULES" section at the top of the specification FIRST
- ⚠️ All 6 reference files are embedded in Section 12 - use them for exact values
- ⚠️ Section 14 lists the redline corrections - make sure you implement them correctly
- ⚠️ Zero tolerance for deviations from EASA/JARUS standards

Please provide the complete implementation now."""
    
    # Initialize Anthropic client
    client = anthropic.Anthropic(api_key=api_key)
    
    print(f"\n🚀 Sending request to Claude Sonnet 4...")
    print(f"   Model: {MODEL}")
    print(f"   Max Output Tokens: {MAX_TOKENS:,}")
    print()
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    
    try:
        # Send request with streaming
        print("⏳ Waiting for response (this may take 1-2 minutes)...")
        print()
        
        message = client.messages.create(
            model=MODEL,
            max_tokens=MAX_TOKENS,
            system=system_prompt,
            messages=[
                {
                    "role": "user",
                    "content": user_prompt
                }
            ]
        )
        
        # Extract text content
        text_content = ""
        for block in message.content:
            if block.type == "text":
                text_content += block.text + "\n\n"
        
        # Save response
        print("✅ Response received!")
        print()
        print("📊 Usage Stats:")
        print(f"   • Input Tokens: {message.usage.input_tokens:,}")
        print(f"   • Output Tokens: {message.usage.output_tokens:,}")
        print(f"   • Total Tokens: {message.usage.input_tokens + message.usage.output_tokens:,}")
        
        input_cost = message.usage.input_tokens * 0.003 / 1000
        output_cost = message.usage.output_tokens * 0.015 / 1000
        total_cost = input_cost + output_cost
        
        print(f"   • Input Cost: ${input_cost:.3f}")
        print(f"   • Output Cost: ${output_cost:.3f}")
        print(f"   • Total Cost: ${total_cost:.3f}")
        print()
        
        # Save implementation
        impl_file = output_path / f"sonnet4_arc_implementation_{timestamp}.md"
        impl_file.write_text(text_content, encoding='utf-8')
        
        print(f"📄 Implementation saved to:")
        print(f"   {impl_file}")
        print()
        
        # Show preview
        print("📝 Preview (first 1000 chars):")
        print("─" * 80)
        print(text_content[:1000])
        print("─" * 80)
        print()
        print(f"💡 Full implementation is in: {impl_file}")
        
        # Save JSON response
        json_file = output_path / f"sonnet4_arc_response_{timestamp}.json"
        with open(json_file, 'w', encoding='utf-8') as f:
            json.dump({
                "id": message.id,
                "model": message.model,
                "role": message.role,
                "content": [{"type": b.type, "text": b.text if hasattr(b, 'text') else None} for b in message.content],
                "stop_reason": message.stop_reason,
                "usage": {
                    "input_tokens": message.usage.input_tokens,
                    "output_tokens": message.usage.output_tokens
                }
            }, f, indent=2)
        
        print()
        print(f"📦 Full JSON response saved to:")
        print(f"   {json_file}")
        
    except Exception as e:
        print(f"❌ API request failed: {e}")
        
        # Save error
        error_file = output_path / f"sonnet4_arc_error_{timestamp}.txt"
        error_file.write_text(str(e), encoding='utf-8')
        print(f"🔍 Error details saved to: {error_file}")
        
        sys.exit(1)
    
    print()
    print(f"✅ COMPLETE! Review the implementation in {OUTPUT_DIR}")
    print()
    print("📋 Next Steps:")
    print("   1. Review the implementation file")
    print("   2. Extract Python files from the response")
    print("   3. Create Backend_Python/arc/ directory structure")
    print("   4. Run tests to verify correctness")
    print("   5. Deploy to FastAPI")

if __name__ == "__main__":
    main()
