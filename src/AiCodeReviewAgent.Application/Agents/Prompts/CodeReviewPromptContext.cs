namespace AiCodeReviewAgent.Application.Agents.Prompts;

public sealed class CodeReviewPromptContext
{
    public required string ChangedFilePath { get; init; }

    public required AgentContext AgentContext { get; init; }
}