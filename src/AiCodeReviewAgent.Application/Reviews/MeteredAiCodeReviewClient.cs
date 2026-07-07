using AiCodeReviewAgent.Application.Observability;

namespace AiCodeReviewAgent.Application.Reviews;

public sealed class MeteredAiCodeReviewClient : IAiCodeReviewClientDecorator
{
    private readonly IAiCodeReviewClient _inner;
    private readonly AiUsageMetrics _metrics;
    private readonly IAiBudgetGuard _budgetGuard;

    public MeteredAiCodeReviewClient(
        IAiCodeReviewClient inner,
        AiUsageMetrics metrics,
        IAiBudgetGuard budgetGuard)
    {
        _inner = inner;
        _metrics = metrics;
        _budgetGuard = budgetGuard;
    }

    public async Task<string> AnalyzeCodeAsync(
        AnalyzeCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (!_budgetGuard.CanExecuteAiCall(request.Code))
        {
            return _budgetGuard.CreateBudgetExceededMessage();
        }

        var response = await _inner.AnalyzeCodeAsync(
            request,
            cancellationToken);

        _metrics.TrackCall(
            request.Code,
            response);

        return response;
    }
}