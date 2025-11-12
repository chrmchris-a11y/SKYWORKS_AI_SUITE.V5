using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Service for generating rule-based Emergency Response Plans (ERP).
/// Implements EASA/JARUS SORA guidance for contingency procedures.
/// </summary>
public interface IErpGeneratorService
{
    /// <summary>
    /// Generate Emergency Response Plan for a mission
    /// </summary>
    /// <param name="mission">Mission details</param>
    /// <param name="geometry">Mission geometry (including CGA)</param>
    /// <param name="sora">SORA assessment results</param>
    /// <returns>ERP with structured JSON and narrative text</returns>
    ErpResult Generate(Mission mission, MissionGeometry geometry, MissionSoraAssessment sora);
}
