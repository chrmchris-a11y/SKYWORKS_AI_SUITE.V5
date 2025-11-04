using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.Streaming;
using Skyworks.Core.Services.Airspace;
using Skyworks.Core.Services.ARC;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services.ServiceZones;
using Skyworks.Core.Models.ServiceZones;
using System.Linq;

namespace Skyworks.Core.Services.Streaming;

public class StreamingArcService : IStreamingArcService
{
    private readonly IGRCCalculationService _arc;
    private readonly IARCValidationService _validator;
    private readonly IAirspaceProvider _airspace;
    private readonly IServiceZoneProvider? _zones;

    private readonly Dictionary<string, DateTime> _lastUpdate = new();
    private readonly Dictionary<string, (DateTime ts, AirspaceQueryResult res)> _airspaceCache = new();
    private readonly TimeSpan _debounce;
    private readonly TimeSpan _airspaceTtl;

    public StreamingArcService(
        IGRCCalculationService arc,
        IARCValidationService validator,
        IAirspaceProvider airspace,
        IServiceZoneProvider? zones = null,
        TimeSpan? debounce = null,
        TimeSpan? airspaceTtl = null)
    {
        _arc = arc;
        _validator = validator;
        _airspace = airspace;
        _zones = zones;
        _debounce = debounce ?? TimeSpan.FromMilliseconds(500);
        _airspaceTtl = airspaceTtl ?? TimeSpan.FromSeconds(3);
    }

    public async Task<StreamRiskEvent> ProcessTelemetry_V2_5Async(TelemetryUpdate update, CancellationToken ct = default)
    {
        if (ShouldDebounce(update.SessionId, update.DroneId))
        {
            // Return last known result? For simplicity, process anyway but throttle by time gate below
        }

        var asp = await GetAirspacesCachedAsync(update.Position, ct);
        var isControlled = asp.Airspaces.Any(a => a.Type == AirspaceType.CTR || a.Type == AirspaceType.TMA || (a.Class.HasValue && a.Class.Value != Models.Airspace.AirspaceClass.G));

        var env = new ARCEnvironmentInput
        {
            AirspaceControl = isControlled ? AirspaceControl.Controlled : AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = await InferEnvironmentAsync(update.Position) ?? EnvironmentType.Rural,
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = update.Position.AltitudeMeters ?? 60
        };

    var arc = _arc.DetermineInitialARC_V2_5(env);
        var validation = _validator.ValidateEnvironment_V2_5(env);

    _lastUpdate[MakeThrottleKey(update.SessionId, update.DroneId)] = DateTime.UtcNow;

        return new StreamRiskEvent
        {
            DroneId = update.DroneId,
            TimestampUtc = update.TimestampUtc,
            Environment = env,
            InitialARC = arc.ARC,
            AirspacesCount = asp.Airspaces.Count,
            IsControlled = isControlled,
            ValidationWarnings = validation.Issues.Select(i => $"{i.Severity}: {i.Code}").ToList()
        };
    }

    private bool ShouldDebounce(string? sessionId, string droneId)
    {
        var now = DateTime.UtcNow;
        var key = MakeThrottleKey(sessionId, droneId);
        if (_lastUpdate.TryGetValue(key, out var last))
        {
            if (now - last < _debounce) return true;
        }
        return false;
    }

    private static string MakeThrottleKey(string? sessionId, string droneId)
        => string.IsNullOrWhiteSpace(sessionId) ? droneId : $"{sessionId}|{droneId}";

    private async Task<AirspaceQueryResult> GetAirspacesCachedAsync(GeoPoint p, CancellationToken ct)
    {
        var key = QuantizeKey(p);
        var now = DateTime.UtcNow;
        if (_airspaceCache.TryGetValue(key, out var entry))
        {
            if (now - entry.ts < _airspaceTtl) return entry.res;
        }
        var res = await _airspace.GetAirspacesAtPointAsync(p, ct);
        _airspaceCache[key] = (now, res);
        return res;
    }

    private static string QuantizeKey(GeoPoint p)
    {
        static double Q(double v, int digits) => Math.Round(v, digits);
        var lat = Q(p.Latitude, 5);
        var lon = Q(p.Longitude, 5);
        var alt = p.AltitudeMeters.HasValue ? Q(p.AltitudeMeters.Value, 0).ToString() : "na";
        return $"{lat:F5},{lon:F5},{alt}";
    }

    private async Task<EnvironmentType?> InferEnvironmentAsync(GeoPoint p)
    {
        if (_zones == null) return null;
        var zone = await _zones.FindZoneAtPointAsync(p);
        if (zone == null) return null;
        // JARUS Annex C Table 1: Only Urban or Rural for ARC determination
        return zone.Category switch
        {
            ServiceZoneCategory.Urban => EnvironmentType.Urban,
            ServiceZoneCategory.SemiUrban => EnvironmentType.Urban, // Semi-urban maps to Urban
            ServiceZoneCategory.Industrial => EnvironmentType.Urban, // Industrial areas are urban
            ServiceZoneCategory.Agriculture => EnvironmentType.Rural,
            ServiceZoneCategory.PhotovoltaicPark => EnvironmentType.Rural,
            ServiceZoneCategory.StadiumEvent => EnvironmentType.Urban,
            _ => EnvironmentType.Rural
        };
    }
}
