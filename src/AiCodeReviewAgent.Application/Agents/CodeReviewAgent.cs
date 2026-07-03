using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents;

public interface ICodeReviewAgent
{
    Task<string> ReviewPullRequestAsync(
        string repositoryPath,
        string changedFilePath,
        string patch,
        CancellationToken cancellationToken);
}

public sealed class CodeReviewAgent : ICodeReviewAgent
{
    private readonly IEnumerable<IAgentTool> _tools;
    private readonly IAiCodeReviewClient _aiClient;

    public CodeReviewAgent(
        IEnumerable<IAgentTool> tools,
        IAiCodeReviewClient aiClient)
    {
        _tools = tools;
        _aiClient = aiClient;
    }

    public async Task<string> ReviewPullRequestAsync(
        string repositoryPath,
        string changedFilePath,
        string patch,
        CancellationToken cancellationToken)
    {
        var context = new AgentContext
        {
            RepositoryPath = repositoryPath,
            PullRequestDiff = patch
        };

        var readFileTool = _tools.FirstOrDefault(x => x.Name == "read_file");

        if (readFileTool is not null)
        {
            var result = await readFileTool.ExecuteAsync(
                repositoryPath,
                changedFilePath,
                cancellationToken);

            context.ToolResults.Add(result);
        }

        var searchTextTool = _tools.FirstOrDefault(x => x.Name == "search_text");

        if (searchTextTool is not null)
        {
            var className = Path.GetFileNameWithoutExtension(changedFilePath);

            var result = await searchTextTool.ExecuteAsync(
                repositoryPath,
                className,
                cancellationToken);

            context.ToolResults.Add(result);
        }

        var runBuildTool = _tools.FirstOrDefault(x => x.Name == "run_build");

        if (runBuildTool is not null)
        {
            var result = await runBuildTool.ExecuteAsync(
                repositoryPath,
                string.Empty,
                cancellationToken);

            context.ToolResults.Add(result);
        }

        var prompt = BuildPrompt(changedFilePath, context);

        return await _aiClient.AnalyzeCodeAsync(
            new AnalyzeCodeRequest
            {
                FileName = changedFilePath,
                Language = "diff",
                Code = prompt
            },
            cancellationToken);
    }

    private static string BuildPrompt(string changedFilePath, AgentContext context)
    {
        var toolContext = string.Join(
            Environment.NewLine,
            context.ToolResults.Select(x =>
                $"""
                Tool: {x.ToolName}
                Success: {x.Success}
                Output:
                {x.Output}
                Error:
                {x.Error}
                """));

        return $"""
        Estás actuando como un AI Code Review Agent con herramientas.

        Archivo modificado:
        {changedFilePath}

        Diff del Pull Request:
        ```diff
        {context.PullRequestDiff}
        ```

        Contexto obtenido mediante tools del agente:
        {toolContext}

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
}