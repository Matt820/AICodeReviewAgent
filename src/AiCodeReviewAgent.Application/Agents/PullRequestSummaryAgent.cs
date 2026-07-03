using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents;

public interface IPullRequestSummaryAgent
{
    Task<string> GenerateSummaryAsync(
        PullRequestReviewSummaryRequest request,
        CancellationToken cancellationToken);
}

public sealed class PullRequestSummaryAgent : IPullRequestSummaryAgent
{
    private readonly IAiCodeReviewClient _aiClient;

    public PullRequestSummaryAgent(IAiCodeReviewClient aiClient)
    {
        _aiClient = aiClient;
    }

    public async Task<string> GenerateSummaryAsync(
        PullRequestReviewSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
        Actúa como un Tech Lead revisando un Pull Request.

        PR: #{request.PullRequestNumber}
        Archivos revisados: {request.FilesReviewed}
        Score: {request.ReviewScore}/100
        Build pasó: {request.BuildPassed}
        Tests pasaron: {request.TestsPassed}

        Reviews por archivo:
        {AgentTextLimiter.Limit(request.ReviewsMarkdown, 8000)}

        Genera un resumen ejecutivo en español con:
        - Riesgo general del PR
        - Principales hallazgos
        - Recomendación final: aprobar, aprobar con cambios menores, o solicitar cambios

        Responde en Markdown.
        """;

        return await _aiClient.AnalyzeCodeAsync(
            new AnalyzeCodeRequest
            {
                FileName = $"PR-{request.PullRequestNumber}-summary",
                Language = "markdown",
                Code = prompt
            },
            cancellationToken);
    }
}