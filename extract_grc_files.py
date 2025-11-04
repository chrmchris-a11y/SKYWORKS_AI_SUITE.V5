"""Extract GRC implementation files from Sonnet 4 response"""
import re
from pathlib import Path

# Read response
response_file = Path("SONNET_GRC_RESPONSE_20251030_155759.txt")
content = response_file.read_text(encoding="utf-8")

print(f"📄 Response size: {len(content):,} characters\n")

# Create directory structure
grc_dir = Path("Backend_Python/grc")
for subdir in ["models", "calculators", "validators", "rules", "tests"]:
    (grc_dir / subdir).mkdir(parents=True, exist_ok=True)
    print(f"✅ Created directory: {grc_dir / subdir}")

# Create __init__.py files
init_content = '"""GRC (Ground Risk Class) calculation module for SORA 2.0 and 2.5"""'
for subdir in ["", "models", "calculators", "validators", "tests"]:
    init_file = grc_dir / subdir / "__init__.py"
    init_file.write_text(init_content, encoding="utf-8")
    print(f"✅ Created: {init_file}")

print("\n📂 Extracting files...")

# Extract files using regex
files_to_extract = [
    ("grc_rules_sora_2_0.yaml", "rules/grc_rules_sora_2_0.yaml", "yaml"),
    ("grc_rules_sora_2_5.yaml", "rules/grc_rules_sora_2_5.yaml", "yaml"),
    ("models/grc_models.py", "models/grc_models.py", "python"),
    ("validators/grc_validator.py", "validators/grc_validator.py", "python"),
    ("calculators/grc_calculator.py", "calculators/grc_calculator.py", "python"),
    ("grc_api.py", "grc_api.py", "python"),
    ("tests/test_grc_calculator_2_0.py", "tests/test_grc_calculator_2_0.py", "python"),
    ("tests/test_grc_calculator_2_5.py", "tests/test_grc_calculator_2_5.py", "python"),
]

for marker, target_path, language in files_to_extract:
    print(f"  Processing: {marker}")
    
    # Pattern: ### `marker`\n```language\n...code...```
    pattern = rf"### `{re.escape(marker)}`\s+```{language}\s+(.*?)```"
    match = re.search(pattern, content, re.DOTALL)
    
    if match:
        code = match.group(1).strip()
        output_file = grc_dir / target_path
        output_file.write_text(code, encoding="utf-8")
        lines = code.count("\n") + 1
        print(f"    ✅ Extracted {lines} lines to {output_file}")
    else:
        print(f"    ⚠️  Could not find code block for {marker}")

print("\n✅ Extraction complete!\n")
print("📊 Summary:")
for file in sorted(grc_dir.rglob("*")):
    if file.is_file():
        rel_path = file.relative_to(Path.cwd())
        size = file.stat().st_size
        print(f"  {rel_path} ({size:,} bytes)")

print("\n📝 Next steps:")
print("  1. Review extracted files for any issues")
print("  2. Fix any Pydantic V2 compatibility issues")
print("  3. Run tests: pytest grc/tests/ -v")
