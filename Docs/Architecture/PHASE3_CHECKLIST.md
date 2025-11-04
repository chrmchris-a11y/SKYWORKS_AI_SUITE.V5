# Phase 3 — ARC Engine Development (Steps 21–30)

Goal: Air Risk Classification per Annex B, with Residual ARC per Annex C (strategic mitigations).

Scope and acceptance criteria
- Step 21: ARC Basic Algorithms
  - Define ARCInput/ARCOutput (Annex B). Initial ARC computed from: airspace control, location type, typicality, height AGL, special areas.
  - Residual ARC pipeline: Initial (Annex B) → Strategic mitigations (Annex C S1–S4, caps, no double counting) → TMPR level.
  - Unit tests: happy path + edge cases (airport/heliport → D, atypical/segregated → A).
- Step 22: Airspace Charts Integration
  - Interface: IAirspaceProvider { GetAirspaceAt(point/volume), GetSpecialUseAreas(polygon) }.
  - Demo static provider with 2–3 CTR/ATZ polygons. Controller endpoints and tests.
- Step 23: ARC Compliance Validation
  - Validators for Annex B rules (controlled vs uncontrolled, airport/heliport proximity, altitude limits). Return warnings/errors in notes.
- Step 24: Composite ARC Computations
  - Multi-segment and time-window aggregation (max ARC per segment; optional exposure-weighted). Tests with 3-segment mission.
- Step 25: ARC Documentation
  - Docs/API updated with Annex B examples + JSON Schemas. Add Annex mapping quick reference (B vs C responsibilities).
- Step 26: Real-time Processing ✅
  - Minimal streaming endpoint (SignalR or SSE) that recomputes ARC for moving points at ≤1 Hz. Debounce + memoized airspace queries.
  - ✅ SignalR hub at /hubs/arc με Subscribe(sessionId) και Telemetry(update)
  - ✅ Per-session debounce (500ms/DroneId) και airspace cache (3s TTL)
  - ✅ StreamingArcService: ProcessTelemetry_V2_5Async με environment inference
  - ✅ Frontend demo: streaming.html με Connect/Subscribe/Send/Burst
  - ✅ Unit tests: Cache validation με FakeAirspaceProvider
  - ✅ Docs: Section 12 στο SORA_Endpoints_Reference.md
- Step 27: Weather Data APIs ✅
  - IWeatherProvider (METAR/TAF or Open‑Meteo fallback). Weather gates that influence ARC notes (e.g., visibility < VMC thresholds).
  - ✅ WeatherModels: WeatherConditions, VMCCriteria (EASA minima), WeatherRiskGate, WeatherRequest, WeatherQueryResult
  - ✅ IWeatherProvider interface: GetWeatherAsync, GetForecastAsync, EvaluateWeatherGatesAsync
  - ✅ StaticWeatherProvider: 4 demo stations (LCLK, LCPH, LGAV, DEMO_LIMASSOL), VMC/Wind/Gust/Precipitation gates
  - ✅ WeatherController: /api/weather/current, /api/weather/forecast, /api/weather/evaluate
  - ✅ DI registration στο Program.cs
  - ✅ Unit tests: 10 weather tests (VMC criteria, gate evaluation, nearest station)
  - ✅ Docs: Section 13 στο SORA_Endpoints_Reference.md
  - ⏳ TODO (future): Integration με ARC calculation (weather gates στα ARC notes)
- Step 28: Appendix Compliance ✅
  - Checklist for U‑space/NOTAM/ATC coordination where applicable. Expose in ARC notes as "operator obligations".
  - ✅ ComplianceModels: ComplianceObligation, ComplianceChecklistItem, OperatorReadiness, ComplianceRequest/Result
  - ✅ IComplianceProvider interface: GetComplianceObligationsAsync, ValidateOperatorReadinessAsync, GetPreFlightChecklistAsync
  - ✅ StaticComplianceProvider: Airspace obligations (CTR/ATZ/TMA/Restricted), ARC obligations (OSO/TMPR), U-space, BVLOS, Special use areas
  - ✅ OSO/TMPR mapping: ARC-a→Low/TMPR1, ARC-b→Medium/TMPR1, ARC-c→High/TMPR2, ARC-d→High/TMPR3-4
  - ✅ Pre-flight checklists: Weather, NOTAM, ATC, Equipment (Remote ID, Geo-fence, RTH, Parachute, DAA), Documentation
  - ✅ ComplianceController: /api/compliance/obligations, /api/compliance/checklist, /api/compliance/readiness
  - ✅ DI registration στο Program.cs
  - ✅ Unit tests: 11 compliance tests (obligations, checklists, readiness, ARC-based requirements)
  - ✅ Docs: Compliance section στο SORA_Endpoints_Reference.md (inline με existing compliance endpoints)
  - ⏳ TODO (future): Integration με ARC calculation (compliance obligations στα ARC notes)
- Step 29: Traffic Models ✅
  - Prototype traffic density heuristic (ADS‑B/OpenSky snapshot) to annotate residual ARC notes. Pluggable ITrafficProvider.
  - ✅ TrafficModels: TrafficTarget (ICAO24, callsign, position, velocity, type), TrafficDensity (count, density level, avg altitude)
  - ✅ TrafficDensityThresholds: Low (≤2), Medium (3-5), High (6-10), Very High (>10 targets per 5km radius)
  - ✅ ITrafficProvider interface: GetTrafficDensityAsync, GetNearbyTargetsAsync
  - ✅ StaticTrafficProvider: 8 demo targets (Cyprus: CYP123, 5BFLY, HELI01, DRONE01, GLIDE1; Athens: AEE345, SXFLY, WZZ1234)
  - ✅ Filtering: Radius, altitude range (MinAltitudeMsl/MaxAltitudeMsl), target type (Aircraft/Helicopter/Drone/Glider)
  - ✅ Risk notes: Low→"Low density", High→"Consider DAA", Very High→"Enhanced TMPR required"
  - ✅ TrafficController: /api/traffic/density, /api/traffic/nearby
  - ✅ DI registration στο Program.cs
  - ✅ Unit tests: 14 traffic tests (density calculation, filtering, ordering, risk notes, thresholds)
  - ⏳ TODO (future): Integration με ARC calculation (traffic density στα ARC notes), Live OpenSky API integration
- Step 30: Integrated ARC Testing ⚠️
  - End‑to‑end tests with airspace + weather + traffic mocks; golden inputs/outputs frozen.
  - ⚠️ STATUS: Partially complete
  - ✅ All subsystems tested independently (148 unit tests pass in 3 seconds)
  - ✅ APIs functional: Weather, Traffic, Compliance all expose working endpoints
  - ✅ Manual integration testing possible via API calls
  - ❌ Automated E2E tests NOT created (complexity too high - would require full ARC pipeline implementation)
  - ❌ Frontend integration UI NOT created (no unified page combining Weather + Traffic + Compliance + ARC)
  - ⏳ RECOMMENDATION: Step 30 completion deferred to Phase 4 (requires integrated mission planner UI)
  - 📋 See PHASE3_VERIFICATION_GUIDE.md for complete testing instructions

Dependencies and prep
- Optional API keys: OpenSky, Open‑Meteo (or similar). No hard dependency for initial merge; mocks provided.
- Coordinate ref system: WGS‑84 lat/lon (EPSG:4326) with geometry ops via NetTopologySuite if needed.

Milestone definition (done when)
- All steps have tests green and Docs updated. Controllers expose required endpoints with versioning. Code paths tagged with Annex references in notes.
