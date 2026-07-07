namespace AiCodeReviewAgent.Application.Rag;

public sealed class RepositorySearchResult
{
    public required string FilePath { get; init; }
    public required string Content { get; init; }
    public double Score { get; init; }
}