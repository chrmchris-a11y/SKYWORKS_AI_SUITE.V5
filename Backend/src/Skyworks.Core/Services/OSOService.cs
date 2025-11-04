using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services;

/// <summary>
/// OSO (Operational Safety Objectives) Service - Step 32: SAIL Adjustment
/// Implements OSO requirements per JARUS SORA 2.0 Table 6 and SORA 2.5 Table 14
/// Dual version support for both SORA 2.0 (24 OSOs) and SORA 2.5 (17 OSOs)
/// Reference: JARUS SORA 2.0 Section 2.5.2, SORA 2.5 Section 4.9, Annex E
/// </summary>
public interface IOSOService
{
    /// <summary>
    /// Get OSO requirements for a given SAIL level
    /// </summary>
    /// <param name="sail">SAIL level (I-VI)</param>
    /// <param name="soraVersion">SORA version (2.0 or 2.5)</param>
    OSORequirementsResult GetOSORequirements(SAILLevel sail, string soraVersion = "2.5");
    
    /// <summary>
    /// Validate OSO compliance for an operation
    /// </summary>
    OSOComplianceResult ValidateOSOCompliance(SAILLevel sail, List<OSOImplementation> implementedOSOs, string soraVersion = "2.5");
    
    /// <summary>
    /// Get detailed OSO information by ID
    /// </summary>
    OSODetails? GetOSODetails(int osoId, string soraVersion = "2.5");
}

public class OSOService : IOSOService
{
    // SORA 2.0 Table 6 - 24 OSOs (O = Optional, L = Low, M = Medium, H = High)
    // SORA 2.5 Table 14 - 17 OSOs (NR = Not Required instead of O)
    private static readonly Dictionary<int, OSODefinition> _osoDefinitions = new()
    {
        // OSO #01 - Operator Competence
        {1, new OSODefinition
        {
            Id = 1,
            Name = "Ensure the Operator is competent and/or proven",
            Category = OSOCategory.Organizational,
            SORA20Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#01"
        }},
        
        // OSO #02 - UAS Manufacturer
        {2, new OSODefinition
        {
            Id = 2,
            Name = "UAS manufactured by competent and/or proven entity",
            Category = OSOCategory.Design,
            SORA20Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer" },
            AnnexESection = "E.2 OSO#02"
        }},
        
        // OSO #03 - UAS Maintenance
        {3, new OSODefinition
        {
            Id = 3,
            Name = "UAS maintained by competent and/or proven entity",
            Category = OSOCategory.Operational,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#03"
        }},
        
        // OSO #04 - Airworthiness Design Standard
        {4, new OSODefinition
        {
            Id = 4,
            Name = "UAS components essential to safe operations are designed to an Airworthiness Design Standard (ADS)",
            Category = OSOCategory.Design,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Optional, OSORobustnessLevel.Optional, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer" },
            AnnexESection = "E.2 OSO#04"
        }},
        
        // OSO #05 - System Safety & Reliability
        {5, new OSODefinition
        {
            Id = 5,
            Name = "UAS is designed considering system safety and reliability",
            Category = OSOCategory.Design,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Optional, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer" },
            AnnexESection = "E.2 OSO#05",
            Notes = "SORA 2.5: Merged OSO#10 into OSO#05"
        }},
        
        // OSO #06 - C3 Link
        {6, new OSODefinition
        {
            Id = 6,
            Name = "C3 link characteristics are appropriate for the operation",
            Category = OSOCategory.Technical,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer", "Operator" },
            AnnexESection = "E.2 OSO#06"
        }},
        
        // OSO #07 - Conformity Check
        {7, new OSODefinition
        {
            Id = 7,
            Name = "Conformity check of the UAS configuration",
            Category = OSOCategory.Operational,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#07"
        }},
        
        // OSO #08 - Operational Procedures
        {8, new OSODefinition
        {
            Id = 8,
            Name = "Operational procedures are defined, validated and adhered to",
            Category = OSOCategory.Operational,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#08"
        }},
        
        // OSO #09 - Remote Crew Training
        {9, new OSODefinition
        {
            Id = 9,
            Name = "Remote crew trained and current",
            Category = OSOCategory.Personnel,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator", "Training org" },
            AnnexESection = "E.2 OSO#09"
        }},
        
        // OSO #10 - Safe Recovery (SORA 2.0 only - merged into OSO#05 in 2.5)
        {10, new OSODefinition
        {
            Id = 10,
            Name = "Safe recovery from technical issue",
            Category = OSOCategory.Technical,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = null, // Merged into OSO#05 in SORA 2.5
            AnnexESection = "Merged into OSO#05 in SORA 2.5"
        }},
        
        // OSO #11 - External Systems Procedures (SORA 2.0 only)
        {11, new OSODefinition
        {
            Id = 11,
            Name = "Procedures are in-place to handle the deterioration of external systems supporting UAS operation",
            Category = OSOCategory.ExternalSystems,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = null, // Removed in SORA 2.5
            AnnexESection = "Removed in SORA 2.5"
        }},
        
        // OSO #12 - External Systems Design (SORA 2.0 only)
        {12, new OSODefinition
        {
            Id = 12,
            Name = "The UAS is designed to manage the deterioration of external systems supporting UAS operation",
            Category = OSOCategory.ExternalSystems,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = null, // Removed in SORA 2.5
            AnnexESection = "Removed in SORA 2.5"
        }},
        
        // OSO #13 - External Services
        {13, new OSODefinition
        {
            Id = 13,
            Name = "External services supporting UAS operations are adequate to the operation",
            Category = OSOCategory.ExternalSystems,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#13"
        }},
        
        // OSO #14 - Human Error Procedures (SORA 2.0 only)
        {14, new OSODefinition
        {
            Id = 14,
            Name = "Operational procedures are defined, validated and adhered to (Human Error)",
            Category = OSOCategory.HumanError,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = null, // Removed in SORA 2.5 (covered by OSO#08)
            AnnexESection = "Removed in SORA 2.5 - covered by OSO#08"
        }},
        
        // OSO #15 - Human Error Training (SORA 2.0 only)
        {15, new OSODefinition
        {
            Id = 15,
            Name = "Remote crew trained and current and able to control the abnormal situation (Human Error)",
            Category = OSOCategory.HumanError,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = null, // Removed in SORA 2.5 (covered by OSO#09)
            AnnexESection = "Removed in SORA 2.5 - covered by OSO#09"
        }},
        
        // OSO #16 - Multi-Crew Coordination
        {16, new OSODefinition
        {
            Id = 16,
            Name = "Multi crew coordination",
            Category = OSOCategory.Personnel,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#16"
        }},
        
        // OSO #17 - Crew Fitness
        {17, new OSODefinition
        {
            Id = 17,
            Name = "Remote crew is fit to operate",
            Category = OSOCategory.Personnel,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator" },
            AnnexESection = "E.2 OSO#17"
        }},
        
        // OSO #18 - Flight Envelope Protection
        {18, new OSODefinition
        {
            Id = 18,
            Name = "Automatic protection of the flight envelope from human errors",
            Category = OSOCategory.HumanError,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Optional, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer" },
            AnnexESection = "E.2 OSO#18"
        }},
        
        // OSO #19 - Human Error Recovery
        {19, new OSODefinition
        {
            Id = 19,
            Name = "Safe recovery from Human Error",
            Category = OSOCategory.HumanError,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Optional, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer" },
            AnnexESection = "E.2 OSO#19"
        }},
        
        // OSO #20 - Human Factors Evaluation
        {20, new OSODefinition
        {
            Id = 20,
            Name = "A Human Factors evaluation has been performed and the HMI found appropriate for the mission",
            Category = OSOCategory.HumanError,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer", "Operator" },
            AnnexESection = "E.2 OSO#20"
        }},
        
        // OSO #21 - Adverse Conditions Procedures (SORA 2.0 only)
        {21, new OSODefinition
        {
            Id = 21,
            Name = "Operational procedures are defined, validated and adhered to (Adverse Conditions)",
            Category = OSOCategory.AdverseConditions,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = null, // Removed in SORA 2.5 (covered by OSO#08)
            AnnexESection = "Removed in SORA 2.5 - covered by OSO#08"
        }},
        
        // OSO #22 - Adverse Conditions Training (SORA 2.0 only)
        {22, new OSODefinition
        {
            Id = 22,
            Name = "The remote crew is trained to identify critical environmental conditions and to avoid them",
            Category = OSOCategory.AdverseConditions,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High },
            SORA25Robustness = null, // Removed in SORA 2.5 (covered by OSO#09)
            AnnexESection = "Removed in SORA 2.5 - covered by OSO#09"
        }},
        
        // OSO #23 - Environmental Conditions
        {23, new OSODefinition
        {
            Id = 23,
            Name = "Environmental conditions for safe operations defined, measurable and adhered to",
            Category = OSOCategory.AdverseConditions,
            SORA20Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.Low, OSORobustnessLevel.Low, OSORobustnessLevel.Medium, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Operator", "Designer" },
            AnnexESection = "E.2 OSO#23"
        }},
        
        // OSO #24 - Adverse Environmental Design
        {24, new OSODefinition
        {
            Id = 24,
            Name = "UAS designed and qualified for adverse environmental conditions",
            Category = OSOCategory.AdverseConditions,
            SORA20Robustness = new[] { OSORobustnessLevel.Optional, OSORobustnessLevel.Optional, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            SORA25Robustness = new[] { OSORobustnessLevel.NotRequired, OSORobustnessLevel.NotRequired, OSORobustnessLevel.Medium, OSORobustnessLevel.High, OSORobustnessLevel.High, OSORobustnessLevel.High },
            Dependencies = new[] { "Designer" },
            AnnexESection = "E.2 OSO#24"
        }}
    };

    public OSORequirementsResult GetOSORequirements(SAILLevel sail, string soraVersion = "2.5")
    {
        var result = new OSORequirementsResult
        {
            SAIL = sail,
            SORAVersion = soraVersion,
            Requirements = new List<OSORequirement>()
        };

        int sailIndex = (int)sail - 1; // SAIL I=1 => index 0, SAIL II=2 => index 1, ..., SAIL VI=6 => index 5
        bool isSora20 = soraVersion == "2.0";

        foreach (var kvp in _osoDefinitions)
        {
            var oso = kvp.Value;
            
            // Skip OSOs removed in SORA 2.5
            if (!isSora20 && oso.SORA25Robustness == null)
                continue;

            var robustness = isSora20 
                ? oso.SORA20Robustness![sailIndex] 
                : oso.SORA25Robustness![sailIndex];

            result.Requirements.Add(new OSORequirement
            {
                OSOId = oso.Id,
                Name = oso.Name,
                Category = oso.Category,
                RobustnessLevel = robustness,
                Dependencies = oso.Dependencies?.ToList() ?? new List<string>(),
                AnnexESection = oso.AnnexESection,
                Notes = oso.Notes
            });
        }

        result.TotalCount = result.Requirements.Count;
        result.RequiredCount = result.Requirements.Count(r => 
            r.RobustnessLevel != OSORobustnessLevel.NotRequired && 
            r.RobustnessLevel != OSORobustnessLevel.Optional);
        
        result.Summary = $"SAIL {GetRomanNumeral(sail)}: {result.RequiredCount} OSOs required (SORA {soraVersion})";

        return result;
    }

    public OSOComplianceResult ValidateOSOCompliance(SAILLevel sail, List<OSOImplementation> implementedOSOs, string soraVersion = "2.5")
    {
        var requirements = GetOSORequirements(sail, soraVersion);
        var result = new OSOComplianceResult
        {
            SAIL = sail,
            SORAVersion = soraVersion,
            IsCompliant = true,
            MissingOSOs = new List<string>(),
            InsufficientRobustness = new List<string>(),
            Warnings = new List<string>()
        };

        foreach (var req in requirements.Requirements)
        {
            if (req.RobustnessLevel == OSORobustnessLevel.NotRequired || 
                req.RobustnessLevel == OSORobustnessLevel.Optional)
                continue;

            var impl = implementedOSOs.FirstOrDefault(i => i.OSOId == req.OSOId);
            
            if (impl == null)
            {
                result.MissingOSOs.Add($"OSO#{req.OSOId:D2}: {req.Name}");
                result.IsCompliant = false;
            }
            else if (impl.AchievedRobustness < req.RobustnessLevel)
            {
                result.InsufficientRobustness.Add(
                    $"OSO#{req.OSOId:D2}: Required {req.RobustnessLevel}, Achieved {impl.AchievedRobustness}");
                result.IsCompliant = false;
            }
        }

        result.Summary = result.IsCompliant 
            ? $"✅ All {requirements.RequiredCount} required OSOs compliant for SAIL {GetRomanNumeral(sail)}"
            : $"❌ {result.MissingOSOs.Count + result.InsufficientRobustness.Count} non-compliances found";

        return result;
    }

    public OSODetails? GetOSODetails(int osoId, string soraVersion = "2.5")
    {
        if (!_osoDefinitions.TryGetValue(osoId, out var oso))
            return null;

        // Check if OSO exists in requested version
        if (soraVersion == "2.5" && oso.SORA25Robustness == null)
            return null;

        return new OSODetails
        {
            OSOId = oso.Id,
            Name = oso.Name,
            Category = oso.Category,
            RobustnessPerSAIL = soraVersion == "2.0" ? oso.SORA20Robustness! : oso.SORA25Robustness!,
            Dependencies = oso.Dependencies?.ToList() ?? new List<string>(),
            AnnexESection = oso.AnnexESection,
            Notes = oso.Notes,
            SORAVersion = soraVersion
        };
    }

    private string GetRomanNumeral(SAILLevel sail) => sail switch
    {
        SAILLevel.I => "I",
        SAILLevel.II => "II",
        SAILLevel.III => "III",
        SAILLevel.IV => "IV",
        SAILLevel.V => "V",
        SAILLevel.VI => "VI",
        _ => sail.ToString()
    };
}

/// <summary>
/// OSO Definition (from JARUS SORA)
/// </summary>
public class OSODefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OSOCategory Category { get; set; }
    public OSORobustnessLevel[]? SORA20Robustness { get; set; } // 6 values for SAIL I-VI
    public OSORobustnessLevel[]? SORA25Robustness { get; set; } // null if removed in 2.5
    public string[]? Dependencies { get; set; } // Operator, Designer, Training org
    public string AnnexESection { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// OSO Requirements Result
/// </summary>
public class OSORequirementsResult
{
    public SAILLevel SAIL { get; set; }
    public string SORAVersion { get; set; } = "2.5";
    public List<OSORequirement> Requirements { get; set; } = new();
    public int TotalCount { get; set; }
    public int RequiredCount { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual OSO Requirement
/// </summary>
public class OSORequirement
{
    public int OSOId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OSOCategory Category { get; set; }
    public OSORobustnessLevel RobustnessLevel { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public string AnnexESection { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// OSO Compliance Validation Result
/// </summary>
public class OSOComplianceResult
{
    public SAILLevel SAIL { get; set; }
    public string SORAVersion { get; set; } = "2.5";
    public bool IsCompliant { get; set; }
    public List<string> MissingOSOs { get; set; } = new();
    public List<string> InsufficientRobustness { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// OSO Implementation (provided by operator)
/// </summary>
public class OSOImplementation
{
    public int OSOId { get; set; }
    public OSORobustnessLevel AchievedRobustness { get; set; }
    public string Evidence { get; set; } = string.Empty;
    public DateTime? VerificationDate { get; set; }
}

/// <summary>
/// Detailed OSO Information
/// </summary>
public class OSODetails
{
    public int OSOId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OSOCategory Category { get; set; }
    public OSORobustnessLevel[] RobustnessPerSAIL { get; set; } = Array.Empty<OSORobustnessLevel>();
    public List<string> Dependencies { get; set; } = new();
    public string AnnexESection { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string SORAVersion { get; set; } = "2.5";
}
