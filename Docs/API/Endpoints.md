# Skyworks API — Endpoints (Phase 1-4)

Όλα τα endpoints εκτίθενται από το `Skyworks.Api` (ASP.NET Core 8).

## SORA — Complete E2E Pipeline (Step 37) ⭐

- POST `/api/sora/complete`
  - **Unified end-to-end SORA assessment**: Executes full pipeline GRC → ARC → SAIL → OSO → TMPR → Compliance
  - **Supports both SORA 2.0 and 2.5**
  - Body: `SORACompleteRequest`
    ```json
    {
      "soraVersion": "2.5",
      "missionId": "M-001",
      "missionName": "Urban Inspection",
      "groundRisk": {
        "populationDensity": 5000,
        "isControlledGroundArea": false,
        "maxCharacteristicDimension": 1.5,
        "maxSpeed": 25,
        "mitigations": [
          { "type": "M1A", "robustness": "Medium" }
        ]
      },
      "airRisk": {
        "explicitARC": "B",
        "strategicMitigations": ["S1"],
        "isAtypicalSegregated": false
      },
      "implementedOSOs": [
        { "osoId": 1, "achievedRobustness": "High" },
        { "osoId": 2, "achievedRobustness": "Medium" }
      ]
    }
    ```
  - Response: `SORACompleteResult` με:
    - Ground Risk: `intrinsicGRC`, `finalGRC`, `groundRiskNotes`
    - Air Risk: `initialARC`, `residualARC`, `airRiskNotes`
    - SAIL: `sail` (I-VI), `tmpr`, `sailNotes`
    - OSO Compliance: `requiredOSOCount`, `implementedOSOCount`, `missingOSOs`, `insufficientRobustnessOSOs`, `isCompliant`
    - Risk Modeling: `riskScore` (1-10), `riskBand` (Low/Moderate/High/VeryHigh)
    - Overall: `isSuccessful`, `warnings`, `errors`, `summary`
  - Σημείωση: Για SORA 2.0 χρησιμοποίησε `scenario_V2_0` στο `groundRisk` αντί για `populationDensity`.

  - Παράδειγμα SORA 2.0 (End-to-End)
    - Request:
      ```json
      {
        "soraVersion": "2.0",
        "missionId": "M-200",
        "missionName": "BVLOS Sparsely Populated",
        "groundRisk": {
          "scenario_V2_0": "BVLOS_SparselyPopulated",
          "maxCharacteristicDimension": 2.5,
          "mitigations": [
            { "type": "M1", "robustness": "Medium" },
            { "type": "M2", "robustness": "Medium" }
          ]
        },
        "airRisk": {
          "explicitARC": "C",
          "strategicMitigations": ["S1", "S2"]
        },
        "implementedOSOs": [
          { "osoId": 1, "achievedRobustness": "Medium" },
          { "osoId": 2, "achievedRobustness": "Medium" },
          { "osoId": 3, "achievedRobustness": "Medium" },
          { "osoId": 4, "achievedRobustness": "High" }
        ]
      }
      ```
    - Response (ενδεικτικό):
      ```json
      {
        "isSuccessful": true,
        "soraVersion": "2.0",
        "missionId": "M-200",
        "finalGRC": 3,
        "initialARC": "C",
        "residualARC": "A",
        "sail": "II",
        "tmpr": { "level": "None", "robustness": "Low" },
        "requiredOSOCount": 10,
        "implementedOSOCount": 4,
        "isCompliant": false,
        "warnings": ["OSO compliance not met - operation cannot proceed without mitigation"],
        "errors": [],
        "summary": "SORA 2.0 Assessment: GRC 4→3, ARC C→A, SAIL II, TMPR None (Low), OSO 4/10, Compliant: False, Risk: Low (2.5), Warnings: 1 (e.g., OSO compliance not met - operation cannot proceed without mitigation)"
      }
      ```

  - Παράδειγμα αποτυχημένης περίπτωσης (Out of Scope) με διαγνωστικά iGRC
    - Request (SORA 2.5 — υπερβολική ταχύτητα/διαστάσεις οδηγούν εκτός πεδίου):
      ```json
      {
        "soraVersion": "2.5",
        "missionId": "M-OUT-001",
        "missionName": "Stress Test",
        "groundRisk": {
          "populationDensity": 20000,
          "isControlledGroundArea": false,
          "maxCharacteristicDimension": 5.0,
          "maxSpeed": 120,
          "mitigations": []
        },
        "airRisk": { "explicitARC": "B" },
        "implementedOSOs": []
      }
      ```
    - Response (HTTP 400 BadRequest) — σημειώστε ότι επιστρέφουμε διαγνωστικά `intrinsicGRC` αλλά το `finalGRC` μπορεί να είναι null:
      ```json
      {
        "isSuccessful": false,
        "soraVersion": "2.5",
        "missionId": "M-OUT-001",
        "missionName": "Stress Test",
        "intrinsicGRC": 7,
        "finalGRC": null,
        "groundRiskNotes": "...",
        "initialARC": null,
        "residualARC": null,
        "airRiskNotes": "",
        "sail": null,
        "tmpr": null,
        "sailNotes": "",
        "requiredOSOCount": 0,
        "implementedOSOCount": 0,
        "missingOSOs": [],
        "insufficientRobustnessOSOs": [],
        "isCompliant": false,
        "riskScore": null,
        "riskBand": null,
        "warnings": [],
        "errors": [ "Ground Risk: Operation out of SORA 2.5 scope" ],
        "summary": ""
      }
      ```

- GET `/api/sora/info`
  - Metadata για το SORA pipeline και υποστηριζόμενες εκδόσεις

## Auth

- POST `/api/auth/login`
  - Body: `{ "username": "admin", "password": "admin123" }`
  - Response: `{ access_token, token_type, expires_in }`

Χρήση: Στα secured endpoints περάστε header `Authorization: Bearer <token>`.

## Knowledge

- GET `/api/knowledge/docs?filter=term`
- POST `/api/knowledge/search`
  - Body: `{ "query": "SORA Annex", "top": 20 }`

## Compliance

- GET `/api/compliance/matrix/raw`
- GET `/api/compliance/binder/list`
- GET `/api/compliance/reports/list`

## Misc (v1 group)

- GET `/api/v1/health`
- GET `/api/v1/info`

## SORA — GRC (Ground Risk Class)

- POST `/api/grc/v2.0/intrinsic`
  - Body: Intrinsic GRC input (SORA 2.0 AMC)
- POST `/api/grc/v2.0/final`
  - Body: Final GRC with mitigations (SORA 2.0 AMC M1/M2/M3)

- POST `/api/grc/v2.5/intrinsic`
  - Body: Intrinsic GRC input (SORA 2.5)
- POST `/api/grc/v2.5/final`
  - Body: Final GRC with mitigations (SORA 2.5 M1A/M1B/M1C/M2)
- GET `/api/grc/v2.5/population-category?density=...&isControlledArea=...`
- GET `/api/grc/versions`

## SORA — ARC (Air Risk Class)

- POST `/api/arc/v2.0/initial`
  - Body: `{ "ExplicitARC": "A|B|C|D" }` or leave empty for default behavior
- POST `/api/arc/v2.0/initial/environment`
  - Body: `ARCEnvironmentInput` for decision tree (airspace control, location type, environment, typicality, max AGL)
- POST `/api/arc/v2.0/residual`
  - Body: Residual ARC input
    - Υποστηρίζει:
      - `strategicMitigations`: ["S1".."S4"] (συντηρητικά -1/mitigation με όρια ανά αρχικό ARC, χωρίς διπλομετρήσεις)
      - `localDensityRating`: 1..5 σύμφωνα με Annex C Table 2 (1=πολύ χαμηλή)
      - `isAtypicalSegregated`: true για αίτημα ARC-a (απαιτεί αποδοχή από αρχή/ANSP κατά Annex G 3.20)
- GET `/api/arc/v2.0/tmpr?residualArc=A|B|C|D`

- POST `/api/arc/v2.5/initial`
  - Body: `{ "ExplicitARC": "A|B|C|D" }` or leave empty for default behavior
- POST `/api/arc/v2.5/initial/environment`
  - Body: `ARCEnvironmentInput` for decision tree (SORA 2.5 Figure 6 mapping)
- POST `/api/arc/v2.5/residual`
  - Body: Residual ARC input — ίδιο συμβόλαιο με v2.0
- GET `/api/arc/v2.5/tmpr?residualArc=A|B|C|D`
 - GET `/api/arc/strategic-mitigations` — λίστα αναγνωρισμένων IDs Annex C (S1..S4)

### ARC Validation (SORA 2.5)

- POST `/api/arc/v2.5/validate/environment`
  - Body: `ARCEnvironmentInput`
  - Επιστρέφει `ARCValidationResult` με θέματα (warnings/errors) για Annex B constraints
- GET `/api/arc/v2.5/validate/at-point?latitude=...&longitude=...&maxHeightAGL=...`
  - Φτιάχνει env από το σημείο (airspace control demo) και επιστρέφει αντίστοιχα validation issues

### Composite ARC (SORA 2.5)

- POST `/api/arc/v2.5/composite/initial`
  - Body:
    - `{ "segments": [ { "name": "Segment A", "startTimeUtc": "2025-10-22T08:00:00Z", "endTimeUtc": "2025-10-22T08:10:00Z", "durationMinutes": 10, "environment": { "airspaceControl": "Controlled", "locationType": "NonAirport", "environment": "Urban", "typicality": "Typical", "maxHeightAGL": 80 }, "centerPoint": { "latitude": 34.68, "longitude": 33.0 }, "operationalArea": [...] }, ... ], "computeTimeWeightedProfile": true, "applyHighRiskRules": true }`
  - Returns overall initial ARC (max across segments) + per-segment results with duration/time-weight %, histogram, time-weighted profile (% per ARC), and compliance warnings (e.g., >50% in ARC-d)

## SORA — SAIL (Specific Assurance and Integrity Level)
- ## Risk Modeling (advisory)

- POST `/api/risk/assess`
  - Body: `RiskAssessmentRequest`
    - `{ "soraVersion": "2.0|2.5", "finalGRC": 1..7, "residualARC": "A|B|C|D", "sail": "I|II|III|IV|V|VI", "operationType": "VLOS|BVLOS", "environment": "Urban|NonUrban", "missionDurationMinutes": 10 }`
  - Returns: `RiskAssessmentResult` με `score` (1..10), `band` (Low/Moderate/High/VeryHigh), `factors[]`, `notes`.
  - Σημείωση: Συμβουλευτικό (όχι επίσημη μετρική SORA).


- POST `/api/sail/v2.0/determine`
  - Body: `{ "FinalGRC": 1..7, "ResidualARC": "A|B|C|D" }`
- POST `/api/sail/v2.5/determine`
  - Body: `{ "FinalGRC": 1..7, "ResidualARC": "A|B|C|D" }`

- POST `/api/sail/report?format=pdf|html`  (Phase 4 — Step 34)
  - Body: `SAILReportRequest`
    - `{ "soraVersion": "2.0|2.5", "finalGRC": 1..7, "residualARC": "A|B|C|D", "missionId": "...", "missionName": "...", "implementedOSOs": [ { "osoId": 1, "achievedRobustness": "Low|Medium|High" } ] }`
  - Response:
    - `application/pdf` (default) με αρχείο `SAIL-Report_<mission>_<timestamp>.pdf`
    - ή `text/html` όταν `format=html`
  - Περιέχει: Final GRC, Residual ARC, SAIL, TMPR, OSO compliance (missing/insufficient), exposure, σημειώσεις

## History — Trends & Statistics (Step 36)

- POST `/api/history/analyze`
  - Body: `HistoricalAnalysisRequest`
    - `{ "soraVersion": "2.0|2.5|null", "records": [ { "missionId": "M1", "occurredAtUtc": "2025-09-01T12:00:00Z", "soraVersion": "2.5", "finalGRC": 2, "residualARC": "A|B|C|D", "sail": "I|II|III|IV|V|VI", "isCompliant": true, "riskScore": 2.5 } ] }`
  - Returns: `HistoricalAnalysisResult` με
    - `count`, `byVersion{}`, `bySAIL{}`, `byARC{}`, `complianceRate` (%), `averageRiskScore`, `trendMonthly[] (yyyy-MM,count)`, `notes`
  - Σημείωση: Αναλύει δεδομένα και για SORA 2.0 και 2.5. Βάλε `soraVersion` για φιλτράρισμα. Χρήσιμο για φάση 4 reporting & continuous improvement.

## Population Density (SORA 2.5 Table 3)

- GET `/api/population/density?latitude=...&longitude=...`
  - Επιστρέφει πληθυσμιακή πυκνότητα (people/km²) για συγκεκριμένο σημείο
- GET `/api/population/category?latitude=...&longitude=...&isControlledArea=...`
  - Επιστρέφει κατηγορία πληθυσμιακής πυκνότητας (Table 3) για σημείο
- POST `/api/population/area/max-density`
  - Body: `[{ "latitude": ..., "longitude": ... }, ...]` (polygon points)
  - Επιστρέφει μέγιστη πυκνότητα εντός λειτουργικού όγκου

## Airspace (Annex B context)

- GET `/api/airspace/at-point?latitude=...&longitude=...&altitudeMeters=...`
  - Επιστρέφει τα ενεργά airspaces για το σημείο (γεωμετρία + υψόμετρο, demo static polygons)
- POST `/api/airspace/area/intersections?minAltitudeMeters=...&maxAltitudeMeters=...`
  - Body: `[{ "latitude": ..., "longitude": ..., "altitudeMeters": ... }, ...]`
  - Επιστρέφει airspaces που τέμνουν την επιχειρησιακή περιοχή (χρήση centroid, demo)

## Service Zones (Cyprus demo)

- GET `/api/service-zones/list`
  - Λίστα προκαθορισμένων ζωνών επιχειρήσεων (malls, PV parks, stadium)
- GET `/api/service-zones/at-point?latitude=...&longitude=...`
  - Επιστρέφει τη ζώνη που περιέχει το σημείο, αν υπάρχει
- GET `/api/service-zones/suggest-arc/v2.5?latitude=...&longitude=...&altitudeMeters=...`
  - Προτείνει ARC Environment input (SORA 2.5) βάσει business category & airspace control, και υπολογίζει Initial ARC

## Σημειώσεις
- Τα endpoints Knowledge/Compliance απαιτούν JWT.
- Τα paths των αρχείων επιστρέφονται ως σχετικά προς το root του repo.
- Τα endpoints SORA (GRC/ARC/SAIL) υλοποιούν επίσημους πίνακες/διαγράμματα JARUS SORA 2.0 AMC & SORA 2.5.
- Το residual ARC χρησιμοποιεί προς το παρόν συντηρητική λογική για Annex C (S1..S4, credits -1/mitigation με όρια ανά αρχικό ARC). Θα εξειδικευτεί με τους επίσημους πίνακες.
- Population density provider: static/demo για ευρωπαϊκές ζώνες (αντικαταστήστε με GIS service για παραγωγή).
- **Για αναλυτική τεκμηρίωση με JSON παραδείγματα και schemas, δες: `SORA_Endpoints_Reference.md`**
