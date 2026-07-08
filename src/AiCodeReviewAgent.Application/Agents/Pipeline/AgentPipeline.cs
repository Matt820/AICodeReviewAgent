/* namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public sealed class AgentPipeline : IAgentPipeline
{
    private readonly IEnumerable<IAgentPipelineStage> _stages;

    public AgentPipeline(IEnumerable<IAgentPipelineStage> stages)
    {
        _stages = stages;
    }

    public async Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        foreach (var stage in _stages)
        {
            await stage.ExecuteAsync(context, cancellationToken);
        }
    }
} */

using System.Diagnostics;
using AiCodeReviewAgent.Application.Observability;

namespace AiCodeReviewAgent.Application.Agents.Pipeline;

public sealed class AgentPipeline : IAgentPipeline
{
    private readonly IEnumerable<IAgentPipelineStage> _stages;
    private readonly PipelineExecutionMetrics _metrics;

    public AgentPipeline(
        IEnumerable<IAgentPipelineStage> stages,
        PipelineExecutionMetrics metrics)
    {
        _stages = stages;
        _metrics = metrics;
    }

    public async Task ExecuteAsync(
        AgentPipelineContext context,
        CancellationToken cancellationToken)
    {
        foreach (var stage in _stages)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await stage.ExecuteAsync(context, cancellationToken);

                stopwatch.Stop();

                _metrics.AddStage(
                    stage.Name,
                    stopwatch.ElapsedMilliseconds,
                    success: true);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _metrics.AddStage(
                    stage.Name,
                    stopwatch.ElapsedMilliseconds,
                    success: false,
                    error: ex.Message);

                throw;
            }
        }
    }
}