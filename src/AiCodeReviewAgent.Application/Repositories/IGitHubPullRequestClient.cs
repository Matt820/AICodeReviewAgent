namespace AiCodeReviewAgent.Application.Repositories;

public interface IGitHubPullRequestClient
{
    Task<IReadOnlyList<PullRequestChangedFile>> GetChangedFilesAsync(
        PullRequestAnalysisRequest request,
        CancellationToken cancellationToken);
}