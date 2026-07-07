namespace AiCodeReviewAgent.Application.Rag;

public sealed class RepositoryDocument
{
    public required string Id { get; init; }
    public required string FilePath { get; init; }
    public required string Content { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];
}