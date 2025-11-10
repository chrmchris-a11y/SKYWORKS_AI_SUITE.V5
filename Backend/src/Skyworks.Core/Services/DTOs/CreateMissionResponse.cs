using System;

namespace Skyworks.Core.Services.DTOs;

/// <summary>
/// Response DTO for mission auto-creation.
/// Returns the unique mission ID after successful pipeline execution.
/// </summary>
public class CreateMissionResponse
{
    /// <summary>
    /// Unique identifier for the created mission
    /// </summary>
    public Guid MissionId { get; set; }
}
