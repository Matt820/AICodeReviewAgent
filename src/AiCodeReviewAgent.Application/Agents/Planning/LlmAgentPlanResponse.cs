using System.Text.Json.Serialization;

namespace AiCodeReviewAgent.Application.Agents.Planning;

public sealed class LlmAgentPlanResponse
{
    [JsonPropertyName("steps")]
    public List<LlmAgentPlanStep> Steps { get; set; } = [];
}

public sealed class LlmAgentPlanStep
{
    [JsonPropertyName("toolName")]
    public string ToolName { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}