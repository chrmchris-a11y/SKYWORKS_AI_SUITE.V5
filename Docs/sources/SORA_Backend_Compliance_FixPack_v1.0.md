---
title: SORA Backend Compliance FixPack v1.0
description: Συγκεντρωτικές διορθώσεις συμμόρφωσης για GRC/SAIL/ARC με επίσημα references
date: 2025-11-01
---

# SORA Backend Compliance FixPack v1.0

Σε αυτό το FixPack συγκεντρώνονται οι κρίσιμες διορθώσεις για πλήρη ευθυγράμμιση με EASA AMC/GM (Reg. (EU) 2019/947) και JARUS SORA v2.5. Κάθε αλλαγή συνοδεύεται από σύντομο rationale και επίσημο reference.

## 1) Python API – Υγιεινή ελέγχου ροής (indentation)
- Διόρθωση στο `/api/v1/calculate/grc/2.5`: ορισμός των `intrinsic_grc`/`final_grc` εντός του κύριου block, πριν από mitigations και πριν το return.
- Rationale: αποτρέπει αβέβαιες τιμές και διαρροές σε tests/outputs.
- Reference: EASA AMC/GM 2019/947 – Step #2/#3 (GRC flow integrity), JARUS SORA v2.5 Main Body Annex A/B.

## 2) SORA 2.0 – Κατάργηση ad-hoc κανόνων micro‑UAS
- Αφαιρέθηκε από το endpoint 2.0 ο κανόνας «<250 g + <19 m/s ⇒ cap GRC=2».
- Rationale: δεν υπάρχει GRC cap στην AMC ροή SORA 2.0· το «<250 g» εμφανίζεται ως containment σημείωση (Step #8), όχι ως κανόνας GRC.
- Reference: EASA AMC/GM 2019/947 – Step #8 (Containment), Annex B.

## 3) SORA 2.0 – Ροή υπολογισμού by-the-book
- Προτροπή χρήσης του authoritative calculator (calculations/grc_calculator.py) για iGRC από Table 1 και Final GRC από Annex B (M1/M2/M3) με floor στους M1.
- Το REST endpoint παραμένει backward-compatible αλλά χωρίς micro‑UAS cap και με σταθερό snake_case JSON.
- Reference: EASA AMC/GM 2019/947 – Table 1 (Intrinsic GRC), Annex B (Mitigations & floor).

## 4) SORA 2.5 – Επίσημα population bands και schema
- Διατήρηση των επίσημων bands (π.χ. <5, <50, <500, <5.000, <50.000, ≥50.000 people/km²) και αναφορά στον Annex A/B.
- Εφαρμογή mitigations κατά σειρά M1A → M1B → M1C → M2, με floor όπως στον Annex B/Table 5 (όπου εφαρμόζεται).
- Reference: JARUS SORA v2.5 Main Body – Annex A (Table A‑1), Annex B (Table 5).

## 5) SAIL mapping – Επιβεβαίωση πίνακα
- Διασφάλιση ότι «GRC=5 + ARC=c ⇒ SAIL IV».
- Reference: EASA AMC/GM 2019/947 – Step #5 (SAIL determination table).

## 6) JSON σταθερότητα
- Ενιαίο schema σε snake_case για GRC/ARC/SAIL: `intrinsic_grc`, `final_grc`, `initial_arc`, `residual_arc`, `sail`, `notes`, `reference`.
- Rationale: σταθερή διαλειτουργικότητα με .NET client (deserializers) και testing.

## 7) ARC normalization (Annex C mapping)
- Ρητή τεκμηρίωση: FE τιμή `Suburban` → normalizes σε `Urban` για να παραμένει ντετερμινιστική η απόφαση Annex C.
- Reference: JARUS SORA Annex C (AEC decision tree), engineering note στο API.

## 8) Scope rules (AMC alignment)
- SAIL VI κάτω από SPECIFIC → out-of-scope (πάει σε CERTIFIED). Τα πρόσθετα business guardrails να δηλώνονται ρητώς, διακριτά από το καθαρά κανονιστικό.
- Reference: EASA AMC/GM 2019/947 – Step #7 context.

---

## Παραπομπές (συνιστάται αποθήκευση PDF στο repo)
- EASA AMC/GM to Regulation (EU) 2019/947, Issue 1, Amendment 3 (ED Decision 2025/018/R)
  - Table 1 (Intrinsic GRC), Annex B (Mitigations & floor), Step #5 (SAIL), Step #8 (Containment)
- JARUS SORA v2.5 – Main Body (JAR_doc_25)
  - Annex A (Table A‑1 – iGRC), Annex B (Table 5 – Mitigations), Annex C (AEC mapping)

Τοποθέτησε τα έγγραφα στο `docs/sources/` για long‑term traceability.
