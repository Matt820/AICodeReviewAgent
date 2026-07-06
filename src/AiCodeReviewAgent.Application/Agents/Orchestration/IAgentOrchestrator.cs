namespace AiCodeReviewAgent.Application.Agents.Orchestration;

public interface IAgentOrchestrator
{
    Task<AgentExecutionResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken);
}