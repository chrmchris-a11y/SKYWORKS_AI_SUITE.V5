using Skyworks.Core.Models.Airspace;
using Skyworks.Infrastructure.Services;
using Xunit;

namespace Skyworks.Api.Tests.ServiceZones;

public class ServiceZones_More_Tests
{
    public static IEnumerable<object[]> ZonePoints => new List<object[]>
    {
        new object[] { "CY_NIC_NICOSIA_MALL", 35.13473, 33.279285 },
        new object[] { "CY_NIC_MALL_OF_CYPRUS", 35.1827, 33.0569 },
        new object[] { "CY_EPT_PV_PARK", 34.846331, 33.1590499 },
        new object[] { "CY_KOF_PV_PARK", 34.81141, 33.417 },
        new object[] { "CY_AGE_PV_PARK", 35.0539, 32.8764 },
        new object[] { "CY_AKA_PV_PARK", 35.1371, 33.1624 },
        new object[] { "CY_LIM_ALPHA_MEGA_STADIUM", 34.693333, 32.939167 },
    };

    [Theory]
    [MemberData(nameof(ZonePoints))]
    public async Task Zone_at_point_is_detected(string expectedZoneId, double lat, double lon)
    {
        var zones = new StaticServiceZoneProvider();
        var p = new GeoPoint { Latitude = lat, Longitude = lon };
        var z = await zones.FindZoneAtPointAsync(p);
        Assert.NotNull(z);
        Assert.Equal(expectedZoneId, z!.Id);
    }
}
