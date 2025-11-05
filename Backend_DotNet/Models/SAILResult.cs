using System.Collections.Generic;

namespace SORA.Compliance.Services
{
    /// <summary>
    /// Result of SAIL calculation.
    /// </summary>
    public class SAILResult
    {
        public bool Success { get; set; }
        public string? Sail { get; set; }
        public string? Category { get; set; }
        public int? OsoCount { get; set; }
        public string? ErrorMessage { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response for OSO requirements.
    /// </summary>
    public class OSOResponse
    {
        public bool Success { get; set; }
        public int? OsoCount { get; set; }
        public List<OSORequirement> Requirements { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    /// <summary>
    /// OSO requirement definition.
    /// </summary>
    public class OSORequirement
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Robustness { get; set; }
        public string? ApplicabilityNote { get; set; }
    }

    /// <summary>
    /// Complete SAIL mapping table.
    /// </summary>
    public class MappingTable
    {
        public bool Success { get; set; }
        public Dictionary<string, Dictionary<string, string>> Table { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    // Internal DTOs for Python API responses
    internal class SAILCalculationResponse
    {
        public string? Sail { get; set; }
        public string? Category { get; set; }
    }

    internal class OSORequirementsResponse
    {
        public int? OsoCount { get; set; }
        public List<OSORequirement> Requirements { get; set; } = new();
    }
}

