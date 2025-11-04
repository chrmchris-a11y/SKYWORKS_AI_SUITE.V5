#!/usr/bin/env python3
"""
Simple script to send a single markdown file to Claude Sonnet 4.5
Usage: python send_single_file.py <filename.md>
"""

import sys
import os
import anthropic
from pathlib import Path

def send_to_claude(file_path: str):
    """Send a single markdown file to Claude Sonnet 4.5"""
    
    # Validate file exists
    if not os.path.exists(file_path):
        print(f"❌ Error: File '{file_path}' not found!")
        sys.exit(1)
    
    # Read file content
    print(f"📁 Reading {file_path}...")
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    print(f"📊 File size: {len(content):,} characters")
    
    # Get API key from environment
    api_key = os.environ.get("ANTHROPIC_API_KEY")
    if not api_key:
        print("❌ Error: ANTHROPIC_API_KEY environment variable not set!")
        sys.exit(1)
    
    # Initialize Anthropic client
    print("🔑 Connecting to Anthropic API...")
    client = anthropic.Anthropic(api_key=api_key)
    
    # Send request
    print("📤 Sending request to Claude Sonnet 4.5...")
    print("⏳ This may take 30-60 seconds...\n")
    
    try:
        message = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=8192,
            messages=[{
                "role": "user",
                "content": content
            }]
        )
        
        response_text = message.content[0].text
        
        print("=" * 80)
        print("CLAUDE SONNET 4.5 RESPONSE:")
        print("=" * 80)
        print(response_text)
        print("=" * 80)
        
        # Save response
        response_file = "SONNET_RESPONSE_CSHARP_FIX.txt"
        with open(response_file, 'w', encoding='utf-8') as f:
            f.write(response_text)
        
        print(f"\n💾 Response saved to: {os.path.abspath(response_file)}")
        
        # Print usage stats
        print(f"\n📊 Usage Statistics:")
        print(f"   Input tokens: {message.usage.input_tokens:,}")
        print(f"   Output tokens: {message.usage.output_tokens:,}")
        print(f"   Total tokens: {message.usage.input_tokens + message.usage.output_tokens:,}")
        
        return response_text
        
    except anthropic.APIError as e:
        print(f"❌ API Error: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python send_single_file.py <filename.md>")
        sys.exit(1)
    
    file_path = sys.argv[1]
    send_to_claude(file_path)
