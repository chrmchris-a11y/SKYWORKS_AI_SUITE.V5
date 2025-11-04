"""
SKYWORKS AI Suite - Send Table 2 Analysis to Claude Sonnet 4
=============================================================

This script prepares and sends the complete analysis package to Claude Sonnet 4
for expert review of the SORA Table 2 discrepancy.

Usage:
    python send_to_sonnet4_table2.py

Requires:
    pip install anthropic
    Set environment variable: ANTHROPIC_API_KEY
"""

import os
import sys
from datetime import datetime

def read_file(filepath):
    """Read file content safely"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            return f.read()
    except Exception as e:
        print(f"⚠️  Warning: Could not read {filepath}: {e}")
        return None

def create_analysis_package():
    """Gather all files for Sonnet 4 analysis"""
    
    print("📦 Preparing analysis package for Claude Sonnet 4...\n")
    
    base_path = os.path.dirname(__file__)
    
    # Core analysis request
    analysis_request = read_file(os.path.join(base_path, "SONNET_TABLE2_ANALYSIS_REQUEST.md"))
    
    # Python backend files
    python_main = read_file(os.path.join(base_path, "Backend_Python", "main.py"))
    
    # .NET reference files  
    dotnet_reference = read_file(os.path.join(base_path, "Backend", "JARUS_25_TABLE2_REFERENCE.ps1"))
    
    # Test file showing the problem
    test_file = read_file(os.path.join(base_path, "test_sora_direct.py"))
    
    # Collaboration guide
    collab_guide = read_file(os.path.join(base_path, "HOW_TO_COLLABORATE_WITH_SONNET4.md"))
    
    # Build comprehensive prompt
    prompt = f"""# SKYWORKS AI SUITE - CRITICAL TABLE 2 ANALYSIS REQUEST

**Analysis Date:** {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}
**Priority:** HIGH - Blocking accurate SORA calculations
**Requester:** SKYWORKS Development Team via GitHub Copilot

---

## PROBLEM SUMMARY

The SKYWORKS AI Suite has two different implementations of "JARUS SORA Table 2":

1. **Python Backend**: Uses SORA 2.0 MTOM-based 5×4 matrix
2. **.NET Backend**: Uses JARUS SORA 2.5 Dimension+Speed-based 7×5 matrix

This causes calculation discrepancies and test failures. We need your expert analysis to:

1. Identify which Table 2 belongs to which SORA version (from official JARUS docs)
2. Determine if both backends are correct (for their respective versions)
3. Provide specific code fixes with exact line numbers
4. Recommend implementation strategy (dual support vs migration)

---

## DETAILED ANALYSIS REQUEST

{analysis_request}

---

## CURRENT PYTHON IMPLEMENTATION (Backend_Python/main.py)

```python
{python_main[:3000] if python_main else "File not found"}
```

[Full file: 617 lines - see attached]

---

## OFFICIAL JARUS SORA 2.5 REFERENCE (Backend/JARUS_25_TABLE2_REFERENCE.ps1)

```powershell
{dotnet_reference if dotnet_reference else "File not found"}
```

---

## TEST SHOWING THE PROBLEM (test_sora_direct.py)

Test #4 fails:
- Input: 0.8kg drone, 8000 ppl/km²
- Expected: iGRC=5 (based on SORA 2.5 logic)
- Got: iGRC=4 (Python returns SORA 2.0 result)

```python
{test_file[:2000] if test_file else "File not found"}
```

---

## COLLABORATION GUIDE

{collab_guide}

---

## WHAT WE NEED FROM YOU

Please provide a comprehensive analysis with:

### 1. OFFICIAL SPECIFICATION VERIFICATION
- Quote exact Table 2 from JARUS SORA 2.0 official docs
- Quote exact Table 2 from JARUS SORA 2.5 official docs  
- Confirm which structure (MTOM vs Dimension+Speed) belongs to which version

### 2. CODE CORRECTNESS ASSESSMENT
- Is Python backend correct for SORA 2.0? ✅/❌
- Is .NET backend correct for SORA 2.5? ✅/❌
- Line-by-line analysis of discrepancies

### 3. RECOMMENDED SOLUTION
Choose ONE and provide detailed implementation:

**Option A: Dual Implementation**
- Keep both Table 2 versions
- Separate classes: `SORAv2_0_Table2` and `JARUSSORAv2_5_Table2`
- Clear endpoint separation: `/grc/2.0` vs `/grc/2.5`

**Option B: Migrate to SORA 2.5 Only**
- Replace MTOM logic with Dimension+Speed logic
- Deprecate SORA 2.0 endpoints
- Migration guide for existing data

**Option C: Current Code is Correct**
- Tests have wrong expectations
- Just fix test expected values
- Document why values are correct

### 4. DETAILED IMPLEMENTATION PLAN
- Specific file paths to edit
- Exact line numbers to change
- Before/after code snippets
- Updated test expectations
- Verification steps

---

## CRITICAL QUESTIONS

1. **Does SORA 2.0 use MTOM-based Table 2?** (5×4 matrix with weight categories)
2. **Does JARUS SORA 2.5 use Dimension+Speed-based Table 2?** (7×5 matrix)
3. **Are both valid simultaneously?** (Different SORA versions, both correct)
4. **What is the migration path?** (If SORA 2.5 supersedes 2.0)

---

## DELIVERABLES REQUESTED

1. **Table 2 Specification Document** (from official JARUS sources)
2. **Code Analysis Report** (line-by-line review with corrections)
3. **Recommended Solution** (Option A, B, or C with justification)
4. **Implementation Code** (exact Python code changes)
5. **Updated Test Suite** (correct expected values)
6. **Verification Plan** (how to prove fix is correct)

---

**Please provide your analysis with high confidence and cite official JARUS documentation for all claims.**

Thank you!

---
**Prepared by:** GitHub Copilot
**For:** Claude Sonnet 4 Expert Analysis
**Project:** SKYWORKS AI Suite V5
"""

    return prompt

def send_to_sonnet4_api(prompt):
    """Send analysis request to Claude Sonnet 4 via API"""
    
    try:
        import anthropic
    except ImportError:
        print("❌ Error: anthropic package not installed")
        print("   Run: pip install anthropic")
        return None
    
    api_key = os.environ.get("ANTHROPIC_API_KEY")
    if not api_key:
        print("❌ Error: ANTHROPIC_API_KEY environment variable not set")
        print("   Set it with: $env:ANTHROPIC_API_KEY = 'your-key-here'")
        return None
    
    print("🚀 Sending analysis request to Claude Sonnet 4...")
    print(f"📊 Prompt length: {len(prompt)} characters\n")
    
    try:
        client = anthropic.Anthropic(api_key=api_key)
        
        response = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=8192,
            temperature=0.3,  # Lower temperature for precise technical analysis
            messages=[{
                "role": "user",
                "content": prompt
            }]
        )
        
        return response.content[0].text
        
    except Exception as e:
        print(f"❌ Error calling Anthropic API: {e}")
        return None

def save_response(response):
    """Save Sonnet 4's response to file"""
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    filename = f"SONNET4_TABLE2_ANALYSIS_{timestamp}.md"
    
    with open(filename, 'w', encoding='utf-8') as f:
        f.write(f"# CLAUDE SONNET 4 - SORA TABLE 2 ANALYSIS\n\n")
        f.write(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write(f"**Model:** Claude Sonnet 4\n")
        f.write(f"**Purpose:** SKYWORKS SORA Table 2 Discrepancy Analysis\n\n")
        f.write("---\n\n")
        f.write(response)
    
    print(f"\n✅ Response saved to: {filename}")
    return filename

def main():
    """Main execution"""
    
    print("╔═══════════════════════════════════════════════════════════════════════════╗")
    print("  SKYWORKS AI SUITE - SEND TO CLAUDE SONNET 4")
    print("  Table 2 Discrepancy Analysis Request")
    print("╚═══════════════════════════════════════════════════════════════════════════╝\n")
    
    # Step 1: Create analysis package
    prompt = create_analysis_package()
    
    # Step 2: Save prompt for manual use if API fails
    with open("PROMPT_FOR_SONNET4.txt", 'w', encoding='utf-8') as f:
        f.write(prompt)
    print("✅ Prompt saved to: PROMPT_FOR_SONNET4.txt\n")
    
    # Step 3: Attempt API call
    print("Choose delivery method:")
    print("1. Send via Anthropic API (requires API key)")
    print("2. Copy prompt to clipboard (paste manually in claude.ai)")
    print("3. Just save prompt to file (already done)")
    
    choice = input("\nEnter choice (1/2/3): ").strip()
    
    if choice == "1":
        response = send_to_sonnet4_api(prompt)
        if response:
            print("\n✅ Received response from Claude Sonnet 4!")
            print(f"📊 Response length: {len(response)} characters\n")
            filename = save_response(response)
            print(f"\n🎉 Analysis complete! Review: {filename}")
        else:
            print("\n❌ API call failed. Use prompt file for manual submission.")
    
    elif choice == "2":
        try:
            import pyperclip
            pyperclip.copy(prompt)
            print("\n✅ Prompt copied to clipboard!")
            print("📋 Paste it in: https://claude.ai/new")
        except ImportError:
            print("\n⚠️  pyperclip not installed. Prompt saved to file instead.")
            print("   Install with: pip install pyperclip")
    
    else:
        print("\n✅ Prompt ready in: PROMPT_FOR_SONNET4.txt")
        print("📝 Copy/paste it manually to Claude Sonnet 4")
    
    print("\n" + "="*79)
    print("NEXT STEPS:")
    print("="*79)
    print("1. Wait for Claude Sonnet 4's analysis response")
    print("2. Review recommended solution (Option A/B/C)")
    print("3. Implement code changes per Sonnet 4's instructions")
    print("4. Re-run: python test_sora_direct.py")
    print("5. Verify 100% pass rate (20/20 tests)")
    print("="*79 + "\n")

if __name__ == "__main__":
    main()
