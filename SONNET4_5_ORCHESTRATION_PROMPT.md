# SORA Orchestration Service — Sonnet 4.5 Request

Goal: Provide precise, production-safe edits to the .NET orchestration layer to fully align with EASA/JARUS SORA 2.0/2.5 and our Python calculators. Output must be concrete C# patches (unified diff or explicit before/after) for the files listed below, plus any new types as needed. Keep .NET tests green.

## Repository landmarks

- Orchestrator (target):
  - `Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs`
  - `Backend/src/Skyworks.Core/Services/Orchestration/ISORAOrchestrationService.cs`
- Related models/services (for references only):
  - `Backend/src/Skyworks.Core/Models/*`
  - `Backend/src/Skyworks.Core/Services/GRC/*`
  - `Backend/src/Skyworks.Core/Services/ARC/*`
  - `Backend/src/Skyworks.Core/Services/SAILService.cs`, `.../OSOService.cs`, `.../Risk/*`
- Python backend (already updated for compat):
  - GRC 2.0/2.5: `/api/v1/calculate/grc/{version}` returns snake_case and adds aliases `initial_grc`, `mitigation_total`
  - SAIL: `/api/v1/calculate/sail` accepts missing `sora_version` (defaults 2.0) and normalizes ARC tokens
  - ARC legacy routes: `/api/v1/calculate/arc/2.0` and `/api/v1/calculate/arc/2.5` forward to current calculators

## Your calibration (apply to orchestrator)

Paste of `SONNET4_CALIBRATION.txt` (YAML):

```yaml
# SORA Orchestration Service - Sonnet 4.5 Calibration
# Apply these fixes to SORAOrchestrationService.cs

critical_fixes:
  grc_2_0:
    - Remove: "Population density inference from Scenario"
    - Action: "Pass Scenario enum directly to Python engine"
    - Validation: "input.Scenario_V2_0 required, no PD mapping"
    
  grc_2_5:
    - Remove: "MTOM_kg fallback to MaxCharacteristicDimension"
    - Action: "Require explicit MTOM_kg and MaxSpeed - reject if missing"
    - Sub250g: "Apply iGRC=1 only if MTOM ≤ 0.25kg AND speed within bin"
    - Validation: "Fail early with clear error if MTOM or MaxSpeed missing"
  
  arc_inputs:
    - Add: "IsNearAerodrome, DistanceToAerodrome_km, IsInCTR to ARCEnvironmentInput"
    - Units: "2.0 uses feet (convert), 2.5 uses meters (direct)"
    - Normalization: "Suburban → Urban, Industrial → Urban before API call"
    - Remove: "Default MaxHeightAGL=120m (let it be explicit 0 if not set)"
  
  explicit_arc:
    - Restrict: "Only allow in test mode (_allowExplicitARC flag)"
    - Production: "Always derive from AEC decision tree"
    - Flag: "Add constructor parameter allowExplicitARC=false"
  
  mitigation_filtering:
    - Evidence: "Filter U-space, Geofencing tokens from StrategicMitigations"
    - Method: "FilterMitigations() removes evidence-only tokens"
    - Apply: "Before OSO compliance check"
  
  scope_validation:
    - Add: "ReasonCode to ValidateOperationScope return tuple"
    - Codes: "OOS.SAIL_VI, OOS.GRC_GE_6, OOS.GRC5_PLUS_ARCd"
    - Field: "result.OutOfScopeReason for HTTP 400 response"
  
  async_pattern:
    - Remove: "All .Result blocking calls"
    - Pattern: "async/await throughout, keep sync wrapper for compatibility"
    - Method: "ExecuteCompleteAsync() with ExecuteComplete() wrapper"

required_model_changes:
  PythonGRCRequest_2_0:
    - Add: "string Scenario (e.g. 'VLOS_SparselyPopulated')"
    - Remove: "int PopulationDensity, double MTOM_kg, string EnvironmentType"
  
  PythonGRCRequest_2_5:
    - Add: "double MaxSpeed_mps"
    - Ensure: "double MTOM_kg is mandatory"
  
  PythonARCRequest_2_0:
    - Add: "double? DistanceToAerodrome_nm"
    - Add: "bool IsInCTR"
  
  PythonARCRequest_2_5:
    - Add: "double? DistanceToAerodrome_km"
    - Add: "bool IsInCTR"
    - Add: "bool IsNearAerodrome"
  
  ARCEnvironmentInput:
    - Add: "bool IsNearAerodrome"
    - Add: "double? DistanceToAerodrome_km"
    - Add: "bool IsInCTR"
  
  AirRiskInput:
    - Add: "bool? IsNearAerodrome"
    - Add: "double? DistanceToAerodrome_km"
    - Add: "bool? IsInCTR"
    - Add: "double? MaxSpeed (for 2.5)"
  
  SORACompleteResult:
    - Add: "string? OutOfScopeReason"

test_expectations:
  sora_2_0:
    low_risk_rural_vlos:
      inputs: "Class G, ~300ft AGL, rural, no SM"
      expected: "iGRC=3, ARC=b, SAIL=III"
    
    high_altitude_controlled:
      inputs: "Class D, 800ft AGL, urban"
      expected: "ARC=d, iGRC=5, SAIL=V"
  
  sora_2_5:
    sub_250g:
      inputs: "MTOM ≤ 0.25kg, speed within bin"
      expected: "iGRC=1, SAIL=I"
    
    urban_sheltering:
      inputs: "Urban + documented M1A sheltering"
      expected: "iGRC reduced by 1"
  
  sail_matrix:
    grc5_arc_c:
      inputs: "FinalGRC=5, ResidualARC=ARC_c"
      expected: "SAIL=IV (verify matrix cell)"
  
  out_of_scope:
    grc_ge_6:
      inputs: "FinalGRC ≥ 6"
      expected: "HTTP 400, reason='OOS.GRC_GE_6'"
    
    grc5_plus_arcd:
      inputs: "FinalGRC=5, ResidualARC=ARC_d"
      expected: "HTTP 400, reason='OOS.GRC5_PLUS_ARCd'"

controller_changes:
  BadRequestMapping:
    code: |
      if (!result.IsSuccessful && !string.IsNullOrEmpty(result.OutOfScopeReason))
      {
          return BadRequest(new { 
              reason = result.OutOfScopeReason,
              message = result.Errors.FirstOrDefault(),
              errors = result.Errors 
          });
      }
```

## Official references (knowledge)

Cite the following files by name and ground rules against them:
- ARC_CALCULATION_AUTHORITATIVE_REFERENCE.md
- ARC_CALCULATOR_EASA_JARUS_COMPLIANCE_REPORT.md
- ARC_FORMULA_FIX_COMPLETE_REPORT.md
- ARC_IMPLEMENTATION_SPECIFICATION.md
- EASA_JARUS_COMPLIANCE_COMPLETE.md
- JARUS/EASA SAIL table: GM1 to Article 11; SORA 2.0 AMC Table 3; SORA 2.5 Annex A/F
- DUALSORA_VALIDATION_JARUS_COMPLIANCE_AUDIT.md

## Constraints & acceptance criteria

- Do not break existing .NET tests (currently green: 273 total; 272 passed; 1 skipped)
- Produce async-first orchestration with ExecuteCompleteAsync(), preserving ExecuteComplete() wrapper
- Enforce explicit MTOM and MaxSpeed for 2.5; reject missing inputs with clear reason codes
- Restrict explicit ARC to test mode only (_allowExplicitARC flag false by default)
- Add OutOfScopeReason propagation and controller 400 mapping as shown
- Normalize ARC environment inputs per calibration (Suburban→Urban, etc.)
- Provide minimal, isolated changes: prefer adding parameters/overloads over rewrites
- Include code diffs only for files that require edits. If interface changes are needed, include those too.

## What to output

1) A short summary of the approach
2) Unified diffs (or explicit before→after) for:
   - SORAOrchestrationService.cs (mandatory)
   - ISORAOrchestrationService.cs (if needed)
   - Any small model DTOs invoked by orchestration (only as required)
3) Notes on migration (if any), and where to add flags (e.g., DI constructor param allowExplicitARC)

You have full context above. Generate precise, compilable C# edits that satisfy the calibration and constraints.
