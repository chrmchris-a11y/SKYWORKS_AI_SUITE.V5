using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests;

public class AgentsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AgentsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // No special overrides needed; Program computes repo root
        });
    }

    [Fact]
    public async Task AskAgent_Sora_MockMode_ReturnsStructuredAnswer()
    {
        var client = _factory.CreateClient();

        var payload = new
        {
            AgentName = "SORA_Compliance_Agent",
            Question = "What are the OSO requirements for SAIL III?"
        };

        using var resp = await client.PostAsJsonAsync("/api/agents/ask", payload);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("success").GetBoolean());

        var answer = json.GetProperty("answer").GetString();
        Assert.False(string.IsNullOrWhiteSpace(answer));
        Assert.Contains("Direct Answer", answer, StringComparison.OrdinalIgnoreCase);

    var model = json.GetProperty("model").GetString();
    Assert.False(string.IsNullOrWhiteSpace(model));

        var sources = json.GetProperty("sources");
        Assert.True(sources.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task ListAgents_ReturnsTwoAgents()
    {
        var client = _factory.CreateClient();
        using var resp = await client.GetAsync("/api/agents/list");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.True(json.GetArrayLength() >= 2);

    var names = json.EnumerateArray().Select(e => e.GetProperty("name").GetString()).ToList();
        Assert.Contains("SORA_Compliance_Agent", names);
        Assert.Contains("Mission_Planning_Agent", names);
    }

    [Fact]
    public async Task TrainingStatus_ReturnsLatestReport()
    {
        var client = _factory.CreateClient();
        using var resp = await client.GetAsync("/api/agents/training/status");

        // We expect 200 OK because training reports exist under Tools/TrainingCenter/agent_memory
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        // Basic shape check: should have training_session block
        Assert.True(json.TryGetProperty("training_session", out var trainingSession));
        Assert.Equal(JsonValueKind.Object, trainingSession.ValueKind);
    }
}
