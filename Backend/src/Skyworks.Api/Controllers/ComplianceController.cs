using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Models.Compliance;
using Skyworks.Core.Services.Compliance;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComplianceController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IComplianceProvider? _complianceProvider;

    public ComplianceController(IWebHostEnvironment env, IComplianceProvider? complianceProvider = null)
    {
        _env = env;
        _complianceProvider = complianceProvider;
    }

    private string WorkspaceRoot => Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "..", ".."));

    #region Step 28 - Compliance Obligations & Checklists

    /// <summary>
    /// Get compliance obligations for a mission based on parameters (ARC, airspace, operation type)
    /// </summary>
    [HttpPost("obligations")]
    public async Task<ActionResult<ComplianceResult>> GetObligations([FromBody] ComplianceRequest request)
    {
        if (_complianceProvider == null)
        {
            return StatusCode(500, new { error = "Compliance provider not configured" });
        }

        var result = await _complianceProvider.GetComplianceObligationsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get pre-flight checklist for a mission
    /// </summary>
    [HttpPost("checklist")]
    public async Task<ActionResult<List<ComplianceChecklistItem>>> GetChecklist([FromBody] ComplianceRequest request)
    {
        if (_complianceProvider == null)
        {
            return StatusCode(500, new { error = "Compliance provider not configured" });
        }

        var checklist = await _complianceProvider.GetPreFlightChecklistAsync(request);
        return Ok(checklist);
    }

    /// <summary>
    /// Validate operator readiness for SORA submission
    /// </summary>
    [HttpPost("readiness")]
    public async Task<ActionResult<OperatorReadiness>> ValidateReadiness([FromBody] ReadinessRequest request)
    {
        if (_complianceProvider == null)
        {
            return StatusCode(500, new { error = "Compliance provider not configured" });
        }

        var readiness = await _complianceProvider.ValidateOperatorReadinessAsync(
            request.OperatorId,
            request.MissionId,
            request.ComplianceRequest);
        return Ok(readiness);
    }

    #endregion

    #region Legacy Compliance Docs Endpoints

    [HttpGet("matrix/raw")]
    public IActionResult GetMatrixRaw()
    {
        try
        {
            var path = Path.Combine(WorkspaceRoot, "Docs", "Compliance", "OSO_to_Evidence_Matrix.md");
            if (!System.IO.File.Exists(path)) return NotFound(new { error = "Matrix not found" });
            var text = System.IO.File.ReadAllText(path);
            return Ok(new { path = ToRel(path), content = text });
        }
        catch
        {
            return Ok(new { path = (string?)null, content = "" });
        }
    }

    [HttpGet("binder/list")]
    public IActionResult ListBinder()
    {
        try
        {
            var dir = Path.Combine(WorkspaceRoot, "Docs", "Compliance", "Binder");
            var items = ListFiles(dir, "*.md");
            return Ok(items);
        }
        catch
        {
            return Ok(Array.Empty<object>());
        }
    }

    [HttpGet("reports/list")]
    public IActionResult ListReports()
    {
        try
        {
            var dir = Path.Combine(WorkspaceRoot, "Docs", "Compliance", "Reports");
            var items = ListFiles(dir, "*.md");
            return Ok(items);
        }
        catch
        {
            return Ok(Array.Empty<object>());
        }
    }

    #endregion

    #region Helpers

    private List<object> ListFiles(string dir, string pattern)
    {
        if (!Directory.Exists(dir)) return new List<object>();
        return Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly)
            .Select(p => new
            {
                path = ToRel(p),
                name = Path.GetFileName(p),
                size = new FileInfo(p).Length,
                lastModifiedUtc = System.IO.File.GetLastWriteTimeUtc(p)
            })
            .Cast<object>()
            .ToList();
    }

    private string ToRel(string abs) => Path.GetRelativePath(WorkspaceRoot, abs).Replace('\\', '/');

    #endregion
}

/// <summary>
/// Request για operator readiness validation
/// </summary>
public class ReadinessRequest
{
    public required string OperatorId { get; set; }
    public required string MissionId { get; set; }
    public required ComplianceRequest ComplianceRequest { get; set; }
}
