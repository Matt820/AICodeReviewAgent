namespace AiCodeReviewAgent.Application.Repositories;

public interface IGitHubPullRequestCommentManager
{
    Task UpsertCommentAsync(
        string repository,
        int pullRequestNumber,
        string githubToken,
        string body,
        CancellationToken cancellationToken);
}