using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Tools;

public sealed class ReadSolutionTool : IAgentTool
{
    public string Name => "read_solution";

    public string Description => "Lee archivos .sln del repositorio para entender la estructura de proyectos.";

    public async Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        var solutionFiles = Directory.GetFiles(
            repositoryPath,
            "*.sln",
            SearchOption.AllDirectories);

        if (solutionFiles.Length == 0)
        {
            return new AgentToolResult
            {
                ToolName = Name,
                Success = false,
                Error = "No se encontraron archivos .sln."
            };
        }

        var output = new List<string>();

        foreach (var solutionFile in solutionFiles.Take(3))
        {
            var relativePath = Path.GetRelativePath(repositoryPath, solutionFile);
            var content = await File.ReadAllTextAsync(solutionFile, cancellationToken);

            output.Add($"""
            File: {relativePath}

            {AgentTextLimiter.Limit(content, 4000)}
            """);
        }

        return new AgentToolResult
        {
            ToolName = Name,
            Success = true,
            Output = string.Join(Environment.NewLine + "---" + Environment.NewLine, output)
        };
    }
}