namespace Skyworks.Core.Models.ARC;

/// <summary>
/// Tactical Mitigation Performance Requirement (TMPR) level
/// </summary>
public enum TMPRLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4
}

public class TMPRResult
{
    public required TMPRLevel Level { get; set; }
    public required Skyworks.Core.Models.GRC.RobustnessLevel Robustness { get; set; }
    public string Notes { get; set; } = string.Empty;
}
