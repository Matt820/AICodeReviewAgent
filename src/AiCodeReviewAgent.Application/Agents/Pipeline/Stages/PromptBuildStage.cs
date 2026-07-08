using AiCodeReviewAgent.Application.Agents.Prompts;

namespace AiCodeReviewAgent.Application.Agents.Pipeline.Stages;

public sealed class PromptBuildStage : IAgentPipelineStage
{
    private readonly ICodeReviewPromptBuilder _promptBuilder;

    public string Name => "prompt_build";

    public PromptBuildStage(ICodeReviewPromptBuilder promptBuilder)
    {
        _promptBuilder = promptBuilder;
    }

    public Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        context.Prompt = _promptBuilder.Build(
            new CodeReviewPromptContext
            {
                ChangedFilePath = context.ChangedFilePath,
                AgentContext = context.AgentContext
            });

        return Task.CompletedTask;
    }
}