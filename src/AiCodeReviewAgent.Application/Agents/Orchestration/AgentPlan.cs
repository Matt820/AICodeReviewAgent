namespace AiCodeReviewAgent.Application.Agents.Orchestration;

public sealed class AgentPlan
{
    public IReadOnlyList<AgentStep> Steps { get; init; } = [];
}