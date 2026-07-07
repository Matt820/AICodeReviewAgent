namespace AiCodeReviewAgent.Application.Rag;

public interface IRepositoryIndexer
{
    Task IndexAsync(
        string repositoryPath,
        CancellationToken cancellationToken);
}