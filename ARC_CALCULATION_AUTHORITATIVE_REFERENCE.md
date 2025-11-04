# ARC CALCULATION - AUTHORITATIVE REFERENCE
**CRITICAL BUG FIX DOCUMENTATION**

## 🚨 PROBLEM IDENTIFIED

**Current Backend Behavior:**
- `DetermineInitialARC_V2_0()` always returns **ARC-b** (default fallback)
- `DetermineInitialARC_V2_5()` always returns **ARC-b** (default fallback)
- This causes **SAIL** to always calculate as **SAIL I** regardless of actual operation risk!

**Impact:**
- ALL SORA assessments producing incorrect results
- Urban BVLOS operations incorrectly categorized as SAIL I instead of SAIL IV-VI
- Controlled airspace operations showing same risk as rural VLOS
- **CRITICAL SAFETY ISSUE** - operations approved with insufficient mitigations

---

## 📚 AUTHORITATIVE SOURCES

### SORA 2.0 - Annex C Table 1 (Page 12)

```
┌─────────────────────────────────────────────────────┬─────────┬───────┬─────────┐
│ Operational Environment                             │ AEC     │ Dens. │ Init.   │
│                                                     │         │ Rating│ ARC     │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ AIRPORT/HELIPORT ENVIRONMENT                        │         │       │         │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ Airport/Heliport in Class B, C or D                 │ AEC 1   │   5   │ ARC-d   │
│ Airport/Heliport in Class E, F or G                 │ AEC 6   │   3   │ ARC-c   │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ OPERATIONS ABOVE 500 FT AGL (but < FL 600)          │         │       │         │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ Mode-S Veil or TMZ                                  │ AEC 2   │   5   │ ARC-d   │
│ Controlled airspace                                 │ AEC 3   │   5   │ ARC-d   │
│ Uncontrolled airspace over Urban Area              │ AEC 4   │   3   │ ARC-c   │
│ Uncontrolled airspace over Rural Area              │ AEC 5   │   2   │ ARC-c   │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ OPERATIONS BELOW 500 FT AGL                         │         │       │         │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ Mode-S Veil or TMZ                                  │ AEC 7   │   3   │ ARC-c   │
│ Controlled airspace                                 │ AEC 8   │   3   │ ARC-c   │
│ Uncontrolled airspace over Urban Area              │ AEC 9   │   2   │ ARC-c   │
│ Uncontrolled airspace over Rural Area              │ AEC 10  │   1   │ ARC-b   │
├─────────────────────────────────────────────────────┼─────────┼───────┼─────────┤
│ OPERATIONS ABOVE FL 600                             │ AEC 11  │   1   │ ARC-b   │
│ Atypical/Segregated Airspace                        │ AEC 12  │   1   │ ARC-a   │
└─────────────────────────────────────────────────────┴─────────┴───────┴─────────┘
```

### DECISION TREE LOGIC

**Step 1: Altitude Check**
```
Is operation ABOVE FL 600 (18,000m)?
├─ YES → ARC-b (AEC 11)
└─ NO  → Continue to Step 2
```

**Step 2: Atypical/Segregated Check**
```
Is airspace Atypical/Segregated per Annex G 3.20?
├─ YES → ARC-a (AEC 12) [Requires authority acceptance]
└─ NO  → Continue to Step 3
```

**Step 3: Airport/Heliport Environment Check**
```
Is operation in Airport/Heliport environment?
├─ YES → Check Airspace Class
│   ├─ Class B, C, or D → ARC-d (AEC 1) [Density 5]
│   └─ Class E, F, or G → ARC-c (AEC 6) [Density 3]
└─ NO  → Continue to Step 4
```

**Step 4: Altitude AGL Check**
```
Is operation ABOVE 500 ft AGL (but < FL 600)?
├─ YES → Check Airspace Type & Environment
│   ├─ Mode-S Veil or TMZ        → ARC-d (AEC 2) [Density 5]
│   ├─ Controlled airspace       → ARC-d (AEC 3) [Density 5]
│   ├─ Uncontrolled + Urban      → ARC-c (AEC 4) [Density 3]
│   └─ Uncontrolled + Rural      → ARC-c (AEC 5) [Density 2]
└─ NO  → BELOW 500 ft AGL → Check Airspace Type & Environment
    ├─ Mode-S Veil or TMZ        → ARC-c (AEC 7) [Density 3]
    ├─ Controlled airspace       → ARC-c (AEC 8) [Density 3]
    ├─ Uncontrolled + Urban      → ARC-c (AEC 9) [Density 2]
    └─ Uncontrolled + Rural      → ARC-b (AEC 10) [Density 1]
```

---

## 🔍 KEY FINDINGS

### 1. **Airspace Classification** (ICAO)
- **Class A**: IFR only, controlled
- **Class B**: IFR & VFR, controlled, ATC clearance required
- **Class C**: IFR & VFR, controlled, two-way radio required
- **Class D**: IFR & VFR, controlled, two-way radio required
- **Class E**: Controlled airspace, less restrictive
- **Class F**: Advisory service (rare)
- **Class G**: Uncontrolled airspace

### 2. **Environment Classification**
- **Urban**: Population density > 1,500 people/km² (JARUS SORA Table 3)
- **Suburban**: 500-1,500 people/km²
- **Rural/Sparsely Populated**: < 500 people/km²

### 3. **Altitude Thresholds**
- **VLL (Very Low Level)**: < 500 ft AGL
- **Low Level**: 500 ft - FL 600 (18,000 ft)
- **High Level**: > FL 600

### 4. **Special Zones**
- **Mode-S Veil**: Airspace requiring Mode-S transponder (typically around major airports)
- **TMZ (Transponder Mandatory Zone)**: Airspace requiring transponder
- **Airport/Heliport Environment**: Within airport traffic pattern or vicinity

---

## ✅ CORRECT IMPLEMENTATION REQUIRED

### Input Parameters Needed:
```csharp
public class ARCEnvironmentInput
{
    // Altitude
    public int MaxHeightAGL { get; set; }           // Meters above ground level
    public int MaxHeightAMSL { get; set; }          // Meters above mean sea level (for FL check)
    
    // Airspace
    public AirspaceClass AirspaceClass { get; set; } // A, B, C, D, E, F, G
    public bool IsControlled { get; set; }          // Controlled vs Uncontrolled
    public bool IsModeS_Veil { get; set; }          // Mode-S Veil zone
    public bool IsTMZ { get; set; }                 // Transponder Mandatory Zone
    
    // Environment
    public EnvironmentType Environment { get; set; } // Urban, Suburban, Rural
    public double PopulationDensity { get; set; }   // People per km²
    
    // Location
    public LocationType LocationType { get; set; }   // Airport, NonAirport
    
    // Special
    public bool IsAtypicalSegregated { get; set; }  // Per Annex G 3.20
}

public enum AirspaceClass { A, B, C, D, E, F, G }
public enum EnvironmentType { Urban, Suburban, Rural }
public enum LocationType { Airport, Heliport, NonAirport }
```

### Algorithm Logic:
```csharp
public ARCResult DetermineInitialARC_V2_0(ARCEnvironmentInput env)
{
    // Step 1: Above FL 600?
    if (env.MaxHeightAMSL > 18000) // FL 600 ≈ 18,000m
    {
        return new ARCResult 
        { 
            ARC = ARCRating.ARC_b, 
            AEC = 11,
            DensityRating = 1,
            Notes = "AEC 11: Operations above FL 600 → ARC-b (Density 1)"
        };
    }
    
    // Step 2: Atypical/Segregated?
    if (env.IsAtypicalSegregated)
    {
        return new ARCResult 
        { 
            ARC = ARCRating.ARC_a, 
            AEC = 12,
            DensityRating = 1,
            Notes = "AEC 12: Atypical/Segregated Airspace (Annex G 3.20) → ARC-a (Requires authority acceptance)"
        };
    }
    
    // Step 3: Airport/Heliport environment?
    if (env.LocationType == LocationType.Airport || env.LocationType == LocationType.Heliport)
    {
        if (env.AirspaceClass == AirspaceClass.B || 
            env.AirspaceClass == AirspaceClass.C || 
            env.AirspaceClass == AirspaceClass.D)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_d, 
                AEC = 1,
                DensityRating = 5,
                Notes = "AEC 1: Airport/Heliport in Class B/C/D → ARC-d (Density 5)"
            };
        }
        else // Class E, F, or G
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_c, 
                AEC = 6,
                DensityRating = 3,
                Notes = "AEC 6: Airport/Heliport in Class E/F/G → ARC-c (Density 3)"
            };
        }
    }
    
    // Step 4: Above or below 500 ft AGL?
    if (env.MaxHeightAGL > 152) // 500 ft ≈ 152m
    {
        // ABOVE 500 ft AGL (but < FL 600)
        if (env.IsModeS_Veil || env.IsTMZ)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_d, 
                AEC = 2,
                DensityRating = 5,
                Notes = "AEC 2: >500ft AGL in Mode-S Veil/TMZ → ARC-d (Density 5)"
            };
        }
        else if (env.IsControlled)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_d, 
                AEC = 3,
                DensityRating = 5,
                Notes = "AEC 3: >500ft AGL in controlled airspace → ARC-d (Density 5)"
            };
        }
        else if (env.Environment == EnvironmentType.Urban || env.PopulationDensity > 1500)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_c, 
                AEC = 4,
                DensityRating = 3,
                Notes = "AEC 4: >500ft AGL uncontrolled over Urban → ARC-c (Density 3)"
            };
        }
        else // Rural
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_c, 
                AEC = 5,
                DensityRating = 2,
                Notes = "AEC 5: >500ft AGL uncontrolled over Rural → ARC-c (Density 2)"
            };
        }
    }
    else
    {
        // BELOW 500 ft AGL
        if (env.IsModeS_Veil || env.IsTMZ)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_c, 
                AEC = 7,
                DensityRating = 3,
                Notes = "AEC 7: <500ft AGL in Mode-S Veil/TMZ → ARC-c (Density 3)"
            };
        }
        else if (env.IsControlled)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_c, 
                AEC = 8,
                DensityRating = 3,
                Notes = "AEC 8: <500ft AGL in controlled airspace → ARC-c (Density 3)"
            };
        }
        else if (env.Environment == EnvironmentType.Urban || env.PopulationDensity > 1500)
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_c, 
                AEC = 9,
                DensityRating = 2,
                Notes = "AEC 9: <500ft AGL uncontrolled over Urban → ARC-c (Density 2)"
            };
        }
        else // Rural
        {
            return new ARCResult 
            { 
                ARC = ARCRating.ARC_b, 
                AEC = 10,
                DensityRating = 1,
                Notes = "AEC 10: <500ft AGL uncontrolled over Rural → ARC-b (Density 1) [Reference environment]"
            };
        }
    }
}
```

---

## 📊 SORA 2.5 DIFFERENCES

**SORA 2.5** uses a **simplified decision tree** (Figure 6) but follows **same AEC principles**:

### SORA 2.5 Figure 6 Decision Tree
```
┌─────────────────────────────────────────────────────────────────┐
│ Is operation in ATYPICAL/SEGREGATED airspace?                  │
│ (Per Annex C criteria)                                          │
├─────────────────────────────────────────────────────────────────┤
│ YES → ARC-a                                                     │
│ NO  → Continue                                                  │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Determine encounter rate based on:                             │
│ - Altitude (>500ft AGL vs <500ft AGL vs >FL600)                │
│ - Airspace control (Controlled vs Uncontrolled)                │
│ - Environment (Urban vs Rural)                                 │
│ - Airport proximity                                            │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Assign Initial ARC:                                            │
│ - High encounter rate    → ARC-d                               │
│ - Medium encounter rate  → ARC-c                               │
│ - Low encounter rate     → ARC-b                               │
│ - Negligible (segregated)→ ARC-a                               │
└─────────────────────────────────────────────────────────────────┘
```

**Key Point:** SORA 2.5 has **same AEC table** as 2.0! Implementation logic is **IDENTICAL**!

---

## 🎯 EXPECTED TEST CASES

### Test Case 1: Urban BVLOS (should be SAIL IV-V, NOT SAIL I)
```
Input:
  - MaxHeightAGL: 100m (< 500 ft)
  - AirspaceClass: G (Uncontrolled)
  - Environment: Urban
  - PopulationDensity: 2500 people/km²
  - LocationType: NonAirport

Expected:
  - Initial ARC: ARC-c (AEC 9)
  - Density: 2
  - Notes: "AEC 9: <500ft AGL uncontrolled over Urban → ARC-c (Density 2)"
```

### Test Case 2: Rural VLOS (correct as SAIL I)
```
Input:
  - MaxHeightAGL: 120m (< 500 ft)
  - AirspaceClass: G (Uncontrolled)
  - Environment: Rural
  - PopulationDensity: 200 people/km²
  - LocationType: NonAirport

Expected:
  - Initial ARC: ARC-b (AEC 10)
  - Density: 1
  - Notes: "AEC 10: <500ft AGL uncontrolled over Rural → ARC-b (Density 1)"
```

### Test Case 3: Airport Environment Class C (should be SAIL VI)
```
Input:
  - MaxHeightAGL: 50m
  - AirspaceClass: C (Controlled)
  - Environment: Urban
  - LocationType: Airport

Expected:
  - Initial ARC: ARC-d (AEC 1)
  - Density: 5
  - Notes: "AEC 1: Airport/Heliport in Class B/C/D → ARC-d (Density 5)"
```

---

## 🚀 IMPLEMENTATION PRIORITY

**Priority 1 - CRITICAL:**
1. Fix `DetermineInitialARC_V2_0()` with decision tree logic
2. Fix `DetermineInitialARC_V2_5()` (same logic)
3. Add `ARCEnvironmentInput` class with all parameters
4. Update `ARCResult` to include `AEC` and `DensityRating`

**Priority 2 - HIGH:**
5. Update Frontend mission.html to collect environment data
6. Update API controllers to pass environment parameters
7. Fix SAIL calculation to use correct ARC

**Priority 3 - MEDIUM:**
8. Add ARC reduction logic (Annex C Table 2) for strategic mitigations
9. Add residual ARC calculation
10. Update all test cases

---

## 📝 REFERENCES

- **SORA 2.0**: JAR-DEL-WG6-D.04, Section 2.4.2, Figure 4, Page 22-23
- **SORA 2.0 Annex C**: Strategic Mitigation, Table 1 (Page 12), Table 2 (Page 14)
- **SORA 2.5 Main Body**: JAR-DEL-SRM-SORA-MB-2.5, Step #4 (Page 16)
- **SORA 2.5 Annex C**: (Will be updated when available - uses same AEC table)

---

**Status:** READY FOR IMPLEMENTATION  
**Next Step:** Implement corrected ARC calculation logic in GRCCalculationService.cs  
**Testing:** All mission.html assessments must be re-validated after fix
