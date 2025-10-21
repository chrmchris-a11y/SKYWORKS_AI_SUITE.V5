// Phase1 Step2 — Skyworks V5
using Xunit;

namespace Skyworks.Api.Tests;

public class ApiSmokeTests
{
    [Fact]
    public void Sanity_Check()
    {
        Assert.True(1 + 1 == 2);
    }
}
