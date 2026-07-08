namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public interface IAgentPipeline
{
    Task<AgentPipelineResult> ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken);
}