using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents.Pipeline.Stages;

public sealed class LlmReviewStage : IAgentPipelineStage
{
    private readonly IAiCodeReviewClient _aiClient;

    public string Name => "llm_review";

    public LlmReviewStage(IAiCodeReviewClient aiClient)
    {
        _aiClient = aiClient;
    }

    public async Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        context.ReviewMarkdown = await _aiClient.AnalyzeCodeAsync(
            new AnalyzeCodeRequest
            {
                FileName = context.ChangedFilePath,
                Language = "diff",
                Code = context.Prompt
            },
            cancellationToken);
    }
}