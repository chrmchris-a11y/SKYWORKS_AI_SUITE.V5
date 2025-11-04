using Xunit;
using Skyworks.Infrastructure.Services.Weather;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Weather;

namespace Skyworks.Api.Tests.Weather;

public class WeatherProvider_Tests
{
    [Fact]
    public async Task GetWeather_ReturnsNearestStation()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var larnacaAirportPosition = new GeoPoint { Latitude = 34.875, Longitude = 33.625 }; // Near LCLK

        // Act
        var wx = await provider.GetWeatherAsync(larnacaAirportPosition);

        // Assert
        Assert.NotNull(wx);
        Assert.Equal("LCLK", wx.LocationId);
        Assert.True(wx.IsVMC, "LCLK should be VMC in demo");
    }

    [Fact]
    public async Task GetWeather_LimassolReturnsIMC()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var limassolPosition = new GeoPoint { Latitude = 34.7, Longitude = 33.0 }; // Near DEMO_LIMASSOL

        // Act
        var wx = await provider.GetWeatherAsync(limassolPosition);

        // Assert
        Assert.NotNull(wx);
        Assert.Equal("DEMO_LIMASSOL", wx.LocationId);
        Assert.False(wx.IsVMC, "Limassol should be IMC in demo (low vis)");
        Assert.Equal(3000, wx.VisibilityMeters); // Below VMC minima (5000m)
    }

    [Theory]
    [InlineData(8000, 600, 200, true)]  // Good VFR: 8km vis, 600m ceiling, 200m AGL → VMC
    [InlineData(4000, 600, 200, false)] // Poor vis: 4km < 5km minima → IMC
    [InlineData(8000, 300, 200, false)] // Low ceiling: 300m < 450m minima → IMC
    [InlineData(8000, 600, 3500, true)] // Above 3000ft: minima don't apply → VMC (if vis OK)
    public void VMCCriteria_CorrectlyEvaluatesConditions(int visibilityMeters, int cloudCeilingMeters, int altitudeAGL, bool expectedVMC)
    {
        // Arrange
        var wx = new WeatherConditions
        {
            LocationId = "TEST",
            Position = new GeoPoint { Latitude = 0, Longitude = 0 },
            ObservationTime = DateTime.UtcNow,
            VisibilityMeters = visibilityMeters,
            CloudCeilingMeters = cloudCeilingMeters
        };

        // Act
        bool isVMC = VMCCriteria.IsVMC(wx, altitudeAGL);

        // Assert
        Assert.Equal(expectedVMC, isVMC);
    }

    [Fact]
    public async Task EvaluateGates_VMCGate_PassesForGoodWeather()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var request = new WeatherRequest
        {
            Position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 }, // Near LCLK (good weather)
            AltitudeAGL = 150,
            TargetTime = DateTime.UtcNow
        };

        // Act
        var result = await provider.EvaluateWeatherGatesAsync(request);

        // Assert
        Assert.NotNull(result);
        var vmcGate = result.Gates.FirstOrDefault(g => g.GateType == "VMC");
        Assert.NotNull(vmcGate);
        Assert.True(vmcGate.IsAcceptable, "VMC gate should pass for LCLK");
    }

    [Fact]
    public async Task EvaluateGates_VMCGate_FailsForIMC()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var request = new WeatherRequest
        {
            Position = new GeoPoint { Latitude = 34.7, Longitude = 33.0 }, // Near DEMO_LIMASSOL (IMC)
            AltitudeAGL = 150,
            TargetTime = DateTime.UtcNow
        };

        // Act
        var result = await provider.EvaluateWeatherGatesAsync(request);

        // Assert
        Assert.NotNull(result);
        var vmcGate = result.Gates.FirstOrDefault(g => g.GateType == "VMC");
        Assert.NotNull(vmcGate);
        Assert.False(vmcGate.IsAcceptable, "VMC gate should fail for IMC conditions");
        Assert.Equal("Error", vmcGate.Severity);
    }

    [Fact]
    public async Task EvaluateGates_WindGate_PassesForModerateWind()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var request = new WeatherRequest
        {
            Position = new GeoPoint { Latitude = 34.7, Longitude = 33.0 }, // DEMO_LIMASSOL has 8.2 m/s wind (below 10 m/s threshold)
            AltitudeAGL = 150,
            TargetTime = DateTime.UtcNow
        };

        // Act
        var result = await provider.EvaluateWeatherGatesAsync(request);

        // Assert
        var windGate = result.Gates.FirstOrDefault(g => g.GateType == "Wind");
        // Wind gate is only added if wind > 10 m/s, so it should be null or acceptable
        if (windGate != null)
        {
            Assert.True(windGate.IsAcceptable, "Wind gate should pass for winds < 10 m/s");
        }
    }

    [Fact]
    public async Task EvaluateGates_PrecipitationGate_WarnsForRain()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var request = new WeatherRequest
        {
            Position = new GeoPoint { Latitude = 34.7, Longitude = 33.0 }, // DEMO_LIMASSOL has rain
            AltitudeAGL = 150,
            TargetTime = DateTime.UtcNow
        };

        // Act
        var result = await provider.EvaluateWeatherGatesAsync(request);

        // Assert
        var precipGate = result.Gates.FirstOrDefault(g => g.GateType == "Precipitation");
        Assert.NotNull(precipGate);
        Assert.False(precipGate.IsAcceptable, "Precipitation gate should warn for rain");
        Assert.Equal("Warning", precipGate.Severity);
    }

    [Fact]
    public async Task GetForecast_ReturnsCurrentWeather()
    {
        // Arrange
        var provider = new StaticWeatherProvider();
        var position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 }; // LCLK
        var targetTime = DateTime.UtcNow.AddHours(3);

        // Act
        var forecast = await provider.GetForecastAsync(position, targetTime);

        // Assert
        Assert.NotNull(forecast);
        Assert.Equal("LCLK", forecast.LocationId);
        // NOTE: Static provider returns current weather (no TAF parsing yet)
    }
}
