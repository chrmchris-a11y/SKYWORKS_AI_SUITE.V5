# Evidence and Citations Policy (SKYWORKS)

Goal: Every agent must proceed only with full knowledge from the 23 EASA/JARUS sources and provide citations for every material assertion.

Mandatory guardrails:
- Always call the MCP tool `knowledge_check` at session start; do not proceed unless status is ✅ READY.
- Use MCP tools for tables/calculations; do not re-implement from memory.
- For each conclusion, add EASA/JARUS citation (document name and section/page) or the exact Annex/Table used.
- If citation is missing or uncertain, call `search_sora_docs` and ask for clarification—do not proceed.
- Log evidence to `Docs/Knowledge/SESSION_EVIDENCE.md` (append): date/time, step, citations used.

Quick checklist before any step:
1) knowledge_check → READY token.
2) Read `skyworks://knowledge/project-status` and confirm step.
3) Read `skyworks://policy/evidence-policy` (this file) and follow it.
4) Use `get_grc_table`, `apply_mitigation`, `calculate_sail`, `get_oso_requirements` for SORA logic.
5) For Operations Manual content, use `get_operations_manual_structure`.

Sample citation style:
- “JARUS SORA v2.5, Table 2 (iGRC), sparsely populated, 3m UA → iGRC=5.”
- “JARUS SORA v2.0, Table 5 (SAIL matrix): GRC 4 + ARC b → SAIL III.”
- “Annex A, Part C: Containment (CV/GRB) requirements.”
