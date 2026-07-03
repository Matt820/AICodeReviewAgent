using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Tools;

public sealed class SearchTextTool : IAgentTool
{
    public string Name => "search_text";

    public string Description => "Busca texto dentro de archivos .cs del repositorio.";

    public Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        try
        {
            var matches = Directory
                .GetFiles(repositoryPath, "*.cs", SearchOption.AllDirectories)
                .Where(Repositories.RepositoryFileFilter.ShouldAnalyze)
                .SelectMany(file =>
                    File.ReadLines(file)
                        .Select((line, index) => new
                        {
                            File = Path.GetRelativePath(repositoryPath, file),
                            Line = index + 1,
                            Text = line
                        }))
                .Where(x => x.Text.Contains(input, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .Select(x => $"{x.File}:{x.Line}: {x.Text.Trim()}")
                .ToList();

            return Task.FromResult(new AgentToolResult
            {
                ToolName = Name,
                Success = true,
                Output = matches.Count == 0
                    ? "No se encontraron coincidencias."
                    : string.Join(Environment.NewLine, matches)
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentToolResult
            {
                ToolName = Name,
                Success = false,
                Error = ex.Message
            });
        }
    }
}