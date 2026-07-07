using AiCodeReviewAgent.Application.Rag;

namespace AiCodeReviewAgent.Infrastructure.Rag;

public sealed class LocalRepositoryRetriever : IRepositoryRetriever
{
    private static readonly string[] AllowedExtensions =
    [
        ".cs",
        ".csproj",
        ".sln",
        ".json",
        ".yml",
        ".yaml",
        ".md"
    ];

    public async Task<IReadOnlyList<RepositorySearchResult>> SearchAsync(
        string repositoryPath,
        string query,
        int maxResults,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var terms = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (terms.Count == 0)
            return [];

        var files = Directory
            .GetFiles(repositoryPath, "*.*", SearchOption.AllDirectories)
            .Where(file => AllowedExtensions.Contains(
                Path.GetExtension(file),
                StringComparer.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}"));

        var results = new List<RepositorySearchResult>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = await File.ReadAllTextAsync(file, cancellationToken);

            var score = terms.Count(term =>
                content.Contains(term, StringComparison.OrdinalIgnoreCase));

            if (score == 0)
                continue;

            results.Add(new RepositorySearchResult
            {
                FilePath = Path.GetRelativePath(repositoryPath, file),
                Content = content,
                Score = score
            });
        }

        return results
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .ToList();
    }
}