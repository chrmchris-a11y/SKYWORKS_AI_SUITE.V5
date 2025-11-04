using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Skyworks.Api.Controllers;
using System.Collections;
using Xunit;

namespace Skyworks.Api.Tests.PDRA_STS;

public class PDRA_STS_Evaluation_Tests
{
    private class FakeLocalizer : IStringLocalizer<Resources.SharedResources>
    {
        public LocalizedString this[string name]
            => new(name, name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments]
            => new(name, string.Format(name, arguments), resourceNotFound: false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => Array.Empty<LocalizedString>();

        public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture)
            => this;
    }

    private PDRASTSController CreateController()
        => new(new FakeLocalizer());

    [Fact]
    public void STS01_Valid_Request_Returns_Approved_With_Expected_Metrics()
    {
        var controller = CreateController();
        var request = new PDRASTSEvaluationRequest
        {
            Category = "STS-01",
            MissionId = "T_STS01_OK",
            UasClass = "C5",
            ControlledGroundArea = true,
            MaxHeight = 120,
            MaxSpeed = 5,
            Vlos = true
        };

        var action = controller.EvaluatePDRASTS(request);
        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<PDRASTSEvaluationResult>(ok.Value);

        Assert.True(result.Approved);
        Assert.Equal("GRC_3", result.Grc);
        Assert.Equal("ARC_b", result.Arc);
        Assert.Equal("SAIL_II", result.Sail);
    }

    [Fact]
    public void STS02_Invalid_Class_Is_Rejected()
    {
        var controller = CreateController();
        var request = new PDRASTSEvaluationRequest
        {
            Category = "STS-02",
            MissionId = "T_STS02_BAD_CLASS",
            UasClass = "C5", // should be C6
            MaxHeight = 120,
            FlightRange = 1000,
            MeteorologicalVisibility = 5,
            LaunchRecoveryVlos = true,
            PopulationDensity = 100
        };

        var action = controller.EvaluatePDRASTS(request);
        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<PDRASTSEvaluationResult>(ok.Value);

        Assert.False(result.Approved);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public void PDRAS01_Valid_Request_Returns_Approved_With_Expected_Metrics()
    {
        var controller = CreateController();
        var request = new PDRASTSEvaluationRequest
        {
            Category = "PDRA-S01",
            MissionId = "T_PDRAS01_OK",
            MaxDimension = 2.5,
            MaxHeight = 120,
            Vlos = true,
            ControlledGroundArea = true,
            AutomaticMode = false
        };

        var action = controller.EvaluatePDRASTS(request);
        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<PDRASTSEvaluationResult>(ok.Value);

        Assert.True(result.Approved);
        Assert.Equal("GRC_3", result.Grc);
        Assert.Equal("ARC_b", result.Arc);
        Assert.Equal("SAIL_II", result.Sail);
    }
}
