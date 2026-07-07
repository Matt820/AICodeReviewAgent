using System.Text.RegularExpressions;
using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Tools;

public sealed class FindClassTool : IAgentTool
{
    public string Name => "find_class";

    public string Description => "Busca definiciones de clases C# dentro del repositorio.";

    public async Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new AgentToolResult
            {
                ToolName = Name,
                Success = false,
                Error = "El nombre de la clase es requerido."
            };
        }

        var pattern = $@"\bclass\s+{Regex.Escape(input)}\b";

        var matches = new List<string>();

        foreach (var file in Directory.GetFiles(repositoryPath, "*.cs", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = await File.ReadAllTextAsync(file, cancellationToken);

            if (!Regex.IsMatch(content, pattern))
                continue;

            var relativePath = Path.GetRelativePath(repositoryPath, file);

            matches.Add($"""
            Class: {input}
            File: {relativePath}

            {AgentTextLimiter.Limit(content, 4000)}
            """);
        }

        return new AgentToolResult
        {
            ToolName = Name,
            Success = matches.Count > 0,
            Output = string.Join(Environment.NewLine + "---" + Environment.NewLine, matches.Take(5)),
            Error = matches.Count == 0 ? $"No se encontró la clase: {input}" : null
        };
    }
}