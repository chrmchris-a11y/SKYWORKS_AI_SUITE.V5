namespace Skyworks.Core.Models;

/// <summary>
/// High-level mission categories for organizing mission templates.
/// Based on common UAS operation domains per EASA/JARUS guidance.
/// </summary>
public enum MissionCategory
{
    /// <summary>
    /// Building facade inspection, cleaning, thermal imaging
    /// </summary>
    BuildingsAndFacades = 1,
    
    /// <summary>
    /// Solar PV, wind turbines, powerlines, pipelines, electrical infrastructure
    /// </summary>
    EnergyAndUtilities = 2,
    
    /// <summary>
    /// Bridges, roads, railways, ports, dams, urban networks
    /// </summary>
    InfrastructureAndTransport = 3,
    
    /// <summary>
    /// Crop monitoring, orchards, vineyards, forestry, rural surveys
    /// </summary>
    AgricultureAndForestry = 4,
    
    /// <summary>
    /// Topographic surveys, construction monitoring, 3D modeling, quarries
    /// </summary>
    SurveyAndConstruction = 5,
    
    /// <summary>
    /// SAR mapping, disaster assessment, flood mapping, post-fire assessment
    /// (Assessment/mapping only, NOT tactical response operations)
    /// </summary>
    PublicSafetyAndAssessment = 6,
    
    /// <summary>
    /// Real estate marketing, event filming, tourism aerial imaging
    /// </summary>
    MediaAndCommercial = 7,
    
    /// <summary>
    /// Training flights, system functional tests
    /// </summary>
    TrainingAndTest = 8,
    
    /// <summary>
    /// Uncategorized or custom operations
    /// </summary>
    Other = 9
}
