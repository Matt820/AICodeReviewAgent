using AiCodeReviewAgent.Application.Agents.Orchestration;

namespace AiCodeReviewAgent.Application.Agents.Planning;

public interface IAgentPlanner
{
    Task<AgentPlan> CreatePlanAsync(
        AgentContext context,
        CancellationToken cancellationToken);
}