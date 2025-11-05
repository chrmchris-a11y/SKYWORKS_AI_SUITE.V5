# SAIL: Version-aware behavior (2.0 vs 2.5)

- SORA 2.0
  - SAIL lookup: Annex D, Table D.1 (GRC 1..7 × ARC a–d)
  - GRC > 7 ⇒ Category C (Certified) → Sail=null, OsoCount=null
  - OSO counts (convenience, derived from Annex E): I→6, II→10, III→15, IV→18, V→21, VI→24
  - Normative artifact is the OSO list with robustness per SAIL (Annex E). Counts are provided for client UX only.

- SORA 2.5
  - SAIL lookup: Annex D, Table 7 (GRC 1..10 × numeric residual ARC 1..10)
  - Rule: GRC 9–10 ⇒ SAIL VI for any residual ARC 1..10
  - Numeric residual ARC only on authoritative paths. Do not bin to a–d.
  - OSO counts must not be hard-coded; return None. The OSO list with robustness per SAIL is loaded from `/sail/data/oso_map_2_5.yaml` when available.

- Single source of truth
  - SORA tables live in `Backend_Python/sail/data/` and are imported by calculators and APIs.
  - OSO definitions live in `Backend_Python/sail/data/oso_map_2_0.yaml` and `Backend_Python/sail/data/oso_map_2_5.yaml`.

- API parity
  - The .NET `SailService` in `Skyworks.Core.Services.SAIL2` calls the Python API rather than re-implementing rules.
  - Reference strings are included in responses ("EASA AMC/GM Annex D Table D.1" or "JARUS Annex D Table 7").
