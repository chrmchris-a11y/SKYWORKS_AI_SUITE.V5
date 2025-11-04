using Xunit;
using Skyworks.Infrastructure.Services.Traffic;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Traffic;

namespace Skyworks.Api.Tests.Traffic;

public class TrafficProvider_Tests
{
    [Fact]
    public async Task GetTrafficDensity_LarnacaArea_ReturnsTargets()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 }, // Near LCLK
            RadiusMeters = 50000 // 50km
        };

        // Act
        var result = await provider.GetTrafficDensityAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Density);
        Assert.True(result.Density.TotalTargets > 0, "Should find traffic targets near Larnaca");
        Assert.NotEmpty(result.Targets);
        Assert.NotEmpty(result.RiskNotes);
    }

    [Fact]
    public async Task GetTrafficDensity_SmallRadius_LimitedTargets()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.7, Longitude = 33.0 }, // Limassol
            RadiusMeters = 5000 // 5km only
        };

        // Act
        var result = await provider.GetTrafficDensityAsync(query);

        // Assert
        Assert.NotNull(result);
        // Should find fewer targets in smaller radius
        Assert.True(result.Density.TotalTargets <= 3, "Small radius should find fewer targets");
    }

    [Fact]
    public async Task GetNearbyTargets_ReturnsOrderedByDistance()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 }, // Near LCLK
            RadiusMeters = 100000 // 100km
        };

        // Act
        var targets = await provider.GetNearbyTargetsAsync(query);

        // Assert
        Assert.NotNull(targets);
        if (targets.Count > 1)
        {
            // Check ordering by distance
            for (int i = 0; i < targets.Count - 1; i++)
            {
                Assert.True(targets[i].DistanceMeters <= targets[i + 1].DistanceMeters,
                    "Targets should be ordered by distance (closest first)");
            }
        }
    }

    [Fact]
    public async Task GetNearbyTargets_AltitudeFilter_FiltersCorrectly()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 35.0, Longitude = 33.3 },
            RadiusMeters = 100000,
            MinAltitudeMsl = 5000, // Only high-altitude traffic
            MaxAltitudeMsl = 12000
        };

        // Act
        var targets = await provider.GetNearbyTargetsAsync(query);

        // Assert
        Assert.NotNull(targets);
        // All targets should be within altitude range
        foreach (var target in targets)
        {
            if (target.Position.AltitudeMeters.HasValue)
            {
                Assert.True(target.Position.AltitudeMeters >= 5000, "Target altitude should be >= MinAltitude");
                Assert.True(target.Position.AltitudeMeters <= 12000, "Target altitude should be <= MaxAltitude");
            }
        }
    }

    [Fact]
    public async Task GetNearbyTargets_TypeFilter_Aircraft()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.9, Longitude = 33.7 },
            RadiusMeters = 100000,
            TargetTypes = new List<string> { "Aircraft" }
        };

        // Act
        var targets = await provider.GetNearbyTargetsAsync(query);

        // Assert
        Assert.NotNull(targets);
        Assert.All(targets, t => Assert.Equal("Aircraft", t.TargetType));
    }

    [Fact]
    public async Task GetNearbyTargets_TypeFilter_Drone()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.68, Longitude = 33.04 }, // Near drone demo location
            RadiusMeters = 10000,
            TargetTypes = new List<string> { "Drone" }
        };

        // Act
        var targets = await provider.GetNearbyTargetsAsync(query);

        // Assert
        Assert.NotNull(targets);
        if (targets.Any())
        {
            Assert.All(targets, t => Assert.Equal("Drone", t.TargetType));
        }
    }

    [Theory]
    [InlineData(0, "Low")]
    [InlineData(2, "Low")]
    [InlineData(3, "Medium")]
    [InlineData(5, "Medium")]
    [InlineData(6, "High")]
    [InlineData(10, "High")]
    [InlineData(11, "Very High")]
    [InlineData(20, "Very High")]
    public void TrafficDensityThresholds_ClassifiesCorrectly(int targetCount, string expectedLevel)
    {
        // Act
        var level = TrafficDensityThresholds.ClassifyDensity(targetCount);

        // Assert
        Assert.Equal(expectedLevel, level);
    }

    [Fact]
    public void TrafficDensityThresholds_GetRiskNote_ReturnsMessage()
    {
        // Arrange & Act
        var lowNote = TrafficDensityThresholds.GetRiskNote("Low");
        var mediumNote = TrafficDensityThresholds.GetRiskNote("Medium");
        var highNote = TrafficDensityThresholds.GetRiskNote("High");
        var veryHighNote = TrafficDensityThresholds.GetRiskNote("Very High");

        // Assert
        Assert.NotEmpty(lowNote);
        Assert.NotEmpty(mediumNote);
        Assert.Contains("tactical mitigations", highNote, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TMPR", veryHighNote, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTrafficDensity_IncludesTargetsByType()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 },
            RadiusMeters = 100000
        };

        // Act
        var result = await provider.GetTrafficDensityAsync(query);

        // Assert
        Assert.NotNull(result.Density.TargetsByType);
        Assert.True(result.Density.TargetsByType.Count > 0, "Should have breakdown by type");
        
        // Check sum matches total
        var sum = result.Density.TargetsByType.Values.Sum();
        Assert.Equal(result.Density.TotalTargets, sum);
    }

    [Fact]
    public async Task GetTrafficDensity_CalculatesAverageAltitude()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 },
            RadiusMeters = 100000
        };

        // Act
        var result = await provider.GetTrafficDensityAsync(query);

        // Assert
        if (result.Density.TotalTargets > 0)
        {
            Assert.NotNull(result.Density.AverageAltitudeMsl);
            Assert.True(result.Density.AverageAltitudeMsl > 0);
        }
    }

    [Fact]
    public async Task GetTrafficDensity_HighDensity_AddsRiskNotes()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 34.875, Longitude = 33.625 },
            RadiusMeters = 200000 // Large radius to capture many targets
        };

        // Act
        var result = await provider.GetTrafficDensityAsync(query);

        // Assert
        Assert.NotEmpty(result.RiskNotes);
        
        if (result.Density.TotalTargets > 5)
        {
            Assert.Contains(result.RiskNotes, note => 
                note.Contains("DAA", StringComparison.OrdinalIgnoreCase) ||
                note.Contains("TMPR", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task GetNearbyTargets_RemoteArea_NoTargets()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 35.5, Longitude = 34.5 }, // Far from demo targets
            RadiusMeters = 5000
        };

        // Act
        var targets = await provider.GetNearbyTargetsAsync(query);

        // Assert
        Assert.NotNull(targets);
        Assert.Empty(targets); // Should find no targets in remote area
    }

    [Fact]
    public async Task GetTrafficDensity_Athens_FindsTargets()
    {
        // Arrange
        var provider = new StaticTrafficProvider();
        var query = new TrafficQuery
        {
            Position = new GeoPoint { Latitude = 37.94, Longitude = 23.94 }, // Near LGAV
            RadiusMeters = 50000
        };

        // Act
        var result = await provider.GetTrafficDensityAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Density.TotalTargets > 0, "Should find targets near Athens");
    }
}
