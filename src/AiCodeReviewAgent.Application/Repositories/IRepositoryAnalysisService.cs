namespace AiCodeReviewAgent.Application.Repositories;

public interface IRepositoryAnalysisService
{
    Task<RepositoryAnalysisResponse> AnalyzeLocalAsync(
        AnalyzeLocalRepositoryRequest request,
        CancellationToken cancellationToken);
}