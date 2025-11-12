using System;
using System.Threading.Tasks;
using Skyworks.Core.Services.DTOs;

namespace Skyworks.Core.Services;

/// <summary>
/// Orchestrates the complete mission creation pipeline:
/// Template → Mission → Geometry → SORA → ERP → OSO → Database
/// </summary>
public interface IMissionOrchestratorService
{
    /// <summary>
    /// Creates a complete mission with all related entities:
    /// 1. Resolve template metadata
    /// 2. Create Mission entity
    /// 3. Generate geometry (route/area/pattern)
    /// 4. Calculate SORA (initial/final GRC, ARC, SAIL)
    /// 5. Generate ERP (rule-based emergency response plan)
    /// 6. Build OSO coverage checklist
    /// 7. Persist all entities to database
    /// </summary>
    /// <param name="request">Mission creation parameters</param>
    /// <returns>Unique mission ID (Guid)</returns>
    Task<Guid> CreateMissionAsync(CreateMissionRequest request);
}
