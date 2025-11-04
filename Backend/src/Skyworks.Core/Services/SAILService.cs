using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services;

/// <summary>
/// SAIL Determination Service - JARUS SORA 2.0 Step #7
/// Implements Table 5: SAIL determination based on Final GRC and Residual ARC
/// </summary>
public interface ISAILService
{
    /// <summary>
    /// Determine SAIL level from Final GRC and Residual ARC (JARUS SORA 2.0 Table 5)
    /// </summary>
    SAILResult DetermineSAIL(SAILInput input);
    
    /// <summary>
    /// Determine composite SAIL for multi-segment missions
    /// </summary>
    CompositeSAILResult DetermineCompositeSAIL(List<MissionSegment> segments);
    
    /// <summary>
    /// Get required OSO count for a given SAIL level
    /// </summary>
    int GetRequiredOSOCount(SAILLevel sail);
    
    /// <summary>
    /// Get OSO robustness requirements for a given SAIL level
    /// </summary>
    Dictionary<string, string> GetOSORobustnessRequirements(SAILLevel sail);
}

/// <summary>
/// SAIL Determination Service Implementation
/// Based on JARUS SORA 2.0 Section 2.5.1, Table 5
/// </summary>
public class SAILService : ISAILService
{
    // JARUS SORA 2.0 Table 5 / SORA 2.5 Table 7: SAIL Determination Matrix
    // Rows: Final GRC (≤2, 3, 4, 5, 6, 7, >7)
    // Columns: Residual ARC (ARC-a, ARC-b, ARC-c, ARC-d)
    // Reference: JAR-DEL-WG6-D.04 Table 5, JAR-DEL-SRM-SORA-MB-2.5 Table 7
    private static readonly Dictionary<(int grc, ARCRating arc), SAILLevel> _sailMatrix = new()
    {
        // GRC ≤ 2 (rows 1 and 2 map the same)
        {(1, ARCRating.ARC_a), SAILLevel.I},
        {(1, ARCRating.ARC_b), SAILLevel.II},
        {(1, ARCRating.ARC_c), SAILLevel.III},
        {(1, ARCRating.ARC_d), SAILLevel.V},
        {(2, ARCRating.ARC_a), SAILLevel.I},
        {(2, ARCRating.ARC_b), SAILLevel.II},
        {(2, ARCRating.ARC_c), SAILLevel.III},
        {(2, ARCRating.ARC_d), SAILLevel.V},

        // GRC = 3
        {(3, ARCRating.ARC_a), SAILLevel.I},
        {(3, ARCRating.ARC_b), SAILLevel.III},
        {(3, ARCRating.ARC_c), SAILLevel.IV},
        {(3, ARCRating.ARC_d), SAILLevel.V},

        // GRC = 4
        {(4, ARCRating.ARC_a), SAILLevel.III},
        {(4, ARCRating.ARC_b), SAILLevel.III},
        {(4, ARCRating.ARC_c), SAILLevel.IV},
        {(4, ARCRating.ARC_d), SAILLevel.V},

        // GRC = 5
        {(5, ARCRating.ARC_a), SAILLevel.II},
        {(5, ARCRating.ARC_b), SAILLevel.IV},
        {(5, ARCRating.ARC_c), SAILLevel.IV},
        {(5, ARCRating.ARC_d), SAILLevel.V},

        // GRC = 6
        {(6, ARCRating.ARC_a), SAILLevel.V},
        {(6, ARCRating.ARC_b), SAILLevel.V},
        {(6, ARCRating.ARC_c), SAILLevel.V},
        {(6, ARCRating.ARC_d), SAILLevel.VI},

        // GRC = 7
        {(7, ARCRating.ARC_a), SAILLevel.VI},
        {(7, ARCRating.ARC_b), SAILLevel.VI},
        {(7, ARCRating.ARC_c), SAILLevel.VI},
        {(7, ARCRating.ARC_d), SAILLevel.VI}
    };
    
    /// <summary>
    /// Determine SAIL level from Final GRC and Residual ARC
    /// </summary>
    public SAILResult DetermineSAIL(SAILInput input)
    {
        // Validation
        if (input.FinalGRC < 1)
        {
            return new SAILResult
            {
                IsSupported = false,
                SAIL = null,
                Notes = "ERROR: Final GRC cannot be less than 1 (SORA 2.0 specifies minimum GRC = 1)"
            };
        }
        
        if (input.FinalGRC > 7)
        {
            return new SAILResult
            {
                IsSupported = false,
                SAIL = null,
                Notes = $"CATEGORY C OPERATION: Final GRC = {input.FinalGRC} exceeds SPECIFIC category limit (max GRC = 7). CERTIFIED category required (type certificate needed). SORA does not support GRC > 7."
            };
        }
        
        // Lookup SAIL from matrix
        var key = (input.FinalGRC, input.ResidualARC);
        
        if (!_sailMatrix.TryGetValue(key, out var sailLevel))
        {
            return new SAILResult
            {
                IsSupported = false,
                SAIL = null,
                Notes = $"ERROR: Invalid combination - GRC={input.FinalGRC}, ARC={input.ResidualARC}"
            };
        }
        
        // Build explanation
        var explanation = BuildSAILExplanation(input.FinalGRC, input.ResidualARC, sailLevel);
        var warnings = BuildSAILWarnings(input.FinalGRC, input.ResidualARC, sailLevel);
        
        return new SAILResult
        {
            IsSupported = true,
            SAIL = sailLevel,
            Notes = $"{GetSAILRomanNumeral(sailLevel)} - {explanation}\n" +
                   $"Required OSOs: {GetRequiredOSOCount(sailLevel)}\n" +
                   $"Regulatory Category: {GetRegulatoryCategory(sailLevel)}\n" +
                   $"DCA Cyprus: {GetDCACyprusAcceptance(sailLevel)}\n" +
                   (warnings.Count > 0 ? $"\n⚠️ WARNINGS:\n{string.Join("\n", warnings)}" : "")
        };
    }
    
    /// <summary>
    /// Determine composite SAIL for multi-segment missions
    /// SORA 2.0 Section 2.4.2(g): "The applicant needs to do an air risk assessment for the entire range 
    /// of the Operational Volume. An example scenario of operations in multiple airspace environments 
    /// is provided at the end of Annex C."
    /// Rule: Use the HIGHEST SAIL from all segments
    /// </summary>
    public CompositeSAILResult DetermineCompositeSAIL(List<MissionSegment> segments)
    {
        var segmentResults = new List<SegmentSAILResult>();
        SAILLevel maxSAIL = SAILLevel.I;
        string? criticalSegment = null;
        int totalDuration = 0;
        var exposureBySAIL = new Dictionary<SAILLevel, int>();
        
        foreach (var segment in segments)
        {
            // Parse ARC with validation; default to ARC-a if unknown and record note
            var parsedArc = TryParseARCRating(segment.ResidualARC, out var arcParseNote);

            var input = new SAILInput
            {
                FinalGRC = segment.FinalGRC,
                ResidualARC = parsedArc
            };
            
            var result = DetermineSAIL(input);
            
            if (result.IsSupported && result.SAIL.HasValue)
            {
                var segmentSAIL = result.SAIL.Value;
                
                segmentResults.Add(new SegmentSAILResult
                {
                    SegmentName = segment.SegmentName,
                    SAILLevel = segmentSAIL,
                    FinalGRC = segment.FinalGRC,
                    ResidualARC = segment.ResidualARC,
                    DurationMinutes = segment.DurationMinutes
                });
                
                // Track highest SAIL
                if ((int)segmentSAIL > (int)maxSAIL)
                {
                    maxSAIL = segmentSAIL;
                    criticalSegment = segment.SegmentName;
                }
                
                totalDuration += segment.DurationMinutes;

                // Track exposure per SAIL (in minutes)
                if (!exposureBySAIL.ContainsKey(segmentSAIL))
                    exposureBySAIL[segmentSAIL] = 0;
                exposureBySAIL[segmentSAIL] += segment.DurationMinutes;

                // If ARC string was invalid, append note into per-segment pseudo-log (added later in explanation)
                if (!string.IsNullOrEmpty(arcParseNote))
                {
                    // Store as zero-duration entry with note in SegmentName to surface in explanation
                    segmentResults.Add(new SegmentSAILResult
                    {
                        SegmentName = $"NOTE: {arcParseNote}",
                        SAILLevel = segmentSAIL,
                        FinalGRC = segment.FinalGRC,
                        ResidualARC = segment.ResidualARC,
                        DurationMinutes = 0
                    });
                }
            }
        }
        
        var explanation = $"Composite SAIL determination for multi-segment mission:\n" +
                         $"- Total segments: {segments.Count}\n" +
                         $"- Critical segment: '{criticalSegment}' → {GetSAILRomanNumeral(maxSAIL)}\n" +
                         $"- Rule: COMPOSITE SAIL = HIGHEST SEGMENT SAIL (JARUS SORA 2.0 Section 2.4.2(g))\n" +
                         $"- Rationale: The operation must meet the safety requirements of the most demanding segment\n" +
                         $"- Total duration: {totalDuration} minutes\n" +
                         BuildExposureSummary(exposureBySAIL, totalDuration) + "\n" +
                         $"Segment breakdown:\n" +
                         string.Join("\n", segmentResults.Select(s => 
                             $"  • {s.SegmentName}: GRC={s.FinalGRC}, ARC={s.ResidualARC} → {GetSAILRomanNumeral(s.SAILLevel)} ({s.DurationMinutes} min)"));
        
        return new CompositeSAILResult
        {
            CompositeSAIL = maxSAIL,
            SegmentResults = segmentResults,
            Explanation = explanation,
            CriticalSegment = criticalSegment,
            TotalDurationMinutes = totalDuration,
            JARUSReference = "JARUS SORA 2.0 Section 2.4.2(g), Annex C Example Scenario"
        };
    }
    
    /// <summary>
    /// Get required OSO count for SAIL level
    /// Based on JARUS SORA 2.0 Table 6 (Operational Safety Objectives)
    /// </summary>
    public int GetRequiredOSOCount(SAILLevel sail)
    {
        return sail switch
        {
            SAILLevel.I => 8,    // Minimal OSOs (Optional/Low robustness)
            SAILLevel.II => 12,   // Light OSOs (Low/Medium robustness)
            SAILLevel.III => 16,  // Moderate OSOs (Medium robustness)
            SAILLevel.IV => 18,   // Enhanced OSOs (Medium/High robustness)
            SAILLevel.V => 20,    // Robust OSOs (High robustness)
            SAILLevel.VI => 24,   // Maximum OSOs (High robustness, all objectives)
            _ => 0
        };
    }
    
    /// <summary>
    /// Get OSO robustness requirements for SAIL level
    /// Returns dictionary of OSO# → Robustness Level (O=Optional, L=Low, M=Medium, H=High)
    /// Based on JARUS SORA 2.0 Table 6
    /// </summary>
    public Dictionary<string, string> GetOSORobustnessRequirements(SAILLevel sail)
    {
        // JARUS SORA 2.0 Table 6 - OSO Requirements by SAIL
        // Format: OSO# → Robustness (O/L/M/H)
        return sail switch
        {
            SAILLevel.I => new Dictionary<string, string>
            {
                {"OSO#01", "O"}, {"OSO#02", "O"}, {"OSO#03", "L"}, {"OSO#04", "O"},
                {"OSO#05", "O"}, {"OSO#06", "O"}, {"OSO#07", "L"}, {"OSO#08", "L"}
            },
            SAILLevel.II => new Dictionary<string, string>
            {
                {"OSO#01", "L"}, {"OSO#02", "O"}, {"OSO#03", "L"}, {"OSO#04", "O"},
                {"OSO#05", "O"}, {"OSO#06", "L"}, {"OSO#07", "L"}, {"OSO#08", "L"},
                {"OSO#09", "L"}, {"OSO#10", "L"}, {"OSO#11", "L"}, {"OSO#12", "L"}
            },
            SAILLevel.III => new Dictionary<string, string>
            {
                {"OSO#01", "M"}, {"OSO#02", "L"}, {"OSO#03", "M"}, {"OSO#04", "O"},
                {"OSO#05", "L"}, {"OSO#06", "L"}, {"OSO#07", "M"}, {"OSO#08", "M"},
                {"OSO#09", "M"}, {"OSO#10", "M"}, {"OSO#11", "M"}, {"OSO#12", "M"},
                {"OSO#13", "M"}, {"OSO#14", "M"}, {"OSO#15", "M"}, {"OSO#16", "M"}
            },
            SAILLevel.IV => new Dictionary<string, string>
            {
                {"OSO#01", "H"}, {"OSO#02", "M"}, {"OSO#03", "M"}, {"OSO#04", "L"},
                {"OSO#05", "M"}, {"OSO#06", "M"}, {"OSO#07", "M"}, {"OSO#08", "M"},
                {"OSO#09", "H"}, {"OSO#10", "H"}, {"OSO#11", "H"}, {"OSO#12", "H"},
                {"OSO#13", "H"}, {"OSO#14", "H"}, {"OSO#15", "H"}, {"OSO#16", "H"},
                {"OSO#17", "H"}, {"OSO#18", "M"}
            },
            SAILLevel.V => new Dictionary<string, string>
            {
                {"OSO#01", "H"}, {"OSO#02", "H"}, {"OSO#03", "H"}, {"OSO#04", "M"},
                {"OSO#05", "H"}, {"OSO#06", "H"}, {"OSO#07", "H"}, {"OSO#08", "H"},
                {"OSO#09", "H"}, {"OSO#10", "H"}, {"OSO#11", "H"}, {"OSO#12", "H"},
                {"OSO#13", "H"}, {"OSO#14", "H"}, {"OSO#15", "H"}, {"OSO#16", "H"},
                {"OSO#17", "H"}, {"OSO#18", "H"}, {"OSO#19", "H"}, {"OSO#20", "H"}
            },
            SAILLevel.VI => new Dictionary<string, string>
            {
                {"OSO#01", "H"}, {"OSO#02", "H"}, {"OSO#03", "H"}, {"OSO#04", "H"},
                {"OSO#05", "H"}, {"OSO#06", "H"}, {"OSO#07", "H"}, {"OSO#08", "H"},
                {"OSO#09", "H"}, {"OSO#10", "H"}, {"OSO#11", "H"}, {"OSO#12", "H"},
                {"OSO#13", "H"}, {"OSO#14", "H"}, {"OSO#15", "H"}, {"OSO#16", "H"},
                {"OSO#17", "H"}, {"OSO#18", "H"}, {"OSO#19", "H"}, {"OSO#20", "H"},
                {"OSO#21", "H"}, {"OSO#22", "H"}, {"OSO#23", "H"}, {"OSO#24", "H"}
            },
            _ => new Dictionary<string, string>()
        };
    }
    
    // Helper methods
    
    private string BuildSAILExplanation(int grc, ARCRating arc, SAILLevel sail)
    {
        var grcDesc = grc <= 2 ? "Low ground risk" : grc == 3 ? "Low-Medium ground risk" : grc == 4 ? "Medium ground risk" :
                      grc == 5 ? "Medium-High ground risk" : grc == 6 ? "High ground risk" : "Very high ground risk";
        
        var arcDesc = arc switch
        {
            ARCRating.ARC_a => "Extremely low air risk (atypical/segregated airspace)",
            ARCRating.ARC_b => "Low air risk",
            ARCRating.ARC_c => "Medium air risk (controlled airspace)",
            ARCRating.ARC_d => "High air risk (controlled airspace + airport proximity)",
            _ => "Unknown air risk"
        };
        
        return $"GRC={grc} ({grcDesc}) + ARC-{arc.ToString().ToLower().Replace("arc", "")} ({arcDesc}) → {GetSAILRomanNumeral(sail)}";
    }
    
    private List<string> BuildSAILWarnings(int grc, ARCRating arc, SAILLevel sail)
    {
        var warnings = new List<string>();
        
        if (grc >= 6)
        {
            warnings.Add($"⚠️ High ground risk (GRC={grc}) - Enhanced mitigations critical");
        }
        
        if (arc == ARCRating.ARC_d)
        {
            warnings.Add("⚠️ High air risk (ARC-d) - TMPR High robustness required");
        }
        
        if (sail >= SAILLevel.V)
        {
            warnings.Add($"⚠️ {GetSAILRomanNumeral(sail)} requires extensive documentation and may need Light UAS Operator Certificate (LUC)");
        }
        
        if (sail == SAILLevel.VI)
        {
            warnings.Add("⚠️ SAIL VI is near-CERTIFIED level - exceptional authorization needed, consider CERTIFIED category");
        }
        
        if (grc >= 5 && arc >= ARCRating.ARC_c)
        {
            warnings.Add("⚠️ High combined risk (GRC≥5 + ARC≥c) - Detailed SORA submission required");
        }
        
        return warnings;
    }
    
    private string GetSAILRomanNumeral(SAILLevel sail)
    {
        return sail switch
        {
            SAILLevel.I => "SAIL I",
            SAILLevel.II => "SAIL II",
            SAILLevel.III => "SAIL III",
            SAILLevel.IV => "SAIL IV",
            SAILLevel.V => "SAIL V",
            SAILLevel.VI => "SAIL VI",
            _ => "UNKNOWN"
        };
    }
    
    private string GetRegulatoryCategory(SAILLevel sail)
    {
        return sail switch
        {
            SAILLevel.I => "OPEN Category (with limitations) or Light-Touch SPECIFIC",
            SAILLevel.II or SAILLevel.III => "SPECIFIC Category (Operational Authorization)",
            SAILLevel.IV or SAILLevel.V => "SPECIFIC Category (Enhanced Authorization)",
            SAILLevel.VI => "SPECIFIC Category (Exceptional) or CERTIFIED Category",
            _ => "Unknown"
        };
    }
    
    private string GetDCACyprusAcceptance(SAILLevel sail)
    {
        return sail switch
        {
            SAILLevel.I => "✅ Likely accepted (low-risk operations)",
            SAILLevel.II => "✅ Light-Touch authorization",
            SAILLevel.III => "⚠️ Standard authorization (SORA submission required)",
            SAILLevel.IV => "⚠️ Enhanced authorization (detailed risk assessment)",
            SAILLevel.V => "⚠️ High scrutiny (LUC may be required)",
            SAILLevel.VI => "❌ Very unlikely (near-CERTIFIED level)",
            _ => "Unknown"
        };
    }
    
    private ARCRating TryParseARCRating(string arc, out string? note)
    {
        note = null;
        var token = arc?.Trim().ToUpperInvariant() ?? string.Empty;
        switch (token)
        {
            case "ARC-A":
            case "A":
            case "ARCA":
                return ARCRating.ARC_a;
            case "ARC-B":
            case "B":
            case "ARCB":
                return ARCRating.ARC_b;
            case "ARC-C":
            case "C":
            case "ARCC":
                return ARCRating.ARC_c;
            case "ARC-D":
            case "D":
            case "ARCD":
                return ARCRating.ARC_d;
            default:
                note = $"Unrecognized ARC '{arc}'. Defaulted to ARC-a for safety (validation gate).";
                return ARCRating.ARC_a;
        }
    }

    private string BuildExposureSummary(Dictionary<SAILLevel, int> exposureBySail, int totalMinutes)
    {
        if (totalMinutes <= 0 || exposureBySail.Count == 0) return string.Empty;

        var parts = exposureBySail
            .OrderBy(kv => (int)kv.Key)
            .Select(kv =>
            {
                var percent = (int)Math.Round(100.0 * kv.Value / totalMinutes);
                return $"{GetSAILRomanNumeral(kv.Key)}: {kv.Value} min ({percent}%)";
            });

        return "- Exposure by SAIL: " + string.Join(", ", parts);
    }
}

/// <summary>
/// Mission segment for composite SAIL calculation
/// </summary>
public class MissionSegment
{
    public string SegmentName { get; set; } = string.Empty;
    public int FinalGRC { get; set; }
    public string ResidualARC { get; set; } = "ARC-a";
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Composite SAIL result
/// </summary>
public class CompositeSAILResult
{
    public SAILLevel CompositeSAIL { get; set; }
    public List<SegmentSAILResult> SegmentResults { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public string? CriticalSegment { get; set; }
    public int TotalDurationMinutes { get; set; }
    public string JARUSReference { get; set; } = string.Empty;
}

/// <summary>
/// Segment SAIL result
/// </summary>
public class SegmentSAILResult
{
    public string SegmentName { get; set; } = string.Empty;
    public SAILLevel SAILLevel { get; set; }
    public int FinalGRC { get; set; }
    public string ResidualARC { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}
