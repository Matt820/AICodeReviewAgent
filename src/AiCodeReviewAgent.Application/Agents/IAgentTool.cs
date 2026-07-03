namespace AiCodeReviewAgent.Application.Agents;

public interface IAgentTool
{
    string Name { get; }
    string Description { get; }

    Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken);
}