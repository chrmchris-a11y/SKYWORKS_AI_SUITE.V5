namespace Skyworks.Core.Models.SAIL;

/// <summary>
/// OSO Robustness Level per JARUS SORA Annex E
/// Different from GRC RobustnessLevel (which has None/Low/Medium/High)
/// </summary>
public enum OSORobustnessLevel
{
    NotRequired = 0,  // NR in SORA 2.5
    Optional = 1,     // O in SORA 2.0 (same as NR)
    Low = 2,          // L
    Medium = 3,       // M
    High = 4          // H
}

/// <summary>
/// OSO Category
/// </summary>
public enum OSOCategory
{
    Organizational,
    Design,
    Operational,
    Technical,
    Personnel,
    ExternalSystems,
    HumanError,
    AdverseConditions
}
