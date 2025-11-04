# RobustnessLevel Enum — SORA Compliance Report (Fix #14)

Date: 2025-11-03
Scope: Τεκμηρίωση και ευθυγράμμιση περιγραφής του κοινoύ enum `RobustnessLevel` (Low/Medium/High) με SORA 2.0 AMC και SORA 2.5.

## Πηγές/Αναφορές
- SORA 2.0 AMC — Πίνακες 6–8 (M1, M2, M3): Credits ανά Low/Medium/High
- SORA 2.5 — Πίνακας 5 (M1A, M1B, M1C, M2): Credits ανά Low/Medium/High
- Project services:
  - `GetMitigationCredit_V2_0` (SORA 2.0 AMC)
  - `GetMitigationCredit_V2_5` (SORA 2.5)

## Τι αλλάξαμε
- Εμπλουτισμός XML σχολίων στο `Backend/src/Skyworks.Core/Models/GRC/RobustnessLevel.cs`:
  - Σαφής αναφορά στα επίσημα SORA tables όπου καθορίζονται τα credits.
  - Διευκρίνιση ότι το enum δεν εμπεριέχει credits· τα credits εξαρτώνται από τον τύπο μετριασμού και την έκδοση SORA.
  - Περιγραφές στα `None`, `Low`, `Medium`, `High`.
- Καμία αλλαγή σε API/λογική/ονόματα.

## Αναμενόμενη συμπεριφορά (σε επίπεδο project)
- Το mapping Robustness→credits γίνεται αποκλειστικά στο service (ανά mitigation & SORA έκδοση).
- JSON serialization ως strings καλύπτεται (attribute `JsonStringEnumConverter` + global ρυθμίσεις API).

## Validation
- Build: PASS
- Tests: PASS (default build-and-test task)
- Συμβατότητα: Δεν υπάρχουν breaking changes, enums/DTOs παραμένουν ίδια.

## Επόμενα βήματα (προαιρετικά)
- Αν αλλάξουν επίσημες οδηγίες/credits, ενημερώνουμε τις μεθόδους `GetMitigationCredit_*` και επεκτείνουμε tests.
- Προσθήκη cross-links στη `SORA_2_5_CHEAT_SHEET.md` για τα Robustness credits ανά mitigation (ήδη υπάρχει επισκόπηση).
