namespace AiCodeReviewAgent.Application.Agents.Prompts;

public sealed class CodeReviewPromptBuilder : ICodeReviewPromptBuilder
{
    public string Build(CodeReviewPromptContext context)
    {
        var agentContext = context.AgentContext;

        var toolContext = BuildToolContext(agentContext);
        var buildContext = BuildBuildContext(agentContext);
        var testContext = BuildTestContext(agentContext);
        var rulesContext = BuildRulesContext(agentContext);

        var ragContext = string.IsNullOrWhiteSpace(agentContext.RagContext)
            ? "No se obtuvo contexto RAG."
            : agentContext.RagContext;

        var specializedContext = BuildSpecializedReviewsContext(agentContext);        

        return $"""
        Estás actuando como un AI Code Review Agent con herramientas.

        Archivo modificado:
        {context.ChangedFilePath}

        Diff del Pull Request:
        ```diff
        {agentContext.PullRequestDiff}
        ```

        Resultado del build:
        {buildContext}

        Resultado de tests:
        {testContext}

        Contexto obtenido mediante tools del agente:
        {toolContext}

        Contexto RAG del repositorio:
        {ragContext}

        Reglas configuradas para este repositorio:
        {rulesContext}

        Reviews especializados:
        {specializedContext}

        Usa el contexto de las tools para detectar si el cambio afecta otras partes del sistema.

        Realiza un code review profesional en español.

        Evalúa:
        - Bugs potenciales
        - Seguridad
        - Clean Architecture
        - Mantenibilidad
        - Buenas prácticas .NET
        - Posibles tests faltantes

        Responde en Markdown con:
        - Resumen
        - Observaciones
        - Recomendaciones
        - Riesgo: Low, Medium o High
        """;
    }

    private static string BuildSpecializedReviewsContext(AgentContext context)
    {
        if (context.SpecializedReviews.Count == 0)
            return "No se ejecutaron agentes especializados.";

        return string.Join(
            Environment.NewLine + "---" + Environment.NewLine,
            context.SpecializedReviews.Select(x =>
                $"""
                Agent: {x.AgentName}

                {AgentTextLimiter.Limit(x.Markdown, 4000)}
                """));
    }

    private static string BuildToolContext(AgentContext context)
    {
        if (context.ToolResults.Count == 0)
            return "No se obtuvo contexto adicional mediante tools.";

        return string.Join(
            Environment.NewLine,
            context.ToolResults.Select(x =>
                $"""
                Tool: {x.ToolName}
                Success: {x.Success}
                Output:
                {AgentTextLimiter.Limit(x.Output)}
                Error:
                {AgentTextLimiter.Limit(x.Error)}
                """));
    }

    private static string BuildBuildContext(AgentContext context)
    {
        return context.BuildResult is null
            ? "Build no ejecutado."
            : $"""
            Build:
            Success: {context.BuildResult.Success}
            Output:
            {AgentTextLimiter.Limit(context.BuildResult.Output)}
            Error:
            {AgentTextLimiter.Limit(context.BuildResult.Error)}
            """;
    }

    private static string BuildTestContext(AgentContext context)
    {
        return context.TestResult is null
            ? "Tests no ejecutados."
            : $"""
            Tests:
            Success: {context.TestResult.Success}
            Output:
            {AgentTextLimiter.Limit(context.TestResult.Output)}
            Error:
            {AgentTextLimiter.Limit(context.TestResult.Error)}
            """;
    }

    private static string BuildRulesContext(AgentContext context)
    {
        return context.Rules.Count == 0
            ? "No se configuraron reglas personalizadas."
            : string.Join(Environment.NewLine, context.Rules.Select(rule => $"- {rule}"));
    }
}