namespace Skyworks.Core.Models;

/// <summary>
/// Mission type categories based on common UAS operations.
/// Used to auto-generate appropriate mission geometry and parameters.
/// </summary>
public enum MissionType
{
    /// <summary>
    /// Facade inspection: vertical grid pattern along building face
    /// </summary>
    Facade = 1,
    
    /// <summary>
    /// Roof inspection: horizontal grid pattern over building top
    /// </summary>
    Roof = 2,
    
    /// <summary>
    /// Solar panel inspection: orthogonal grid over solar farm
    /// </summary>
    Solar = 3,
    
    /// <summary>
    /// Linear infrastructure: railway, pipeline, powerline corridor
    /// </summary>
    Linear = 4,
    
    /// <summary>
    /// Agriculture: crop monitoring, spraying patterns
    /// </summary>
    Agriculture = 5,
    
    /// <summary>
    /// Custom mission: user provides KML/GeoJSON/CSV geometry
    /// </summary>
    Custom = 6
}
