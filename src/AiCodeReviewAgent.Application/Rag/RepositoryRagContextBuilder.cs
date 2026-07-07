using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Rag;

public sealed class RepositoryRagContextBuilder
{
    private readonly IRepositoryRetriever _retriever;

    public RepositoryRagContextBuilder(IRepositoryRetriever retriever)
    {
        _retriever = retriever;
    }

    public async Task<string> BuildAsync(
        string repositoryPath,
        string query,
        CancellationToken cancellationToken)
    {
        var results = await _retriever.SearchAsync(
            repositoryPath,
            query,
            maxResults: 5,
            cancellationToken);

        if (results.Count == 0)
            return "No se encontró contexto RAG relevante.";

        return string.Join(
            Environment.NewLine + "---" + Environment.NewLine,
            results.Select(x =>
                $"""
                File: {x.FilePath}
                Score: {x.Score}

                {AgentTextLimiter.Limit(x.Content, 3000)}
                """));
    }
}