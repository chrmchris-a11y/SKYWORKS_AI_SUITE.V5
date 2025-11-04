# SORA 2.5 — Γρήγορη Σύνοψη (Cheat Sheet)

Η σελίδα αυτή συνοψίζει τα βασικά του SORA 2.5 για GRC: Πίνακας 2 (iGRC), Πίνακας 3 (πληθυσμιακές κατηγορίες), Πίνακας 5 (μετριασμοί), και τον κανόνα micro‑UAS. Περιγράφει και πώς αυτά χαρτογραφούνται στα DTOs/Services του project.

## Πίνακας 3 — Κατηγορίες Πυκνότητας Πληθυσμού (Rows για Πίνακα 2)

- Controlled Ground Area (μόνο εμπλεκόμενα άτομα)
- Remote: < 5 άτομα/km²
- Lightly Populated: < 50 άτομα/km²
- Sparsely Populated: < 500 άτομα/km²
- Suburban: < 5.000 άτομα/km²
- High Density Metropolitan: < 50.000 άτομα/km²
- Assemblies of People: ≥ 50.000 άτομα/km²

Project mapping:
- Enum: `PopulationDensityCategory`
- Helper: `IGRCCalculationService.GetPopulationCategory(density, controlled)`

## Πίνακας 2 — Κατηγορίες Διάστασης/Ταχύτητας UA (Columns) και iGRC

Στήλες (διάσταση / μέγ. ταχύτητα):
- Small: 1 m / 25 m/s
- Medium: 3 m / 35 m/s
- Large: 8 m / 75 m/s
- VeryLarge: 20 m / 120 m/s
- ExtraLarge: 40 m+ / 200 m/s+

Project mapping:
- Enum: `UADimensionCategory`
- Helper: `IGRCCalculationService.GetDimensionCategory(dimension, speed)`
- DTO εισόδου: `IntrinsicGRCInput` (πεδία: `MaxCharacteristicDimension`, `MaxSpeed`, `PopulationDensity`, `IsControlledGroundArea`, `MTOM_Kg`)
- Αποτέλεσμα: `IntrinsicGRCResult` (πεδία: `IGRC`, `DimensionCategory`, `PopulationCategory`, `IsOutOfScope`, `Notes`)

## Κανόνας micro‑UAS (Σημείωση του Πίνακα 2)

- Εάν MTOM ≤ 250 g (0.25 kg) και μέγιστη ταχύτητα ≤ 25 m/s ⇒ iGRC = 1, ανεξαρτήτως πυκνότητας.
- Στο project υπάρχει πεδίο `MTOM_Kg` (JSON: `mtom_kg`) στο `IntrinsicGRCInput`.

## Πίνακας 5 — Μετριασμοί GRC (M1A/M1B/M1C/M2) και Credits

Ο τελικός GRC υπολογίζεται από iGRC (Πίνακας 2) με αφαίρεση credits από τους μετριασμούς (Πίνακας 5), με περιορισμό Final GRC ≤ 7 και κάτω όριο το 1.

Mitigations όπως εφαρμόζονται στον κώδικα:
- M1A — Sheltering: Low = −1, Medium = −2, High = 0
- M1B — Operational Restrictions: Low = 0, Medium = −1, High = −2
- M1C — Ground Observation: Low = −1, Medium = 0, High = 0
- M2 — Impact Dynamics: Low = 0, Medium = −1, High = −2

Project mapping:
- Enum: `GroundRiskMitigation`
- DTO μετριασμού: `AppliedMitigation` (πεδία: `Type`, `Robustness`, `GRCReduction`)
- Τελικός υπολογισμός: `IGRCCalculationService.CalculateFinalGRC_V2_5(FinalGRCInput)`
- Είσοδος: `FinalGRCInput` (πεδία: `IntrinsicGRC`, `Mitigations`)
- Έξοδος: `FinalGRCResult` (πεδία: `FinalGRC`, `TotalGRCReduction`, `AppliedMitigations`, `IsValid`, `ValidationMessage`)

Σημειώσεις:
- Το `GetMitigationCredit_V2_5` περιέχει τον ακριβή χάρτη credits.
- Δεν επιτρέπεται Final GRC > 7 (σημαίνεται `IsValid = false` με σχετικό `ValidationMessage`).
- Δεν επιτρέπεται μείωση κάτω από 1.

## Endpoints API (για γρήγορα calls)

- POST `api/grc/v2.5/intrinsic` — Υπολογισμός iGRC (Πίνακας 2 + κανόνας micro‑UAS)
- POST `api/grc/v2.5/final` — Τελικός GRC με μετριασμούς (Πίνακας 5)
- GET `api/grc/v2.5/population-category?density=...&controlled=...` — Χαρτογράφηση πυκνότητας

## Χρήσιμα αρχεία στον κώδικα

- Models (SORA 2.5):
  - `Backend/src/Skyworks.Core/Models/GRC/V2_5/IntrinsicGRCModels.cs`
  - `Backend/src/Skyworks.Core/Models/GRC/V2_5/MitigationModels.cs`
- Service λογικής:
  - `Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs`
- Tests:
  - `Backend/tests/Skyworks.Api.Tests/GRC/GRCCalculationTests_SORA_2_5.cs`

## Σχόλια συμμόρφωσης

- Υπάρχει αναλυτικό report: `GRC_MITIGATION_MODELS_SORA_2_5_COMPLIANCE_REPORT.md` με Expected vs Actual mapping και validation αποτελέσματα.
- Αν υπάρξουν επίσημες διευκρινίσεις στο SORA 2.5 για credits, ενημερώνουμε τον χάρτη στο service και επεκτείνουμε τα tests.
