using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.ServiceZones;
using Skyworks.Core.Services.ServiceZones;
using Skyworks.Core.Utils.Geometry;

namespace Skyworks.Infrastructure.Services;

public class StaticServiceZoneProvider : IServiceZoneProvider
{
    private readonly List<ServiceZone> _zones;

    public StaticServiceZoneProvider()
    {
        _zones = new List<ServiceZone>
        {
            new ServiceZone
            {
                Id = "CY_LIM_MY_MALL",
                Name = "My Mall Limassol",
                Category = ServiceZoneCategory.Urban,
                Center = new GeoPoint{ Latitude = 34.6790, Longitude = 33.0053 },
                RadiusMeters = 600,
                Notes = "Mall area in Limassol"
            },
            new ServiceZone
            {
                Id = "CY_NIC_NICOSIA_MALL",
                Name = "Nicosia Mall",
                Category = ServiceZoneCategory.Urban,
                Center = new GeoPoint{ Latitude = 35.13473, Longitude = 33.279285 },
                RadiusMeters = 600
            },
            new ServiceZone
            {
                Id = "CY_NIC_MALL_OF_CYPRUS",
                Name = "Mall of Cyprus, Nicosia",
                Category = ServiceZoneCategory.Urban,
                Center = new GeoPoint{ Latitude = 35.1827, Longitude = 33.0569 },
                RadiusMeters = 600
            },
            new ServiceZone
            {
                Id = "CY_AVG_PV_PARK",
                Name = "Avgorou Photovoltaic Park",
                Category = ServiceZoneCategory.PhotovoltaicPark,
                Center = new GeoPoint{ Latitude = 35.041178, Longitude = 33.85636 },
                RadiusMeters = 1200
            },
            new ServiceZone
            {
                Id = "CY_EPT_PV_PARK",
                Name = "Eptagonia Photovoltaic Park",
                Category = ServiceZoneCategory.PhotovoltaicPark,
                Center = new GeoPoint{ Latitude = 34.846331, Longitude = 33.1590499 },
                RadiusMeters = 1200
            },
            new ServiceZone
            {
                Id = "CY_KOF_PV_PARK",
                Name = "Kofinou Photovoltaic Park",
                Category = ServiceZoneCategory.PhotovoltaicPark,
                Center = new GeoPoint{ Latitude = 34.81141, Longitude = 33.417 },
                RadiusMeters = 1200
            },
            new ServiceZone
            {
                Id = "CY_AGE_PV_PARK",
                Name = "Agios Epifanios Photovoltaic Park",
                Category = ServiceZoneCategory.PhotovoltaicPark,
                Center = new GeoPoint{ Latitude = 35.0539, Longitude = 32.8764 },
                RadiusMeters = 1200
            },
            new ServiceZone
            {
                Id = "CY_AKA_PV_PARK",
                Name = "Akaki Photovoltaic Park",
                Category = ServiceZoneCategory.PhotovoltaicPark,
                Center = new GeoPoint{ Latitude = 35.1371, Longitude = 33.1624 },
                RadiusMeters = 1200
            },
            new ServiceZone
            {
                Id = "CY_LCA_METRO_MALL",
                Name = "Metropolitan Mall, Larnaca",
                Category = ServiceZoneCategory.Urban,
                Center = new GeoPoint{ Latitude = 34.92463, Longitude = 33.60543 },
                RadiusMeters = 600
            },
            new ServiceZone
            {
                Id = "CY_LIM_ALPHA_MEGA_STADIUM",
                Name = "Alpha Mega Stadium, Limassol",
                Category = ServiceZoneCategory.StadiumEvent,
                Center = new GeoPoint{ Latitude = 34.693333, Longitude = 32.939167 }, // 34°41'36"N, 32°56'21"E approx
                RadiusMeters = 500
            }
        };
    }

    public Task<List<ServiceZone>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult(_zones.ToList());

    public Task<ServiceZone?> FindZoneAtPointAsync(GeoPoint point, CancellationToken ct = default)
    {
        foreach (var z in _zones)
        {
            var dist = GeometryUtils.HaversineMeters(point, z.Center);
            if (dist <= z.RadiusMeters)
                return Task.FromResult<ServiceZone?>(z);
        }
        return Task.FromResult<ServiceZone?>(null);
    }
}
