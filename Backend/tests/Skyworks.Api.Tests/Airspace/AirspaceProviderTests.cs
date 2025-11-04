using Skyworks.Core.Models.Airspace;
using Skyworks.Infrastructure.Services;
using Xunit;

namespace Skyworks.Api.Tests.Airspace;

public class AirspaceProviderTests
{
    private readonly StaticAirspaceProvider _provider = new();

    [Fact]
    public async Task Point_inside_demo_CTR_returns_CTR()
    {
        var point = new GeoPoint { Latitude = 37.95, Longitude = 23.90, AltitudeMeters = 120 }; // inside CTR_LGAV
        var result = await _provider.GetAirspacesAtPointAsync(point);

        Assert.NotEmpty(result.Airspaces);
        Assert.Contains(result.Airspaces, a => a.Id == "CTR_LGAV" && a.Type == AirspaceType.CTR);
    }

    [Fact]
    public async Task Point_outside_all_returns_empty()
    {
        var point = new GeoPoint { Latitude = 37.40, Longitude = 22.90, AltitudeMeters = 100 }; // outside
        var result = await _provider.GetAirspacesAtPointAsync(point);

        Assert.Empty(result.Airspaces);
    }

    [Fact]
    public async Task Altitude_filter_excludes_above_upper()
    {
        var point = new GeoPoint { Latitude = 38.08, Longitude = 23.35, AltitudeMeters = 1500 }; // above ATZ_LGMG upper
        var result = await _provider.GetAirspacesAtPointAsync(point);

        Assert.DoesNotContain(result.Airspaces, a => a.Id == "ATZ_LGMG");
    }
}
