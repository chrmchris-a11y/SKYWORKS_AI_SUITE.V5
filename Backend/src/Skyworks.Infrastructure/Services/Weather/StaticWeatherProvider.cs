using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Weather;
using Skyworks.Core.Services.Weather;

namespace Skyworks.Infrastructure.Services.Weather;

/// <summary>
/// Static weather provider with demo data for Cyprus and Athens regions.
/// Production systems should use real METAR/TAF or Open-Meteo APIs.
/// </summary>
public class StaticWeatherProvider : IWeatherProvider
{
    private readonly List<WeatherConditions> _demoStations;

    public StaticWeatherProvider()
    {
        _demoStations = LoadDemoWeather();
    }

    public Task<WeatherConditions?> GetWeatherAsync(GeoPoint position, CancellationToken ct = default)
    {
        // Find nearest station (simple distance check)
        var nearest = _demoStations
            .OrderBy(w => Distance(position, w.Position))
            .FirstOrDefault();
        
        return Task.FromResult(nearest);
    }

    public Task<WeatherConditions?> GetForecastAsync(GeoPoint position, DateTime targetTime, CancellationToken ct = default)
    {
        // For demo, return current weather (no real TAF)
        return GetWeatherAsync(position, ct);
    }

    public async Task<WeatherQueryResult> EvaluateWeatherGatesAsync(WeatherRequest request, CancellationToken ct = default)
    {
        var wx = await GetWeatherAsync(request.Position, ct);
        var result = new WeatherQueryResult
        {
            Conditions = wx,
            Notes = "Demo weather data (static provider)"
        };

        if (wx == null)
        {
            result.Gates.Add(new WeatherRiskGate
            {
                IsAcceptable = false,
                GateType = "Availability",
                Message = "No weather data available for this location",
                Severity = "Warning"
            });
            return result;
        }

        // VMC gate
        var altitudeAGL = request.AltitudeAGL ?? 60; // default 60m
        var isVMC = VMCCriteria.IsVMC(wx, altitudeAGL);
        result.Gates.Add(new WeatherRiskGate
        {
            IsAcceptable = isVMC,
            GateType = "VMC",
            Message = isVMC 
                ? $"VMC conditions met (visibility: {wx.VisibilityMeters}m, ceiling: {wx.CloudCeilingMeters}m)"
                : $"IMC conditions - visibility {wx.VisibilityMeters}m < {VMCCriteria.MinVisibilityMeters_Below3000ft}m or ceiling {wx.CloudCeilingMeters}m < {VMCCriteria.MinCloudCeilingMeters_Below3000ft}m",
            Severity = isVMC ? "Info" : "Error"
        });

        // Wind gate (example: >10 m/s is warning for small drones)
        if (wx.WindSpeedMps > 10)
        {
            result.Gates.Add(new WeatherRiskGate
            {
                IsAcceptable = false,
                GateType = "Wind",
                Message = $"High wind speed: {wx.WindSpeedMps:F1} m/s (>10 m/s threshold)",
                Severity = "Warning"
            });
        }

        // Gust gate
        if (wx.WindGustMps > 15)
        {
            result.Gates.Add(new WeatherRiskGate
            {
                IsAcceptable = false,
                GateType = "Gust",
                Message = $"Strong gusts: {wx.WindGustMps:F1} m/s (>15 m/s threshold)",
                Severity = "Error"
            });
        }

        // Precipitation gate
        if (wx.IsPrecipitating && wx.PrecipitationType != "None")
        {
            result.Gates.Add(new WeatherRiskGate
            {
                IsAcceptable = false,
                GateType = "Precipitation",
                Message = $"Precipitation detected: {wx.PrecipitationType}",
                Severity = "Warning"
            });
        }

        return result;
    }

    private List<WeatherConditions> LoadDemoWeather()
    {
        return new List<WeatherConditions>
        {
            // Larnaca, Cyprus (LCLK)
            new WeatherConditions
            {
                LocationId = "LCLK",
                Position = new GeoPoint { Latitude = 34.8751, Longitude = 33.6249 },
                ObservationTime = DateTime.UtcNow.AddMinutes(-15),
                VisibilityMeters = 10000,
                IsVMC = true,
                WindSpeedMps = 5.1, // ~10 kt
                WindGustMps = 7.7,  // ~15 kt
                WindDirectionDegrees = 270,
                CloudCeilingMeters = 1200,
                CloudCoverage = "SCT",
                IsPrecipitating = false,
                PrecipitationType = "None",
                TemperatureCelsius = 22,
                QNH_hPa = 1015,
                RawMETAR = "LCLK 221200Z 27010KT 9999 SCT040 22/14 Q1015",
                Notes = "Good VFR conditions"
            },
            
            // Paphos, Cyprus (LCPH)
            new WeatherConditions
            {
                LocationId = "LCPH",
                Position = new GeoPoint { Latitude = 34.7180, Longitude = 32.4857 },
                ObservationTime = DateTime.UtcNow.AddMinutes(-10),
                VisibilityMeters = 8000,
                IsVMC = true,
                WindSpeedMps = 6.7, // ~13 kt
                WindGustMps = 9.3,  // ~18 kt
                WindDirectionDegrees = 320,
                CloudCeilingMeters = 900,
                CloudCoverage = "BKN",
                IsPrecipitating = false,
                PrecipitationType = "None",
                TemperatureCelsius = 21,
                QNH_hPa = 1014,
                RawMETAR = "LCPH 221200Z 32013G18KT 8000 BKN030 21/13 Q1014",
                Notes = "Marginal VFR"
            },
            
            // Athens, Greece (LGAV)
            new WeatherConditions
            {
                LocationId = "LGAV",
                Position = new GeoPoint { Latitude = 37.9364, Longitude = 23.9445 },
                ObservationTime = DateTime.UtcNow.AddMinutes(-20),
                VisibilityMeters = 9000,
                IsVMC = true,
                WindSpeedMps = 4.1, // ~8 kt
                WindGustMps = 6.2,  // ~12 kt
                WindDirectionDegrees = 180,
                CloudCeilingMeters = 1500,
                CloudCoverage = "FEW",
                IsPrecipitating = false,
                PrecipitationType = "None",
                TemperatureCelsius = 24,
                QNH_hPa = 1018,
                RawMETAR = "LGAV 221200Z 18008KT 9999 FEW050 24/12 Q1018",
                Notes = "Excellent VFR conditions"
            },
            
            // Demo: Low visibility scenario (Limassol area)
            new WeatherConditions
            {
                LocationId = "DEMO_LIMASSOL",
                Position = new GeoPoint { Latitude = 34.68, Longitude = 33.04 },
                ObservationTime = DateTime.UtcNow.AddMinutes(-5),
                VisibilityMeters = 3000, // Below VMC minima
                IsVMC = false,
                WindSpeedMps = 8.2,
                WindGustMps = 12.4,
                WindDirectionDegrees = 90,
                CloudCeilingMeters = 300, // Low ceiling
                CloudCoverage = "OVC",
                IsPrecipitating = true,
                PrecipitationType = "Rain",
                TemperatureCelsius = 18,
                QNH_hPa = 1010,
                RawMETAR = "DEMO 221200Z 09016G24KT 3000 RA OVC010 18/16 Q1010",
                Notes = "IMC - not suitable for VFR operations"
            }
        };
    }

    private static double Distance(GeoPoint a, GeoPoint b)
    {
        // Simple Euclidean distance (good enough for demo)
        var dx = a.Latitude - b.Latitude;
        var dy = a.Longitude - b.Longitude;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
