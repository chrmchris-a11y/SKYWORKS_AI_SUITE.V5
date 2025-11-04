using Xunit;
using Skyworks.Core.Services.History;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Api.Tests.History;

public class HistoricalAnalysis_Tests
{
    private static List<MissionRecord> SampleRecords()
    {
        var baseDate = new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        return new List<MissionRecord>
        {
            new() { MissionId = "M1", OccurredAtUtc = baseDate.AddDays(0),  SoraVersion="2.5", FinalGRC=2, ResidualARC=ARCRating.ARC_a, SAIL=SAILLevel.I,  IsCompliant=true,  RiskScore=2.0 },
            new() { MissionId = "M2", OccurredAtUtc = baseDate.AddDays(1),  SoraVersion="2.5", FinalGRC=3, ResidualARC=ARCRating.ARC_b, SAIL=SAILLevel.II, IsCompliant=true,  RiskScore=3.5 },
            new() { MissionId = "M3", OccurredAtUtc = baseDate.AddDays(2),  SoraVersion="2.5", FinalGRC=4, ResidualARC=ARCRating.ARC_b, SAIL=SAILLevel.III,IsCompliant=false, RiskScore=5.0 },
            new() { MissionId = "M4", OccurredAtUtc = baseDate.AddDays(15), SoraVersion="2.0", FinalGRC=5, ResidualARC=ARCRating.ARC_c, SAIL=SAILLevel.III,IsCompliant=true,  RiskScore=6.0 },
            new() { MissionId = "M5", OccurredAtUtc = baseDate.AddDays(20), SoraVersion="2.0", FinalGRC=6, ResidualARC=ARCRating.ARC_c, SAIL=SAILLevel.IV, IsCompliant=false, RiskScore=7.2 },
            new() { MissionId = "M6", OccurredAtUtc = baseDate.AddDays(30), SoraVersion="2.5", FinalGRC=2, ResidualARC=ARCRating.ARC_a, SAIL=SAILLevel.I,  IsCompliant=true,  RiskScore=null },
            new() { MissionId = "M7", OccurredAtUtc = baseDate.AddDays(31), SoraVersion="2.5", FinalGRC=7, ResidualARC=ARCRating.ARC_d, SAIL=SAILLevel.V,  IsCompliant=false, RiskScore=8.5 },
            new() { MissionId = "M8", OccurredAtUtc = baseDate.AddDays(35), SoraVersion="2.0", FinalGRC=1, ResidualARC=ARCRating.ARC_a, SAIL=SAILLevel.I,  IsCompliant=true,  RiskScore=1.0 }
        };
    }

    [Fact]
    public void Aggregates_By_SAIL_Version_Compliance_Trend()
    {
        var svc = new HistoricalAnalysisService();
        var req = new HistoricalAnalysisRequest { Records = SampleRecords() };
        var res = svc.Analyze(req);

        Assert.Equal(8, res.Count);
        Assert.True(res.ByVersion.ContainsKey("2.0") && res.ByVersion.ContainsKey("2.5"));
        Assert.Equal(3, res.ByVersion["2.0"]);
        Assert.Equal(5, res.ByVersion["2.5"]);

        // SAIL counts
        Assert.True(res.BySAIL[SAILLevel.I] >= 2);
        Assert.True(res.BySAIL[SAILLevel.III] >= 2);

        // Compliance rate (5/8 = 62.5%)
        Assert.Equal(62.5, res.ComplianceRate, 1);

        // Average risk: ignore null → (2.0+3.5+5.0+6.0+7.2+8.5+1.0)/7 = 33.2/7 ≈ 4.74
        Assert.True(res.AverageRiskScore.HasValue);
        Assert.InRange(res.AverageRiskScore!.Value, 4.7, 4.8);

        // Monthly trend across two months
        Assert.True(res.TrendMonthly.Count >= 2);
        Assert.Matches("^2025-09$|^2025-10$", res.TrendMonthly[0].Month);
    }

    [Fact]
    public void Filters_By_SoraVersion()
    {
        var svc = new HistoricalAnalysisService();
        var req = new HistoricalAnalysisRequest { Records = SampleRecords(), SoraVersion = "2.0" };
        var res = svc.Analyze(req);

        Assert.Equal(3, res.Count);
        Assert.Equal(3, res.ByVersion["2.0"]);
        Assert.False(res.ByVersion.ContainsKey("2.5"));
        Assert.Contains("SORA 2.0", res.Notes);
    }
}
