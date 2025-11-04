using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/pdra-sts")]
public class PDRASTSController : ControllerBase
{
    private readonly IStringLocalizer<Resources.SharedResources> _localizer;

    public PDRASTSController(
        IStringLocalizer<Resources.SharedResources> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Evaluate PDRA or STS operation compliance
    /// Supports STS-01, STS-02, PDRA-S01, PDRA-S02
    /// Based on EU Regulation 2019/947 (Easy Access Rules for UAS)
    /// </summary>
    /// <param name="request">PDRA/STS evaluation request with operational parameters</param>
    /// <returns>Evaluation result with approval status and applicable SORA metrics</returns>
    [HttpPost("evaluate")]
    public ActionResult<PDRASTSEvaluationResult> EvaluatePDRASTS([FromBody] PDRASTSEvaluationRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required" });

        // Validate category
        var validCategories = new[] { "STS-01", "STS-02", "PDRA-S01", "PDRA-S02", "PDRA-G01", "PDRA-G02", "PDRA-G03" };
        if (!validCategories.Contains(request.Category))
            return BadRequest(new { error = $"Invalid category. Must be one of: {string.Join(", ", validCategories)}" });

        try
        {
            var result = new PDRASTSEvaluationResult
            {
                MissionId = request.MissionId,
                Category = request.Category,
                Approved = false
            };

            // Validate based on category-specific requirements
            var validation = ValidateCategoryRequirements(request);
            if (!validation.IsValid)
            {
                result.Approved = false;
                result.RejectionReason = validation.Reason;
                return Ok(result);
            }

            // Authoritative fixed metrics per scenario (PDRA/STS tables)
            var (grcResult, arcResult, sailResult) = GetFixedMetricsForScenario(request.Category);

            result.Approved = true;
            result.Grc = grcResult;
            result.Arc = arcResult;
            result.Sail = sailResult;

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get PDRA/STS information and supported categories
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetInfo()
    {
        return Ok(new
        {
            name = "PDRA/STS Evaluation Service",
            description = "Evaluates compliance with Standard Scenarios (STS) and Pre-Defined Risk Assessments (PDRA) per EU Regulation 2019/947",
            supportedCategories = new
            {
                sts = new[]
                {
                    new { id = "STS-01", name = "VLOS over controlled ground area", classRequired = "C5" },
                    new { id = "STS-02", name = "BVLOS with airspace observers", classRequired = "C6" }
                },
                pdra = new[]
                {
                    new { id = "PDRA-S01", name = "VLOS (similar to STS-01, custom UAS)", classRequired = "None" },
                    new { id = "PDRA-S02", name = "BVLOS (similar to STS-02, custom UAS)", classRequired = "None" },
                    new { id = "PDRA-G01", name = "Aerial survey BVLOS", classRequired = "None" },
                    new { id = "PDRA-G02", name = "Aerial survey BVLOS extended", classRequired = "None" },
                    new { id = "PDRA-G03", name = "BVLOS low altitude", classRequired = "None" }
                }
            },
            endpoints = new
            {
                evaluate = "POST /api/pdra-sts/evaluate",
                info = "GET /api/pdra-sts/info"
            }
        });
    }

    #region Private Validation and Calculation Methods

    private (bool IsValid, string Reason) ValidateCategoryRequirements(PDRASTSEvaluationRequest request)
    {
        switch (request.Category)
        {
            case "STS-01":
                return ValidateSTS01(request);
            case "STS-02":
                return ValidateSTS02(request);
            case "PDRA-S01":
                return ValidatePDRAS01(request);
            case "PDRA-S02":
                return ValidatePDRAS02(request);
            case "PDRA-G01":
            case "PDRA-G02":
            case "PDRA-G03":
                return ValidatePDRAG(request);
            default:
                return (false, $"Unknown category: {request.Category}");
        }
    }

    private (bool IsValid, string Reason) ValidateSTS01(PDRASTSEvaluationRequest request)
    {
        // STS-01: VLOS over controlled ground area
        // Requirements: Class C5, max 120m height, max 5m/s speed, controlled ground area

        if (request.UasClass != "C5")
            return (false, "STS-01 requires UAS Class C5 marking");

        if (!request.ControlledGroundArea.GetValueOrDefault(false))
            return (false, "STS-01 requires a controlled ground area");

        if (request.MaxHeight.GetValueOrDefault(0) > 120)
            return (false, "STS-01 max height is 120m AGL");

        if (request.MaxSpeed.GetValueOrDefault(0) > 5)
            return (false, "STS-01 max speed is 5 m/s");

        if (!request.Vlos.GetValueOrDefault(false))
            return (false, "STS-01 requires VLOS operation");

        return (true, string.Empty);
    }

    private (bool IsValid, string Reason) ValidateSTS02(PDRASTSEvaluationRequest request)
    {
        // STS-02: BVLOS with airspace observers
        // Requirements: Class C6, max 120m height, 1-2km range, min 5km visibility, sparsely populated

        if (request.UasClass != "C6")
            return (false, "STS-02 requires UAS Class C6 marking");

        if (request.MaxHeight.GetValueOrDefault(0) > 120)
            return (false, "STS-02 max height is 120m AGL");

        if (request.FlightRange.GetValueOrDefault(0) > 2000)
            return (false, "STS-02 max range is 2km");

        if (request.MeteorologicalVisibility.GetValueOrDefault(0) < 5)
            return (false, "STS-02 requires minimum 5km meteorological visibility");

        if (!request.LaunchRecoveryVlos.GetValueOrDefault(false))
            return (false, "STS-02 requires VLOS during launch and recovery");

        // Sparsely populated check
        if (request.PopulationDensity.GetValueOrDefault(double.MaxValue) >= 500)
            return (false, "STS-02 requires sparsely populated area (<500 people/km²)");

        return (true, string.Empty);
    }

    private (bool IsValid, string Reason) ValidatePDRAS01(PDRASTSEvaluationRequest request)
    {
        // PDRA-S01: Similar to STS-01 but allows custom UAS (no Class C5 required)
        // Requirements: max 3m dimension, max 150m height, controlled ground area, VLOS

        if (request.MaxDimension.GetValueOrDefault(0) > 3)
            return (false, "PDRA-S01 max characteristic dimension is 3m");

        if (request.MaxHeight.GetValueOrDefault(0) > 150)
            return (false, "PDRA-S01 max height is 150m AGL");

        if (!request.Vlos.GetValueOrDefault(false))
            return (false, "PDRA-S01 requires VLOS operation");

        if (request.AutomaticMode.GetValueOrDefault(false))
            return (false, "PDRA-S01 requires pilot control at all times (no fully automatic mode)");

        return (true, string.Empty);
    }

    private (bool IsValid, string Reason) ValidatePDRAS02(PDRASTSEvaluationRequest request)
    {
        // PDRA-S02: Similar to STS-02 but allows custom UAS (no Class C6 required)
        // Requirements: max 3m dimension, max 25kg MTOM, max 150m height, sparsely populated

        if (request.MaxDimension.GetValueOrDefault(0) > 3)
            return (false, "PDRA-S02 max characteristic dimension is 3m");

        if (request.Mtom.GetValueOrDefault(0) > 25)
            return (false, "PDRA-S02 max MTOM is 25kg");

        if (request.MaxHeight.GetValueOrDefault(0) > 150)
            return (false, "PDRA-S02 max height is 150m AGL");

        // Sparsely populated check
        if (request.PopulationDensity.GetValueOrDefault(double.MaxValue) >= 500)
            return (false, "PDRA-S02 requires sparsely populated area (<500 people/km²)");

        // Check airspace observers if BVLOS without them (1km limit)
        if (request.FlightRange.GetValueOrDefault(0) > 1000 && request.AirspaceObservers.GetValueOrDefault(0) == 0)
            return (false, "PDRA-S02 BVLOS beyond 1km requires airspace observers");

        return (true, string.Empty);
    }

    private (bool IsValid, string Reason) ValidatePDRAG(PDRASTSEvaluationRequest request)
    {
        // PDRA-G series: Aerial survey operations
        // General requirements: sparsely populated, specific operational constraints per variant

        if (request.PopulationDensity.GetValueOrDefault(double.MaxValue) >= 500)
            return (false, $"{request.Category} requires sparsely populated area (<500 people/km²)");

        // Additional category-specific validations could be added here

        return (true, string.Empty);
    }

    private (string Grc, string Arc, string Sail) GetFixedMetricsForScenario(string category)
    {
        // Fixed, scenario-authoritative values from PDRA/STS official tables
        // CSV provided by user and EASA/JARUS references
        return category switch
        {
            // STS
            "STS-01" => ("GRC_3", "ARC_b", "SAIL_II"),
            "STS-02" => ("GRC_3", "ARC_b", "SAIL_II"),

            // PDRA S-series
            "PDRA-S01" => ("GRC_3", "ARC_b", "SAIL_II"),
            "PDRA-S02" => ("GRC_3", "ARC_b", "SAIL_II"),

            // PDRA G-series
            "PDRA-G01" => ("GRC_3", "ARC_b", "SAIL_II"),
            "PDRA-G02" => ("GRC_3", "ARC_a", "SAIL_II"),
            "PDRA-G03" => ("GRC_3", "ARC_a", "SAIL_II"),

            _ => ("GRC_3", "ARC_b", "SAIL_II")
        };
    }

    #endregion
}

#region Request and Response Models

public class PDRASTSEvaluationRequest
{
    public string Category { get; set; } = string.Empty;
    public string MissionId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // UAS specifications
    public string? UasClass { get; set; }
    public double? MaxDimension { get; set; }
    public double? Mtom { get; set; }
    
    // Operational parameters
    public bool? ControlledGroundArea { get; set; }
    public double? GroundRiskBuffer { get; set; }
    public double? PopulationDensity { get; set; }
    
    // Flight parameters
    public bool? Vlos { get; set; }
    public double? MaxHeight { get; set; }
    public double? MaxSpeed { get; set; }
    public double? FlightRange { get; set; }
    
    // Safety features
    public int? AirspaceObservers { get; set; }
    public double? MeteorologicalVisibility { get; set; }
    public bool? LaunchRecoveryVlos { get; set; }
    
    // Airspace information
    public bool? ControlledAirspace { get; set; }
    public bool? AtcCoordination { get; set; }
    
    // Operational features
    public bool? TetheredOperation { get; set; }
    public double? TetherLength { get; set; }
    public bool? AutomaticMode { get; set; }
    
    public Dictionary<string, string>? AdditionalRequirements { get; set; }
}

public class PDRASTSEvaluationResult
{
    public string MissionId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Approved { get; set; }
    public string? Grc { get; set; }
    public string? Arc { get; set; }
    public string? Sail { get; set; }
    public string? RejectionReason { get; set; }
}

#endregion
