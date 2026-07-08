using AiCodeReviewAgent.Application.Agents.Orchestration;

namespace AiCodeReviewAgent.Application.Agents.Pipeline.Stages;

public sealed class OrchestrationStage : IAgentPipelineStage
{
    private readonly IAgentOrchestrator _orchestrator;

    public string Name => "orchestration";

    public OrchestrationStage(IAgentOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        await _orchestrator.ExecuteAsync(
            context.AgentContext,
            cancellationToken);
    }
}