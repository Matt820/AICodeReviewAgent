namespace AiCodeReviewAgent.Application.Agents.Orchestration;

public sealed class AgentStep
{
    public required string ToolName { get; init; }
    public required string Input { get; init; }
    public bool Required { get; init; } = true;
}