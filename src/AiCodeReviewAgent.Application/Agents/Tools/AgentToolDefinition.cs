namespace AiCodeReviewAgent.Application.Agents.Tools;

public sealed class AgentToolDefinition
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public Dictionary<string, string> InputSchema { get; init; } = [];
}