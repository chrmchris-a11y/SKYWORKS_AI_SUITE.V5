"""
Send documents to Claude Sonnet 4.5 via API
Uses your $18.47 credits from console.anthropic.com
"""

import anthropic
import os

# ΒΗΜΑ 1: Λάβε API key από μεταβλητή περιβάλλοντος
API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

# ΒΗΜΑ 2: Paths των αρχείων που θα στείλεις
FILES_TO_SEND = [
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SUMMARY_FOR_SONNET.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\LATEST_TEST_RESULTS_FOR_SONNET.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\CURRENT_CODE_STATE_FOR_SONNET.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SONNET_FIX_REQUEST_V2_COMPLETE.md",
    r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python\calculations\grc_calculator.py",
]

def read_file(filepath):
    """Read file content"""
    with open(filepath, 'r', encoding='utf-8') as f:
        return f.read()

def send_to_sonnet():
    """Send request to Claude Sonnet 4.5"""
    
    print("🚀 Preparing to send to Claude Sonnet 4.5...")
    print(f"📁 Loading {len(FILES_TO_SEND)} files...")
    
    # Build the prompt with all files
    prompt = """Hi Claude Sonnet 4.5! 

Your previous SORA calculator fix was 65% successful (14/20 tests pass)! 🎉

I've attached 5 documents to help you complete the fix:

"""
    
    # Add each file content
    for i, filepath in enumerate(FILES_TO_SEND, 1):
        filename = os.path.basename(filepath)
        print(f"  [{i}/{len(FILES_TO_SEND)}] Loading {filename}...")
        
        content = read_file(filepath)
        prompt += f"\n\n{'='*80}\n"
        prompt += f"FILE {i}: {filename}\n"
        prompt += f"{'='*80}\n\n"
        prompt += content
    
    # Add final request
    prompt += """

---

🎯 YOUR TASK:

Please provide the COMPLETE corrected version of:
- `Backend_Python/calculations/grc_calculator.py`

With all fixes applied so that all 20 tests pass.

Focus on:
1. Single unified 7×5 table (not two separate tables)
2. Correct table values (verify with the test expectations)
3. Floor enforcement working correctly
4. OUT_OF_SCOPE check AFTER mitigations (not before)

Please provide the FULL corrected file content. Thank you! 🚀
"""
    
    print(f"\n📊 Total prompt size: {len(prompt):,} characters")
    print(f"💰 Estimated cost: ~${len(prompt) * 0.000003:.4f}")
    
    # Initialize client
    print("\n🔑 Connecting to Anthropic API...")
    client = anthropic.Anthropic(api_key=API_KEY)
    
    # Send request
    print("📤 Sending request to Claude Sonnet 4.5...")
    print("⏳ This may take 30-60 seconds...\n")
    
    try:
        message = client.messages.create(
            model="claude-sonnet-4-20250514",  # Claude Sonnet 4.5 latest
            max_tokens=8000,
            temperature=0,
            messages=[
                {"role": "user", "content": prompt}
            ]
        )
        
        # Get response
        response_text = message.content[0].text
        
        print("✅ SUCCESS! Response received!\n")
        print("="*80)
        print("CLAUDE SONNET 4.5 RESPONSE:")
        print("="*80)
        print(response_text)
        print("="*80)
        
        # Save response to file
        output_file = r"C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\SONNET_RESPONSE.txt"
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(response_text)
        
        print(f"\n💾 Response saved to: {output_file}")
        
        # Show usage
        print(f"\n📊 Usage Statistics:")
        print(f"   Input tokens: {message.usage.input_tokens:,}")
        print(f"   Output tokens: {message.usage.output_tokens:,}")
        print(f"   Total tokens: {message.usage.input_tokens + message.usage.output_tokens:,}")
        print(f"   Cost: ~${(message.usage.input_tokens * 0.000003 + message.usage.output_tokens * 0.000015):.4f}")
        
        return response_text
        
    except anthropic.APIError as e:
        print(f"❌ API Error: {e}")
        return None
    except Exception as e:
        print(f"❌ Error: {e}")
        return None

if __name__ == "__main__":
    print("="*80)
    print("CLAUDE SONNET 4.5 - GRC CALCULATOR FIX REQUEST")
    print("="*80)
    send_to_sonnet()
