namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public sealed class AgentPipeline : IAgentPipeline
{
    private readonly IEnumerable<IAgentPipelineStage> _stages;

    public AgentPipeline(IEnumerable<IAgentPipelineStage> stages)
    {
        _stages = stages;
    }

    public async Task<AgentPipelineResult> ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        foreach (var stage in _stages)
        {
            await stage.ExecuteAsync(context, cancellationToken);
        }

        return new AgentPipelineResult
        {
            ReviewMarkdown = context.ReviewMarkdown
        };
    }
}