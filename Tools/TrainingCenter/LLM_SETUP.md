# 🤖 AI Agent LLM Setup — Expert Reasoning με Azure OpenAI

## Τι κάνει αυτό;

Οι AI agents (SORA_Compliance_Agent, Mission_Planning_Agent) **δεν απαντούν πια μηχανικά** με keyword search.

Τώρα χρησιμοποιούν **Azure OpenAI GPT-4o** για:
- Πλήρη reasoning με τεκμηρίωση
- Step-by-step μεθοδολογίες
- Practical recommendations
- Edge cases και nuances
- Συζήτηση σαν super expert

---

## 🚀 Setup (5 λεπτά)

### 1. Δημιούργησε Azure OpenAI Resource

1. Πήγαινε στο [Azure Portal](https://portal.azure.com)
2. Create Resource → AI + Machine Learning → Azure OpenAI
3. Επίλεξε region (π.χ. East US, West Europe)
4. Δημιούργησε deployment:
   - Model: **GPT-4o** (recommended) ή **GPT-4o-mini** (οικονομικό)
   - Deployment name: `gpt-4o` (ή custom)

### 2. Πάρε credentials

1. Στο Azure Portal, βρες το resource σου
2. Copy:
   - **Endpoint**: `https://YOUR-RESOURCE-NAME.openai.azure.com/`
   - **API Key**: από "Keys and Endpoint" section

### 3. Κάνε configure

**PowerShell** (Windows — recommended):
```pwsh
# Set environment variables για session
$env:AZURE_OPENAI_ENDPOINT = "https://YOUR-RESOURCE-NAME.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY = "your-api-key-here"
$env:AZURE_OPENAI_DEPLOYMENT = "gpt-4o"

# Για μόνιμη ρύθμιση (persist across sessions):
[System.Environment]::SetEnvironmentVariable('AZURE_OPENAI_ENDPOINT', 'https://YOUR-RESOURCE-NAME.openai.azure.com/', 'User')
[System.Environment]::SetEnvironmentVariable('AZURE_OPENAI_API_KEY', 'your-api-key-here', 'User')
[System.Environment]::SetEnvironmentVariable('AZURE_OPENAI_DEPLOYMENT', 'gpt-4o', 'User')
```

**Bash** (Linux/Mac):
```bash
export AZURE_OPENAI_ENDPOINT="https://YOUR-RESOURCE-NAME.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key-here"
export AZURE_OPENAI_DEPLOYMENT="gpt-4o"

# Για μόνιμη ρύθμιση, πρόσθεσε στο ~/.bashrc ή ~/.zshrc
```

### 4. Εγκατάσταση Python dependencies

```pwsh
pip install openai
```

### 5. Test

```pwsh
cd Tools\TrainingCenter
python agent_llm.py SORA_Compliance_Agent "What is SAIL level for GRC=3 and ARC=b?"
```

**Expected output** (JSON):
```json
{
  "success": true,
  "agent_name": "SORA_Compliance_Agent",
  "question": "What is SAIL level for GRC=3 and ARC=b?",
  "answer": "## Direct Answer\n\nFor GRC=3 (Ground Risk Class) and ARC=b (Air Risk Class), the SAIL level is **II** (Specific Assurance & Integrity Level 2)...",
  "sources": ["SORA-v2.5-Main-Body_chunk_006", "GRC Context Pack", ...],
  "tokens_used": 1247,
  "model": "gpt-4o"
}
```

---

## � Εναλλακτικά: OpenAI API (χωρίς Azure)

Αν δεν έχεις ακόμη ενεργή συνδρομή/πρόσβαση στο Azure OpenAI, μπορείς να χρησιμοποιήσεις απευθείας το OpenAI API.

### Βήματα

1) Πάρε API Key
   - Πήγαινε στο https://platform.openai.com/ → View API keys → Create new secret key.

2) Ρύθμισε μεταβλητές περιβάλλοντος (PowerShell)
```pwsh
$env:OPENAI_API_KEY = "your-openai-key"
# Προαιρετικά: μοντέλο (default: gpt-4o-mini)
$env:OPENAI_MODEL = "gpt-4o-mini"
```

Ή με έτοιμο script (συστήνεται):
```pwsh
pwsh -ExecutionPolicy Bypass -File .\Tools\TrainingCenter\setup_openai_env.ps1 -ApiKey "your-openai-key" -Model "gpt-4o-mini" -Persist
```
Το `-Persist` αποθηκεύει μόνιμα τις μεταβλητές (User env). Αν δεν το βάλεις, ισχύουν μόνο για το τρέχον PowerShell.

3) Test CLI
```pwsh
python .\Tools\TrainingCenter\agent_llm.py SORA_Compliance_Agent "What is SAIL level for GRC=3 and ARC=b?"
```
Αναμενόμενο: JSON με "mode": "openai" και "model": "gpt-4o-mini" (ή ό,τι όρισες).

4) VS Code Chat
```
@skyworks /ask-sora What are the OSO requirements for SAIL III?
```
Θα δεις στην αρχή: 🟣 Mode: OpenAI API, και στα Metadata το μοντέλο.

Σημείωση: Αν ορίσεις και Azure env vars, προτεραιότητα έχει το Azure. Αν θες μόνο OpenAI, μην ορίζεις τα Azure env vars στο τρέχον session.

---

## �💬 Χρήση στο VS Code Chat

Μόλις κάνεις setup, άνοιξε VS Code Chat (`Ctrl+Shift+I`) και γράψε:

```
@skyworks /ask-sora What are the OSO requirements for SAIL III?
```

**Το extension θα:**
1. Φορτώσει τη μνήμη του agent (1,520 entries)
2. Θα κάνει RAG retrieval (top 10 σχετικές πηγές)
3. Θα στείλει ερώτηση στο Azure OpenAI με expert system prompt
4. Θα εμφανίσει πλήρη απάντηση με:
   - Direct answer
   - Regulatory basis με citations
   - Step-by-step methodology
   - Practical recommendations
   - Related considerations
   - Sources

---

## ⚙️ Configuration (optional)

Αν θες να αλλάξεις παραμέτρους:

```pwsh
# Μέγιστα tokens για response (default: 4096)
$env:AZURE_OPENAI_MAX_TOKENS = "8192"

# Temperature (default: 0.7, range: 0.0-2.0)
# 0.0 = πιο deterministic, 2.0 = πιο creative
$env:AZURE_OPENAI_TEMPERATURE = "0.7"
```

---

## 📊 Πόσο κοστίζει;

**GPT-4o pricing** (Μάιος 2025):
- Input: $2.50 per 1M tokens
- Output: $10.00 per 1M tokens

**Παράδειγμα ερώτησης:**
- Input: ~3,000 tokens (system prompt + context + question)
- Output: ~1,500 tokens (comprehensive answer)
- **Κόστος**: ~$0.015 per question (~€0.014)

**GPT-4o-mini** (οικονομικό):
- Input: $0.15 per 1M tokens
- Output: $0.60 per 1M tokens
- **Κόστος**: ~$0.0015 per question (~€0.0014)

💡 Για testing, χρησιμοποίησε **gpt-4o-mini** — 10x φθηνότερο, ακόμα πολύ καλό.

---

## 🔒 Security

- Ποτέ μην κάνεις commit το API key στο git
- Χρησιμοποίησε environment variables ή Azure Key Vault
- Για production: χρησιμοποίησε Managed Identity

---

## 🐛 Troubleshooting

### Error: "openai package not installed"
```pwsh
pip install openai
```

### Error: "Azure OpenAI credentials not found"
```pwsh
# Επανέλεγξε τα env vars:
$env:AZURE_OPENAI_ENDPOINT
$env:AZURE_OPENAI_API_KEY

# Αν είναι κενά, κάνε set ξανά
```

### Error: "model not found"
- Σιγουρέψου ότι έχεις κάνει deploy το model στο Azure Portal
- Check deployment name: `$env:AZURE_OPENAI_DEPLOYMENT`

### Slow responses
- GPT-4o: ~5-15 seconds για comprehensive answer
- GPT-4o-mini: ~2-5 seconds
- Αν πάρει > 30s, check Azure region/quota

---

## 📖 Examples

### SORA Compliance Agent

**Question:** "How do I calculate GRC for an operation over a residential area?"

**Answer (summary):**
```
## Direct Answer
To calculate GRC for residential area operations, follow the SORA 2.0 AMC methodology
using population density metrics...

## Regulatory Basis
JARUS SORA 2.5, Section 3.1.2 defines GRC calculation based on:
- Population density (people/km²)
- Sheltering availability...

## Methodology / Step-by-Step
1. Determine operational area characteristics
2. Assess population density...
3. Evaluate sheltering (buildings/structures)
4. Apply TMPR (Technical Mitigation of Population Risk)
5. Calculate final GRC...

[+ Practical Recommendations, Related Considerations, Sources]
```

### Mission Planning Agent

**Question:** "What documentation is required for STS-01 operations?"

**Answer (summary):**
```
## Executive Summary
STS-01 VLOS operations require: Operation Manual, crew competency records,
UAS technical documentation, insurance, and operational declarations...

## Regulatory Requirements
EU Regulation 2019/947, Annex A (STS-01) mandates...

## Step-by-Step Procedures
1. Prepare Operation Manual covering:
   - Normal procedures...
   - Emergency procedures...
2. Crew certification (A1/A3 + STS-01 training)...

[+ Required Documentation, Safety & Contingencies, Sources]
```

---

## ✅ Summary

Τώρα οι agents:
- ✅ Απαντούν με πλήρη reasoning (όχι keywords)
- ✅ Τεκμηριώνουν με citations από SORA/JARUS/STS
- ✅ Δίνουν step-by-step guidance
- ✅ Προσφέρουν practical advice
- ✅ Συζητούν σαν super experts (10+ years experience)

Αρχίζεις να τους μιλάς μέσα από VS Code Chat με `/ask-sora` και `/ask-mission` 🚀

Modes:
- 🔷 Azure OpenAI — όταν έχεις ρυθμίσει τα Azure env vars
- 🟣 OpenAI API — όταν έχεις θέσει OPENAI_API_KEY (χωρίς Azure)
- 🟠 Local Reasoner — όταν δεν υπάρχουν κλειδιά, με βελτιωμένη offline ανάλυση
