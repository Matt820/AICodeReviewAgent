namespace AiCodeReviewAgent.Application.Agents.Specialized;

public sealed class SpecializedReviewOrchestrator
{
    private readonly IEnumerable<ISpecializedReviewAgent> _agents;

    public SpecializedReviewOrchestrator(
        IEnumerable<ISpecializedReviewAgent> agents)
    {
        _agents = agents;
    }

    public async Task<IReadOnlyList<SpecializedReviewAgentResult>> ReviewAsync(
        SpecializedReviewAgentRequest request,
        CancellationToken cancellationToken)
    {
        var results = new List<SpecializedReviewAgentResult>();

        foreach (var agent in _agents)
        {
            var result = await agent.ReviewAsync(
                request,
                cancellationToken);

            results.Add(result);
        }

        return results;
    }
}