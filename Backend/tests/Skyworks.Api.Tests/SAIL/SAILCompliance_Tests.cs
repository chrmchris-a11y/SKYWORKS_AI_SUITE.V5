using Xunit;
using Skyworks.Core.Services;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Api.Tests.SAIL;

public class SAILCompliance_Tests
{
    private readonly ISAILComplianceService _svc;

    public SAILCompliance_Tests()
    {
        var sail = new SAILService();
        var oso = new OSOService();
        var grc = new GRCCalculationService();
        _svc = new SAILComplianceService(sail, oso, grc);
    }

    [Fact]
    public void Compliance_SORA25_SAIL_I_Partial_Missing_OSOs_Fails()
    {
        var req = new SAILComplianceRequest
        {
            FinalGRC = 3, // with ARC-a → SAIL I per corrected matrix
            ResidualARC = ARCRating.ARC_a,
            SoraVersion = "2.5",
            ImplementedOSOs = new List<OSOImplementation>
            {
                new() { OSOId = 1, AchievedRobustness = OSORobustnessLevel.Low },
                new() { OSOId = 3, AchievedRobustness = OSORobustnessLevel.Low }
            }
        };

        var res = _svc.CheckCompliance(req);

        Assert.False(res.IsCompliant);
        Assert.Equal(SAILLevel.I, res.SAIL); // Corrected per authoritative matrix
        Assert.NotNull(res.TMPR);
        Assert.NotNull(res.OSOCompliance);
        Assert.True(res.OSOCompliance!.MissingOSOs.Count > 0);
        Assert.Contains("OSO", res.Notes);
    }

    [Fact]
    public void Compliance_SORA25_SAIL_III_All_High_Passes()
    {
        // For test purpose, assume SAIL III and provide high robustness for most OSOs
        var req = new SAILComplianceRequest
        {
            FinalGRC = 4, // ARC-a → SAIL III
            ResidualARC = ARCRating.ARC_a,
            SoraVersion = "2.5",
            ImplementedOSOs = Enumerable.Range(1, 24)
                .Select(i => new OSOImplementation { OSOId = i, AchievedRobustness = OSORobustnessLevel.High })
                .ToList()
        };

        var res = _svc.CheckCompliance(req);
        Assert.True(res.IsCompliant);
        Assert.Equal(SAILLevel.III, res.SAIL);
        Assert.Equal(TMPRLevel.None, res.TMPR!.Level); // ARC-a
    }

    [Fact]
    public void Compliance_SORA20_vs_25_Different_OSO_Set()
    {
        // Same SAIL (I per corrected matrix), but SORA 2.0 expects the full 24 OSOs to exist
        var req20 = new SAILComplianceRequest
        {
            FinalGRC = 3, ResidualARC = ARCRating.ARC_a, SoraVersion = "2.0",
            ImplementedOSOs = new List<OSOImplementation>() // empty → should be non-compliant
        };
        var res20 = _svc.CheckCompliance(req20);

        var req25 = new SAILComplianceRequest
        {
            FinalGRC = 3, ResidualARC = ARCRating.ARC_a, SoraVersion = "2.5",
            ImplementedOSOs = new List<OSOImplementation>()
        };
        var res25 = _svc.CheckCompliance(req25);

        Assert.Equal(SAILLevel.I, res20.SAIL); // Corrected per authoritative matrix
        Assert.Equal(SAILLevel.I, res25.SAIL); // Corrected per authoritative matrix
        // Both should fail since no OSOs implemented
        Assert.False(res20.IsCompliant);
        Assert.False(res25.IsCompliant);
        // Both should have missing OSOs
        Assert.NotEmpty(res20.OSOCompliance!.MissingOSOs);
        Assert.NotEmpty(res25.OSOCompliance!.MissingOSOs);
    }
}
