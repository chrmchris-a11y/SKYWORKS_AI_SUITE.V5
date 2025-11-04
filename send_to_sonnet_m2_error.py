#!/usr/bin/env python3
"""
Send M2 Python API Error details to Claude Sonnet 4.5 for analysis and fix
"""

import anthropic
import os
from datetime import datetime

def send_to_sonnet_45():
    # Read the problem report
    with open('SONNET_M2_PYTHON_API_ERROR.md', 'r', encoding='utf-8') as f:
        problem_report = f.read()
    
    # Initialize Anthropic client
    client = anthropic.Anthropic(
        api_key=os.environ.get("ANTHROPIC_API_KEY")
    )
    
    print("\n" + "="*80)
    print("🚀 SENDING M2 PYTHON API ERROR TO CLAUDE SONNET 4.5")
    print("="*80 + "\n")
    
    # Create the message
    message = client.messages.create(
        model="claude-sonnet-4-20250514",  # Sonnet 4.5
        max_tokens=8000,
        temperature=0.2,
        messages=[
            {
                "role": "user",
                "content": f"""You are an expert C# and Python API integration specialist. 

I have a critical bug in my SORA assessment system where:
1. ✅ Backend PowerShell tests work perfectly (M2=-1 correct)
2. ❌ Frontend UI fails with "API Validation Error" 
3. ❌ Python API rejects requests: "Field required: m1, m3"

Below is the COMPLETE problem analysis with:
- Current system state
- Exact error messages
- Backend C# code (SoraProxyController.cs)
- Python API expectations
- Root cause analysis
- Proposed fixes

**PLEASE PROVIDE:**
1. **Fixed SnakeCaseNamingPolicy** (handles underscores correctly)
2. **Fixed ExtractSora20FromMissionHtml** (initialize M1=0, M3=0)
3. **Any other critical fixes needed**
4. **Complete code snippets ready to paste**

---

{problem_report}

---

**CRITICAL REQUIREMENTS:**
- M2 must display as -1 (not -2) for "Low" robustness
- All fixes must be compatible with existing Python API
- Maintain null-safety throughout
- Use snake_case for Python API communication
- Ensure mission.html UI works end-to-end

**PLEASE PROVIDE COMPLETE, PRODUCTION-READY CODE SNIPPETS!**
"""
            }
        ]
    )
    
    # Extract response
    response_text = message.content[0].text
    
    # Save to file with timestamp
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    output_file = f"SONNET_45_M2_FIX_RESPONSE_{timestamp}.txt"
    
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write("="*80 + "\n")
        f.write("CLAUDE SONNET 4.5 RESPONSE - M2 PYTHON API ERROR FIX\n")
        f.write(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write("="*80 + "\n\n")
        f.write(response_text)
    
    print(f"\n✅ Response saved to: {output_file}")
    print(f"📄 Response length: {len(response_text)} characters")
    print("\n" + "="*80)
    print("SONNET 4.5 RESPONSE:")
    print("="*80 + "\n")
    print(response_text)
    print("\n" + "="*80)
    print(f"✅ COMPLETE! Check {output_file} for full response")
    print("="*80 + "\n")
    
    return response_text

if __name__ == "__main__":
    try:
        send_to_sonnet_45()
    except Exception as e:
        print(f"\n❌ ERROR: {e}")
        import traceback
        traceback.print_exc()
