using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly AgentLLMService _llmService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentsController> _logger;
    private readonly IWebHostEnvironment _env;

    public AgentsController(
        AgentLLMService llmService,
        IConfiguration configuration,
        ILogger<AgentsController> logger,
        IWebHostEnvironment env)
    {
        _llmService = llmService;
        _configuration = configuration;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Ρωτά έναν AI agent με πλήρη LLM reasoning
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskAgent([FromBody] AskAgentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AgentName))
            return BadRequest(new { error = "AgentName is required" });

        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question is required" });

        // Validate agent name
        var validAgents = new[] { "SORA_Compliance_Agent", "Mission_Planning_Agent" };
        if (!validAgents.Contains(request.AgentName))
            return BadRequest(new { error = $"Invalid agent. Valid: {string.Join(", ", validAgents)}" });

        try
        {
            var response = await _llmService.AskAgentAsync(
                request.AgentName,
                request.Question,
                HttpContext.RequestAborted
            );

            if (!response.Success)
            {
                return StatusCode(500, new { error = response.Error });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asking agent {AgentName}", request.AgentName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Λίστα όλων των agents με status
    /// </summary>
    [HttpGet("list")]
    public IActionResult ListAgents()
    {
        var agents = new[]
        {
            new
            {
                Name = "SORA_Compliance_Agent",
                Expertise = _configuration.GetSection("Agents:SORA_Compliance_Agent:Expertise").Get<List<string>>(),
                MemoryPath = _configuration["Agents:SORA_Compliance_Agent:MemoryPath"]
            },
            new
            {
                Name = "Mission_Planning_Agent",
                Expertise = _configuration.GetSection("Agents:Mission_Planning_Agent:Expertise").Get<List<string>>(),
                MemoryPath = _configuration["Agents:Mission_Planning_Agent:MemoryPath"]
            }
        };

        return Ok(agents);
    }

    /// <summary>
    /// Training status (τελευταίο report)
    /// </summary>
    [HttpGet("training/status")]
    public IActionResult GetTrainingStatus()
    {
        try
        {
            // Compute repo root from API content root, then Tools/TrainingCenter/agent_memory
            var contentRoot = _env.ContentRootPath; // Backend/src/Skyworks.Api
            var workspaceRoot = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", ".."));
            var memoryPath = Path.Combine(workspaceRoot, "Tools", "TrainingCenter", "agent_memory");

            var reportFiles = Directory.Exists(memoryPath)
                ? Directory.GetFiles(memoryPath, "training_report_*.json")
                    .OrderByDescending(System.IO.File.GetLastWriteTime)
                    .ToList()
                : new List<string>();

            if (!reportFiles.Any())
                return NotFound(new { error = "No training reports found" });

            var latestReport = System.IO.File.ReadAllText(reportFiles[0]);
            return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(latestReport));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading training status");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class AskAgentRequest
{
    public string AgentName { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}
