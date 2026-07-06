using AiCodeReviewAgent.Application.Agents.Execution;
using AiCodeReviewAgent.Application.Agents.Planning;

namespace AiCodeReviewAgent.Application.Agents.Orchestration;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IAgentPlanner _planner;
    private readonly IAgentToolExecutor _toolExecutor;

    public AgentOrchestrator(
        IAgentPlanner planner,
        IAgentToolExecutor toolExecutor)
    {
        _planner = planner;
        _toolExecutor = toolExecutor;
    }

    public async Task<AgentExecutionResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var plan = await _planner.CreatePlanAsync(context, cancellationToken);

        var results = new List<AgentToolResult>();

        foreach (var step in plan.Steps)
        {
            var result = await _toolExecutor.ExecuteAsync(
                step,
                context,
                cancellationToken);

            results.Add(result);
            context.ToolResults.Add(result);

            if (!result.Success && step.Required)
            {
                break;
            }
        }

        return new AgentExecutionResult
        {
            Plan = plan,
            ToolResults = results
        };
    }
}