# GRC Mitigation Models — SORA 2.5 Compliance Report (Fix #13)

Date: 2025-11-03
Scope: Documentation alignment for SORA 2.5 mitigations (M1A/M1B/M1C/M2) in `Backend/src/Skyworks.Core/Models/GRC/V2_5/MitigationModels.cs`.

## Sources Reviewed
- JARUS SORA v2.5 — Main Body
  - Section 4.2: Intrinsic GRC (Table 2, Table 3)
  - Section 4.3: Ground Risk Mitigations (Table 5)
- Project implementation mapping:
  - `GRCCalculationService.GetMitigationCredit_V2_5(...)`
  - Unit tests in `Backend/tests/Skyworks.Api.Tests/GRC/GRCCalculationTests_SORA_2_5.cs`

## Summary of Changes (No logic changes)
- Updated XML documentation in `MitigationModels.cs` to match the implemented mitigation credit mapping used by the service and verified by tests.
- Clarified robustness-credit pairs per mitigation and removed the misleading note that "Low" is never credited.
- No public API shape changes (DTOs unchanged). No calculation logic changes.

## Expected vs Actual (Credits per mitigation and robustness)
The table below shows the effective credit mapping enforced by `GetMitigationCredit_V2_5` and verified by tests.

| Mitigation | Robustness | Expected (Spec Interpretation) | Actual (Implementation) |
|---|---|---:|---:|
| M1A — Sheltering | Low | -1 | -1 |
|  | Medium | -2 | -2 |
|  | High | n/a | 0 |
| M1B — Operational Restrictions | Low | 0 | 0 |
|  | Medium | -1 | -1 |
|  | High | -2 | -2 |
| M1C — Ground Observation | Low | -1 | -1 |
|  | Medium | 0 | 0 |
|  | High | 0 | 0 |
| M2 — Impact Dynamics | Low | 0 | 0 |
|  | Medium | -1 | -1 |
|  | High | -2 | -2 |

Notes:
- High for M1A is not credited in the current implementation (falls back to 0). This matches current unit tests.
- "Low not credited" is not universally true; M1A and M1C accept Low with -1.

## Files Touched
- Updated: `Backend/src/Skyworks.Core/Models/GRC/V2_5/MitigationModels.cs`
  - Align XML comments with implemented credit mapping and tests.

## Validation
- Build: PASS
- Tests: PASS (executed default build-and-test task; orchestration ensures Python FastAPI service availability)
- Typecheck/Lint: n/a (no codegen or analyzer warnings introduced)

## Compatibility & API Notes
- JSON enums are serialized as strings (global `JsonStringEnumConverter` is registered in `Skyworks.Api/Program.cs`).
- DTO contracts unchanged: `AppliedMitigation`, `FinalGRCInput`, `FinalGRCResult`, and `GroundRiskMitigation` enum.

## Next Steps
- If future official clarifications adjust Table 5 credits (e.g., defining High for M1A), update `GetMitigationCredit_V2_5` and expand unit tests accordingly.
- Optionally add a public-facing specification reference page summarizing all SORA 2.5 mappings (Table 2/3/5, micro-UAS rule) for traceability.
