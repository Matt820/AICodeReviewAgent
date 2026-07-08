using AiCodeReviewAgent.Application.Agents.Specialized;

namespace AiCodeReviewAgent.Application.Agents.Pipeline.Stages;

public sealed class SpecializedReviewStage : IAgentPipelineStage
{
    private readonly SpecializedReviewOrchestrator _specializedReviewOrchestrator;

    public string Name => "specialized_reviews";

    public SpecializedReviewStage(
        SpecializedReviewOrchestrator specializedReviewOrchestrator)
    {
        _specializedReviewOrchestrator = specializedReviewOrchestrator;
    }

    public async Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        if (!context.AgentContext.Features.SpecializedAgents)
            return;

        var specializedReviews = await _specializedReviewOrchestrator.ReviewAsync(
            new SpecializedReviewAgentRequest
            {
                ChangedFilePath = context.ChangedFilePath,
                Context = context.AgentContext
            },
            cancellationToken);

        context.AgentContext.SpecializedReviews.AddRange(specializedReviews);
    }
}