using System.Collections.Generic;

namespace Skyworks.Core.Services;

/// <summary>
/// Configuration provider for mission template metadata.
/// Maps each MissionTemplateCode to default parameters (category, geometry type, environment, SORA version).
/// </summary>
public interface IMissionTemplatesConfig
{
    /// <summary>
    /// Get metadata for a specific template code
    /// </summary>
    /// <param name="code">Mission template code</param>
    /// <returns>Template metadata</returns>
    /// <exception cref="KeyNotFoundException">If template code not found</exception>
    Models.MissionTemplateMetadata Get(Models.MissionTemplateCode code);
    
    /// <summary>
    /// Get all template metadata (48 entries)
    /// </summary>
    IReadOnlyList<Models.MissionTemplateMetadata> GetAll();
}
