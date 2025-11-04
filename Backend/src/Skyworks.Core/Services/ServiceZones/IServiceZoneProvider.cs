using Skyworks.Core.Models.ServiceZones;
using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Services.ServiceZones;

public interface IServiceZoneProvider
{
    Task<List<ServiceZone>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceZone?> FindZoneAtPointAsync(GeoPoint point, CancellationToken ct = default);
}
