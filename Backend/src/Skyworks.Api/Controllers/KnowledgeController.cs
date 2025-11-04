using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeBaseService _kb;

    public KnowledgeController(IKnowledgeBaseService kb)
    {
        _kb = kb;
    }

    [HttpGet("docs")]
    public async Task<IActionResult> ListDocs([FromQuery] string? filter)
    {
        try
        {
            var docs = await _kb.ListDocumentsAsync(filter, HttpContext.RequestAborted);
            return Ok(docs);
        }
        catch
        {
            // Phase1: be defensive — return empty list on errors
            return Ok(Array.Empty<object>());
        }
    }

    public record SearchRequest(string Query, int? Top);

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Query)) return BadRequest(new { error = "query is required" });
        try
        {
            var results = await _kb.SearchAsync(req.Query, req.Top ?? 20, HttpContext.RequestAborted);
            return Ok(results);
        }
        catch
        {
            return Ok(Array.Empty<object>());
        }
    }

    [HttpPost("train")]
    public async Task<IActionResult> Train()
    {
        try
        {
            var count = await _kb.TrainAsync(HttpContext.RequestAborted);
            return Ok(new { indexed = count, status = "OK" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
