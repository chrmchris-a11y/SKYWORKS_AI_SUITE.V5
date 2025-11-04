using Xunit;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Models.ARC;
using System.Collections.Generic;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Residual_Tests
{
    private readonly IGRCCalculationService _svc = new GRCCalculationService();

    [Fact]
    public void V25_From_D_With_S1_S2_Goes_To_B_Capped()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_d,
            StrategicMitigations = new List<string> { "S1", "S2" }
        });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void V20_From_C_With_Many_Goes_To_B_Only()
    {
        var res = _svc.DetermineResidualARC_V2_0(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_c,
            StrategicMitigations = new List<string> { "S1", "S2", "S3" }
        });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void V25_From_B_With_S1_Stays_B_Typical()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_b,
            StrategicMitigations = new List<string> { "S1" }
        });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void V25_From_B_AtypicalSegregated_Goes_To_A()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_b,
            StrategicMitigations = new List<string> { "S1" },
            IsAtypicalSegregated = true
        });
        Assert.Equal(ARCRating.ARC_a, res.ARC);
    }

    [Fact]
    public void V20_From_A_Stays_A()
    {
        var res = _svc.DetermineResidualARC_V2_0(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_a,
            StrategicMitigations = new List<string> { "S1" }
        });
        Assert.Equal(ARCRating.ARC_a, res.ARC);
    }

    [Fact]
    public void V20_From_B_With_Density1_Stays_B_Typical()
    {
        var res = _svc.DetermineResidualARC_V2_0(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_b,
            LocalDensityRating = 1
        });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Fact]
    public void ExplicitResidualOverride_Wins()
    {
        var res = _svc.DetermineResidualARC_V2_5(new ARCResidualInput
        {
            InitialARC = ARCRating.ARC_d,
            ExplicitResidualARC = ARCRating.ARC_c,
            StrategicMitigations = new List<string> { "S1", "S2" }
        });
        Assert.Equal(ARCRating.ARC_c, res.ARC);
    }
}
