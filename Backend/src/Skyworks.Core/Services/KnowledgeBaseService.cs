using System.Text;

namespace Skyworks.Core.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly string _workspaceRoot;
    private readonly string[] _kbRoots;
    private volatile List<KbDocument> _index = new();
    private readonly object _lock = new();

    public KnowledgeBaseService(string workspaceRoot)
    {
        _workspaceRoot = workspaceRoot;
        _kbRoots = new[]
        {
            Path.Combine(_workspaceRoot, "KnowledgeBase"),
            Path.Combine(_workspaceRoot, "Docs", "Knowledge"),
            Path.Combine(_workspaceRoot, "Docs", "Compliance"),
            Path.Combine(_workspaceRoot, "Docs", "API"),
            Path.Combine(_workspaceRoot, "ContextPacks")
        };
    }

    public async Task<IReadOnlyList<KbDocument>> ListDocumentsAsync(string? filter = null, CancellationToken ct = default)
    {
        if (_index.Count == 0)
        {
            await TrainAsync(ct);
        }

        IEnumerable<KbDocument> docs = _index;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            docs = docs.Where(d => d.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
                                 || (d.Preview?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        return docs.OrderBy(d => d.Category).ThenBy(d => d.Title).ToList();
    }

    public async Task<IReadOnlyList<KbSearchResult>> SearchAsync(string query, int top = 20, CancellationToken ct = default)
    {
        query = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<KbSearchResult>();

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(t => t.ToLowerInvariant())
                         .ToArray();

        var results = new List<KbSearchResult>();
        if (_index.Count == 0)
        {
            await TrainAsync(ct);
        }
        var docs = _index;
        foreach (var d in docs)
        {
            var score = 0.0;
            var hayTitle = d.Title.ToLowerInvariant();
            var hayPrev = (d.Preview ?? string.Empty).ToLowerInvariant();
            foreach (var t in terms)
            {
                if (hayTitle.Contains(t)) score += 3;
                if (hayPrev.Contains(t)) score += 1;
            }
            if (score <= 0) continue;
            results.Add(new KbSearchResult
            {
                Path = d.Path,
                Title = d.Title,
                Snippet = MakeSnippetSafe(d.Preview ?? string.Empty, terms.FirstOrDefault() ?? string.Empty),
                Score = score
            });
        }

        return results.OrderByDescending(r => r.Score).Take(top).ToList();
    }

    public async Task<int> TrainAsync(CancellationToken ct = default)
    {
        var list = new List<KbDocument>();
        foreach (var root in _kbRoots)
        {
            if (!Directory.Exists(root)) continue;
            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                                          .Where(f => HasSupportedExtension(f)))
            {
                try
                {
                    var rel = Path.GetRelativePath(_workspaceRoot, file);
                    var title = Path.GetFileNameWithoutExtension(file);
                    var category = GetCategory(file);
                    var info = new FileInfo(file);
                    string? preview = await SafeReadPreviewAsync(file, 500, ct);

                    list.Add(new KbDocument
                    {
                        Path = rel.Replace('\\', '/'),
                        Title = title,
                        Category = category,
                        SizeBytes = info.Length,
                        LastModifiedUtc = info.LastWriteTimeUtc,
                        Preview = preview
                    });
                }
                catch
                {
                    // ignore problematic file entries
                }
            }
        }
        lock (_lock)
        {
            _index = list.OrderBy(d => d.Category).ThenBy(d => d.Title).ToList();
        }
        return _index.Count;
    }

    private static bool HasSupportedExtension(string path)
        => new[] { ".md", ".txt" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    private static string GetCategory(string path)
    {
        // Category = immediate folder under KnowledgeBase/ or Docs/Knowledge/
        var dir = Path.GetDirectoryName(path) ?? string.Empty;
        var parts = dir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (parts.Length == 0) return "General";
        return parts.Last();
    }

    private static async Task<string?> SafeReadPreviewAsync(string file, int maxChars, CancellationToken ct)
    {
        try
        {
            using var fs = File.OpenRead(file);
            using var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            char[] buf = new char[Math.Min(2048, maxChars)];
            int read = await sr.ReadAsync(buf.AsMemory(0, buf.Length), ct);
            var text = new string(buf, 0, read);
            if (text.Length > maxChars) text = text.Substring(0, maxChars);
            return text;
        }
        catch
        {
            return null;
        }
    }

    private static string? MakeSnippetSafe(string text, string term)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term)) return text?.Length > 140 ? text[..140] + "…" : text;
        var idx = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return text.Length > 140 ? text[..140] + "…" : text;
        var start = Math.Max(0, idx - 50);
        var len = Math.Min(140, text.Length - start);
        var snippet = text.Substring(start, len);
        return (start > 0 ? "…" : "") + snippet + (start + len < text.Length ? "…" : "");
    }
}
