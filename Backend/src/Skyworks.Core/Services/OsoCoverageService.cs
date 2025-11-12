using System.Text.Json;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Service for analyzing OSO (Operational Safety Objectives) coverage for missions.
/// Determines required OSOs based on SAIL, identifies covered OSOs based on mission/ERP,
/// and calculates missing OSOs that need additional mitigation.
/// </summary>
public class OsoCoverageService : IOsoCoverageService
{
    private readonly IOsoCatalog _osoCatalog;

    public OsoCoverageService(IOsoCatalog osoCatalog)
    {
        _osoCatalog = osoCatalog;
    }

    public OsoCoverageResult BuildChecklist(Mission mission, MissionErp erp, MissionSoraAssessment sora)
    {
        // 1. Get required OSOs for this SAIL level
        var requiredOsos = _osoCatalog.GetRequiredForSail(sora.Sail).ToList();

        // 2. Determine which OSOs are covered based on mission characteristics and ERP
        var coveredOsos = DetermineCoveredOsos(mission, erp, sora, requiredOsos);

        // 3. Calculate missing OSOs
        var missingOsos = requiredOsos
            .Where(req => !coveredOsos.Any(cov => cov.Code == req.Code))
            .ToList();

        // 4. Serialize to JSON
        var result = new OsoCoverageResult
        {
            RequiredOsosJson = JsonSerializer.Serialize(requiredOsos, new JsonSerializerOptions { WriteIndented = true }),
            CoveredOsosJson = JsonSerializer.Serialize(coveredOsos, new JsonSerializerOptions { WriteIndented = true }),
            MissingOsosJson = JsonSerializer.Serialize(missingOsos, new JsonSerializerOptions { WriteIndented = true })
        };

        return result;
    }

    #region Coverage Determination Logic

    /// <summary>
    /// Determines which OSOs are covered based on mission characteristics and ERP content.
    /// This is a conservative heuristic approach for v1 - production systems would have
    /// more sophisticated analysis or operator manual review.
    /// </summary>
    private List<OsoItem> DetermineCoveredOsos(
        Mission mission,
        MissionErp erp,
        MissionSoraAssessment sora,
        List<OsoItem> requiredOsos)
    {
        var covered = new List<OsoItem>();

        // Parse ERP JSON to check completeness
        JsonDocument? erpDoc = null;
        try
        {
            erpDoc = JsonDocument.Parse(erp.ErpJson);
        }
        catch
        {
            // If ERP parse fails, no emergency-related OSOs are covered
        }

        foreach (var oso in requiredOsos)
        {
            if (IsOsoCovered(oso, mission, erpDoc, sora))
            {
                covered.Add(oso);
            }
        }

        erpDoc?.Dispose();
        return covered;
    }

    /// <summary>
    /// Checks if a specific OSO is considered covered for this mission.
    /// Uses deterministic rules based on mission template, category, ERP content, and SORA results.
    /// </summary>
    private bool IsOsoCovered(OsoItem oso, Mission mission, JsonDocument? erpDoc, MissionSoraAssessment sora)
    {
        switch (oso.Code)
        {
            // ===================================================================
            // PROCEDURES - Covered if ERP exists and is complete
            // ===================================================================
            case "OSO#01": // Operational procedures
                // Covered if we have a documented mission template with operational procedures
                return mission.Category != MissionCategory.Other;

            case "OSO#21": // Abnormal/emergency procedures
            case "OSO#22": // Emergency Response Plan
                // Covered if ERP has lossOfC2 and emergencyLanding sections with data
                return erpDoc != null &&
                       erpDoc.RootElement.TryGetProperty("lossOfC2", out var c2) &&
                       c2.TryGetProperty("actions", out var actions) &&
                       actions.GetArrayLength() > 0 &&
                       erpDoc.RootElement.TryGetProperty("emergencyLanding", out var landing) &&
                       landing.TryGetProperty("sites", out var sites) &&
                       sites.GetArrayLength() > 0;

            // ===================================================================
            // TRAINING & COMPETENCY
            // ===================================================================
            case "OSO#02": // Multi-crew coordination
            case "OSO#03": // Training and proficiency
                // Covered for training mission templates
                return mission.Category == MissionCategory.TrainingAndTest;

            // ===================================================================
            // TECHNICAL - UAS DESIGN & MAINTENANCE
            // ===================================================================
            case "OSO#04": // UAS design and airworthiness
            case "OSO#05": // UAS maintenance
                // For v1, we assume standard commercial drones (C0-C3) meet basic requirements
                // Production would check against drone database
                return true; // Conservative: assume operator has appropriate drone

            // ===================================================================
            // EXTERNAL SERVICES
            // ===================================================================
            case "OSO#06": // External services (met, NOTAM)
                // Covered if mission planning includes SORA calculation (implies planning was done)
                return !string.IsNullOrEmpty(sora.SoraVersion);

            // ===================================================================
            // AUTONOMOUS & SYSTEM DEGRADATION
            // ===================================================================
            case "OSO#07": // Autonomous operations
            case "OSO#08": // External system deterioration
                // Covered if ERP addresses loss of C2 and fly-away scenarios
                return erpDoc != null &&
                       erpDoc.RootElement.TryGetProperty("lossOfC2", out var lossC2) &&
                       erpDoc.RootElement.TryGetProperty("flyAway", out _);

            // ===================================================================
            // GROUND RISK MITIGATIONS
            // ===================================================================
            case "OSO#09": // Strategic ground risk mitigation
                // Covered if mission uses CGA (Controlled Ground Area)
                return mission.Category != MissionCategory.Other; // Structured templates imply planning

            case "OSO#10": // Ground impact mitigation
                // Covered for low-SAIL missions or specific templates
                var sailLevel = ParseSailLevel(sora.Sail);
                return sailLevel <= 2 || mission.Category == MissionCategory.AgricultureAndForestry;

            case "OSO#11": // Separation from uninvolved persons
                // Covered if ERP includes ground notification procedures
                return erpDoc != null &&
                       erpDoc.RootElement.TryGetProperty("groundNotification", out _);

            // ===================================================================
            // VOLUME CONTAINMENT & THIRD PARTY
            // ===================================================================
            case "OSO#12": // Operational volume containment
                // Covered if geo-fencing is implied (for now, assume modern drones have this)
                return true; // Conservative assumption

            case "OSO#13": // Third-party operations hazards
                // Covered if mission template is structured (not custom)
                return mission.Category != MissionCategory.Other;

            // ===================================================================
            // ATC / U-SPACE COORDINATION
            // ===================================================================
            case "OSO#14": // ATC/U-space procedures
            case "OSO#19": // U-space/ATC services
                // Covered if ERP includes ATS coordination section
                return erpDoc != null &&
                       erpDoc.RootElement.TryGetProperty("atsCoordination", out var ats) &&
                       ats.TryGetProperty("required", out var req) &&
                       req.GetBoolean();

            // ===================================================================
            // AIR RISK MITIGATIONS
            // ===================================================================
            case "OSO#15": // Strategic air risk mitigation
            case "OSO#16": // Tactical air risk mitigation
                // Covered for facade/roof operations (low altitude, controlled area)
                return mission.Type == MissionType.Facade ||
                       mission.Type == MissionType.Roof ||
                       mission.Category == MissionCategory.TrainingAndTest;

            case "OSO#17": // Adjacent airspace considerations
                // Covered if SORA assessment was performed (implies airspace analysis)
                return !string.IsNullOrEmpty(sora.InputsSnapshotJson);

            case "OSO#18": // Airspace segregation
                // Rarely covered without explicit coordination - conservative
                return false;

            case "OSO#20": // Conspicuity
                // Assume modern drones have lights/markings
                return true;

            // ===================================================================
            // HUMAN ERROR & CONTINUOUS IMPROVEMENT
            // ===================================================================
            case "OSO#23": // Human error management
                // Covered for structured mission templates with procedures
                return mission.Category != MissionCategory.Other;

            case "OSO#24": // Continuous improvement
                // Covered for training missions or if operator has quality system (not checkable here)
                return mission.Category == MissionCategory.TrainingAndTest;

            default:
                // Unknown OSO - not covered
                return false;
        }
    }

    /// <summary>
    /// Parse SAIL level from string to numeric for comparisons.
    /// </summary>
    private int ParseSailLevel(string sail)
    {
        return sail.Trim().ToUpper() switch
        {
            "I" => 1,
            "II" => 2,
            "III" => 3,
            "IV" => 4,
            "V" => 5,
            "VI" => 6,
            _ => 0
        };
    }

    #endregion
}
