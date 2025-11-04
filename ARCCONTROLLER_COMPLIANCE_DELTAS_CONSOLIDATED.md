# ARCController.cs - CONSOLIDATED COMPLIANCE DELTAS (Audit-Ready)

**File Under Review**: `Backend/src/Skyworks.Api/Controllers/ARCController.cs`  
**Total Issues**: 19  
**Status**: ⚠️ **CRITICAL COMPLIANCE GAPS** - Requires IARCCalculationService injection & Annex C alignment

---

## 📋 CONSOLIDATED DELTAS (One-Liner Format)

| # | File | Endpoint/Section | Current Behavior Issue | Required Change | Rationale | Official Reference |
|---|------|------------------|------------------------|-----------------|-----------|-------------------|
| 1 | ARCController.cs | Constructor DI | Uses IGRCCalculationService for all ARC methods. | Inject and use a dedicated IARCCalculationService for ARC; keep IGRCCalculationService only for GRC paths. | Avoids cross‑concern coupling and ensures ARC logic maps to Annex C tables without leaking GRC assumptions. | JARUS SORA 2.0/2.5 architecture-neutral; Annex C Table C.1 governs ARC; keep separation of concerns. |
| 2 | ARCController.cs | POST /api/arc/v2.0/initial | Calls _service.DetermineInitialARC_V2_0 on a GRC service. | Replace call with _arc.DetermineInitialARC_V2_0 using IARCCalculationService; surface AEC code and density in response DTO. | Initial ARC must be driven by Annex C AEC mapping and expose density for downstream use. | SORA 2.0, Annex C Table 1 (AEC & density column). |
| 3 | ARCController.cs | POST /api/arc/v2.5/initial | Same service mis‑wiring as v2.0 (GRC service used). | Use IARCCalculationService.DetermineInitialARC_V2_5 and return { arc, aec_code (1–12), density_rating }. | Annex C Table C.1 requires deterministic AEC and density reporting. | SORA 2.5, Annex C Table C.1. |
| 4 | ARCController.cs | POST /api/arc/v2.0/residual & /v2.5/residual | Residual ARC calculation wired to GRC service. | Route residual ARC to IARCCalculationService.DetermineResidualARC_* implementing Annex C Table C.2 reduction limits and §C.6.3 rules. | Strategic mitigations for air risk have bounded effect; avoid linear decrements. | SORA 2.x, Annex C Table C.2; §C.6.3 'Common structures & rules'. |
| 5 | ARCController.cs | CompositeInitial_V25 | Computes composite but no rule specified for aggregation. | Aggregate by worst‑case ARC (d>c>b>a). For ties, select highest density rating and highest AEC code severity (1>…>12 priority by encounter rate). Document rule in API notes. | SORA does not prescribe segment aggregation; worst‑case across mission envelope is defensible for safety cases. | SORA MB Step #4 intent (deterministic initial ARC across the operational volume). |
| 6 | ARCController.cs | Initial_V20_FromEnvironment / Initial_V25_FromEnvironment | Environment inputs do not cover airport/heliport environment or FL600 gate. | Extend ARCEnvironmentInput to include: isAirportHeliportEnvironment (bool), flightLevel/heightAMSL for FL600, and map SUBURBAN→URBAN. | AEC 1/6 (airport) and AEC 11 (>FL600) require explicit inputs; suburban is not a separate class in Annex C. | Annex C Table C.1 (AEC 1, 6, 11); wording on urban vs rural. |
| 7 | ARCController.cs | ValidateEnv_V25 | Validation opaque; may accept values outside Annex C scope. | Enforce table‑driven validation: reject OUT_OF_SCOPE configurations; require AEC row trace and density in the response for audit. | Authorities expect a clear AEC row trace and explicit out‑of‑scope handling. | Annex C Table C.1 (completeness) and §C.6.3 (rules). |
| 8 | ARCController.cs | GET /api/arc/v2.5/validate/at-point | Defaults Environment=Urban; forces LocationType=NonAirport; uses 'controlled' heuristic only. | Derive isAirportHeliportEnvironment from CTR/ATZ/TIA/airport polygons; classify environment via population layers; surface both flags. Avoid defaulting to Urban unless source unavailable (then mark 'assumed'). | AEC 1/6 depend on airport environment; urban/rural must be evidenced, not assumed. | Annex C Table C.1 (airport/heliport rows; urban vs rural split for uncontrolled). |
| 9 | ARCController.cs | GET /api/arc/v2.5/validate/at-point | MaxHeightAGL defaults to 60 m but Annex C thresholds are 150 m (2.5) / 500 ft (2.0). | Document threshold logic in response and accept client‑provided altitude band; do not infer ARC decisions from 120 m Open category limits. | ARC gating uses 150 m (2.5) not 120 m; 120 m is an Open category constraint. | Annex C Table C.1 bands; SORA MB vs Open category limits. |
| 10 | ARCController.cs | TMPR endpoints (/v2.0/tmpr, /v2.5/tmpr) | TMPR calculated by GRC service and tied to ARC without Annex reference. | Base TMPR on residual ARC per air‑risk path and cite Annex E; ensure inputs include surveillance/tactical mitigation capabilities (e.g., DAA sensors) required to meet TMPR class. | TMPR is defined against air‑risk tactical mitigations; must not be derived from ground‑risk services. | SORA 2.5 Annex E (TMPR); SORA 2.0 Annex C notes on tactical mitigations. |
| 11 | ARCController.cs | StrategicMitigations() | Returns list but no versioning or qualification level (SM1–SM4). | Return structured payload with mitigation IDs, quality levels, prerequisites, and allowed ARC reduction per Table C.2. | Prevents clients from assuming linear 1‑level decrements; codifies constraints. | Annex C Table C.2 (mitigation mapping and reductions). |
| 12 | ARCController.cs | All endpoints (v2.0 & v2.5) | No AEC code/density in DTOs; no source trace (table row) in responses. | Augment response with { aec_code, density_rating, annex_ref, aec_row_descriptor } and include an 'out_of_scope' flag when applicable. | Improves auditability and makes SAIL/OSO derivations deterministic. | Annex C Table C.1 density column; SORA MB Step #4 evidence requirements. |
| 13 | ARCController.cs | Route naming / semantics | Uses 'near aerodrome' semantics inconsistently across stack (service & controller). | Replace 'near aerodrome' with explicit 'isAirportHeliportEnvironment' in inputs and AEC 1/6 mapping; reserve TMZ/Mode S for AEC 2/7 cases. | Annex C separates airport environment (AEC 1/6) from TMZ/Mode S (AEC 2/7). | Annex C Table C.1 (distinct rows). |
| 14 | ARCController.cs | Airspace control heuristic | Considers any non‑G class as controlled; ignores RMZ/TMZ overlays and airspace status (active/inactive). | Query provider for active status and overlays (TMZ/RMZ); expose isTMZ/isRMZ flags to ARC service. | TMZ modifies AEC path; inactive control should not elevate ARC. | Annex C Table C.1 (TMZ rows) and national AIP for activation (process note). |
| 15 | ARCController.cs | Error handling | Always returns 200 OK even when inputs are invalid or out‑of‑scope. | Return 400 with validation errors when outside Annex C scope; 422 when internally inconsistent; include annex_ref in errors. | Clear client feedback reduces misclassification risk in safety cases. | SORA MB Step #4 (deterministic process & evidence). |
| 16 | ARCController.cs | Logging | No structured logging of AEC decisions. | Log { missionId, version, aec_code, density, inputs_hash, annex_ref } at Information level for traceability. | Authorities may request replay of the decision trail. | SORA compliance practice (audit trace); Annex C row trace intent. |
| 17 | ARCController.cs | Versioning clarity | Mixes 2.0 and 2.5 terminology (e.g., 120 m vs 150 m bands) across endpoints and comments. | Document version‑specific thresholds on each endpoint and validate accordingly (500 ft for 2.0; 150 m for 2.5). | Prevents accidental cross‑version leakage in client implementations. | SORA 2.0 Annex C bands (500 ft); SORA 2.5 Annex C bands (150 m). |
| 18 | ARCController.cs | Security / input trust | Controller trusts client‑provided environment and control flags without evidence link. | Require evidence tokens (e.g., airspace snapshot ID) from _airspace provider for critical flags (controlled/TMZ/airport env). | Evidence‑based decisions are necessary for regulatory submissions. | SORA MB Step #4 & dossier expectations (traceable inputs). |
| 19 | ARCController.cs | Documentation | Comments cite 'JARUS Annex C Table 1' inconsistently; some references point to Annex B. | Normalize inline docs: Initial ARC → Annex C Table C.1; Residual reductions → Table C.2; TMPR → Annex E. | Aligns developer guidance with the governing tables to avoid future regressions. | SORA 2.5 Annex C (Tables C.1/C.2); Annex E (TMPR). |

---

## 🎯 PRIORITY BREAKDOWN

### **🔴 CRITICAL (Architectural - Blocks Compliance)**
1. **Constructor DI** (#1) - Wrong service injection (IGRCCalculationService instead of IARCCalculationService)
2. **All Initial/Residual Endpoints** (#2, #3, #4) - Service mis-wiring to GRC instead of ARC
3. **Missing AEC/Density in DTOs** (#12) - Cannot trace to Annex C Table C.1 rows

### **🟠 HIGH (Incorrect AEC Assignment)**
4. **Environment Input Model** (#6) - Missing isAirportHeliportEnvironment, heightAMSL (AEC 1, 6, 11)
5. **ValidateAtPoint** (#8, #9) - Hardcoded defaults, wrong threshold (60m vs 150m)
6. **TMZ/RMZ Detection** (#14) - Not detecting TMZ overlays (affects AEC 2/7)
7. **Composite Aggregation** (#5) - No documented worst-case rule

### **🟡 MEDIUM (Operational/Audit Trail)**
8. **TMPR Endpoints** (#10) - Wrong service (GRC vs air-risk path)
9. **Strategic Mitigations** (#11) - No Table C.2 structure (SM quality levels)
10. **Validation Logic** (#7) - No OUT_OF_SCOPE rejection
11. **Error Handling** (#15) - Always 200 OK (should return 400/422)
12. **Logging** (#16) - No structured AEC decision logging

### **🟢 LOW (Documentation/Clarity)**
13. **Naming Semantics** (#13) - "Near aerodrome" vs "airport environment"
14. **Versioning Clarity** (#17) - Mixed 2.0/2.5 terminology
15. **Input Trust** (#18) - No evidence tokens for critical flags
16. **Documentation** (#19) - Inconsistent Annex references

---

## 🚨 **IMMEDIATE ACTION REQUIRED**

### **1. Service Architecture Fix (BLOCKS ALL ARC)**
**Current Code (Line 16-18):**
```csharp
private readonly IGRCCalculationService _service;
// ...
public ARCController(IGRCCalculationService service, ...)
```

**Required Fix:**
```csharp
private readonly IARCCalculationService _arcService;  // NEW - dedicated ARC service
private readonly IGRCCalculationService _grcService;   // Keep for GRC-only endpoints
// ...
public ARCController(
    IARCCalculationService arcService,
    IGRCCalculationService grcService,  // Optional - only if GRC endpoints exist
    ...
)
{
    _arcService = arcService;
    _grcService = grcService;
    ...
}
```

**Impact**: Currently ALL ARC endpoints call wrong service → incorrect Annex C mapping

---

### **2. Endpoint Rewiring (Lines 34-83)**

**Current Code:**
```csharp
[HttpPost("v2.0/initial")]
public IActionResult Initial_V20([FromBody] ARCInitialInput input)
{
    var result = _service.DetermineInitialARC_V2_0(input);  // ❌ Wrong service
    return Ok(result);
}
```

**Required Fix:**
```csharp
[HttpPost("v2.0/initial")]
public IActionResult Initial_V20([FromBody] ARCInitialInput input)
{
    var result = _arcService.DetermineInitialARC_V2_0(input);  // ✅ Correct service
    
    // Add structured logging
    _logger.LogInformation("ARC 2.0 Initial: AEC={AEC}, Density={Density}, ARC={ARC}", 
        result.AecCode, result.DensityRating, result.InitialArc);
    
    return Ok(result);
}
```

**Apply to ALL endpoints**: v2.0/initial, v2.5/initial, v2.0/residual, v2.5/residual

---

### **3. Response DTO Enhancement**

**Current Response** (implied from code):
```csharp
{ 
    "initialArc": "ARC-d"
}
```

**Required Response** (per CSV #12):
```csharp
{ 
    "initialArc": "ARC-d",
    "aecCode": 3,                           // NEW - AEC number (1-12)
    "densityRating": 5,                     // NEW - Density from Table C.1
    "annexRef": "Annex C Table C.1 Row 3",  // NEW - Source trace
    "aecRowDescriptor": "OPS >500ft AGL but <FL600 in controlled airspace",  // NEW
    "outOfScope": false                     // NEW - Flag for undefined cases
}
```

---

### **4. ValidateAtPoint Fix (Lines 96-113)**

**Current Code:**
```csharp
var env = new ARCEnvironmentInput
{
    AirspaceControl = isControlled ? AirspaceControl.Controlled : AirspaceControl.Uncontrolled,
    LocationType = LocationType.NonAirport,  // ❌ Hardcoded
    Environment = EnvironmentType.Urban,     // ❌ Hardcoded default
    Typicality = AirspaceTypicality.Typical,
    MaxHeightAGL = maxHeightAGL              // ❌ Defaults to 60m (wrong threshold)
};
```

**Required Fix:**
```csharp
// Detect airport environment from CTR/ATZ/TIA
var isAirportEnv = asp.Airspaces.Any(a => 
    a.Type == AirspaceType.CTR || 
    a.Type == AirspaceType.ATZ || 
    a.Type == AirspaceType.TIA ||
    a.IsAirportPolygon);  // Requires airspace provider enhancement

// Detect TMZ/RMZ overlays
var isTMZ = asp.Airspaces.Any(a => a.Type == AirspaceType.TMZ);
var isRMZ = asp.Airspaces.Any(a => a.Type == AirspaceType.RMZ);

// Classify environment from population data (if available)
var envType = await _airspace.ClassifyEnvironmentAsync(point);  // Returns Urban/Rural/Unknown
if (envType == EnvironmentType.Unknown)
{
    _logger.LogWarning("Environment type unknown at {Lat},{Lon} - defaulting to Urban (conservative)", 
        latitude, longitude);
    envType = EnvironmentType.Urban;
}

var env = new ARCEnvironmentInput
{
    AirspaceControl = isControlled ? AirspaceControl.Controlled : AirspaceControl.Uncontrolled,
    IsAirportHeliportEnvironment = isAirportEnv,  // NEW
    IsTMZ = isTMZ,                                 // NEW
    IsRMZ = isRMZ,                                 // NEW
    Environment = envType,                         // Evidence-based
    Typicality = AirspaceTypicality.Typical,
    MaxHeightAGL = maxHeightAGL,                   // Document: 150m threshold for 2.5
    MaxHeightAMSL = point.AltitudeMeters,          // NEW - for FL600 check
    EnvironmentEvidence = envType == EnvironmentType.Unknown ? "Assumed (no data)" : "Population layer"  // NEW
};
```

---

## 📊 **COMPLIANCE IMPACT ANALYSIS**

| Category | Current State | Required State | Gap |
|----------|---------------|----------------|-----|
| **Service Architecture** | ❌ IGRCCalculationService only | ✅ IARCCalculationService dedicated | **CRITICAL** |
| **AEC Traceability** | ❌ No AEC code/density exposed | ✅ Full Table C.1 row trace | **CRITICAL** |
| **Airport Environment** | ❌ Hardcoded NonAirport | ✅ Detected from CTR/ATZ/TIA | **HIGH** |
| **TMZ Detection** | ❌ Not detected | ✅ Overlay detection | **HIGH** |
| **Threshold Accuracy** | ❌ 60m default | ✅ 150m (2.5), 500ft (2.0) | **HIGH** |
| **Strategic Mitigation** | ❌ Unstructured list | ✅ Table C.2 mapping | **MEDIUM** |
| **Error Handling** | ❌ Always 200 OK | ✅ 400/422 with annex_ref | **MEDIUM** |
| **Audit Logging** | ❌ None | ✅ Structured AEC decisions | **MEDIUM** |

---

## 🎯 **RECOMMENDED FIX SEQUENCE**

### **Phase 1: Service Architecture (Day 1)**
1. Create `IARCCalculationService` interface
2. Update constructor DI
3. Rewire all 8 endpoints (Initial/Residual × 2.0/2.5 × Initial/Env)
4. Test service routing

### **Phase 2: Response Enhancement (Day 2)**
5. Update response DTOs (add aec_code, density_rating, annex_ref, out_of_scope)
6. Update service implementation to populate new fields
7. Update API documentation

### **Phase 3: Input Model Extension (Day 3)**
8. Extend `ARCEnvironmentInput` with isAirportHeliportEnvironment, heightAMSL, isTMZ
9. Update ValidateAtPoint to detect airport/TMZ
10. Add population layer classification

### **Phase 4: Operational Hardening (Day 4)**
11. Implement proper error handling (400/422)
12. Add structured logging
13. Add evidence tokens
14. Update documentation

### **Phase 5: Strategic Mitigation Structure (Day 5)**
15. Restructure StrategicMitigations() with Table C.2 mapping
16. Add SM quality levels (SM1-SM4)
17. Add reduction constraints

---

## 📝 **VERIFICATION CHECKLIST**

- [ ] All endpoints call `_arcService` (not `_service`)
- [ ] All responses include `aec_code` (1-12)
- [ ] All responses include `density_rating` (1-5)
- [ ] All responses include `annex_ref` (e.g., "Annex C Table C.1 Row 3")
- [ ] ValidateAtPoint detects airport environment (not hardcoded)
- [ ] ValidateAtPoint detects TMZ/RMZ overlays
- [ ] ValidateAtPoint uses 150m threshold (not 60m default)
- [ ] Composite aggregation documented (worst-case rule)
- [ ] TMPR endpoints cite Annex E (not GRC service)
- [ ] StrategicMitigations returns Table C.2 structure
- [ ] Error responses return 400/422 (not always 200)
- [ ] AEC decisions logged with structured fields
- [ ] Documentation references correct Annex (C for ARC, not B)

---

## 🔗 **OFFICIAL REFERENCES**

All corrections traceable to:
- **JARUS SORA 2.0**: JAR-DEL-WG6-D.04, Annex C Table 1 & Table 2, §2.3.2 Step #4
- **JARUS SORA 2.5**: JAR-DEL-SRM-SORA-MB-2.5, Annex C Table C.1 & C.2, §C.6.3
- **SORA 2.5 Annex E**: TMPR definitions
- **SORA Main Body Step #4**: Evidence requirements and deterministic process

---

**Report Generated**: 2025-10-31  
**Total Issues**: 19 (4 Critical, 4 High, 5 Medium, 6 Low)  
**Estimated Fix Effort**: 5 days (full-time developer)  
**Compliance Risk**: 🔴 **HIGH** - Service mis-wiring blocks Annex C compliance
