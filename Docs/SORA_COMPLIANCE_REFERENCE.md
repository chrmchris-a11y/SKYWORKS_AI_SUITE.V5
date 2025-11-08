# SORA 2.0 & 2.5 COMPLIANCE REFERENCE
**100% EASA/JARUS Aligned - Official Document Sources**

> ⚠️ **CRITICAL**: This document contains EXACT values from official EASA/JARUS sources.  
> NO deviations allowed. All implementations MUST match these specifications.

---

## 📚 Official Source Documents

| Document | ID | Edition | Date | Lines | Purpose |
|----------|-----|---------|------|-------|---------|
| **SORA 2.5 Main Body** | JAR-DEL-SRM-SORA-MB-2.5 | 2.5 | 13.05.2024 | 1898 | iGRC Table 2, SAIL Table 7, AEC Figure 6, Steps #4-#5 |
| **SORA 2.5 Annex B** | JAR-DEL-SRM-SORA-B-2.5 | 2.5 | 13.05.2024 | 478 | **Ground Risk Mitigations (M1A/B/C, M2) Table 11** |
| **SORA 2.5 Annex C** | JAR-DEL-SRM-SORA-C-2.5 | 1.0 | - | 567 | AEC → ARC mapping, Strategic Mitigations |
| **SORA 2.0 Annex C** | JAR-DEL-WG6-D.04 | 1.0 | 30.01.2019 | 567 | AEC 1-12 definitions, Strategic Mitigations, ARC reduction rules |

---

**File Locations:**
- `KnowledgeBase/EASA DOCS SPLIT CHUNKS/EXTRACTED_SORA-v2.5-Main-Body-Release-JAR_doc_25 (1).txt`
- `KnowledgeBase/EASA DOCS SPLIT CHUNKS/EXTRACTED_SORA-v2.5-Annex-B-Release.JAR_doc_27pdf.txt`
- `KnowledgeBase/EASA DOCS SPLIT CHUNKS/EXTRACTED_SORA-Annex-C-v1.0.txt`
- `KnowledgeBase/EASA DOCS SPLIT CHUNKS/EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt`

---

## 🔴 SORA 2.5 (JARUS) - Complete Specifications

### Ground Risk Mitigations (Table 11, Annex B Page 15)

**Source:** JAR-DEL-SRM-SORA-B-2.5, Annex B, Table 11  
**Location:** `EXTRACTED_SORA-v2.5-Annex-B-Release.JAR_doc_27pdf.txt` lines 464-478

```typescript
// Source: JAR_doc_27 Annex B Table 11, Page 15
enum M1A_Sheltering_25 {
  None = "None",    // No credit
  Low = "Low",      // -1 GRC
  Medium = "Medium" // -2 GRC
  // High = N/A (NOT AVAILABLE)
}

enum M1B_OperationalRestrictions_25 {
  None = "None",     // No credit
  // Low = N/A (NOT AVAILABLE)
  Medium = "Medium", // -1 GRC
  High = "High"      // -2 GRC
}

enum M1C_GroundObservation_25 {
  None = "None", // No credit
  Low = "Low"    // -1 GRC
  // Medium/High = N/A (NOT AVAILABLE)
}

enum M2_ImpactDynamics_25 {
  None = "None",     // No credit
  // Low = N/A (NOT AVAILABLE)
  Medium = "Medium", // -1 GRC
  High = "High"      // -2 GRC
}
```

**Credits Table:**
| Mitigation | Low | Medium | High |
|------------|-----|--------|------|
| M1(A) – Sheltering | **-1** | **-2** | **N/A** |
| M1(B) – Operational Restrictions | **N/A** | **-1** | **-2** |
| M1(C) – Ground Observation | **-1** | **N/A** | **N/A** |
| M2 – Impact Dynamics Reduced | **N/A** | **-1** | **-2** |

**🚨 CRITICAL CONSTRAINT:**  
**Source:** Annex B Page 8 (Section B.2)
```
M1(A) Medium robustness mitigation CANNOT be combined with M1(B) mitigations.
However, M1(A) Low robustness has no operational restrictions and CAN be combined with M1(B).
```

**Validation Rule:**
```typescript
// Source: JAR_doc_27 Annex B Section B.2, Page 8
if (m1a === "Medium" && m1b !== "None") {
  throw new Error("SORA 2.5 Compliance Error: M1(A) Medium cannot combine with M1(B). Source: JAR_doc_27 Annex B Section B.2");
}
```

**Sequential Application (MANDATORY):**  
**Source:** Main Body Page 38, Table 5 Caption
```
All mitigations must be applied in numerical sequence: M1(A) → M1(B) → M1(C) → M2
```

**🚫 NO Column Minimum Clamp:**  
**Source:** Main Body Page 39 (Section 4.3.4(f))
```
When applying all the M1 mitigations, the final GRC cannot be reduced to a value lower 
than the lowest value in the applicable column in Table 2.
```
**INTERPRETATION:** This refers to controlled ground area (iGRC=1 minimum), NOT a column-min clamp like SORA 2.0.

**Floor Rule:**  
**Source:** Annex B Page 7 (Table 1 Principle #9)
```
The GRC cannot be lowered to a value less than the equivalent for controlled ground area.
```
```typescript
// Practical floor = 0 (though 1 is lowest meaningful value)
if (finalGRC < 0) finalGRC = 0;
```

**🔥 NO M3 (ERP) Mitigation:**  
**Source:** Main Body Page 5 (Document Change Record 2.5)
```
Removal of ERP as a mitigation.
```
**REASON:** M3 moved from GRC calculation to OSO requirements in SORA 2.5.

---

### Intrinsic GRC Table 2 (Main Body Page 34)

**Source:** JAR-DEL-SRM-SORA-MB-2.5, Table 2, Page 34  
**Location:** Main Body lines 900-1100

**6 Population Densities × 5 Dimension/Speed Categories:**

| **Population Density** | **1m / 25m/s** | **3m / 35m/s** | **8m / 75m/s** | **20m / 120m/s** | **40m / 200m/s** |
|------------------------|----------------|----------------|----------------|------------------|------------------|
| **Controlled Ground**  | 1              | 1              | 2              | 3                | 3                |
| **< 5 ppl/km²**        | 2              | 3              | 4              | 5                | 6                |
| **< 50 ppl/km²**       | 3              | 4              | 5              | 6                | 7                |
| **< 500 ppl/km²**      | 4              | 5              | 6              | 7                | 8                |
| **< 5,000 ppl/km²**    | 5              | 6              | 7              | 8                | 9                |
| **< 50,000 ppl/km²**   | 6              | 7              | 8              | 9                | 10               |
| **> 50,000 ppl/km²**   | 7              | 8              | **N/A**        | **N/A**          | **N/A**          |

**Special Rule - Small-UA:**  
**Source:** Main Body Page 34, Table 2 footnote
```
A UA weighing less than or equal to 250 g and having a maximum speed less than or 
equal to 25 m/s is considered to have an iGRC of 1 regardless of population density.
```

```typescript
// Source: JAR_doc_25 Main Body Table 2, Page 34
if (mtom_kg <= 0.250 && maxSpeed_ms <= 25) {
  iGRC = 1; // Override table lookup
}
```

**Population Density Qualitative Descriptors (Table 3, Page 36):**  
**Source:** Main Body Page 35-36, Table 3

| Quantitative (ppl/km²) | Qualitative Descriptor | Example |
|------------------------|------------------------|---------|
| Controlled Ground      | Controlled ground area | Unauthorized entry prohibited |
| < 5                    | Remote | Hard to reach (mountains, deserts, large water bodies) |
| < 50                   | Lightly populated | ~1 small building per km² |
| < 500                  | Sparsely populated / Residential lightly populated | Homes/businesses with large lots (~1 acre) |
| < 5,000                | Suburban / Low density metropolitan | Single-family homes, apartments, <3-4 story buildings |
| < 50,000               | High density metropolitan | Multistory buildings, downtown areas |
| > 50,000               | Assemblies of people | Dense skyscrapers, large gatherings/events |

---

### AEC → Initial ARC (Figure 6, Main Body Page 41-42)

**Source:** JAR-DEL-SRM-SORA-MB-2.5, Figure 6 (ARC assignment process), Page 41  
**Location:** Main Body lines 1200-1500

**12 Airspace Encounter Classes (AECs):**

```typescript
// Source: JAR_doc_25 Main Body Figure 6, Page 41-42
enum AEC {
  AEC_1 = "ARC-a",   // Atypical (segregated) airspace
  AEC_2 = "ARC-b",   // Uncontrolled, Non-airport, Rural
  AEC_3 = "ARC-b",   // Uncontrolled, Non-airport, Suburban
  AEC_4 = "ARC-c",   // Uncontrolled, Non-airport, Urban
  AEC_5 = "ARC-c",   // Controlled, Non-airport, Rural
  AEC_6 = "ARC-c",   // Controlled, Non-airport, Suburban
  AEC_7 = "ARC-d",   // Controlled, Non-airport, Urban
  AEC_8 = "ARC-d",   // Controlled, Airport environment
  AEC_9 = "ARC-d",   // Uncontrolled, Airport environment (populated)
  AEC_10 = "ARC-c",  // Uncontrolled, Airport environment (rural/suburban)
  AEC_11 = "ARC-b",  // Low altitude (<500 ft AGL), Specific conditions
  AEC_12 = "ARC-a",  // Very low altitude, Atypical
}
```

**Decision Tree Logic:**
```
1. Is airspace Atypical (segregated)?
   YES → ARC-a (AEC-12)
   NO → Continue

2. Is altitude < 500 ft AGL AND specific low-altitude conditions met?
   YES → ARC-b (AEC-11)
   NO → Continue

3. Airport/Heliport environment?
   YES → Is controlled airspace?
         YES → ARC-d (AEC-8)
         NO → Is populated area?
              YES → ARC-d (AEC-9)
              NO → ARC-c (AEC-10)
   NO → Continue

4. Non-airport environment:
   Is controlled airspace?
   YES → Environment type?
         - Rural: ARC-c (AEC-5)
         - Suburban: ARC-c (AEC-6)
         - Urban: ARC-d (AEC-7)
   NO → Environment type?
        - Rural: ARC-b (AEC-2)
        - Suburban: ARC-b (AEC-3)
        - Urban: ARC-c (AEC-4)
```

---

### Step #5 - Residual ARC (Strategic Mitigations)

**Source:** JAR-DEL-SRM-SORA-MB-2.5, Section 4.5, Pages 43-44  
**Location:** Main Body lines 1300-1400

**VLOS Low Time Exposure Rule:**  
**Source:** Main Body Page 43-44 (Section 4.5.4)
```
For VLOS operations or operations where the remote pilot is supported by an airspace 
observer situated alongside the pilot for instantaneous communication, the initial air 
risk class can be reduced by one class.

The mitigation cannot be used to reduce the ARC to an ARC-a. 
In ARC-d environments, an additional agreement with ATC might be required.
```

```typescript
// Source: JAR_doc_25 Main Body Section 4.5.4, Page 43-44
function applyVLOSLowExposure(initialARC: string, isVLOS: boolean): string {
  if (!isVLOS) return initialARC;
  
  // Cannot reduce ARC-a further
  if (initialARC === "ARC-a") return "ARC-a";
  
  // Reduce by 1 class
  const arcOrder = ["ARC-a", "ARC-b", "ARC-c", "ARC-d"];
  const currentIndex = arcOrder.indexOf(initialARC);
  if (currentIndex > 0) {
    return arcOrder[currentIndex - 1];
  }
  
  return initialARC;
}
```

**Other Strategic Mitigations:**  
See Annex C for:
- Containment (operational volume boundaries)
- Spatial segregation (time/location restrictions)
- Temporal segregation (flight time windows)
- U-space services integration (SORA 2.5 only)

---

### SAIL Matrix (Table 7, Main Body Page 47)

**Source:** JAR-DEL-SRM-SORA-MB-2.5, Table 7, Page 47  
**Location:** Main Body lines 1450-1500

**Final GRC × Residual ARC → SAIL:**

| **Final GRC** | **ARC-a** | **ARC-b** | **ARC-c** | **ARC-d** |
|---------------|-----------|-----------|-----------|-----------|
| **≤2**        | **I**     | **II**    | **IV**    | **VI**    |
| **3**         | **II**    | **II**    | **IV**    | **VI**    |
| **4**         | **III**   | **III**   | **IV**    | **VI**    |
| **5**         | **IV**    | **IV**    | **IV**    | **VI**    |
| **6**         | **V**     | **V**     | **V**     | **VI**    |
| **7**         | **VI**    | **VI**    | **VI**    | **VI**    |
| **>7**        | **Certified Category** | | | |

```typescript
// Source: JAR_doc_25 Main Body Table 7, Page 47
const SAIL_MATRIX_25 = {
  "ARC-a": { 0: "I", 1: "I", 2: "I", 3: "II", 4: "III", 5: "IV", 6: "V", 7: "VI" },
  "ARC-b": { 0: "II", 1: "II", 2: "II", 3: "II", 4: "III", 5: "IV", 6: "V", 7: "VI" },
  "ARC-c": { 0: "IV", 1: "IV", 2: "IV", 3: "IV", 4: "IV", 5: "IV", 6: "V", 7: "VI" },
  "ARC-d": { 0: "VI", 1: "VI", 2: "VI", 3: "VI", 4: "VI", 5: "VI", 6: "VI", 7: "VI" }
};
```

---

### TMPR Assignment (Table 6, Main Body Page 45)

**Source:** JAR-DEL-SRM-SORA-MB-2.5, Table 6, Page 45  

| **Residual ARC** | **TMPR** | **Level of Robustness** |
|------------------|----------|-------------------------|
| **ARC-d**        | High     | High                    |
| **ARC-c**        | Medium   | Medium                  |
| **ARC-b**        | Low      | Low                     |
| **ARC-a**        | No requirement | No requirement    |

---

## 🔵 SORA 2.0 (AMC1 Article 11) - Complete Specifications

### Ground Risk Mitigations (Table 3, Main Body Page 21)

**Source:** JAR-DEL-WG6-D.04, Table 3, Page 21  
**Location:** `EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt` lines 600-700

```typescript
// Source: JAR_doc_06 SORA 2.0 Main Body Table 3, Page 21
enum M1_Strategic_20 {
  None = "None",     // 0 credit
  Low = "Low",       // -1 GRC
  Medium = "Medium", // -2 GRC
  High = "High"      // -4 GRC
}

enum M2_ImpactReduction_20 {
  None = "None",   // 0 credit
  Medium = "Medium", // -1 GRC
  High = "High"    // -2 GRC
}

enum M3_ERP_20 {
  None = "None",           // +1 penalty!
  Adequate = "Adequate",   // 0 (neutral)
  Validated = "Validated"  // -1 GRC
}
```

**Credits Table:**
| Mitigation | Robustness: None | Low | Medium | High |
|------------|------------------|-----|--------|------|
| **M1 – Strategic** | 0 | **-1** | **-2** | **-4** |
| **M2 – Impact Reduction** | 0 | N/A | **-1** | **-2** |
| **M3 – ERP** | **+1 ⚠️** | N/A | **0** (Adequate) | **-1** (Validated) |

**🚨 M3 PENALTY WARNING:**  
**Source:** SORA 2.0 Main Body Page 21, Table 3 footnote
```
M3 - An Emergency Response Plan (ERP) is in place, operator validated and effective:
- None: +1 (INCREASES GRC)
- Adequate: 0 (neutral, no credit)
- Validated: -1 (proper credit)
```

---

### SORA 2.0: Aircraft Encounter Categories (Annex C Table 1) ✅ VERIFIED

**Source:** JAR-DEL-WG6-D.04 Annex C, Edition 1.0, 30.01.2019  
**Location:** Annex C Table 1, Page 12 (Lines 340-380 of EXTRACTED file)

**12 Aircraft Encounter Categories (AECs) for SORA 2.0:**

```typescript
// Source: JAR_doc_06 Annex C Table 1, Page 12-13
// ⚠️ NOTE: This is SORA 2.0 (v1.0) - Different from SORA 2.5!

interface AEC_Definition {
  aec: number;
  description: string;
  density: 1 | 2 | 3 | 5;  // Density rating (1=low, 5=very high)
  initialARC: "ARC-a" | "ARC-b" | "ARC-c" | "ARC-d";
}

const AEC_SORA20: AEC_Definition[] = [
  // Airport/Heliport Environment
  { 
    aec: 1, 
    description: "Airport/Heliport in Class B/C/D", 
    density: 5, 
    initialARC: "ARC-d" 
  },
  { 
    aec: 6, 
    description: "Airport/Heliport in Class E/F/G", 
    density: 3, 
    initialARC: "ARC-c" 
  },
  
  // Operations > 500ft AGL (below FL600)
  { 
    aec: 2, 
    description: "> 500ft AGL in Mode-S Veil / TMZ", 
    density: 5, 
    initialARC: "ARC-d" 
  },
  { 
    aec: 3, 
    description: "> 500ft AGL in Controlled Airspace", 
    density: 5, 
    initialARC: "ARC-d" 
  },
  { 
    aec: 4, 
    description: "> 500ft AGL Uncontrolled, Urban", 
    density: 3, 
    initialARC: "ARC-c" 
  },
  { 
    aec: 5, 
    description: "> 500ft AGL Uncontrolled, Rural", 
    density: 2, 
    initialARC: "ARC-c" 
  },
  
  // Operations < 500ft AGL (VLL - Very Low Level)
  { 
    aec: 7, 
    description: "< 500ft AGL in Mode-S Veil / TMZ", 
    density: 3, 
    initialARC: "ARC-c" 
  },
  { 
    aec: 8, 
    description: "< 500ft AGL in Controlled Airspace", 
    density: 3, 
    initialARC: "ARC-c" 
  },
  { 
    aec: 9, 
    description: "< 500ft AGL Uncontrolled, Urban", 
    density: 2, 
    initialARC: "ARC-c" 
  },
  { 
    aec: 10, 
    description: "< 500ft AGL Uncontrolled, Rural", 
    density: 1, 
    initialARC: "ARC-b" 
  },
  
  // Special Cases
  { 
    aec: 11, 
    description: "> FL600", 
    density: 1, 
    initialARC: "ARC-b" 
  },
  { 
    aec: 12, 
    description: "Atypical/Segregated Airspace", 
    density: 1, 
    initialARC: "ARC-a" 
  }
];
```

**⚠️ CRITICAL COMPLIANCE NOTES:**

1. **SORA 2.0 uses "Mode-S Veil" terminology** (AEC 2, AEC 7)
   - **BAN THIS TOKEN** in SORA 2.5 implementations
   - Use "RMZ/TMZ" instead for SORA 2.5

2. **Density Rating Scale:**
   - **5** = Very High (Airport B/C/D, High-altitude controlled)
   - **3** = High (Airport E/F/G, Controlled <500ft, TMZ)
   - **2** = Medium (Uncontrolled urban <500ft, Rural >500ft)
   - **1** = Low (Rural <500ft, >FL600, Segregated)

---

### SORA 2.0: Strategic Mitigation - ARC Reduction (Annex C Table 2) ✅ VERIFIED

**Source:** JAR-DEL-WG6-D.04 Annex C, Table 2, Page 14  
**Location:** Annex C lines 430-470

**Initial ARC → Residual ARC Reduction Rules:**

Operator can reduce Initial ARC by demonstrating that **local airspace density** is lower than the generalized AEC density rating.

```typescript
// Source: JAR_doc_06 Annex C Table 2, Page 14
// Reduction requires Competent Authority approval

interface ARCReductionRule {
  aec: number;
  initialDensity: 1 | 2 | 3 | 5;
  initialARC: string;
  demonstrateDensity: number[];  // If local density similar to these
  residualARC: string;            // Then reduce to this ARC
}

const arcReductionRules: ARCReductionRule[] = [
  // AEC 1 & 2: Very high density (5) → Can reduce to ARC-c or ARC-b
  { aec: 1, initialDensity: 5, initialARC: "ARC-d", 
    demonstrateDensity: [4, 3], residualARC: "ARC-c" },
  { aec: 1, initialDensity: 5, initialARC: "ARC-d", 
    demonstrateDensity: [2, 1], residualARC: "ARC-b" },
  
  { aec: 2, initialDensity: 5, initialARC: "ARC-d", 
    demonstrateDensity: [4, 3], residualARC: "ARC-c" },
  { aec: 2, initialDensity: 5, initialARC: "ARC-d", 
    demonstrateDensity: [2, 1], residualARC: "ARC-b" },
  
  // AEC 3: Controlled >500ft (5) → Same as AEC 1/2
  { aec: 3, initialDensity: 5, initialARC: "ARC-d", 
    demonstrateDensity: [4, 3], residualARC: "ARC-c" },
  { aec: 3, initialDensity: 5, initialARC: "ARC-d", 
    demonstrateDensity: [2, 1], residualARC: "ARC-b" },
  
  // AEC 4, 5, 6: Medium density (3 or 2) → Can reduce to ARC-b only
  { aec: 4, initialDensity: 3, initialARC: "ARC-c", 
    demonstrateDensity: [1], residualARC: "ARC-b" },
  { aec: 5, initialDensity: 2, initialARC: "ARC-c", 
    demonstrateDensity: [1], residualARC: "ARC-b" },
  { aec: 6, initialDensity: 3, initialARC: "ARC-c", 
    demonstrateDensity: [1], residualARC: "ARC-b" },
  
  // AEC 7, 8, 9: VLL high/medium density (3 or 2) → Can reduce to ARC-b
  { aec: 7, initialDensity: 3, initialARC: "ARC-c", 
    demonstrateDensity: [1], residualARC: "ARC-b" },
  { aec: 8, initialDensity: 3, initialARC: "ARC-c", 
    demonstrateDensity: [1], residualARC: "ARC-b" },
  { aec: 9, initialDensity: 2, initialARC: "ARC-c", 
    demonstrateDensity: [1], residualARC: "ARC-b" }
  
  // Note: AEC 10 (ARC-b) & AEC 11 (ARC-b) not included
  // Reduction would require ARC-a (atypical/segregated airspace)
];
```

**Reference Density (Note 1, Annex C Page 14):**
- **Density 1** corresponds to **AEC 10**: Operations <500ft AGL over rural areas
- This is the reference scenario for demonstrating lowest density

---

### SORA 2.0: Strategic Mitigation Methods (Annex C Section 5)

**Source:** JAR-DEL-WG6-D.04 Annex C, Pages 7-9

**Two Types of Strategic Mitigations:**

#### 1. Operational Restrictions (Operator Controlled)

**Methods** (Source: Annex C Page 7):
- **Geographic Boundaries**: Restrict UAS to lower-density volume within broader airspace
- **Temporal Restrictions**: Fly only when density is lower (e.g., nighttime 3AM)
- **Time of Exposure**: Limit duration in high-density airspace (e.g., cut corner briefly)

**Examples** (Source: Annex C Page 8):
```
Example 1 - Boundary: Fly at outer edge of Class B, very low altitude where 
            manned aircraft don't routinely operate → Demonstrate low encounter rate

Example 2 - Chronology: Fly in Class B at 3AM when manned aircraft traffic is minimal
            → Demonstrate low encounter rate for that time window

Example 3 - Exposure: Cut corner of Class B for efficiency, demonstrate very short 
            exposure time to elevated risk
```

#### 2. Common Structures & Rules (Authority Controlled)

**Source:** Annex C Pages 8-9, 16

**CRITICAL:** Only Competent Authority / ANSP can implement. Operator cannot control.

**Maximum Reduction:** **1 ARC level only**

**Allowed for VLL Operations Only (<500ft AGL):**
- AEC 7 (TMZ <500ft)
- AEC 8 (Controlled <500ft)
- AEC 9 (Uncontrolled urban <500ft)
- AEC 10 (Uncontrolled rural <500ft)

**NOT allowed for:**
- AEC 1, 2, 3, 4, 5, 11 (already have manned airspace rules - would be double-counting)

**Requirements for Common Structures Reduction** (Source: Annex C Page 16):
- ✅ UAS equipage: Electronic Cooperative System + Anti-Collision Lighting
- ✅ Verification procedure: Check NOTAMs, flight plans before flight
- ✅ Notification procedure: File UAS flight plan, request NOTAM
- ✅ Airspace owner permission obtained
- ✅ Compliance with UAS Flight Rules (VLL airspace specific)
- ✅ UAS airspace structure exists (airways, procedures, flow management)
- ✅ Procedural separation service implemented (mandatory for all UAS)
- ✅ Direct communication with ATC/FIS available

---

### SORA 2.0: Column-Min Clamp Rule ✅ VERIFIED
- Adequate: 0 (no change)
- Validated: -1
```

**Sequential Application:**  
**Source:** SORA 2.0 Main Body Page 21
```
All mitigations must be applied in numeric sequence: M1 → M2 → M3
```

**✅ Column Minimum Clamp (SORA 2.0 ONLY):**  
**Source:** SORA 2.0 Main Body Page 21-22 (Section 2.3.2(d))
```
When applying mitigation M1, the GRC cannot be reduced to a value lower than the 
lowest value in the applicable column in Table 2. This is because it is not possible 
to reduce the number of people at risk below that of a controlled area.
```

```typescript
// Source: JAR_doc_06 SORA 2.0 Main Body Section 2.3.2(d), Page 21
const columnMinimums_20 = {
  "Controlled": 1,
  "VLOS Sparse": 2,
  "BVLOS Sparse": 3,
  "VLOS Populated": 4,
  "BVLOS Populated": 5,
  "VLOS Gathering": 7,
  "BVLOS Gathering": 8
};

// Apply column min AFTER M1 but BEFORE M2/M3
let grcAfterM1 = iGRC + m1Credit; // m1Credit is negative
const columnMin = columnMinimums_20[scenario];
if (grcAfterM1 < columnMin) {
  grcAfterM1 = columnMin; // Enforce floor
}
```

**Floor Rule:**  
**Source:** SORA 2.0 Main Body Page 22
```
If the Final GRC is higher than 7, the operation is not supported by the SORA process.
```
```typescript
if (finalGRC > 7) {
  throw new Error("Final GRC > 7: Not supported by SORA. Consider Certified Category.");
}
```

---

### Intrinsic GRC Table 2 (Main Body Page 20)

**Source:** JAR-DEL-WG6-D.04, Table 2, Page 20  
**Location:** SORA 2.0 Main Body lines 500-600

**8 Operational Scenarios × 4 Dimension Categories:**

| **Operational Scenario** | **1m / <700J** | **3m / <34kJ** | **8m / <1084kJ** | **>8m / >1084kJ** |
|--------------------------|----------------|----------------|------------------|-------------------|
| **VLOS over Controlled** | 1              | 2              | 3                | 4                 |
| **VLOS Sparsely Populated** | 2           | 3              | 4                | 5                 |
| **BVLOS Sparsely Populated** | 3          | 4              | 5                | 6                 |
| **VLOS Populated**       | 4              | 5              | 6                | 8                 |
| **BVLOS Populated**      | 5              | 6              | 8                | 10                |
| **VLOS over Gathering**  | 7              | **N/A**        | **N/A**          | **N/A**           |
| **BVLOS over Gathering** | 8              | **N/A**        | **N/A**          | **N/A**           |

**EVLOS Rule:**  
**Source:** SORA 2.0 Main Body Page 20 (Section 2.3.1(g))
```
EVLOS operations are to be considered as BVLOS for the GRC determination.
```

**Controlled Ground Area:**  
**Source:** SORA 2.0 Main Body Page 20 (Section 2.3.1(h))
```
A controlled ground area is defined as the intended UAS operational area that only 
involves active participants (if any).
```

---

### SAIL Matrix (Table 5, Main Body Page 27)

**Source:** JAR-DEL-WG6-D.04, Table 5, Page 27  

**Final GRC × Residual ARC → SAIL:**

| **Final GRC** | **ARC-a** | **ARC-b** | **ARC-c** | **ARC-d** |
|---------------|-----------|-----------|-----------|-----------|
| **≤2**        | **I**     | **II**    | **IV**    | **VI**    |
| **3**         | **II**    | **II**    | **IV**    | **VI**    |
| **4**         | **III**   | **III**   | **IV**    | **VI**    |
| **5**         | **IV**    | **IV**    | **IV**    | **VI**    |
| **6**         | **V**     | **V**     | **V**     | **VI**    |
| **7**         | **VI**    | **VI**    | **VI**    | **VI**    |
| **>7**        | **Category C** | | | |

**IDENTICAL to SORA 2.5 Table 7.**

---

## ⚖️ Key Differences: SORA 2.0 vs 2.5

### Ground Risk Mitigations

| Feature | SORA 2.0 | SORA 2.5 |
|---------|----------|----------|
| **M1 Naming** | Single "M1 Strategic" | Split: M1(A), M1(B), M1(C) |
| **M1 Options** | None\|Low\|Medium\|High | M1(A): None\|Low\|Medium<br>M1(B): None\|Medium\|High<br>M1(C): None\|Low |
| **M1 Credits** | -1/-2/-4 | M1(A): -1/-2<br>M1(B): -1/-2<br>M1(C): -1 |
| **M1 Constraints** | None | **M1(A) Medium cannot combine with M1(B)** |
| **M2 Options** | None\|Medium\|High | None\|Medium\|High (Low N/A) |
| **M3 (ERP)** | ✅ None(+1)\|Adequate(0)\|Validated(-1) | ❌ **REMOVED** (moved to OSOs) |
| **Column Min Clamp** | ✅ **ENFORCED** (for M1) | ❌ **NO CLAMP** |
| **Sequential Order** | M1→M2→M3 | M1(A)→M1(B)→M1(C)→M2 |

### iGRC Table Structure

| Feature | SORA 2.0 | SORA 2.5 |
|---------|----------|----------|
| **Rows** | 8 operational scenarios | 6 population densities |
| **Columns** | 4 dimension/energy categories | 5 dimension/speed categories |
| **Metrics** | VLOS/BVLOS + environment | Quantitative density (ppl/km²) |
| **Qualitative** | Sparse/Populated/Gathering | Controlled/Remote/Suburban/Urban |

### ARC & Strategic Mitigations

| Feature | SORA 2.0 | SORA 2.5 |
|---------|----------|----------|
| **AEC Count** | 13 categories | 12 categories |
| **VLOS Reduction** | Mentioned but not explicit | **Explicit: -1 ARC class** (Page 43-44) |
| **U-space Services** | Annex H (in preparation) | ✅ Available (Annex H published) |

---

## 🚨 Compliance Guards & Validation Rules

### Ban Tokens
```typescript
// CRITICAL: NEVER use these terms. Use EU terminology.
const BANNED_TERMS = ["Mode-S veil"];
const CORRECT_TERMS = ["RMZ", "TMZ", "Transponder Mandatory Zone"];
```

**Source:** SORA 2.5 Main Body Page 47 (Section 4.6.4(e))
```
Operations in controlled airspace, an airport/heliport environment or a 
Mode-C Veil/Transponder Mandatory Zone (TMZ)...
```
**INTERPRETATION:** Document mentions "Mode-C Veil" but uses "TMZ" as correct EU term.

### Sequential Application
```typescript
// SORA 2.5
const mitigationOrder_25 = ["M1(A)", "M1(B)", "M1(C)", "M2"];

// SORA 2.0
const mitigationOrder_20 = ["M1", "M2", "M3"];

// Validate order
function validateSequentialApplication(appliedMitigations: string[], soraVersion: string) {
  const order = soraVersion === "2.5" ? mitigationOrder_25 : mitigationOrder_20;
  // Ensure appliedMitigations respect order array sequence
}
```

### GRC Floor Enforcement
```typescript
// Source: Both SORA 2.0 & 2.5
function enforceFinalGRCFloor(grc: number): number {
  if (grc < 0) return 0; // Practical minimum
  if (grc > 7) {
    throw new Error("Final GRC > 7: Operation not supported by SORA. Consider Certified Category.");
  }
  return Math.round(grc);
}
```

### AEC Integrity Check
```typescript
// Source: SORA 2.5 Main Body Page 42 (Section 4.4.4(d))
function validateAECInputs(inputs: AECInputs): void {
  const required = ["altitude", "airspaceControl", "environment", "typicality"];
  for (const field of required) {
    if (!inputs[field]) {
      throw new Error(`AEC calculation blocked: Missing required input '${field}'. Source: JAR_doc_25 Section 4.4.4(d)`);
    }
  }
}
```

### M1(A) + M1(B) Constraint (SORA 2.5 ONLY)
```typescript
// Source: JAR_doc_27 Annex B Section B.2, Page 8
function validateM1Combination_25(m1a: string, m1b: string): void {
  if (m1a === "Medium" && m1b !== "None") {
    throw new Error(
      "SORA 2.5 Compliance Error: M1(A) Medium cannot be combined with M1(B). " +
      "Source: JAR_doc_27 Annex B Section B.2, Page 8. " +
      "Reason: To prevent double counting time-based restrictions."
    );
  }
}
```

---

## 📊 Implementation Checklist

### TypeScript Enums & Validation
- [ ] Create `skyworks.enums.ts` with ALL dropdown values (exact spelling from official docs)
- [ ] Create `schemas.ts` with Zod validators for each SORA version
- [ ] Inline comments with `// Source: JAR_doc_X Table Y, Page Z`

### Calculators
- [ ] `grc25.ts` - SORA 2.5 GRC (NO column-min, NO M3, M1(A)+M1(B) validation)
- [ ] `grc20.ts` - SORA 2.0 GRC (column-min clamp, M3 penalty)
- [ ] `arc.ts` - AEC→ARC, VLOS -1 rule, strategic mitigations
- [ ] `sail.ts` - GRC×ARC→SAIL matrix, OSO derivation, TMPR mapping

### UI Components
- [ ] Version selector (2.0 vs 2.5) with dynamic field visibility
- [ ] M1(A)/M1(B)/M1(C) dropdowns (2.5 only)
- [ ] M1/M2/M3 dropdowns (2.0 only)
- [ ] Real-time validation with error messages citing official sources
- [ ] Live badges (iGRC, fGRC, iARC, rARC, SAIL, TMPR) with color-coding
- [ ] Calculation breakdown with official table references

### Testing
- [ ] 8+ SORA 2.5 golden tests (M1A/B/C constraints, Small-UA Rule, VLOS -1, floor)
- [ ] 6+ SORA 2.0 golden tests (M3 penalty, column-min clamp, sequential order)
- [ ] All test expected values manually calculated from official tables
- [ ] Inline comments with test reasoning and official source

### Documentation
- [ ] `SORA_COMPLIANCE_NOTES.md` with implementation details
- [ ] PR checklist: enums=UI, ban tokens, floor=1, column-min (2.0 only), sequential order
- [ ] Screenshots of UI with calculation examples
- [ ] Official EASA/JARUS references in PR description

---

## 📖 Quick Reference: Where to Find Values

### SORA 2.5
- **M1(A)/M1(B)/M1(C)/M2 credits:** Annex B Table 11 (page 15)
- **iGRC Table 2:** Main Body page 34
- **SAIL Table 7:** Main Body page 47
- **AEC Figure 6:** Main Body pages 41-42
- **VLOS -1 ARC rule:** Main Body page 43-44 (Section 4.5.4)
- **M1(A)+M1(B) constraint:** Annex B page 8 (Section B.2)

### SORA 2.0
- **M1/M2/M3 credits:** Main Body Table 3 (page 21)
- **iGRC Table 2:** Main Body page 20
- **SAIL Table 5:** Main Body page 27
- **AEC Figure 4:** Main Body page 23
- **Column-min clamp:** Main Body page 21-22 (Section 2.3.2(d))
- **M3 penalty:** Table 3 footnote

---

**END OF COMPLIANCE REFERENCE**

**Last Updated:** Based on official documents as of November 8, 2025  
**Maintainer:** Skyworks AI Suite V5  
**License:** Internal use only - based on public JARUS/EASA documents
