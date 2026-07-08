using AiCodeReviewAgent.Application.Rag;

namespace AiCodeReviewAgent.Application.Agents.Pipeline.Stages;

public sealed class RagContextStage : IAgentPipelineStage
{
    private readonly RepositoryRagContextBuilder _ragContextBuilder;

    public string Name => "rag_context";

    public RagContextStage(RepositoryRagContextBuilder ragContextBuilder)
    {
        _ragContextBuilder = ragContextBuilder;
    }

    public async Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        if (!context.AgentContext.Features.Rag)
            return;

        context.AgentContext.RagContext = await _ragContextBuilder.BuildAsync(
            context.RepositoryPath,
            $"{context.ChangedFilePath} {Path.GetFileNameWithoutExtension(context.ChangedFilePath)}",
            cancellationToken);
    }
}