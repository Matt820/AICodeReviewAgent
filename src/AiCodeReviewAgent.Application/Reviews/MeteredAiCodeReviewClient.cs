using AiCodeReviewAgent.Application.Observability;

namespace AiCodeReviewAgent.Application.Reviews;

public sealed class MeteredAiCodeReviewClient : IAiCodeReviewClientDecorator
{
    private readonly IAiCodeReviewClient _inner;
    private readonly AiUsageMetrics _metrics;

    public MeteredAiCodeReviewClient(
        IAiCodeReviewClient inner,
        AiUsageMetrics metrics)
    {
        _inner = inner;
        _metrics = metrics;
    }

    public async Task<string> AnalyzeCodeAsync(
        AnalyzeCodeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _inner.AnalyzeCodeAsync(
            request,
            cancellationToken);

        _metrics.TrackCall(
            request.Code,
            response);

        return response;
    }
}