namespace AiCodeReviewAgent.Application.Observability;

public sealed class PipelineStageMetric
{
    public required string StageName { get; init; }

    public long DurationMs { get; init; }

    public bool Success { get; init; }

    public string? Error { get; init; }
}