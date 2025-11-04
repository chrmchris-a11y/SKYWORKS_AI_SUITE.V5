using Xunit;
using Skyworks.Core.Services.Risk;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Api.Tests.Risk;

public class RiskModeling_Tests
{
    private readonly IRiskModelingService _risk = new RiskModelingService();

    [Fact]
    public void SAIL_I_ARC_A_NonUrban_VLOS_Is_Low()
    {
        var req = new RiskAssessmentRequest
        {
            SoraVersion = "2.5",
            FinalGRC = 2,
            ResidualARC = ARCRating.ARC_a,
            SAIL = SAILLevel.I,
            OperationType = "VLOS",
            Environment = "NonUrban",
            MissionDurationMinutes = 10
        };
        var res = _risk.AssessOperationalRisk(req);
        Assert.True(res.Score <= 3);
        Assert.Equal("Low", res.Band);
    }

    [Fact]
    public void SAIL_V_ARC_D_Urban_BVLOS_Is_HighOrVeryHigh()
    {
        var req = new RiskAssessmentRequest
        {
            FinalGRC = 6,
            ResidualARC = ARCRating.ARC_d,
            SAIL = SAILLevel.V,
            OperationType = "BVLOS",
            Environment = "Urban",
            MissionDurationMinutes = 45
        };
        var res = _risk.AssessOperationalRisk(req);
        Assert.True(res.Score >= 7);
        Assert.True(res.Band == "High" || res.Band == "VeryHigh");
    }
}
