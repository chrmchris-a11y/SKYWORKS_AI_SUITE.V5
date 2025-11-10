namespace Skyworks.Core.Models;

/// <summary>
/// Result of ERP generation containing structured JSON and narrative text
/// </summary>
public class ErpResult
{
    /// <summary>
    /// Structured ERP as JSON (serialized ErpPlan object)
    /// For machine processing and structured storage
    /// </summary>
    public string ErpJson { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable ERP narrative text
    /// For reports, manuals, Operations Manual (OM), briefings
    /// </summary>
    public string ErpText { get; set; } = string.Empty;
}
