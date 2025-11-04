# 🎯 Quick Start: Send ARC to Sonnet 4

## ⚡ Fast Track (2 minutes)

### Step 1: Set API Key
```powershell
$env:ANTHROPIC_API_KEY = "sk-ant-api03-YOUR-KEY-HERE"
```

### Step 2: Run Script
```powershell
.\send_arc_to_sonnet4.ps1
```

### Step 3: Wait ~2 minutes ☕

### Step 4: Check Output
```powershell
cd SONNET4_ARC_OUTPUT
ls | sort LastWriteTime -Descending | select -First 1
```

---

## 📊 What You Get

```
SONNET4_ARC_OUTPUT/
└── sonnet4_arc_implementation_YYYYMMDD_HHMMSS.md
    ├── arc_rules_sora_2_0.yaml          # Initial ARC + Strategic Mitigations
    ├── arc_rules_sora_2_5.yaml          # Enhanced Mechanisms
    ├── arc_models.py                    # Pydantic models
    ├── arc_calculator.py                # Calculator classes (2.0 & 2.5)
    ├── arc_validator.py                 # Validation logic
    ├── arc_api.py                       # FastAPI endpoints
    ├── test_arc_calculator_2_0.py       # Golden tests SORA 2.0
    ├── test_arc_calculator_2_5.py       # Golden tests SORA 2.5
    └── README_ARC.md                    # Usage documentation
```

---

## 💰 Cost

- **Input**: 13,750 tokens = **$0.041**
- **Output**: ~16,000 tokens = **$0.240**
- **Total**: **~$0.28**

---

## ✅ Quality Guarantees

The implementation will have:

1. ✅ **Integer Classes Only** (no 0.5 fractional)
2. ✅ **No DAA in Strategic ARC** (tactical only)
3. ✅ **No Arbitrary Thresholds** (extracted from sources)
4. ✅ **Caps Enforced** (≤1 or ≤2 with certification)
5. ✅ **U-space as Evidence** (not automatic -1)
6. ✅ **Geo-fencing Supporting Only** (0 direct reduction)
7. ✅ **Full Trace References** (doc + annex + section)

---

## 🔍 Verify Implementation

```bash
# 1. Check files created
grep -E "^(def|class)" arc_calculator.py

# 2. Look for YAML rules
cat arc_rules_sora_2_0.yaml | head -20

# 3. Check test cases
grep "def test_" test_arc_calculator_2_0.py
```

---

## 🚀 Deploy

```bash
# 1. Create directory
mkdir -p Backend_Python/arc/{models,calculators,validators,rules,tests}

# 2. Copy files
cp arc_*.py Backend_Python/arc/
cp arc_rules_*.yaml Backend_Python/arc/rules/
cp test_*.py Backend_Python/arc/tests/

# 3. Install deps
pip install pydantic pyyaml fastapi hypothesis pytest

# 4. Run tests
pytest Backend_Python/arc/tests/ -v

# 5. Add to FastAPI
# Edit Backend_Python/main.py to include arc_api routes
```

---

## ⚠️ Troubleshooting

| Error | Solution |
|-------|----------|
| API key not found | `$env:ANTHROPIC_API_KEY = "sk-ant-..."` |
| Rate limit | Wait 60 seconds, try again |
| Invalid key | Check key from console.anthropic.com |
| Module not found (Python) | `pip install anthropic` |

---

## 📚 Full Documentation

See: `SEND_TO_SONNET4_README.md`

---

**Ready?** 🎯

```powershell
.\send_arc_to_sonnet4.ps1
```
