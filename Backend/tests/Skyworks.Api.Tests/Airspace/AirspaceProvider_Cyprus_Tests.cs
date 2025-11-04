using Skyworks.Core.Models.Airspace;
using Skyworks.Infrastructure.Services;
using Xunit;

namespace Skyworks.Api.Tests.Airspace;

public class AirspaceProvider_Cyprus_Tests
{
    private readonly StaticAirspaceProvider _provider = new();

    [Fact]
    public async Task MetropolitanMall_Larnaca_is_in_Larnaca_CTR()
    {
        var p = new GeoPoint { Latitude = 34.92463, Longitude = 33.60543, AltitudeMeters = 60 };
        var res = await _provider.GetAirspacesAtPointAsync(p);
        Assert.Contains(res.Airspaces, a => a.Id == "CTR_LCLK");
    }

    [Fact]
    public async Task MyMall_Limassol_is_in_Akrotiri_CTR()
    {
        var p = new GeoPoint { Latitude = 34.6790, Longitude = 33.0053, AltitudeMeters = 60 };
        var res = await _provider.GetAirspacesAtPointAsync(p);
        Assert.Contains(res.Airspaces, a => a.Id == "CTR_LCRA");
    }
}
