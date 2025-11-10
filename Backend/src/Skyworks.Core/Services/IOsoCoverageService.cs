using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Service for analyzing OSO (Operational Safety Objectives) coverage for a mission.
/// Based on SORA 2.5 Annex D, determines required, covered, and missing OSOs.
/// </summary>
public interface IOsoCoverageService
{
    /// <summary>
    /// Builds OSO checklist for a mission based on its SAIL level and characteristics.
    /// </summary>
    /// <param name="mission">Mission with template/category information.</param>
    /// <param name="erp">Emergency Response Plan generated for the mission.</param>
    /// <param name="sora">SORA assessment with SAIL level.</param>
    /// <returns>OSO coverage result with required, covered, and missing OSOs.</returns>
    OsoCoverageResult BuildChecklist(Mission mission, MissionErp erp, MissionSoraAssessment sora);
}
