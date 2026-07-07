namespace AiCodeReviewAgent.Application.Agents.Tools;

public interface IAgentToolProvider
{
    Task<IReadOnlyList<AgentToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken);

    Task<AgentToolResult> ExecuteAsync(
        string toolName,
        string repositoryPath,
        string input,
        CancellationToken cancellationToken);
}