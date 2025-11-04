# SORA API — Αναλυτική Τεκμηρίωση Endpoints

Πλήρης αναφορά για τα SORA (Specific Operations Risk Assessment) endpoints με JSON παραδείγματα.

---

> Annex mapping (γρήγορη αναφορά)
>
> - Annex A → GRC (Ground Risk Class)
> - Annex B → Initial ARC (Air Risk Class)
> - Annex C → Strategic Mitigations για Residual ARC (S1..S4, caps, no double counting). Η “local density” εδώ αναφέρεται σε επανδρωμένη εναέρια κυκλοφορία.
> - Annex G → Atypical/Segregated Airspace (διαδρομή προς ARC‑a με αποδοχή αρχής).
>
> Σημείωση: Η “population density” αφορά GRC (Annex A/Table 3) και όχι το ARC baseline.

## 1. GRC (Ground Risk Class) — SORA 2.0 & 2.5

### 1.1 Intrinsic GRC (SORA 2.5)

**POST** `/api/grc/v2.5/intrinsic`

Υπολογίζει το αρχικό GRC με βάση τον πληθυσμό και τα χαρακτηριστικά του UAS (πριν μετριάσεις).

**Request:**
```json
{
  "populationDensity": 5000.0,
  "isControlledGroundArea": false,
  "dimension": 1.2,
  "maxSpeed": 15.0
}
```

**Response:**
```json
{
  "iGRC": 4,
  "populationCategory": "Controlled",
  "dimensionCategory": "D2",
  "notes": "Calculated per SORA 2.5 Table 2. Special rule: if ≤250g AND ≤25m/s → iGRC=1."
}
```

---

### 1.2 Final GRC (SORA 2.5)

**POST** `/api/grc/v2.5/final`

Υπολογίζει το τελικό GRC μετά την εφαρμογή μετριάσεων (M1A/M1B/M1C/M2).

**Request:**
```json
{
  "intrinsicGRC": 5,
  "appliedMitigations": [
    {
      "mitigation": "M1A_StrategicLimitation",
      "robustness": "Medium"
    },
    {
      "mitigation": "M2_ImpactDynamics",
      "robustness": "High"
    }
  ]
}
```

**Response:**
```json
{
  "finalGRC": 2,
  "intrinsicGRC": 5,
  "totalReduction": -3,
  "appliedMitigations": [
    {
      "mitigation": "M1A_StrategicLimitation",
      "robustness": "Medium",
      "creditApplied": -1
    },
    {
      "mitigation": "M2_ImpactDynamics",
      "robustness": "High",
      "creditApplied": -2
    }
  ],
  "notes": "Final GRC per SORA 2.5 Table 5. Final GRC ≤7 validated."
}
```

---

### 1.3 Intrinsic GRC (SORA 2.0 AMC)

**POST** `/api/grc/v2.0/intrinsic`

**Request:**
```json
{
  "scenario": "OPS_500_to_FL600_Controlled",
  "dimension": 2.5,
  "kineticEnergy": 320.0
}
```

**Response:**
```json
{
  "iGRC": 6,
  "scenario": "OPS_500_to_FL600_Controlled",
  "dimensionCategory": "D3",
  "notes": "Calculated per SORA 2.0 AMC Table 2 (JAR-doc-06 v2.0)"
}
```

---

### 1.4 Final GRC (SORA 2.0 AMC)

**POST** `/api/grc/v2.0/final`

**Request:**
```json
{
  "intrinsicGRC": 6,
  "appliedMitigations": [
    {
      "mitigation": "M1_PeopleOnGround",
      "robustness": "High"
    },
    {
      "mitigation": "M2_ImpactEnergyReduction",
      "robustness": "Medium"
    }
  ]
}
```

**Response:**
```json
{
  "finalGRC": 3,
  "intrinsicGRC": 6,
  "totalReduction": -3,
  "notes": "Final GRC per SORA 2.0 AMC Table 3. M1/M2/M3 credits applied with column-minimum floor."
}
```

---

### 1.5 Population Category Helper (SORA 2.5)

**GET** `/api/grc/v2.5/population-category?density=5000&isControlledArea=false`

**Response:**
```json
{
  "density": 5000.0,
  "isControlledArea": false,
  "category": "Controlled",
  "notes": "Per SORA 2.5 Table 3"
}
```

---

### 1.6 Version Info

**GET** `/api/grc/versions`

**Response:**
```json
{
  "supportedVersions": [
    {
      "version": "SORA 2.0 AMC",
      "reference": "JAR-doc-06 v2.0",
      "endpoints": ["/api/grc/v2.0/intrinsic", "/api/grc/v2.0/final"]
    },
    {
      "version": "SORA 2.5",
      "reference": "JAR-DEL-SRM-SORA-MB-2.5",
      "endpoints": ["/api/grc/v2.5/intrinsic", "/api/grc/v2.5/final"]
    }
  ]
}
```

---

## 2. ARC (Air Risk Class) — SORA 2.0 & 2.5

### 2.1 Initial ARC (Explicit Override)

**POST** `/api/arc/v2.5/initial`

**Request:**
```json
{
  "explicitARC": "C"
}
```

**Response:**
```json
{
  "arc": "C",
  "notes": "Explicit ARC provided by operator/authority map (SORA 2.5)."
}
```

---

### 2.2 Initial ARC (Environment-based Decision Tree)

**POST** `/api/arc/v2.5/initial/environment`

Υπολογίζει το αρχικό ARC με βάση τα περιβαλλοντικά χαρακτηριστικά (SORA 2.5 Figure 6).

**Request:**
```json
{
  "airspaceControl": "Controlled",
  "locationType": "NonAirport",
  "environment": "Urban",
  "typicality": "Typical",
  "maxHeightAGL": 90.0
}
```

**Response:**
```json
{
  "arc": "C",
  "notes": "Controlled airspace at ≤120m AGL → ARC-c."
}
```

**Παραδείγματα περιβάλλοντος:**

- **Airport/Heliport → ARC-D**
  ```json
  {
    "airspaceControl": "Controlled",
    "locationType": "AirportHeliport",
    "environment": "Urban",
    "typicality": "Typical",
    "maxHeightAGL": 60.0
  }
  // Response: ARC-D
  ```

- **Atypical/Segregated → ARC-A**
  ```json
  {
    "airspaceControl": "Uncontrolled",
    "locationType": "NonAirport",
    "environment": "Remote",
    "typicality": "AtypicalSegregated",
    "maxHeightAGL": 400.0
  }
  // Response: ARC-A
  ```

- **Uncontrolled Rural → ARC-B**
  ```json
  {
    "airspaceControl": "Uncontrolled",
    "locationType": "NonAirport",
    "environment": "Rural",
    "typicality": "Typical",
    "maxHeightAGL": 100.0
  }
  // Response: ARC-B
  ```

---

### 2.3 Residual ARC (Annex C Strategic Mitigations)

**POST** `/api/arc/v2.5/residual`

Υπολογίζει το residual ARC μετά την εφαρμογή στρατηγικών μετριάσεων (Annex C).

**Request (S1..S4 mitigations):**
```json
{
  "initialARC": "D",
  "strategicMitigations": ["S1", "S2"]
}
```

**Response:**
```json
{
  "arc": "B",
  "notes": "Recognized mitigations [S1, S2], credit -2 (cap 2). No double counting applied (took maximum credit per Annex C intent)."
}
```

**Request (Density-based reduction):**
```json
{
  "initialARC": "D",
  "localDensityRating": 2
}
```

**Response:**
```json
{
  "arc": "B",
  "notes": "Local density ≤2 → D→B per Table 2. No double counting applied (took maximum credit per Annex C intent)."
}
```

**Request (Combined density + mitigations):**
```json
{
  "initialARC": "D",
  "localDensityRating": 4,
  "strategicMitigations": ["S1", "S2", "S3"]
}
```

**Response:**
```json
{
  "arc": "B",
  "notes": "Local density 3–4 → D→C per Table 2. Recognized mitigations [S1, S2, S3], credit -2 (cap 2). No double counting applied (took maximum credit per Annex C intent)."
}
```

**Request (Atypical/Segregated claim):**
```json
{
  "initialARC": "C",
  "isAtypicalSegregated": true
}
```

**Response:**
```json
{
  "arc": "A",
  "notes": "Claimed Atypical/Segregated Airspace per Annex G 3.20 → Residual ARC-a (requires authority acceptance)."
}
```

---

### 2.4 TMPR (Tactical Mitigation Performance Requirement)

**GET** `/api/arc/v2.5/tmpr?residualArc=C`

**Response:**
```json
{
  "level": "Medium",
  "robustness": "Medium",
  "notes": "ARC-c: Medium TMPR and Medium robustness (SORA 2.5 Table 4)."
}
```

---

### 2.5 Strategic Mitigations List

**GET** `/api/arc/strategic-mitigations`

**Response:**
```json
[
  {
    "id": "S1",
    "description": "Συντονισμός/συνεργασία με ATS/ANSP ή ισοδύναμη παροχή υπηρεσιών για τακτική επίγνωση/διαχείριση κινδύνου εναέριας κυκλοφορίας."
  },
  {
    "id": "S2",
    "description": "Γεω-περιορισμός/Geo-fencing & οριοθέτηση περιοχής επιχειρήσεων (στρατηγικός διαχωρισμός)."
  },
  {
    "id": "S3",
    "description": "Τακτική αποσυμφόρηση/Deconfliction μέσω διαδικασιών/υπηρεσιών (π.χ. ενημέρωση κυκλοφορίας, διαδικασίες R/T)."
  },
  {
    "id": "S4",
    "description": "Εναλλακτικές διαδρομές/contingency & διαδικασίες εκτροπής για μείωση στυνοικίας με επανδρωμένη κίνηση."
  }
]
```

---

## 3. SAIL (Specific Assurance and Integrity Level)

### 3.1 Determine SAIL (SORA 2.5)

**POST** `/api/sail/v2.5/determine`

---

## FastAPI v1 — SAIL endpoint quick reference

This section documents the actively served FastAPI v1 SAIL endpoint used by the Python service.

- Endpoint: `POST /api/v1/calculate/sail`
- Inputs:
  - `sora_version`: `"2.0" | "2.5"`
  - `final_grc`: integer
  - `residual_arc`: string token representing residual ARC category; accepted forms: `a|b|c|d`, or variants like `ARC-c`, `ARC_C` (case-insensitive)
- Behavior:
  - SORA 2.0 mapping follows AMC/GM Table 5 strictly (e.g., `GRC 5–6 × ARC-c → SAIL IV`, `GRC 5–6 × ARC-d → SAIL V`).
  - SORA 2.5 mapping follows JARUS Table 7; token normalization as above.
  - Category C guard: ισχύει μόνο για SORA 2.0 με `final_grc > 7`. Στη SORA 2.5, οι τιμές `final_grc = 8..10` αντιστοιχούν σε `SAIL VI` (Table 7).
  - Numeric ARC levels (SORA 2.5) υποστηρίζονται μέσω `residual_arc_level` (1..10). Τα γράμματα `a–d` παραμένουν συμβατά μέσω `residual_arc`.

Example (authoritative SORA 2.0):

Request
```json
{ "sora_version": "2.0", "final_grc": 5, "residual_arc": "c" }
```

Response
```json
{
  "sail": "IV",
  "oso_count": 18,
  "final_grc": 5,
  "final_arc": "c",
  "residual_arc": "ARC-c",
  "sora_version": "2.0",
  "reference": "EASA GM1 to Article 11 - SAIL Table (Step #5)",
  "notes": "GRC 5 + ARC C → SAIL IV"
}
```

Example (Category C guard):

Request
```json
{ "sora_version": "2.0", "final_grc": 8, "residual_arc": "b" }
```

Response
```json
{
  "category": "C",
  "sail": null,
  "oso_count": null,
  "final_grc": 8,
  "residual_arc": "ARC-b",
  "notes": "Category C (Certified) operation required per SORA Table for final GRC > 7"
}
```

Example (SORA 2.5 letter-based):

Request
```json
{ "sora_version": "2.5", "final_grc": 3, "residual_arc": "c" }
```

Response
```json
{
  "sail": "III",
  "oso_count": 15,
  "final_grc": 3,
  "final_arc": "c",
  "residual_arc": "ARC-c",
  "sora_version": "2.5",
  "reference": "EASA GM1 to Article 11 - SAIL Table (Step #5)",
  "notes": "GRC 3 + ARC C → SAIL III"
}
```

Numeric ARC (SORA 2.5) — παράδειγμα:

Request
```json
{ "sora_version": "2.5", "final_grc": 3, "residual_arc_level": 4 }
```

Response
```json
{
  "sail": "IV",
  "oso_count": 18,
  "final_grc": 3,
  "residual_arc_level": 4,
  "residual_arc": "ARC-b",
  "sora_version": "2.5",
  "reference": "EASA GM1 to Article 11 - SAIL Table (Step #5)",
  "notes": "GRC 3 + ARC B → SAIL IV"
}
```

PowerShell (Windows) — κλήση του endpoint με numeric ARC:

```powershell
pwsh -NoProfile -Command "Invoke-RestMethod -Method Post -Uri http://localhost:8001/api/v1/calculate/sail -ContentType 'application/json' -Body ( @{ sora_version = '2.5'; final_grc = 3; residual_arc_level = 4 } | ConvertTo-Json ) | ConvertTo-Json -Depth 4"
```

Σημείωση για numeric ARC (SORA 2.5): Το `residual_arc_level` (1..10) γίνεται εσωτερικά bin σε γράμματα για το επίσημο Step #5 lookup: 1–2 → a, 3–4 → b, 5–6 → c, 7–10 → d. Το SAIL προκύπτει πάντα από GRC × ARC (a–d).

### OpenAPI 3.0 snippet (SAIL v1)

Παρακάτω ένα έτοιμο κομμάτι OpenAPI 3.0 για το endpoint `POST /api/v1/calculate/sail` με υποστήριξη numeric ARC (SORA 2.5) και την ειδική περίπτωση Category C για SORA 2.0.

```yaml
openapi: 3.0.3
info:
  title: SORA SAIL API (v1)
  version: 1.0.0
paths:
  /api/v1/calculate/sail:
    post:
      summary: Determine SAIL (Specific Assurance and Integrity Level)
      description: |
        Υπολογίζει το SAIL με βάση το τελικό GRC και το residual ARC.
        - SORA 2.0: γράμματα a–d μέσω `residual_arc`.
        - SORA 2.5: προτιμάται αριθμητικό ARC (1..10) μέσω `residual_arc_level`.
        - Category C guard ισχύει μόνο για SORA 2.0 όταν `final_grc > 7`.
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/SAILRequestV1'
            examples:
              sora20_letters:
                summary: SORA 2.0 με γράμμα ARC
                value:
                  sora_version: "2.0"
                  final_grc: 5
                  residual_arc: "c"
              sora25_numeric:
                summary: SORA 2.5 με numeric residual_arc_level
                value:
                  sora_version: "2.5"
                  final_grc: 3
                  residual_arc_level: 4
      responses:
        '200':
          description: OK
          content:
            application/json:
              schema:
                oneOf:
                  - $ref: '#/components/schemas/SAILResponseV1'
                  - $ref: '#/components/schemas/SAILCategoryCResponseV1'
              examples:
                sora20_letters:
                  summary: SORA 2.0 αποτέλεσμα (γράμματα)
                  value:
                    sail: "IV"
                    oso_count: 18
                    final_grc: 5
                    final_arc: "c"
                    residual_arc: "ARC-c"
                    sora_version: "2.0"
                    reference: "EASA GM1 to Article 11 - SAIL Table (Step #5)"
                    notes: "GRC 5 + ARC C → SAIL IV"
                sora20_category_c:
                  summary: SORA 2.0 Category C (GRC > 7)
                  value:
                    category: "C"
                    sail: null
                    oso_count: null
                    final_grc: 8
                    residual_arc: "ARC-b"
                    notes: "Category C (Certified) operation required per SORA 2.0 when final GRC > 7"
                sora25_numeric:
                  summary: SORA 2.5 numeric residual ARC
                  value:
                    sail: "IV"
                    oso_count: 18
                    final_grc: 3
                    residual_arc_level: 4
                    residual_arc: "ARC-b"
                    sora_version: "2.5"
                    reference: "EASA GM1 to Article 11 - SAIL Table (Step #5)"
                    notes: "GRC 3 + ARC B → SAIL IV"

components:
  schemas:
    SAILRequestV1:
      type: object
      required: [final_grc]
      properties:
        sora_version:
          type: string
          enum: ["2.0", "2.5"]
          default: "2.0"
        final_grc:
          type: integer
          minimum: 1
          description: Τελικό GRC μετά από μετριάσεις (1..10 για 2.5, 1..7 για 2.0)
        residual_arc:
          type: string
          description: Γράμμα ARC (a/b/c/d) ή παραλλαγές (π.χ. ARC-c)
        residual_arc_level:
          type: integer
          minimum: 1
          maximum: 10
          description: Αριθμητικό residual ARC για SORA 2.5 (1..10)
      anyOf:
        - required: [residual_arc]
        - required: [residual_arc_level]

    SAILResponseV1:
      type: object
      required: [sail, oso_count, final_grc]
      properties:
        sail:
          type: string
          enum: ["I","II","III","IV","V","VI"]
        oso_count:
          type: integer
          description: Πλήθος OSO που απαιτούνται για το SAIL
        final_grc:
          type: integer
        final_arc:
          type: string
          description: Echo γράμματος (a/b/c/d) για συμβατότητα
        residual_arc:
          type: string
          description: Echo (π.χ. ARC-c ή ARC-4)
        residual_arc_level:
          type: integer
          minimum: 1
          maximum: 10
        sora_version:
          type: string
        reference:
          type: string
        notes:
          type: string
        sail_explanation:
          type: string

    SAILCategoryCResponseV1:
      type: object
      required: [category, sail, oso_count, final_grc, residual_arc]
      properties:
        category:
          type: string
          enum: ["C"]
        sail:
          type: string
          nullable: true
        oso_count:
          type: integer
          nullable: true
        final_grc:
          type: integer
        residual_arc:
          type: string
        notes:
          type: string
```

---

## 4. Population Density (SORA 2.5 Table 3)

### 4.1 Get Density at Point

**GET** `/api/population/density?latitude=38.0&longitude=23.7`

**Response:**
```json
{
  "latitude": 38.0,
  "longitude": 23.7,
  "density": 17000.0
}
```

---

### 4.2 Get Population Category

**GET** `/api/population/category?latitude=38.0&longitude=23.7&isControlledArea=false`

**Response:**
```json
{
  "latitude": 38.0,
  "longitude": 23.7,
  "density": 17000.0,
  "isControlledArea": false,
  "category": "Controlled",
  "notes": "Per SORA 2.5 Table 3"
}
```

---

### 4.3 Get Max Density in Operational Area

**POST** `/api/population/area/max-density`

**Request:**
```json
[
  { "latitude": 37.9, "longitude": 23.6 },
  { "latitude": 38.1, "longitude": 23.6 },
  { "latitude": 38.1, "longitude": 23.9 },
  { "latitude": 37.9, "longitude": 23.9 }
]
```

**Response:**
```json
{
  "maxDensity": 17000.0,
  "pointCount": 4
}
```

---

## 5. JSON Schema Reference

### ARCEnvironmentInput
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["airspaceControl", "locationType", "environment"],
  "properties": {
    "airspaceControl": {
      "type": "string",
      "enum": ["Uncontrolled", "Controlled"]
    },
    "locationType": {
      "type": "string",
      "enum": ["NonAirport", "AirportHeliport"]
    },
    "environment": {
      "type": "string",
      "enum": ["Urban", "Suburban", "Rural", "Remote", "Water"]
    },
    "typicality": {
      "type": "string",
      "enum": ["Typical", "AtypicalSegregated"],
      "default": "Typical"
    },
    "maxHeightAGL": {
      "type": "number",
      "default": 120.0,
      "description": "Maximum height above ground level in meters"
    }
  }
}
```

### ARCResidualInput
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["initialARC"],
  "properties": {
    "initialARC": {
      "type": "string",
      "enum": ["A", "B", "C", "D"]
    },
    "explicitResidualARC": {
      "type": "string",
      "enum": ["A", "B", "C", "D"],
      "description": "Optional explicit override"
    },
    "strategicMitigations": {
      "type": "array",
      "items": {
        "type": "string",
        "pattern": "^S[1-4]$"
      },
      "description": "S1..S4 strategic mitigation IDs"
    },
    "localDensityRating": {
      "type": "integer",
      "minimum": 1,
      "maximum": 5,
      "description": "Demonstrated local manned-aircraft density (1=very low, 5=very high) per Annex C Table 2"
    },
    "isAtypicalSegregated": {
      "type": "boolean",
      "description": "Claim Atypical/Segregated Airspace per Annex G 3.20 for ARC-a"
    }
  }
}
```

---

## Σημειώσεις Υλοποίησης

- Όλα τα endpoints GRC/ARC/SAIL υλοποιούν επίσημους πίνακες JARUS SORA 2.0 AMC και SORA 2.5.
- Το residual ARC χρησιμοποιεί Annex C λογική: density-based reduction (Table 2), S1..S4 conservative credits, και max-of αποφυγή διπλομετρήσεων.
- Population density provider είναι static/demo (ευρωπαϊκές ζώνες) — αντικαταστήστε με GIS service (WorldPop, Eurostat, κ.λπ.) για παραγωγή.
- SAIL mapping βασίζεται σε SORA 2.0 Table 5 (επαληθεύστε ότι η SORA 2.5 διατηρεί την ίδια δομή).

---

## 6. ARC Validation (SORA 2.5)

### 6.1 Validate Environment

**POST** `/api/arc/v2.5/validate/environment`

Ελέγχει το ARCEnvironmentInput για compliance warnings/errors (Annex B constraints).

**Request:**
```json
{
  "airspaceControl": "Controlled",
  "locationType": "NonAirport",
  "environment": "Urban",
  "typicality": "Typical",
  "maxHeightAGL": 150.0
}
```

**Response:**
```json
{
  "isCompliant": true,
  "issues": [
    {
      "severity": "Warning",
      "code": "HEIGHT.AGL.>120",
      "message": "MaxHeightAGL > 120 m. Επιβεβαιώστε ότι επιτρέπεται από την Αρχή (SORA specific category)."
    },
    {
      "severity": "Warning",
      "code": "AIRSPACE.CONTROLLED",
      "message": "Ελεγχόμενος εναέριος χώρος: πιθανή απαίτηση συντονισμού με ANSP/ATC."
    }
  ],
  "notes": "Heuristic validation (Annex B oriented). Προσθήκη τοπικών κανόνων ανά Αρχή επιτρέπεται."
}
```

**Παραδείγματα validation issues:**

- **Airport/Heliport:**
  ```json
  {
    "airspaceControl": "Controlled",
    "locationType": "AirportHeliport",
    "environment": "Urban",
    "maxHeightAGL": 60
  }
  // Response issues: [AIRSPACE.CONTROLLED, LOCATION.AERODROME]
  ```

- **Atypical claim:**
  ```json
  {
    "typicality": "AtypicalSegregated",
    "maxHeightAGL": 400
  }
  // Response issues: [HEIGHT.AGL.>120, TYPICALITY.ATYPICAL (Info severity)]
  ```

---

### 6.2 Validate at Point

**GET** `/api/arc/v2.5/validate/at-point?latitude=34.92&longitude=33.60&maxHeightAGL=80`

Φτιάχνει ARCEnvironmentInput από το σημείο (airspace control από provider) και επιστρέφει validation.

**Response:**
```json
{
  "env": {
    "airspaceControl": "Controlled",
    "locationType": "NonAirport",
    "environment": "Suburban",
    "typicality": "Typical",
    "maxHeightAGL": 80.0
  },
  "validation": {
    "isCompliant": true,
    "issues": [
      {
        "severity": "Warning",
        "code": "AIRSPACE.CONTROLLED",
        "message": "Ελεγχόμενος εναέριος χώρος: πιθανή απαίτηση συντονισμού με ANSP/ATC."
      }
    ],
    "notes": "Heuristic validation (Annex B oriented)."
  }
}
```

---

## 7. Composite ARC (SORA 2.5) — Multi-Segment Missions

### 7.1 Composite Initial ARC

**POST** `/api/arc/v2.5/composite/initial`

Υπολογίζει ARC ανά segment και συνολικό (Overall = max). Προαιρετικά: time-weighted profile και compliance warnings.

**Request (basic):**
```json
{
  "segments": [
    {
      "name": "Takeoff",
      "environment": {
        "airspaceControl": "Uncontrolled",
        "locationType": "NonAirport",
        "environment": "Remote",
        "maxHeightAGL": 30
      }
    },
    {
      "name": "Transit",
      "environment": {
        "airspaceControl": "Controlled",
        "locationType": "NonAirport",
        "environment": "Urban",
        "maxHeightAGL": 150
      }
    }
  ]
}
```

**Response:**
```json
{
  "overallInitialARC": "D",
  "segmentResults": [
    {
      "name": "Takeoff",
      "initialARC": "A",
      "notes": "Remote/Water environment in uncontrolled airspace → ARC-a."
    },
    {
      "name": "Transit",
      "initialARC": "D",
      "notes": "Controlled airspace at >120m AGL → ARC-d."
    }
  ],
  "histogram": {
    "A": 1,
    "B": 0,
    "C": 0,
    "D": 1
  },
  "complianceWarnings": [
    "Overall ARC-d: Απαιτείται High TMPR & tactical mitigations (Annex B)."
  ],
  "notes": "Overall = max ARC across segments (conservative)."
}
```

**Request (time-weighted profile):**
```json
{
  "segments": [
    {
      "name": "Takeoff",
      "durationMinutes": 5,
      "environment": {
        "airspaceControl": "Uncontrolled",
        "locationType": "NonAirport",
        "environment": "Remote",
        "maxHeightAGL": 30
      }
    },
    {
      "name": "Transit",
      "startTimeUtc": "2025-10-22T08:05:00Z",
      "endTimeUtc": "2025-10-22T08:25:00Z",
      "environment": {
        "airspaceControl": "Controlled",
        "locationType": "NonAirport",
        "environment": "Urban",
        "maxHeightAGL": 150
      }
    },
    {
      "name": "Work Area",
      "durationMinutes": 30,
      "environment": {
        "airspaceControl": "Controlled",
        "locationType": "NonAirport",
        "environment": "Urban",
        "maxHeightAGL": 80
      }
    }
  ],
  "computeTimeWeightedProfile": true,
  "applyHighRiskRules": true
}
```

**Response:**
```json
{
  "overallInitialARC": "D",
  "segmentResults": [
    {
      "name": "Takeoff",
      "initialARC": "A",
      "durationMinutes": 5.0,
      "timeWeightPercent": 9.09,
      "notes": "Remote/Water environment in uncontrolled airspace → ARC-a.",
      "warnings": []
    },
    {
      "name": "Transit",
      "initialARC": "D",
      "durationMinutes": 20.0,
      "timeWeightPercent": 36.36,
      "notes": "Controlled airspace at >120m AGL → ARC-d.",
      "warnings": [
        "ARC-d: Ελέγξτε Annex B απαιτήσεις για tactical mitigations & coordination."
      ]
    },
    {
      "name": "Work Area",
      "initialARC": "C",
      "durationMinutes": 30.0,
      "timeWeightPercent": 54.55,
      "notes": "Controlled airspace at ≤120m AGL → ARC-c.",
      "warnings": []
    }
  ],
  "histogram": {
    "A": 1,
    "B": 0,
    "C": 1,
    "D": 1
  },
  "timeWeightedProfile": {
    "A": 9.09,
    "B": 0.0,
    "C": 54.55,
    "D": 36.36
  },
  "complianceWarnings": [
    "Overall ARC-d: Απαιτείται High TMPR & tactical mitigations (Annex B)."
  ],
  "notes": "Overall = max ARC across segments (conservative). Time-weighted profile: 55.0 min total."
}
```

---

### 7.2 Composite Schema

**CompositeArcRequest**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "segments": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["environment"],
        "properties": {
          "name": { "type": "string" },
          "startTimeUtc": { "type": "string", "format": "date-time" },
          "endTimeUtc": { "type": "string", "format": "date-time" },
          "durationMinutes": { "type": "number", "minimum": 0 },
          "environment": { "$ref": "#/definitions/ARCEnvironmentInput" },
          "centerPoint": { "$ref": "#/definitions/GeoPoint" },
          "operationalArea": {
            "type": "array",
            "items": { "$ref": "#/definitions/GeoPoint" }
          }
        }
      }
    },
    "computeTimeWeightedProfile": { "type": "boolean", "default": false },
    "applyHighRiskRules": { "type": "boolean", "default": true }
  },
  "definitions": {
    "ARCEnvironmentInput": {
      "type": "object",
      "required": ["airspaceControl", "locationType", "environment"],
      "properties": {
        "airspaceControl": { "type": "string", "enum": ["Uncontrolled", "Controlled"] },
        "locationType": { "type": "string", "enum": ["NonAirport", "AirportHeliport"] },
        "environment": { "type": "string", "enum": ["Urban", "Suburban", "Rural", "Remote", "Water"] },
        "typicality": { "type": "string", "enum": ["Typical", "AtypicalSegregated"], "default": "Typical" },
        "maxHeightAGL": { "type": "number", "default": 120.0 }
      }
    },
    "GeoPoint": {
      "type": "object",
      "required": ["latitude", "longitude"],
      "properties": {
        "latitude": { "type": "number" },
        "longitude": { "type": "number" },
        "altitudeMeters": { "type": "number" }
      }
    }
  }
}
```

---

## 8. Airspace Provider (Demo polygons for CY/GR)

### 8.1 Get Airspaces at Point

**GET** `/api/airspace/at-point?latitude=34.92&longitude=33.60&altitudeMeters=80`

**Response:**
```json
{
  "point": {
    "latitude": 34.92,
    "longitude": 33.60,
    "altitudeMeters": 80.0
  },
  "count": 1,
  "airspaces": [
    {
      "id": "CTR_LCLK",
      "name": "Larnaca CTR (demo)",
      "type": "CTR",
      "class": "D",
      "lowerLimitMeters": 0.0,
      "upperLimitMeters": 2500.0
    }
  ],
  "notes": "Static demo provider: approximate polygons"
}
```

---

### 8.2 Get Intersecting Airspaces (Area)

**POST** `/api/airspace/area/intersections?minAltitudeMeters=0&maxAltitudeMeters=300`

**Request:**
```json
[
  { "latitude": 34.9, "longitude": 33.55 },
  { "latitude": 34.9, "longitude": 33.65 },
  { "latitude": 34.95, "longitude": 33.65 },
  { "latitude": 34.95, "longitude": 33.55 }
]
```

**Response:**
```json
{
  "count": 1,
  "airspaces": [
    {
      "id": "CTR_LCLK",
      "name": "Larnaca CTR (demo)",
      "type": "CTR",
      "class": "D",
      "lowerLimitMeters": 0.0,
      "upperLimitMeters": 2500.0
    }
  ]
}
```

---

## 9. Service Zones (Cyprus demo for Skyworks)

### 9.1 List All Zones

**GET** `/api/service-zones/list`

**Response:**
```json
[
  {
    "id": "CY_LIM_MY_MALL",
    "name": "My Mall Limassol",
    "category": "Urban",
    "radiusMeters": 600.0,
    "center": { "latitude": 34.6790, "longitude": 33.0053 },
    "notes": "Mall area in Limassol"
  },
  {
    "id": "CY_AVG_PV_PARK",
    "name": "Avgorou Photovoltaic Park",
    "category": "PhotovoltaicPark",
    "radiusMeters": 1200.0,
    "center": { "latitude": 35.041178, "longitude": 33.85636 },
    "notes": null
  }
]
```

---

### 9.2 Find Zone at Point

**GET** `/api/service-zones/at-point?latitude=34.6790&longitude=33.0053`

**Response:**
```json
{
  "id": "CY_LIM_MY_MALL",
  "name": "My Mall Limassol",
  "category": "Urban",
  "radiusMeters": 600.0,
  "center": { "latitude": 34.6790, "longitude": 33.0053 },
  "notes": "Mall area in Limassol"
}
```

---

### 9.3 Suggest ARC for Zone

**GET** `/api/service-zones/suggest-arc/v2.5?latitude=34.6790&longitude=33.0053&altitudeMeters=60`

Χαρτογραφεί business category → SORA environment, ανιχνεύει airspace control, και υπολογίζει Initial ARC.

**Response:**
```json
{
  "zone": {
    "id": "CY_LIM_MY_MALL",
    "name": "My Mall Limassol",
    "category": "Urban"
  },
  "environmentInput": {
    "airspaceControl": "Controlled",
    "locationType": "NonAirport",
    "environment": "Urban",
    "typicality": "Typical",
    "maxHeightAGL": 60.0
  },
  "initialARC": "C",
  "notes": "Heuristic mapping of business categories to SORA environment for initial ARC"
}
```

---

## 10. Complete Mission Example (SORA 2.5)

### Σενάριο: Καθαρισμός PV Park στην Κύπρο

**Mission Profile:**
- Takeoff από αγροτική περιοχή (5 λεπτά, Uncontrolled, Remote)
- Transit σε PV Park (15 λεπτά, Uncontrolled, Rural)
- Καθαρισμός πάνελ (45 λεπτά, Uncontrolled, Rural)
- Return (15 λεπτά, Uncontrolled, Remote)

**Step 1: Composite ARC**
```json
POST /api/arc/v2.5/composite/initial
{
  "segments": [
    { "name": "Takeoff", "durationMinutes": 5, "environment": { "airspaceControl": "Uncontrolled", "locationType": "NonAirport", "environment": "Remote", "maxHeightAGL": 30 } },
    { "name": "Transit to Site", "durationMinutes": 15, "environment": { "airspaceControl": "Uncontrolled", "locationType": "NonAirport", "environment": "Rural", "maxHeightAGL": 60 } },
    { "name": "Cleaning Operations", "durationMinutes": 45, "environment": { "airspaceControl": "Uncontrolled", "locationType": "NonAirport", "environment": "Rural", "maxHeightAGL": 20 } },
    { "name": "Return", "durationMinutes": 15, "environment": { "airspaceControl": "Uncontrolled", "locationType": "NonAirport", "environment": "Remote", "maxHeightAGL": 30 } }
  ],
  "computeTimeWeightedProfile": true
}
```

**Response:**
```json
{
  "overallInitialARC": "B",
  "timeWeightedProfile": {
    "A": 25.0,
    "B": 75.0,
    "C": 0.0,
    "D": 0.0
  },
  "notes": "Overall = max ARC across segments (conservative). Time-weighted profile: 80.0 min total."
}
```

**Step 2: Residual ARC (με S2 geo-fencing)**
```json
POST /api/arc/v2.5/residual
{
  "initialARC": "B",
  "strategicMitigations": ["S2"]
}
```

**Response:**
```json
{
  "arc": "A",
  "notes": "Recognized mitigations [S2], credit -1 (cap 1). No double counting applied."
}
```

**Step 3: TMPR**
```json
GET /api/arc/v2.5/tmpr?residualArc=A
```

**Response:**
```json
{
  "level": "None",
  "robustness": "None",
  "notes": "ARC-a: No TMPR required (SORA 2.5 Table 4)."
}
```

**Step 4: GRC + SAIL**
```json
POST /api/grc/v2.5/intrinsic
{
  "populationDensity": 10.0,
  "isControlledGroundArea": false,
  "dimension": 1.5,
  "maxSpeed": 12.0
}
// Response: { "iGRC": 2 }

POST /api/sail/v2.5/determine
{
  "finalGRC": 2,
  "residualARC": "A"
}
// Response: { "sail": "I" }
```

**Αποτέλεσμα:**
- Overall ARC: A (μετά S2)
- TMPR: None
- Final GRC: 2 (υποθετικά χωρίς μετριάσεις)
- SAIL: I
- **Συμπέρασμα:** Low-risk mission profile, minimal compliance burden.

---

## 11. SORA 2.0 AMC Examples

### 11.1 Initial ARC (Environment, SORA 2.0)

**POST** `/api/arc/v2.0/initial/environment`

**Request:**
```json
{
  "airspaceControl": "Controlled",
  "locationType": "NonAirport",
  "environment": "Urban",
  "maxHeightAGL": 90.0
}
```

**Response:**
```json
{
  "arc": "C",
  "notes": "Controlled airspace at ≤120m AGL → ARC-c (SORA 2.0)."
}
```

---

### 11.2 Residual ARC (SORA 2.0)

**POST** `/api/arc/v2.0/residual`

**Request:**
```json
{
  "initialARC": "D",
  "strategicMitigations": ["S1", "S2"],
  "localDensityRating": 2
}
```

**Response:**
```json
{
  "arc": "B",
  "notes": "Local density ≤2 → D→B per Table 2. Recognized mitigations [S1, S2], credit -2 (cap 2). No double counting applied (took maximum credit per Annex C intent)."
}
```

---

### 11.3 TMPR (SORA 2.0)

**GET** `/api/arc/v2.0/tmpr?residualArc=C`

**Response:**
```json
{
  "level": "Medium",
  "robustness": "Medium",
  "notes": "ARC-c: Medium TMPR and Medium robustness (SORA 2.0 Table 4)."
}
```

---

### 11.4 SAIL (SORA 2.0)

**POST** `/api/sail/v2.0/determine`

**Request:**
```json
{
  "finalGRC": 4,
  "residualARC": "C"
}
```

**Response:**
```json
{
  "isSupported": true,
  "sail": "IV",
  "notes": "SAIL determined per SORA 2.0 Table 5."
}
```

---

## Σημειώσεις Υλοποίησης (Ενημερωμένο)

- **Dual Version Support:** Όλα τα endpoints GRC/ARC/SAIL/TMPR υπάρχουν σε v2.0 (SORA 2.0 AMC) και v2.5 (JARUS SORA 2.5).
- **Annex Mapping:** 
  - Annex A → GRC
  - Annex B → Initial ARC (environment-based decision tree)
  - Annex C → Residual ARC (S1..S4 mitigations, density reductions, caps, no double counting)
  - Annex G → Atypical/Segregated pathway to ARC-a
- **Composite ARC:** Multi-segment missions με time-weighted profile, high-risk warnings, και compliance checks για >50% ARC-d.
- **Validation:** Heuristic Annex B checks (height, airspace control, location, typicality) με Warning/Error/Info severities.
- **Demo Data:** 
  - Airspace provider: Approximate CTR/TMA polygons για Κύπρο (LCLK, LCPH, LCRA) και Αθήνα (LGAV).
  - Service Zones: 10 ζώνες Κύπρου (malls, PV parks, stadium) με category→environment mapping.
  - Population density: Static European zones—αντικαταστήστε με GIS για παραγωγή.
- **References:** 
  - SORA 2.0 AMC: JAR-doc-06 v2.0
  - SORA 2.5: JAR-DEL-SRM-SORA-MB-2.5, Annexes A/B/C/G

---

## 12. Streaming (SignalR) — Real-time ARC (Step 26)

Hub: `/hubs/arc`

- Method: `Subscribe(sessionId: string)`
  - Προσθέτει τον client σε group με id = `sessionId`.
- Method: `Telemetry(update: TelemetryUpdate)`
  - Επεξεργάζεται live το σημείο, υπολογίζει Initial ARC v2.5 και εκπέμπει event στο αντίστοιχο group.
  - Εκπομπή: `riskEvent` με payload `StreamRiskEvent`.

Contracts:
- `TelemetryUpdate`: `{ droneId, timestampUtc?, position{ lat, lon, altitudeMeters? }, speedMps?, sessionId? }`
- `StreamRiskEvent`: `{ droneId, timestampUtc, environment{ ... }, initialARC, airspacesCount, isControlled, validationWarnings[] }`

Σημειώσεις:
- Debounce 500ms ανά DroneId για να αποφεύγονται καταιγισμοί.
- Environment inference: controlled airspace από provider + mapping service zones → urban/suburban/rural.

---

## 13. Weather Data APIs (Step 27)

### 13.1 Current Weather

**POST** `/api/weather/current`

Επιστρέφει τις τρέχουσες καιρικές συνθήκες (METAR) για το πλησιέστερο σημείο αναφοράς.

**Request:**
```json
{
  "latitude": 34.875,
  "longitude": 33.625
}
```

**Response:**
```json
{
  "locationId": "LCLK",
  "position": {
    "latitude": 34.875,
    "longitude": 33.625
  },
  "observationTime": "2024-01-22T12:00:00Z",
  "visibilityMeters": 10000,
  "isVMC": true,
  "windSpeed": 5.1,
  "windGust": null,
  "windDirection": 270,
  "cloudCeilingMeters": 1200,
  "cloudCoverage": "SCT",
  "isPrecipitating": false,
  "precipitationType": null,
  "temperatureCelsius": 22.0,
  "qnH_hPa": 1015,
  "rawMETAR": "LCLK 221200Z 27010KT 9999 SCT040 22/14 Q1015",
  "rawTAF": null,
  "notes": "Demo static weather for Larnaca"
}
```

---

### 13.2 Weather Forecast

**POST** `/api/weather/forecast`

Επιστρέφει πρόβλεψη καιρού (TAF) για συγκεκριμένη θέση και χρόνο.

**Request:**
```json
{
  "position": {
    "latitude": 34.875,
    "longitude": 33.625
  },
  "targetTime": "2024-01-22T15:00:00Z"
}
```

**Response:**
```json
{
  "locationId": "LCLK",
  "position": {
    "latitude": 34.875,
    "longitude": 33.625
  },
  "observationTime": "2024-01-22T12:00:00Z",
  "visibilityMeters": 10000,
  "isVMC": true,
  "windSpeed": 5.1,
  "windGust": null,
  "windDirection": 270,
  "cloudCeilingMeters": 1200,
  "cloudCoverage": "SCT",
  "isPrecipitating": false,
  "precipitationType": null,
  "temperatureCelsius": 22.0,
  "qnH_hPa": 1015,
  "rawMETAR": "LCLK 221200Z 27010KT 9999 SCT040 22/14 Q1015",
  "rawTAF": null,
  "notes": "Static provider returns current weather (no TAF parsing yet)"
}
```

---

### 13.3 Weather Gate Evaluation

**POST** `/api/weather/evaluate`

Αξιολογεί καιρικές συνθήκες με operational gates (VMC, Wind, Gust, Precipitation).

**Request:**
```json
{
  "position": {
    "latitude": 34.7,
    "longitude": 33.0
  },
  "altitudeAGL": 150,
  "targetTime": "2024-01-22T12:00:00Z"
}
```

**Response:**
```json
{
  "conditions": {
    "locationId": "DEMO_LIMASSOL",
    "position": {
      "latitude": 34.7,
      "longitude": 33.0
    },
    "observationTime": "2024-01-22T12:00:00Z",
    "visibilityMeters": 3000,
    "isVMC": false,
    "windSpeed": 8.2,
    "windGust": 12.0,
    "windDirection": 220,
    "cloudCeilingMeters": 300,
    "cloudCoverage": "BKN",
    "isPrecipitating": true,
    "precipitationType": "Rain",
    "temperatureCelsius": 16.0,
    "qnH_hPa": 1008,
    "rawMETAR": "DEMO 221200Z 22016G23KT 3000 RA BKN010 16/14 Q1008",
    "rawTAF": null,
    "notes": "Low visibility scenario for demo"
  },
  "gates": [
    {
      "isAcceptable": false,
      "gateType": "VMC",
      "message": "IMC conditions detected (Vis=3000m, Ceiling=300m). VLOS operations not recommended.",
      "severity": "Error"
    },
    {
      "isAcceptable": false,
      "gateType": "Precipitation",
      "message": "Precipitation detected: Rain",
      "severity": "Warning"
    }
  ],
  "notes": "2 gates failed. Review weather before flight."
}
```

---

### Weather System Notes

**VMC Criteria (EASA):**
- Below 3000ft AGL: Visibility ≥ 5000m AND Ceiling ≥ 450m
- Above 3000ft AGL: Visibility ≥ 8000m
- `VMCCriteria.IsVMC(conditions, altitudeAGL)` υπολογίζει αυτόματα

**Operational Gates:**
- **VMC Gate**: Error αν IMC (για VLOS operations)
- **Wind Gate**: Warning αν άνεμος > 10 m/s
- **Gust Gate**: Error αν ριπές > 15 m/s
- **Precipitation Gate**: Warning αν βροχή/χιόνι/χαλάζι
- **Availability Gate**: Warning αν δεν υπάρχουν δεδομένα

**Demo Stations:**
- `LCLK` (Larnaca): Good VFR (10km vis, 5.1m/s wind)
- `LCPH` (Paphos): Marginal VFR (8km vis, 6.7m/s wind)
- `LGAV` (Athens): Excellent VFR (9km vis, 4.1m/s wind)
- `DEMO_LIMASSOL`: IMC (3km vis, rain, low ceiling 300m)

**Integration με ARC:**
- Weather gates θα προστεθούν στα ARC notes (future Step)
- Δεν επηρεάζουν το baseline ARC calculation (μόνο advisory warnings)

---


