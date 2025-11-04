# ACCEPTANCE CRITERIA

1. **Determinism**
   - No RNG; all transitions are integer and reproducible.
2. **Correctness vs Standards**
   - ARC 2.0: iARC via Table C.1; strategic mitigation via Table C.2 (cap ≤1 unless certified segregation).
   - GRC 2.0: M1 (0/-1/-2/-4) with **floor after M1**, M2 (0/-1/-2), M3 (**+1/0/-1**).
   - GRC 2.5: M1A (0/-1/-2/NA), M1B (0/NA/-1/-2), M1C (0/-1/NA/NA), M2 (0/NA/-1/-2).
   - SAIL mapping per official 2D tables (2.0 AMC & 2.5 JARUS).
3. **Validation**
   - Pydantic rejects invalid enums/combos (422).
   - Hypothesis property tests cover edges (caps/floors/clamps).
4. **Traceability**
   - `calculation_trace` includes rule references (doc/annex/section).
   - Version pinning in every response (e.g., `EASA_SORA_2.0_AMC_2024-07`).
5. **Performance & API**
   - O(1) lookups; average call < 10ms locally.
   - FastAPI endpoints documented with OpenAPI, examples & schemas.
