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
        var controller = new MissionsController(orchestrator);

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
        var controller = new MissionsController(orchestrator);

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

    #region Helper: Mock Orchestrator

    /// <summary>
    /// Creates a simple mock orchestrator that returns a valid mission ID.
    /// For more comprehensive testing, use the real orchestrator with in-memory deps.
    /// </summary>
    private IMissionOrchestratorService CreateMockOrchestrator()
    {
        return new SimpleMockOrchestratorService();
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

    #endregion
}
