namespace AiCodeReviewAgent.Application.Reviews;

public interface IAiCodeReviewClient
{
    Task<string> AnalyzeCodeAsync(
        AnalyzeCodeRequest request,
        CancellationToken cancellationToken);
}