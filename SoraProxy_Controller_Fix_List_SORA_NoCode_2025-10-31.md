# SoraProxyController — Compliance Fix List (No Code)
**Version:** 2025-10-31  
**Scope:** Text/API-contract fixes only (no code). Each item cites an **official EASA/JARUS** reference your reviewer can click.

---

## 1) Stop treating SORA 2.0 as “GRC-only”
**Fix:** Do not forward SORA 2.0 requests to a **/grc/**-only endpoint for a “complete” assessment. A complete SORA 2.0 pipeline must cover: Step #2–3 (GRC), Step #4 (ARC), Step #5 (strategic mitigations), Step #6 (TMPR), Step #7 (SAIL), Step #8 (OSO).  
**Why:** SORA defines an end‑to‑end process; ARC/TMPR/SAIL/OSO are mandatory outcomes before the operator can evidence compliance.  
**Ref:** JARUS **SORA v2.0 Main Body** (JAR‑DEL‑WG6‑D.04) — Steps #2–#8; EASA SORA workshop overview of the SORA flow.  
Links:  
- https://www.scribd.com/document/410980114/Jar-Doc-06-Jarus-Sora-v2-0  
- https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_1_introduction.pdf

---

## 2) Enforce correct altitude semantics per SORA
**Fix:** In 2.5 flows, avoid defaulting **MaxAltitudeFt = 400**; require explicit altitude and process it with the **150 m (≈500 ft)** split for initial ARC (Step #4).  
**Why:** Step #4 determination of the initial ARC uses **500 ft (~150 m)** as the key VLL split; defaults at 400 ft distort the classification.  
**Ref:** EASA SORA workshop — *Step #4 Determination of the initial ARC* (slide shows **500 ft (~150 m)** line); JARUS **SORA v2.5 Main Body §4.4 (Step #4)**.  
Links:  
- https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-v2.5-Main-Body-Release-JAR_doc_25.pdf

---

## 3) Urban/Rural only (no “Suburban” lane for ARC)
**Fix:** Normalize any “Suburban” values to **Urban** for Step #4; expose only **Urban** and **Rural** at API/UI.  
**Why:** The generalized encounter‑rate model used for the initial ARC is framed using **Urban** vs **Rural** categories; “Suburban” is not a separate AEC row.  
**Ref:** EASA SORA workshop *Step #4*.  
Link: https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf

---

## 4) Remove “Mode‑S veil / 30 NM” and use EU concepts (TMZ/RMZ)
**Fix:** Purge “Mode‑S veil (30 NM)” language; when needed, model **TMZ/RMZ** per State **AIP (ENR)** as operational constraints, not as new AEC rows.  
**Why:** “Mode‑S veil” is a US concept; EU uses **TMZ/RMZ**. Initial ARC is set by the AEC/Step #4; TMZ/RMZ do not create new AEC rows by themselves.  
**Ref:** EASA AMC/GM to Article 11 (SORA flow, Step #4/5) + State AIPs for TMZ/RMZ; EASA eRules (UAS).  
Links:  
- https://www.easa.europa.eu/en/document-library/easy-access-rules/online-publications/easy-access-rules-unmanned-aircraft-systems  
- State AIP ENR (per country)

---

## 5) Treat **U‑space services** and **TMPR** correctly
**Fix:** Do not reduce the numerical ARC based on U‑space or “tactical” means. Keep **tactical mitigations (TMPR)** separate and apply them **after** residual ARC is set by Step #5.  
**Why:** **Annex D (TMPR)** addresses *residual* collision risk after strategic mitigations; it does not change ARC indices.  
**Ref:** JARUS **Annex D — Tactical Mitigation Performance Requirements (TMPR)**; EASA/GM Step #6 narrative.  
Links:  
- https://uas.gov.ge/dashboard/pdf/TMPR.pdf  
- https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf

---

## 6) Strategic mitigations ≠ “−1 ARC per item”
**Fix:** Replace any uniform “−1 per mitigation” assumptions with the **Annex C** method: enter via the relevant **AEC row**, then justify **spatial/temporal/density** reductions to reach **residual ARC**.  
**Why:** Residual ARC is obtained via the Annex‑C reduction pathway, **not** by additive decrements.  
**Ref:** JARUS **Annex C — Strategic Mitigation of Collision Risk**; JARUS **SORA v2.5 Main Body §4.4/Step #5**.  
Links:  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-Annex-C-v1.0.pdf  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-v2.5-Main-Body-Release-JAR_doc_25.pdf

---

## 7) Don’t compute MTOM from KE with v=10 m/s (non‑SORA)
**Fix:** Remove the fallback “mass = KE/50 (v=10 m/s)” pathway. Require **declared MTOM** (and in 2.5, use **max cruise speed** where relevant for ground risk model).  
**Why:** SORA 2.0’s earlier “typical energy” concept is not a license for ad‑hoc mass derivations; SORA 2.5 explicitly transitions to **max cruise speed** in the ground‑risk model.  
**Ref:** EASA SORA workshop (v2.0 → v2.5 changes: *typical energy replaced by max cruise speed*).  
Link: https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf

---

## 8) Population density: avoid hard thresholds (e.g., 500 / 10,000) in API
**Fix:** Remove baked‑in numeric cut‑offs to label **Low/Medium/High**; instead, align with the quantitative approach of **SORA 2.5** and document how density is evidenced (grid, source, time window).  
**Why:** SORA 2.5 formalizes **quantitative** ground‑risk and uses Annex B robustness/evidence rather than universal thresholds; 2.0’s qualitative bands were a source of inconsistency.  
**Ref:** EASA SORA workshop (Step #2/#3 changes; quantitative GRC, Annex B flexibility).  
Link: https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf

---

## 9) M3 is **removed** in SORA 2.5; don’t surface it in 2.5 requests
**Fix:** For category **SORA‑2.5**, do not accept or propagate **M3**; ERP considerations moved to **OSO‑8** and do not act as GRC credits.  
**Why:** SORA 2.5 removes M3 and re‑homes ERP obligations into OSO; GRC is now quantitative via actual people‑at‑risk and critical area.  
**Ref:** EASA SORA workshop — *SORA 2.5 Ground risk mitigations* slide.  
Link: https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf

---

## 10) Map environment and altitude into **AEC rows** explicitly
**Fix:** Ensure the Python backend determines **AEC** per Step #4 (airport/heliport vs non‑airport, controlled vs uncontrolled, Urban vs Rural, altitude bands), and the proxy **does not invent extra AECs** (e.g., “AEC 11/12”).  
**Why:** Step #4 relies on a **finite AEC set**; “Atypical/Segregated” is handled via Annex C reasoning — it is **not** an extra AEC in Step #4.  
**Ref:** EASA GM extract with AEC examples; JARUS **Annex C** narrative on when atypical airspace can justify ARC‑a.  
Links:  
- https://www.transpordiamet.ee/sites/default/files/documents/2024-08/SORA.pdf  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-Annex-C-v1.0.pdf

---

## 11) Capture **evidence references** in the payload (Step #4/#5 outcome)
**Fix:** Add fields (texts/URLs) for the operator to cite **AIP/AIS extracts, traffic studies, NOTAMs, U‑space service descriptions, time‑windows** used in the Step #4 (initial ARC) & Step #5 (Annex C) determinations.  
**Why:** Step outcomes require **documentation of information and references** used to substantiate iARC and residual ARC.  
**Ref:** EASA SORA workshop — Step #4 outcome bullets; JARUS Main Body §4.4.  
Links:  
- https://www.easa.europa.eu/sites/default/files/dfu/sora_workshop_feb_2023_-_2_sora.pdf  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-v2.5-Main-Body-Release-JAR_doc_25.pdf

---

## 12) Keep **Annex B** (GRC mitigations) separate from **Annex C** (ARC mitigations)
**Fix:** Ensure your proxy does **not** conflate Annex B robustness (M1/M2) with Annex C reductions; keep them in distinct structures and pass‑through unaltered to the back‑end calculators.  
**Why:** Annex B defines integrity/assurance for **ground** mitigations; Annex C is the method to **reduce initial ARC**. Mixing yields invalid credits.  
**Ref:** JARUS **Annex B v2.5 (JAR_doc_27)** and **Annex C v1.0**.  
Links:  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-v2.5-Annex-B-Release.JAR_doc_27pdf.pdf  
- https://jarus-rpas.org/wp-content/uploads/2024/06/SORA-Annex-C-v1.0.pdf

---

## 13) Terminology: use **ICAO Airspace Classes (A–G)**; tie TMZ/RMZ to AIP
**Fix:** Standardize labels to ICAO A–G; where TMZ/RMZ apply, require **State AIP ENR references** in the evidence fields (see item 11).  
**Why:** Harmonized terminology eases NAA review; TMZ/RMZ are state‑published constraints, not ARC categories.  
**Ref:** EASA eRules (UAS/Part‑SERA) and State AIPs.  
Link: https://www.easa.europa.eu/en/document-library/easy-access-rules/online-publications/easy-access-rules-unmanned-aircraft-systems

---

## 14) Data model hints (no code)
- **Altitude:** accept both ft and m; normalize to m for 2.5 Step #4 logic; store the raw provided value.  
- **Environment:** limit to **Urban/Rural** for ARC; allow “Suburban” only as a UI alias that normalizes to Urban.  
- **U‑space/TMPR:** carry as *tactical* justifications, not ARC numbers.  
- **Population density:** store value + source + time window + grid/area method; avoid magic thresholds.  
**Refs:** JARUS **SORA v2.5 Main Body**; EASA SORA workshop (Step #2/#3/#4/#5).

---

### One‑liner for your changelog
> “Proxy normalized to SORA v2.0/v2.5: no GRC‑only ‘complete’; altitude split at 150 m for ARC; Urban/Rural only; removed Mode‑S veil; TMPR kept tactical; Annex C reduction model (no −1 per SM); M3 removed in 2.5; explicit AEC mapping; evidence fields added; Annex B vs Annex C clean split.”
