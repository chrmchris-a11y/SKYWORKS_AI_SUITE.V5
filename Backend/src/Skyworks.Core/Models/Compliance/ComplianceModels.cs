namespace Skyworks.Core.Models.Compliance;

/// <summary>
/// Operator obligation/requirement για SORA compliance.
/// Προκύπτει από Annexes, ARC level, airspace type, κ.λπ.
/// </summary>
public class ComplianceObligation
{
    /// <summary>
    /// Τύπος υποχρέωσης (e.g., "U-space", "NOTAM", "ATC", "Airspace Auth", "OSO", "TMPR")
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Σύντομη περιγραφή της υποχρέωσης
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Αναλυτικές οδηγίες για τον operator
    /// </summary>
    public string? DetailedGuidance { get; set; }

    /// <summary>
    /// Priority/Severity: "Mandatory", "Recommended", "Optional"
    /// </summary>
    public required string Priority { get; set; }

    /// <summary>
    /// Source reference (e.g., "Annex B §2.3.1", "EASA Regulation XYZ")
    /// </summary>
    public string? ReferenceSource { get; set; }

    /// <summary>
    /// Deadline για submission (αν υπάρχει)
    /// </summary>
    public string? Deadline { get; set; }

    /// <summary>
    /// External links (e.g., NOTAM submission portal, U-space registration)
    /// </summary>
    public List<string>? RelatedLinks { get; set; }
}

/// <summary>
/// Checklist item για pre-flight compliance verification
/// </summary>
public class ComplianceChecklistItem
{
    public required string Category { get; set; } // "Airspace", "Weather", "Equipment", "Documentation"
    public required string Item { get; set; }
    public required bool IsMandatory { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceSource { get; set; }
}

/// <summary>
/// Request για compliance obligations (based on mission parameters)
/// </summary>
public class ComplianceRequest
{
    /// <summary>
    /// ARC level (a/b/c/d) - affects TMPR and OSO requirements
    /// </summary>
    public string? ARC { get; set; }

    /// <summary>
    /// Airspace type (CTR, ATZ, TMA, Uncontrolled, etc.)
    /// </summary>
    public string? AirspaceType { get; set; }

    /// <summary>
    /// Operation type (VLOS, BVLOS, urban, rural)
    /// </summary>
    public string? OperationType { get; set; }

    /// <summary>
    /// Max altitude AGL (meters)
    /// </summary>
    public int? MaxAltitudeAGL { get; set; }

    /// <summary>
    /// Country code (e.g., "CY", "GR") - for jurisdiction-specific rules
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Is U-space airspace?
    /// </summary>
    public bool IsUSpaceAirspace { get; set; }

    /// <summary>
    /// Special use areas (e.g., near airport, military, restricted)
    /// </summary>
    public List<string>? SpecialUseAreas { get; set; }
}

/// <summary>
/// Response με compliance obligations και checklist
/// </summary>
public class ComplianceResult
{
    /// <summary>
    /// List of obligations/requirements
    /// </summary>
    public List<ComplianceObligation> Obligations { get; set; } = new();

    /// <summary>
    /// Pre-flight checklist items
    /// </summary>
    public List<ComplianceChecklistItem> Checklist { get; set; } = new();

    /// <summary>
    /// Overall compliance status
    /// </summary>
    public string Status { get; set; } = "Unknown"; // "Ready", "Pending", "Incomplete"

    /// <summary>
    /// Additional notes or warnings
    /// </summary>
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Operator readiness assessment (για SORA submission)
/// </summary>
public class OperatorReadiness
{
    public required string OperatorId { get; set; }
    public required string MissionId { get; set; }
    
    /// <summary>
    /// Required OSO levels (e.g., "OSO#1: Medium", "OSO#2: High")
    /// </summary>
    public List<string> RequiredOSOs { get; set; } = new();

    /// <summary>
    /// Required TMPR level (1-4)
    /// </summary>
    public int? RequiredTMPR { get; set; }

    /// <summary>
    /// Documentation checklist (OpManual, Risk Assessment, etc.)
    /// </summary>
    public List<ComplianceChecklistItem> DocumentationChecklist { get; set; } = new();

    /// <summary>
    /// Equipment checklist (Remote ID, Parachute, Geo-Fencing, etc.)
    /// </summary>
    public List<ComplianceChecklistItem> EquipmentChecklist { get; set; } = new();

    /// <summary>
    /// Authorization status
    /// </summary>
    public string AuthorizationStatus { get; set; } = "Not Submitted"; // "Not Submitted", "Pending", "Approved", "Rejected"

    /// <summary>
    /// Validation notes/errors
    /// </summary>
    public List<string> ValidationNotes { get; set; } = new();
}
