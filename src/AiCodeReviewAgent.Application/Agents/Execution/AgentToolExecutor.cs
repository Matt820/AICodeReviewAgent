using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Agents.Tools;

namespace AiCodeReviewAgent.Application.Agents.Execution;

public sealed class AgentToolExecutor : IAgentToolExecutor
{
    private readonly IAgentToolProvider _toolProvider;

    public AgentToolExecutor(IAgentToolProvider toolProvider)
    {
        _toolProvider = toolProvider;
    }

    public async Task<AgentToolResult> ExecuteAsync(
        AgentStep step,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _toolProvider.ExecuteAsync(
                step.ToolName,
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