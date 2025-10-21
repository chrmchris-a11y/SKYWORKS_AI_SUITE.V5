using System.Text.Json;
using System.Diagnostics;

namespace Skyworks.Core.Services;

/// <summary>
/// LLM service για AI Agent reasoning με Azure OpenAI
/// Παρέχει RAG (Retrieval-Augmented Generation) με full context από agent memory
/// </summary>
public class AgentLLMService
{
    private readonly string _workspaceRoot;
    private readonly string _pythonExe;

    public AgentLLMService(
        string? workspaceRoot = null,
        string? pythonExecutable = null)
    {
        _workspaceRoot = workspaceRoot ?? Directory.GetCurrentDirectory();
        _pythonExe = string.IsNullOrWhiteSpace(pythonExecutable) ? "python" : pythonExecutable;
    }

    /// <summary>
    /// Ρωτά έναν agent με πλήρη reasoning και citations
    /// </summary>
    public async Task<AgentResponse> AskAgentAsync(
        string agentName,
        string question,
        CancellationToken cancellationToken = default)
    {
        // Load agent memory
        var memory = await LoadAgentMemoryAsync(agentName, cancellationToken);
        if (memory == null)
        {
            return new AgentResponse
            {
                Success = false,
                Error = $"Agent memory not found for {agentName}"
            };
        }

        // Retrieve relevant context (RAG)
        var relevantSources = RetrieveRelevantContext(question, memory);

        // Build system prompt
        var systemPrompt = BuildSystemPrompt(agentName, memory, relevantSources);

        // Build user prompt
        var userPrompt = BuildUserPrompt(question, relevantSources);

        // Call Python bridge (agent_llm.py) to perform LLM/RAG (supports mock mode without Azure)
        try
        {
            var scriptPath = Path.Combine(_workspaceRoot, "Tools", "TrainingCenter", "agent_llm.py");
            if (!File.Exists(scriptPath))
            {
                return new AgentResponse { Success = false, Error = $"Python LLM script not found at {scriptPath}" };
            }

            var psi = new ProcessStartInfo
            {
                FileName = _pythonExe,
                Arguments = $"\"{scriptPath}\" {EscapeArg(agentName)} {EscapeArg(question)}",
                WorkingDirectory = _workspaceRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                return new AgentResponse { Success = false, Error = "Failed to start Python process" };
            }

            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync(cancellationToken);

            if (proc.ExitCode != 0)
            {
                return new AgentResponse { Success = false, Error = $"Python error: {stderr}" };
            }

            var doc = JsonDocument.Parse(stdout);
            var root = doc.RootElement;
            if (!root.TryGetProperty("success", out var successProp) || !successProp.GetBoolean())
            {
                var err = root.TryGetProperty("error", out var errProp) ? errProp.GetString() : "Unknown error";
                return new AgentResponse { Success = false, Error = err };
            }

            var response = new AgentResponse
            {
                Success = true,
                AgentName = agentName,
                Question = question,
                Answer = root.GetProperty("answer").GetString(),
                Sources = root.TryGetProperty("sources", out var sourcesEl) && sourcesEl.ValueKind == JsonValueKind.Array
                    ? sourcesEl.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
                    : new List<string>(),
                TokensUsed = root.TryGetProperty("tokens_used", out var tokensEl) ? tokensEl.GetInt32() : 0,
                Model = root.TryGetProperty("model", out var modelEl) ? modelEl.GetString() : null
            };

            return response;
        }
        catch (Exception ex)
        {
            return new AgentResponse { Success = false, Error = $"LLM bridge failed: {ex.Message}" };
        }
    }

    private async Task<AgentMemory?> LoadAgentMemoryAsync(string agentName, CancellationToken cancellationToken)
    {
        var memoryPath = Path.Combine(
            _workspaceRoot,
            "Tools",
            "TrainingCenter",
            "agent_memory",
            $"{agentName}_memory.json"
        );

        if (!File.Exists(memoryPath))
            return null;

        var json = await File.ReadAllTextAsync(memoryPath, cancellationToken);
        return JsonSerializer.Deserialize<AgentMemory>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private List<MemoryEntry> RetrieveRelevantContext(string question, AgentMemory memory)
    {
        // Extract keywords από την ερώτηση
        var keywords = question
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .ToHashSet();

        // Score memory entries βάσει keyword overlap
        var scoredEntries = memory.Memory
            .Select(entry => new
            {
                Entry = entry,
                Score = CalculateRelevanceScore(entry, keywords)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(10) // Top 10 πιο σχετικές πηγές
            .Select(x => x.Entry)
            .ToList();

        return scoredEntries;
    }

    private int CalculateRelevanceScore(MemoryEntry entry, HashSet<string> keywords)
    {
        var score = 0;

        // Score από source name
        var sourceLower = entry.Source.ToLowerInvariant();
        score += keywords.Count(kw => sourceLower.Contains(kw)) * 3;

        // Score από key terms
        if (entry.KeyTerms != null)
        {
            var termsLower = entry.KeyTerms.Select(t => t.ToLowerInvariant()).ToHashSet();
            score += keywords.Count(kw => termsLower.Any(t => t.Contains(kw) || kw.Contains(t))) * 5;
        }

        return score;
    }

    private string BuildSystemPrompt(string agentName, AgentMemory memory, List<MemoryEntry> relevantSources)
    {
        var expertiseList = string.Join("\n- ", memory.Expertise ?? new List<string>());
        var sourcesList = string.Join("\n- ", relevantSources.Select(s => s.Source));

        string basePrompt = agentName == "SORA_Compliance_Agent"
            ? @"You are a world-class expert in SORA (Specific Operations Risk Assessment) methodology, EASA UAS regulations, and JARUS SORA 2.5.

Your expertise covers:
- {EXPERTISE}

Your role:
1. **Provide comprehensive, well-reasoned answers** based on regulatory documents
2. **Cite specific sources** (document names, sections, annexes) — be precise
3. **Explain methodology step-by-step** when calculating risk levels or demonstrating compliance
4. **Offer practical advice** for operators seeking authorization
5. **Highlight edge cases, exceptions, and regulatory nuances** that experienced compliance officers know
6. **Use examples and scenarios** when helpful for understanding
7. **Structure your response clearly** with headings, bullet points, and numbered steps

Tone: Professional, authoritative, but approachable. Think like a senior SORA consultant with 10+ years experience.

Critical: Never give short, mechanical, or single-sentence answers. Always provide context, reasoning, and actionable guidance.

Response structure template:
## Direct Answer
[Clear, concise answer to the question]

## Regulatory Basis
[Cite SORA 2.0 AMC, JARUS 2.5, PDRA, or relevant annexes with specific sections]

## Methodology / Step-by-Step
[If applicable: walk through calculation, assessment, or compliance steps]

## Practical Recommendations
[Actionable advice for operators]

## Related Considerations
[Edge cases, alternatives, or related requirements]

## Sources
[List specific documents/sections cited]"
            : @"You are an expert UAS mission planner and flight operations specialist with complete mastery of STS-01/02 and operational procedures.

Your expertise covers:
- {EXPERTISE}

Your role:
1. **Guide users through mission planning** with detailed, practical steps
2. **Cite STS requirements** and operational procedures precisely
3. **Explain flight authorization workflows** step-by-step
4. **Provide checklists, templates, and operational tools** in your responses
5. **Address safety, coordination, and contingency considerations** proactively
6. **Use real-world operational scenarios** to illustrate procedures
7. **Structure responses for immediate operational use**

Tone: Practical, detail-oriented, safety-focused. Think like a chief pilot or operations manager.

Critical: Never give vague or incomplete guidance. Provide full operational procedures, checklists, and decision trees.

Response structure template:
## Executive Summary
[What the user needs to do]

## Regulatory Requirements
[STS-01/02 citations, operational rules]

## Step-by-Step Procedures
[Detailed operational steps with numbering]

## Required Documentation
[Checklists, forms, templates needed]

## Safety & Contingencies
[Risk mitigations, emergency procedures]

## Sources
[STS documents, operation manual sections cited]";

        basePrompt = basePrompt.Replace("{EXPERTISE}", expertiseList);

        var sourcesSection = relevantSources.Any()
            ? $"\n\nYou have access to these knowledge sources for this question:\n{sourcesList}\n\nUse these to support your answer with specific citations."
            : "\n\nNote: No specific sources were retrieved for this question. Provide answer based on general SORA/EASA knowledge from your training.";

        return basePrompt + sourcesSection;
    }

    private string BuildUserPrompt(string question, List<MemoryEntry> relevantSources)
    {
        var prompt = $"User Question: {question}\n\n";

        if (relevantSources.Any())
        {
            prompt += "Relevant context from training data:\n\n";
            foreach (var source in relevantSources.Take(5))
            {
                prompt += $"Source: {source.Source}\n";
                if (source.KeyTerms?.Any() == true)
                {
                    prompt += $"Key Terms: {string.Join(", ", source.KeyTerms.Take(8))}\n";
                }
                prompt += $"Content Length: {source.ContentLength} characters\n\n";
            }
        }

        prompt += "Please provide a comprehensive, expert-level response following the structure template in your system prompt.";
        return prompt;
    }

    private static string EscapeArg(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return "\"\"";
        // Wrap in quotes and escape internal quotes for safe CLI passing
        return "\"" + arg.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Models
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class AgentResponse
{
    public bool Success { get; set; }
    public string? AgentName { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public List<string> Sources { get; set; } = new();
    public int TokensUsed { get; set; }
    public string? Model { get; set; }
    public string? Error { get; set; }
}

public class AgentMemory
{
    public string Agent { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public int TotalMemoryEntries { get; set; }
    public List<string> Expertise { get; set; } = new();
    public List<MemoryEntry> Memory { get; set; } = new();
}

public class MemoryEntry
{
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int ContentLength { get; set; }
    public List<string>? KeyTerms { get; set; }
}
