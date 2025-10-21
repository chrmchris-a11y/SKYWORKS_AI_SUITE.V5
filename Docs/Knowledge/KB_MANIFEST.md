<!-- Phase1 Step5 — Skyworks V5 -->
# Knowledge Base Manifest

## Overview

This manifest describes the official EASA/SORA corpus used by the SKYWORKS AI SUITE for regulatory compliance training and context injection.

---

## Corpus Location

**Relative Path (in-repo):**
```
KnowledgeBase/EASA DOCS SPLIT CHUNKS
```

**Description:**  
Pre-chunked and embedded EASA documentation including SORA Annex A-H, PDRA, STS, and related regulatory materials.

**Status:** Read-only. Do not modify files in this directory.

---

## Supported Formats

- `.md` — Markdown documents
- `.txt` — Plain text documents
- `.jsonl` — JSON Lines (keys: `text`, `source`)
- `.csv` — CSV files (columns: `text`, `source`)

---

## Usage

The corpus is consumed by:

1. **Training Center** — Generates context packs for AI agents (Copilot, Spark)
2. **GRC/ARC/SAIL/OSO Engines** — Compliance validation and risk assessment
3. **Operational Manual Generation** — SORA-compliant documentation

---

## Versioning & Traceability

- **Corpus Version:** v2.5 (SORA 2.5 + Cyprus DCA)
- **Last Updated:** 2025-10-21
- **Maintained By:** SKYWORKS AI SUITE Team
- **Changelog:** See `KnowledgeBase/CHANGELOG.md` (if present)

---

## References

- EASA SORA Methodology: https://www.easa.europa.eu/domains/civil-drones/specific-category-civil-drones
- Cyprus DCA Regulations: https://www.mcw.gov.cy/dca

---

**End of Manifest**
