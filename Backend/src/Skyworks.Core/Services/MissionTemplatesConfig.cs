using System;
using System.Collections.Generic;
using System.Linq;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// In-memory configuration provider for all 48 mission templates.
/// Provides default metadata for auto-population of mission parameters.
/// </summary>
public class MissionTemplatesConfig : IMissionTemplatesConfig
{
    private readonly Dictionary<MissionTemplateCode, MissionTemplateMetadata> _templates;

    public MissionTemplatesConfig()
    {
        _templates = BuildTemplateMetadata().ToDictionary(t => t.Code);
    }

    public MissionTemplateMetadata Get(MissionTemplateCode code)
    {
        if (!_templates.TryGetValue(code, out var metadata))
        {
            throw new KeyNotFoundException($"Mission template not found: {code}");
        }
        return metadata;
    }

    public IReadOnlyList<MissionTemplateMetadata> GetAll()
    {
        return _templates.Values.ToList().AsReadOnly();
    }

    private static List<MissionTemplateMetadata> BuildTemplateMetadata()
    {
        return new List<MissionTemplateMetadata>
        {
            // ========================================================================
            // A. Buildings & Facades (8 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.FacadeCleaning,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.FacadeInspection,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.HighRiseFacadeInspection,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.WindowCleaningHighRise,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RoofInspection_Flat,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Roof,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RoofInspection_Pitched,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Roof,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.ChimneyInspection,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.BuildingEnvelopeThermalInspection,
                Category = MissionCategory.BuildingsAndFacades,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            // ========================================================================
            // B. Energy & Utilities / PV (8 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.PhotovoltaicParkInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.SolarPanelCleaning,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Roof,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RooftopSolarInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Roof,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.WindTurbineBladeInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.PowerlineCorridorInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.ElectricalSubstationInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.PipelineCorridorInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.IndustrialPlantInfrastructureInspection,
                Category = MissionCategory.EnergyAndUtilities,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            // ========================================================================
            // C. Infrastructure & Transport (7 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.BridgeStructuralInspection,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RoadCorridorMapping,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.HighwaySignageInspection,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RailwayCorridorInspection,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.PortInfrastructureInspection,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.DamAndReservoirInspection,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.UrbanUtilityNetworkInspection,
                Category = MissionCategory.InfrastructureAndTransport,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            // ========================================================================
            // D. Agriculture & Forestry (7 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.AgriculturalFieldMapping,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.CropHealthMonitoring,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.OrchardInspection,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.VineyardInspection,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.ForestryHealthMonitoring,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RuralPropertySurvey,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.IrrigationInfrastructureInspection,
                Category = MissionCategory.AgricultureAndForestry,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            // ========================================================================
            // E. Survey & Construction (7 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.TopographicSurvey,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.ConstructionProgressMonitoring,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.StockpileVolumeMeasurement,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.QuarryMiningSurvey,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.ThreeDBuildingModelCapture,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = true
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.GenericCorridorMapping,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Linear,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = true,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.LandDevelopmentSurvey,
                Category = MissionCategory.SurveyAndConstruction,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            // ========================================================================
            // F. Public Safety & Assessment (5 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.SearchAreaMapping_SAR,
                Category = MissionCategory.PublicSafetyAndAssessment,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.DisasterDamageAssessment,
                Category = MissionCategory.PublicSafetyAndAssessment,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.FloodExtentMapping,
                Category = MissionCategory.PublicSafetyAndAssessment,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.PostFireAssessment,
                Category = MissionCategory.PublicSafetyAndAssessment,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.CriticalInfrastructureSecuritySurvey,
                Category = MissionCategory.PublicSafetyAndAssessment,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            // ========================================================================
            // G. Media & Commercial (3 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.RealEstateMarketingImaging,
                Category = MissionCategory.MediaAndCommercial,
                DefaultMissionType = MissionType.Facade,
                DefaultEnvironment = "suburban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.EventAerialFilming,
                Category = MissionCategory.MediaAndCommercial,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "urban",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.TourismLocationAerialImaging,
                Category = MissionCategory.MediaAndCommercial,
                DefaultMissionType = MissionType.Solar,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            // ========================================================================
            // H. Training & Test (3 templates)
            // ========================================================================
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.TrainingFlightVLOS,
                Category = MissionCategory.TrainingAndTest,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.TrainingFlightBVLOS_LowRisk,
                Category = MissionCategory.TrainingAndTest,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = true,
                IsHighStructure = false
            },
            
            new MissionTemplateMetadata
            {
                Code = MissionTemplateCode.SystemFunctionalTestFlight,
                Category = MissionCategory.TrainingAndTest,
                DefaultMissionType = MissionType.Agriculture,
                DefaultEnvironment = "rural",
                DefaultSoraVersion = "2.5",
                IsLinear = false,
                IsLargeArea = false,
                IsHighStructure = false
            }
        };
    }
}
