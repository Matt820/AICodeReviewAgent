using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiCodeReviewAgent.Application.Repositories;

namespace AiCodeReviewAgent.Infrastructure.GitHub;

public sealed class GitHubPullRequestCommentManager : IGitHubPullRequestCommentManager
{
    private const string Marker = "<!-- ai-code-review-agent -->";
    private readonly HttpClient _httpClient;

    public GitHubPullRequestCommentManager(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task UpsertCommentAsync(
        string repository,
        int pullRequestNumber,
        string githubToken,
        string body,
        CancellationToken cancellationToken)
    {
        ConfigureHeaders(githubToken);

        var commentsUrl = $"https://api.github.com/repos/{repository}/issues/{pullRequestNumber}/comments?per_page=100";

        using var listResponse = await _httpClient.GetAsync(commentsUrl, cancellationToken);
        var listContent = await listResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!listResponse.IsSuccessStatusCode)
            throw new InvalidOperationException(listContent);

        using var document = JsonDocument.Parse(listContent);

        long? existingCommentId = null;

        foreach (var comment in document.RootElement.EnumerateArray())
        {
            var commentBody = comment.GetProperty("body").GetString() ?? string.Empty;

            if (commentBody.Contains(Marker, StringComparison.OrdinalIgnoreCase))
            {
                existingCommentId = comment.GetProperty("id").GetInt64();
                break;
            }
        }

        var finalBody = $"{Marker}{Environment.NewLine}{body}";

        if (existingCommentId is null)
        {
            await CreateCommentAsync(repository, pullRequestNumber, finalBody, cancellationToken);
            return;
        }

        await UpdateCommentAsync(repository, existingCommentId.Value, finalBody, cancellationToken);
    }

    private void ConfigureHeaders(string githubToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AiCodeReviewAgent/1.0");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", githubToken);

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    private async Task CreateCommentAsync(
        string repository,
        int pullRequestNumber,
        string body,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.github.com/repos/{repository}/issues/{pullRequestNumber}/comments";

        var json = JsonSerializer.Serialize(new { body });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(responseContent);
    }

    private async Task UpdateCommentAsync(
        string repository,
        long commentId,
        string body,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.github.com/repos/{repository}/issues/comments/{commentId}";

        var json = JsonSerializer.Serialize(new { body });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PatchAsync(url, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(responseContent);
    }
}