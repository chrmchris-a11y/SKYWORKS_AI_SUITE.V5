using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services.History;

public interface IHistoricalAnalysisService
{
    HistoricalAnalysisResult Analyze(HistoricalAnalysisRequest request);
}

public class HistoricalAnalysisRequest
{
    public IList<MissionRecord> Records { get; set; } = new List<MissionRecord>();
    /// <summary>
    /// Optional filter: "2.0" or "2.5"; when null, analyze all.
    /// </summary>
    public string? SoraVersion { get; set; }
}

public class MissionRecord
{
    public string? MissionId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int FinalGRC { get; set; }
    public ARCRating ResidualARC { get; set; }
    public SAILLevel SAIL { get; set; }
    public bool IsCompliant { get; set; }
    public double? RiskScore { get; set; }
    /// <summary>
    /// "2.0" for SORA 2.0 AMC, "2.5" for JARUS SORA 2.5
    /// </summary>
    public string SoraVersion { get; set; } = "2.5";
}

public class HistoricalAnalysisResult
{
    public int Count { get; set; }
    public Dictionary<string, int> ByVersion { get; set; } = new();
    public Dictionary<SAILLevel, int> BySAIL { get; set; } = new();
    public Dictionary<ARCRating, int> ByARC { get; set; } = new();
    public double ComplianceRate { get; set; }
    public double? AverageRiskScore { get; set; }
    public List<MonthlyCount> TrendMonthly { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class MonthlyCount
{
    public string Month { get; set; } = string.Empty; // yyyy-MM
    public int Count { get; set; }
}
