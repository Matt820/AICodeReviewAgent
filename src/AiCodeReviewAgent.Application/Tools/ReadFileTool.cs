using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Tools;

public sealed class ReadFileTool : IAgentTool
{
    public string Name => "read_file";

    public string Description => "Lee el contenido de un archivo dentro del repositorio.";

    public async Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        try
        {
            var fullPath = Path.Combine(repositoryPath, input);

            if (!File.Exists(fullPath))
            {
                return new AgentToolResult
                {
                    ToolName = Name,
                    Success = false,
                    Error = $"No existe el archivo: {input}"
                };
            }

            var content = await File.ReadAllTextAsync(fullPath, cancellationToken);

            return new AgentToolResult
            {
                ToolName = Name,
                Success = true,
                Output = content.Length > 6000
                    ? content[..6000]
                    : content
            };
        }
        catch (Exception ex)
        {
            return new AgentToolResult
            {
                ToolName = Name,
                Success = false,
                Error = ex.Message
            };
        }
    }
}