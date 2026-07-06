namespace AiCodeReviewAgent.Application.PullRequests;

public interface IPullRequestReviewWorkflow
{
    Task ExecuteAsync(
        PullRequestReviewWorkflowRequest request,
        CancellationToken cancellationToken);
}