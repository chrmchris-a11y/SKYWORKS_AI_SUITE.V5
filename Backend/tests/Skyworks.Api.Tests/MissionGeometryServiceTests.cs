using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Skyworks.Core.Models;
using Skyworks.Core.Services;

namespace Skyworks.Api.Tests;

public class MissionGeometryServiceTests
{
    private readonly MissionGeometryService _service = new();

    [Fact]
    public async Task FacadeGeometry_ShouldHaveRouteAndCga()
    {
        // Arrange
        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Facade,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 50.0
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.MissionGeometryId);
        Assert.Equal(50.0m, result.MaxHeightAGL_m);
        Assert.True(result.RouteLength_m > 0, "Route length should be greater than 0");
        Assert.True(result.CgaArea_m2 > 0, "CGA area should be greater than 0");

        // Verify GeoJSON structure
        var geoJson = JsonDocument.Parse(result.GeoJson);
        Assert.Equal("FeatureCollection", geoJson.RootElement.GetProperty("type").GetString());
        
        var features = geoJson.RootElement.GetProperty("features");
        Assert.True(features.GetArrayLength() >= 3, "Should have waypoints, route, and CGA features");

        // Check for route LineString
        bool hasRoute = false;
        foreach (var feature in features.EnumerateArray())
        {
            if (feature.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("name", out var name) &&
                name.GetString() == "route")
            {
                hasRoute = true;
                var geometry = feature.GetProperty("geometry");
                Assert.Equal("LineString", geometry.GetProperty("type").GetString());
            }
        }
        Assert.True(hasRoute, "Should contain route LineString");

        // Check for CGA Polygon
        bool hasCga = false;
        foreach (var feature in features.EnumerateArray())
        {
            if (feature.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("name", out var name) &&
                name.GetString() == "cga")
            {
                hasCga = true;
                var geometry = feature.GetProperty("geometry");
                Assert.Equal("Polygon", geometry.GetProperty("type").GetString());
            }
        }
        Assert.True(hasCga, "Should contain CGA polygon");
    }

    [Fact]
    public async Task RoofGeometry_ShouldGenerateLawnmowerPattern()
    {
        // Arrange
        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Roof,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 30.0,
            AreaWidth_m = 40.0,
            AreaLength_m = 40.0
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30.0m, result.MaxHeightAGL_m);
        Assert.True(result.RouteLength_m > 0, "Route length should be greater than 0");
        Assert.True(result.CgaArea_m2 > 1000, "CGA area should be at least 1000 m² for 40x40m roof");

        // Verify lawn-mower pattern has multiple waypoints
        var geoJson = JsonDocument.Parse(result.GeoJson);
        var features = geoJson.RootElement.GetProperty("features");
        
        foreach (var feature in features.EnumerateArray())
        {
            if (feature.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("name", out var name) &&
                name.GetString() == "waypoints")
            {
                var geometry = feature.GetProperty("geometry");
                var coordinates = geometry.GetProperty("coordinates");
                Assert.True(coordinates.GetArrayLength() >= 6, "Should have multiple waypoints for lawn-mower pattern");
            }
        }
    }

    [Fact]
    public async Task SolarGeometry_ShouldRespectIsLargeArea()
    {
        // Arrange
        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Solar,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 50.0,
            AreaWidth_m = 200.0,
            AreaLength_m = 300.0
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50.0m, result.MaxHeightAGL_m);
        
        // Solar PV park should have significantly longer route than small roof
        Assert.True(result.RouteLength_m > 1000, "Solar park route should be > 1000m (multiple passes over large area)");
        
        // Large CGA area for PV park
        Assert.True(result.CgaArea_m2 > 40000, "Solar park CGA should be > 40,000 m² (200x300m + buffer)");

        // Verify GeoJSON has proper structure
        var geoJson = JsonDocument.Parse(result.GeoJson);
        Assert.Equal("FeatureCollection", geoJson.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task LinearGeometry_ShouldGenerateCorridor()
    {
        // Arrange
        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Linear,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 60.0,
            AreaLength_m = 1000.0 // 1km corridor
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(60.0m, result.MaxHeightAGL_m);
        Assert.True(result.RouteLength_m > 0, "Route length should be greater than 0");
        
        // Corridor should have narrow CGA (not as wide as Solar park)
        // Expect corridor width ~100m, length ~1000m = ~100,000 m²
        Assert.True(result.CgaArea_m2 > 50000, "Linear corridor CGA should be at least 50,000 m²");
        Assert.True(result.CgaArea_m2 < 500000, "Linear corridor CGA should be less than 500,000 m² (narrow corridor)");

        // Verify GeoJSON structure
        var geoJson = JsonDocument.Parse(result.GeoJson);
        var features = geoJson.RootElement.GetProperty("features");

        bool hasLineString = false;
        bool hasPolygon = false;
        
        foreach (var feature in features.EnumerateArray())
        {
            var geometry = feature.GetProperty("geometry");
            var geoType = geometry.GetProperty("type").GetString();
            
            if (geoType == "LineString") hasLineString = true;
            if (geoType == "Polygon") hasPolygon = true;
        }

        Assert.True(hasLineString, "Should have LineString route");
        Assert.True(hasPolygon, "Should have Polygon CGA");
    }

    [Fact]
    public async Task CustomGeometry_WithImportedGeoJson_ShouldUseImportedShape()
    {
        // Arrange
        var importedGeoJson = @"{
            ""type"": ""LineString"",
            ""coordinates"": [
                [23.7275, 37.9838, 50],
                [23.7285, 37.9848, 50],
                [23.7295, 37.9858, 50],
                [23.7305, 37.9868, 50]
            ]
        }";

        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Custom,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 50.0,
            UseImportedGeometry = true,
            ImportedGeoJson = importedGeoJson
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50.0m, result.MaxHeightAGL_m);
        Assert.True(result.RouteLength_m > 0, "Route length should be greater than 0");
        Assert.True(result.CgaArea_m2 > 0, "CGA area should be greater than 0");

        // Verify route matches imported geometry (4 waypoints)
        var geoJson = JsonDocument.Parse(result.GeoJson);
        var features = geoJson.RootElement.GetProperty("features");
        
        foreach (var feature in features.EnumerateArray())
        {
            if (feature.TryGetProperty("properties", out var props) &&
                props.TryGetProperty("name", out var name) &&
                name.GetString() == "route")
            {
                var geometry = feature.GetProperty("geometry");
                var coordinates = geometry.GetProperty("coordinates");
                Assert.Equal(4, coordinates.GetArrayLength()); // Should have exactly 4 waypoints from imported data
            }
        }
    }

    [Fact]
    public async Task CustomGeometry_WithoutImportedData_ShouldThrow()
    {
        // Arrange
        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Custom,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 50.0,
            UseImportedGeometry = true,
            ImportedGeoJson = null // Missing imported data
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GenerateAsync(request)
        );

        Assert.Contains("Custom mission requires imported geometry", exception.Message);
    }

    [Fact]
    public async Task AgricultureGeometry_ShouldGenerateLargeFieldPattern()
    {
        // Arrange
        var request = new GeometryGenerationRequest
        {
            MissionType = MissionType.Agriculture,
            CenterLat = 37.9838,
            CenterLon = 23.7275,
            MaxHeightAGL_m = 40.0,
            AreaWidth_m = 150.0,
            AreaLength_m = 200.0
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40.0m, result.MaxHeightAGL_m);
        Assert.True(result.RouteLength_m > 500, "Agriculture field route should be > 500m");
        Assert.True(result.CgaArea_m2 > 25000, "Agriculture field CGA should be > 25,000 m² (150x200m)");

        // Verify GeoJSON structure
        var geoJson = JsonDocument.Parse(result.GeoJson);
        Assert.Equal("FeatureCollection", geoJson.RootElement.GetProperty("type").GetString());
    }
}
