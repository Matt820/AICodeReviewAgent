namespace AiCodeReviewAgent.Application.Rag;

public interface IRepositoryRetriever
{
    Task<IReadOnlyList<RepositorySearchResult>> SearchAsync(
        string repositoryPath,
        string query,
        int maxResults,
        CancellationToken cancellationToken);
}