namespace AiCodeReviewAgent.Application.Agents.Specialized;

public interface ISpecializedReviewAgent
{
    string Name { get; }

    Task<SpecializedReviewAgentResult> ReviewAsync(
        SpecializedReviewAgentRequest request,
        CancellationToken cancellationToken);
}