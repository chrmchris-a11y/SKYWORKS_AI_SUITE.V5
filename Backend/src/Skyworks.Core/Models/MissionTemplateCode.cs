namespace Skyworks.Core.Models;

/// <summary>
/// Specific mission template codes (~48 predefined operation types).
/// Each template has associated metadata (category, default geometry type, environment, SORA version).
/// Used by MissionTemplatesConfig to auto-populate mission parameters.
/// </summary>
public enum MissionTemplateCode
{
    // ========================================================================
    // A. Buildings & Facades
    // ========================================================================
    
    /// <summary>
    /// Facade cleaning (urban high-rise buildings)
    /// </summary>
    FacadeCleaning = 101,
    
    /// <summary>
    /// Facade inspection (structural defects, cracks)
    /// </summary>
    FacadeInspection = 102,
    
    /// <summary>
    /// High-rise facade inspection (>30m height)
    /// </summary>
    HighRiseFacadeInspection = 103,
    
    /// <summary>
    /// Window cleaning for high-rise buildings
    /// </summary>
    WindowCleaningHighRise = 104,
    
    /// <summary>
    /// Flat roof inspection (horizontal surface)
    /// </summary>
    RoofInspection_Flat = 105,
    
    /// <summary>
    /// Pitched roof inspection (sloped surface)
    /// </summary>
    RoofInspection_Pitched = 106,
    
    /// <summary>
    /// Chimney inspection (vertical structure)
    /// </summary>
    ChimneyInspection = 107,
    
    /// <summary>
    /// Building envelope thermal imaging (energy efficiency assessment)
    /// </summary>
    BuildingEnvelopeThermalInspection = 108,
    
    // ========================================================================
    // B. Energy & Utilities / PV
    // ========================================================================
    
    /// <summary>
    /// Photovoltaic park inspection (large solar farms)
    /// </summary>
    PhotovoltaicParkInspection = 201,
    
    /// <summary>
    /// Solar panel cleaning (rooftop or ground-mounted)
    /// </summary>
    SolarPanelCleaning = 202,
    
    /// <summary>
    /// Rooftop solar inspection (residential/commercial buildings)
    /// </summary>
    RooftopSolarInspection = 203,
    
    /// <summary>
    /// Wind turbine blade inspection
    /// </summary>
    WindTurbineBladeInspection = 204,
    
    /// <summary>
    /// Powerline corridor inspection (transmission lines)
    /// </summary>
    PowerlineCorridorInspection = 205,
    
    /// <summary>
    /// Electrical substation inspection
    /// </summary>
    ElectricalSubstationInspection = 206,
    
    /// <summary>
    /// Pipeline corridor inspection (oil, gas, water)
    /// </summary>
    PipelineCorridorInspection = 207,
    
    /// <summary>
    /// Industrial plant infrastructure inspection
    /// </summary>
    IndustrialPlantInfrastructureInspection = 208,
    
    // ========================================================================
    // C. Infrastructure & Transport
    // ========================================================================
    
    /// <summary>
    /// Bridge structural inspection
    /// </summary>
    BridgeStructuralInspection = 301,
    
    /// <summary>
    /// Road corridor mapping
    /// </summary>
    RoadCorridorMapping = 302,
    
    /// <summary>
    /// Highway signage inspection
    /// </summary>
    HighwaySignageInspection = 303,
    
    /// <summary>
    /// Railway corridor inspection
    /// </summary>
    RailwayCorridorInspection = 304,
    
    /// <summary>
    /// Port infrastructure inspection
    /// </summary>
    PortInfrastructureInspection = 305,
    
    /// <summary>
    /// Dam and reservoir inspection
    /// </summary>
    DamAndReservoirInspection = 306,
    
    /// <summary>
    /// Urban utility network inspection (underground/overhead)
    /// </summary>
    UrbanUtilityNetworkInspection = 307,
    
    // ========================================================================
    // D. Agriculture & Forestry
    // ========================================================================
    
    /// <summary>
    /// Agricultural field mapping (NDVI, multispectral)
    /// </summary>
    AgriculturalFieldMapping = 401,
    
    /// <summary>
    /// Crop health monitoring (disease detection, stress analysis)
    /// </summary>
    CropHealthMonitoring = 402,
    
    /// <summary>
    /// Orchard inspection (fruit trees)
    /// </summary>
    OrchardInspection = 403,
    
    /// <summary>
    /// Vineyard inspection (grape vines)
    /// </summary>
    VineyardInspection = 404,
    
    /// <summary>
    /// Forestry health monitoring (tree canopy, disease)
    /// </summary>
    ForestryHealthMonitoring = 405,
    
    /// <summary>
    /// Rural property survey (land parcels)
    /// </summary>
    RuralPropertySurvey = 406,
    
    /// <summary>
    /// Irrigation infrastructure inspection
    /// </summary>
    IrrigationInfrastructureInspection = 407,
    
    // ========================================================================
    // E. Survey & Construction
    // ========================================================================
    
    /// <summary>
    /// Topographic survey (terrain elevation mapping)
    /// </summary>
    TopographicSurvey = 501,
    
    /// <summary>
    /// Construction progress monitoring
    /// </summary>
    ConstructionProgressMonitoring = 502,
    
    /// <summary>
    /// Stockpile volume measurement
    /// </summary>
    StockpileVolumeMeasurement = 503,
    
    /// <summary>
    /// Quarry/mining survey
    /// </summary>
    QuarryMiningSurvey = 504,
    
    /// <summary>
    /// 3D building model capture (photogrammetry)
    /// </summary>
    ThreeDBuildingModelCapture = 505,
    
    /// <summary>
    /// Generic corridor mapping (roads, trails, boundaries)
    /// </summary>
    GenericCorridorMapping = 506,
    
    /// <summary>
    /// Land development survey
    /// </summary>
    LandDevelopmentSurvey = 507,
    
    // ========================================================================
    // F. Public Safety & Assessment (mapping only, NOT tactical)
    // ========================================================================
    
    /// <summary>
    /// Search area mapping for SAR (Search and Rescue)
    /// </summary>
    SearchAreaMapping_SAR = 601,
    
    /// <summary>
    /// Disaster damage assessment (post-earthquake, storm)
    /// </summary>
    DisasterDamageAssessment = 602,
    
    /// <summary>
    /// Flood extent mapping
    /// </summary>
    FloodExtentMapping = 603,
    
    /// <summary>
    /// Post-fire assessment (burned area mapping)
    /// </summary>
    PostFireAssessment = 604,
    
    /// <summary>
    /// Critical infrastructure security survey
    /// </summary>
    CriticalInfrastructureSecuritySurvey = 605,
    
    // ========================================================================
    // G. Media & Commercial
    // ========================================================================
    
    /// <summary>
    /// Real estate marketing imaging
    /// </summary>
    RealEstateMarketingImaging = 701,
    
    /// <summary>
    /// Event aerial filming
    /// </summary>
    EventAerialFilming = 702,
    
    /// <summary>
    /// Tourism location aerial imaging
    /// </summary>
    TourismLocationAerialImaging = 703,
    
    // ========================================================================
    // H. Training & Test
    // ========================================================================
    
    /// <summary>
    /// Training flight VLOS (low-risk environment)
    /// </summary>
    TrainingFlightVLOS = 801,
    
    /// <summary>
    /// Training flight BVLOS (low-risk rural environment)
    /// </summary>
    TrainingFlightBVLOS_LowRisk = 802,
    
    /// <summary>
    /// System functional test flight
    /// </summary>
    SystemFunctionalTestFlight = 803
}
