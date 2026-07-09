namespace AiCodeReviewAgent.Application.Caching;

public sealed class AgentCacheEntry<T>
{
    public required string Key { get; init; }

    public required T Value { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}