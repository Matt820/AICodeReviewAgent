namespace AiCodeReviewAgent.Application.Agents.Tools;

public interface IAgentToolRegistry
{
    IAgentTool GetRequiredTool(string toolName);
}