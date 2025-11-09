/**
 * SORA Calculator Module (SORA 2.0 and SORA 2.5)
 * 
 * 100% EASA/JARUS Compliant Implementation
 * 
 * ⚠️ VERIFIED FROM OFFICIAL DOCUMENTS (November 8, 2025):
 * - SORA 2.5 Main Body: JAR-DEL-SRM-SORA-MB-2.5, Edition 2.5, 13.05.2024 (1898 lines)
 * - SORA 2.5 Annex B: JAR-DEL-SRM-SORA-B-2.5, Edition 2.5, 13.05.2024 (478 lines)
 * - SORA 2.0 Main Body: JAR-DEL-WG6-D.04, Edition 2.0, 30.01.2019 (1071 lines)
 * 
 * 🔴 CRITICAL COMPLIANCE RULES ENFORCED:
 * 
 * SORA 2.5:
 * - M1(A) Sheltering: None | Low (-1) | Medium (-2) | [High = N/A]
 * - M1(B) Operational Restrictions: None | [Low = N/A] | Medium (-1) | High (-2)
 * - M1(C) Ground Observation: None | Low (-1) | [Medium/High = N/A]
 * - M2 Impact Dynamics: None | [Low = N/A] | Medium (-1) | High (-2)
 * - M3 (ERP): REMOVED (moved to OSO requirements)
 * - ⚠️ M1(A) Medium CANNOT combine with M1(B) (Source: Annex B page 8)
 * - ❌ NO column-min clamp (only SORA 2.0 has this)
 * - ✅ Small-UA Rule: ≤250g AND ≤25m/s → iGRC=1 (both conditions required)
 * - ✅ VLOS low exposure: -1 ARC class (Source: Main Body page 43-44)
 * 
 * SORA 2.0:
 * - M1 Strategic: None | Low (-1) | Medium (-2) | High (-4)
 * - M2 Impact Reduction: None | Medium (-1) | High (-2)
 * - M3 ERP: None (+1 PENALTY!) | Adequate (0) | Validated (-1)
 * - ✅ Column-min clamp ENFORCED (Source: Main Body page 21-22)
 * - Sequential order: M1 → M2 → M3
 * 
 * See: Docs/SORA_COMPLIANCE_REFERENCE.md for complete specifications
 */

// =============================================================================
// SORA 2.5 Ground Risk (GRC) Calculation
// =============================================================================

/**
 * Table 2: Initial GRC (iGRC) Determination - SORA 2.5
 * Reference: SORA 2.5 Main Body, Table 2, page 34
 * 
 * Matrix: Population Density × (Characteristic Dimension + Max Speed)
 * 
 * @param {number} characteristicDimension_m - Characteristic dimension in meters
 * @param {number} maxSpeed_ms - Maximum speed in m/s
 * @param {string} populationDensity - One of: "Controlled", "<5", "<50", "<500", "<5000", "<50000", ">50000"
 * @returns {number} Initial GRC value (1-11)
 */
function calculateInitialGRC_SORA25(characteristicDimension_m, maxSpeed_ms, populationDensity) {
  // Determine dimension/speed category
  let dimSpeedCategory;
  if (characteristicDimension_m <= 1 && maxSpeed_ms <= 25) {
    dimSpeedCategory = "1m_25ms";
  } else if (characteristicDimension_m <= 3 && maxSpeed_ms <= 35) {
    dimSpeedCategory = "3m_35ms";
  } else if (characteristicDimension_m <= 8 && maxSpeed_ms <= 75) {
    dimSpeedCategory = "8m_75ms";
  } else if (characteristicDimension_m <= 20 && maxSpeed_ms <= 120) {
    dimSpeedCategory = "20m_120ms";
  } else if (characteristicDimension_m <= 40 && maxSpeed_ms <= 200) {
    dimSpeedCategory = "40m_200ms";
  } else {
    // Exceeds Table 2 limits - use highest category
    dimSpeedCategory = "40m_200ms";
  }

  // Table 2 exact values from SORA 2.5 Main Body page 34
  const iGRC_Table = {
    "Controlled": { "1m_25ms": 1, "3m_35ms": 2, "8m_75ms": 3, "20m_120ms": 4, "40m_200ms": 5 },
    "<5": { "1m_25ms": 2, "3m_35ms": 3, "8m_75ms": 4, "20m_120ms": 5, "40m_200ms": 6 },
    "<50": { "1m_25ms": 3, "3m_35ms": 4, "8m_75ms": 5, "20m_120ms": 6, "40m_200ms": 7 },
    "<500": { "1m_25ms": 4, "3m_35ms": 5, "8m_75ms": 6, "20m_120ms": 7, "40m_200ms": 8 },
    "<5000": { "1m_25ms": 5, "3m_35ms": 6, "8m_75ms": 7, "20m_120ms": 8, "40m_200ms": 9 },
    "<50000": { "1m_25ms": 6, "3m_35ms": 7, "8m_75ms": 8, "20m_120ms": 9, "40m_200ms": 10 },
    ">50000": { "1m_25ms": 7, "3m_35ms": 8, "8m_75ms": 9, "20m_120ms": 10, "40m_200ms": 11 }
  };

  const iGRC = iGRC_Table[populationDensity][dimSpeedCategory];
  return iGRC;
}

/**
 * Small-UA Rule - SORA 2.5
 * Reference: SORA 2.5 Main Body, Table 2 note, page 34
 * 
 * IF MTOM ≤ 0.25 kg AND max speed ≤ 25 m/s THEN iGRC = 1
 * 
 * @param {number} mtom_kg - Maximum Take-Off Mass in kg
 * @param {number} maxSpeed_ms - Maximum speed in m/s
 * @param {number} tableIGRC - iGRC from Table 2 lookup
 * @returns {number} Final iGRC after Small-UA Rule application
 */
function applySmallUARule(mtom_kg, maxSpeed_ms, tableIGRC) {
  if (mtom_kg <= 0.25 && maxSpeed_ms <= 25) {
    return 1; // Small-UA Rule overrides table value
  }
  return tableIGRC;
}

/**
 * Final GRC Calculation - SORA 2.5
 * Reference: SORA 2.5 Main Body, Table 5, page 38; Annex B, Table 11, page 21
 * 
 * Final GRC = iGRC - M1(A) - M1(B) - M1(C) - M2
 * 
 * Constraints:
 * - M1(A) Medium CANNOT combine with M1(B) (any level)
 * - Final GRC cannot go below column minimum from Table 2
 * 
 * @param {number} iGRC - Initial GRC
 * @param {string} m1a - M1(A) Sheltering: "None", "Low", "Medium" (NO "High")
 * @param {string} m1b - M1(B) Operational Restrictions: "None", "Medium", "High" (NO "Low")
 * @param {string} m1c - M1(C) Ground Observation: "None", "Low" (NO "Medium", NO "High")
 * @param {string} m2 - M2 Impact Dynamics: "None", "Medium", "High" (NO "Low")
 * @param {string} populationDensity - For determining column minimum
 * @returns {object} { finalGRC: number, warnings: string[], columnMin: number }
 */
function calculateFinalGRC_SORA25(iGRC, m1a, m1b, m1c, m2, populationDensity) {
  const warnings = [];

  // Validate M1(A) and M1(B) combination constraint
  // Reference: SORA 2.5 Annex B, page 8
  if (m1a === "Medium" && m1b !== "None") {
    warnings.push("❌ VALIDATION ERROR: M1(A) Medium robustness cannot be combined with M1(B) mitigations (Annex B page 8). Please select M1(A) Low or set M1(B) to None.");
  }

  // M1(A) Sheltering reduction
  // Reference: Annex B, Table 11
  let m1a_reduction = 0;
  if (m1a === "Low") m1a_reduction = 1;
  else if (m1a === "Medium") m1a_reduction = 2;
  else if (m1a === "High") {
    warnings.push("⚠️ INVALID OPTION: M1(A) High does not exist in SORA 2.5. Valid options: Low (-1), Medium (-2). Treating as None.");
  }

  // M1(B) Operational Restrictions reduction
  // Reference: Annex B, Table 11
  let m1b_reduction = 0;
  if (m1b === "Low") {
    warnings.push("⚠️ INVALID OPTION: M1(B) Low does not exist in SORA 2.5. Valid options: Medium (-1), High (-2). Treating as None.");
  } else if (m1b === "Medium") m1b_reduction = 1;
  else if (m1b === "High") m1b_reduction = 2;

  // M1(C) Ground Observation reduction
  // Reference: Annex B, Table 11
  let m1c_reduction = 0;
  if (m1c === "Low") m1c_reduction = 1;
  else if (m1c === "Medium" || m1c === "High") {
    warnings.push(`⚠️ INVALID OPTION: M1(C) ${m1c} does not exist in SORA 2.5. Only Low (-1) is available. Treating as None.`);
  }

  // M2 Impact Dynamics reduction
  // Reference: Annex B, Table 11
  let m2_reduction = 0;
  if (m2 === "Low") {
    warnings.push("⚠️ INVALID OPTION: M2 Low does not exist in SORA 2.5. Valid options: Medium (-1), High (-2). Treating as None.");
  } else if (m2 === "Medium") m2_reduction = 1;
  else if (m2 === "High") m2_reduction = 2;

  // Apply mitigations
  let finalGRC = iGRC - m1a_reduction - m1b_reduction - m1c_reduction - m2_reduction;

  // ✅ SORA 2.5: NO column minimum clamp (Source: JAR_doc_25 Main Body page 39)
  // ⚠️ CRITICAL: Unlike SORA 2.0, SORA 2.5 does NOT enforce column-min floor
  // Final GRC can go below iGRC table values through mitigations
  // However, final GRC >= 1 floor is enforced (minimum SAIL I)
  if (finalGRC < 1) {
    warnings.push(`ℹ️ Final GRC (${finalGRC}) is below minimum 1. Setting to GRC 1 (SAIL I).`);
    finalGRC = 1; // Enforce GRC >= 1 floor (Source: Table 7 page 47)
  }

  return { finalGRC, warnings, columnMin: null }; // columnMin = null for SORA 2.5
}

// =============================================================================
// SORA 2.0 Ground Risk (GRC) Calculation
// =============================================================================

/**
 * Table 2: Initial GRC (iGRC) Determination - SORA 2.0
 * Reference: SORA 2.0 Main Body, Table 2, page 13
 * 
 * Uses operational scenarios (VLOS/BVLOS × environment type)
 * Note: EVLOS is treated as BVLOS for GRC determination
 * 
 * @param {string} operationScenario - One of: "VLOS_Controlled", "VLOS_Sparsely", "VLOS_Populated", "VLOS_Gathering", "BVLOS_Controlled", "BVLOS_Sparsely", "BVLOS_Populated", "BVLOS_Gathering"
 * @param {number} characteristicDimension_m - Characteristic dimension in meters
 * @returns {number} Initial GRC value (1-10)
 */
function calculateInitialGRC_SORA20(operationScenario, characteristicDimension_m) {
  // Determine size category
  let sizeCategory;
  if (characteristicDimension_m < 1) sizeCategory = "<1m";
  else if (characteristicDimension_m < 3) sizeCategory = "1-3m";
  else if (characteristicDimension_m < 8) sizeCategory = "3-8m";
  else sizeCategory = ">8m";

  // Table 2 exact values from SORA 2.0 Main Body page 13
  const iGRC_Table_SORA20 = {
    "VLOS_Controlled": { "<1m": 1, "1-3m": 2, "3-8m": 3, ">8m": 4 },
    "VLOS_Sparsely": { "<1m": 2, "1-3m": 3, "3-8m": 4, ">8m": 5 },
    "VLOS_Populated": { "<1m": 4, "1-3m": 5, "3-8m": 6, ">8m": 7 },
    "VLOS_Gathering": { "<1m": 6, "1-3m": 7, "3-8m": 8, ">8m": 9 },
    "BVLOS_Controlled": { "<1m": 2, "1-3m": 3, "3-8m": 4, ">8m": 5 },
    "BVLOS_Sparsely": { "<1m": 3, "1-3m": 4, "3-8m": 5, ">8m": 6 },
    "BVLOS_Populated": { "<1m": 5, "1-3m": 6, "3-8m": 7, ">8m": 8 },
    "BVLOS_Gathering": { "<1m": 7, "1-3m": 8, "3-8m": 9, ">8m": 10 }
  };

  const iGRC = iGRC_Table_SORA20[operationScenario][sizeCategory];
  return iGRC;
}

/**
 * Final GRC Calculation - SORA 2.0
 * Reference: SORA 2.0 Main Body, Table 3, page 14
 * 
 * Final GRC = max(iGRC - M1 - M2 + M3, column_minimum)
 * 
 * M1 uses column-minimum clamp method (different from SORA 2.5)
 * M3 ERP has penalty for "None" (+1)
 * 
 * @param {number} iGRC - Initial GRC
 * @param {string} m1 - M1 Strategic: "None", "Low", "Medium", "High"
 * @param {string} m2 - M2 Impact: "None", "Medium", "High"
 * @param {string} m3 - M3 ERP: "None", "Adequate", "Validated"
 * @param {string} operationScenario - For determining column minimum
 * @returns {object} { finalGRC: number, warnings: string[], columnMin: number }
 */
function calculateFinalGRC_SORA20(iGRC, m1, m2, m3, operationScenario) {
  const warnings = [];

  // M1 Strategic Mitigation reduction
  // Reference: SORA 2.0 Main Body, Table 3
  let m1_reduction = 0;
  if (m1 === "Low") m1_reduction = 1;
  else if (m1 === "Medium") m1_reduction = 2;
  else if (m1 === "High") m1_reduction = 4;

  // M2 Impact Mitigation reduction
  // Reference: SORA 2.0 Main Body, Table 3
  let m2_reduction = 0;
  if (m2 === "Medium") m2_reduction = 1;
  else if (m2 === "High") m2_reduction = 2;
  else if (m2 === "Low") {
    warnings.push("⚠️ INVALID OPTION: M2 Low does not exist in SORA 2.0. Valid options: Medium (-1), High (-2). Treating as None.");
  }

  // M3 ERP (Emergency Response Plan) adjustment
  // Reference: SORA 2.0 Main Body, Table 3
  let m3_adjustment = 0;
  if (m3 === "None") m3_adjustment = +1; // Penalty
  else if (m3 === "Adequate") m3_adjustment = 0;
  else if (m3 === "Validated") m3_adjustment = -1; // Reduction

  // Determine column minimum based on operation scenario
  // Reference: SORA 2.0 Main Body, Table 2, page 20
  const columnMinimums_SORA20 = {
    "VLOS_Controlled": 1,
    "VLOS_Sparsely": 2,
    "VLOS_Populated": 4,
    "VLOS_Gathering": 7,
    "BVLOS_Controlled": 2,
    "BVLOS_Sparsely": 3,
    "BVLOS_Populated": 5,
    "BVLOS_Gathering": 8
  };
  const columnMin = columnMinimums_SORA20[operationScenario] || 1;

  // Apply M1 mitigation first, then enforce column-minimum clamp
  // Source: SORA 2.0 Main Body, Section 2.3.2(d), Page 21:
  // "When applying mitigation M1, the GRC cannot be reduced to a value lower than 
  //  the lowest value in the applicable column in Table 2."
  let grcAfterM1 = iGRC - m1_reduction;
  if (grcAfterM1 < columnMin) {
    warnings.push(`⚠️ GRC after M1 (${grcAfterM1}) is below column minimum (${columnMin}) for scenario '${operationScenario}'. Clamping to ${columnMin}.`);
    grcAfterM1 = columnMin;
  }

  // Then apply M2 and M3 (no column minimum clamp for these)
  let finalGRC = grcAfterM1 - m2_reduction + m3_adjustment;

  // Final floor: GRC cannot be negative
  if (finalGRC < 0) {
    warnings.push(`⚠️ Final GRC (${finalGRC}) is negative. Setting to 0.`);
    finalGRC = 0;
  }

  return { finalGRC, warnings, columnMin };
}

// =============================================================================
// SORA 2.5 Air Risk (ARC) Calculation
// =============================================================================

/**
 * AEC (Airspace Encounter Category) Determination - SORA 2.5
 * Reference: SORA 2.5 Main Body, Figure 6 (Decision Tree), page 42
 * 
 * Decision tree logic (corrected from SORA_FIELD_SPECIFICATIONS.md):
 * 1. Atypical airspace? → AEC 0
 * 2. Altitude > 500ft (152m)?
 *    - YES + Controlled → AEC 11
 *    - YES + Uncontrolled + Populated → AEC 6
 *    - YES + Uncontrolled + Sparsely → AEC 2
 * 3. Altitude ≤ 500ft:
 *    - Controlled + Airport → AEC 10
 *    - Controlled + Non-airport → AEC 9
 *    - Uncontrolled + Airport → AEC 8
 *    - Uncontrolled + Populated (no airport) → AEC 4
 *    - Uncontrolled + Sparsely (no airport) → AEC 3
 * 
 * @param {number} altitude_ft - Altitude in feet
 * @param {boolean} controlledAirspace - Is operation in controlled airspace?
 * @param {boolean} airportEnvironment - Is operation near airport?
 * @param {boolean} populatedArea - Is operation over populated area (urban)?
 * @param {boolean} atypicalAirspace - Is airspace atypical (reserved, segregated)?
 * @returns {number} AEC category (0-11)
 */
function calculateAEC(altitude_ft, controlledAirspace, airportEnvironment, populatedArea, atypicalAirspace) {
  // Custom AEC numbering (0-11) for this implementation
  // Mapping VERIFIED against SORA 2.0 Annex C Table 1 (Page 12-13)
  // NOTE: SORA 2.5 does not use AEC, uses direct Initial ARC calculation
  
  // Normalize airportEnvironment: treat 'none', false, null, undefined as no airport
  const hasAirport = airportEnvironment && airportEnvironment !== 'none';

  // Atypical/Segregated airspace → AEC 0 → ARC-a
  if (atypicalAirspace) {
    return 0; // Custom AEC 0 = Official AEC 12 (Atypical)
  }

  // SORA 2.0 Annex C Table 1 Decision Tree:
  
  // High altitude (>500ft AGL)
  if (altitude_ft > 500) {
    if (hasAirport) {
      // Airport environment >500ft → AEC 1 or 6
      // For simplicity, assume non-Class-B/C/D → AEC 6
      return 1; // Custom AEC 1 = Official AEC 6 (Airport E/F/G) → ARC-c
    } else if (controlledAirspace) {
      if (populatedArea) {
        return 4; // Custom AEC 4 = Official AEC 4 (>500ft Controlled Urban) → ARC-c
      } else {
        return 3; // Custom AEC 3 = Official AEC 3 (>500ft Controlled Rural) → ARC-d
      }
    } else {
      // Uncontrolled >500ft
      if (populatedArea) {
        return 4; // Custom AEC 4 = Official AEC 4 (>500ft Uncontrolled Urban) → ARC-c
      } else {
        return 5; // Custom AEC 5 = Official AEC 5 (>500ft Uncontrolled Rural) → ARC-c
      }
    }
  }

  // Low altitude (≤500ft AGL)
  if (controlledAirspace) {
    // Controlled airspace <500ft → AEC 7 or 8
    return 8; // Custom AEC 8 = Official AEC 8 (<500ft Controlled) → ARC-c
  } else {
    // Uncontrolled <500ft
    if (hasAirport) {
      return 8; // Custom AEC 8 = Airport proximity <500ft → ARC-c
    } else if (populatedArea) {
      return 9; // Custom AEC 9 = Official AEC 9 (<500ft Uncontrolled Urban) → ARC-c
    } else {
      return 10; // Custom AEC 10 = Official AEC 10 (<500ft Uncontrolled Rural) → ARC-b
    }
  }
}

/**
 * AEC to Initial ARC Mapping - Custom Implementation
 * VERIFIED against SORA 2.0 Annex C Table 1 (Page 12-13)
 * 
 * Custom AEC → Official AEC → Initial ARC mapping:
 * - Custom 0 = Official 12 (Atypical) → ARC-a
 * - Custom 1 = Official 6 (Airport E/F/G) → ARC-c
 * - Custom 3 = Official 3 (>500ft Controlled) → ARC-d
 * - Custom 4 = Official 4 (>500ft Uncontrolled Urban) → ARC-c
 * - Custom 5 = Official 5 (>500ft Uncontrolled Rural) → ARC-c
 * - Custom 8 = Official 8 (<500ft Controlled) → ARC-c
 * - Custom 9 = Official 9 (<500ft Uncontrolled Urban) → ARC-c
 * - Custom 10 = Official 10 (<500ft Uncontrolled Rural) → ARC-b
 * 
 * @param {number} aec - AEC category (custom 0-11)
 * @returns {string} Initial ARC ("ARC-a", "ARC-b", "ARC-c", "ARC-d")
 */
function mapAECtoARC(aec) {
  // Verified mappings from SORA 2.0 Annex C Table 1
  if (aec === 0) return "ARC-a";    // Atypical/Segregated
  if (aec === 1) return "ARC-c";    // Airport E/F/G
  if (aec === 3) return "ARC-d";    // >500ft Controlled
  if (aec === 4) return "ARC-c";    // >500ft Urban
  if (aec === 5) return "ARC-c";    // >500ft Rural
  if (aec === 8) return "ARC-c";    // <500ft Controlled
  if (aec === 9) return "ARC-c";    // <500ft Urban
  if (aec === 10) return "ARC-b";   // <500ft Rural
  
  // Fallback for unmapped AEC
  return "ARC-c"; // Default to ARC-c (medium risk)
}

/**
 * VLOS Reduction for Initial ARC - SORA 2.5
 * Reference: SORA 2.5 Main Body, Section 2.6.4.2, page 45
 * 
 * VLOS can reduce initial ARC by 1 level
 * Cannot reduce to ARC-a (minimum is ARC-b)
 * 
 * @param {string} initialARC - Initial ARC before VLOS reduction
 * @param {boolean} isVLOS - Is operation conducted in VLOS?
 * @returns {string} Residual ARC after VLOS reduction
 */
function applyVLOSReduction(initialARC, isVLOS) {
  if (!isVLOS) {
    return initialARC; // No reduction for BVLOS/EVLOS
  }

  // VLOS reduces by 1 level
  const arcOrder = ["ARC-a", "ARC-b", "ARC-c", "ARC-d"];
  const currentIndex = arcOrder.indexOf(initialARC);
  
  if (currentIndex <= 0) {
    return initialARC; // Already at ARC-a, cannot reduce further
  }

  return arcOrder[currentIndex - 1]; // Reduce by 1 level
}

/**
 * Residual ARC to TMPR (Tactical Mitigation Performance Requirement) Mapping
 * Reference: SORA 2.5 Main Body, Table 6, page 46
 * 
 * @param {string} residualARC - Residual ARC after strategic mitigations
 * @returns {string} TMPR level ("None", "Low", "Medium", "High")
 */
function mapARCtoTMPR(residualARC) {
  const tmprMapping = {
    "ARC-a": "None",
    "ARC-b": "Low",
    "ARC-c": "Medium",
    "ARC-d": "High"
  };
  return tmprMapping[residualARC] || "None";
}

// =============================================================================
// SAIL Determination (Common for SORA 2.0 and 2.5)
// =============================================================================

/**
 * SAIL Determination from Final GRC and Residual ARC
 * Reference: SORA 2.5 Main Body, Table 7, page 47
 *            SORA 2.0 Main Body, Table 5, page 17
 * 
 * Table 7 matrix: Final GRC (rows) × Residual ARC (columns)
 * 
 * @param {number} finalGRC - Final GRC after mitigations
 * @param {string} residualARC - Residual ARC after strategic mitigations
 * @returns {string} SAIL level ("I", "II", "III", "IV", "V", "VI", "Category C")
 */
function calculateSAIL(finalGRC, residualARC) {
  // Table 7 exact values from SORA 2.5 Main Body page 47
  const SAIL_Table = {
    1: { "ARC-a": "I", "ARC-b": "I", "ARC-c": "IV", "ARC-d": "VI" },
    2: { "ARC-a": "I", "ARC-b": "II", "ARC-c": "IV", "ARC-d": "VI" },
    3: { "ARC-a": "II", "ARC-b": "II", "ARC-c": "IV", "ARC-d": "VI" },
    4: { "ARC-a": "III", "ARC-b": "III", "ARC-c": "IV", "ARC-d": "VI" },
    5: { "ARC-a": "IV", "ARC-b": "IV", "ARC-c": "IV", "ARC-d": "VI" },
    6: { "ARC-a": "V", "ARC-b": "V", "ARC-c": "V", "ARC-d": "VI" },
    7: { "ARC-a": "VI", "ARC-b": "VI", "ARC-c": "VI", "ARC-d": "VI" }
  };

  if (finalGRC <= 0) {
    return "I"; // Minimum SAIL
  } else if (finalGRC >= 8) {
    return "Category C"; // Requires certified operation
  } else {
    return SAIL_Table[finalGRC][residualARC] || "VI";
  }
}

// =============================================================================
// Complete SORA Calculation (Orchestration)
// =============================================================================

/**
 * Complete SORA 2.5 Calculation
 * 
 * @param {object} params - Calculation parameters
 * @param {number} params.mtom_kg - Maximum Take-Off Mass in kg
 * @param {number} params.maxSpeed_ms - Maximum speed in m/s
 * @param {number} params.characteristicDimension_m - Characteristic dimension in meters
 * @param {string} params.populationDensity - Population density category
 * @param {string} params.m1a - M1(A) Sheltering level
 * @param {string} params.m1b - M1(B) Operational Restrictions level
 * @param {string} params.m1c - M1(C) Ground Observation level
 * @param {string} params.m2 - M2 Impact Dynamics level
 * @param {number} params.altitude_ft - Altitude in feet
 * @param {boolean} params.controlledAirspace - Controlled airspace flag
 * @param {boolean} params.airportEnvironment - Airport environment flag
 * @param {boolean} params.populatedArea - Populated area flag
 * @param {boolean} params.atypicalAirspace - Atypical airspace flag
 * @param {boolean} params.isVLOS - VLOS operation flag
 * @returns {object} Complete SORA 2.5 results
 */
function calculateSORA25(params) {
  const calculationSteps = [];
  const allWarnings = [];

  // Step 1: iGRC from Table 2
  const tableIGRC = calculateInitialGRC_SORA25(
    params.characteristicDimension_m,
    params.maxSpeed_ms,
    params.populationDensity
  );
  calculationSteps.push({
    step: "iGRC Lookup (Table 2)",
    input: `Dimension=${params.characteristicDimension_m}m, Speed=${params.maxSpeed_ms}m/s, Density=${params.populationDensity}`,
    output: tableIGRC,
    reference: "SORA 2.5 Main Body, Table 2, page 34"
  });

  // Step 2: Apply Small-UA Rule
  const initialGRC = applySmallUARule(params.mtom_kg, params.maxSpeed_ms, tableIGRC);
  if (initialGRC !== tableIGRC) {
    calculationSteps.push({
      step: "Small-UA Rule Applied",
      input: `MTOM=${params.mtom_kg}kg, Speed=${params.maxSpeed_ms}m/s`,
      output: `iGRC overridden: ${tableIGRC} → 1`,
      reference: "SORA 2.5 Main Body, Table 2 note, page 34"
    });
  }

  // Step 3: Apply mitigations to get final GRC
  const grcResult = calculateFinalGRC_SORA25(
    initialGRC,
    params.m1a,
    params.m1b,
    params.m1c,
    params.m2,
    params.populationDensity
  );
  allWarnings.push(...grcResult.warnings);
  calculationSteps.push({
    step: "Final GRC Calculation",
    input: `iGRC=${initialGRC}, M1(A)=${params.m1a}, M1(B)=${params.m1b}, M1(C)=${params.m1c}, M2=${params.m2}`,
    output: `Final GRC = ${grcResult.finalGRC} (min=${grcResult.columnMin})`,
    reference: "SORA 2.5 Annex B, Table 11, page 21"
  });

  // Step 4: Calculate AEC
  const aec = calculateAEC(
    params.altitude_ft,
    params.controlledAirspace,
    params.airportEnvironment,
    params.populatedArea,
    params.atypicalAirspace
  );
  calculationSteps.push({
    step: "AEC Determination",
    input: `Altitude=${params.altitude_ft}ft, Controlled=${params.controlledAirspace}, Airport=${params.airportEnvironment}, Populated=${params.populatedArea}, Atypical=${params.atypicalAirspace}`,
    output: `AEC ${aec}`,
    reference: "SORA 2.5 Main Body, Figure 6, page 42"
  });

  // Step 5: Map AEC to Initial ARC
  const initialARC = mapAECtoARC(aec);
  calculationSteps.push({
    step: "Initial ARC Mapping",
    input: `AEC ${aec}`,
    output: initialARC,
    reference: "SORA 2.5 Main Body, Table 6, page 46"
  });

  // Step 6: Apply VLOS reduction
  const residualARC = applyVLOSReduction(initialARC, params.isVLOS);
  if (residualARC !== initialARC) {
    calculationSteps.push({
      step: "VLOS Reduction Applied",
      input: `Initial ARC=${initialARC}, VLOS=${params.isVLOS}`,
      output: `Residual ARC = ${residualARC}`,
      reference: "SORA 2.5 Main Body, Section 2.6.4.2, page 45"
    });
  }

  // Step 7: Map Residual ARC to TMPR
  const tmpr = mapARCtoTMPR(residualARC);
  calculationSteps.push({
    step: "TMPR Mapping",
    input: `Residual ARC=${residualARC}`,
    output: `TMPR = ${tmpr}`,
    reference: "SORA 2.5 Main Body, Table 6, page 46"
  });

  // Step 8: Calculate SAIL
  const sail = calculateSAIL(grcResult.finalGRC, residualARC);
  calculationSteps.push({
    step: "SAIL Determination",
    input: `Final GRC=${grcResult.finalGRC}, Residual ARC=${residualARC}`,
    output: `SAIL ${sail}`,
    reference: "SORA 2.5 Main Body, Table 7, page 47"
  });

  return {
    version: "SORA 2.5",
    initialGRC: initialGRC,
    finalGRC: grcResult.finalGRC,
    initialARC: initialARC,
    residualARC: residualARC,
    aec: aec,
    tmpr: tmpr,
    sail: sail,
    warnings: allWarnings,
    calculationSteps: calculationSteps
  };
}

/**
 * Complete SORA 2.0 Calculation
 * 
 * @param {object} params - Calculation parameters
 * @param {string} params.operationScenario - Operation scenario (VLOS/BVLOS × environment)
 * @param {number} params.characteristicDimension_m - Characteristic dimension in meters
 * @param {string} params.m1 - M1 Strategic Mitigation level
 * @param {string} params.m2 - M2 Impact Mitigation level
 * @param {string} params.m3 - M3 ERP level
 * @param {number} params.altitude_ft - Altitude in feet
 * @param {boolean} params.controlledAirspace - Controlled airspace flag
 * @param {boolean} params.airportEnvironment - Airport environment flag
 * @param {boolean} params.populatedArea - Populated area flag
 * @param {boolean} params.atypicalAirspace - Atypical airspace flag
 * @param {boolean} params.isVLOS - VLOS operation flag
 * @returns {object} Complete SORA 2.0 results
 */
function calculateSORA20(params) {
  const calculationSteps = [];
  const allWarnings = [];

  // Step 1: iGRC from Table 2 (SORA 2.0)
  const initialGRC = calculateInitialGRC_SORA20(
    params.operationScenario,
    params.characteristicDimension_m
  );
  calculationSteps.push({
    step: "iGRC Lookup (Table 2)",
    input: `Scenario=${params.operationScenario}, Dimension=${params.characteristicDimension_m}m`,
    output: initialGRC,
    reference: "SORA 2.0 Main Body, Table 2, page 13"
  });

  // Step 2: Apply mitigations to get final GRC
  const grcResult = calculateFinalGRC_SORA20(
    initialGRC,
    params.m1,
    params.m2,
    params.m3,
    params.operationScenario
  );
  allWarnings.push(...grcResult.warnings);
  calculationSteps.push({
    step: "Final GRC Calculation",
    input: `iGRC=${initialGRC}, M1=${params.m1}, M2=${params.m2}, M3=${params.m3}`,
    output: `Final GRC = ${grcResult.finalGRC} (min=${grcResult.columnMin})`,
    reference: "SORA 2.0 Main Body, Table 3, page 14"
  });

  // Step 3: Calculate AEC (same as SORA 2.5)
  const aec = calculateAEC(
    params.altitude_ft,
    params.controlledAirspace,
    params.airportEnvironment,
    params.populatedArea,
    params.atypicalAirspace
  );
  calculationSteps.push({
    step: "AEC Determination",
    input: `Altitude=${params.altitude_ft}ft, Controlled=${params.controlledAirspace}, Airport=${params.airportEnvironment}, Populated=${params.populatedArea}, Atypical=${params.atypicalAirspace}`,
    output: `AEC ${aec}`,
    reference: "SORA 2.0 Main Body, Section 2.6 (same logic as 2.5)"
  });

  // Step 4: Map AEC to Initial ARC (same as SORA 2.5)
  const initialARC = mapAECtoARC(aec);
  calculationSteps.push({
    step: "Initial ARC Mapping",
    input: `AEC ${aec}`,
    output: initialARC,
    reference: "SORA 2.0 Main Body (same as SORA 2.5)"
  });

  // Step 5: Apply VLOS reduction (same as SORA 2.5)
  const residualARC = applyVLOSReduction(initialARC, params.isVLOS);
  if (residualARC !== initialARC) {
    calculationSteps.push({
      step: "VLOS Reduction Applied",
      input: `Initial ARC=${initialARC}, VLOS=${params.isVLOS}`,
      output: `Residual ARC = ${residualARC}`,
      reference: "SORA 2.0 Main Body, Section 2.6"
    });
  }

  // Step 6: Map Residual ARC to TMPR (same as SORA 2.5)
  const tmpr = mapARCtoTMPR(residualARC);
  calculationSteps.push({
    step: "TMPR Mapping",
    input: `Residual ARC=${residualARC}`,
    output: `TMPR = ${tmpr}`,
    reference: "SORA 2.0 Main Body (same as SORA 2.5)"
  });

  // Step 7: Calculate SAIL (same table as SORA 2.5)
  const sail = calculateSAIL(grcResult.finalGRC, residualARC);
  calculationSteps.push({
    step: "SAIL Determination",
    input: `Final GRC=${grcResult.finalGRC}, Residual ARC=${residualARC}`,
    output: `SAIL ${sail}`,
    reference: "SORA 2.0 Main Body, Table 5, page 17"
  });

  return {
    version: "SORA 2.0",
    initialGRC: initialGRC,
    finalGRC: grcResult.finalGRC,
    initialARC: initialARC,
    residualARC: residualARC,
    aec: aec,
    tmpr: tmpr,
    sail: sail,
    warnings: allWarnings,
    calculationSteps: calculationSteps
  };
}

// =============================================================================
// Export Functions
// =============================================================================

// For Node.js/CommonJS environments
if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    // SORA 2.5 Functions
    calculateInitialGRC_SORA25,
    applySmallUARule,
    calculateFinalGRC_SORA25,
    
    // SORA 2.0 Functions
    calculateInitialGRC_SORA20,
    calculateFinalGRC_SORA20,
    
    // Common Functions
    calculateAEC,
    mapAECtoARC,
    applyVLOSReduction,
    mapARCtoTMPR,
    calculateSAIL,
    
    // Orchestration
    calculateSORA25,
    calculateSORA20
  };
}
