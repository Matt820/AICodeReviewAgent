using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiCodeReviewAgent.Application.Repositories;

namespace AiCodeReviewAgent.Infrastructure.GitHub;

public sealed class GitHubPullRequestCommentClient : IGitHubPullRequestCommentClient
{
    private readonly HttpClient _httpClient;

    public GitHubPullRequestCommentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateCommentAsync(
        string repository,
        int pullRequestNumber,
        string githubToken,
        string body,
        CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AiCodeReviewAgent/1.0");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", githubToken);

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var url = $"https://api.github.com/repos/{repository}/issues/{pullRequestNumber}/comments";

        var json = JsonSerializer.Serialize(new
        {
            body
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(url, content, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(responseContent);
    }
}