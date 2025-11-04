namespace Skyworks.Core.Services.Airports;

public record RunwayDeclaredDistances(
    string AirportIcao,
    string Runway,
    int LengthM,
    int? ToraM,
    int? TodaM,
    int? AsdaM,
    int? LdaM
);

public interface IAirportDeclaredDistancesProvider
{
    IEnumerable<RunwayDeclaredDistances> GetRunwaysByIcao(string icao);
}
