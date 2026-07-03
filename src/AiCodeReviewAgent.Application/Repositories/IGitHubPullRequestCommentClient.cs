namespace AiCodeReviewAgent.Application.Repositories;

public interface IGitHubPullRequestCommentClient
{
    Task CreateCommentAsync(
        string repository,
        int pullRequestNumber,
        string githubToken,
        string body,
        CancellationToken cancellationToken);
}