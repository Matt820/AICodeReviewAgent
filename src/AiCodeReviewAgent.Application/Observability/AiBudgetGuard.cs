namespace AiCodeReviewAgent.Application.Observability;

public sealed class AiBudgetGuard : IAiBudgetGuard
{
    private readonly AiUsageMetrics _metrics;
    private readonly AiBudgetOptions _options;
    public bool WasBudgetExceeded { get; private set; }

    public AiBudgetGuard(
        AiUsageMetrics metrics,
        AiBudgetOptions options)
    {
        _metrics = metrics;
        _options = options;
    }

    public bool CanExecuteAiCall(string input)
    {
        var estimatedInputTokens = (input?.Length ?? 0) / 4;

        if (_metrics.AiCalls >= _options.MaxAiCalls)
        {            
            return false;
        }

        if (_metrics.EstimatedTotalTokens + estimatedInputTokens >= _options.MaxEstimatedTokens)
            return false;

        WasBudgetExceeded = true;
        return true;
    }

    public string CreateBudgetExceededMessage()
    {
        return $"""
        ⚠️ AI budget exceeded.

        The agent stopped additional AI analysis to avoid unexpected cost.

        Current usage:
        - AI calls: {_metrics.AiCalls}/{_options.MaxAiCalls}
        - Estimated tokens: {_metrics.EstimatedTotalTokens}/{_options.MaxEstimatedTokens}

        Partial results were preserved.
        """;
    }
}