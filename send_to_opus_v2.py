import anthropic
import os
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Read the comprehensive fix request
request_file = Path(r'C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\OPUS_FIX_REQUEST_V3_FINAL.md')
with open(request_file, 'r', encoding='utf-8') as f:
    request_text = f.read()

print("\n" + "="*90)
print("🚀 SENDING TO CLAUDE OPUS 4.1 (Streaming Mode)")
print("="*90 + "\n")

# Create client
api_key = os.environ.get('ANTHROPIC_API_KEY')
if not api_key:
    print("❌ ERROR: ANTHROPIC_API_KEY not found in environment!")
    print("Please add it to your .env file")
    exit(1)
    
client = anthropic.Anthropic(api_key=api_key)

# Create output directory
output_dir = Path(r'C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\OPUS_RESPONSE_V2')
output_dir.mkdir(exist_ok=True)

# Stream the response
response_text = ""
with client.messages.stream(
    model="claude-opus-4-20250514",
    max_tokens=16000,
    messages=[
        {"role": "user", "content": request_text}
    ]
) as stream:
    for text in stream.text_stream:
        print(text, end="", flush=True)
        response_text += text

# Save response
output_file = output_dir / "COMPLETE_FIX_ITERATION3_FINAL.txt"
with open(output_file, 'w', encoding='utf-8') as f:
    f.write(response_text)

print("\n\n" + "="*90)
print(f"✅ Response saved to: {output_file}")
print("="*90)
