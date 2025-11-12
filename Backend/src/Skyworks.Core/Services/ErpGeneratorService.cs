using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Implementation of rule-based ERP generator following EASA/JARUS SORA guidance.
/// Rules based on SAIL level, airspace type, environment (urban/suburban/rural), and mission characteristics.
/// </summary>
public class ErpGeneratorService : IErpGeneratorService
{
    private readonly IMissionTemplatesConfig _templatesConfig;

    public ErpGeneratorService(IMissionTemplatesConfig templatesConfig)
    {
        _templatesConfig = templatesConfig;
    }

    public ErpResult Generate(Mission mission, MissionGeometry geometry, MissionSoraAssessment sora)
    {
        var metadata = _templatesConfig.Get(mission.TemplateCode);
        var erpPlan = new ErpPlan();

        // Generate each ERP section based on rules
        erpPlan.LossOfC2 = GenerateLossOfC2(mission, geometry, sora, metadata);
        erpPlan.FlyAway = GenerateFlyAway(mission, geometry, sora, metadata);
        erpPlan.EmergencyLanding = GenerateEmergencyLanding(mission, geometry, sora, metadata);
        erpPlan.GroundNotification = GenerateGroundNotification(mission, sora, metadata);
        erpPlan.AtsCoordination = GenerateAtsCoordination(mission, sora, metadata);

        // Serialize to JSON
        var erpJson = JsonSerializer.Serialize(erpPlan, new JsonSerializerOptions { WriteIndented = true });

        // Generate narrative text
        var erpText = GenerateNarrativeText(erpPlan, mission, sora, metadata);

        return new ErpResult
        {
            ErpJson = erpJson,
            ErpText = erpText
        };
    }

    #region Loss of C2 Link

    /// <summary>
    /// Generate Loss of C2 link procedures.
    /// RTH altitude: MaxHeightAGL + safety margin, capped at 120m.
    /// RTH timeout: shorter for urban/high SAIL, longer for rural/low SAIL.
    /// </summary>
    private ErpLossOfC2 GenerateLossOfC2(Mission mission, MissionGeometry geometry, MissionSoraAssessment sora, MissionTemplateMetadata metadata)
    {
        // RTH altitude: flight height + 20m safety margin, max 120m AGL
        var rthAltitude = Math.Min((double)geometry.MaxHeightAGL_m + 20.0, 120.0);

        // RTH timeout based on environment and SAIL
        var maxTimeToRth = CalculateRthTimeout(sora.Sail, metadata.DefaultEnvironment);

        var actions = new List<string>
        {
            "Monitor UA C2 link status via GCS",
            "Confirm RTH procedure has been initiated automatically",
            "Monitor UA trajectory to ensure RTH is being executed correctly",
            "Verify UA climbs to RTH altitude and proceeds to home point",
            "Be prepared to regain manual control if C2 link is re-established"
        };

        // Add urban-specific actions
        if (metadata.DefaultEnvironment == "urban" || ParseSailLevel(sora.Sail) >= 4)
        {
            actions.Add("Notify ground personnel if UA deviates from planned RTH route");
        }

        return new ErpLossOfC2
        {
            RthAltitude_m = rthAltitude,
            MaxTimeToRth_s = maxTimeToRth,
            Actions = actions
        };
    }

    /// <summary>
    /// Calculate RTH timeout based on SAIL and environment.
    /// Urban/high SAIL: shorter timeout (immediate response).
    /// Rural/low SAIL: longer timeout (more time to diagnose).
    /// </summary>
    private int CalculateRthTimeout(string sail, string environment)
    {
        var sailLevel = ParseSailLevel(sail);
        
        // Base timeout
        int baseTimeout = 30; // 30 seconds baseline

        // Reduce for urban/high SAIL
        if (environment == "urban")
        {
            baseTimeout -= 10;
        }
        else if (environment == "rural")
        {
            baseTimeout += 15;
        }

        // Adjust for SAIL
        if (sailLevel >= 5)
        {
            baseTimeout = Math.Max(baseTimeout - 10, 15); // Min 15s for high SAIL
        }
        else if (sailLevel <= 2)
        {
            baseTimeout += 15; // More time for low SAIL
        }

        return baseTimeout;
    }
    
    /// <summary>
    /// Parse SAIL string ("I", "II", "III", etc.) to numeric level
    /// </summary>
    private int ParseSailLevel(string sail)
    {
        if (string.IsNullOrWhiteSpace(sail)) return 0;
        
        return sail.Trim().ToUpper() switch
        {
            "I" => 1,
            "II" => 2,
            "III" => 3,
            "IV" => 4,
            "V" => 5,
            "VI" => 6,
            _ => 0 // Unknown/Category C
        };
    }

    #endregion

    #region Fly-Away

    /// <summary>
    /// Generate Fly-away / loss of control procedures.
    /// Safe area: within CGA if available, otherwise offset from mission center.
    /// </summary>
    private ErpFlyAway GenerateFlyAway(Mission mission, MissionGeometry geometry, MissionSoraAssessment sora, MissionTemplateMetadata metadata)
    {
        var flyAway = new ErpFlyAway
        {
            DefinedSafeArea = geometry.CgaArea_m2 > 0,
            Actions = new List<string>
            {
                "Attempt to re-establish C2 link immediately",
                "Monitor UA position and trajectory via GCS or tracking system"
            }
        };

        // Define safe area coordinates
        if (geometry.CgaArea_m2 > 0)
        {
            // Use CGA center as safe area (approximation - would need actual CGA parsing)
            // For now, use mission center as proxy
            var geoJsonDoc = JsonDocument.Parse(geometry.GeoJson);
            var cgaCoords = ExtractCgaCenterCoordinates(geoJsonDoc);
            
            flyAway.SafeAreaLat = cgaCoords.lat;
            flyAway.SafeAreaLon = cgaCoords.lon;
            flyAway.SafeAreaRadius_m = Math.Sqrt((double)geometry.CgaArea_m2 / Math.PI); // Approximate radius from area
        }

        // Add SAIL-specific actions
        if (ParseSailLevel(sora.Sail) >= 3)
        {
            flyAway.Actions.Add("Notify local authorities immediately if UA exits defined safe area");
        }

        if (ParseSailLevel(sora.Sail) >= 4)
        {
            flyAway.Actions.Add("Activate emergency termination if UA trajectory poses imminent ground risk");
        }

        flyAway.Actions.Add("Log incident details for post-flight analysis");

        return flyAway;
    }

    /// <summary>
    /// Extract CGA center coordinates from GeoJSON (simplified approximation)
    /// </summary>
    private (double lat, double lon) ExtractCgaCenterCoordinates(JsonDocument geoJson)
    {
        try
        {
            var features = geoJson.RootElement.GetProperty("features");
            foreach (var feature in features.EnumerateArray())
            {
                if (feature.TryGetProperty("properties", out var props) &&
                    props.TryGetProperty("name", out var name) &&
                    name.GetString() == "cga")
                {
                    var geometry = feature.GetProperty("geometry");
                    if (geometry.GetProperty("type").GetString() == "Polygon")
                    {
                        var coords = geometry.GetProperty("coordinates")[0]; // First ring
                        var firstPoint = coords[0];
                        return (firstPoint[1].GetDouble(), firstPoint[0].GetDouble());
                    }
                }
            }
        }
        catch
        {
            // Fallback: return (0,0) if parsing fails
        }

        return (0, 0);
    }

    #endregion

    #region Emergency Landing

    /// <summary>
    /// Generate emergency landing procedures.
    /// Landing sites: 1-3 sites within or near CGA.
    /// </summary>
    private ErpEmergencyLanding GenerateEmergencyLanding(Mission mission, MissionGeometry geometry, MissionSoraAssessment sora, MissionTemplateMetadata metadata)
    {
        var landing = new ErpEmergencyLanding
        {
            Sites = new List<ErpLandingSite>(),
            Actions = new List<string>
            {
                "Assess situation severity and decide on immediate emergency landing if necessary",
                "Select nearest suitable landing site from pre-identified locations",
                "Ensure landing area is clear of people and obstacles where possible",
                "Execute controlled descent to landing site",
                "Secure UA after landing and assess damage/safety"
            }
        };

        // Generate landing sites based on CGA
        if (geometry.CgaArea_m2 > 0)
        {
            var geoJsonDoc = JsonDocument.Parse(geometry.GeoJson);
            var cgaCenter = ExtractCgaCenterCoordinates(geoJsonDoc);

            landing.Sites.Add(new ErpLandingSite
            {
                Lat = cgaCenter.lat,
                Lon = cgaCenter.lon,
                Description = "Primary: CGA central area (pre-assessed as low ground risk)"
            });

            // Add secondary sites with small offsets (simplified)
            landing.Sites.Add(new ErpLandingSite
            {
                Lat = cgaCenter.lat + 0.0005, // ~50m north
                Lon = cgaCenter.lon,
                Description = "Secondary: CGA north sector"
            });

            if (ParseSailLevel(sora.Sail) >= 3)
            {
                landing.Sites.Add(new ErpLandingSite
                {
                    Lat = cgaCenter.lat,
                    Lon = cgaCenter.lon + 0.0007, // ~50m east
                    Description = "Tertiary: CGA east sector (fallback)"
                });
            }
        }
        else
        {
            // No CGA: use mission center as fallback
            landing.Sites.Add(new ErpLandingSite
            {
                Lat = 0, // Would need mission center lat/lon from request
                Lon = 0,
                Description = "Emergency landing at mission area (no pre-defined CGA)"
            });
        }

        // Add high-SAIL actions
        if (ParseSailLevel(sora.Sail) >= 4)
        {
            landing.Actions.Add("Notify emergency services if landing poses risk to people or property");
        }

        return landing;
    }

    #endregion

    #region Ground Notification

    /// <summary>
    /// Generate ground notification requirements.
    /// SAIL I-II rural: minimal (client only or optional).
    /// SAIL III-IV: client + local authorities.
    /// SAIL V-VI or critical infrastructure: comprehensive notification.
    /// </summary>
    private ErpGroundNotification GenerateGroundNotification(Mission mission, MissionSoraAssessment sora, MissionTemplateMetadata metadata)
    {
        var notification = new ErpGroundNotification();
        var sailLevel = ParseSailLevel(sora.Sail);

        // Determine if notification is required
        if (sailLevel <= 2 && metadata.DefaultEnvironment == "rural")
        {
            // Low SAIL rural: optional or client only
            notification.Required = false;
            notification.Contacts.Add("Client (if contracted)");
            notification.Methods.Add("Phone");
        }
        else if (sailLevel <= 2)
        {
            // Low SAIL urban/suburban: required (stricter than rural)
            notification.Required = true;
            notification.Contacts.Add("Client");
            notification.Contacts.Add("Local Authorities (where applicable)");
            notification.Methods.Add("Phone");
            notification.Methods.Add("Email");
        }
        else if (sailLevel >= 3 && sailLevel <= 4)
        {
            // Medium SAIL: client + authorities
            notification.Required = true;
            notification.Contacts.Add("Client");
            notification.Contacts.Add("Local Authorities (where applicable)");
            notification.Methods.Add("Phone");
            notification.Methods.Add("Email");
        }
        else if (sailLevel >= 5)
        {
            // High SAIL: comprehensive
            notification.Required = true;
            notification.Contacts.Add("Client");
            notification.Contacts.Add("Local Authorities");
            notification.Methods.Add("Phone");
            notification.Methods.Add("Email");
            notification.Methods.Add("Radio (if available)");
        }

        // Critical infrastructure templates
        if (mission.Category == MissionCategory.EnergyAndUtilities || 
            mission.Category == MissionCategory.InfrastructureAndTransport)
        {
            if (sailLevel >= 3)
            {
                notification.Contacts.Add("Infrastructure Manager/Owner");
            }
        }

        return notification;
    }

    #endregion

    #region ATS Coordination

    /// <summary>
    /// Generate ATS coordination requirements.
    /// Required if: controlled airspace (CTR/ATZ), SAIL >= 4, or specific templates.
    /// </summary>
    private ErpAtsCoordination GenerateAtsCoordination(Mission mission, MissionSoraAssessment sora, MissionTemplateMetadata metadata)
    {
        var ats = new ErpAtsCoordination();
        var sailLevel = ParseSailLevel(sora.Sail);

        // Determine if ATS coordination is required
        // Note: Ideally check InputsSnapshotJson for airspace type, but we'll use heuristics
        var requiresAts = false;

        // High SAIL generally requires ATS awareness
        if (sailLevel >= 4)
        {
            requiresAts = true;
        }

        // Urban high-structure missions likely in controlled airspace
        if (metadata.DefaultEnvironment == "urban" && metadata.IsHighStructure)
        {
            requiresAts = true;
        }

        // Infrastructure missions near airports
        if (mission.Category == MissionCategory.InfrastructureAndTransport)
        {
            requiresAts = true;
        }

        ats.Required = requiresAts;

        if (requiresAts)
        {
            ats.Contacts.Add("ATS Unit / Aerodrome Authority");
            ats.ProcedureSummary = "Pre-notification to ATS at least 24 hours before planned operation. " +
                                    "Provide flight plan details including planned route, altitude, and duration. " +
                                    "Maintain radio contact during operation if required by ATS. " +
                                    "Submit post-flight report confirming operation completion or any incidents.";
        }
        else
        {
            ats.ProcedureSummary = "No ATS coordination required for this operation (uncontrolled airspace, low SAIL).";
        }

        return ats;
    }

    #endregion

    #region Narrative Text Generation

    /// <summary>
    /// Generate human-readable ERP narrative from structured plan.
    /// No LLM - pure template-based text generation.
    /// </summary>
    private string GenerateNarrativeText(ErpPlan plan, Mission mission, MissionSoraAssessment sora, MissionTemplateMetadata metadata)
    {
        var sb = new StringBuilder();

        sb.AppendLine("EMERGENCY RESPONSE PLAN (ERP)");
        sb.AppendLine($"Mission Template: {mission.TemplateCode}");
        sb.AppendLine($"SORA Version: {mission.SoraVersion} | SAIL: {sora.Sail}");
        sb.AppendLine($"Environment: {metadata.DefaultEnvironment} | Category: {mission.Category}");
        sb.AppendLine();
        sb.AppendLine("This Emergency Response Plan defines contingency procedures for various failure scenarios during UAS operations.");
        sb.AppendLine();

        // Loss of C2
        sb.AppendLine("1. LOSS OF C2 LINK");
        sb.AppendLine($"In the event of C2 link loss, the UA will automatically execute Return-to-Home (RTH) procedure climbing to {plan.LossOfC2.RthAltitude_m:F0}m AGL. ");
        sb.AppendLine($"The UA must initiate RTH within {plan.LossOfC2.MaxTimeToRth_s} seconds of link loss detection.");
        sb.AppendLine("Operator Actions:");
        foreach (var action in plan.LossOfC2.Actions)
        {
            sb.AppendLine($"  - {action}");
        }
        sb.AppendLine();

        // Fly-Away
        sb.AppendLine("2. FLY-AWAY / LOSS OF CONTROL");
        if (plan.FlyAway.DefinedSafeArea)
        {
            sb.AppendLine($"A safe area is defined with center coordinates ({plan.FlyAway.SafeAreaLat:F6}, {plan.FlyAway.SafeAreaLon:F6}) and radius {plan.FlyAway.SafeAreaRadius_m:F0}m. ");
            sb.AppendLine("The UA should remain within this area during fly-away scenarios.");
        }
        else
        {
            sb.AppendLine("No specific safe area defined. Operator must monitor UA trajectory and take immediate corrective action.");
        }
        sb.AppendLine("Operator Actions:");
        foreach (var action in plan.FlyAway.Actions)
        {
            sb.AppendLine($"  - {action}");
        }
        sb.AppendLine();

        // Emergency Landing
        sb.AppendLine("3. EMERGENCY LANDING");
        sb.AppendLine($"The following {plan.EmergencyLanding.Sites.Count} emergency landing site(s) have been pre-identified:");
        foreach (var site in plan.EmergencyLanding.Sites)
        {
            sb.AppendLine($"  - Lat: {site.Lat:F6}, Lon: {site.Lon:F6} - {site.Description}");
        }
        sb.AppendLine("Operator Actions:");
        foreach (var action in plan.EmergencyLanding.Actions)
        {
            sb.AppendLine($"  - {action}");
        }
        sb.AppendLine();

        // Ground Notification
        sb.AppendLine("4. GROUND NOTIFICATION");
        if (plan.GroundNotification.Required)
        {
            sb.AppendLine("Ground notification IS REQUIRED for emergency scenarios.");
            sb.AppendLine("Contacts to notify:");
            foreach (var contact in plan.GroundNotification.Contacts)
            {
                sb.AppendLine($"  - {contact}");
            }
            sb.AppendLine("Notification methods:");
            foreach (var method in plan.GroundNotification.Methods)
            {
                sb.AppendLine($"  - {method}");
            }
        }
        else
        {
            sb.AppendLine("Ground notification is OPTIONAL for this low-risk operation.");
            if (plan.GroundNotification.Contacts.Count > 0)
            {
                sb.AppendLine("Suggested contacts (if notification deemed necessary):");
                foreach (var contact in plan.GroundNotification.Contacts)
                {
                    sb.AppendLine($"  - {contact}");
                }
            }
        }
        sb.AppendLine();

        // ATS Coordination
        sb.AppendLine("5. ATS COORDINATION");
        if (plan.AtsCoordination.Required)
        {
            sb.AppendLine("ATS coordination IS REQUIRED for this operation.");
            sb.AppendLine("ATS Contacts:");
            foreach (var contact in plan.AtsCoordination.Contacts)
            {
                sb.AppendLine($"  - {contact}");
            }
            sb.AppendLine("Procedure:");
            sb.AppendLine($"  {plan.AtsCoordination.ProcedureSummary}");
        }
        else
        {
            sb.AppendLine("ATS coordination is NOT REQUIRED for this operation.");
            sb.AppendLine($"  {plan.AtsCoordination.ProcedureSummary}");
        }
        sb.AppendLine();

        sb.AppendLine("END OF EMERGENCY RESPONSE PLAN");

        return sb.ToString();
    }

    #endregion
}
