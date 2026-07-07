namespace AiCodeReviewAgent.Application.Observability;

public sealed class AiBudgetOptions
{
    public int MaxAiCalls { get; set; } = 20;

    public int MaxEstimatedTokens { get; set; } = 60000;
}