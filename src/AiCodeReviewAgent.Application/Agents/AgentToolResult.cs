namespace AiCodeReviewAgent.Application.Agents;

public sealed class AgentToolResult
{
    public string ToolName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string? Error { get; set; }
}