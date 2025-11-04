using Skyworks.Core.Models.Streaming;

namespace Skyworks.Core.Services.Streaming;

public interface IStreamingArcService
{
    Task<StreamRiskEvent> ProcessTelemetry_V2_5Async(TelemetryUpdate update, CancellationToken ct = default);
}
