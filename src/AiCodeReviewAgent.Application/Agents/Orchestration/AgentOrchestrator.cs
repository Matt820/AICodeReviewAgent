using AiCodeReviewAgent.Application.Agents.Planning;
using AiCodeReviewAgent.Application.Agents.Tools;

namespace AiCodeReviewAgent.Application.Agents.Orchestration;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IAgentPlanner _planner;
    private readonly IAgentToolRegistry _toolRegistry;

    public AgentOrchestrator(
        IAgentPlanner planner,
        IAgentToolRegistry toolRegistry)
    {
        _planner = planner;
        _toolRegistry = toolRegistry;
    }

    public async Task<AgentExecutionResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var plan = await _planner.CreatePlanAsync(context, cancellationToken);

        var results = new List<AgentToolResult>();

        foreach (var step in plan.Steps)
        {
            var tool = _toolRegistry.GetRequiredTool(step.ToolName);

            var result = await tool.ExecuteAsync(
                context.RepositoryPath,
                step.Input,
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