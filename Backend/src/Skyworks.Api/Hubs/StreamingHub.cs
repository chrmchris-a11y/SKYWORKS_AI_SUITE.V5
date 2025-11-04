using Microsoft.AspNetCore.SignalR;
using Skyworks.Core.Models.Streaming;
using Skyworks.Core.Services.Streaming;

namespace Skyworks.Api.Hubs;

public class StreamingHub : Hub
{
    private readonly IStreamingArcService _stream;

    public StreamingHub(IStreamingArcService stream)
    {
        _stream = stream;
    }

    public async Task Subscribe(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }

    public async Task Telemetry(TelemetryUpdate update)
    {
        var ev = await _stream.ProcessTelemetry_V2_5Async(update);
        var group = update.SessionId ?? update.DroneId;
        await Clients.Group(group).SendAsync("riskEvent", ev);
    }
}
