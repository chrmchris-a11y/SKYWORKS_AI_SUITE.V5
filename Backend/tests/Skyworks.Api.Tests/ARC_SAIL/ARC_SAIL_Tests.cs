using Xunit;
using Skyworks.Core.Models.SAIL;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_SAIL_Tests
{
    private readonly IGRCCalculationService _svc = new GRCCalculationService();

    [Fact]
    public void ARC_V20_Explicit_ReturnsSame()
    {
        var res = _svc.DetermineInitialARC_V2_0(new ARCInitialInput { ExplicitARC = ARCRating.ARC_c });
        Assert.Equal(ARCRating.ARC_c, res.ARC);
    }

    [Fact]
    public void ARC_V25_Default_B()
    {
        var res = _svc.DetermineInitialARC_V2_5(new ARCInitialInput { });
        Assert.Equal(ARCRating.ARC_b, res.ARC);
    }

    [Theory]
    [InlineData(2, ARCRating.ARC_a, SAILLevel.I)]
    [InlineData(3, ARCRating.ARC_b, SAILLevel.III)]
    [InlineData(4, ARCRating.ARC_c, SAILLevel.IV)]
    [InlineData(5, ARCRating.ARC_d, SAILLevel.V)]
    public void SAIL_V20_Table5_Mapping(int finalGrc, ARCRating arc, SAILLevel expected)
    {
        var res = _svc.DetermineSAIL_V2_0(new SAILInput { FinalGRC = finalGrc, ResidualARC = arc });
        Assert.True(res.IsSupported);
        Assert.Equal(expected, res.SAIL);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(0)]
    public void SAIL_V25_OutOfScope_ReturnsNotSupported(int finalGrc)
    {
        var res = _svc.DetermineSAIL_V2_5(new SAILInput { FinalGRC = finalGrc, ResidualARC = ARCRating.ARC_b });
        Assert.False(res.IsSupported);
    }

    [Fact]
    public void TMPR_Mapping_V20_V25_IsConsistent()
    {
        var v20a = _svc.DetermineTMPR_V2_0(ARCRating.ARC_a);
        var v25a = _svc.DetermineTMPR_V2_5(ARCRating.ARC_a);
        Assert.Equal(TMPRLevel.None, v20a.Level);
        Assert.Equal(TMPRLevel.None, v25a.Level);

        var v20d = _svc.DetermineTMPR_V2_0(ARCRating.ARC_d);
        var v25d = _svc.DetermineTMPR_V2_5(ARCRating.ARC_d);
        Assert.Equal(TMPRLevel.VeryHigh, v20d.Level); // Corrected: ARC-d → VeryHigh
        Assert.Equal(TMPRLevel.VeryHigh, v25d.Level); // Corrected: ARC-d → VeryHigh
    }
}
