using System.Net.Http.Headers;
using System.Text.Json;
using AiCodeReviewAgent.Application.Repositories;

namespace AiCodeReviewAgent.Infrastructure.GitHub;

public sealed class GitHubPullRequestClient : IGitHubPullRequestClient
{
    private readonly HttpClient _httpClient;

    public GitHubPullRequestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PullRequestChangedFile>> GetChangedFilesAsync(
        PullRequestAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Repository))
            throw new ArgumentException("Repository es requerido. Ejemplo: owner/repo");

        if (request.PullRequestNumber <= 0)
            throw new ArgumentException("PullRequestNumber es requerido.");

        if (string.IsNullOrWhiteSpace(request.GitHubToken))
            throw new ArgumentException("GitHubToken es requerido.");

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AiCodeReviewAgent/1.0");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", request.GitHubToken);

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var url = $"https://api.github.com/repos/{request.Repository}/pulls/{request.PullRequestNumber}/files?per_page=100";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(content);

        using var document = JsonDocument.Parse(content);

        var files = new List<PullRequestChangedFile>();

        foreach (var item in document.RootElement.EnumerateArray())
        {
            var fileName = item.GetProperty("filename").GetString() ?? string.Empty;
            var status = item.GetProperty("status").GetString() ?? string.Empty;

            var patch = item.TryGetProperty("patch", out var patchProperty)
                ? patchProperty.GetString() ?? string.Empty
                : string.Empty;

            if (!fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                continue;

            files.Add(new PullRequestChangedFile
            {
                FileName = fileName,
                Status = status,
                Patch = patch
            });
        }

        return files.Take(request.MaxFiles).ToList();
    }
}