using AiCodeReviewAgent.Application.Agents.Orchestration;

namespace AiCodeReviewAgent.Application.Agents.Execution;

public interface IAgentToolExecutor
{
    Task<AgentToolResult> ExecuteAsync(
        AgentStep step,
        AgentContext context,
        CancellationToken cancellationToken);
}