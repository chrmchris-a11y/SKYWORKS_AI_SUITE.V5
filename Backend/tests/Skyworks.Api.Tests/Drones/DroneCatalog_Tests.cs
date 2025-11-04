using Skyworks.Core.Models.Drones;
using Skyworks.Infrastructure.Services.Drones;
using Xunit;

namespace Skyworks.Api.Tests.Drones;

public class DroneCatalog_Tests
{
    private readonly StaticDroneCatalogService _service;

    public DroneCatalog_Tests()
    {
        _service = new StaticDroneCatalogService();
    }

    [Fact]
    public async Task GetAllDrones_ReturnsNonEmptyList()
    {
        // Act
        var drones = await _service.GetAllDronesAsync();

        // Assert
        Assert.NotEmpty(drones);
        Assert.True(drones.Count >= 60, $"Expected >= 60 drones, got {drones.Count}");
    }

    [Fact]
    public async Task GetDroneById_DJI_Mini4Pro_ReturnsCorrectDrone()
    {
        // Act
        var drone = await _service.GetDroneByIdAsync("DJI_MINI4PRO");

        // Assert
        Assert.NotNull(drone);
        Assert.Equal("Mini 4 Pro", drone.Model);
        Assert.Equal("DJI", drone.Manufacturer);
        Assert.Equal(EUCClass.C1, drone.CClass);
        Assert.True(drone.MTOM_kg < 0.25);
        Assert.Equal("A", drone.SuggestedARC);
    }

    [Fact]
    public async Task GetDroneById_DJI_M350RTK_ReturnsCorrectDrone()
    {
        // Act
        var drone = await _service.GetDroneByIdAsync("DJI_M350RTK");

        // Assert
        Assert.NotNull(drone);
        Assert.Equal("M350 RTK", drone.Model);
        Assert.Equal(EUCClass.C3, drone.CClass);
        Assert.True(drone.MTOM_kg > 9);
        Assert.Equal("C", drone.SuggestedARC);
        Assert.True(drone.IsBVLOSCapable);
    }

    [Fact]
    public async Task SearchDrones_ByManufacturerDJI_ReturnsMultiple()
    {
        // Arrange
        var filter = new DroneCatalogFilter { Manufacturer = "DJI" };

        // Act
        var results = await _service.SearchDronesAsync(filter);

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, d => Assert.Contains("DJI", d.Manufacturer));
    }

    [Fact]
    public async Task SearchDrones_ByCClassC1_ReturnsSubKilogram()
    {
        // Arrange
        var filter = new DroneCatalogFilter { CClass = EUCClass.C1 };

        // Act
        var results = await _service.SearchDronesAsync(filter);

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, d => Assert.True(d.MTOM_kg < 0.91)); // C1: <900g (allow rounding)
    }

    [Fact]
    public async Task SearchDrones_BVLOSCapable_ReturnsMatching()
    {
        // Arrange
        var filter = new DroneCatalogFilter { IsBVLOSCapable = true };

        // Act
        var results = await _service.SearchDronesAsync(filter);

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, d => Assert.True(d.IsBVLOSCapable));
    }

    [Fact]
    public async Task GetDroneWithSORAHints_DJI_Agras_T30_ReturnsHighGRC()
    {
        // Act
        var result = await _service.GetDroneWithSORASuggestionsAsync("DJI_AGRAS_T30");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("D", result.SuggestedInitialARC);
        Assert.True(result.SuggestedGRC_Dimension >= 4); // Large dimension
        Assert.True(result.SuggestedGRC_Speed >= 3); // Moderate speed
        Assert.Contains(result.ComplianceNotes, note => note.Contains("Specific Category"));
    }

    [Fact]
    public async Task GetDroneWithSORAHints_ParrotANAFI_ReturnsLowGRC()
    {
        // Act
        var result = await _service.GetDroneWithSORASuggestionsAsync("PARROT_ANAFI_AI");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("B", result.SuggestedInitialARC);
        Assert.True(result.SuggestedGRC_Dimension <= 2); // Small dimension
        Assert.Contains(result.ComplianceNotes, note => note.Contains("Open Category"));
    }

    [Theory]
    [InlineData("DJI_MINI4PRO", true)] // Has Remote ID
    [InlineData("SC15", false)] // Generic, no Remote ID
    public async Task CheckRemoteIDCompliance(string droneId, bool expectedHasRemoteID)
    {
        // Act
        var drone = await _service.GetDroneByIdAsync(droneId);

        // Assert
        Assert.NotNull(drone);
        Assert.Equal(expectedHasRemoteID, drone.HasRemoteID);
    }

    [Fact]
    public async Task SearchDrones_CombinedFilters_ReturnsCorrectSubset()
    {
        // Arrange: Heavy agricultural drones
        var filter = new DroneCatalogFilter
        {
            MinMTOM_kg = 20,
            CClass = EUCClass.C3
        };

        // Act
        var results = await _service.SearchDronesAsync(filter);

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, d =>
        {
            Assert.True(d.MTOM_kg >= 20);
            Assert.Equal(EUCClass.C3, d.CClass);
        });
    }
}
