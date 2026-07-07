using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents.Specialized;

public sealed class TestingReviewAgent : ISpecializedReviewAgent
{
    private readonly IAiCodeReviewClient _aiClient;

    public string Name => "testing";

    public TestingReviewAgent(IAiCodeReviewClient aiClient)
    {
        _aiClient = aiClient;
    }

    public async Task<SpecializedReviewAgentResult> ReviewAsync(
        SpecializedReviewAgentRequest request,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
        Actúa como un agente especializado en pruebas automatizadas.

        Archivo:
        {request.ChangedFilePath}

        Diff:
        ```diff
        {AgentTextLimiter.Limit(request.Context.PullRequestDiff, 6000)}
        ```

        Resultado de tests:
        Success: {request.Context.TestResult?.Success}
        Output:
        {AgentTextLimiter.Limit(request.Context.TestResult?.Output)}

        Contexto RAG:
        {AgentTextLimiter.Limit(request.Context.RagContext, 6000)}

        Evalúa:
        - Tests faltantes
        - Casos borde
        - Riesgos de regresión
        - Cobertura insuficiente
        - Si los tests actuales validan realmente el cambio

        Responde en Markdown, breve y accionable.
        """;

        var markdown = await _aiClient.AnalyzeCodeAsync(
            new AnalyzeCodeRequest
            {
                FileName = $"{request.ChangedFilePath}-testing",
                Language = "diff",
                Code = prompt
            },
            cancellationToken);

        return new SpecializedReviewAgentResult
        {
            AgentName = Name,
            Markdown = markdown
        };
    }
}