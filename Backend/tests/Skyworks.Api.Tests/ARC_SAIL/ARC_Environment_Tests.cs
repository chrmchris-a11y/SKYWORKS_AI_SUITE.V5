using Xunit;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Environment_Tests
{
    private readonly IGRCCalculationService _svc = new GRCCalculationService();

    [Fact]
    public void V25_AirportHeliport_Is_ARC_D()
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Controlled,
            LocationType = LocationType.Airport, // Fixed: Changed from AirportHeliport
            AirspaceClass = AirspaceClass.C, // Added: Required for airport classification
            Environment = EnvironmentType.Urban,
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 60
        };
        var res = _svc.DetermineInitialARC_V2_5(env);
        Assert.Equal(ARCRating.ARC_d, res.ARC);
    }

    [Fact]
    public void V20_Controlled_AtOrBelow120_Is_ARC_C()
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Controlled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Urban,
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 120
        };
        var res = _svc.DetermineInitialARC_V2_0(env);
        Assert.Equal(ARCRating.ARC_c, res.ARC);
    }

    [Fact]
    public void V25_Uncontrolled_Urban_AtOrBelow120_Is_ARC_C() // Fixed: AEC 9 → ARC-c (SORA Annex C Table 1)
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Urban,
            Typicality = AirspaceTypicality.Typical,
            MaxHeightAGL = 100,
            PopulationDensity = 2000 // >1500 ppl/km² for urban
        };
        var res = _svc.DetermineInitialARC_V2_5(env);
        Assert.Equal(ARCRating.ARC_c, res.ARC); // AEC 9: VLL Uncontrolled Urban
        Assert.Equal(9, res.AEC); // Verify correct AEC category
        Assert.Equal(2, res.DensityRating); // Density rating 2 for VLL urban
    }

    [Fact]
    public void V20_AtypicalSegregated_Is_ARC_A()
    {
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Rural,
            Typicality = AirspaceTypicality.AtypicalSegregated,
            MaxHeightAGL = 400
        };
        var res = _svc.DetermineInitialARC_V2_0(env);
        Assert.Equal(ARCRating.ARC_a, res.ARC);
    }
}

