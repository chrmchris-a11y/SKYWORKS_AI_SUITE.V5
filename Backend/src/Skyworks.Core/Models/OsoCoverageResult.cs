namespace Skyworks.Core.Models;

/// <summary>
/// Result of OSO coverage analysis for a mission.
/// Contains required OSOs based on SAIL, covered OSOs based on mission/ERP, and missing OSOs.
/// </summary>
public class OsoCoverageResult
{
    /// <summary>
    /// JSON array of OsoItem objects that are REQUIRED for this mission's SAIL level.
    /// Serialized from List&lt;OsoItem&gt;.
    /// </summary>
    public string RequiredOsosJson { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of OsoItem objects that are considered COVERED based on mission characteristics and ERP.
    /// Serialized from List&lt;OsoItem&gt;.
    /// </summary>
    public string CoveredOsosJson { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of OsoItem objects that are MISSING (Required but not Covered).
    /// Serialized from List&lt;OsoItem&gt;.
    /// </summary>
    public string MissingOsosJson { get; set; } = string.Empty;
}
