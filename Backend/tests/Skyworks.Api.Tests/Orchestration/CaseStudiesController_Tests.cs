using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests.Orchestration;

public class CaseStudiesController_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CaseStudiesController_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_And_Run_CaseStudy()
    {
        var client = _factory.CreateClient();

        var listResp = await client.GetAsync("/api/sora/case-studies");
        var listBody = await listResp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var list = JsonDocument.Parse(listBody).RootElement.EnumerateArray().Select(e => e.GetString()!).ToList();
        Assert.Contains("2.5_low_suburban_bvlos", list);

        var runResp = await client.PostAsync("/api/sora/case-studies/2.5_low_suburban_bvlos/run", content: null);
        var runBody = await runResp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, runResp.StatusCode);
        var json = JsonDocument.Parse(runBody).RootElement;
        Assert.True(json.GetProperty("isSuccessful").GetBoolean());
        Assert.Equal("2.5", json.GetProperty("soraVersion").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("summary").GetString()));
    }
}
