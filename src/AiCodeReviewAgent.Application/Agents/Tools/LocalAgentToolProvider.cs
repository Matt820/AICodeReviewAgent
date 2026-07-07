namespace AiCodeReviewAgent.Application.Agents.Tools;

public sealed class LocalAgentToolProvider : IAgentToolProvider
{
    private readonly IReadOnlyDictionary<string, IAgentTool> _tools;

    public LocalAgentToolProvider(IEnumerable<IAgentTool> tools)
    {
        _tools = tools.ToDictionary(
            x => x.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public Task<IReadOnlyList<AgentToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AgentToolDefinition> definitions = _tools.Values
            .Select(tool => new AgentToolDefinition
            {
                Name = tool.Name,
                Description = tool.Description,
                InputSchema =
                {
                    ["repositoryPath"] = "string",
                    ["input"] = "string"
                }
            })
            .ToList();

        return Task.FromResult(definitions);
    }

    public async Task<AgentToolResult> ExecuteAsync(
        string toolName,
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
        {
            return new AgentToolResult
            {
                ToolName = toolName,
                Success = false,
                Error = $"Tool '{toolName}' no registrada."
            };
        }

        return await tool.ExecuteAsync(
            repositoryPath,
            input,
            cancellationToken);
    }
}