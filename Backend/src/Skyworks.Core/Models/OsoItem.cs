namespace Skyworks.Core.Models;

/// <summary>
/// Represents a single Operational Safety Objective (OSO) from SORA 2.5 Annex D.
/// OSOs are the safety requirements that must be met based on the SAIL level.
/// </summary>
public class OsoItem
{
    /// <summary>
    /// OSO code, e.g., "OSO#01", "OSO#02", etc.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Short title/name of the OSO.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of what this OSO requires.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category/type of OSO (e.g., "Procedures", "Technical", "Training").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Robustness level if applicable ("Low", "Medium", "High").
    /// Per SORA, different SAIL levels may require different robustness.
    /// </summary>
    public string? RequiredRobustness { get; set; }
}
