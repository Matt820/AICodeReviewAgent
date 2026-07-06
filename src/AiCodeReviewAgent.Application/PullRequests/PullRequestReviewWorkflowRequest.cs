namespace AiCodeReviewAgent.Application.PullRequests;

public sealed class PullRequestReviewWorkflowRequest
{
    public required string Repository { get; init; }

    public required string GitHubToken { get; init; }

    public required int PullRequestNumber { get; init; }

    public required string WorkspacePath { get; init; }
}