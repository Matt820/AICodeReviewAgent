namespace AiCodeReviewAgent.Application.Agents;

public sealed class AgentContext
{
    public string RepositoryPath { get; set; } = string.Empty;
    public string PullRequestDiff { get; set; } = string.Empty;
    public AgentToolResult? BuildResult { get; set; }
    public AgentToolResult? TestResult { get; set; }
    public List<AgentToolResult> ToolResults { get; set; } = [];    
}