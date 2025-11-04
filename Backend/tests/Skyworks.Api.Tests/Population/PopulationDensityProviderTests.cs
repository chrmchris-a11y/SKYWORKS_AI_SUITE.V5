using Xunit;
using Skyworks.Infrastructure.Services;

namespace Skyworks.Api.Tests.Population;

public class PopulationDensityProviderTests
{
    private readonly StaticPopulationDensityProvider _provider = new();

    [Fact]
    public async Task Athens_Center_Returns_High_Density()
    {
        var density = await _provider.GetDensityAsync(38.0, 23.7);
        Assert.NotNull(density);
        Assert.True(density > 10000); // Urban area
    }

    [Fact]
    public async Task Rural_Greece_Returns_Low_Density()
    {
        var density = await _provider.GetDensityAsync(37.0, 22.0);
        Assert.NotNull(density);
        Assert.True(density < 100); // Rural
    }

    [Fact]
    public async Task MaxDensityInArea_Returns_Max()
    {
        var polygon = new List<(double, double)>
        {
            (37.9, 23.6), (38.1, 23.6), (38.1, 23.9), (37.9, 23.9)
        };
        var max = await _provider.GetMaxDensityInAreaAsync(polygon);
        Assert.NotNull(max);
        Assert.True(max > 5000); // Should find Athens density
    }
}
