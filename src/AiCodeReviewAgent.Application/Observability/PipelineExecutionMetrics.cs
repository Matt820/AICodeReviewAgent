namespace AiCodeReviewAgent.Application.Observability;

public sealed class PipelineExecutionMetrics
{
    private readonly List<PipelineStageMetric> _stages = [];

    public IReadOnlyList<PipelineStageMetric> Stages => _stages;

    public void AddStage(
        string stageName,
        long durationMs,
        bool success,
        string? error = null)
    {
        _stages.Add(new PipelineStageMetric
        {
            StageName = stageName,
            DurationMs = durationMs,
            Success = success,
            Error = error
        });
    }

    public long TotalDurationMs => _stages.Sum(x => x.DurationMs);
}