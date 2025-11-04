using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.GRC;
using Skyworks.Core.Models.GRC.V2_0;
using Skyworks.Core.Models.GRC.V2_5;
using Skyworks.Core.Models.SAIL;
using System.Text.Json.Serialization;

namespace Skyworks.Core.Services.Orchestration;

public interface ISORAOrchestrationService
{
    SORACompleteResult ExecuteComplete(SORACompleteRequest request);
    
    // FIX #11: Async wrapper for better performance and deadlock avoidance (Sonnet 4.5)
    Task<SORACompleteResult> ExecuteCompleteAsync(SORACompleteRequest request);
}

public class SORACompleteRequest
{
    public string SoraVersion { get; set; } = "2.5"; // "2.0" or "2.5"
    public string? MissionId { get; set; }
    public string? MissionName { get; set; }
    public string? MissionDescription { get; set; }

    // Drone Selection (NEW!) - if provided, auto-populates GroundRisk parameters
    public string? DroneId { get; set; } // e.g., "DJI_MINI4PRO" - loads specs from catalog

    // Ground Risk inputs
    public GroundRiskInput GroundRisk { get; set; } = new();
    
    // Air Risk inputs
    public AirRiskInput AirRisk { get; set; } = new();
    
    // OSO implementations
    public List<OSOImplementation> ImplementedOSOs { get; set; } = new();
}

public class GroundRiskInput
{
    // SORA 2.0 specific
    public OperationalScenario? Scenario_V2_0 { get; set; }
    
    // SORA 2.5 specific
    public double? PopulationDensity { get; set; }
    public bool IsControlledGroundArea { get; set; }
    
    // Common - Required for Table 2 unified calculation
    public double? MTOM_kg { get; set; } // Maximum Take-Off Mass in kg
    public double MaxCharacteristicDimension { get; set; }
    public double? MaxSpeed { get; set; }
    public double? KineticEnergy { get; set; }
    
    // Mitigations - version dependent
    public List<MitigationInput> Mitigations { get; set; } = new();
}

public class MitigationInput
{
    // For SORA 2.0: M1, M2, M3
    public string Type { get; set; } = string.Empty; // "M1", "M2", "M3", "M1A", "M1B", "M1C"
    public RobustnessLevel Robustness { get; set; } = RobustnessLevel.Low;
}

public class AirRiskInput
{
    // Explicit ARC or environment-based
    public ARCRating? ExplicitARC { get; set; }
    
    // Environment inputs for decision tree
    public string? AirspaceControl { get; set; } // "Controlled", "Uncontrolled"
    public string? LocationType { get; set; } // "Airport", "NonAirport"
    public string? Environment { get; set; } // "Urban", "NonUrban", "Rural"
    public string? Typicality { get; set; } // "Typical", "Atypical"
    public double? MaxHeightAGL { get; set; }
    
    // Optional aerodrome proximity fields (advisory; non-breaking)
    public bool? IsNearAerodrome { get; set; }
    public double? DistanceToAerodrome_km { get; set; }
    public bool? IsInCTR { get; set; }
    
    // Strategic mitigations for residual ARC
    public List<string> StrategicMitigations { get; set; } = new(); // ["S1", "S2", "S3", "S4"]
    public int? LocalDensityRating { get; set; } // 1..5
    public bool IsAtypicalSegregated { get; set; }
}

public class SORACompleteResult
{
    public bool IsSuccessful { get; set; }
    public string SoraVersion { get; set; } = "2.5";
    
    // Mission metadata
    public string? MissionId { get; set; }
    public string? MissionName { get; set; }
    
    // Ground Risk outputs
    public int? IntrinsicGRC { get; set; }
    public int? FinalGRC { get; set; }
    public string GroundRiskNotes { get; set; } = string.Empty;
    
    // Air Risk outputs
    public ARCRating? InitialARC { get; set; }
    public ARCRating? ResidualARC { get; set; }
    public string AirRiskNotes { get; set; } = string.Empty;
    
    // SAIL outputs
    [JsonIgnore]
    public SAILLevel? SAIL { get; set; }

    // Normalized SAIL label for API responses (tests expect raw roman numerals: I..VI, then add "SAIL_" prefix themselves)
    [JsonPropertyName("sail")]
    public string? SAILLabel => SAIL switch
    {
        // Return raw roman numerals only; E2E tests construct actual as "SAIL_" + this value
        SAILLevel.I => "I",
        SAILLevel.II => "II",
        SAILLevel.III => "III",
        SAILLevel.IV => "IV",
        SAILLevel.V => "V",
        SAILLevel.VI => "VI",
        _ => null
    };
    
    [JsonIgnore]
    public TMPRResult? TMPRDetails { get; set; }
    
    // TMPR level as string for API compatibility (tests expect "Low"/"Medium"/"High"/"VeryHigh").
    // Compute directly from ResidualARC to avoid serialization/assignment ordering issues.
    [JsonPropertyName("tmpr")]
    public string? TMPR => ResidualARC switch
    {
        ARCRating.ARC_a => "Low",
        ARCRating.ARC_b => "Medium",
        ARCRating.ARC_c => "High",
        ARCRating.ARC_d => "VeryHigh",
        _ => null
    };
    
    public string SAILNotes { get; set; } = string.Empty;
    
    // OSO Compliance
    public int RequiredOSOCount { get; set; }
    public int ImplementedOSOCount { get; set; }
    public List<string> MissingOSOs { get; set; } = new();
    public List<string> InsufficientRobustnessOSOs { get; set; } = new();
    public bool IsCompliant { get; set; }
    
    // Risk Modeling (advisory)
    public double? RiskScore { get; set; }
    public string? RiskBand { get; set; }
    
    // Overall
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    
    // FIX #9: Structured error/warning codes for API responses (e.g., "SCOPE_SAIL_VI", "SCOPE_HIGH_GRC")
    public string? ReasonCode { get; set; }
    
    // FIX #10: Detailed reason for out-of-scope operations (from Sonnet 4.5 calibration)
    public string? OutOfScopeReason { get; set; }
}
