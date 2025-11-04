using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Weather;
using Skyworks.Core.Services.Weather;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherProvider _weatherProvider;

    public WeatherController(IWeatherProvider weatherProvider)
    {
        _weatherProvider = weatherProvider;
    }

    /// <summary>
    /// Get current weather conditions at a location.
    /// </summary>
    [HttpPost("current")]
    public async Task<ActionResult<WeatherConditions>> GetCurrent([FromBody] GeoPoint position)
    {
        var wx = await _weatherProvider.GetWeatherAsync(position);
        if (wx == null)
        {
            return NotFound(new { error = "No weather data available for this location" });
        }
        return Ok(wx);
    }

    /// <summary>
    /// Evaluate weather conditions with operational gates.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<WeatherQueryResult>> Evaluate([FromBody] WeatherRequest request)
    {
        var result = await _weatherProvider.EvaluateWeatherGatesAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get weather forecast (TAF) for a location and time.
    /// </summary>
    [HttpPost("forecast")]
    public async Task<ActionResult<WeatherConditions>> GetForecast([FromBody] WeatherForecastRequest request)
    {
        var wx = await _weatherProvider.GetForecastAsync(request.Position, request.TargetTime);
        if (wx == null)
        {
            return NotFound(new { error = "No forecast data available" });
        }
        return Ok(wx);
    }
}

public class WeatherForecastRequest
{
    public required GeoPoint Position { get; set; }
    public DateTime TargetTime { get; set; }
}
