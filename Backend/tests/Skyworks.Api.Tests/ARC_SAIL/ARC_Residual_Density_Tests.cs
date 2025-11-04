using Xunit;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Residual_Density_Tests
{
    private readonly IGRCCalculationService _svc = new GRCCalculationService();

    [Fact]
    public void V25_Density_Reduction_D_to_C_When_3_or_4()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_d,
            LocalDensityRating = 4
        });
        Assert.Equal(ARCRating.ARC_c, res.ARC);
    }

    [Fact]
    public void V20_Density_Reduction_D_to_B_When_1_or_2()
    {
        var res = _svc.DetermineResidualARC_V2_0(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_d,
            LocalDensityRating = 2
        });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void V20_Density_Reduction_C_to_B_When_1()
    {
        var res = _svc.DetermineResidualARC_V2_0(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_c,
            LocalDensityRating = 1
        });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void V25_AtypicalSegregated_Claim_Goes_To_A()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_c,
            IsAtypicalSegregated = true
        });
        Assert.Equal(ARCRating.ARC_a, res.ARC);
    }

    [Fact]
    public void No_Double_Counting_Takes_Max_Of_Density_Or_S()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_d,
            LocalDensityRating = 4, // would give -1
            StrategicMitigations = new System.Collections.Generic.List<string>{"S1","S2","S3"} // would cap at -2
        });
        // Max of (-1 density vs -2 mitigations) => -2 from D -> B
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }
}
