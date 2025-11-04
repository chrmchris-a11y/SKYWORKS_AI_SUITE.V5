namespace Skyworks.Core.Models.GRC;

/// <summary>
/// SORA methodology version selector
/// Supports both SORA 2.0 AMC (2019) and JARUS SORA 2.5 (latest)
/// </summary>
public enum SoraVersion
{
    /// <summary>
    /// SORA 2.0 AMC (JAR-DEL-WG6-D.04, Edition 2.0, 30.01.2019)
    /// - 7 operational scenarios (controlled area → gathering of people)
    /// - 4 UAS dimension categories (1m, 3m, 8m, >8m)
    /// - Table 2: 7×4 matrix (GRC 1-10)
    /// - Mitigations: M1 (strategic), M2 (impact reduction), M3 (ERP)
    /// </summary>
    SORA_2_0_AMC = 20,

    /// <summary>
    /// JARUS SORA 2.5 (JAR-DEL-SRM-SORA-MB-2.5, latest release)
    /// - 7 population density categories (controlled → assemblies >50k/km²)
    /// - 5 UAS dimension categories (1m/25m/s → 40m+/200m/s+)
    /// - Table 2: 7×5 matrix (GRC 1-10)
    /// - Mitigations: M1A (sheltering), M1B (operational restrictions), M1C (ground observation), M2 (impact dynamics)
    /// - Special rule: ≤250g & ≤25m/s → always GRC 1
    /// </summary>
    SORA_2_5 = 25
}
