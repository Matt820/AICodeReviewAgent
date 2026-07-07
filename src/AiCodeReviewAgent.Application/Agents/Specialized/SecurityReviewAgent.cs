using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents.Specialized;

public sealed class SecurityReviewAgent : ISpecializedReviewAgent
{
    private readonly IAiCodeReviewClient _aiClient;

    public string Name => "security";

    public SecurityReviewAgent(IAiCodeReviewClient aiClient)
    {
        _aiClient = aiClient;
    }

    public async Task<SpecializedReviewAgentResult> ReviewAsync(
        SpecializedReviewAgentRequest request,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
        Actúa como un agente especializado en seguridad de código.

        Archivo:
        {request.ChangedFilePath}

        Diff:
        ```diff
        {AgentTextLimiter.Limit(request.Context.PullRequestDiff, 6000)}
        ```

        Contexto RAG:
        {AgentTextLimiter.Limit(request.Context.RagContext, 6000)}

        Busca únicamente riesgos de seguridad:
        - Validación de entrada
        - Exposición de secretos
        - Inyección
        - Uso inseguro de dependencias
        - Manejo inseguro de errores
        - Autorización/autenticación

        Responde en Markdown, breve y accionable.
        """;

        var markdown = await _aiClient.AnalyzeCodeAsync(
            new AnalyzeCodeRequest
            {
                FileName = $"{request.ChangedFilePath}-security",
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