namespace AiCodeReviewAgent.Application.Agents.Tools;

public sealed class AgentToolRegistry : IAgentToolRegistry
{
    private readonly IReadOnlyDictionary<string, IAgentTool> _tools;

    public AgentToolRegistry(IEnumerable<IAgentTool> tools)
    {
        _tools = tools.ToDictionary(
            x => x.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IAgentTool GetRequiredTool(string toolName)
    {
        if (_tools.TryGetValue(toolName, out var tool))
        {
            return tool;
        }

        throw new InvalidOperationException(
            $"Agent tool '{toolName}' is not registered.");
    }
}