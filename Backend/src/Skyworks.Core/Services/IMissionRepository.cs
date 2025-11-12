using System;
using System.Threading.Tasks;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Repository interface for mission persistence.
/// Abstracts database operations for mission entities.
/// </summary>
public interface IMissionRepository
{
    /// <summary>
    /// Saves a complete mission with all related entities (Geometry, SORA, ERP, OSO).
    /// </summary>
    Task<Guid> SaveMissionAsync(Mission mission);
    
    /// <summary>
    /// Retrieves a mission by ID with all related entities.
    /// </summary>
    Task<Mission?> GetMissionByIdAsync(Guid missionId);
}
