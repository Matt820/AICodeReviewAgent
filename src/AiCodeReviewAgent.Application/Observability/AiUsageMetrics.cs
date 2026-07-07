namespace AiCodeReviewAgent.Application.Observability;

public sealed class AiUsageMetrics
{
    public int AiCalls { get; private set; }

    public int EstimatedInputCharacters { get; private set; }

    public int EstimatedOutputCharacters { get; private set; }

    public void TrackCall(string input, string output)
    {
        AiCalls++;
        EstimatedInputCharacters += input?.Length ?? 0;
        EstimatedOutputCharacters += output?.Length ?? 0;
    }

    public int EstimatedInputTokens => EstimatedInputCharacters / 4;

    public int EstimatedOutputTokens => EstimatedOutputCharacters / 4;

    public int EstimatedTotalTokens => EstimatedInputTokens + EstimatedOutputTokens;
}