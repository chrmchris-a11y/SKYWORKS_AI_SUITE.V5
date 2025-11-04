using Skyworks.Core.Models.Compliance;

namespace Skyworks.Core.Services.Compliance;

/// <summary>
/// Compliance provider για SORA obligations (U-space, NOTAM, ATC, OSO, TMPR, etc.)
/// </summary>
public interface IComplianceProvider
{
    /// <summary>
    /// Get all compliance obligations based on mission parameters
    /// </summary>
    Task<ComplianceResult> GetComplianceObligationsAsync(ComplianceRequest request);

    /// <summary>
    /// Validate operator readiness for SORA submission
    /// </summary>
    Task<OperatorReadiness> ValidateOperatorReadinessAsync(string operatorId, string missionId, ComplianceRequest request);

    /// <summary>
    /// Get pre-flight checklist for a given mission
    /// </summary>
    Task<List<ComplianceChecklistItem>> GetPreFlightChecklistAsync(ComplianceRequest request);
}
