using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Tools;

public sealed class ReadProjectFileTool : IAgentTool
{
    public string Name => "read_project_file";

    public string Description => "Lee archivos .csproj relacionados al repositorio.";

    public async Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        var projectFiles = Directory.GetFiles(
            repositoryPath,
            "*.csproj",
            SearchOption.AllDirectories);

        if (projectFiles.Length == 0)
        {
            return new AgentToolResult
            {
                ToolName = Name,
                Success = false,
                Error = "No se encontraron archivos .csproj."
            };
        }

        var output = new List<string>();

        foreach (var projectFile in projectFiles.Take(5))
        {
            var relativePath = Path.GetRelativePath(repositoryPath, projectFile);
            var content = await File.ReadAllTextAsync(projectFile, cancellationToken);

            output.Add($"""
            File: {relativePath}

            {AgentTextLimiter.Limit(content, 3000)}
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