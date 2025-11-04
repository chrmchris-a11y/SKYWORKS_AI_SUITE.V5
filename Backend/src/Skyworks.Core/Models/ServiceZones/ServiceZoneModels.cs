using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Models.ServiceZones;

public enum ServiceZoneCategory
{
    Urban,
    SemiUrban,
    Agriculture,
    Industrial,
    PhotovoltaicPark,
    StadiumEvent
}

public class ServiceZone
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required ServiceZoneCategory Category { get; set; }
    public required GeoPoint Center { get; set; }
    public double RadiusMeters { get; set; } = 500; // default radius
    public string? Notes { get; set; }
}
