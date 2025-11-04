using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Models.ARC.Composite;

public class CompositeSegmentInput
{
    public string? Name { get; set; }
    public DateTimeOffset? StartTimeUtc { get; set; }
    public DateTimeOffset? EndTimeUtc { get; set; }
    public double? DurationMinutes { get; set; }
    public required ARCEnvironmentInput Environment { get; set; }
    public GeoPoint? CenterPoint { get; set; }
    public List<GeoPoint>? OperationalArea { get; set; }
}

public class CompositeArcRequest
{
    public List<CompositeSegmentInput> Segments { get; set; } = new();
    public bool ComputeTimeWeightedProfile { get; set; } = false;
    public bool ApplyHighRiskRules { get; set; } = true;
}

public class CompositeArcSegmentResult
{
    public string? Name { get; set; }
    public required ARCRating InitialARC { get; set; }
    public double? DurationMinutes { get; set; }
    public double? TimeWeightPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}

public class CompositeArcResult
{
    public List<CompositeArcSegmentResult> SegmentResults { get; set; } = new();
    public ARCRating OverallInitialARC { get; set; }
    public Dictionary<ARCRating, int> Histogram { get; set; } = new();
    public Dictionary<ARCRating, double>? TimeWeightedProfile { get; set; }
    public List<string> ComplianceWarnings { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
