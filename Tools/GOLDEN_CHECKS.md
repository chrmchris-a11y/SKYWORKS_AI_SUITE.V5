# Golden Checks: SORA Endpoints

Quick smoke to verify parity and health across Python FastAPI (8001) and .NET API (5210).

What it runs:
- GET .NET /api/sora/info
- GET Python /health
- POST Python SORA 2.0 (GRC=5, ARC=b) → expect SAIL IV, OSO count 18
- POST Python SORA 2.5 (GRC=10, residualArcLevel=10) → expect SAIL VI, OSO count null
- POST .NET SORA 2.0 (GRC=5, residualArc=c) → expect SAIL V, OSO count present
- POST .NET SORA 2.5 (GRC=10, residualArcLevel=10) → expect SAIL VI, OSO count omitted

How to run (as a VS Code task):
1. Ensure Python FastAPI and .NET API are running (tasks start them automatically if you use the combined launch or the task dependsOn).
2. Run the task: "Golden Checks: SORA endpoints".

Outputs are printed to the terminal; non-200 responses will include the error body for quick triage.

Notes:
- 2.0 uses letter ARC (a–d) and returns `oso_count`.
- 2.5 uses numeric residual ARC (1–10) and suppresses OSO count in API responses by design.
- High GRC (9–10) maps to SAIL VI regardless of ARC in 2.5.
