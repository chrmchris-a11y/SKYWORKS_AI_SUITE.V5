using System.Text.Json.Serialization;

namespace Skyworks.Core.Models.Drones;

/// <summary>
/// EU C-Class category for drone certification (EU 2019/945 & 2020/1058).
/// </summary>
public enum EUCClass
{
    C0,
    C1,
    C2,
    C3,
    C4,
    C5,
    C6
}

/// <summary>
/// Drone specification model with SORA-relevant parameters.
/// </summary>
public class DroneSpecification
{
    public string Id { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    
    // EU Certification
    public EUCClass CClass { get; set; }
    
    // SORA Parameters
    public string SuggestedARC { get; set; } = "B"; // Heuristic based on MTOM/characteristics
    
    // Weight (kg)
    [JsonPropertyName("mTOM_kg")]
    public double MTOM_kg { get; set; } // Maximum Takeoff Mass
    
    [JsonPropertyName("emptyWeight_kg")]
    public double EmptyWeight_kg { get; set; }
    
    [JsonPropertyName("maxPayload_kg")]
    public double MaxPayload_kg { get; set; }
    
    // Dimensions (meters)
    [JsonPropertyName("length_m")]
    public double Length_m { get; set; }
    
    [JsonPropertyName("width_m")]
    public double Width_m { get; set; }
    
    [JsonPropertyName("height_m")]
    public double Height_m { get; set; }
    
    [JsonPropertyName("characteristicDimension_m")]
    public double CharacteristicDimension_m { get; set; } // Max(L, W) for GRC
    
    // Performance
    [JsonPropertyName("maxSpeed_mps")]
    public double MaxSpeed_mps { get; set; } // meters per second
    
    [JsonPropertyName("cruiseSpeed_mps")]
    public double CruiseSpeed_mps { get; set; }
    
    [JsonPropertyName("maxHeight_m")]
    public double MaxHeight_m { get; set; } // Service ceiling (AGL or AMSL depending on context)
    
    [JsonPropertyName("maxFlightTime_min")]
    public int MaxFlightTime_min { get; set; }
    
    [JsonPropertyName("maxRange_km")]
    public double MaxRange_km { get; set; }
    
    // Operational Features
    [JsonPropertyName("isTethered")]
    public bool IsTethered { get; set; }
    
    [JsonPropertyName("hasParachute")]
    public bool HasParachute { get; set; }
    
    [JsonPropertyName("hasGeoFencing")]
    public bool HasGeoFencing { get; set; }
    
    [JsonPropertyName("hasRemoteID")]
    public bool HasRemoteID { get; set; }
    
    [JsonPropertyName("hasObstacleAvoidance")]
    public bool HasObstacleAvoidance { get; set; }
    
    [JsonPropertyName("hasRTH")]
    public bool HasRTH { get; set; } // Return to Home
    
    // Sensors & Payloads
    [JsonPropertyName("sensors")]
    public List<string> Sensors { get; set; } = new();
    
    [JsonPropertyName("payloads")]
    public List<string> Payloads { get; set; } = new();
    
    // Propulsion
    [JsonPropertyName("propulsionType")]
    public string PropulsionType { get; set; } = "Electric"; // Electric, Hybrid, Gas
    
    [JsonPropertyName("numberOfRotors")]
    public int NumberOfRotors { get; set; }
    
    // Regulatory
    [JsonPropertyName("requiresPilotLicense")]
    public bool RequiresPilotLicense { get; set; }
    
    [JsonPropertyName("isBVLOSCapable")]
    public bool IsBVLOSCapable { get; set; }
    
    // Notes
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("manufacturerWebsite")]
    public string? ManufacturerWebsite { get; set; }
}

/// <summary>
/// Request to filter drone catalog by criteria.
/// </summary>
public class DroneCatalogFilter
{
    public EUCClass? CClass { get; set; }
    public string? Manufacturer { get; set; }
    public double? MaxMTOM_kg { get; set; }
    public double? MinMTOM_kg { get; set; }
    public bool? IsBVLOSCapable { get; set; }
    public bool? HasObstacleAvoidance { get; set; }
    public int? MinFlightTime_min { get; set; }
}

/// <summary>
/// Drone selection result with SORA pre-computed hints.
/// </summary>
public class DroneSelectionResult
{
    public DroneSpecification Drone { get; set; } = new();
    
    // SORA hints
    public string SuggestedInitialARC { get; set; } = "B";
    public int SuggestedGRC_Dimension { get; set; } // Based on CharacteristicDimension
    public int SuggestedGRC_Speed { get; set; } // Based on MaxSpeed
    
    public List<string> ComplianceNotes { get; set; } = new();
}
