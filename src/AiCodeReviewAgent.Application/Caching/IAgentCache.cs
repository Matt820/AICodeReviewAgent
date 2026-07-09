namespace AiCodeReviewAgent.Application.Caching;

public interface IAgentCache
{
    bool TryGet<T>(string key, out T? value);

    void Set<T>(string key, T value);
}