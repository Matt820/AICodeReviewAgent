namespace AiCodeReviewAgent.Application.Agents.Specialized;

public sealed class SpecializedReviewAgentResult
{
    public required string AgentName { get; init; }
    public required string Markdown { get; init; }
}