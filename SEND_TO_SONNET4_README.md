# 🚀 Send ARC Specification to Sonnet 4

## Overview

Αυτά τα scripts στέλνουν το **ARC_IMPLEMENTATION_SPECIFICATION.md** στο Claude Sonnet 4 για να δημιουργήσει την πλήρη υλοποίηση των ARC υπολογισμών.

## 📋 Prerequisites

### 1. Anthropic API Key

Πρέπει να έχεις ένα Anthropic API key. Πάρε το από: https://console.anthropic.com/

### 2. Set Environment Variable

**PowerShell:**
```powershell
$env:ANTHROPIC_API_KEY = "sk-ant-api03-..."
```

**Bash/Linux:**
```bash
export ANTHROPIC_API_KEY="sk-ant-api03-..."
```

**Για μόνιμη αποθήκευση (Windows):**
```powershell
[System.Environment]::SetEnvironmentVariable('ANTHROPIC_API_KEY', 'sk-ant-api03-...', 'User')
```

### 3. Python Requirements (για Python script)

```bash
pip install anthropic
```

## 🎯 Usage

### Option A: PowerShell Script (Recommended για Windows)

```powershell
.\send_arc_to_sonnet4.ps1
```

**Με custom παραμέτρους:**
```powershell
.\send_arc_to_sonnet4.ps1 -MaxTokens 20000 -OutputDir "MY_OUTPUT"
```

### Option B: Python Script (Cross-platform)

```bash
python send_arc_to_sonnet4.py
```

## 📊 What Happens

1. **Reads Specification**: Διαβάζει το ARC_IMPLEMENTATION_SPECIFICATION.md (1,563 lines, 55KB)
2. **Estimates Cost**: Υπολογίζει ~13,750 tokens input (~$0.41)
3. **Sends to Sonnet 4**: Στέλνει με detailed instructions
4. **Receives Implementation**: Παίρνει complete Python code
5. **Saves Output**: Αποθηκεύει σε `SONNET4_ARC_OUTPUT/`

## 📁 Output Files

```
SONNET4_ARC_OUTPUT/
├── sonnet4_arc_implementation_YYYYMMDD_HHMMSS.md  # Main implementation
├── sonnet4_arc_response_YYYYMMDD_HHMMSS.json      # Full API response
└── sonnet4_arc_error_YYYYMMDD_HHMMSS.txt          # Error log (if failed)
```

## 💰 Cost Estimate

- **Input**: ~13,750 tokens × $0.003/1K = **$0.041**
- **Output**: ~16,000 tokens × $0.015/1K = **$0.240** (estimated)
- **Total**: ~**$0.28 per request**

## 🔍 What Sonnet 4 Will Create

Το Sonnet 4 θα δημιουργήσει:

### 1. YAML Rule Files
- `arc_rules_sora_2_0.yaml` - Initial ARC + Strategic Mitigations
- `arc_rules_sora_2_5.yaml` - Enhanced Mechanisms (Annex C v1.0)

### 2. Python Modules
- `arc_models.py` - Pydantic models (ArcInputs, ArcResult, etc.)
- `arc_calculator.py` - ArcCalculator_v2_0 & ArcCalculator_v2_5 classes
- `arc_validator.py` - Validation rules (422 errors για invalid combos)

### 3. FastAPI Endpoints
- `arc_api.py` - `/sora/2.0/arc` και `/sora/2.5/arc` endpoints

### 4. Tests
- `test_arc_calculator_2_0.py` - Golden tests για SORA 2.0
- `test_arc_calculator_2_5.py` - Golden tests για SORA 2.5
- Property-based tests με Hypothesis

### 5. Documentation
- `README_ARC.md` - Usage examples, API docs

## ⚠️ Critical Implementation Rules

Το Sonnet 4 θα ακολουθήσει αυτά τα **non-negotiable** rules:

1. ✅ **Integer Classes Only** - Καμία fractional reduction (0.5)
2. ✅ **No DAA in Strategic** - DAA είναι tactical, όχι strategic
3. ✅ **No Arbitrary Thresholds** - Όλα από official sources
4. ✅ **Caps are Sacred** - ≤1 χωρίς certification, ≤2 με certification
5. ✅ **U-space = Supporting Evidence** - Όχι automatic -1 class
6. ✅ **Geo-fencing = Supporting Evidence Only** - 0 direct reduction
7. ✅ **Full Trace References** - doc_id + annex + section/page

## 🧪 Verification After Implementation

Μετά το implementation, έλεγξε:

```bash
# 1. Verify file structure
ls Backend_Python/arc/

# 2. Run tests
pytest Backend_Python/arc/tests/test_arc_calculator_2_0.py -v
pytest Backend_Python/arc/tests/test_arc_calculator_2_5.py -v

# 3. Test API
curl -X POST http://localhost:8001/sora/2.0/arc \
  -H "Content-Type: application/json" \
  -d @test_payload.json
```

## 📋 Next Steps After Receiving Implementation

1. **Review Output**: Άνοιξε το `sonnet4_arc_implementation_YYYYMMDD_HHMMSS.md`
2. **Extract Files**: Copy Python files to `Backend_Python/arc/`
3. **Create Directory Structure**:
   ```bash
   mkdir -p Backend_Python/arc/{models,calculators,validators,rules,tests}
   ```
4. **Install Dependencies**:
   ```bash
   pip install pydantic pyyaml fastapi hypothesis pytest
   ```
5. **Run Tests**: Verify all golden tests pass
6. **Deploy**: Add to FastAPI main app

## 🐛 Troubleshooting

### "ANTHROPIC_API_KEY not found"
```powershell
# Verify key is set
$env:ANTHROPIC_API_KEY
```

### "API request failed: rate limit"
Περίμενε 1 λεπτό και ξαναπροσπάθησε. Anthropic έχει rate limits.

### "API request failed: invalid API key"
Έλεγξε ότι το key ξεκινάει με `sk-ant-api03-` και είναι valid.

### Python: "Module 'anthropic' not found"
```bash
pip install anthropic
```

## 📖 Reference Files Included

Το specification περιέχει **ALL 6 reference files completely embedded**:

1. ✅ ARC_SORA20_TableC1_README.md (Initial ARC determination)
2. ✅ ARC_SORA20_TableC2_README.md (Strategic mitigations)
3. ✅ ARC_SORA25_AnnexC_INDEX.md (JARUS 2.5 strategic mitigations)
4. ✅ ARC_SORA20_UI.json (UI field definitions)
5. ✅ ARC_SORA25_UI.json (UI field definitions)
6. ✅ ARC_DROPDOWNS_SUMMARY.json (Common dropdowns)

Το Sonnet 4 **ΔΕΝ χρειάζεται** να έχει access σε external files!

## 🎓 Quality Standards

Το implementation θα ακολουθεί:

- ✅ **Deterministic**: Κανένα randomness
- ✅ **YAML-driven**: Όλα τα rules σε YAML
- ✅ **Traced**: Κάθε decision με rule_ref
- ✅ **Tested**: Golden + Property-based tests
- ✅ **Validated**: 422 errors για invalid inputs
- ✅ **Versioned**: Explicit SORA 2.0 vs 2.5
- ✅ **EASA/JARUS Compliant**: 100% standards compliance

## 🔗 Related Files

- `ARC_IMPLEMENTATION_SPECIFICATION.md` - The specification (input)
- `ARC_Package/` - Original reference files
- `MASTER_PROMPT_PACK/` - Implementation guidelines

## 💡 Tips

1. **Review before deploying**: Διάβασε το output προσεκτικά
2. **Test thoroughly**: Τρέξε όλα τα tests πριν το production
3. **Compare with existing**: Σύγκρινε με τα existing calculations για consistency
4. **Save the prompt**: Κράτα το output για future reference

---

**Ready to send?** Run the script! 🚀

```powershell
.\send_arc_to_sonnet4.ps1
```
