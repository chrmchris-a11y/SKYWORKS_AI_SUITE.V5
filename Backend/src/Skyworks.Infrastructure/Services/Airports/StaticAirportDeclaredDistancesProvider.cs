using Skyworks.Core.Services.Airports;

namespace Skyworks.Infrastructure.Services.Airports;

public class StaticAirportDeclaredDistancesProvider : IAirportDeclaredDistancesProvider
{
    private static readonly Dictionary<string, List<RunwayDeclaredDistances>> _db =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Greece sample: Athens (LGAV) and Thessaloniki (LGTS)
            ["LGAV"] = new()
            {
                // Lengths are approximate; declared distances often differ. Use length as proxy where unknown.
                new RunwayDeclaredDistances("LGAV", "03L", 4000, 4000, 4000, 4000, 3800),
                new RunwayDeclaredDistances("LGAV", "21R", 4000, 4000, 4000, 4000, 3800),
                new RunwayDeclaredDistances("LGAV", "03R", 3800, 3800, 3800, 3800, 3600),
                new RunwayDeclaredDistances("LGAV", "21L", 3800, 3800, 3800, 3800, 3600)
            },
            ["LGTS"] = new()
            {
                new RunwayDeclaredDistances("LGTS", "10", 2441, 2441, 2441, 2441, 2400),
                new RunwayDeclaredDistances("LGTS", "28", 2441, 2441, 2441, 2441, 2400)
            }
        };

    public IEnumerable<RunwayDeclaredDistances> GetRunwaysByIcao(string icao)
    {
        if (string.IsNullOrWhiteSpace(icao)) return Enumerable.Empty<RunwayDeclaredDistances>();
        return _db.TryGetValue(icao.Trim().ToUpperInvariant(), out var list) ? list : Enumerable.Empty<RunwayDeclaredDistances>();
    }
}
