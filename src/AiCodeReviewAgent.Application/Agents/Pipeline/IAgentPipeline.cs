namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public interface IAgentPipeline
{
    Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken);
}