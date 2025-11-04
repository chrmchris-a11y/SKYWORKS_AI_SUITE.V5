using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Weather;

namespace Skyworks.Core.Services.Weather;

/// <summary>
/// Weather data provider interface.
/// Implementations can use METAR/TAF, Open-Meteo, or other sources.
/// </summary>
public interface IWeatherProvider
{
    /// <summary>
    /// Get current weather conditions at a location.
    /// </summary>
    Task<WeatherConditions?> GetWeatherAsync(GeoPoint position, CancellationToken ct = default);
    
    /// <summary>
    /// Get weather forecast for a future time (if supported).
    /// </summary>
    Task<WeatherConditions?> GetForecastAsync(GeoPoint position, DateTime targetTime, CancellationToken ct = default);
    
    /// <summary>
    /// Evaluate weather conditions against operational gates.
    /// </summary>
    Task<WeatherQueryResult> EvaluateWeatherGatesAsync(WeatherRequest request, CancellationToken ct = default);
}
