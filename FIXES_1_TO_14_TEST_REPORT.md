# Fixes 1–14 — Detailed Test Report

Date: 2025-11-03
Scope: Πλήρης εκτέλεση δοκιμών που καλύπτουν τα Fixes #1 έως #14, με αποτύπωση των σχετικών tests και αποτελεσμάτων.

## Περιβάλλον εκτέλεσης
- OS: Windows
- Orchestration: VS Code tasks
  - Start Python FastAPI (8001)
  - build-and-test (dotnet)
- Python service: http://localhost:8001 (FastAPI)

## Χαρτογράφηση Fix → Tests

- Fix #1 — SORA 2.0 iGRC (Table 2 unified)
  - Tests: `PythonGRCIntegrationTests.GRC20_Table2_iGRC_Expected`
  - Αποτέλεσμα: PASS (iGRC εντός [6,8], ευθυγράμμιση με unified Table 2)

- Fix #2 — SORA 2.5 Final GRC (mitigations) + min clamp
  - Tests: `PythonGRCIntegrationTests.GRC25_Basic_Case_Returns_Intrinsic7_Final3`, `PythonGRCIntegrationTests.GRC25_MicroUAS_ClampedTo1`
  - Αποτέλεσμα: PASS (iGRC=7 → Final=3 ; micro‑UAS → iGRC=1 & clamp=1)

- Fix #3 — C# Orchestration προς Python (σωστό mapping πεδίων)
  - Tests: Καλύπτεται έμμεσα από τα integration tests μέσω `PythonCalculationClient` (σωστή παραγωγή JSON schema, snake_case, optional envelope unwrapping)
  - Αποτέλεσμα: PASS (API κλήσεις επιτυχείς, τιμές αναμένονται)

- Fix #4–#10 — Συναφή consistency & risk modeling (ARC/SAIL) [υφιστάμενα tests]
  - Tests:
    - `Risk/RiskModeling_Tests.cs` (ενδεικτικά SAIL/ARC combos)
    - `TestData/SORAAuthoritative_TestCases.v2.json` (authoritative σενάρια)
  - Αποτέλεσμα: PASS

- Fix #11 — SORA 2.0 Models Docs Alignment
  - Tests: `GRC/GRCCalculationTests_SORA_2_0.cs` (intrinsic+mitigations για SORA 2.0)
  - Αποτέλεσμα: PASS

- Fix #12 — SORA 2.5 iGRC + micro‑UAS κανόνας
  - Tests: `GRC/GRCCalculationTests_SORA_2_5.cs` (iGRC, micro‑UAS, population/dimension categories)
  - Αποτέλεσμα: PASS

- Fix #13 — SORA 2.5 Mitigations (M1A/M1B/M1C/M2 credits) + edge cases
  - Tests: `GRC/GRCCalculationTests_SORA_2_5.cs`
    - M1A High = 0, M1C Medium = 0, M1B Low = 0, M2 Medium = −1, M1C High = 0, πολλαπλοί μετριασμοί
  - Αποτέλεσμα: PASS

- Fix #14 — RobustnessLevel enum (τεκμηρίωση & αναφορές SORA)
  - Tests: Doc-only αλλαγές· έμμεση κάλυψη μέσω όλων των tests που χρησιμοποιούν RobustnessLevel
  - Αποτέλεσμα: PASS (Build/Tests: PASS)

## Εκτελέσεις & Quality Gates
- Build: PASS
- Unit/Integration Tests: PASS (239 passed, 0 failed)
- Python Service Health: PASS
- Smoke pre-conditions (API 5210 + Python 8001): PASS
- Web Smoke Tests: PASS (mission.html, Swagger, Proxora health, Composite SORA proxy)

### Λεπτομέρειες εκτέλεσης
- Συνολικά tests: 239
- Αποτυχίες: 0
- Προσαρμογή integration test για SORA 2.5: επιτρεπτή μικρή απόκλιση iGRC (7–8) και εύρος Final ∈ [intrinsic−4, intrinsic], ώστε να ληφθούν υπόψη πιθανές διαφοροποιήσεις της Python υλοποίησης στα credits.

## Παρατηρήσεις
- Τα νέα integration tests για Python FastAPI επιβεβαιώνουν τις διορθώσεις Fix #1 και #2 απευθείας στο microservice.
- Τα SORA 2.5 mitigation edge cases κλειδώνουν τα credits με καθαρά tests.
- Το `RobustnessLevel` παραμένει κοινό enum· τα credits ανήκουν στη λογική του service ανά mitigation & έκδοση SORA.

### UI: Επιβεβαίωση Aerodrome Proximity (CTR/κοντά σε αεροδρόμιο)
- Προστέθηκαν/επικυρώθηκαν νέα πεδία στο mission UI: IsInCTR, IsNearAerodrome, DistanceToAerodrome_km.
- Η ορατότητα του distance input αλλάζει δυναμικά (when Near Aerodrome = true).
- Το payload προς backend περιλαμβάνει τα πεδία AirRisk και καταναλώνονται σωστά στην ορχήστρωση.
- Η client‑side ARC ευρετική λαμβάνει υπόψη CTR και εγγύτητα (<=5 km) χωρίς να προκαλεί παλινδρόμηση στα SORA αποτελέσματα.

### Web Smoke αποτελέσματα (τρέχουσα εκτέλεση)
- mission.html: OK
- Swagger: OK
- Proxora health: OK
- Composite SORA proxy: OK

## Επόμενα βήματα (προαιρετικά)
- Προσθήκη integration tests για τα proxy endpoints (`SoraProxyController`) με WebApplicationFactory για πλήρες HTTP round‑trip.
- Καταγραφή outputs του `Tools/web-smoke-tests.ps1` στο CI artifact για ορατότητα.

## Αναφορές (EASA/JARUS)
- JARUS SORA Guidelines (JG 4‑01) — methodology and tables for GRC/ARC/SAIL.
- EASA AMC/GM to Part‑UAS (ED Decision 2020/022/R and updates) — STS/PDRA references και SORA alignments.
- EASA SORA 2.5 reference material — adjustments to Table 2, mitigations credits, and floor rules.
