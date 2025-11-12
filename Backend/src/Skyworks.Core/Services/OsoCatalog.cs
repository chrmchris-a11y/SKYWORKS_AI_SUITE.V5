using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// In-memory catalog of all Operational Safety Objectives (OSOs) from SORA 2.5 Annex D.
/// Provides SAIL-based filtering to determine which OSOs are required for each operation.
/// </summary>
public class OsoCatalog : IOsoCatalog
{
    private readonly List<OsoItem> _allOsos;
    private readonly Dictionary<string, List<string>> _sailToOsoMapping;

    public OsoCatalog()
    {
        _allOsos = InitializeOsoCatalog();
        _sailToOsoMapping = InitializeSailMapping();
    }

    public IReadOnlyList<OsoItem> GetAll() => _allOsos.AsReadOnly();

    public OsoItem? GetByCode(string code) => _allOsos.FirstOrDefault(o => o.Code == code);

    public IReadOnlyList<OsoItem> GetRequiredForSail(string sail)
    {
        var sailKey = sail.Trim().ToUpper();
        if (!_sailToOsoMapping.ContainsKey(sailKey))
        {
            return new List<OsoItem>().AsReadOnly();
        }

        var requiredCodes = _sailToOsoMapping[sailKey];
        return _allOsos.Where(o => requiredCodes.Contains(o.Code)).ToList().AsReadOnly();
    }

    #region OSO Catalog Initialization

    /// <summary>
    /// Initializes the complete OSO catalog based on SORA 2.5 Annex D.
    /// Total: 24 OSOs covering procedures, technical, and training requirements.
    /// </summary>
    private List<OsoItem> InitializeOsoCatalog()
    {
        return new List<OsoItem>
        {
            // ===================================================================
            // PROCEDURES (OSO#01 - OSO#08)
            // ===================================================================
            new OsoItem
            {
                Code = "OSO#01",
                Title = "Operational procedures designed, maintained and adhered to",
                Description = "Ensure operational procedures are properly designed, documented, maintained and followed throughout operations.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#02",
                Title = "UAS operator competencies (multi-crew coordination & communication)",
                Description = "Operator must demonstrate competencies in multi-crew coordination and communication when applicable.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#03",
                Title = "Training and proficiency of remote crew and other personnel",
                Description = "Remote crew and other involved personnel must receive appropriate training and maintain proficiency.",
                Category = "Training"
            },
            new OsoItem
            {
                Code = "OSO#04",
                Title = "UAS design and airworthiness",
                Description = "UAS design must be appropriate for the operation and airworthiness must be assured.",
                Category = "Technical"
            },
            new OsoItem
            {
                Code = "OSO#05",
                Title = "UAS maintenance",
                Description = "UAS must be maintained according to manufacturer recommendations and operational requirements.",
                Category = "Technical"
            },
            new OsoItem
            {
                Code = "OSO#06",
                Title = "External services supporting UAS operations",
                Description = "External services (e.g., meteorology, NOTAM) must be used when necessary and be reliable.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#07",
                Title = "Operational procedures designed to manage  autonomous operations",
                Description = "Procedures must address autonomous flight modes, decision-making, and intervention capabilities.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#08",
                Title = "Operational procedures designed to handle  the deterioration of external systems",
                Description = "Procedures must address loss of external support systems (e.g., GNSS, communication).",
                Category = "Procedures"
            },

            // ===================================================================
            // ADJACENCY & GROUND RISK MITIGATIONS (OSO#09 - OSO#14)
            // ===================================================================
            new OsoItem
            {
                Code = "OSO#09",
                Title = "Ground risk mitigation: strategic (location and features)",
                Description = "Strategic measures to reduce ground risk through operation location selection and operational area features.",
                Category = "Ground Risk"
            },
            new OsoItem
            {
                Code = "OSO#10",
                Title = "Ground risk mitigation: effects of ground impact",
                Description = "Tactical mitigations to reduce impact consequences (e.g., parachute, low kinetic energy).",
                Category = "Ground Risk"
            },
            new OsoItem
            {
                Code = "OSO#11",
                Title = "Separation from uninvolved persons",
                Description = "Measures to maintain adequate separation from people not involved in the operation.",
                Category = "Ground Risk"
            },
            new OsoItem
            {
                Code = "OSO#12",
                Title = "Operational volume containment",
                Description = "Ensure UAS remains within defined operational volume (e.g., geo-fencing, detect & avoid).",
                Category = "Technical"
            },
            new OsoItem
            {
                Code = "OSO#13",
                Title = "Operational procedures designed to manage the  hazards from third party operations",
                Description = "Procedures to manage risks from other airspace users and third-party operations.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#14",
                Title = "Operational procedures for management of  ATC/U-space coordination",
                Description = "Procedures for coordination with air traffic control or U-space service providers.",
                Category = "Procedures"
            },

            // ===================================================================
            // AIR RISK MITIGATIONS (OSO#15 - OSO#20)
            // ===================================================================
            new OsoItem
            {
                Code = "OSO#15",
                Title = "Strategic mitigation to reduce air risk",
                Description = "Strategic measures to reduce collision risk with manned aircraft (e.g., airspace selection, altitude limits).",
                Category = "Air Risk"
            },
            new OsoItem
            {
                Code = "OSO#16",
                Title = "Tactical mitigation to reduce air risk",
                Description = "Tactical measures for collision avoidance (e.g., visual observers, electronic conspicuity, detect & avoid).",
                Category = "Air Risk"
            },
            new OsoItem
            {
                Code = "OSO#17",
                Title = "Adjacent area / airspace considerations",
                Description = "Consideration of adjacent areas and airspace that may affect safety (e.g., proximity to aerodromes, CTR).",
                Category = "Air Risk"
            },
            new OsoItem
            {
                Code = "OSO#18",
                Title = "Activation of airspace and procedure segregation",
                Description = "Procedures for airspace activation and segregation when operating in controlled or restricted airspace.",
                Category = "Air Risk"
            },
            new OsoItem
            {
                Code = "OSO#19",
                Title = "U-space / ATC services and procedures",
                Description = "Use of U-space services or coordination with ATC for traffic information and airspace management.",
                Category = "Air Risk"
            },
            new OsoItem
            {
                Code = "OSO#20",
                Title = "Conspicuity",
                Description = "UAS must be conspicuous to other airspace users (e.g., lights, transponder, FLARM).",
                Category = "Technical"
            },

            // ===================================================================
            // CONTINGENCY & EMERGENCY (OSO#21 - OSO#24)
            // ===================================================================
            new OsoItem
            {
                Code = "OSO#21",
                Title = "Operational procedures for abnormal and emergency situations",
                Description = "Procedures for handling abnormal situations, emergencies, and contingencies (e.g., loss of C2, fly-away).",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#22",
                Title = "Emergency response plan (ERP)",
                Description = "A documented Emergency Response Plan must be in place and include coordination with emergency services.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#23",
                Title = "Human error management",
                Description = "Procedures and design features to minimize and manage human error during operations.",
                Category = "Procedures"
            },
            new OsoItem
            {
                Code = "OSO#24",
                Title = "Evaluation and continuous improvement",
                Description = "Continuous evaluation of operations and safety performance, with improvement mechanisms.",
                Category = "Procedures"
            }
        };
    }

    #endregion

    #region SAIL to OSO Mapping

    /// <summary>
    /// Maps each SAIL level to required OSO codes based on SORA 2.5 Annex D Table D-1.
    /// Higher SAIL levels progressively require more OSOs with higher robustness.
    /// 
    /// Note: This is a simplified mapping. Actual SORA 2.5 specifies robustness levels
    /// (Low/Medium/High) per OSO per SAIL, but for v1 we track presence/absence.
    /// </summary>
    private Dictionary<string, List<string>> InitializeSailMapping()
    {
        return new Dictionary<string, List<string>>
        {
            // SAIL I: Minimal requirements (very low risk)
            ["I"] = new List<string>
            {
                "OSO#01",  // Operational procedures
                "OSO#03",  // Training
                "OSO#04",  // UAS design
                "OSO#05",  // Maintenance
                "OSO#09",  // Ground risk strategic
                "OSO#21",  // Emergency procedures
                "OSO#22"   // ERP
            },

            // SAIL II: Low risk, additional operational controls
            ["II"] = new List<string>
            {
                "OSO#01", "OSO#03", "OSO#04", "OSO#05", "OSO#09", "OSO#21", "OSO#22",
                "OSO#06",  // External services
                "OSO#10",  // Ground impact mitigation
                "OSO#11",  // Separation from people
                "OSO#23"   // Human error
            },

            // SAIL III: Medium risk, more stringent requirements
            ["III"] = new List<string>
            {
                "OSO#01", "OSO#03", "OSO#04", "OSO#05", "OSO#06", "OSO#09", "OSO#10",
                "OSO#11", "OSO#21", "OSO#22", "OSO#23",
                "OSO#02",  // Multi-crew coordination
                "OSO#12",  // Volume containment
                "OSO#13",  // Third-party ops
                "OSO#15",  // Air risk strategic
                "OSO#16",  // Air risk tactical
                "OSO#24"   // Continuous improvement
            },

            // SAIL IV: High risk, comprehensive controls
            ["IV"] = new List<string>
            {
                "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06", "OSO#09",
                "OSO#10", "OSO#11", "OSO#12", "OSO#13", "OSO#15", "OSO#16", "OSO#21",
                "OSO#22", "OSO#23", "OSO#24",
                "OSO#07",  // Autonomous operations
                "OSO#08",  // External system degradation
                "OSO#14",  // ATC/U-space coordination
                "OSO#17",  // Adjacent airspace
                "OSO#19"   // U-space/ATC services
            },

            // SAIL V: Very high risk, near-maximum requirements (23 OSOs)
            ["V"] = new List<string>
            {
                "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06", "OSO#07",
                "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12", "OSO#13", "OSO#14",
                "OSO#15", "OSO#16", "OSO#17", "OSO#19", "OSO#21", "OSO#22", "OSO#23",
                "OSO#18",  // Airspace segregation (23 total)
                "OSO#24"   // Continuous improvement
            },

            // SAIL VI: Maximum risk, all OSOs required with high robustness
            ["VI"] = new List<string>
            {
                "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06", "OSO#07",
                "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12", "OSO#13", "OSO#14",
                "OSO#15", "OSO#16", "OSO#17", "OSO#18", "OSO#19", "OSO#20", "OSO#21",
                "OSO#22", "OSO#23", "OSO#24"
            }
        };
    }

    #endregion
}
