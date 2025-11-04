using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests.Orchestration;

public class SORAController_Info_SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SORAController_Info_SmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_SORA_Info_Returns_Metadata()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/sora/info");
        var body = await resp.Content.ReadAsStringAsync();
        Assert.True(resp.StatusCode == HttpStatusCode.OK, $"Expected 200 OK, got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("SORA Complete Assessment Pipeline", json.GetProperty("name").GetString());
        Assert.True(json.GetProperty("supportedVersions").GetArrayLength() >= 2);
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("description").GetString()));
        Assert.Equal("POST /api/sora/complete", json.GetProperty("endpoints").GetProperty("complete").GetString());
    }
}
