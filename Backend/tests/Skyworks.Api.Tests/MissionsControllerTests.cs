using Microsoft.AspNetCore.Mvc;
using Skyworks.Api.Controllers;
using Skyworks.Core.Models;
using Skyworks.Core.Services;
using Skyworks.Core.Services.DTOs;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Skyworks.Api.Tests;

/// <summary>
/// Tests for MissionsController auto-create endpoint.
/// </summary>
public class MissionsControllerTests
{
    #region Test 1: Happy Path - Valid Request

    [Fact]
    public async Task AutoCreateMission_ValidRequest_ShouldReturn200WithMissionId()
    {
        // Arrange
        var orchestrator = CreateMockOrchestrator();
        var repository = CreateMockRepository();
        var controller = new MissionsController(orchestrator, repository);

        var request = new CreateMissionRequest
        {
            TemplateCode = MissionTemplateCode.PhotovoltaicParkInspection,
            CenterLat = 41.0082,
            CenterLon = 28.9784,
            MaxHeightAGL_m = 60,
            DroneModel = "DJI Matrice 300 RTK",
            DroneMtom_kg = 6.3,
            DroneClass = "C3"
        };

        // Act
        var result = await controller.AutoCreateMission(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CreateMissionResponse>(okResult.Value);
        Assert.NotEqual(Guid.Empty, response.MissionId);
    }

    #endregion

    #region Test 2: Invalid Payload - Missing Required Fields

    [Fact]
    public async Task AutoCreateMission_MissingRequiredFields_ShouldReturn400()
    {
        // Arrange
        var orchestrator = CreateMockOrchestrator();
        var repository = CreateMockRepository();
        var controller = new MissionsController(orchestrator, repository);

        // Create invalid request (missing drone info)
        var request = new CreateMissionRequest
        {
            TemplateCode = MissionTemplateCode.PhotovoltaicParkInspection,
            CenterLat = 41.0082,
            CenterLon = 28.9784,
            MaxHeightAGL_m = 60,
            // Missing DroneModel, DroneMtom_kg, DroneClass
            DroneModel = null!,
            DroneClass = null!
        };

        // Simulate ModelState error (normally done by ASP.NET Core)
        controller.ModelState.AddModelError("DroneModel", "The DroneModel field is required.");

        // Act
        var result = await controller.AutoCreateMission(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    #endregion

    #region Test 3: GET /missions/{id}/overview - Success

    [Fact]
    public async Task GetMissionOverview_ValidId_ShouldReturn200WithData()
    {
        // Arrange
        var orchestrator = CreateMockOrchestrator();
        var repository = CreateMockRepository();
        var controller = new MissionsController(orchestrator, repository);
        var missionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await controller.GetMissionOverview(missionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MissionOverviewResponse>(okResult.Value);
        Assert.Equal(missionId, response.MissionId);
        Assert.Equal("Test Mission", response.Name);
        Assert.Equal("PhotovoltaicParkInspection", response.TemplateCode);
    }

    #endregion

    #region Test 4: GET /missions/{id}/overview - Not Found

    [Fact]
    public async Task GetMissionOverview_InvalidId_ShouldReturn404()
    {
        // Arrange
        var orchestrator = CreateMockOrchestrator();
        var repository = CreateMockRepository();
        var controller = new MissionsController(orchestrator, repository);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.GetMissionOverview(nonExistentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region Test 5: GET /missions/{id}/overview - CGA Extraction Success

    [Fact]
    public async Task GetMissionOverview_WithCgaInGeoJson_ShouldExtractControlledGroundAreaGeoJson()
    {
        // Arrange
        var orchestrator = CreateMockOrchestrator();
        var repository = CreateMockRepositoryWithCGA();
        var controller = new MissionsController(orchestrator, repository);
        var missionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        var result = await controller.GetMissionOverview(missionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MissionOverviewResponse>(okResult.Value);
        
        Assert.NotNull(response.Geometry);
        Assert.NotNull(response.Geometry.ControlledGroundAreaGeoJson);
        Assert.Contains("Polygon", response.Geometry.ControlledGroundAreaGeoJson);
        Assert.Contains("13.4", response.Geometry.ControlledGroundAreaGeoJson);
    }

    #endregion

    #region Test 6: GET /missions/{id}/overview - No CGA in GeoJson

    [Fact]
    public async Task GetMissionOverview_WithoutCga_ShouldReturnNullControlledGroundAreaGeoJson()
    {
        // Arrange
        var orchestrator = CreateMockOrchestrator();
        var repository = CreateMockRepository(); // Uses GeoJSON without CGA
        var controller = new MissionsController(orchestrator, repository);
        var missionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await controller.GetMissionOverview(missionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MissionOverviewResponse>(okResult.Value);
        
        Assert.NotNull(response.Geometry);
        Assert.Null(response.Geometry.ControlledGroundAreaGeoJson); // No CGA in mock data
    }

    #endregion

    #region Helper: Mock Orchestrator

    /// <summary>
    /// Creates a simple mock orchestrator that returns a valid mission ID.
    /// For more comprehensive testing, use the real orchestrator with in-memory deps.
    /// </summary>
    private IMissionOrchestratorService CreateMockOrchestrator()
    {
        return new SimpleMockOrchestratorService();
    }

    private IMissionRepository CreateMockRepository()
    {
        return new SimpleMockRepositoryService();
    }

    private IMissionRepository CreateMockRepositoryWithCGA()
    {
        return new SimpleMockRepositoryWithCGAService();
    }

    private class SimpleMockOrchestratorService : IMissionOrchestratorService
    {
        public Task<Guid> CreateMissionAsync(CreateMissionRequest request)
        {
            // Basic validation
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Return a new mission ID
            return Task.FromResult(Guid.NewGuid());
        }
    }

    private class SimpleMockRepositoryService : IMissionRepository
    {
        public Task<Mission?> GetMissionByIdAsync(Guid id)
        {
            if (id == Guid.Parse("11111111-1111-1111-1111-111111111111"))
            {
                return Task.FromResult<Mission?>(new Mission
                {
                    MissionId = id,
                    Name = "Test Mission",
                    TemplateCode = MissionTemplateCode.PhotovoltaicParkInspection,
                    Category = MissionCategory.EnergyAndUtilities,
                    Type = MissionType.Solar,
                    Geometry = new MissionGeometry 
                    { 
                        GeoJson = "{\"type\":\"LineString\",\"coordinates\":[[13.4,52.5],[13.5,52.6]]}",
                        RouteLength_m = 1500,
                        CgaArea_m2 = 5000,
                        MaxHeightAGL_m = 100
                    },
                    SoraAssessment = new MissionSoraAssessment
                    {
                        InitialGrc = 5,
                        FinalGrc = 3,
                        InitialArc = "c",
                        ResidualArc = "a",
                        Sail = "II",
                        InputsSnapshotJson = "{}"
                    },
                    Erp = new MissionErp { ErpJson = "{}", ErpText = "Emergency plan" },
                    OsoCoverage = new MissionOsoCoverage 
                    { 
                        RequiredOsosJson = "[\"OSO#1\",\"OSO#2\"]",
                        CoveredOsosJson = "[\"OSO#1\"]",
                        MissingOsosJson = "[\"OSO#2\"]"
                    }
                });
            }
            return Task.FromResult<Mission?>(null);
        }

        public Task<Guid> SaveMissionAsync(Mission mission)
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }

    private class SimpleMockRepositoryWithCGAService : IMissionRepository
    {
        public Task<Mission?> GetMissionByIdAsync(Guid id)
        {
            if (id == Guid.Parse("22222222-2222-2222-2222-222222222222"))
            {
                // GeoJSON FeatureCollection with CGA polygon
                var geoJsonWithCGA = @"{
                    ""type"": ""FeatureCollection"",
                    ""features"": [
                        {
                            ""type"": ""Feature"",
                            ""properties"": { ""type"": ""route"" },
                            ""geometry"": {
                                ""type"": ""LineString"",
                                ""coordinates"": [[13.4, 52.5], [13.5, 52.6]]
                            }
                        },
                        {
                            ""type"": ""Feature"",
                            ""properties"": { ""type"": ""cga"", ""name"": ""Controlled Ground Area"" },
                            ""geometry"": {
                                ""type"": ""Polygon"",
                                ""coordinates"": [
                                    [[13.4, 52.5], [13.5, 52.5], [13.5, 52.6], [13.4, 52.6], [13.4, 52.5]]
                                ]
                            }
                        }
                    ]
                }";

                return Task.FromResult<Mission?>(new Mission
                {
                    MissionId = id,
                    Name = "Mission with CGA",
                    TemplateCode = MissionTemplateCode.PhotovoltaicParkInspection,
                    Category = MissionCategory.EnergyAndUtilities,
                    Type = MissionType.Solar,
                    Geometry = new MissionGeometry
                    {
                        GeoJson = geoJsonWithCGA,
                        RouteLength_m = 2000,
                        CgaArea_m2 = 8000,
                        MaxHeightAGL_m = 120
                    },
                    SoraAssessment = new MissionSoraAssessment
                    {
                        InitialGrc = 6,
                        FinalGrc = 4,
                        InitialArc = "c",
                        ResidualArc = "b",
                        Sail = "III"
                    },
                    Erp = new MissionErp { ErpJson = "{}", ErpText = "ERP with safe area" },
                    OsoCoverage = new MissionOsoCoverage
                    {
                        RequiredOsosJson = "[\"OSO#1\"]",
                        CoveredOsosJson = "[\"OSO#1\"]",
                        MissingOsosJson = "[]"
                    }
                });
            }
            return Task.FromResult<Mission?>(null);
        }

        public Task<Guid> SaveMissionAsync(Mission mission)
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }

    #endregion
}
