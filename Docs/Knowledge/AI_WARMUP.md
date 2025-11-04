# AI Warmup — SKYWORKS Project Memory

Purpose: Ensure every new agent/chat immediately has full context and does not re-read the 23 EASA/JARUS files. Use the MCP server as the authoritative memory.

What to do on startup:
1) Treat the MCP server "skyworks-sora" as the source of truth for SORA 2.0/2.5.
2) Call tools instead of reading documents:
   - get_grc_table, apply_mitigation, calculate_sail, get_oso_requirements, validate_floor_rule, get_operations_manual_structure
3) Read these two onboarding docs first:
   - PROJECT_ONBOARDING.md
   - MCP_SERVER_GUIDE.md
4) Then call "get_project_status" to know exactly:
   - Phase, current step, next step, last update, notes

Pre-loaded memory (no reading needed):
- SORA 2.0/2.5 GRC tables, SAIL, OSO, M1/M2/M3 & M1A/M1B/M1C
- Operations Manual structure A–T (Annex A)
- ARC/TMPR basics and references

If you need raw references, use MCP resources:
- skyworks://knowledge/sora-2.0-tables
- skyworks://knowledge/sora-2.5-tables
- skyworks://knowledge/operations-manual
- skyworks://knowledge/air-risk-arc-tmpr
- skyworks://knowledge/project-status
- skyworks://knowledge/project-phases

Workflow template:
- "SKYWORKS PROJECT — Step [X] (Phase [Y]). Use MCP Server: skyworks-sora. Read AI_WARMUP.md, PROJECT_ONBOARDING.md, MCP_SERVER_GUIDE.md. Then call get_project_status and proceed."
