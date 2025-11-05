Role: You are a senior safety/compliance engineer implementing the SAIL calculator for an aviation risk engine. Deliver a deterministic, spec-accurate module with zero hallucinations.

Objective
Implement SAILCalculator with a single public method:

def calculate_sail(self, request: SAILCalculationRequest) -> SAILCalculationResponse

The method must compute SAIL based on Final GRC and Residual ARC, using the authoritative lookup matrices for:

- SORA 2.0 (EASA AMC/GM, Annex D, Table D.1 — residual ARC letters a–d vs GRC 1–7)
- SORA 2.5 (JARUS SORA 2.5, Annex D, Table 7 — numeric residual ARC 1..10 vs GRC 1..10)

Preserve Category C handling for SORA 2.0 when GRC > 7 (no SAIL; return None).

Sources of truth (do not deviate):
- EASA AMC/GM SORA 2.0: Annex D “SAIL determination” (table “Residual ARC vs Final GRC”). This is published in EASA’s Easy Access Rules; Step 7 table shows the exact mapping a–d x GRC 1–7 → SAIL I–VI. EASA
- JARUS SORA 2.5: Main Body Annex D, Table 7 “SAIL determination” using numeric residual ARC (1..10) with GRC 1..10. Note: Rows GRC 9–10 ⇒ SAIL VI for any residual ARC (1..10). (Use the official JARUS 2.5 table verbatim.)

Inputs & Models (do not change signatures)
Use the existing Pydantic enums & models from sail.models.sail_models:
- GRCLevel (int), ARCLevel (enum a|b|c|d), SAILLevel (enum I..VI), SORAVersion (SORA_2_0 / SORA_2_5)
- SAILCalculationRequest with:
  - grc_level: GRCLevel (int)
  - 2.0 path: arc_level: ARCLevel | None
  - 2.5 path: residual_arc_level: int | None (1..10)
  - sora_version: SORAVersion

Return SAILCalculationResponse with:
- sail_level: SAILLevel | None (None for Category C on 2.0 with GRC>7)
- Back-compat alias: sail mirror
- Echo inputs, plus optional reference: str (human friendly source note)

Behavior — Must Pass These Rules
Version gating
- If sora_version == SORA_2_0:
  - Validate 1 ≤ grc ≤ 7. If grc > 7 ⇒ Category C: return sail_level=None, sail=None, include reference="EASA AMC/GM SORA 2.0 Annex D – Category C for GRC>7".
  - Require arc_level ∈ {a,b,c,d}; 400 if missing.
  - Lookup SAIL from the exact Table D.1 mapping (letters a–d vs GRC 1–7). No algorithmic shortcuts.
  - Include reference="EASA AMC/GM SORA 2.0 Annex D (Table D.1)". EASA

- If sora_version == SORA_2_5:
  - Require residual_arc_level ∈ [1..10]; 400 if missing/out of range.
  - If grc ∈ {9,10} return SAIL VI for any residual ARC.
  - Otherwise lookup exactly from JARUS 2.5 Annex D Table 7 (numeric ARC 1..10 vs GRC 1..8).
  - Include reference="JARUS SORA 2.5 Annex D (Table 7)".

OSO count policy
- 2.0: do not compute here; that’s owned by OSOMapper. Return nothing for counts in this calculator.
- 2.5: never hard-code OSO counts here.

Determinism
- Keep mapping tables as constant dicts within this module or import a single YAML/JSON snapshot from sail/data/. No external calls. No math heuristics.

Error model
- Raise ValueError (caught by API layer) with clear messages when required fields are missing/invalid.
- Do not silently coerce inputs.

Back-compat echoes
- Mirror sail_level to sail in the response model (the API layer already handles this, but keep consistency).

Acceptance Tests (must pass exactly)
Hard-lock the SORA 2.0 mapping with these checks (all from Annex D Table D.1). Your implementation must return:
- GRC 3, ARC-a  → SAIL I
- GRC 4, ARC-a  → SAIL II
- GRC 5, ARC-b  → SAIL IV
- GRC 6, ARC-b  → SAIL IV
- GRC 7, ARC-c  → SAIL VI
- GRC 7, ARC-d  → SAIL VI
These align with the official EASA table and directly address previously observed mismatches (your code had returned II/III/V; fix the matrix). Source for the table: EASA AMC/GM SORA Annex D “SAIL determination”. EASA

For SORA 2.5, assert at minimum:
- GRC 9, residual_arc_level=1..10 → SAIL VI
- GRC 10, residual_arc_level=1..10 → SAIL VI
(Use the full numeric matrix from JARUS 2.5 Table 7 for all other cells.)

Implementation Constraints
- No edits to models or public API signatures.
- No external I/O. Mappings must be embedded constants or loaded from a co-located resource file checked into the repo.
- No counting OSOs here; SAIL only.
- Keep the class small, pure, and unit-testable.

Skeleton (directional, not code you must reuse)
SAILCalculator.__init__:
- Build two in-memory dicts:
  - TABLE_20: dict[int, dict[str, SAILLevel]]  # GRC 1..7 × ARC ‘a|b|c|d’
  - TABLE_25: dict[int, dict[int, SAILLevel]]  # GRC 1..10 × ARC 1..10 (rows 9–10 all VI)

calculate_sail(request):
- Validate version and required fields
- Dispatch to 2.0 or 2.5 path
- Lookup and return SAILCalculationResponse with reference (as above)

Notes & Edge Cases
- 2.0 + GRC > 7 → Category C: sail_level=None. Don’t “clip” to VI.
- 2.5 numeric ARC is not letter-binned. Don’t convert 1..10 into a/b/c/d. Use Table 7 as is.
- Types must honor Pydantic enums (SAILLevel("VI"), etc.). Don’t return plain strings outside the model types.

Minimal Unit Test Vectors (illustrative)
Create/extend tests so these exact expectations hold:
- 2.0: (3, a)→I; (4, a)→II; (5, b)→IV; (6, b)→IV; (7, c)→VI; (7, d)→VI
- 2.0: grc=8, arc=a ⇒ Category C (None)
- 2.5: grc=9, arc_num=1 ⇒ VI; grc=10, arc_num=10 ⇒ VI
- 2.5: grc=4, arc_num in {1,4,7,10} ⇒ must match Table 7 exactly

Documentation Strings (embed)
- For 2.0 mapping: “EASA AMC/GM SORA 2.0 — Annex D (Table D.1) SAIL determination.” (Include link footnote text in code comments.) EASA
- For 2.5 mapping: “JARUS SORA 2.5 — Annex D (Table 7) SAIL determination (numeric residual ARC 1..10).”

Delivery Checklist
- sail_calculator.py with class SAILCalculator and method calculate_sail implemented exactly as above.
- Constant mapping tables embedded or loaded from sail/data/ with a single import.
- Unit tests covering all acceptance cases and at least one full row/column per version.
- No changes to request/response models. No OSO logic here.
- All prior failing cases now pass.

Citations for this prompt (for your own cross-verification)
- EASA AMC/GM “SAIL determination” table (residual ARC a–d vs GRC 1–7), which drives SORA 2.0 mapping. EASA
- General EASA/JARUS references and Annex D pointers. ResearchGate+1

Output Format (strict)
Return ONLY file blocks using this exact wrapper per file:

=== FILE: sail/calculators/sail_calculator.py ===
[complete file content]
=== END FILE ===

Do the same for any additional files you generate or modify, e.g.:
- sail/data/sail_tables_20.py
- sail/tests/test_sail_calculator_20.py
- sail/tests/test_sail_calculator_25.py

No commentary outside file blocks.

Official sources (for traceability in code comments)
- EASA AMC & GM to Part-UAS — SORA Step 7, Annex D “SAIL determination” (Residual ARC vs Final GRC, Table D.1). Easy Access Rules for UAS.
- JARUS SORA v2.5 — Annex D, Table 7 “SAIL determination (numeric residual ARC 1..10 vs GRC 1..10)”.
- EASA SORA AMC/GM consolidated guidance notes referring to Annex D and Annex F.

Context snippets (current repository files; use for non-breaking diffs)
- sail/models/sail_models.py (do not change public signatures)
- sail/calculators/sail_calculator.py (target for this change)
- sail/data/sail_tables_20.py (authoritative 2.0 table snapshot location)
- sail/tests/* (extend/adjust tests to match acceptance rules)
