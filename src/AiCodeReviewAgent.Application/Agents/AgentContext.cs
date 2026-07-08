using AiCodeReviewAgent.Application.Agents.Specialized;
using AiCodeReviewAgent.Application.Configuration;

namespace AiCodeReviewAgent.Application.Agents;

public sealed class AgentContext
{
    public string RepositoryPath { get; set; } = string.Empty;
    public string PullRequestDiff { get; set; } = string.Empty;
    public AgentToolResult? BuildResult { get; set; }
    public AgentToolResult? TestResult { get; set; }
    public List<AgentToolResult> ToolResults { get; set; } = [];
    public List<string> Rules { get; set;} = [];
    public string RagContext { get; set; } = string.Empty;
    public bool UseLlmPlanner { get; set; }
    public List<SpecializedReviewAgentResult> SpecializedReviews { get; set; } = [];
    public AiReviewFeatureOptions Features { get; set; } = new();
}