namespace Skyworks.Core.Models.ARC.Validation;

public enum RuleSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

public class ValidationIssue
{
    public required RuleSeverity Severity { get; set; }
    public required string Code { get; set; }
    public required string Message { get; set; }
}

public class ARCValidationResult
{
    public bool IsCompliant => Issues.All(i => i.Severity != RuleSeverity.Error);
    public List<ValidationIssue> Issues { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
