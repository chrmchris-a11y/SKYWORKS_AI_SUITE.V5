using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.ARC.Composite;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services.Airspace;

namespace Skyworks.Core.Services.ARC;

public class ARCCompositeService : IARCCompositeService
{
    private readonly IGRCCalculationService _arc;
    private readonly IAirspaceProvider? _airspace;

    public ARCCompositeService(IGRCCalculationService arc, IAirspaceProvider? airspace = null)
    {
        _arc = arc;
        _airspace = airspace;
    }

    public CompositeArcResult ComputeInitial_V2_5(CompositeArcRequest request)
    {
        var result = new CompositeArcResult();
        if (request.Segments == null || request.Segments.Count == 0)
        {
            result.Notes = "No segments provided.";
            result.OverallInitialARC = ARCRating.ARC_a;
            result.Histogram = new() { { ARCRating.ARC_a, 0 }, { ARCRating.ARC_b, 0 }, { ARCRating.ARC_c, 0 }, { ARCRating.ARC_d, 0 } };
            return result;
        }

        var hist = new Dictionary<ARCRating, int>
        {
            { ARCRating.ARC_a, 0 }, { ARCRating.ARC_b, 0 }, { ARCRating.ARC_c, 0 }, { ARCRating.ARC_d, 0 }
        };

        var segmentResults = new List<CompositeArcSegmentResult>();
        double totalDuration = 0;

        foreach (var seg in request.Segments)
        {
            var arcRes = _arc.DetermineInitialARC_V2_5(seg.Environment);
            hist[arcRes.ARC]++;

            // Compute duration (explicit or from time span)
            double duration = seg.DurationMinutes ?? 0;
            if (duration == 0 && seg.StartTimeUtc.HasValue && seg.EndTimeUtc.HasValue)
            {
                duration = (seg.EndTimeUtc.Value - seg.StartTimeUtc.Value).TotalMinutes;
            }
            totalDuration += duration;

            var segRes = new CompositeArcSegmentResult
            {
                Name = seg.Name,
                InitialARC = arcRes.ARC,
                DurationMinutes = duration > 0 ? duration : null,
                Notes = arcRes.Notes
            };

            // High-risk rules
            if (request.ApplyHighRiskRules && arcRes.ARC == ARCRating.ARC_d)
            {
                segRes.Warnings.Add("ARC-d: Ελέγξτε Annex B απαιτήσεις για tactical mitigations & coordination.");
            }

            segmentResults.Add(segRes);
        }

        // Time-weighted profile
        Dictionary<ARCRating, double>? timeProfile = null;
        if (request.ComputeTimeWeightedProfile && totalDuration > 0)
        {
            timeProfile = new Dictionary<ARCRating, double>
            {
                { ARCRating.ARC_a, 0 }, { ARCRating.ARC_b, 0 }, { ARCRating.ARC_c, 0 }, { ARCRating.ARC_d, 0 }
            };
            foreach (var sr in segmentResults)
            {
                if (sr.DurationMinutes.HasValue)
                {
                    double pct = (sr.DurationMinutes.Value / totalDuration) * 100.0;
                    sr.TimeWeightPercent = pct;
                    timeProfile[sr.InitialARC] += pct;
                }
            }
        }

        var overall = segmentResults.Select(s => s.InitialARC).DefaultIfEmpty(ARCRating.ARC_a).Max();
        result.SegmentResults = segmentResults;
        result.OverallInitialARC = overall;
        result.Histogram = hist;
        result.TimeWeightedProfile = timeProfile;

        // Compliance warnings
        if (overall == ARCRating.ARC_d)
        {
            result.ComplianceWarnings.Add("Overall ARC-d: Απαιτείται High TMPR & tactical mitigations (Annex B).");
        }
        if (timeProfile != null && timeProfile[ARCRating.ARC_d] > 50)
        {
            result.ComplianceWarnings.Add($"Περισσότερο από 50% του χρόνου σε ARC-d ({timeProfile[ARCRating.ARC_d]:F1}%) — εξετάστε στρατηγικές μειώσεις.");
        }

        result.Notes = request.ComputeTimeWeightedProfile
            ? $"Overall = max ARC across segments (conservative). Time-weighted profile: {totalDuration:F1} min total."
            : "Overall = max ARC across segments (conservative).";
        return result;
    }
}
