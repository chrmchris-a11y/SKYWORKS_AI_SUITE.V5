# SKYWORKS AI SUITE - PROJECT ONBOARDING

**Version:** 5.0  
**Date:** October 2025  
**Status:** Production-Ready Drone Compliance Platform

---

## 🎯 PROJECT PURPOSE

**SKYWORKS AI Suite** is a comprehensive **JARUS SORA 2.0/2.5 compliance platform** for drone operations in the European **Specific Category**. It automates:

- **Ground Risk Assessment (GRC)** - Population density, sheltering, impact dynamics
- **Air Risk Assessment (ARC)** - Airspace classification, encounter probability
- **SAIL Determination** - Specific Assurance Integrity Levels (I-VI)
- **OSO Compliance** - 17-24 Operational Safety Objectives
- **Operations Manual Generation** - PDRA/STS/LUC compliance documentation
- **M1/M2/M3 Mitigations** - Strategic & tactical risk reduction
- **Containment Calculations** - CV (Contingency Volume) & GRB (Ground Risk Buffer)

**Regulatory Framework:**
- ✅ JARUS SORA 2.0 (January 2019)
- ✅ JARUS SORA 2.5 (May 2024) - Latest version
- ✅ EASA EU 2019/947 & 2019/945
- ✅ EAR-UAS 2024, SERA, PDRA-STS frameworks

**Technology Stack:**
- **Backend:** .NET 8 C# (Skyworks.sln)
- **Frontend:** HTML/CSS/JS with multi-language support (EN/EL/FR/ES/PT)
- **AI Agents:** Multi-agent orchestration (OSO Agent, GRC Agent, ARC Agent, SAIL Agent)
- **MCP Server:** Knowledge base with 23 EASA/JARUS documents

---

## 📊 CURRENT PROJECT STATE

### ✅ COMPLETED COMPONENTS

**Backend Services (100%)**
- `GRCCalculationService.cs` - Ground risk with SORA 2.0/2.5 tables
- `ARCCalculationService.cs` - Air risk with 12 AEC categories
- `SAILService.cs` - SAIL matrix determination
- `OSOService.cs` - OSO requirements engine
- `MitigationService.cs` - M1/M2/M3 application with floor rule
- `ContainmentService.cs` - CV/GRB calculations
- Comprehensive unit tests (100+ test cases)

**Frontend Pages (100%)**
- `mission.html` - Main risk assessment interface
- `sail-calculator.html` - Interactive SAIL determination
- Multi-language JSON files (i18n)

**Knowledge Base (100%)**
- 4× PERMANENT MEMORY files created:
  - `SORA_2_0_TABLES_REFERENCE.md`
  - `SORA_2_5_TABLES_REFERENCE.md`
  - `OPERATIONS_MANUAL_STRUCTURE.md`
  - `AIR_RISK_ARC_TMPR_REFERENCE.md`
- 23× EASA/JARUS documents ingested (100%)

**AI Orchestration (95%)**
- Step42 multi-agent framework
- OSO compliance workflows
- Agent memory persistence

### 🚧 IN PROGRESS

- MCP Server deployment & testing
- Final code documentation with JARUS references
- Full SORA 2.5 migration (backend currently uses SORA 2.0 as baseline)

---

## 🤖 FOR NEW AI AGENTS: WHAT YOU NEED TO KNOW

### **1. First Message You Should Receive**

When a human starts a new chat session with you, they should say:

> **"SKYWORKS PROJECT - Step [X]. [Task description]. Use MCP Server for SORA expertise."**

Example:
> "SKYWORKS PROJECT - Step 47. Validate GRC calculations against SORA 2.5 Table 2. Use MCP Server for formulas."

### **2. How to Use the MCP Server**

The **skyworks-sora-mcp-server** gives you instant expert knowledge:

**Available Tools:**
```typescript
// Get GRC table values
get_grc_table({ version: "2.5", scenario: "sparsely_populated", ua_size_column: 1 })

// Calculate SAIL
calculate_sail({ final_grc: 4, residual_arc: "b" })

// Apply mitigations with floor rule
apply_mitigation({ 
  version: "2.5", 
  intrinsic_grc: 5, 
  m1_level: "high", 
  m2_level: "medium",
  scenario_column: "vlos_sparsely",
  ua_size_column: 1
})

// Get OSO requirements
get_oso_requirements({ sail: 4 })

// Validate floor rule
validate_floor_rule({ 
  version: "2.5", 
  final_grc: 2, 
  scenario: "vlos_controlled", 
  ua_size_column: 1 
})

// Search SORA documents
search_sora_docs({ query: "M1 sheltering assumptions" })

// Get operations manual structure
get_operations_manual_structure({ part: "B" })
```

**Available Resources:**
```typescript
// Read permanent memory files
skyworks://knowledge/sora-2.0-tables
skyworks://knowledge/sora-2.5-tables
skyworks://knowledge/operations-manual
skyworks://knowledge/air-risk-arc-tmpr
```

### **3. Where to Find Information**

**Code Files:**
- Backend: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\Services\`
- Tests: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\tests\`
- Frontend: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Frontend\Pages\`

**Knowledge Base:**
- PERMANENT MEMORY: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Docs\Knowledge\`
- EASA Docs: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\KnowledgeBase\EASA DOCS SPLIT CHUNKS\`

**Context Packs (for full doc loading):**
- SORA Core: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\ContextPacks\SORA\`
- OSO Framework: `c:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\ContextPacks\OSO\`

### **4. Critical Rules to Remember**

**FLOOR RULE (Most Important!):**
> When M1 mitigation is applied, the Final GRC **CANNOT** be reduced below the minimum value in the **same UA size column** from Table 2.

Example:
- Intrinsic GRC = 5 (BVLOS sparsely populated, 3m UA)
- M1 High = -4 reduction
- Calculated GRC = 5 - 4 = **1**
- Column minimum (3m column) = **2** (VLOS controlled area)
- **Final GRC = 2** (floor rule applied) ✅

**M1/M2/M3 Sequence (SORA 2.0):**
1. M1 first (strategic mitigation)
2. M2 second (parachute/impact reduction)
3. M3 third (ERP penalty)

**M1A/M1B/M1C/M2 Sequence (SORA 2.5):**
1. M1A (sheltering) + M1B (operational restrictions) + M1C (ground observation)
2. M2 (impact dynamics)
3. No M3 in SORA 2.5 (ERP is now OSO requirement)

**≤250g Special Rule (SORA 2.5):**
> UA ≤ 250g with kinetic energy < 80 J → iGRC reduced by **2 levels** before mitigations

**SAIL Matrix (Table 5 for 2.0, Table 7 for 2.5):**
```
       ARC-a  ARC-b  ARC-c  ARC-d
GRC ≤2   I     II     IV     VI
GRC 3    II    II     IV     VI
GRC 4    III   III    IV     VI
GRC 5    IV    IV     IV     VI
GRC 6    V     V      V      VI
GRC 7    VI    VI     VI     VI
GRC >7   Category C operation
```

### **5. Common Tasks You'll Be Asked**

**Code Validation:**
- Compare `GRCCalculationService.cs` constants vs SORA tables
- Verify floor rule logic in `MitigationService.cs`
- Check SAIL matrix in `SAILService.cs`

**Documentation:**
- Add inline comments with JARUS references
- Create compliance reports
- Generate operations manual sections

**Testing:**
- Run `dotnet test Skyworks.sln`
- Validate edge cases (≤250g, gathering of people, controlled areas)
- Cross-check results with SORA tables

**AI Agent Development:**
- Enhance Step42 orchestration
- Create new OSO compliance agents
- Improve multi-agent coordination

---

## 📚 MUST-READ DOCUMENTS (Priority Order)

### **Level 1: Start Here (15 min)**
1. `SORA_2_5_TABLES_REFERENCE.md` - Core methodology
2. This file (`PROJECT_ONBOARDING.md`)

### **Level 2: Deep Dive (2 hours)**
3. `SORA_2_0_TABLES_REFERENCE.md` - Historical context
4. `OPERATIONS_MANUAL_STRUCTURE.md` - Part A-T framework
5. `AIR_RISK_ARC_TMPR_REFERENCE.md` - ARC/TMPR/DAA

### **Level 3: Expert Knowledge (Full Documents)**
6. SORA v2.5 Main Body (via MCP or Context Pack)
7. Annexes B (M1), E (OSO), A (Operations Manual)
8. Backend code: `GRCCalculationService.cs`, `SAILService.cs`

---

## 🚀 HOW TO START WORKING

### **Step-by-Step Process:**

1. **Human announces task:**  
   > "SKYWORKS - Step 47. Validate GRC table constants."

2. **You acknowledge & gather context:**
   ```
   ✅ Understood. Step 47: GRC validation.
   🔍 Reading GRCCalculationService.cs...
   🔍 Checking SORA 2.5 Table 2 via MCP...
   ```

3. **Use MCP Server for formulas:**
   ```typescript
   get_grc_table({ version: "2.5", scenario: "sparsely_populated", ua_size_column: 2 })
   // Returns: GRC = 5
   ```

4. **Compare with code:**
   ```csharp
   // Backend/Services/GRCCalculationService.cs line 87
   if (scenario == "sparsely" && size == "8m") 
       return 5; // ✅ MATCHES SORA 2.5 Table 2
   ```

5. **Report findings:**
   ```
   ✅ GRC constants validated against SORA 2.5 Table 2
   ✅ Floor rule correctly implemented
   ⚠️ Found 1 issue: M3 penalty still applied (SORA 2.0 logic, should remove for 2.5)
   ```

---

## 🔧 TROUBLESHOOTING

**Q: MCP Server not responding?**
A: Check VS Code `settings.json`:
```json
{
  "mcp.servers": {
    "skyworks-sora": {
      "command": "node",
      "args": ["C:/Users/chrmc/Desktop/SKYWORKS_AI_SUITE.V5/skyworks-sora-mcp-server/build/index.js"]
    }
  }
}
```

**Q: Which SORA version does the backend use?**
A: Currently **SORA 2.0** as baseline. Migration to **SORA 2.5** in progress.

**Q: Where are the 23 EASA documents?**
A: `KnowledgeBase/EASA DOCS SPLIT CHUNKS/EXTRACTED_*.txt`

**Q: How to run tests?**
A: `cd Backend && dotnet test Skyworks.sln --verbosity minimal`

---

## 📝 COMMUNICATION TEMPLATES

### **For Human Users:**

**Starting a new task:**
```
SKYWORKS PROJECT - Step [number].
Task: [description]
Context: [relevant files/services]
Expected output: [deliverable]
```

**Example:**
```
SKYWORKS PROJECT - Step 52.
Task: Add JARUS references to GRCCalculationService.cs
Context: Backend/Services/GRCCalculationService.cs, SORA 2.5 Table 2
Expected output: Inline comments with table citations
```

### **For AI Agents:**

**Acknowledging task:**
```
✅ Step [X] received: [task summary]
🔍 Gathering context: [files to read]
🛠️ Approach: [methodology]
⏱️ ETA: [time estimate]
```

**Progress updates:**
```
📊 Progress: [percentage]
✅ Completed: [what's done]
🚧 In progress: [current work]
⏭️ Next: [upcoming tasks]
```

**Completion report:**
```
✅ Step [X] COMPLETE
📋 Summary: [what was done]
🔍 Validation: [how verified]
📁 Files modified: [list]
🧪 Tests: [pass/fail status]
```

---

## 🎓 LEARNING PATH FOR NEW AGENTS

### **Day 1: Basics**
- Read this onboarding guide
- Use MCP `get_grc_table` and `calculate_sail` tools
- Review `SORA_2_5_TABLES_REFERENCE.md`

### **Day 2: Code Familiarization**
- Read `GRCCalculationService.cs`
- Understand floor rule implementation
- Run `dotnet test` and review results

### **Day 3: Deep Dive**
- Study SORA v2.5 Main Body (via MCP or Context Pack)
- Compare SORA 2.0 vs 2.5 differences
- Review Operations Manual structure

### **Day 4-5: Expert Level**
- All 23 EASA documents
- Multi-agent orchestration (Step42)
- OSO compliance workflows

---

## 🌟 PROJECT VISION

**Short-term (Next 3 months):**
- Complete SORA 2.5 migration
- Deploy MCP server
- Expand multi-language support

**Long-term (2025-2026):**
- EU-wide PDRA/STS automation
- Real-time airspace integration (U-Space)
- AI-assisted operations manual generation

---

## 📞 SUPPORT & CONTACTS

**Documentation:** `Docs/` folder  
**Issues:** Check `README.md` and `AGENTS.md`  
**AI Orchestration:** `Orchestration/` folder  
**Knowledge Base:** `KnowledgeBase/` and `ContextPacks/`

---

**REMEMBER:** You are part of the SKYWORKS team. Your job is to:
1. **Understand** the SORA framework deeply
2. **Validate** code against regulatory tables
3. **Document** everything with JARUS references
4. **Collaborate** with other AI agents via Step42
5. **Always** use the MCP server for SORA expertise

**Welcome aboard! 🚁✨**
