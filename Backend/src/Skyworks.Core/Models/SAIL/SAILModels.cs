namespace Skyworks.Core.Models.SAIL;

/// <summary>
/// Specific Assurance and Integrity Levels (SAIL I-VI)
/// </summary>
public enum SAILLevel
{
    I = 1,
    II = 2,
    III = 3,
    IV = 4,
    V = 5,
    VI = 6
}

public class SAILInput
{
    /// <summary>
    /// Final GRC (must be 1..7)
    /// </summary>
    public required int FinalGRC { get; set; }

    /// <summary>
    /// Residual ARC (a..d)
    /// </summary>
    public required Skyworks.Core.Models.ARC.ARCRating ResidualARC { get; set; }
}

public class SAILResult
{
    public bool IsSupported { get; set; }
    public SAILLevel? SAIL { get; set; }
    public string Notes { get; set; } = string.Empty;
}
