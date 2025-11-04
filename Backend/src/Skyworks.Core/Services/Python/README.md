# Python Calculation Client

A lightweight .NET HttpClient wrapper for the Python FastAPI calculation service. It does not compute GRC/ARC/SAIL itself; it forwards requests to the authoritative Python endpoints and normalizes responses.

## Endpoints used

- SORA 2.0 GRC: POST /api/v1/calculate/grc/2.0
  - Body (subset): { mtom_kg, population_density, m1_strategic?, m2_impact?, m3_erp? }
  - Response: { intrinsic_grc, final_grc, m1_effect, m2_effect, m3_effect, notes, source }

- SORA 2.5 GRC: POST /api/v1/calculate/grc/2.5
  - Body (subset): { mtom_kg, population_density, max_characteristic_dimension_m?, max_speed_ms?, m1a_sheltering?, m1b_operational?, m1c_ground_observation?, m2_impact? }
  - Response: { intrinsic_grc, final_grc, m1_effect, m2_effect, notes, source }

- SAIL (2.0/2.5): POST /api/v1/calculate/sail
  - Body: { sora_version, final_grc, residual_arc }
  - Response (normal): { sail, final_grc, final_arc (a|b|c|d), residual_arc, oso_count, sora_version }
  - Response (Category C): { category: "C", sail: null, oso_count: null, final_grc, residual_arc }

## Configuration

- Appsettings: PythonService:BaseUrl (default: http://localhost:8001)
- Environment variable fallback: PYTHON_API_BASE

## Data flow (high-level)

Controller/Orchestration → IPythonCalculationClient → Python FastAPI → JSON → Normalize & return: intrinsic_grc, final_grc (and extras)

## Notes

- The client normalizes string levels (e.g., M1/M2 Low/Medium/High) to the exact tokens the Python expects.
- For SORA 2.5 M2, "Low" is mapped to "None" (per Annex F constraints).
- Response parsing is tolerant to minor schema variations (e.g., intrinsic_grc vs initial_grc) and case.
