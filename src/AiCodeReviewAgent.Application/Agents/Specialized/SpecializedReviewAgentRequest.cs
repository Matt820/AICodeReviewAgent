namespace AiCodeReviewAgent.Application.Agents.Specialized;

public sealed class SpecializedReviewAgentRequest
{
    public required string ChangedFilePath { get; init; }
    public required AgentContext Context { get; init; }
}