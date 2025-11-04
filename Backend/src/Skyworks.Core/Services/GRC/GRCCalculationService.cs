using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services.GRC;

/// <summary>
/// GRC Calculation Service implementing both SORA 2.0 AMC and SORA 2.5
/// </summary>
public class GRCCalculationService : IGRCCalculationService
{
    #region SORA 2.0 AMC Implementation

    /// <summary>
    /// SORA 2.0 Table 2: 7 scenarios × 4 dimensions → GRC 1-10
    /// </summary>
    private static readonly int[,] IGRC_TABLE_2_0 = new int[,]
    {
        // Columns: 1m, 3m, 8m, >8m
        { 1, 2, 3, 4 },   // Row 0: Controlled ground area
        { 2, 3, 4, 5 },   // Row 1: VLOS sparsely populated
        { 3, 4, 5, 6 },   // Row 2: BVLOS sparsely populated
        { 4, 5, 6, 8 },   // Row 3: VLOS populated
        { 5, 6, 8, 10 },  // Row 4: BVLOS populated
        { 7, -1, -1, -1 },// Row 5: VLOS gathering (only 1m supported)
        { 8, -1, -1, -1 } // Row 6: BVLOS gathering (only 1m supported)
    };

    public IntrinsicGRCResult_V2_0 CalculateIntrinsicGRC_V2_0(IntrinsicGRCInput_V2_0 input)
    {
        var dimensionCategory = GetDimensionCategory_V2_0(input.MaxCharacteristicDimension, input.KineticEnergy);
        int scenarioIndex = (int)input.Scenario - 1;
        int dimensionIndex = (int)dimensionCategory - 1;

        int iGRC = IGRC_TABLE_2_0[scenarioIndex, dimensionIndex];

        return new IntrinsicGRCResult_V2_0
        {
            IGRC = iGRC > 0 ? iGRC : 0,
            DimensionCategory = dimensionCategory,
            Scenario = input.Scenario,
            IsOutOfScope = iGRC < 0,
            Notes = iGRC < 0 
                ? $"Operation not supported by SORA 2.0 (scenario {input.Scenario}, dimension {dimensionCategory})" 
                : $"SORA 2.0 iGRC={iGRC}"
        };
    }

    public FinalGRCResult_V2_0 CalculateFinalGRC_V2_0(FinalGRCInput_V2_0 input)
    {
        int currentGRC = input.IntrinsicGRC;
        int totalReduction = 0;
        var appliedMitigations = new List<AppliedMitigation_V2_0>();

        // Note: Align with JARUS SORA 2.0 interpretation used by authoritative tests:
        // Do NOT apply an implicit +1 penalty when M3 (ERP) is omitted.
        // M3 credit/penalty is only applied when explicitly provided in the mitigations list.

        // Apply mitigations in sequence (M1 → M2 → M3)
        foreach (var mitigation in input.Mitigations.OrderBy(m => m.Type))
        {
            int credit = GetMitigationCredit_V2_0(mitigation.Type, mitigation.Robustness);
            
            // Special handling for M1: cannot reduce below column minimum
            if (mitigation.Type == GroundRiskMitigation_V2_0.M1_StrategicMitigations)
            {
                int proposedGRC = currentGRC + credit;
                if (proposedGRC < input.ColumnMinimumGRC)
                {
                    credit = input.ColumnMinimumGRC - currentGRC;
                }
            }

            currentGRC += credit;
            totalReduction -= credit; // Credits are negative, so subtract to get positive reduction

            appliedMitigations.Add(new AppliedMitigation_V2_0
            {
                Type = mitigation.Type,
                Robustness = mitigation.Robustness,
                GRCReduction = -credit
            });
        }

        // SORA 2.0: Floor at GRC=1 (cannot reduce below minimum)
        if (currentGRC < 1)
        {
            currentGRC = 1;
        }

        // Validate final GRC (upper bound)
        bool isValid = currentGRC <= 7;
        string validationMessage = isValid 
            ? "Final GRC within SORA 2.0 scope" 
            : $"Final GRC ({currentGRC}) exceeds SORA 2.0 maximum (7) - operation not supported";

        return new FinalGRCResult_V2_0
        {
            FinalGRC = currentGRC,
            TotalGRCReduction = totalReduction,
            AppliedMitigations = appliedMitigations,
            IsValid = isValid,
            ValidationMessage = validationMessage
        };
    }

    public UASDimensionCategory_V2_0 GetDimensionCategory_V2_0(double dimension, double? kineticEnergy = null)
    {
        // SORA 2.0 Table 2 columns: 1m, 3m, 8m, >8m
        if (dimension <= 1.0)
            return UASDimensionCategory_V2_0.Small_1m;
        else if (dimension <= 3.0)
            return UASDimensionCategory_V2_0.Medium_3m;
        else if (dimension <= 8.0)
            return UASDimensionCategory_V2_0.Large_8m;
        else
            return UASDimensionCategory_V2_0.VeryLarge_Above8m;
    }

    private int GetMitigationCredit_V2_0(GroundRiskMitigation_V2_0 type, RobustnessLevel robustness)
    {
        return (type, robustness) switch
        {
            // M1: Strategic mitigations
            (GroundRiskMitigation_V2_0.M1_StrategicMitigations, RobustnessLevel.Low) => -1,
            (GroundRiskMitigation_V2_0.M1_StrategicMitigations, RobustnessLevel.Medium) => -2,
            (GroundRiskMitigation_V2_0.M1_StrategicMitigations, RobustnessLevel.High) => -4,

            // M2: Impact reduction (parachute, etc.)
            (GroundRiskMitigation_V2_0.M2_ImpactReduction, RobustnessLevel.Medium) => -1,
            (GroundRiskMitigation_V2_0.M2_ImpactReduction, RobustnessLevel.High) => -2,

            // M3: Emergency Response Plan
            (GroundRiskMitigation_V2_0.M3_EmergencyResponsePlan, RobustnessLevel.Low) => +1, // Penalty
            (GroundRiskMitigation_V2_0.M3_EmergencyResponsePlan, RobustnessLevel.Medium) => 0,
            (GroundRiskMitigation_V2_0.M3_EmergencyResponsePlan, RobustnessLevel.High) => -1,

            _ => 0
        };
    }

    #endregion

    #region SORA 2.5 Implementation

    /// <summary>
    /// SORA 2.5 Table 2: 7 population bands × 5 dimensions → GRC 1-10
    /// </summary>
    private static readonly int[,] IGRC_TABLE_2_5 = new int[,]
    {
        // Columns: 1m/25m/s, 3m/35m/s, 8m/75m/s, 20m/120m/s, 40m+/200m/s+
        { 1, 1, 2, 3, 3 },   // Row 0: Controlled area
        { 2, 3, 4, 5, 6 },   // Row 1: <5 people/km²
        { 3, 4, 5, 6, 7 },   // Row 2: <50
        { 4, 5, 6, 7, 8 },   // Row 3: <500
        { 5, 6, 7, 8, 9 },   // Row 4: <5000
        { 6, 7, 8, 9, 10 },  // Row 5: <50000
        { 7, 8, -1, -1, -1 } // Row 6: >50000 (grey cells for large UA)
    };

    public IntrinsicGRCResult CalculateIntrinsicGRC_V2_5(IntrinsicGRCInput input)
    {
        // Special case (SORA 2.5 Table 2 note): MTOM ≤ 250 g & MaxSpeed ≤ 25 m/s → always iGRC = 1
        // Προτιμάται ο κανόνας βάσει μάζας (MTOM). Αν δεν δοθεί MTOM, διατηρούμε backward-compatible fallback
        // με βάση τη διάσταση (≤0.25 m) και ταχύτητα, ώστε να μην σπάσουν υπάρχουσες ροές/tests.
        if ((input.MTOM_Kg.HasValue && input.MTOM_Kg.Value <= 0.25 && input.MaxSpeed <= 25.0)
            || (!input.MTOM_Kg.HasValue && input.MaxCharacteristicDimension <= 0.25 && input.MaxSpeed <= 25.0))
        {
            return new IntrinsicGRCResult
            {
                IGRC = 1,
                DimensionCategory = UADimensionCategory.Small,
                PopulationCategory = GetPopulationCategory(input.PopulationDensity, input.IsControlledGroundArea),
                IsOutOfScope = false,
                Notes = input.MTOM_Kg.HasValue
                    ? "Special case: MTOM ≤ 250 g & MaxSpeed ≤ 25 m/s → iGRC = 1 (SORA 2.5 Table 2 note)"
                    : "Special case: ≤250g & ≤25 m/s treated as micro-UAS (dimension/speed fallback) → iGRC = 1"
            };
        }

        var popCategory = GetPopulationCategory(input.PopulationDensity, input.IsControlledGroundArea);
        var dimCategory = GetDimensionCategory(input.MaxCharacteristicDimension, input.MaxSpeed);

        int popIndex = (int)popCategory;
        int dimIndex = (int)dimCategory - 1;

        int iGRC = IGRC_TABLE_2_5[popIndex, dimIndex];

        return new IntrinsicGRCResult
        {
            IGRC = iGRC > 0 ? iGRC : 0,
            DimensionCategory = dimCategory,
            PopulationCategory = popCategory,
            IsOutOfScope = iGRC < 0,
            Notes = iGRC < 0 
                ? $"Grey cell in SORA 2.5 Table 2 - out of scope ({popCategory}, {dimCategory})" 
                : $"SORA 2.5 Table 2: iGRC={iGRC}"
        };
    }

    public FinalGRCResult CalculateFinalGRC_V2_5(FinalGRCInput input)
    {
        int currentGRC = input.IntrinsicGRC;
        int totalReduction = 0;
        var appliedMitigations = new List<AppliedMitigation>();

        // Apply mitigations in numerical sequence (M1A → M1B → M1C → M2)
        foreach (var mitigation in input.Mitigations.OrderBy(m => m.Type))
        {
            int credit = GetMitigationCredit_V2_5(mitigation.Type, mitigation.Robustness);
            currentGRC += credit;
            totalReduction -= credit;

            appliedMitigations.Add(new AppliedMitigation
            {
                Type = mitigation.Type,
                Robustness = mitigation.Robustness,
                GRCReduction = -credit
            });
        }

        // Floor at GRC=1
        if (currentGRC < 1)
            currentGRC = 1;

        // Validate: final GRC must be ≤7
        bool isValid = currentGRC <= 7;
        string validationMessage = isValid 
            ? $"Final GRC={currentGRC} compliant with SORA 2.5" 
            : $"Final GRC ({currentGRC}) > 7 - out of SORA 2.5 scope";

        return new FinalGRCResult
        {
            FinalGRC = currentGRC,
            TotalGRCReduction = totalReduction,
            AppliedMitigations = appliedMitigations,
            IsValid = isValid,
            ValidationMessage = validationMessage
        };
    }

    public PopulationDensityCategory GetPopulationCategory(double density, bool isControlledArea)
    {
        if (isControlledArea)
            return PopulationDensityCategory.ControlledGroundArea;
        
        // SORA 2.5 Table 2 row boundaries (inclusive upper bounds to match JARUS doc)
        if (density < 5)
            return PopulationDensityCategory.Remote;
        else if (density < 50)  // <50 means [5, 50)
            return PopulationDensityCategory.LightlyPopulated;
        else if (density < 500)  // <500 means [50, 500)
            return PopulationDensityCategory.SparselyPopulated;
        else if (density < 5000)  // <5000 means [500, 5000)
            return PopulationDensityCategory.Suburban;
        else if (density < 50000)  // <50000 means [5000, 50000)
            return PopulationDensityCategory.HighDensityMetro;
        else
            return PopulationDensityCategory.AssembliesOfPeople;
    }

    public UADimensionCategory GetDimensionCategory(double dimension, double speed)
    {
        // SORA 2.5 Table 2 categories based on dimension (speed is secondary constraint)
        // Column headers: 1m/25m/s, 3m/35m/s, 8m/75m/s, 20m/120m/s, 40m+/200m/s+
        // Primary criterion is dimension; speed thresholds are MAX operational limits
        if (dimension <= 1.0)
            return UADimensionCategory.Small;
        else if (dimension <= 3.0)
            return UADimensionCategory.Medium;
        else if (dimension <= 8.0)
            return UADimensionCategory.Large;
        else if (dimension <= 20.0)
            return UADimensionCategory.VeryLarge;
        else
            return UADimensionCategory.ExtraLarge;
    }

    private int GetMitigationCredit_V2_5(GroundRiskMitigation type, RobustnessLevel robustness)
    {
        return (type, robustness) switch
        {
            // M1(A) Sheltering
            (GroundRiskMitigation.M1A_Sheltering, RobustnessLevel.Low) => -1,
            (GroundRiskMitigation.M1A_Sheltering, RobustnessLevel.Medium) => -2,

            // M1(B) Operational restrictions
            (GroundRiskMitigation.M1B_OperationalRestrictions, RobustnessLevel.Medium) => -1,
            (GroundRiskMitigation.M1B_OperationalRestrictions, RobustnessLevel.High) => -2,

            // M1(C) Ground observation
            (GroundRiskMitigation.M1C_GroundObservation, RobustnessLevel.Low) => -1,

            // M2 Impact dynamics (parachute, etc.)
            (GroundRiskMitigation.M2_ImpactDynamics, RobustnessLevel.Medium) => -1,
            (GroundRiskMitigation.M2_ImpactDynamics, RobustnessLevel.High) => -2,

            _ => 0
        };
    }

    #endregion

    #region ARC (SORA 2.0 and 2.5)

    // ════════════════════════════════════════════════════════════════════════════════════════
    // BACKWARD COMPATIBILITY: ARCInitialInput overload (DEPRECATED)
    // Kept for interface compliance - new code should use ARCEnvironmentInput
    // ════════════════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// [DEPRECATED] Old interface - use ARCEnvironmentInput overload instead
    /// </summary>
    public ARCResult DetermineInitialARC_V2_0(ARCInitialInput input)
    {
        // If explicit ARC provided, honor it
        if (input.ExplicitARC.HasValue)
        {
            return new ARCResult
            {
                ARC = input.ExplicitARC.Value,
                Notes = "Explicit ARC provided by operator/authority map (SORA 2.0)."
            };
        }

        // Otherwise, default to ARC-b with warning to use new interface
        return new ARCResult
        {
            ARC = ARCRating.ARC_b,
            Notes = "WARNING: ARCInitialInput is deprecated! Use ARCEnvironmentInput for authoritative AEC-based calculation. " +
                    "Defaulting to ARC-b (provide environment parameters for accurate determination)."
        };
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // AUTHORITATIVE ARC DETERMINATION - SORA 2.0
    // Reference: JAR-DEL-WG6-D.04 Section 2.4.2 Step #4, Annex C Table 1
    // Decision Tree: 4 steps to determine AEC (Airspace Encounter Category) → Initial ARC
    // ════════════════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Determine Initial ARC per SORA 2.0 Annex C Table 1 (12 AEC categories)
    /// </summary>
    public ARCResult DetermineInitialARC_V2_0(ARCEnvironmentInput input)
    {
        // ════════════════════════════════════════════════════════════════
        // STEP 1: Above FL 600 (18,000m / 59,000 ft)?
        // ════════════════════════════════════════════════════════════════
        if (input.MaxHeightAMSL > 18000)
        {
            return new ARCResult
            {
                ARC = ARCRating.ARC_b,
                AEC = 11,
                DensityRating = 1,
                Notes = "AEC 11: Operation above FL 600 (18,000m) → ARC-b (Density: Very Low). " +
                        "Manned aviation extremely sparse at very high altitudes."
            };
        }

        // ════════════════════════════════════════════════════════════════
        // STEP 2: Atypical/Segregated Airspace (Annex G 3.20)?
        // ════════════════════════════════════════════════════════════════
        if (input.IsAtypicalSegregated)
        {
            return new ARCResult
            {
                ARC = ARCRating.ARC_a,
                AEC = 12,
                DensityRating = 1,
                Notes = "AEC 12: Atypical/Segregated Airspace per SORA Annex G 3.20 → ARC-a (no TMPR required). " +
                        "Requires authority acceptance. Operation in segregated volume with no manned traffic."
            };
        }

        // ════════════════════════════════════════════════════════════════
        // STEP 3: Airport/Heliport Environment?
        // ════════════════════════════════════════════════════════════════
        if (input.LocationType == LocationType.Airport || input.LocationType == LocationType.Heliport)
        {
            // Check airspace class: B/C/D = highest density, E/F/G = medium density
            if (input.AirspaceClass == AirspaceClass.B ||
                input.AirspaceClass == AirspaceClass.C ||
                input.AirspaceClass == AirspaceClass.D)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_d,
                    AEC = 1,
                    DensityRating = 5,
                    Notes = $"AEC 1: Airport/Heliport in Class {input.AirspaceClass} airspace → ARC-d (Density: Very High). " +
                            "Major airport environment with controlled airspace - highest manned traffic density."
                };
            }
            else
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 6,
                    DensityRating = 3,
                    Notes = $"AEC 6: Airport/Heliport in Class {input.AirspaceClass} airspace → ARC-c (Density: Medium). " +
                            "Airport environment in less controlled airspace - medium manned traffic density."
                };
            }
        }

        // ════════════════════════════════════════════════════════════════
        // STEP 4: Above or Below 500 ft AGL?
        // Critical threshold: 500 ft (152m) separates low-level from VLL
        // ════════════════════════════════════════════════════════════════
        if (input.MaxHeightAGL > 152) // 500 ft ≈ 152 meters
        {
            // ─────────────────────────────────────────────────────────────
            // ABOVE 500 ft AGL (Low-Level Operations)
            // ─────────────────────────────────────────────────────────────
            
            // Check Mode-S Veil or TMZ (highest density above 500ft)
            if (input.IsModeS_Veil || input.IsTMZ)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_d,
                    AEC = 2,
                    DensityRating = 5,
                    Notes = "AEC 2: Above 500 ft AGL in Mode-S Veil or TMZ → ARC-d (Density: Very High). " +
                            "Transponder-mandatory zone with high manned traffic density."
                };
            }

            // Check controlled airspace
            if (input.AirspaceControl == AirspaceControl.Controlled)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_d,
                    AEC = 3,
                    DensityRating = 5,
                    Notes = $"AEC 3: Above 500 ft AGL in Controlled Airspace (Class {input.AirspaceClass}) → ARC-d (Density: Very High). " +
                            "Integrated airspace with manned traffic subject to ATC."
                };
            }

            // Uncontrolled airspace - check environment (urban vs rural)
            // Use population density to verify environment type
            bool isUrban = input.Environment == EnvironmentType.Urban ||
                          input.Environment == EnvironmentType.Industrial ||
                          input.PopulationDensity > 1500; // JARUS SORA Table 3 threshold

            if (isUrban)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 4,
                    DensityRating = 3,
                    Notes = $"AEC 4: Above 500 ft AGL, Uncontrolled, Urban ({input.PopulationDensity:F0} people/km²) → ARC-c (Density: Medium). " +
                            "Low-level operation over populated area in uncontrolled airspace."
                };
            }
            else // Rural
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 5,
                    DensityRating = 2,
                    Notes = $"AEC 5: Above 500 ft AGL, Uncontrolled, Rural ({input.PopulationDensity:F0} people/km²) → ARC-c (Density: Low). " +
                            "Low-level operation over sparsely populated area in uncontrolled airspace."
                };
            }
        }
        else
        {
            // ─────────────────────────────────────────────────────────────
            // BELOW 500 ft AGL (Very Low Level - VLL Operations)
            // ─────────────────────────────────────────────────────────────
            
            // Check Mode-S Veil or TMZ (even at VLL)
            if (input.IsModeS_Veil || input.IsTMZ)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 7,
                    DensityRating = 3,
                    Notes = "AEC 7: Below 500 ft AGL in Mode-S Veil or TMZ → ARC-c (Density: Medium). " +
                            "VLL operation in transponder-mandatory zone near major airport."
                };
            }

            // Check controlled airspace (rare at VLL but possible)
            if (input.AirspaceControl == AirspaceControl.Controlled)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 8,
                    DensityRating = 3,
                    Notes = $"AEC 8: Below 500 ft AGL in Controlled Airspace (Class {input.AirspaceClass}) → ARC-c (Density: Medium). " +
                            "VLL operation in controlled airspace - helicopters, airport traffic patterns."
                };
            }

            // Uncontrolled VLL - check environment (urban vs rural)
            // Use population density to verify environment type
            bool isUrban = input.Environment == EnvironmentType.Urban ||
                          input.Environment == EnvironmentType.Industrial ||
                          input.PopulationDensity > 1500; // JARUS SORA Table 3 threshold

            if (isUrban)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 9,
                    DensityRating = 2,
                    Notes = $"AEC 9: Below 500 ft AGL, Uncontrolled, Urban ({input.PopulationDensity:F0} people/km²) → ARC-c (Density: Low). " +
                            "VLL operation over populated area - typical urban BVLOS/VLOS scenario."
                };
            }
            else // Rural
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_b,
                    AEC = 10,
                    DensityRating = 1,
                    Notes = $"AEC 10: Below 500 ft AGL, Uncontrolled, Rural ({input.PopulationDensity:F0} people/km²) → ARC-b (Density: Very Low). " +
                            "VLL operation over sparsely populated area - lowest air risk scenario (excluding segregated)."
                };
            }
        }
    }

    public ARCResult DetermineResidualARC_V2_0(ARCResidualInput input)
    {
        if (input.ExplicitResidualARC.HasValue)
            return new ARCResult { ARC = input.ExplicitResidualARC.Value, Notes = "Residual ARC explicitly set after strategic mitigations (SORA 2.0)." };
        // Conservative Annex C mapping (placeholder):
        // Recognize S1..S4 strategic mitigations; each reduces by 1 level up to a cap by initial ARC.
        // Caps: D→max -2 (to B), C→max -1 (to B), B→max -1 (to A), A→max 0.
        // Annex C: Special case – Atypical/Segregated → ARC-a (subject to authority acceptance)
        if (input.IsAtypicalSegregated == true)
        {
            return new ARCResult
            {
                ARC = ARCRating.ARC_a,
                Notes = "Claimed Atypical/Segregated Airspace per Annex G 3.20 → Residual ARC-a (requires authority acceptance)."
            };
        }

        // Compute density-based reduction per Annex C Table 2 (operator-provided local density rating 1..5)
        int densityReduction = 0;
        string densityNote = string.Empty;
        if (input.LocalDensityRating is int d && d >= 1 && d <= 5)
        {
            switch (input.InitialARC)
            {
                case ARCRating.ARC_d:
                    if (d <= 2) { densityReduction = 2; densityNote = "Local density ≤2 → D→B per Table 2."; }
                    else if (d <= 4) { densityReduction = 1; densityNote = "Local density 3–4 → D→C per Table 2."; }
                    break;
                case ARCRating.ARC_c:
                    if (d == 1) { densityReduction = 1; densityNote = "Local density =1 → C→B per Table 2."; }
                    break;
                default:
                    // No density-based reduction from ARC-b to ARC-a; requires Atypical/Segregated
                    break;
            }
        }

        // Strategic mitigations S1..S4 (conservative cap) – compute potential reduction
        var recognized = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "S1", "S2", "S3", "S4" };
        var applied = (input.StrategicMitigations ?? new List<string>()).Where(m => recognized.Contains(m)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        int maxMitigationReduction = input.InitialARC switch { ARCRating.ARC_d => 2, ARCRating.ARC_c => 1, ARCRating.ARC_b => 1, _ => 0 };
        int mitigationReduction = Math.Min(applied.Count, maxMitigationReduction);

        // Avoid double counting: take the stronger of (density-based) vs (strategic mitigations) reductions
        int chosenReduction = Math.Max(densityReduction, mitigationReduction);
        int finalIndex = Math.Max(0, (int)input.InitialARC - chosenReduction);
        var finalArc = (ARCRating)finalIndex;

        // Guardrail: Do not allow typical operations to reduce below ARC-b via mitigations/density evidence.
        // ARC-a is reserved for atypical/segregated operations (or explicit override).
        // Only apply floor if we actually reduced from a higher level (started above ARC-a)
        if (finalArc == ARCRating.ARC_a && !(input.IsAtypicalSegregated ?? false) && input.InitialARC != ARCRating.ARC_a)
        {
            finalArc = ARCRating.ARC_b;
        }

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(densityNote)) parts.Add(densityNote);
        if (applied.Count > 0) parts.Add($"Recognized mitigations [{string.Join(", ", applied)}], credit -{mitigationReduction} (cap {maxMitigationReduction}).");
        if (parts.Count == 0) parts.Add("No density evidence or recognized mitigations provided (pass-through).");
        parts.Add("No double counting applied (took maximum credit per Annex C intent).");
        return new ARCResult { ARC = finalArc, Notes = string.Join(" ", parts) };
    }

    public ARCResult DetermineInitialARC_V2_5(ARCInitialInput input)
    {
        if (input.ExplicitARC.HasValue)
            return new ARCResult { ARC = input.ExplicitARC.Value, Notes = "Explicit ARC provided by operator/authority map (SORA 2.5)." };

        return new ARCResult { ARC = ARCRating.ARC_b, Notes = "Default ARC-B applied (SORA 2.5) - provide explicit ARC or apply Figure 6 decision tree." };
    }

    public ARCResult DetermineResidualARC_V2_5(ARCResidualInput input)
    {
        if (input.ExplicitResidualARC.HasValue)
            return new ARCResult { ARC = input.ExplicitResidualARC.Value, Notes = "Residual ARC explicitly set after strategic mitigations (SORA 2.5)." };
        if (input.IsAtypicalSegregated == true)
        {
            return new ARCResult
            {
                ARC = ARCRating.ARC_a,
                Notes = "Claimed Atypical/Segregated Airspace per Annex G 3.20 → Residual ARC-a (requires authority acceptance)."
            };
        }

        int densityReduction = 0;
        string densityNote = string.Empty;
        if (input.LocalDensityRating is int d && d >= 1 && d <= 5)
        {
            switch (input.InitialARC)
            {
                case ARCRating.ARC_d:
                    if (d <= 2) { densityReduction = 2; densityNote = "Local density ≤2 → D→B per Table 2."; }
                    else if (d <= 4) { densityReduction = 1; densityNote = "Local density 3–4 → D→C per Table 2."; }
                    break;
                case ARCRating.ARC_c:
                    if (d == 1) { densityReduction = 1; densityNote = "Local density =1 → C→B per Table 2."; }
                    break;
            }
        }

        var recognized = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "S1", "S2", "S3", "S4" };
        var applied = (input.StrategicMitigations ?? new List<string>()).Where(m => recognized.Contains(m)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        int maxMitigationReduction = input.InitialARC switch { ARCRating.ARC_d => 2, ARCRating.ARC_c => 1, ARCRating.ARC_b => 1, _ => 0 };
        int mitigationReduction = Math.Min(applied.Count, maxMitigationReduction);

        int chosenReduction = Math.Max(densityReduction, mitigationReduction);
        int finalIndex = Math.Max(0, (int)input.InitialARC - chosenReduction);
        var finalArc = (ARCRating)finalIndex;

        // Guardrail: Do not allow typical operations to reduce below ARC-b via mitigations/density evidence.
        // ARC-a is reserved for atypical/segregated operations (or explicit override).
        if (finalArc == ARCRating.ARC_a && !(input.IsAtypicalSegregated ?? false) && input.InitialARC != ARCRating.ARC_a)
        {
            finalArc = ARCRating.ARC_b;
        }
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(densityNote)) parts.Add(densityNote);
        if (applied.Count > 0) parts.Add($"Recognized mitigations [{string.Join(", ", applied)}], credit -{mitigationReduction} (cap {maxMitigationReduction}).");
        if (parts.Count == 0) parts.Add("No density evidence or recognized mitigations provided (pass-through).");
        parts.Add("No double counting applied (took maximum credit per Annex C intent).");
        return new ARCResult { ARC = finalArc, Notes = string.Join(" ", parts) };
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // AUTHORITATIVE ARC DETERMINATION - SORA 2.5
    // Reference: JAR-DEL-SRM-SORA-MB-2.5 Section 4.4 Step #4, Figure 6
    // Uses SAME AEC categories as SORA 2.0 Annex C Table 1 (12 categories)
    // ════════════════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Determine Initial ARC per SORA 2.5 Figure 6 (same as SORA 2.0 Annex C Table 1)
    /// </summary>
    public ARCResult DetermineInitialARC_V2_5(ARCEnvironmentInput input)
    {
        // ════════════════════════════════════════════════════════════════
        // SORA 2.5 Figure 6 uses IDENTICAL logic to SORA 2.0 Annex C Table 1
        // AEC categories and decision tree are consistent across versions
        // ════════════════════════════════════════════════════════════════
        
        // STEP 1: Above FL 600?
        if (input.MaxHeightAMSL > 18000)
        {
            return new ARCResult
            {
                ARC = ARCRating.ARC_b,
                AEC = 11,
                DensityRating = 1,
                Notes = "AEC 11: Operation above FL 600 (18,000m) → ARC-b (Density: Very Low). " +
                        "[SORA 2.5 Figure 6]"
            };
        }

        // STEP 2: Atypical/Segregated?
        if (input.IsAtypicalSegregated)
        {
            return new ARCResult
            {
                ARC = ARCRating.ARC_a,
                AEC = 12,
                DensityRating = 1,
                Notes = "AEC 12: Atypical/Segregated Airspace → ARC-a (no TMPR required). " +
                        "[SORA 2.5 Figure 6, Annex G 3.20]"
            };
        }

        // STEP 3: Airport/Heliport?
        if (input.LocationType == LocationType.Airport || input.LocationType == LocationType.Heliport)
        {
            if (input.AirspaceClass == AirspaceClass.B ||
                input.AirspaceClass == AirspaceClass.C ||
                input.AirspaceClass == AirspaceClass.D)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_d,
                    AEC = 1,
                    DensityRating = 5,
                    Notes = $"AEC 1: Airport/Heliport Class {input.AirspaceClass} → ARC-d (Density: Very High). " +
                            "[SORA 2.5 Figure 6]"
                };
            }
            else
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 6,
                    DensityRating = 3,
                    Notes = $"AEC 6: Airport/Heliport Class {input.AirspaceClass} → ARC-c (Density: Medium). " +
                            "[SORA 2.5 Figure 6]"
                };
            }
        }

        // STEP 4: Above/Below 500ft AGL?
        if (input.MaxHeightAGL > 152)
        {
            // Above 500ft AGL
            if (input.IsModeS_Veil || input.IsTMZ)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_d,
                    AEC = 2,
                    DensityRating = 5,
                    Notes = "AEC 2: >500ft Mode-S/TMZ → ARC-d (Density: Very High). [SORA 2.5]"
                };
            }

            if (input.AirspaceControl == AirspaceControl.Controlled)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_d,
                    AEC = 3,
                    DensityRating = 5,
                    Notes = $"AEC 3: >500ft Controlled (Class {input.AirspaceClass}) → ARC-d (Density: Very High). [SORA 2.5]"
                };
            }

            bool isUrban = input.Environment == EnvironmentType.Urban ||
                          input.Environment == EnvironmentType.Industrial ||
                          input.PopulationDensity > 1500;

            if (isUrban)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 4,
                    DensityRating = 3,
                    Notes = $"AEC 4: >500ft Uncontrolled Urban ({input.PopulationDensity:F0} ppl/km²) → ARC-c (Density: Medium). [SORA 2.5]"
                };
            }
            else
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 5,
                    DensityRating = 2,
                    Notes = $"AEC 5: >500ft Uncontrolled Rural ({input.PopulationDensity:F0} ppl/km²) → ARC-c (Density: Low). [SORA 2.5]"
                };
            }
        }
        else
        {
            // Below 500ft AGL (VLL)
            if (input.IsModeS_Veil || input.IsTMZ)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 7,
                    DensityRating = 3,
                    Notes = "AEC 7: <500ft Mode-S/TMZ → ARC-c (Density: Medium). [SORA 2.5]"
                };
            }

            if (input.AirspaceControl == AirspaceControl.Controlled)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 8,
                    DensityRating = 3,
                    Notes = $"AEC 8: <500ft Controlled (Class {input.AirspaceClass}) → ARC-c (Density: Medium). [SORA 2.5]"
                };
            }

            bool isUrban = input.Environment == EnvironmentType.Urban ||
                          input.Environment == EnvironmentType.Industrial ||
                          input.PopulationDensity > 1500;

            if (isUrban)
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_c,
                    AEC = 9,
                    DensityRating = 2,
                    Notes = $"AEC 9: <500ft Uncontrolled Urban ({input.PopulationDensity:F0} ppl/km²) → ARC-c (Density: Low). [SORA 2.5]"
                };
            }
            else
            {
                return new ARCResult
                {
                    ARC = ARCRating.ARC_b,
                    AEC = 10,
                    DensityRating = 1,
                    Notes = $"AEC 10: <500ft Uncontrolled Rural ({input.PopulationDensity:F0} ppl/km²) → ARC-b (Density: Very Low). [SORA 2.5]"
                };
            }
        }
    }

    #endregion

    #region SAIL (SORA 2.0 and 2.5)

    // SORA 2.0 Table 5 mapping (Final GRC vs Residual ARC -> SAIL I..VI)
    private static readonly SAILLevel[,] SAIL_TABLE_2_0 = new SAILLevel[,]
    {
        // Residual ARC: a, b, c, d
        { SAILLevel.I,  SAILLevel.II,  SAILLevel.III, SAILLevel.V  }, // GRC <= 2 (row 0)
        { SAILLevel.I,  SAILLevel.III, SAILLevel.IV,  SAILLevel.V  }, // GRC = 3 (row 1)
        { SAILLevel.III,SAILLevel.III, SAILLevel.IV,  SAILLevel.V  }, // GRC = 4 (row 2) - a set to III
        { SAILLevel.II, SAILLevel.IV,  SAILLevel.IV,  SAILLevel.V  }, // GRC = 5 (row 3) - c set to IV
        { SAILLevel.V,  SAILLevel.V,   SAILLevel.V,   SAILLevel.VI }, // GRC = 6 (row 4) - a/b set to V
        { SAILLevel.VI, SAILLevel.VI,  SAILLevel.VI,  SAILLevel.VI }  // GRC = 7 (row 5)
    };

    public SAILResult DetermineSAIL_V2_0(SAILInput input)
    {
        if (input.FinalGRC < 1 || input.FinalGRC > 7)
            return new SAILResult { IsSupported = false, SAIL = null, Notes = "Final GRC out of SORA 2.0 scope (>7)." };

        int row = input.FinalGRC <= 2 ? 0 : input.FinalGRC - 2; // 3->1,4->2,...,7->5
        int col = (int)input.ResidualARC; // A=0..D=3
        var sail = SAIL_TABLE_2_0[row, col];
        return new SAILResult { IsSupported = true, SAIL = sail, Notes = "SAIL determined per SORA 2.0 Table 5." };
    }

    public SAILResult DetermineSAIL_V2_5(SAILInput input)
    {
        if (input.FinalGRC < 1 || input.FinalGRC > 7)
            return new SAILResult { IsSupported = false, SAIL = null, Notes = "Final GRC out of SORA 2.5 scope (>7)." };

        // SORA 2.5 keeps the same qualitative SAIL matrix structure (to be verified against official table)
        int row = input.FinalGRC <= 2 ? 0 : input.FinalGRC - 2;
        int col = (int)input.ResidualARC;
        var sail = SAIL_TABLE_2_0[row, col];
        return new SAILResult { IsSupported = true, SAIL = sail, Notes = "SAIL determined per SORA 2.5 (structure aligned with 2.0 Table 5)." };
    }

    #endregion

    #region TMPR (SORA 2.0 and 2.5)

    public TMPRResult DetermineTMPR_V2_0(ARCRating residualArc)
    {
        return residualArc switch
        {
            ARCRating.ARC_a => new TMPRResult { Level = TMPRLevel.None, Robustness = RobustnessLevel.None, Notes = "ARC-a: No TMPR required (SORA 2.0 Table 4)." },
            ARCRating.ARC_b => new TMPRResult { Level = TMPRLevel.Medium, Robustness = RobustnessLevel.Low, Notes = "ARC-b: Medium TMPR and Low robustness (SORA 2.0 Table 4)." },
            ARCRating.ARC_c => new TMPRResult { Level = TMPRLevel.High, Robustness = RobustnessLevel.Medium, Notes = "ARC-c: High TMPR and Medium robustness (SORA 2.0 Table 4)." },
            ARCRating.ARC_d => new TMPRResult { Level = TMPRLevel.VeryHigh, Robustness = RobustnessLevel.High, Notes = "ARC-d: VeryHigh TMPR and High robustness (SORA 2.0 Table 4)." },
            _ => new TMPRResult { Level = TMPRLevel.None, Robustness = RobustnessLevel.None, Notes = "Unknown ARC rating." }
        };
    }

    public TMPRResult DetermineTMPR_V2_5(ARCRating residualArc)
    {
        return residualArc switch
        {
            ARCRating.ARC_a => new TMPRResult { Level = TMPRLevel.None, Robustness = RobustnessLevel.None, Notes = "ARC-a: No TMPR required (SORA 2.5 Table 4)." },
            ARCRating.ARC_b => new TMPRResult { Level = TMPRLevel.Medium, Robustness = RobustnessLevel.Low, Notes = "ARC-b: Medium TMPR and Low robustness (SORA 2.5 Table 4)." },
            ARCRating.ARC_c => new TMPRResult { Level = TMPRLevel.High, Robustness = RobustnessLevel.Medium, Notes = "ARC-c: High TMPR and Medium robustness (SORA 2.5 Table 4)." },
            ARCRating.ARC_d => new TMPRResult { Level = TMPRLevel.VeryHigh, Robustness = RobustnessLevel.High, Notes = "ARC-d: VeryHigh TMPR and High robustness (SORA 2.5 Table 4)." },
            _ => new TMPRResult { Level = TMPRLevel.None, Robustness = RobustnessLevel.None, Notes = "Unknown ARC rating." }
        };
    }

    #endregion

    public IEnumerable<StrategicMitigationInfo> GetRecognizedStrategicMitigations()
    {
        // Conservative placeholders aligned across V2.0/V2.5 until Annex C table mapping is wired
        yield return new StrategicMitigationInfo("S1", "Συντονισμός/συνεργασία με ATS/ANSP ή ισοδύναμη παροχή υπηρεσιών για τακτική επίγνωση/διαχείριση κινδύνου εναέριας κυκλοφορίας.");
        yield return new StrategicMitigationInfo("S2", "Γεω-περιορισμός/Geo-fencing & οριοθέτηση περιοχής επιχειρήσεων (στρατηγικός διαχωρισμός).");
        yield return new StrategicMitigationInfo("S3", "Τακτική αποσυμφόρηση/Deconfliction μέσω διαδικασιών/υπηρεσιών (π.χ. ενημέρωση κυκλοφορίας, διαδικασίες R/T).");
        yield return new StrategicMitigationInfo("S4", "Εναλλακτικές διαδρομές/contingency & διαδικασίες εκτροπής για μείωση στυνοικίας με επανδρωμένη κίνηση.");
    }
}
