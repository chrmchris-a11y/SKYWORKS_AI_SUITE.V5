using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Core.Models.Streaming;

public class TelemetryUpdate
{
    public required string DroneId { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public required GeoPoint Position { get; set; }
    public double? SpeedMps { get; set; }
    public string? SessionId { get; set; }
}

public class StreamRiskEvent
{
    public required string DroneId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public required ARCEnvironmentInput Environment { get; set; }
    public ARCRating InitialARC { get; set; }
    public int AirspacesCount { get; set; }
    public bool IsControlled { get; set; }
    public List<string> ValidationWarnings { get; set; } = new();
}
