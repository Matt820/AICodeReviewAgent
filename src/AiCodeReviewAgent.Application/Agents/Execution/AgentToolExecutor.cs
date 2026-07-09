using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Agents.Tools;
using AiCodeReviewAgent.Application.Caching;

namespace AiCodeReviewAgent.Application.Agents.Execution;

public sealed class AgentToolExecutor : IAgentToolExecutor
{
    private readonly IAgentToolProvider _toolProvider;
    private readonly IAgentCache _cache;

    public AgentToolExecutor(IAgentToolProvider toolProvider, IAgentCache cache)
    {
        _toolProvider = toolProvider;
        _cache = cache;
    }

    public async Task<AgentToolResult> ExecuteAsync(
    AgentStep step,
    AgentContext context,
    CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(
            context.RepositoryPath,
            step.ToolName,
            step.Input);

        if (IsCacheable(step.ToolName) &&
            _cache.TryGet<AgentToolResult>(cacheKey, out var cachedResult) &&
            cachedResult is not null)
        {
            return new AgentToolResult
            {
                ToolName = cachedResult.ToolName,
                Success = cachedResult.Success,
                Output = $"[cache hit]{Environment.NewLine}{cachedResult.Output}",
                Error = cachedResult.Error
            };
        }

        try
        {
            var result = await _toolProvider.ExecuteAsync(
                step.ToolName,
                context.RepositoryPath,
                step.Input,
                cancellationToken);

            if (IsCacheable(step.ToolName) && result.Success)
            {
                _cache.Set(cacheKey, result);
            }

            return result;
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

    private static bool IsCacheable(string toolName)
    {
        return toolName is
            "read_file" or
            "search_text" or
            "find_class" or
            "find_interface" or
            "read_solution" or
            "read_project_file";
    }

    private static string BuildCacheKey(
        string repositoryPath,
        string toolName,
        string input)
    {
        return $"{repositoryPath}:{toolName}:{input}".ToLowerInvariant();
    }
}