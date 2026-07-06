using AiCodeReviewAgent.Application.Agents.Tools;

namespace AiCodeReviewAgent.Application.Agents.Orchestration;

public sealed class AgentExecutionResult
{
    public required AgentPlan Plan { get; init; }

    public List<AgentToolResult> ToolResults { get; init; } = [];

    public bool Succeeded => ToolResults.All(x => x.Success);

    public string CombinedOutput =>
        string.Join(
            "\n\n---\n\n",
            ToolResults.Select(x =>
                string.IsNullOrWhiteSpace(x.Output)
                    ? x.Error
                    : x.Output));
}