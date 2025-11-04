# SKYWORKS Mission Planner (ARC/AE C Help) — Fix List (No Code)
**Version:** 2025-10-31  
**Author:** GPT-5 Thinking (assistant)  
**Scope:** Text/UI logic corrections only. Every item cites an **official EASA/JARUS source** and pinpoints the **exact place in your file**.

---

## 1) Replace all 45 m thresholds with 150 m (≈500 ft)
- **Fix:** Anywhere the help text or logic says *AGL ≤ 45 m → AEC 7* or *AGL > 45 m → AEC 2*, change the split to **150 m** (500 ft).  
- **Why:** SORA Step #4 uses **500 ft (~150 m)** as the decisive VLL threshold for the initial ARC/AEC logic.  
- **Where in your file:** “AGL ≤ 45m (150ft) → AEC 7 … AGL > 45m (150ft) → AEC 2” in AGL/AMSL help blocks. fileciteturn1file0 fileciteturn1file12  
- **Official reference:** EASA SORA workshop slide “Step #4 – Determination of iARC” showing **500 ft** and **60,000 ft** lines; Step #5 overview. citeturn0search6  
  Also see EASA AMC/GM to Part‑UAS: **Step #4 Determination of the initial ARC** / **Step #5 Strategic mitigations**. citeturn0search11turn0search7

---

## 2) Remove the AMSL/FL600 (18,000 **m**) check — it’s not a SORA requirement
- **Fix:** Delete all text/UI implying you must check **FL600 (18,000 m)** or that exceeding it “automatically raises ARC”. FL600 is **60,000 ft (~18,288 m)** and appears only as a conceptual ceiling in training slides, **not** as an operator check in SORA.  
- **Where in your file:** “Έλεγχος αν ξεπερνάτε **FL600 (18,000m)**… AMSL > 18,000 m → υψηλότερο ARC” and the table cell “AMSL < 18,000 m ✅”. fileciteturn1file3 fileciteturn1file2 fileciteturn1file9 fileciteturn1file13  
- **Official reference:** EASA SORA workshop slide shows **60,000 ft** as a contextual line, not an Annex rule. citeturn0search6  
  JARUS SORA **Main Body v2.5** (Step #4/5) contains **no FL600 operator check**. citeturn1search0

---

## 3) EU context: drop “Mode‑S veil (30 NM)” wording; keep **TMZ/RMZ** only
- **Fix:** Remove “Mode‑S veil / 30 nm” phrases and the **mandatory ADS‑B Out** claim. In the EU/EASA context you should surface **TMZ/RMZ as depicted in the AIP**; the SORA AEC is determined by the **airspace environment (AEC row)**, not by a transponder requirement alone.  
- **Where in your file:** “Mode‑S Veil / Transponder Mandatory Zone … 30nm … ADS‑B Out … AEC 2 or 7” and mirrored in the “Special Airspace Zones” panel. fileciteturn1file1 fileciteturn1file5 fileciteturn1file6 fileciteturn1file7 fileciteturn1file17  
- **Official reference:** EASA AMC/GM **Step #4** focuses on AEC mapping of **airspace characterisation** and then **Step #5** for mitigations; it does **not** add a “Mode‑S veil” rule. citeturn0search11turn0search7  
  (Local TMZ/RMZ are identified in **AIP ENR 2.2/1.6** per State; they’re **operational constraints**, not new AEC rows.)

---

## 4) Do **not** hard‑code OSO numbers to airspace flags
- **Fix:** Delete mappings like “TMZ → OSO#2/#11/#24” or “RMZ → OSO#2/#24”. OSOs are derived **after SAIL is known (Step #8)**, not hard‑wired from TMZ/RMZ.  
- **Where in your file:** “OSO Requirements: OSO#2, OSO#11, OSO#24” under Mode‑S/TMZ/RMZ. fileciteturn1file1 fileciteturn1file6  
- **Official reference:** EASA AMC/GM **Step #8 OSO determination** (after SAIL), and SORA process overview where **Step #4/5** feed into **SAIL/OSO**. citeturn0search11

---

## 5) Stick to AEC **1–9** only; remove invented AEC **11–12** and the “FL600 AEC”
- **Fix:** Eliminate AEC **11: Above FL600** and **AEC 12: Atypical/Segregated** from the UI/help/logic. Step #4 uses **AEC 1–9**. “Atypical/Segregated airspace” is a **mitigation pathway** to **ARC‑a** (see Annex C/G), **not** a Step‑4 AEC row.  
- **Where in your file:** The compute block labeling “AEC 11: Above FL600” and “AEC 12: Atypical/Segregated airspace”. fileciteturn1file14  
- **Official reference:** EASA/NAAs GM excerpt of the SORA ARC/AEC table with examples for **AEC 6, AEC 9** etc. (no AEC 11/12 in Step #4). citeturn0search17  
  JARUS Annex C explains when **Atypical/Segregated** can justify **ARC‑a**; it is **not** a Step‑4 category. citeturn1search1turn1search2

---

## 6) Stop saying “each Strategic Mitigation = −1 ARC”; use the **Annex C reduction table**
- **Fix:** Replace any “1 mitigation reduces ARC by one level” language with the **Annex C Initial ARC Reduction table**: you **enter by AEC row** and justify **density/time/space** reductions to reach the **residual ARC**.  
- **Where in your file:** The ARC help that implies uniform “−1 per mitigation” and the “Enhanced ARC Strategic Mitigations” block. fileciteturn1file18  
- **Official reference:** JARUS **SORA v2.5 Main Body** (Step #5) and **Annex C** on Strategic Mitigation/Residual ARC. citeturn1search0turn1search1  
  EASA SORA workshop “Step #5 – Application of strategic mitigations”. citeturn1search3

---

## 7) Don’t mix **GRC mitigations M1(A/B/C), M2** into the ARC panel
- **Fix:** Move **M1(A/B/C), M2** to the **Ground Risk (Step #3)** UI only. They are **GRC mitigations** in SORA 2.5, not ARC strategic mitigations. Also **remove** the validation rule *“M1(A) Medium cannot be combined with M1(B)”* — there is **no such blanket prohibition** in Annex B; robustness/assurance are evidenced per mitigation, not by a global ban.  
- **Where in your file:** The “SORA 2.5 Enhanced ARC Strategic Mitigations” section listing M1(A/B/C), M2, plus the “Validation Error” block that forbids M1(A)+M1(B). fileciteturn1file18 fileciteturn1file4 fileciteturn1file15  
- **Official reference:** JARUS **Annex B** covers integrity/assurance for **ground** mitigations; **Annex C** covers **air** strategic mitigations & residual ARC. citeturn0search0turn1search1

---

## 8) Tidy “AGL vs AMSL” guidance
- **Fix:** Keep **AGL** as the parameter that drives **ARC**. Present **AMSL** only as optional context for terrain awareness; **remove** any claim that SORA **requires** AMSL entry or uses an AMSL threshold in Step #4/5.  
- **Where in your file:** “AGL for ARC… AMSL for FL600 check… ‘Για SORA 2.0 δεν χρειάζεται AMSL’ → ‘συμπληρώστε το για documentation’.” fileciteturn1file11 fileciteturn1file3  
- **Official reference:** EASA AMC/GM **Step #4/5** and JARUS **Main Body v2.5**. No AMSL threshold is used to compute ARC/Residual ARC. citeturn0search11turn1search0

---

## 9) Align “Airport/Heliport” wording to AEC examples; avoid “near‑aerodrome = X NM” rules
- **Fix:** Don’t invent fixed distances. Use the **AEC rows** per the environment (e.g., airport/heliport in Class C/D → AEC 1; airport/heliport in Class G → AEC 6) as per examples, then apply Annex C if seeking a lower **residual ARC**.  
- **Where in your file:** The “Location Type: Airport” mapping block; avoid adding any “≤3 NM” heuristics in help text. fileciteturn1file14  
- **Official reference:** GM/NAAs extract with **AEC 6, AEC 9** examples and Annex C reduction narrative. citeturn0search17

---

## 10) Small language/label clean‑ups (consistency with SORA)
- “ACE Categories” → “**ICAO Airspace Classes (A–G)**”. fileciteturn1file7  
- In “Special Airspace Zones”, add a neutral note: “**Check State AIP (ENR)** for TMZ/RMZ/Danger/Restricted/Prohibited applicability to **UAS**.” fileciteturn1file17  
- Ensure all “Urban/Suburban/Rural” environment labels are used only where SORA defines them (Step #4 examples, Annex C reasoning).

---

### One‑liner you can paste into your internal changelog
> “Rebased ARC help/UI to SORA v2.5 Step #4/5 & AMC/GM: 500‑ft AGL split; dropped FL600/AMSL checks; removed Mode‑S veil & OSO hard‑wiring; confined M1(A/B/C), M2 to GRC; applied Annex C residual‑ARC method; AEC rows 1–9 only.”

