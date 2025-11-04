#!/usr/bin/env python3
"""
Send FINAL COMPLETE FIX REQUEST to Claude Sonnet 4.5
"""

import anthropic
import sys
import os

API_KEY = os.getenv("ANTHROPIC_API_KEY")
if not API_KEY:
    raise RuntimeError("ANTHROPIC_API_KEY is not set. Please configure it in your environment.")

def send_request():
    print("="*80)
    print("CLAUDE SONNET 4.5 - FINAL COMPLETE FIX REQUEST")
    print("="*80)
    
    # Read the request file
    print("📁 Loading FINAL_COMPLETE_FIX_REQUEST_FOR_SONNET.md...")
    with open("FINAL_COMPLETE_FIX_REQUEST_FOR_SONNET.md", "r", encoding="utf-8") as f:
        request_content = f.read()
    
    print(f"📊 Request size: {len(request_content):,} characters")
    
    # Create client
    print("🔑 Connecting to Anthropic API...")
    client = anthropic.Anthropic(api_key=API_KEY)
    
    # Send request
    print("📤 Sending request to Claude Sonnet 4.5...")
    print("⏳ This may take 60-90 seconds for complete files...")
    print()
    
    response = client.messages.create(
        model="claude-sonnet-4-20250514",
        max_tokens=8000,
        messages=[{
            "role": "user",
            "content": request_content
        }]
    )
    
    # Extract response
    response_text = response.content[0].text
    
    print("✅ SUCCESS! Response received!")
    print()
    print("="*80)
    print("CLAUDE SONNET 4.5 RESPONSE:")
    print("="*80)
    print(response_text)
    print("="*80)
    print()
    
    # Save response
    output_file = "SONNET_FINAL_RESPONSE.txt"
    with open(output_file, "w", encoding="utf-8") as f:
        f.write(response_text)
    
    print(f"💾 Response saved to: {output_file}")
    print()
    
    # Show usage
    print("📊 Usage Statistics:")
    print(f"   Input tokens: {response.usage.input_tokens:,}")
    print(f"   Output tokens: {response.usage.output_tokens:,}")
    print(f"   Total tokens: {response.usage.input_tokens + response.usage.output_tokens:,}")
    
    return 0

if __name__ == "__main__":
    try:
        sys.exit(send_request())
    except Exception as e:
        print(f"❌ ERROR: {e}")
        sys.exit(1)
