# Sonnet 4.5 – Skyworks SORA Hardening Super-Prompt (Copy/Paste into your model)

This prompt describes, end-to-end, what to implement or fix to make the Skyworks SORA stack production-ready and fully green. It includes exact requirements, authoritative standards references (JARUS/EASA), concrete file paths, API contracts, UI/i18n fixes, and acceptance criteria.

IMPORTANT
- Do NOT invent behavior outside what is specified here. If ambiguity exists, prefer strict interpretation of the cited standards.
- Keep public API shapes stable. Use explicit 400s for invalid inputs. No non-authoritative fallbacks in production.
- Centralize SAIL logic behind the authoritative router; remove drift.

---

## 1) Repository context and architecture

Tech stack
- Backend .NET API (serves static web and proxies logic)
- Python FastAPI service for SORA logic (Backend_Python)
- Frontend static pages under /app/Pages with JS modules and i18n JSON files

Key backend Python packages
- Pydantic v2 models
- FastAPI

Authoritative SAIL logic centralized behind Python router (the app endpoint proxies it). No duplicate SAIL logic outside the router.

---

## 2) Standards – Authoritative references to implement

SORA 2.5 (JARUS – NEW)
- Annex D – Table 7: Residual ARC numeric mapping for Step #9
- Annex F – Quantitative GRC inputs (upstream of SAIL and GRC derivation)
- Main body Table 3 – Qualitative descriptors for population density (used by UI aids)

SORA 2.0 (Legacy AMC / EASA AMC/GM SORA)
- Use an authoritative table equivalent to UK SORA AMC Table 6 for SAIL mapping: GRC 1..7 × ARC a–d → SAIL I..VI
- Category C guard: If GRC > 7 then Category C (no SAIL). Return 200 with sail_level = null and category = "C".

Non-negotiable compliance rules
- SORA 2.5 MUST use numeric residual ARC in [1..10]; letters are invalid.
- SORA 2.0 MUST use letter ARC in {a,b,c,d}; numeric residual ARC is invalid.
- No silent coercions. Return 400 with clear, actionable error messages on invalid combos.

---

## 3) Observed issues to fix (from UI console and current code state)

UI console logs (representative)
- mission.html: scripts load correctly
- i18n loader attempts /i18n/en.json (404), then fallback ../i18n/en.json loads
- Missing keys previously logged:
  - missionPlanner.mission.populationDensity
  - ui.editManually
  - missionPlanner.category.SORA-2.5
- /favicon.ico 404
- Earlier we saw /api/drones/dropdown failed in some runs; current run reports "Loaded 64 drones successfully" (keep endpoint reliable)

What to fix on UI/i18n
- Ensure the app serves /i18n/en.json and /i18n/el.json at both relative (../i18n/*.json) and root (/i18n/*.json) paths or update the loader to request the relative path only. Do not rely on 404 + fallback in production.
- Add/ensure the keys exist in both en.json and el.json:
  - missionPlanner.mission.populationDensity
  - ui.editManually (nested under { "ui": { "editManually": "..." } })
  - missionPlanner.category.SORA-2.5 (under missionPlanner.category)
- Provide /favicon.ico (basic static asset) to remove 404 noise.

Backend/API reliability items
- /api/drones/dropdown must be consistently available. If it depends on a cache/file, ensure initialization at API start and return 200 or a small curated fallback payload (not a fetch error).

---

## 4) Python service – Contracts and strict validation

Request/Response models (Pydantic v2)
- Request (SAILCalculationRequest):
  - sora_version: enum { SORA_2.0, SORA_2.5 }
  - grc_level: int
  - SORA 2.0: arc_level required ∈ {a,b,c,d}; residual_arc_level forbidden
  - SORA 2.5: residual_arc_level required ∈ [1..10]; arc_level forbidden
  - extra fields: forbid
  - error messages must be explicit and actionable
- Response (SAILCalculationResponse):
  - sail_level: enum { I, II, III, IV, V, VI } or null (for Category C)
  - sail (deprecated alias) must mirror sail_level
  - sora_version, grc_level, arc_level (echo for 2.0), residual_arc_level (echo for 2.5)
  - category: "C" only when 2.0 and GRC > 7
  - oso_count: optional; do not compute here (non-authoritative)
  - reference, warnings: optional
  - Keep enums in-model (serializers will output strings); tests may access .value

Router endpoint behavior
- Single public SAIL endpoint – inspects sora_version and routes to the respective calculator
- SORA 2.0 with grc_level > 7: return 200 with body { sail_level: null, category: "C", reference: "SORA 2.0 AMC Category C (GRC>7)" }
- SORA 2.5: enforce numeric residual ARC; reject letters with 400

Calculators
- SORA 2.0 calculator:
  - Input: grc:int in [1..7], arc: a|b|c|d
  - Mapping: flat dictionary keyed by (grc:int, arc:str) → SAIL "I..VI"
  - Use explicit lookup; no iteration over ranges
- SORA 2.5 calculator:
  - Input: grc:int in [1..10], residual_arc:int in [1..10]
  - Use JARUS Annex D Table 7 for Step #9 mapping
  - High-GRC rule: if grc ≥ 9 then SAIL = VI regardless of residual

Tests (Python)
- Validation tests:
  - 2.0 accepts letters and forbids residual_arc_level
  - 2.5 accepts residual_arc 1..10 and forbids letters
- 2.0 calculator tests:
  - Verify all (GRC 1..7 × ARC a–d) against the authoritative mapping
  - Verify Category C for GRC>7 returns 200 with null SAIL and category "C"
- 2.5 calculator tests:
  - Verify numeric cases against Annex D Table 7 using an authoritative function or table
  - Verify GRC ≥ 9 rule → SAIL VI for any residual ARC

---

## 5) Data – Authoritative SORA 2.0 mapping table

Implement a flat map similar to:

```
# key: (grc:int, arc:str) → value: "I".."VI"
SAIL_TABLE_20 = {
  (1, "a"): "I",  (1, "b"): "II", (1, "c"): "IV", (1, "d"): "VI",
  (2, "a"): "I",  (2, "b"): "I",  (2, "c"): "II", (2, "d"): "III",
  (3, "a"): "II", (3, "b"): "II", (3, "c"): "III", (3, "d"): "IV",
  (4, "a"): "II", (4, "b"): "II", (4, "c"): "III", (4, "d"): "IV",
  (5, "a"): "III",(5, "b"): "III",(5, "c"): "IV", (5, "d"): "V",
  (6, "a"): "IV", (6, "b"): "IV", (6, "c"): "V",  (6, "d"): "VI",
  (7, "a"): "V",  (7, "b"): "V",  (7, "c"): "VI", (7, "d"): "VI",
}
```

OSO counts (if needed elsewhere) may be maintained in a separate map keyed by SAIL level, but the calculator response should not compute or assert them (non-authoritative at this layer).

---

## 6) UI/i18n – exact fixes

Paths and loading
- Ensure the app serves i18n files at /i18n/en.json and /i18n/el.json. Alternatively, change the loader to always fetch relative path ../i18n/en.json and ../i18n/el.json to avoid root 404s.
- Provide /favicon.ico to eliminate 404.

Keys to ensure exist (both languages)
- missionPlanner.mission.populationDensity
- missionPlanner.category.SORA-2.5
- ui.editManually (nested under "ui")

Mission page behavior
- On first load, do not block calculations due to i18n failures. If a key is missing, log once and continue.
- Confirm the category switches (SORA-2.5 vs SORA-2.0) toggle the proper field sets (population density + ARC 2.5 numeric vs scenario dropdown for 2.0).

---

## 7) .NET API and static hosting

- Ensure static files include i18n directory and favicon.
- The route /api/drones/dropdown should always return 200 with data (either real or curated fallback) to prevent UI hard-failure. Log but do not break the flow.

---

## 8) Non-functional and production semantics

- No ad-hoc or non-authoritative fallbacks in production for SAIL math.
- Return strict 400s for invalid inputs, with specific error messages (e.g., “SORA 2.5 requires residual_arc_level (1..10) and forbids letter arc_level”).
- Keep enums in responses so tests can use .value while serializers output strings.
- Centralize SAIL calculation behind one endpoint; do not duplicate SAIL logic across handlers, services, or UI.

---

## 9) Acceptance criteria (must all be green)

- Python unit tests: 100% pass, covering 2.0, 2.5 and validators.
- .NET build and tests: pass without file-lock errors.
- Web smoke tests: pass for mission.html, Swagger, Proxora health, Composite SORA proxy.
- UI console: no 404 for /i18n/en.json or favicon; no missing keys for the three keys listed.
- Category C (SORA 2.0, GRC>7): returns 200 with sail_level=null, category="C".
- High-GRC rule (SORA 2.5, GRC≥9): returns SAIL VI regardless of residual ARC.

---

## 10) File paths and modules to update

Python (Backend_Python)
- sail/models/sail_models.py — request/response models with validators (extra=forbid), enums preserved
- sail/api/sail_api.py — router endpoint version switching; Category C logic for 2.0 (GRC>7)
- sail/calculators/sail_calculator.py — 2.0 flat map lookup; 2.5 numeric residual handling + high-GRC rule
- sail/data/sail_tables_20.py — flat mapping table (grc, arc) → SAIL
- sail/tests/* — numeric-only tests for 2.5, letter-only tests for 2.0; schema validator tests

Frontend
- app/Pages/mission.html — ensure it references i18n loader consistently; do not block on 404
- Frontend/i18n/en.json — include keys: missionPlanner.mission.populationDensity, missionPlanner.category.SORA-2.5, ui.editManually
- Frontend/i18n/el.json — same keys as above
- Static /favicon.ico — add to static hosting set

.NET Backend
- Ensure wwwroot (or equivalent) includes i18n directory and favicon.
- Ensure /api/drones/dropdown returns consistent data (and/or provide a small fallback)

---

## 11) Explicit error messages (samples)

- For SORA 2.5 with arc_level provided: 400
  - message: "arc_level (letters) is not allowed for SORA 2.5; provide residual_arc_level (1..10)"
- For SORA 2.5 with residual_arc_level missing or out of range: 400
  - message: "residual_arc_level (1..10) is required for SORA 2.5"
- For SORA 2.0 with residual_arc_level provided: 400
  - message: "residual_arc_level is not allowed for SORA 2.0; provide arc_level in {a,b,c,d}"
- For SORA 2.0 with grc_level > 7: 200
  - body: { sail_level: null, category: "C", reference: "SORA 2.0 AMC Category C (GRC>7)" }

---

## 12) Minimal decision log (keep concise in code comments)

- We preserve enums in response models so tests accessing .value continue to work; JSON outputs remain strings.
- We enforce version-aware validation in the schema layer to fail fast.
- We use a flat 2.0 SAIL table keyed by (grc, arc) to avoid range-based ambiguity.
- We keep SAIL logic only in the calculators and router; UI only consumes results.

---

## 13) Verification plan (runbook)

- Python: run pytest in Backend_Python (all tests green)
- .NET: build and test Skyworks.sln (no file lock errors)
- Web smoke tests script: Tools/web-smoke-tests.ps1 (OK on mission, Swagger, Proxora, Composite SORA proxy)
- Manual UI check: open http://localhost:5210/app/Pages/mission.html; avoid file://
  - Open DevTools Console: verify no i18n 404s or missing keys for the 3 keys
  - Switch between SORA-2.5 and SORA-2.0, observe fields and calculations

---

## 14) Out-of-scope

- Do not introduce new product features.
- Do not change public API names or routes.
- Do not relax validation in production.

---

## 15) Final note about secrets

- Do NOT commit any API keys or secrets into the repository. If a key is required to call an external model, request it interactively or via environment variables/secrets store.

---

## Appendix A: Current UI console errors (to address explicitly)

Raw logs (as observed in browser DevTools):

```
mission.html:23 [mission.html] Scripts loaded, checking for window.i18n...
i18n-loader.js?v=20251025-FINAL:284 [i18n] DOMContentLoaded fired, loading translations
arc25-validation.js?v=20251030:144 ✅ SORA 2.5 ARC fields initialized
mission.html:1724 [Init] Running initial mitigation calculations...
mission.html:2364 Category changed to: SORA-2.5
mission.html:3046 [iGRC] Special rule: ≤250g & ≤25m/s → iGRC = 1
mission.html:2406 ✅ SORA 2.5: Showing population density + ARC 2.5 fields, hiding scenario dropdown
:5210/i18n/en.json:1  Failed to load resource: the server responded with a status of 404 (Not Found)
i18n-loader.js?v=20251025-FINAL:92  [i18n] Failed to load from /i18n/en.json: HTTP 404
mission.html:1627 [Drone Load] Loaded 64 drones successfully
test-mission-loader.js?v=20251025:17 Loaded 7 test missions
i18n-loader.js?v=20251025-FINAL:82 [i18n] Loaded language: en from ../i18n/en.json
i18n-loader.js?v=20251025-FINAL:162  [i18n] Missing key (all langs): missionPlanner.mission.populationDensity
i18n-loader.js?v=20251025-FINAL:162  [i18n] Missing key (all langs): ui.editManually
language-switcher.js?v=20251025:250 [LanguageSwitcher] i18nReady event received, rendering switcher
mission.html:3046 [iGRC] Special rule: ≤250g & ≤25m/s → iGRC = 1
i18n-loader.js?v=20251025-FINAL:289 [i18n] Ready event dispatched
:5210/favicon.ico:1  Failed to load resource: the server responded with a status of 404 (Not Found)
mission.html:28 [mission.html] window.i18n is defined correctly
i18n-loader.js?v=20251025-FINAL:162  [i18n] Missing key (all langs): missionPlanner.category.SORA-2.5
```

Required fixes:
- Serve /i18n/en.json and /i18n/el.json at the root path OR adjust loader to use only the relative path (../i18n/en.json). Avoid 404 + fallback in production.
- Ensure the missing keys exist in both files: missionPlanner.mission.populationDensity, ui.editManually (nested under ui), missionPlanner.category.SORA-2.5.
- Provide /favicon.ico via static hosting.

---

## Appendix B: i18n path handling (design option)

Option 1 (preferred): Keep loader requests relative: `../i18n/en.json` and `../i18n/el.json` and remove the root `/i18n/...` fetch to avoid duplicate calls and root 404s.

Option 2: Configure .NET static file hosting to expose `Frontend/i18n` at the web root `/i18n` and ensure mission.html builds the correct href base.

In both options, add a small in-memory cache with ETag support for the i18n JSON.

---

## Appendix C: Fully specified fix list with file-by-file instructions (do these exactly)

Backend_Python (strict contracts and calculators)
1) `Backend_Python/sail/models/sail_models.py`
  - Ensure SORAVersion enum has SORA_2_0 and SORA_2_5 (string values).
  - SAILCalculationRequest validators:
    - For 2.0: arc_level required (enum a,b,c,d); residual_arc_level forbidden; grc_level >= 1.
    - For 2.5: residual_arc_level required in 1..10; arc_level forbidden; grc_level in 1..10.
    - Extra fields: forbid.
  - SAILCalculationResponse:
    - Keep enums (do not flatten to strings in-model); serializer can output strings.
    - Include deprecated alias `sail` mirroring `sail_level`.
    - category: Optional["C"].
    - Ensure field validators coerce legacy enum inputs for `grc_level`.

2) `Backend_Python/sail/data/sail_tables_20.py`
  - Provide authoritative flat map `SAIL_TABLE_20: dict[tuple[int,str], str]` with keys (grc, arc) and values "I".."VI" exactly:
    - (1, a)->I, (1,b)->II, (1,c)->IV, (1,d)->VI
    - (2, a)->I, (2,b)->I, (2,c)->II, (2,d)->III
    - (3, a)->II, (3,b)->II, (3,c)->III, (3,d)->IV
    - (4, a)->II, (4,b)->II, (4,c)->III, (4,d)->IV
    - (5, a)->III, (5,b)->III, (5,c)->IV, (5,d)->V
    - (6, a)->IV, (6,b)->IV, (6,c)->V, (6,d)->VI
    - (7, a)->V, (7,b)->V, (7,c)->VI, (7,d)->VI
  - Provide `OSO_BY_SAIL_20` if needed elsewhere (not authoritative here).

3) `Backend_Python/sail/calculators/sail_calculator.py`
  - 2.0 path: Get `arc = request.arc_level`; `grc = int(request.grc_level)`; `sail_str = SAIL_TABLE_20[(grc, arc_str)]`; `sail_level = SAILLevel(sail_str)`.
  - 2.5 path: If `grc >= 9` then `sail_level = SAILLevel.VI`; else delegate to authoritative `calculate_sail_v25(grc, residual)`.
  - Do not compute OSO counts here.

4) `Backend_Python/sail/api/sail_api.py`
  - Switch by `sora_version`.
  - For 2.0 and `grc > 7`, return 200 with `{ sail_level: None, category: "C", reference: "SORA 2.0 AMC – Category C (GRC>7)" }`.
  - Else call calculator and return canonical response.

5) Tests under `Backend_Python/sail/tests/`
  - 2.5 tests: numeric-only residual ARC; reject letter ARC; test GRC≥9 => VI.
  - 2.0 tests: cover all (GRC 1..7 × a..d) against `SAIL_TABLE_20`.
  - Validation tests for mutual exclusivity and extra=forbid.

Frontend (i18n and static paths)
6) `Frontend/i18n/en.json`
  - Ensure keys:
    - `missionPlanner.mission.populationDensity`
    - `missionPlanner.category.SORA-2.5`
    - `ui.editManually` (nested under `ui`)
7) `Frontend/i18n/el.json` → same keys as above.
8) `app/Pages/mission.html`
  - i18n loader: fetch relative path `../i18n/en.json` and `../i18n/el.json`; remove extra root `/i18n/...` request.
  - Ensure hard errors from i18n do not block calculations; log once and continue.
  - Confirm SORA-2.5 toggles numeric residual ARC fields and hides 2.0 scenario dropdown.
9) Static assets
  - Provide `/favicon.ico` via static hosting.
  - Ensure `i18n/` is exposed under the served root if the loader expects root paths; otherwise make loader use relative paths only.

.NET Backend
10) Static hosting configuration
  - Include `Frontend/i18n` under static files; map to `/i18n` or update loader to use relative URLs.
  - Add `/favicon.ico` to static set.
11) API reliability
  - `/api/drones/dropdown` should initialize on startup and return stable 200 payload; if data source unavailable, serve curated fallback JSON (not HTTP error).

---

## Appendix D: Compliance crosswalk (what maps to which standard clause)

- SORA 2.5 residual ARC numeric constraint → JARUS SORA v2.5 Annex D (Table 7) – Step #9.
- High GRC rule (≥9 → VI) → Implementation rule derived from Annex D structure; document rationale in code comment.
- Quantitative GRC inputs (used upstream) → JARUS SORA v2.5 Annex F. Ensure inputs collected/validated prior to SAIL stage.
- SORA 2.0 SAIL mapping (GRC 1..7 × a–d) → AMC table equivalent to UK SORA AMC Table 6.
- Category C for GRC>7 in 2.0 → AMC/GM guidance: No SAIL, Category C branch with proper justification.

---

## Appendix E: Troubleshooting guide

Symptoms and fixes
1) UI console shows `/i18n/en.json 404` then fallback loads
  - Fix: Load relative path only, or host i18n under `/i18n` root; eliminate the failing request.
2) UI console shows missing keys (populationDensity, SORA-2.5, editManually)
  - Fix: Add keys in both en.json and el.json exactly at the paths above.
3) UI shows `Failed to fetch` for `/api/drones/dropdown`
  - Fix: Ensure backend route is initialized on startup; if needed, serve fallback JSON; never return network error to UI for initial render.
4) Python tests failing with enum `.value` access
  - Fix: Keep enums in response models (do not `use_enum_values=True` for response model), so tests can access `.value`.
5) TypeError in 2.0 calculator (int vs str on ranges)
  - Fix: Use flat dict lookup with key `(grc, arc)`; remove range iteration.

---

## Appendix F: Execution runbook (PowerShell)

1) Start services
```
pwsh -NoProfile -Command "& { cd '${workspaceFolder}\\Backend'; dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210 }"
pwsh -NoProfile -Command "& { cd '${workspaceFolder}\\Backend_Python'; python -m uvicorn main:app --host 0.0.0.0 --port 8001 }"
```

2) Tests
```
pwsh -NoProfile -Command "& { cd '${workspaceFolder}\\Backend_Python'; pytest -q }"
pwsh -NoProfile -Command "& { cd '${workspaceFolder}\\Backend'; dotnet build Skyworks.sln; dotnet test Skyworks.sln --verbosity minimal }"
```

3) Web smoke
```
pwsh -NoProfile -ExecutionPolicy Bypass -File "${workspaceFolder}\\Tools\\web-smoke-tests.ps1" -BaseUrl http://localhost:5210
```

---

## Appendix G: Deliverables checklist (tick all before submitting)

- [ ] 2.0/2.5 validators implemented exactly as specified
- [ ] 2.0 SAIL calculator uses flat map lookup
- [ ] 2.5 SAIL calculator uses numeric residual ARC; high-GRC rule implemented
- [ ] Router returns Category C for 2.0 GRC>7 (200 with null SAIL + category "C")
- [ ] i18n keys exist in both en.json and el.json
- [ ] /i18n paths or loader adjusted to avoid 404s; favicon served
- [ ] Python tests green; .NET build/tests green; smoke tests OK

---

## Appendix H: Official documents to consult (sections/clauses)

These references must be used as authoritative sources for rules and mappings. If multiple editions exist, prefer the latest official release.

1) JARUS SORA, Latest Version (2.5)
   - Annex D: Step #9 Residual ARC Mapping
     - Table 7: Residual ARC (numeric 1..10) mapping used to derive SAIL
   - Annex F: Quantitative GRC
     - Input variables and quantitative methods for GRC; these are upstream of SAIL and must be integrated in the overall pipeline (router enforces upstream preconditions).
   - Main Body: Table 3
     - Qualitative descriptors for population density (used by UI help texts and validations).

2) EASA AMC/GM to Part-UAS — SORA (Legacy 2.0 semantics)
   - AMC tables for SORA 2.0 SAIL mapping
     - Use an authoritative equivalent of UK SORA AMC Table 6: GRC 1..7 × ARC a–d → SAIL I..VI
   - Category C guidance
     - For GRC > 7 return Category C (no SAIL). Application: return 200 with sail_level = null and category = "C" in the API.

3) UK CAA — SORA AMC (reference copy)
   - Table 6: SAIL Mapping (GRC × ARC → SAIL)
   - Use as the authoritative mapping for SORA 2.0 implementation in this project.

4) EASA/EU Profile notes (if applicable)
   - Ensure EU-profile adaptations do not relax SORA mapping semantics; they may constrain inputs (e.g., population density buckets) or add UI hints.

Implementation note: Cite document name and table in code comments near the mapping tables and high-GRC rule; include a short rationale where behavior is inferred (e.g., grc≥9→VI rule derived from Annex D structure).

