using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Agents.Tools;

namespace AiCodeReviewAgent.Application.Agents.Execution;

public sealed class AgentToolExecutor : IAgentToolExecutor
{
    private readonly IAgentToolRegistry _toolRegistry;

    public AgentToolExecutor(IAgentToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public async Task<AgentToolResult> ExecuteAsync(
        AgentStep step,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var tool = _toolRegistry.GetRequiredTool(step.ToolName);

        try
        {
            return await tool.ExecuteAsync(
                context.RepositoryPath,
                step.Input,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return new AgentToolResult
            {
                ToolName = step.ToolName,
                Success = false,
                Error = ex.Message
            };
        }
    }
}