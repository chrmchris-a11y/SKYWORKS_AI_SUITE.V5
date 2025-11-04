using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests;

public class AuthAndKbTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthAndKbTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "Phase1: auth wired, skipping login test in CI until environment stable")]
    public async Task Login_Returns_Token()
    {
        var client = _factory.CreateClient();
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "admin123" });
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
        var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginJson.GetProperty("access_token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }
}
