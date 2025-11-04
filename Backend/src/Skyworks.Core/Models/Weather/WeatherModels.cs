using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Models.Weather;

/// <summary>
/// Weather conditions at a specific location and time.
/// Used for VMC/IMC assessment and operational risk gates.
/// </summary>
public class WeatherConditions
{
    public required string LocationId { get; set; }
    public required GeoPoint Position { get; set; }
    public DateTime ObservationTime { get; set; } = DateTime.UtcNow;
    
    // Visibility
    public double VisibilityMeters { get; set; }
    public bool IsVMC { get; set; } // Visual Meteorological Conditions
    
    // Wind
    public double WindSpeedMps { get; set; } // meters per second
    public double WindGustMps { get; set; }
    public int WindDirectionDegrees { get; set; }
    
    // Clouds and ceiling
    public double CloudCeilingMeters { get; set; } // AGL
    public string CloudCoverage { get; set; } = "Clear"; // Clear, SCT, BKN, OVC
    
    // Precipitation
    public bool IsPrecipitating { get; set; }
    public string PrecipitationType { get; set; } = "None"; // None, Rain, Snow, Hail
    
    // Temperature
    public double TemperatureCelsius { get; set; }
    
    // Pressure
    public double QNH_hPa { get; set; } // Altimeter setting
    
    // Raw data (METAR/TAF if available)
    public string? RawMETAR { get; set; }
    public string? RawTAF { get; set; }
    
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// VMC (Visual Meteorological Conditions) criteria per EASA regulations.
/// Used to determine if weather allows visual flight operations.
/// </summary>
public static class VMCCriteria
{
    // EASA VMC minima for Class G airspace below 3000ft AMSL or 1000ft AGL (whichever is higher)
    public const double MinVisibilityMeters_Below3000ft = 5000; // 5 km
    public const double MinCloudCeilingMeters_Below3000ft = 450; // 1500 ft AGL
    public const string MinCloudClearance = "Clear of clouds and in sight of surface";
    
    // Above 3000ft AMSL: 5 km visibility, 1500m horizontal from clouds, 300m vertical
    public const double MinVisibilityMeters_Above3000ft = 5000;
    
    public static bool IsVMC(WeatherConditions wx, double altitudeAGL)
    {
        if (altitudeAGL <= 914) // Below 3000 ft (914m)
        {
            return wx.VisibilityMeters >= MinVisibilityMeters_Below3000ft
                   && wx.CloudCeilingMeters >= MinCloudCeilingMeters_Below3000ft;
        }
        
        return wx.VisibilityMeters >= MinVisibilityMeters_Above3000ft;
    }
}

/// <summary>
/// Weather-related operational gates for SORA risk assessment.
/// </summary>
public class WeatherRiskGate
{
    public bool IsAcceptable { get; set; }
    public string GateType { get; set; } = string.Empty; // "VMC", "Wind", "Visibility", "Ceiling"
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info"; // Info, Warning, Error
}

/// <summary>
/// Request to get weather conditions at a specific location.
/// </summary>
public class WeatherRequest
{
    public required GeoPoint Position { get; set; }
    public DateTime? TargetTime { get; set; } // For forecast (TAF), null = current (METAR)
    public double? AltitudeAGL { get; set; } // For VMC assessment
}

/// <summary>
/// Weather query result with conditions and operational gates.
/// </summary>
public class WeatherQueryResult
{
    public WeatherConditions? Conditions { get; set; }
    public List<WeatherRiskGate> Gates { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
