using Skyworks.Core.Models.Airspace;
using Skyworks.Infrastructure.Services;
using Xunit;

namespace Skyworks.Api.Tests.ServiceZones;

public class ServiceZonesTests
{
    [Fact]
    public async Task MyMallLimassol_is_detected_as_Urban_zone()
    {
        var zones = new StaticServiceZoneProvider();
        var point = new GeoPoint { Latitude = 34.6790, Longitude = 33.0053 };
        var z = await zones.FindZoneAtPointAsync(point);
        Assert.NotNull(z);
        Assert.Equal("CY_LIM_MY_MALL", z!.Id);
    }

    [Fact]
    public async Task AvgorouPV_is_detected_as_PV_zone()
    {
        var zones = new StaticServiceZoneProvider();
        var point = new GeoPoint { Latitude = 35.041178, Longitude = 33.85636 };
        var z = await zones.FindZoneAtPointAsync(point);
        Assert.NotNull(z);
        Assert.Equal("CY_AVG_PV_PARK", z!.Id);
    }
}
