#!/usr/bin/env python3
"""
Send complete fix request to Claude Sonnet 4.5
Reads SONNET_COMPLETE_FIX_REQUEST_FINAL.md and sends to Anthropic API
"""

import anthropic
import os
from datetime import datetime

def send_to_sonnet():
    # Read the comprehensive problem report
    with open('SONNET_COMPLETE_FIX_REQUEST_FINAL.md', 'r', encoding='utf-8') as f:
        problem_report = f.read()
    
    # Read current mission.html for context
    with open('Frontend/Pages/mission.html', 'r', encoding='utf-8') as f:
        mission_html_content = f.read()
    
    # Read current SoraProxyController.cs for context
    with open('Backend/src/Skyworks.Api/Controllers/SoraProxyController.cs', 'r', encoding='utf-8') as f:
        controller_content = f.read()
    
    # Get API key from environment
    api_key = os.environ.get('ANTHROPIC_API_KEY')
    if not api_key:
        print("❌ ERROR: ANTHROPIC_API_KEY environment variable not set!")
        print("Please run: $env:ANTHROPIC_API_KEY = 'your-key-here'")
        return
    
    client = anthropic.Anthropic(api_key=api_key)
    
    print("📤 Sending complete fix request to Claude Sonnet 4.5...")
    print("=" * 80)
    
    # Construct comprehensive message
    full_message = f"""
{problem_report}

---

## CURRENT CODE STATE (For Your Reference)

### File 1: mission.html (Lines 2464-2500 - M2 Mapping Section)

```javascript
// Build SORA 2.0 mitigations from structured dropdowns
const mitigations20 = [];
const m1 = parseInt(document.getElementById('m1_20')?.value || '0');
const m2 = parseInt(document.getElementById('m2_20')?.value || '0');
const m3 = parseInt(document.getElementById('m3_20')?.value || '1');

// Map numeric values back to robustness levels for API
if (m1 === -1) mitigations20.push({{ Type: 'M1', Robustness: 'Low' }});
else if (m1 === -2) mitigations20.push({{ Type: 'M1', Robustness: 'Medium' }});
else if (m1 === -4) mitigations20.push({{ Type: 'M1', Robustness: 'High' }});

// ❌ WRONG MAPPING HERE!
if (m2 === -1) mitigations20.push({{ Type: 'M2', Robustness: 'Medium' }});  // Should be 'Low'!
else if (m2 === -2) mitigations20.push({{ Type: 'M2', Robustness: 'High' }}); // Should be 'Medium'!

if (m3 === 0) mitigations20.push({{ Type: 'M3', Robustness: 'Medium' }});
else if (m3 === -1) mitigations20.push({{ Type: 'M3', Robustness: 'High' }});

body.GroundRisk = {{
    Scenario_V2_0: document.getElementById('scenario20').value,
    MaxCharacteristicDimension: Number(document.getElementById('maxDimension20').value || 0),
    KineticEnergy: ke20 !== undefined ? Math.round(ke20) : undefined,
    Mitigations: mitigations20,
    IsControlledGroundArea: document.getElementById('controlledGround20')?.value === 'true' || false
    // ❌ MISSING: PopulationDensity and Sheltering
}};
```

### File 2: mission.html (Lines 1695-1740 - calculateGRC20 Function)

```javascript
function calculateGRC20() {{
    const initialGRC = Number(document.getElementById('initialGRC20').value) || 1;
    const m1 = Number(document.getElementById('m1_20').value) || 0;
    const m2 = Number(document.getElementById('m2_20').value) || 0;
    const m3 = Number(document.getElementById('m3_20').value) || 1;
    
    // ✅ FIXED: Use m2_value from API response if available
    const m2Value = (window.soraResult && window.soraResult.m2_value !== undefined) 
        ? window.soraResult.m2_value 
        : m2;
    
    const totalReduction = m1 + m2Value + m3;
    const finalGRC = initialGRC + totalReduction;
    
    const calcDetails = document.getElementById('calculationDetails20');
    if (!calcDetails) return;
    
    // ✅ FIXED: Safe m2 label retrieval
    const m2Label = (window.soraResult && window.soraResult.m2) ? window.soraResult.m2 : 'Impact reduction';
    
    calcDetails.innerHTML = `
        Initial GRC: <strong>${{initialGRC}}</strong><br>
        M1 (Strategic): <strong>${{m1}}</strong> GRC<br>
        M2 (${{m2Label}}): <strong>${{m2Value}}</strong> GRC<br>
        M3 (ERP): <strong>${{m3}}</strong> GRC<br>
        <hr>
        Total Reduction: <strong>${{totalReduction}}</strong> GRC<br>
        <strong>Final GRC: ${{finalGRC}}</strong>
    `;
}}
```

### File 3: SoraProxyController.cs (Lines 153-274 - ExtractSora20FromMissionHtml)

```csharp
private SoraCompleteRequest ExtractSora20FromMissionHtml(JsonElement json)
{{
    var request = new SoraCompleteRequest {{ Category = "SORA-2.0" }};
    
    // ✅ FIXED: Initialize to prevent "Field required" errors
    request.M1 = 0;
    request.M3 = 0;

    if (!json.TryGetProperty("GroundRisk", out var groundRisk))
    {{
        throw new ArgumentException("GroundRisk is required for SORA 2.0");
    }}

    // ⚠️ ISSUE: mission.html doesn't send PopulationDensity
    if (groundRisk.TryGetProperty("PopulationDensity", out var popDensity))
    {{
        var popValue = popDensity.GetDouble();
        request.PopulationDensity = popValue < 500 ? "Low" : 
                                  popValue < 10000 ? "Medium" : "High";
    }}
    else
    {{
        // Current fallback - should this throw error instead?
        _logger.LogWarning("PopulationDensity missing, defaulting to Low");
        request.PopulationDensity = "Low";
    }}

    // ⚠️ ISSUE: mission.html doesn't send Sheltering
    request.Sheltering = ExtractSheltering(groundRisk) ?? "High";

    // ... rest of extraction
}}
```

---

## YOUR TASK (Claude Sonnet 4.5)

**Please provide COMPLETE, READY-TO-USE code that I can directly copy-paste to fix all issues.**

**Specifically, I need:**

1. **Complete JavaScript code for mission.html M2 mapping fix**
   - Show the ENTIRE section I need to replace (with sufficient context lines)
   - Include the corrected mapping: -1→Low, -2→Medium, -3→High
   - Add PopulationDensity and Sheltering to GroundRisk object

2. **Complete JavaScript code for calculateGRC20() improvements**
   - Safe handling when window.soraResult not defined
   - Proper display of M2 label and value

3. **Complete C# code for SoraProxyController.cs error handling**
   - Better error messages (not empty objects)
   - Clear distinction between validation and server errors

4. **Step-by-step instructions for applying fixes**
   - Exact line numbers where changes should be made
   - Before/after code comparisons
   - Testing procedure after changes

5. **Any additional HTML changes needed**
   - If Sheltering dropdown should be added
   - If PopulationDensity field should be exposed

**CRITICAL REQUIREMENTS:**
- Code must be production-ready (no placeholders, no TODO comments)
- Must work with existing Backend and Python API (no breaking changes)
- Must be 100% JARUS SORA 2.0 compliant
- Must fix the "megalo sfalma" bug: M2=Low should display -1 (not -2)

**Thank you! Please provide the complete solution now.** 🙏
"""
    
    try:
        # Send to Claude Sonnet 4.5
        message = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=16000,
            temperature=0,
            system="""You are an expert full-stack developer specializing in C# .NET 8.0 and JavaScript.
You provide complete, production-ready code solutions with proper error handling and null-safety.
Always include sufficient context lines (5+ lines before/after) when showing code changes.
Never use placeholders or TODO comments - provide complete working code.""",
            messages=[
                {
                    "role": "user",
                    "content": full_message
                }
            ]
        )
        
        # Extract response
        response_text = message.content[0].text
        
        # Save response to file with timestamp
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        output_file = f'SONNET_COMPLETE_FIX_RESPONSE_FINAL_{timestamp}.txt'
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(f"=== CLAUDE SONNET 4.5 RESPONSE ===\n")
            f.write(f"Timestamp: {datetime.now().isoformat()}\n")
            f.write(f"Model: claude-sonnet-4-20250514\n")
            f.write(f"Request: Complete SORA 2.0/2.5 Integration Fix\n")
            f.write("=" * 80 + "\n\n")
            f.write(response_text)
        
        print(f"\n✅ SUCCESS! Response saved to: {output_file}")
        print("=" * 80)
        print("\n📄 RESPONSE PREVIEW:\n")
        print(response_text[:2000])
        print("\n... (truncated) ...")
        print(f"\n💾 Full response: {output_file}")
        print(f"📊 Response length: {len(response_text)} characters")
        
        return response_text
        
    except anthropic.APIError as e:
        print(f"❌ API Error: {e}")
        return None
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        import traceback
        traceback.print_exc()
        return None

if __name__ == '__main__':
    print("🚀 Sending complete fix request to Claude Sonnet 4.5...")
    print("📋 Reading comprehensive problem report...")
    print()
    
    response = send_to_sonnet()
    
    if response:
        print("\n✅ Mission accomplished! Check the response file for complete fixes.")
        print("🎯 Next: Apply the fixes to mission.html and SoraProxyController.cs")
    else:
        print("\n❌ Failed to get response. Check error messages above.")
