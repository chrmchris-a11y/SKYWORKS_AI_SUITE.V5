using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.ARC.Composite;
using Skyworks.Core.Services.ARC;
using Skyworks.Core.Services.GRC;
using Xunit;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Composite_Tests
{
    [Fact]
    public void Overall_is_max_segment_arc()
    {
        var grc = new GRCCalculationService();
        var composite = new ARCCompositeService(grc);

        var req = new CompositeArcRequest
        {
            Segments =
            [
                new CompositeSegmentInput
                {
                    Name = "Seg1",
                    Environment = new ARCEnvironmentInput
                    {
                        AirspaceControl = AirspaceControl.Uncontrolled,
                        LocationType = LocationType.NonAirport,
                        Environment = EnvironmentType.Rural,
                        MaxHeightAGL = 60
                    }
                },
                new CompositeSegmentInput
                {
                    Name = "Seg2",
                    Environment = new ARCEnvironmentInput
                    {
                        AirspaceControl = AirspaceControl.Uncontrolled,
                        LocationType = LocationType.NonAirport,
                        Environment = EnvironmentType.Urban,
                        MaxHeightAGL = 60
                    }
                },
                new CompositeSegmentInput
                {
                    Name = "Seg3",
                    Environment = new ARCEnvironmentInput
                    {
                        AirspaceControl = AirspaceControl.Controlled,
                        LocationType = LocationType.NonAirport,
                        Environment = EnvironmentType.Urban,
                        MaxHeightAGL = 200  // Above 500ft (152m) to get ARC-d
                    }
                }
            ]
        };

        var res = composite.ComputeInitial_V2_5(req);
        Assert.Equal(ARCRating.ARC_d, res.OverallInitialARC); // Seg3 should be D
        Assert.Equal(3, res.SegmentResults.Count);
    }

    [Fact]
    public void Empty_request_returns_A_and_notes()
    {
        var grc = new GRCCalculationService();
        var composite = new ARCCompositeService(grc);
        var res = composite.ComputeInitial_V2_5(new CompositeArcRequest());
        Assert.Equal(ARCRating.ARC_a, res.OverallInitialARC);
        Assert.Contains("No segments", res.Notes);
    }

    [Fact]
    public void Time_weighted_profile_computed_when_enabled()
    {
        var grc = new GRCCalculationService();
        var composite = new ARCCompositeService(grc);

        var req = new CompositeArcRequest
        {
            ComputeTimeWeightedProfile = true,
            Segments =
            [
                new CompositeSegmentInput
                {
                    Name = "Seg1",
                    DurationMinutes = 10,
                    Environment = new ARCEnvironmentInput
                    {
                        AirspaceControl = AirspaceControl.Uncontrolled,
                        LocationType = LocationType.NonAirport,
                        Environment = EnvironmentType.Rural,
                        MaxHeightAGL = 60
                    }
                },
                new CompositeSegmentInput
                {
                    Name = "Seg2",
                    DurationMinutes = 30,
                    Environment = new ARCEnvironmentInput
                    {
                        AirspaceControl = AirspaceControl.Controlled,
                        LocationType = LocationType.NonAirport,
                        Environment = EnvironmentType.Urban,
                        MaxHeightAGL = 200  // Above 500ft to get ARC-d
                    }
                }
            ]
        };

        var res = composite.ComputeInitial_V2_5(req);
        Assert.NotNull(res.TimeWeightedProfile);
        Assert.True(res.TimeWeightedProfile[ARCRating.ARC_d] > 70); // 30/40 = 75%
        Assert.NotEmpty(res.ComplianceWarnings);
    }
}

