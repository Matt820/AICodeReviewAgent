using AiCodeReviewAgent.Application.Agents.Prompts;

namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public sealed class AgentPipelineContext
{
    public required string RepositoryPath { get; init; }
    public required string ChangedFilePath { get; init; }
    public required string Patch { get; init; }
    public required AgentContext AgentContext { get; init; }

    public string Prompt { get; set; } = string.Empty;
    public string ReviewMarkdown { get; set; } = string.Empty;
}