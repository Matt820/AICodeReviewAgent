using System.Collections.Concurrent;

namespace AiCodeReviewAgent.Application.Caching;

public sealed class InMemoryAgentCache : IAgentCache
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public bool TryGet<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var cached) && cached is AgentCacheEntry<T> entry)
        {
            value = entry.Value;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<T>(string key, T value)
    {
        _cache[key] = new AgentCacheEntry<T>
        {
            Key = key,
            Value = value
        };
    }
}