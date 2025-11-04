using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services.History;

public class HistoricalAnalysisService : IHistoricalAnalysisService
{
    public HistoricalAnalysisResult Analyze(HistoricalAnalysisRequest request)
    {
        var records = request.Records ?? new List<MissionRecord>();
        if (!string.IsNullOrWhiteSpace(request.SoraVersion))
            records = records.Where(r => string.Equals(r.SoraVersion, request.SoraVersion, StringComparison.OrdinalIgnoreCase)).ToList();

        var result = new HistoricalAnalysisResult();
        result.Count = records.Count;

        // By version
        result.ByVersion = records
            .GroupBy(r => (r.SoraVersion ?? "2.5").Trim())
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        // By SAIL
        foreach (SAILLevel level in Enum.GetValues(typeof(SAILLevel)))
            result.BySAIL[level] = 0;
        foreach (var g in records.GroupBy(r => r.SAIL))
            result.BySAIL[g.Key] = g.Count();

        // By ARC
        foreach (ARCRating arc in Enum.GetValues(typeof(ARCRating)))
            result.ByARC[arc] = 0;
        foreach (var g in records.GroupBy(r => r.ResidualARC))
            result.ByARC[g.Key] = g.Count();

        // Compliance rate
        if (records.Count > 0)
            result.ComplianceRate = Math.Round(100.0 * records.Count(r => r.IsCompliant) / records.Count, 2);

        // Average risk score
        var riskValues = records.Where(r => r.RiskScore.HasValue).Select(r => r.RiskScore!.Value).ToList();
        if (riskValues.Count > 0)
            result.AverageRiskScore = Math.Round(riskValues.Average(), 2);

        // Monthly trend (UTC)
        result.TrendMonthly = records
            .GroupBy(r => new { r.OccurredAtUtc.Year, r.OccurredAtUtc.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyCount { Month = $"{g.Key.Year:D4}-{g.Key.Month:D2}", Count = g.Count() })
            .ToList();

        // Notes emphasizing SORA version scope
        result.Notes = string.IsNullOrWhiteSpace(request.SoraVersion)
            ? "Analysis over mixed SORA versions (2.0 and/or 2.5), grouped in ByVersion."
            : $"Analysis filtered for SORA {request.SoraVersion}.";

        return result;
    }
}
