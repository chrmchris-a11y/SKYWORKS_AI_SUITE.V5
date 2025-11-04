# 🎯 OSO Framework Integration Points - SKYWORKS Web Platform

## ✅ **ΟΛΟΚΛΗΡΩΘΗΚΕ ΤΩΡΑ: SORA Mission Planner (mission.html)**

### **Location:** `Frontend/Pages/mission.html`
### **Integration:** Lines 1339-1410 (renderOSOs function)
### **Status:** ✅ **LIVE - OSO UI v2.0 ενεργό!**

**Χρήση:**
1. User εκτελεί SORA evaluation → Υπολογίζεται SAIL level
2. Καλείται `renderOSOs(sail, soraVersion)` 
3. **ΝΕΟ:** Καλεί `OSOUI.renderOSOGrid('osoGrid', 'SORA-2.5', sail, callback)`
4. User βλέπει **interactive OSO grid** με:
   - ✅ **Πράσινα badges**: OSOs που έχουν selected με αρκετή robustness
   - ⚠️ **Πορτοκαλί badges**: OSOs selected αλλά με χαμηλή robustness
   - ❌ **Κόκκινα badges**: Λείπουν required OSOs
   - 📊 **Compliance percentage**: Real-time progress bar
5. User κάνει click σε OSO badge → **Modal** με:
   - OSO description από JARUS Annex E
   - Dropdown για Robustness Level (NR/L/M/H)
   - Evidence reference field
   - Notes textarea
6. User σώζει OSO → **Auto-sync** στο backend:
   ```javascript
   osos: "1:High,2:Medium,5:High,6:Medium,..."
   ```
7. **Export** compliance report JSON για DCA submission

**Code Flow:**
```javascript
displaySORAResults(soraData) 
  → renderOSOs(sail, 'SORA-2.5')
    → OSOUI.renderOSOGrid('osoGrid', 'SORA-2.5', sail, onUpdate)
      → OSOManager.createComplianceTracker('SORA-2.5', sail)
        → tracker.getRequiredOSOs() // Auto από Table 14
      → User clicks OSO badge
        → OSOUI.showOSOSelectionModal(osoId, ...)
          → User selects robustness + evidence
            → tracker.selectOSO(osoId, robustness, evidence, notes)
              → onUpdate() callback
                → Sync to mission.osos field (CSV)
                → Sync to mission.osoCompliance (JSON)
```

---

## 📊 **ΠΡΟΣ ΕΝΣΩΜΑΤΩΣΗ: SORA Reporting & Export**

### **Location:** `Backend/Services/SoraReportGenerator.cs`
### **Status:** ⏸️ **Pending - Phase 5 Step 42+**

**Χρήση:**
1. User ολοκληρώνει SORA evaluation
2. Κάνει export **Comprehensive Safety Portfolio (CSP)** για DCA
3. Backend διαβάζει `mission.osoCompliance` JSON:
   ```json
   {
     "soraVersion": "SORA-2.5",
     "sail": "IV",
     "timestamp": "2025-01-25T12:00:00Z",
     "compliance": {
       "isCompliant": true,
       "compliancePercentage": 95,
       "missingOSOs": [],
       "insufficientOSOs": [{"id": 6, "required": "H", "actual": "M"}]
     },
     "selectedOSOs": [
       {
         "osoId": 1,
         "osoNumber": "OSO#01",
         "osoName": "Ensure the Operator is competent",
         "robustness": "High",
         "evidence": "SMS_Manual_v3.2.pdf",
         "notes": "Full SMS per ICAO Annex 19 implemented"
       },
       ...
     ]
   }
   ```
4. Report generator δημιουργεί **PDF section**:
   ```
   ┌─────────────────────────────────────────────────┐
   │ OPERATIONAL SAFETY OBJECTIVES (OSOs)            │
   │ SORA 2.5 - SAIL IV Requirements                 │
   ├─────────────────────────────────────────────────┤
   │ Compliance: 95% (20/21 OSOs)                    │
   │ ⚠️ 1 OSO with insufficient robustness           │
   ├─────────────────────────────────────────────────┤
   │ OSO#01 - Operator competent/proven              │
   │   Required: High ✅ Actual: High                │
   │   Evidence: SMS_Manual_v3.2.pdf                 │
   │                                                  │
   │ OSO#06 - C3 link performance                    │
   │   Required: High ⚠️ Actual: Medium              │
   │   Evidence: Datalink_Test_Report.pdf            │
   │   ⚠️ WARNING: Insufficient robustness!          │
   │      Recommend upgrading C3 link                │
   │ ...                                              │
   └─────────────────────────────────────────────────┘
   ```

**Implementation:**
```csharp
// Backend/Services/SoraReportGenerator.cs
public string GenerateOSOComplianceSection(OSOComplianceReport report)
{
    var sb = new StringBuilder();
    sb.AppendLine("## Operational Safety Objectives");
    sb.AppendLine($"SORA Version: {report.SoraVersion}");
    sb.AppendLine($"SAIL Level: {report.Sail}");
    sb.AppendLine($"Compliance: {report.Compliance.CompliancePercentage}%");
    
    if (!report.Compliance.IsCompliant)
    {
        sb.AppendLine("### ⚠️ COMPLIANCE ISSUES DETECTED");
        if (report.Compliance.MissingOSOs.Count > 0)
        {
            sb.AppendLine($"Missing OSOs ({report.Compliance.MissingOSOs.Count}):");
            foreach (var missing in report.Compliance.MissingOSOs)
            {
                sb.AppendLine($"- {missing.Number}: {missing.Name} (Required: {missing.RequiredRobustness})");
            }
        }
    }
    
    foreach (var oso in report.SelectedOSOs)
    {
        sb.AppendLine($"### {oso.OsoNumber}: {oso.OsoName}");
        sb.AppendLine($"Robustness: {oso.Robustness}");
        sb.AppendLine($"Evidence: {oso.Evidence}");
        if (!string.IsNullOrEmpty(oso.Notes))
        {
            sb.AppendLine($"Notes: {oso.Notes}");
        }
        sb.AppendLine();
    }
    
    return sb.ToString();
}
```

---

## 🗂️ **ΠΡΟΣ ΕΝΣΩΜΑΤΩΣΗ: Operations Manager (Multi-Operation OSO Tracking)**

### **Location:** `Frontend/Pages/operations-manager.html` (to be created)
### **Status:** ⏸️ **Pending - Phase 5 Step 45+**

**Χρήση:**
1. Operator έχει **πολλαπλές operations** (π.χ. 10 missions)
2. Operations Manager εμφανίζει **aggregate OSO compliance**:
   ```
   ┌────────────────────────────────────────────────────────┐
   │ OPERATIONS MANAGER - OSO COMPLIANCE DASHBOARD          │
   ├────────────────────────────────────────────────────────┤
   │ Total Operations: 10                                   │
   │ Average OSO Compliance: 87%                            │
   │                                                         │
   │ ⚠️ 3 operations with compliance < 90%                  │
   │ ❌ 1 operation missing critical OSO#05                 │
   ├────────────────────────────────────────────────────────┤
   │ Operation              | SAIL | Compliance | Issues   │
   │────────────────────────┼──────┼────────────┼──────────│
   │ Mission-2025-001 ✅    | III  | 100%       | None     │
   │ Mission-2025-002 ✅    | IV   | 95%        | None     │
   │ Mission-2025-003 ⚠️    | IV   | 85%        | OSO#06-M │
   │ Mission-2025-004 ❌    | V    | 70%        | OSO#05 ❌│
   │ ...                     |      |            |          │
   └────────────────────────────────────────────────────────┘
   ```
3. User κάνει click σε operation → Βλέπει **detailed OSO breakdown**
4. User μπορεί να **bulk export** OSO compliance για όλες τις operations

**Code:**
```javascript
// operations-manager.html
function loadOperationsOSOCompliance() {
  const operations = await fetchAllOperations();
  
  operations.forEach(op => {
    const tracker = OSOManager.createComplianceTracker(op.soraVersion, op.sail);
    
    // Restore saved OSO selections
    if (op.osoCompliance && op.osoCompliance.selectedOSOs) {
      op.osoCompliance.selectedOSOs.forEach(saved => {
        tracker.selectOSO(saved.osoId, saved.robustness, saved.evidence, saved.notes);
      });
    }
    
    const validation = tracker.validateCompliance();
    
    // Display compliance in table
    const row = document.createElement('tr');
    row.innerHTML = `
      <td>${op.name}</td>
      <td>SAIL ${op.sail}</td>
      <td>${validation.compliancePercentage}%</td>
      <td>
        ${validation.missingOSOs.length > 0 ? '❌ ' + validation.missingOSOs.length + ' missing' : ''}
        ${validation.insufficientOSOs.length > 0 ? '⚠️ ' + validation.insufficientOSOs.length + ' low' : ''}
        ${validation.isCompliant ? '✅ Compliant' : ''}
      </td>
    `;
    
    operationsTable.appendChild(row);
  });
}
```

---

## 🏷️ **ΠΡΟΣ ΕΝΣΩΜΑΤΩΣΗ: PDRA/STS Auto-Population**

### **Location:** `Backend/Services/PdraStsService.cs`
### **Status:** ⏸️ **Pending - Phase 5 Step 46+**

**Χρήση:**
1. User επιλέγει **PDRA category** (π.χ. "PDRA-S01 - Visual Line of Sight")
2. Backend **auto-populates** OSOs από PDRA requirements:
   ```json
   // PDRA-S01 OSO Requirements
   {
     "pdraId": "PDRA-S01",
     "requiredOSOs": [
       {"id": 1, "robustness": "Medium"},
       {"id": 2, "robustness": "Low"},
       {"id": 5, "robustness": "Medium"},
       {"id": 6, "robustness": "Low"},
       {"id": 8, "robustness": "Medium"},
       {"id": 16, "robustness": "Low"},
       {"id": 17, "robustness": "Low"}
     ]
   }
   ```
3. Frontend καλεί `OSOManager` να **pre-populate** OSOs:
   ```javascript
   // Pre-fill OSOs from PDRA
   const tracker = OSOManager.createComplianceTracker('SORA-2.5', 'III');
   pdraRequirements.requiredOSOs.forEach(req => {
     tracker.selectOSO(req.id, req.robustness, 'PDRA-S01 requirement', 'Auto-filled from PDRA');
   });
   
   // Render with pre-filled OSOs (green badges)
   OSOUI.renderOSOGrid('osoGrid', 'SORA-2.5', 'III', onUpdate);
   ```
4. User βλέπει **ήδη filled OSOs** (green badges)
5. User μπορεί να **customize** (π.χ. upgrade OSO#05 από M→H)

**Backend Logic:**
```csharp
// Backend/Services/PdraStsService.cs
public class PdraStsService
{
    public PdraOsoRequirements GetPdraOsoRequirements(string pdraId)
    {
        // Read from PDRA database/JSON
        var pdra = _pdraRepository.GetById(pdraId);
        
        return new PdraOsoRequirements
        {
            PdraId = pdraId,
            RequiredOSOs = pdra.OsoRequirements.Select(r => new OsoRequirement
            {
                Id = r.OsoId,
                Robustness = r.MinimumRobustness,
                IsMandatory = r.IsMandatory,
                Notes = $"PDRA {pdraId} requirement"
            }).ToList()
        };
    }
    
    public bool ValidateOsoCompliance(PdraOsoRequirements pdrReqs, OSOComplianceReport userOsos)
    {
        // Check if user's OSOs meet or exceed PDRA requirements
        foreach (var req in pdrReqs.RequiredOSOs)
        {
            var userOso = userOsos.SelectedOSOs.FirstOrDefault(o => o.OsoId == req.Id);
            
            if (userOso == null && req.IsMandatory)
            {
                return false; // Missing mandatory OSO
            }
            
            if (userOso != null && GetRobustnessLevel(userOso.Robustness) < GetRobustnessLevel(req.Robustness))
            {
                return false; // Insufficient robustness
            }
        }
        
        return true; // All PDRA OSO requirements met
    }
}
```

---

## 📄 **ΠΡΟΣ ΕΝΣΩΜΑΤΩΣΗ: Standalone OSO Compliance Page**

### **Location:** `Frontend/Pages/oso-compliance.html` (to be created)
### **Status:** ⏸️ **Pending - Phase 5 Step 47+**

**Χρήση:**
1. Dedicated page για **OSO management** (ανεξάρτητο από mission planner)
2. User επιλέγει:
   - SORA version (2.0 vs 2.5)
   - SAIL level (I-VI)
   - Operation type (VLOS, BVLOS, Urban, etc.)
3. Page εμφανίζει **full OSO catalog** με:
   - Detailed descriptions από Annex E
   - Evidence templates
   - Implementation guidelines
   - Example documents
4. User μπορεί να **work on OSO compliance** independently
5. **Export** compliance report για later import στο mission planner

**UI Layout:**
```
┌────────────────────────────────────────────────────────────────┐
│ 📋 OSO COMPLIANCE MANAGER                                      │
├────────────────────────────────────────────────────────────────┤
│ SORA Version: [SORA-2.5 ▼]  SAIL: [IV ▼]  Type: [BVLOS ▼]    │
├────────────────────────────────────────────────────────────────┤
│ Compliance: 85% ███████████░░░ (18/21 OSOs)                   │
│                                                                 │
│ ⚠️ 3 OSOs incomplete:                                          │
│    - OSO#05: System safety & reliability                       │
│    - OSO#18: Automatic protection from errors                 │
│    - OSO#24: Environmental qualification                       │
├────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────┐ ┌────────────────────────────────────┐│
│ │ OSO CATALOG         │ │ OSO DETAILS                        ││
│ ├─────────────────────┤ ├────────────────────────────────────┤│
│ │ ✅ OSO#01 (High)    │ │ OSO#01: Operator Competence        ││
│ │ ✅ OSO#02 (Medium)  │ │                                    ││
│ │ ⚠️ OSO#05 (Medium)  │ │ Required: High ⚠️ Current: Medium  ││
│ │ ✅ OSO#06 (High)    │ │                                    ││
│ │ ...                 │ │ Description:                       ││
│ │                     │ │ Ensure the UAS is designed and     ││
│ │ [+ Add OSO]         │ │ constructed to minimize risks...   ││
│ │ [Export Report]     │ │                                    ││
│ │ [Import Report]     │ │ Evidence:                          ││
│ │                     │ │ - Design documentation             ││
│ │                     │ │ - Safety analysis                  ││
│ │                     │ │ - Test reports                     ││
│ │                     │ │                                    ││
│ │                     │ │ Robustness: [High ▼]               ││
│ │                     │ │ Evidence: [Browse...]              ││
│ │                     │ │ Notes: [......................]    ││
│ │                     │ │                                    ││
│ │                     │ │ [💾 Save] [🗑️ Remove]              ││
│ └─────────────────────┘ └────────────────────────────────────┘│
└────────────────────────────────────────────────────────────────┘
```

---

## 🔄 **INTEGRATION PRIORITY ORDER**

### **Phase 5.1 - COMPLETED ✅**
- ✅ Step 41: OSO Basic Framework (oso-manager-v2.js, oso-ui-v2.js)
- ✅ Mission Planner Integration (mission.html renderOSOs)

### **Phase 5.2 - IN PROGRESS 🔄**
- 🔄 Step 42: Complex OSO Algorithms
  - Dependency resolution (π.χ. OSO#05 needs OSO#02)
  - Auto-recommendations based on operation type
  - Smart evidence templates

### **Phase 5.3 - PENDING ⏸️**
- ⏸️ Step 43: OSO Compliance Validation
  - Backend validation API
  - Robustness level checking
  - Evidence completeness verification

### **Phase 5.4 - PENDING ⏸️**
- ⏸️ Step 44: SORA Reporting Integration
  - PDF report generation με OSO section
  - CSP export με compliance data
  - DCA submission formatting

### **Phase 5.5 - PENDING ⏸️**
- ⏸️ Step 45: Operations Manager Integration
  - Multi-operation OSO dashboard
  - Aggregate compliance statistics
  - Bulk export/import

### **Phase 5.6 - PENDING ⏸️**
- ⏸️ Step 46: PDRA/STS Auto-Population
  - PDRA OSO requirements database
  - Auto-fill from PDRA selection
  - Compliance verification vs PDRA

### **Phase 5.7 - PENDING ⏸️**
- ⏸️ Step 47: Standalone OSO Compliance Page
  - Dedicated OSO management UI
  - Detailed implementation guides
  - Evidence template library

---

## 📊 **SUMMARY: ΠΟΥ ΧΡΗΣΙΜΟΠΟΙΕΙΤΑΙ ΤΟ OSO FRAMEWORK**

### **1️⃣ SORA Mission Planner (✅ LIVE NOW)**
- **Αυτόματη** εμφάνιση required OSOs μετά από SAIL calculation
- **Interactive** grid με color-coded badges
- **Modal** selection για κάθε OSO
- **Auto-sync** στο backend (CSV format)
- **Export** compliance report

### **2️⃣ SORA Reporting (⏸️ Pending)**
- **PDF generation** με OSO compliance section
- **DCA submission** formatting
- **Evidence references** in report

### **3️⃣ Operations Manager (⏸️ Pending)**
- **Multi-operation** OSO tracking
- **Aggregate statistics** (average compliance, issues)
- **Bulk export** για πολλές operations

### **4️⃣ PDRA/STS Integration (⏸️ Pending)**
- **Auto-population** από PDRA requirements
- **Validation** vs PDRA minimum OSOs
- **Compliance checking** για STS categories

### **5️⃣ Standalone OSO Page (⏸️ Pending)**
- **Dedicated** OSO compliance management
- **Detailed** implementation guides
- **Evidence library** με templates

---

## 🎯 **ΤΕΧΝΙΚΗ ΑΡΧΙΤΕΚΤΟΝΙΚΗ**

```
┌─────────────────────────────────────────────────────────────────┐
│ FRONTEND                                                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐    ┌──────────────────┐                   │
│  │ oso-manager-v2.js│◄───┤ oso-ui-v2.js     │                   │
│  │                  │    │                  │                   │
│  │ • OSO_DEFINITIONS│    │ • renderOSOGrid()│                   │
│  │ • Compliance     │    │ • showModal()    │                   │
│  │   Tracker        │    │ • Export/Import  │                   │
│  └────────┬─────────┘    └────────┬─────────┘                   │
│           │                       │                              │
│           │                       │                              │
│  ┌────────▼───────────────────────▼─────────┐                   │
│  │ mission.html                             │                   │
│  │ • renderOSOs(sail, version)              │                   │
│  │ • displaySORAResults(data)               │                   │
│  │ • Export mission JSON με OSO compliance  │                   │
│  └────────┬─────────────────────────────────┘                   │
│           │                                                      │
│           │ HTTP POST /api/sora/evaluate                        │
│           ▼                                                      │
├─────────────────────────────────────────────────────────────────┤
│ BACKEND                                                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────┐                   │
│  │ SoraController.cs                        │                   │
│  │ • POST /api/sora/evaluate                │                   │
│  │ • Receives: osos="1:High,2:Medium,..."   │                   │
│  └────────┬─────────────────────────────────┘                   │
│           │                                                      │
│           ▼                                                      │
│  ┌──────────────────────────────────────────┐                   │
│  │ SoraService.cs                           │                   │
│  │ • ValidateOSOCompliance(osos, sail)      │                   │
│  │ • CheckOSORobustness(osoId, robustness)  │                   │
│  └────────┬─────────────────────────────────┘                   │
│           │                                                      │
│           ▼                                                      │
│  ┌──────────────────────────────────────────┐                   │
│  │ SoraReportGenerator.cs                   │                   │
│  │ • GenerateOSOComplianceSection(report)   │                   │
│  │ • ExportCSP() με OSO evidence            │                   │
│  └──────────────────────────────────────────┘                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🚀 **ΕΠΟΜΕΝΑ ΒΗΜΑΤΑ**

### **Immediate (Step 42):**
- [ ] Implement OSO dependency resolution
- [ ] Add auto-recommendations based on operation type
- [ ] Create evidence templates library

### **Short-term (Steps 43-45):**
- [ ] Backend OSO validation API
- [ ] PDF report generation με OSO section
- [ ] Operations Manager OSO dashboard

### **Medium-term (Steps 46-47):**
- [ ] PDRA OSO requirements database
- [ ] Auto-population logic
- [ ] Standalone OSO compliance page

### **Long-term (Steps 48-50):**
- [ ] Advanced OSO analytics
- [ ] Machine learning για OSO recommendations
- [ ] Multi-language OSO descriptions

---

**ΤΕΛΟΣ ΑΝΑΦΟΡΑΣ - OSO Integration Points**
**Created:** 2025-01-25
**Status:** Mission Planner integration ✅ COMPLETE, Other integrations ⏸️ PENDING
