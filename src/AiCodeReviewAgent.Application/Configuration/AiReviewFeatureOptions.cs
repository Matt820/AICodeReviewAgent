namespace AiCodeReviewAgent.Application.Configuration;

public sealed class AiReviewFeatureOptions
{
    public bool LlmPlanner { get; set; } = false;
    public bool Rag { get; set; } = false;
    public bool SpecializedAgents { get; set; } = false;
}