namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public interface IAgentPipelineStage
{
    string Name { get; }

    Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken);
}