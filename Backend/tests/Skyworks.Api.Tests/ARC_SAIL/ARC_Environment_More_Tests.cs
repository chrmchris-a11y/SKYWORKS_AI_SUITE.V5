using Xunit;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Environment_More_Tests
{
    private readonly IGRCCalculationService _svc = new GRCCalculationService();

    [Fact]
    public void V25_Controlled_Above500ft_Is_ARC_D()
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Controlled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Urban,
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 200  // Above 500ft (152m)
        };
        var res = _svc.DetermineInitialARC_V2_5(env);
        Assert.Equal(ARCRating.ARC_d, res.ARC);
    }

    [Fact]
    public void V20_Uncontrolled_Rural_Below500ft_Is_ARC_B()
    {
        // JARUS Annex C Table 1: AEC 10 - Uncontrolled <500ft over Rural = ARC-b
        // There is NO "Remote" environment in JARUS specs
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Rural,
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 50
        };
        var res = _svc.DetermineInitialARC_V2_0(env);
        Assert.Equal(ARCRating.ARC_b, res.ARC);  // Rural uncontrolled = ARC-b per JARUS
    }

    [Fact]
    public void V25_Uncontrolled_Suburban_Below500ft_Is_ARC_B()
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Suburban, // maps to Urban-like
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 100
        };
        var res = _svc.DetermineInitialARC_V2_5(env);
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void V25_Uncontrolled_Industrial_Above500ft_Is_ARC_C()
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Industrial, // maps to Urban-like
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 180 // >500ft (~152m)
        };
        var res = _svc.DetermineInitialARC_V2_5(env);
        Assert.Equal(ARCRating.ARC_c, res.ARC);
    }
}

