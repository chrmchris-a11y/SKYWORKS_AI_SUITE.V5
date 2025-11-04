using System.Text.Json.Serialization;

namespace Skyworks.Core.Services;

public interface IKnowledgeBaseService
{
    Task<IReadOnlyList<KbDocument>> ListDocumentsAsync(string? filter = null, CancellationToken ct = default);
    Task<IReadOnlyList<KbSearchResult>> SearchAsync(string query, int top = 20, CancellationToken ct = default);
    /// <summary>
    /// Builds or refreshes the in-memory index of EASA/JARUS knowledge documents.
    /// Returns the number of documents indexed.
    /// </summary>
    Task<int> TrainAsync(CancellationToken ct = default);
}

public class KbDocument
{
    public required string Path { get; set; }
    public required string Title { get; set; }
    public required string Category { get; set; }
    public long SizeBytes { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public string? Preview { get; set; }
}

public class KbSearchResult
{
    public required string Path { get; set; }
    public required string Title { get; set; }
    public string? Snippet { get; set; }
    public double Score { get; set; }
}
